using Microsoft.AspNetCore.Mvc;
using Financial_System.Services;
using System.Threading.Tasks;
using Financial_System.Models;


namespace Financial_System.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ITransactionService _transactionService;

        public ReportController(
            IReportService reportService,
            ITransactionService transactionService)
        {
            _reportService = reportService;
            _transactionService = transactionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Summary(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;

            var transactions = await _transactionService.GetAllTransactionsAsync();
            var filtered = transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToList();

            var totalIncome = filtered
                .Where(t => t.TransactionType == "Income")
                .Sum(t => t.Amount);

            var totalExpenses = filtered
                .Where(t => t.TransactionType == "Expense")
                .Sum(t => t.Amount);

            var viewModel = new SummaryReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetIncome = totalIncome - totalExpenses,
                Transactions = filtered.OrderByDescending(t => t.TransactionDate).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CategoryAnalysis(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;

            var categoryData = await _reportService.GetCategoryBreakdownAsync();
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var filtered = transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToList();

            var viewModel = new CategoryAnalysisViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                CategoryData = categoryData,
                Transactions = filtered
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TrendAnalysis()
        {
            var monthlyData = await _reportService.GetMonthlyExpensesAsync();
            return View(monthlyData);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryChartData()
        {
            var data = await _reportService.GetCategoryBreakdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyChartData(int months = 12)
        {
            var data = await _reportService.GetMonthlyExpensesAsync();
            return Json(data.TakeLast(months).ToList());
        }

        public IActionResult Export(string type)
        {
            // Implementation for PDF/Excel export
            return View();
        }
    }

    public class SummaryReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public List<Transaction> Transactions { get; set; }
    }

    public class CategoryAnalysisViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CategoryData> CategoryData { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
