using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialSystem.Models
{
    /// <summary>
    /// Represents a financial report
    /// </summary>
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public ReportType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }

        public virtual Account Account { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal NetResult { get; set; }

        public string ReportData { get; set; } // JSON serialized data

        // AI Analysis
        public string AIAnalysis { get; set; }

        public string AIPredictions { get; set; }

        public List<string> Recommendations { get; set; } = new();

        public byte[] ExportData { get; set; } // PDF/Excel export

        public string CreatedBy { get; set; }
    }

    public enum ReportType
    {
        IncomeStatement,
        ExpenseBreakdown,
        CashFlow,
        BudgetComparison,
        Quarterly,
        Annual,
        Custom
    }

    /// <summary>
    /// Represents budget alerts and notifications
    /// </summary>
    public class BudgetAlert
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }

        public virtual Account Account { get; set; }

        public BudgetAlertType AlertType { get; set; }

        public decimal Threshold { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? TriggeredDate { get; set; }

        [StringLength(500)]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public string AIRecommendation { get; set; }
    }

    public enum BudgetAlertType
    {
        BudgetExceeded,
        LowBalance,
        UnusualSpending,
        SavingsGoalReached,
        HighExpenseCategory
    }
}
