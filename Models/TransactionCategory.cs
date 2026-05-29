namespace Financial_System.Models;
public class TransactionCategory
{
    public int TransactionCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty; // "Income" or "Expense"
    public bool IsActive { get; set; }

    public ICollection<Transaction>? Transactions { get; set; }
}

