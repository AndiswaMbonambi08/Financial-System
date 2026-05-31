using Financial_System.Controllers;
using Financial_System.ViewModels;

namespace Financial_System.Services
{
    public interface IReportService
    {
        Task<string> GenerateMonthlyReportAsync();

        // NEW methods
        Task<List<MonthlyData>> GetMonthlyExpensesAsync();
        Task<List<CategoryData>> GetCategoryBreakdownAsync();
    }
}



