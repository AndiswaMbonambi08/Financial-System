using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialSystemApp.Models
{
    /// <summary>
    /// Represents a financial transaction in the system
    /// </summary>
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public TransactionCategory Category { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Notes { get; set; }

        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }

        public virtual Account Account { get; set; }

        public bool IsRecurring { get; set; }

        public RecurrencePattern? RecurrencePattern { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; }

        public bool IsArchived { get; set; }

        // AI-Generated Insights
        public string AIInsight { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense,
        Transfer
    }

    public enum TransactionCategory
    {
        Salary,
        Freelance,
        Investment,
        Other,
        Groceries,
        Utilities,
        Transport,
        Entertainment,
        Healthcare,
        Office,
        Equipment,
        Software,
        Marketing
    }

    public enum RecurrencePattern
    {
        Daily,
        Weekly,
        BiWeekly,
        Monthly,
        Quarterly,
        Annually
    }
}
