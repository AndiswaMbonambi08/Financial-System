namespace Financial_System.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        public int TransactionCategoryId { get; set; }
        public TransactionCategory? TransactionCategory { get; set; }

        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }


}





