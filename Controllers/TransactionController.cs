using Financial_System.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Financial_System.Models;

namespace Financial_System.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;

        public TransactionController(
            ITransactionService transactionService,
            IAccountService accountService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index(int? accountId, string category, int page = 1)
        {
            var pageSize = 20;
            var transactions = await _transactionService.GetAllTransactionsAsync();

            if (accountId.HasValue)
                transactions = transactions.Where(t => t.AccountId == accountId).ToList();

            if (!string.IsNullOrEmpty(category))
                transactions = transactions.Where(t => t.TransactionCategory?.CategoryName == category).ToList();

            var totalCount = transactions.Count();
            var pagedTransactions = transactions
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["AccountId"] = accountId;
            ViewData["Category"] = category;
            ViewData["TotalPages"] = (totalCount + pageSize - 1) / pageSize;
            ViewData["CurrentPage"] = page;

            return View(pagedTransactions);
        }

        public async Task<IActionResult> Create()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            var categories = new List<string>
            {
                "Income",
                "Salary",
                "Bonus",
                "Investment Returns",
                "Other Income",
                "Food & Dining",
                "Transportation",
                "Utilities",
                "Shopping",
                "Entertainment",
                "Healthcare",
                "Education",
                "Savings",
                "Debt Payment",
                "Other Expense"
            };

            ViewBag.Accounts = accounts;
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            try
            {
                await _transactionService.AddTransactionAsync(transaction);
                TempData["Success"] = "Transaction created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                var accounts = await _accountService.GetAllAccountsAsync();
                ViewBag.Accounts = accounts;
                return View(transaction);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound();

            var accounts = await _accountService.GetAllAccountsAsync();
            ViewBag.Accounts = accounts;
            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Transaction transaction)
        {
            try
            {
                await _transactionService.UpdateTransactionAsync(transaction);
                TempData["Success"] = "Transaction updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return View(transaction);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _transactionService.DeleteTransactionAsync(id);
                TempData["Success"] = "Transaction deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var transactions = await _transactionService.SearchTransactionsAsync(query);
            return PartialView("_TransactionList", transactions);
        }
    }
}
