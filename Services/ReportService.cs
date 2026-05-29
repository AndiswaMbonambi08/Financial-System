using Financial_System.Controllers;

namespace Financial_System.Services
{
    public class ReportService : IReportService
    {
        public Task<string> GenerateMonthlyReportAsync()
        {
            return Task.FromResult("Monthly report generated.");
        }

        public Task<List<CategoryData>> GetCategoryBreakdownAsync()
        {
            // Placeholder data
            var data = new List<CategoryData>
            {
                new CategoryData { Category = "Food", Amount = 1200, Percentage = 40 },
                new CategoryData { Category = "Transport", Amount = 600, Percentage = 20 },
                new CategoryData { Category = "Entertainment", Amount = 300, Percentage = 10 }
            };
            return Task.FromResult(data);
        }

        public Task<List<MonthlyData>> GetMonthlyExpensesAsync()
        {
            // Placeholder data
            var data = new List<MonthlyData>
            {
                new MonthlyData { Month = "April", Income = 5000, Expenses = 2500 },
                new MonthlyData { Month = "May", Income = 5200, Expenses = 2700 }
            };
            return Task.FromResult(data);
        }
    }
}


