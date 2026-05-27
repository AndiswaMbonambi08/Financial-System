namespace FinancialSystemApp.Models.ERP
{
    public class RiskAssessment
    {
        public int Id { get; set; }
        public string RiskLevel { get; set; }   // e.g., Low, Medium, High
        public string Description { get; set; } // explanation of the risk
        public decimal ImpactScore { get; set; } // numeric score
        public string SuggestedAction { get; set; } // recommended mitigation
    }
}
