using Financial_System.Data;
using Financial_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Financial_System.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId)
        {
            return await _context.Transactions
                                 .Where(t => t.AccountId == accountId)
                                 .Include(t => t.TransactionCategory)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<Transaction>> SearchTransactionsAsync(string query)
        {
            return await _context.Transactions
                                 .Where(t => t.Description.Contains(query) ||
                                             t.TransactionCategory.CategoryName.Contains(query))
                                 .Include(t => t.TransactionCategory)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count)
        {
            return await _context.Transactions
                                 .OrderByDescending(t => t.TransactionDate)
                                 .Take(count)
                                 .Include(t => t.TransactionCategory)
                                 .ToListAsync();
        }
    }
}    


