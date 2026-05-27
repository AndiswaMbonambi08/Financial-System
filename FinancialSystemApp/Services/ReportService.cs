using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinancialSystemApp.Models;
using FinancialSystemApp.Data;

namespace FinancialSystemApp.Services
{
    /// <summary>
    /// Service for generating financial reports with AI-powered analysis
    /// </summary>
    public interface IReportService
    {
        Task<Report> GenerateIncomeStatementAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<Report> GenerateExpenseBreakdownAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<Report> GenerateCashFlowReportAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<Report> GenerateBudgetComparisonAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<List<Report>> GetAccountReportsAsync(int accountId);
        Task<Report> GetReportAsync(int id);
        Task<bool> DeleteReportAsync(int id);
    }

    public class ReportService : IReportService
    {
        private readonly FinancialDbContext _context;
        private readonly ITransactionService _transactionService;
        private readonly IFinancialAIService _aiService;

        public ReportService(
            FinancialDbContext context,
            ITransactionService transactionService,
            IFinancialAIService aiService)
        {
            _context = context;
            _transactionService = transactionService;
            _aiService = aiService;
        }

        public async Task<Report> GenerateIncomeStatementAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
            var account = await _context.Accounts.FindAsync(accountId);

            var totalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var netResult = totalIncome - totalExpenses;

            var incomeByCategory = transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                .ToList();

            var reportData = new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                IncomeStatement = new
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    NetIncome = netResult,
                    IncomeBreakdown = incomeByCategory
                },
                TransactionCount = transactions.Count
            };

            // Generate AI analysis
            var analysis = await _aiService.GenerateAccountAnalysis(account, transactions);
            var predictions = await _aiService.PredictExpenseTrends(transactions);
            var recommendations = await _aiService.GenerateRecommendations(account, transactions, 
                ((account.BudgetLimit - account.CurrentBalance) / account.BudgetLimit) * 100);

            var report = new Report
            {
                Title = $"Income Statement - {account.Name} ({startDate:MMM yyyy} to {endDate:MMM yyyy})",
                Description = $"Income statement for {account.Name}",
                Type = ReportType.IncomeStatement,
                StartDate = startDate,
                EndDate = endDate,
                AccountId = accountId,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetResult = netResult,
                ReportData = JsonSerializer.Serialize(reportData),
                AIAnalysis = analysis,
                AIPredictions = predictions,
                Recommendations = recommendations,
                GeneratedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateExpenseBreakdownAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
            var account = await _context.Accounts.FindAsync(accountId);

            var expenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .ToList();

            var expenseByCategory = expenses
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key.ToString(),
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count(),
                    Average = g.Average(t => t.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            var totalExpenses = expenses.Sum(t => t.Amount);

            var reportData = new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                ExpenseBreakdown = expenseByCategory,
                TotalExpenses = totalExpenses,
                AverageExpense = expenses.Any() ? expenses.Average(t => t.Amount) : 0,
                HighestExpense = expenses.Any() ? expenses.Max(t => t.Amount) : 0,
                LowestExpense = expenses.Any() ? expenses.Min(t => t.Amount) : 0,
                TransactionCount = expenses.Count
            };

            var analysis = await _aiService.GenerateAccountAnalysis(account, transactions);
            var predictions = await _aiService.PredictExpenseTrends(transactions);
            var recommendations = await _aiService.GenerateRecommendations(account, transactions, 
                (totalExpenses / account.BudgetLimit) * 100);

            var report = new Report
            {
                Title = $"Expense Breakdown - {account.Name} ({startDate:MMM yyyy} to {endDate:MMM yyyy})",
                Description = $"Detailed expense breakdown by category",
                Type = ReportType.ExpenseBreakdown,
                StartDate = startDate,
                EndDate = endDate,
                AccountId = accountId,
                TotalExpenses = totalExpenses,
                NetResult = -totalExpenses,
                ReportData = JsonSerializer.Serialize(reportData),
                AIAnalysis = analysis,
                AIPredictions = predictions,
                Recommendations = recommendations,
                GeneratedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateCashFlowReportAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);

            var monthlyCashFlow = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g =>
                {
                    var monthTransactions = g.ToList();
                    var income = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                    var expenses = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                    return new
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Income = income,
                        Expenses = expenses,
                        NetCashFlow = income - expenses
                    };
                })
                .OrderBy(x => x.Month)
                .ToList();

            var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            var reportData = new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                MonthlyCashFlow = monthlyCashFlow,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetCashFlow = totalIncome - totalExpenses
            };

            var analysis = await _aiService.GenerateAccountAnalysis(account, transactions);
            var predictions = await _aiService.PredictExpenseTrends(transactions);

            var report = new Report
            {
                Title = $"Cash Flow Report - {account.Name} ({startDate:MMM yyyy} to {endDate:MMM yyyy})",
                Description = $"Monthly cash flow analysis",
                Type = ReportType.CashFlow,
                StartDate = startDate,
                EndDate = endDate,
                AccountId = accountId,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetResult = totalIncome - totalExpenses,
                ReportData = JsonSerializer.Serialize(reportData),
                AIAnalysis = analysis,
                AIPredictions = predictions,
                GeneratedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateBudgetComparisonAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);

            var actualExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var budgetedAmount = account.BudgetLimit;
            var variance = budgetedAmount - actualExpenses;
            var variancePercentage = (variance / budgetedAmount) * 100;

            var expenseByCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key.ToString(),
                    Actual = g.Sum(t => t.Amount),
                    Percentage = (g.Sum(t => t.Amount) / budgetedAmount) * 100
                })
                .OrderByDescending(x => x.Actual)
                .ToList();

            var reportData = new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                BudgetComparison = new
                {
                    Budgeted = budgetedAmount,
                    Actual = actualExpenses,
                    Variance = variance,
                    VariancePercentage = variancePercentage,
                    OnBudget = variance >= 0
                },
                ExpenseByCategory = expenseByCategory
            };

            var analysis = await _aiService.GenerateAccountAnalysis(account, transactions);
            var recommendations = await _aiService.GenerateRecommendations(account, transactions, 
                (actualExpenses / budgetedAmount) * 100);

            var report = new Report
            {
                Title = $"Budget Comparison - {account.Name} ({startDate:MMM yyyy} to {endDate:MMM yyyy})",
                Description = $"Actual vs budgeted expenses comparison",
                Type = ReportType.BudgetComparison,
                StartDate = startDate,
                EndDate = endDate,
                AccountId = accountId,
                TotalExpenses = actualExpenses,
                NetResult = variance,
                ReportData = JsonSerializer.Serialize(reportData),
                AIAnalysis = analysis,
                Recommendations = recommendations,
                GeneratedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<List<Report>> GetAccountReportsAsync(int accountId)
        {
            return await _context.Reports
                .Where(r => r.AccountId == accountId)
                .OrderByDescending(r => r.GeneratedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Report> GetReportAsync(int id)
        {
            return await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return false;

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
