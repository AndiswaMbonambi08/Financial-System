using Financial_System.Models;

namespace Financial_System.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalBalance { get; set; }
        public int TotalAccounts { get; set; }
        public List<Account> Accounts { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<MonthlyData> MonthlyExpenses { get; set; } = new();
        public List<CategoryData> CategoryBreakdown { get; set; } = new();
    }
}




