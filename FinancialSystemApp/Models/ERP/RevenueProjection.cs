namespace FinancialSystemApp.Models.ERP
{
    public class RevenueProjection
    {
        public int Id { get; set; }
        public string Period { get; set; } // e.g., "2026-Q2"
        public decimal ProjectedRevenue { get; set; }
        public decimal ConfidenceLevel { get; set; } // percentage confidence
        public string Notes { get; set; } // optional explanation
    }
}

