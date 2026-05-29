using Financial_System.Controllers; // for MonthlyData and CategoryData

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



