using Microsoft.AspNetCore.Mvc;
using Financial_System.Services;
using System.Threading.Tasks;
using Financial_System.Models;
using System.Collections.Generic;
using System.Linq;


namespace Financial_System.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IReportService _reportService;

        public DashboardController(
            IAccountService accountService,
            ITransactionService transactionService,
            IReportService reportService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            var totalBalance = 0m;
            foreach (var account in accounts)
            {
                totalBalance += account.CurrentBalance;
            }

            var recentTransactions = await _transactionService.GetRecentTransactionsAsync(5);
            var monthlyData = await _reportService.GetMonthlyExpensesAsync();
            var categoryData = await _reportService.GetCategoryBreakdownAsync();

            var viewModel = new DashboardViewModel
            {
                TotalBalance = totalBalance,
                TotalAccounts = accounts.Count(),
                Accounts = accounts.ToList(),
                RecentTransactions = recentTransactions.ToList(),
                MonthlyExpenses = monthlyData,
                CategoryBreakdown = categoryData
            };

            return View(viewModel);
        }
    }

    public class DashboardViewModel
    {
        public decimal TotalBalance { get; set; }
        public int TotalAccounts { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public List<MonthlyData> MonthlyExpenses { get; set; }
        public List<CategoryData> CategoryBreakdown { get; set; }
    }

    public class MonthlyData
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }

    public class CategoryData
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public int Percentage { get; set; }
    }
}
