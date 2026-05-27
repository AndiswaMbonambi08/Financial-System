using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FinancialSystemApp.Models
{
    /// <summary>
    /// Represents a financial account (project or team account)
    /// </summary>
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public decimal InitialBalance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentBalance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BudgetLimit { get; set; }

        public string CurrencyCode { get; set; } = "USD";

        public AccountStatus Status { get; set; } = AccountStatus.Active;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; }

        // Collections
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

        public virtual ICollection<BudgetAlert> BudgetAlerts { get; set; } = new List<BudgetAlert>();

        // Statistics
        [NotMapped]
        public decimal TotalIncome { get; set; }

        [NotMapped]
        public decimal TotalExpenses { get; set; }

        [NotMapped]
        public decimal NetBalance { get; set; }

        [NotMapped]
        public decimal BudgetUtilizationPercentage { get; set; }

        public string AccountColor { get; set; } = "#F59E0B";
    }

    public enum AccountStatus
    {
        Active,
        Inactive,
        Archived,
        Suspended
    }
}
