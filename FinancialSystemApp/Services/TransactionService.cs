using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinancialSystemApp.Models;
using FinancialSystemApp.Data;

namespace FinancialSystemApp.Services
{
    /// <summary>
    /// Service for managing financial transactions with business logic
    /// </summary>
    public interface ITransactionService
    {
        Task<List<Transaction>> GetTransactionsByAccountAsync(int accountId);
        Task<Transaction> GetTransactionAsync(int id);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(int id);
        Task<List<Transaction>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<List<Transaction>> GetTransactionsByCategoryAsync(int accountId, TransactionCategory category);
        Task<(decimal TotalIncome, decimal TotalExpenses)> GetAccountSummaryAsync(int accountId);
        Task ProcessRecurringTransactionsAsync();
    }

    public class TransactionService : ITransactionService
    {
        private readonly FinancialDbContext _context;
        private readonly IFinancialAIService _aiService;

        public TransactionService(FinancialDbContext context, IFinancialAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<List<Transaction>> GetTransactionsByAccountAsync(int accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId && !t.IsArchived)
                .OrderByDescending(t => t.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Transaction> GetTransactionAsync(int id)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            // Validate transaction
            ValidateTransaction(transaction);

            transaction.CreatedDate = DateTime.UtcNow;

            // Get account and update balance
            var account = await _context.Accounts.FindAsync(transaction.AccountId);
            if (account == null)
                throw new InvalidOperationException("Account not found");

            // Update account balance based on transaction type
            switch (transaction.Type)
            {
                case TransactionType.Income:
                    account.CurrentBalance += transaction.Amount;
                    break;
                case TransactionType.Expense:
                    account.CurrentBalance -= transaction.Amount;
                    break;
                case TransactionType.Transfer:
                    // Transfer logic would be handled separately
                    account.CurrentBalance -= transaction.Amount;
                    break;
            }

            // Generate AI insight
            var historicalTransactions = await GetTransactionsByAccountAsync(transaction.AccountId);
            transaction.AIInsight = await _aiService.GenerateTransactionInsight(transaction, historicalTransactions);

            _context.Transactions.Add(transaction);
            account.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Check budget and create alerts if needed
            await CheckBudgetThresholdsAsync(transaction.AccountId, account);

            return transaction;
        }

        public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var existingTransaction = await _context.Transactions.FindAsync(transaction.Id);
            if (existingTransaction == null)
                throw new InvalidOperationException("Transaction not found");

            var account = await _context.Accounts.FindAsync(existingTransaction.AccountId);
            if (account == null)
                throw new InvalidOperationException("Account not found");

            // Reverse the original transaction's effect
            ReverseTransactionEffect(existingTransaction, account);

            // Apply the new transaction's effect
            ApplyTransactionEffect(transaction, account);

            // Update transaction properties
            existingTransaction.Description = transaction.Description;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Type = transaction.Type;
            existingTransaction.Category = transaction.Category;
            existingTransaction.Date = transaction.Date;
            existingTransaction.Notes = transaction.Notes;
            existingTransaction.ModifiedDate = DateTime.UtcNow;

            account.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingTransaction;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            var account = await _context.Accounts.FindAsync(transaction.AccountId);
            if (account != null)
            {
                // Reverse the transaction's effect
                ReverseTransactionEffect(transaction, account);
                account.ModifiedDate = DateTime.UtcNow;
            }

            // Soft delete
            transaction.IsArchived = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId 
                    && t.Date >= startDate 
                    && t.Date <= endDate 
                    && !t.IsArchived)
                .OrderByDescending(t => t.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByCategoryAsync(int accountId, TransactionCategory category)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId 
                    && t.Category == category 
                    && !t.IsArchived)
                .OrderByDescending(t => t.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(decimal TotalIncome, decimal TotalExpenses)> GetAccountSummaryAsync(int accountId)
        {
            var transactions = await GetTransactionsByAccountAsync(accountId);
            
            var totalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            return (totalIncome, totalExpenses);
        }

        public async Task ProcessRecurringTransactionsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var recurringTransactions = await _context.Transactions
                .Where(t => t.IsRecurring && !t.IsArchived)
                .ToListAsync();

            foreach (var transaction in recurringTransactions)
            {
                if (ShouldProcessRecurring(transaction, today))
                {
                    var newTransaction = new Transaction
                    {
                        Description = transaction.Description,
                        Amount = transaction.Amount,
                        Type = transaction.Type,
                        Category = transaction.Category,
                        Date = today,
                        Notes = $"Auto-generated from recurring pattern",
                        AccountId = transaction.AccountId,
                        IsRecurring = false,
                        CreatedBy = "System"
                    };

                    await CreateTransactionAsync(newTransaction);
                }
            }
        }

        private void ValidateTransaction(Transaction transaction)
        {
            if (string.IsNullOrWhiteSpace(transaction.Description))
                throw new ArgumentException("Description is required");

            if (transaction.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (transaction.Date > DateTime.UtcNow)
                throw new ArgumentException("Transaction date cannot be in the future");
        }

        private void ApplyTransactionEffect(Transaction transaction, Account account)
        {
            switch (transaction.Type)
            {
                case TransactionType.Income:
                    account.CurrentBalance += transaction.Amount;
                    break;
                case TransactionType.Expense:
                    account.CurrentBalance -= transaction.Amount;
                    break;
                case TransactionType.Transfer:
                    account.CurrentBalance -= transaction.Amount;
                    break;
            }
        }

        private void ReverseTransactionEffect(Transaction transaction, Account account)
        {
            switch (transaction.Type)
            {
                case TransactionType.Income:
                    account.CurrentBalance -= transaction.Amount;
                    break;
                case TransactionType.Expense:
                    account.CurrentBalance += transaction.Amount;
                    break;
                case TransactionType.Transfer:
                    account.CurrentBalance += transaction.Amount;
                    break;
            }
        }

        private bool ShouldProcessRecurring(Transaction transaction, DateTime today)
        {
            if (!transaction.RecurrencePattern.HasValue)
                return false;

            return transaction.RecurrencePattern.Value switch
            {
                RecurrencePattern.Daily => true,
                RecurrencePattern.Weekly => today.DayOfWeek == transaction.Date.DayOfWeek,
                RecurrencePattern.BiWeekly => (today - transaction.Date).Days % 14 == 0,
                RecurrencePattern.Monthly => today.Day == transaction.Date.Day,
                RecurrencePattern.Quarterly => today.Month % 3 == transaction.Date.Month % 3,
                RecurrencePattern.Annually => today.Month == transaction.Date.Month && today.Day == transaction.Date.Day,
                _ => false
            };
        }

        private async Task CheckBudgetThresholdsAsync(int accountId, Account account)
        {
            var spending = (account.BudgetLimit - account.CurrentBalance);
            var utilization = (spending / account.BudgetLimit) * 100;

            if (utilization > 90)
            {
                var alert = new BudgetAlert
                {
                    AccountId = accountId,
                    AlertType = BudgetAlertType.BudgetExceeded,
                    Threshold = account.BudgetLimit * 0.9m,
                    TriggeredDate = DateTime.UtcNow,
                    Message = $"Budget utilization at {utilization:F1}%",
                    IsActive = true,
                    AIRecommendation = await _aiService.GenerateBudgetAlert(account, spending)
                };

                _context.BudgetAlerts.Add(alert);
                await _context.SaveChangesAsync();
            }
        }
    }
}
