using Microsoft.AspNetCore.Mvc;
using Financial_System.Services;
using Financial_System.Models;
using Financial_System.ViewModels;

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
            var totalBalance = accounts.Sum(a => a.CurrentBalance);
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
}
