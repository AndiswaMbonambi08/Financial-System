using Microsoft.EntityFrameworkCore;
using FinancialSystemApp.Models;

namespace FinancialSystemApp.Data
{
    public class FinancialDbContext : DbContext
    {
        public FinancialDbContext(DbContextOptions<FinancialDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<BudgetAlert> BudgetAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
                entity.Property(e => e.InitialBalance).HasPrecision(18, 2);
                entity.Property(e => e.BudgetLimit).HasPrecision(18, 2);
                entity.Property(e => e.TotalIncome).HasPrecision(18, 2);
                entity.Property(e => e.TotalExpenses).HasPrecision(18, 2);
                entity.Property(e => e.NetBalance).HasPrecision(18, 2);

                entity.HasMany(e => e.Transactions)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Reports)
                    .WithOne(r => r.Account)
                    .HasForeignKey(r => r.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.BudgetAlerts)
                    .WithOne(ba => ba.Account)
                    .HasForeignKey(ba => ba.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedDate);
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(255);

                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.AccountId);
                entity.HasIndex(e => new { e.AccountId, e.Date });
            });

            // Configure Report
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.TotalIncome).HasPrecision(18, 2);
                entity.Property(e => e.TotalExpenses).HasPrecision(18, 2);
                entity.Property(e => e.NetResult).HasPrecision(18, 2);

                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.GeneratedDate);
                entity.HasIndex(e => e.AccountId);
            });

            // Configure BudgetAlert
            modelBuilder.Entity<BudgetAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Threshold).HasPrecision(18, 2);

                entity.HasIndex(e => e.AlertType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.AccountId, e.IsActive });
            });

            // Seed initial data
            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Seed a sample account
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = 1,
                    Name = "Project Alpha",
                    Description = "Q1 2024 Project Budget",
                    InitialBalance = 50000,
                    CurrentBalance = 42350,
                    BudgetLimit = 50000,
                    CurrencyCode = "USD",
                    Status = AccountStatus.Active,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    AccountColor = "#F59E0B"
                },
                new Account
                {
                    Id = 2,
                    Name = "Operations",
                    Description = "General operational expenses",
                    InitialBalance = 100000,
                    CurrentBalance = 78650,
                    BudgetLimit = 100000,
                    CurrencyCode = "USD",
                    Status = AccountStatus.Active,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    AccountColor = "#D97706"
                }
            );

            // Seed sample transactions
            modelBuilder.Entity<Transaction>().HasData(
                new Transaction
                {
                    Id = 1,
                    Description = "Project Income",
                    Amount = 15000,
                    Type = TransactionType.Income,
                    Category = TransactionCategory.Freelance,
                    Date = DateTime.UtcNow.AddDays(-30),
                    AccountId = 1,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    AIInsight = "Steady income stream - consistent with historical patterns"
                },
                new Transaction
                {
                    Id = 2,
                    Description = "Software Licenses",
                    Amount = 2500,
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.Software,
                    Date = DateTime.UtcNow.AddDays(-15),
                    AccountId = 1,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    AIInsight = "Quarterly software expense - budget allocation optimal"
                },
                new Transaction
                {
                    Id = 3,
                    Description = "Office Equipment",
                    Amount = 3500,
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.Equipment,
                    Date = DateTime.UtcNow.AddDays(-10),
                    AccountId = 1,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    AIInsight = "One-time equipment purchase - within budget parameters"
                }
            );
        }
    }
}
