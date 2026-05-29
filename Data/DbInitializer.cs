
using Financial_System.Models;

namespace Financial_System.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // If database is already populated, don't seed again
            if (context.Accounts.Any())
            {
                return;
            }

            // Create sample accounts
            var accounts = new Account[]
            {
                new Account
                {
                    Name = "Main Savings",
                    AccountType = "Savings",
                    CurrentBalance = 15000m,
                    CreatedDate = DateTime.Now.AddMonths(-6),
                    IsActive = true
                },
                new Account
                {
                    Name = "Daily Checking",
                    AccountType = "Checking",
                    CurrentBalance = 5500m,
                    CreatedDate = DateTime.Now.AddMonths(-3),
                    IsActive = true
                },
                new Account
                {
                    Name = "Investment Account",
                    AccountType = "Investment",
                    CurrentBalance = 25000m,
                    CreatedDate = DateTime.Now.AddMonths(-12),
                    IsActive = true
                },
                new Account
                {
                    Name = "Emergency Fund",
                    AccountType = "Savings",
                    CurrentBalance = 8000m,
                    CreatedDate = DateTime.Now.AddMonths(-8),
                    IsActive = true
                }
            };

            foreach (Account a in accounts)
            {
                context.Accounts.Add(a);
            }
            context.SaveChanges();

            // Create sample categories
            var categories = new TransactionCategory[]
            {
                new TransactionCategory
                {
                    CategoryName = "Salary",
                    TransactionType = "Income",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Bonus",
                    TransactionType = "Income",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Investment Returns",
                    TransactionType = "Income",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Food & Dining",
                    TransactionType = "Expense",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Transportation",
                    TransactionType = "Expense",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Utilities",
                    TransactionType = "Expense",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Shopping",
                    TransactionType = "Expense",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Entertainment",
                    TransactionType = "Expense",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Healthcare",
                    TransactionType = "Expense",
                    IsActive = true
                },
                new TransactionCategory
                {
                    CategoryName = "Education",
                    TransactionType = "Expense",
                    IsActive = true
                }
            };

            foreach (TransactionCategory c in categories)
            {
                context.TransactionCategories.Add(c);
            }
            context.SaveChanges();

            // Create sample transactions
            var transactions = new Transaction[]
            {
                new Transaction
                {
                    AccountId = accounts[0].AccountId,
                    TransactionCategoryId = categories[0].TransactionCategoryId,
                    Amount = 45000m,
                    TransactionType = "Income",
                    Description = "Monthly salary",
                    TransactionDate = DateTime.Now.AddDays(-15)
                },
                new Transaction
                {
                    AccountId = accounts[1].AccountId,
                    TransactionCategoryId = categories[3].TransactionCategoryId,
                    Amount = 450m,
                    TransactionType = "Expense",
                    Description = "Grocery shopping and dining",
                    TransactionDate = DateTime.Now.AddDays(-10)
                },
                new Transaction
                {
                    AccountId = accounts[1].AccountId,
                    TransactionCategoryId = categories[4].TransactionCategoryId,
                    Amount = 150m,
                    TransactionType = "Expense",
                    Description = "Uber and taxi rides",
                    TransactionDate = DateTime.Now.AddDays(-8)
                },
                new Transaction
                {
                    AccountId = accounts[0].AccountId,
                    TransactionCategoryId = categories[5].TransactionCategoryId,
                    Amount = 200m,
                    TransactionType = "Expense",
                    Description = "Electricity and water bills",
                    TransactionDate = DateTime.Now.AddDays(-5)
                },
                new Transaction
                {
                    AccountId = accounts[1].AccountId,
                    TransactionCategoryId = categories[6].TransactionCategoryId,
                    Amount = 320m,
                    TransactionType = "Expense",
                    Description = "Clothing and shoes",
                    TransactionDate = DateTime.Now.AddDays(-3)
                },
                new Transaction
                {
                    AccountId = accounts[2].AccountId,
                    TransactionCategoryId = categories[2].TransactionCategoryId,
                    Amount = 1250m,
                    TransactionType = "Income",
                    Description = "Dividend from investments",
                    TransactionDate = DateTime.Now.AddDays(-2)
                },
                new Transaction
                {
                    AccountId = accounts[1].AccountId,
                    TransactionCategoryId = categories[7].TransactionCategoryId,
                    Amount = 85m,
                    TransactionType = "Expense",
                    Description = "Movie tickets and subscription",
                    TransactionDate = DateTime.Now.AddDays(-1)
                }
            };

            foreach (Transaction t in transactions)
            {
                context.Transactions.Add(t);
            }
            context.SaveChanges();
        }
    }
}
