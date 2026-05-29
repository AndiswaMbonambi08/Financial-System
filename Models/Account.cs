namespace Financial_System.Models
{
    public class Account
    {
        public int AccountId { get; set; }   // Primary key
        public string Name { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;

        public decimal CurrentBalance { get; set; }   // Used in transfers
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation property for related transactions
        public ICollection<Transaction>? Transactions { get; set; }
    }
}





