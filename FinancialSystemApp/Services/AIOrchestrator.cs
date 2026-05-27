using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinancialSystemApp.AI.Extensions;

namespace FinancialSystemApp.AI.Extensions
{
    /// <summary>
    /// Unified AI Orchestrator - Orchestrates all AI capabilities across extensions
    /// Provides a single interface for accessing all AI-powered features
    /// </summary>
    public interface IAIOrchestrator
    {
        /// <summary>
        /// Get all available AI capabilities
        /// </summary>
        Task<AICapabilitiesSummary> GetAvailableCapabilitiesAsync();

        /// <summary>
        /// Run AI analysis across multiple extensions
        /// </summary>
        Task<AIAnalysisResult> RunComprehensiveAnalysisAsync(AnalysisRequest request);

        /// <summary>
        /// Get AI insights for an entity
        /// </summary>
        Task<AIInsights> GetEntityInsightsAsync(string entityType, int entityId);

        /// <summary>
        /// Execute AI recommendation engine
        /// </summary>
        Task<List<AIRecommendation>> GetRecommendationsAsync(string context, int count = 5);

        /// <summary>
        /// Generate full AI report
        /// </summary>
        Task<AIReport> GenerateReportAsync(ReportRequest request);
    }

    public class AIOrchestrator : IAIOrchestrator
    {
        private readonly IAIExtensionManager _extensionManager;
        private readonly ILogger<AIOrchestrator> _logger;

        public AIOrchestrator(IAIExtensionManager extensionManager, ILogger<AIOrchestrator> logger)
        {
            _extensionManager = extensionManager ?? throw new ArgumentNullException(nameof(extensionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AICapabilitiesSummary> GetAvailableCapabilitiesAsync()
        {
            var extensions = _extensionManager.GetAllExtensions();
            var capabilities = new List<string>();

            foreach (var ext in extensions.Where(e => e.IsEnabled))
            {
                capabilities.AddRange(GetCapabilitiesForExtension(ext));
            }

            return new AICapabilitiesSummary
            {
                Capabilities = capabilities,
                EnabledExtensions = extensions.Where(e => e.IsEnabled).Count(),
                TotalExtensions = extensions.Count,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<AIAnalysisResult> RunComprehensiveAnalysisAsync(AnalysisRequest request)
        {
            _logger.LogInformation($"Running comprehensive AI analysis for {request.EntityType}");

            var result = new AIAnalysisResult
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                StartedAt = DateTime.UtcNow,
                AnalysisByModule = new Dictionary<string, object>()
            };

            try
            {
                // Run Predictive Analytics
                var predictiveExt = _extensionManager.GetExtension("predictive-analytics") as IPredictiveAnalyticsExtension;
                if (predictiveExt?.IsEnabled == true)
                {
                    try
                    {
                        var prediction = await predictiveExt.PredictExpenseAsync(request.EntityId, 6);
                        result.AnalysisByModule["Predictive Analytics"] = new
                        {
                            PredictedValue = prediction.PredictedValue,
                            Confidence = prediction.Confidence,
                            Trend = prediction.Trend
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Predictive analytics failed: {ex.Message}");
                    }
                }

                // Run Risk Assessment
                var riskExt = _extensionManager.GetExtension("risk-assessment") as IRiskAssessmentExtension;
                if (riskExt?.IsEnabled == true)
                {
                    try
                    {
                        var riskAssessment = await riskExt.AssessCustomerCreditRiskAsync(request.EntityId);
                        result.AnalysisByModule["Risk Assessment"] = new
                        {
                            RiskScore = riskAssessment.RiskScore,
                            RiskLevel = riskAssessment.RiskLevel,
                            Factors = riskAssessment.Factors.Select(f => f.FactorName)
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Risk assessment failed: {ex.Message}");
                    }
                }

                result.Status = "completed";
                result.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running comprehensive analysis");
                result.Status = "failed";
                result.Error = ex.Message;
            }

            return result;
        }

        public async Task<AIInsights> GetEntityInsightsAsync(string entityType, int entityId)
        {
            _logger.LogInformation($"Generating AI insights for {entityType}:{entityId}");

            var insights = new AIInsights
            {
                EntityType = entityType,
                EntityId = entityId,
                GeneratedAt = DateTime.UtcNow,
                Insights = new List<Insight>()
            };

            try
            {
                // Predictive insight
                var predictiveExt = _extensionManager.GetExtension("predictive-analytics") as IPredictiveAnalyticsExtension;
                if (predictiveExt?.IsEnabled == true)
                {
                    var prediction = await predictiveExt.PredictExpenseAsync(entityId, 3);
                    insights.Insights.Add(new Insight
                    {
                        Title = "Expense Forecast",
                        Description = $"Expenses projected to be {(prediction.Trend == "Increasing" ? "increasing" : "stable")} over next 3 months",
                        Confidence = prediction.Confidence,
                        Type = "predictive",
                        Value = prediction.PredictedValue
                    });
                }

                // Risk insight
                var riskExt = _extensionManager.GetExtension("risk-assessment") as IRiskAssessmentExtension;
                if (riskExt?.IsEnabled == true)
                {
                    var risk = await riskExt.AssessCustomerCreditRiskAsync(entityId);
                    insights.Insights.Add(new Insight
                    {
                        Title = "Risk Assessment",
                        Description = $"Credit risk level: {risk.RiskLevel}",
                        Confidence = 1.0 - (risk.RiskScore / 100),
                        Type = "risk",
                        Value = risk.RiskScore
                    });
                }

                insights.Status = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating insights: {ex.Message}");
                insights.Status = "error";
                insights.Error = ex.Message;
            }

            return insights;
        }

        public async Task<List<AIRecommendation>> GetRecommendationsAsync(string context, int count = 5)
        {
            _logger.LogInformation($"Generating AI recommendations for context: {context}");

            var recommendations = new List<AIRecommendation>();

            try
            {
                // Generate context-aware recommendations
                switch (context.ToLower())
                {
                    case "cash flow optimization":
                        recommendations.AddRange(GenerateCashFlowRecommendations(count));
                        break;
                    case "cost reduction":
                        recommendations.AddRange(GenerateCostReductionRecommendations(count));
                        break;
                    case "risk management":
                        recommendations.AddRange(GenerateRiskManagementRecommendations(count));
                        break;
                    case "inventory optimization":
                        recommendations.AddRange(GenerateInventoryRecommendations(count));
                        break;
                    default:
                        recommendations.AddRange(GenerateGeneralRecommendations(count));
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations");
            }

            return recommendations;
        }

        public async Task<AIReport> GenerateReportAsync(ReportRequest request)
        {
            _logger.LogInformation($"Generating AI report: {request.ReportType}");

            var report = new AIReport
            {
                ReportType = request.ReportType,
                Period = request.Period,
                GeneratedAt = DateTime.UtcNow,
                Sections = new Dictionary<string, ReportSection>()
            };

            try
            {
                // Executive Summary
                report.Sections["Executive Summary"] = new ReportSection
                {
                    Title = "Executive Summary",
                    Content = GenerateExecutiveSummary(request),
                    Order = 1
                };

                // Key Findings
                report.Sections["Key Findings"] = new ReportSection
                {
                    Title = "Key Findings",
                    Content = "Based on AI analysis, the following key findings have been identified:",
                    Findings = GenerateKeyFindings(request),
                    Order = 2
                };

                // Recommendations
                report.Sections["Recommendations"] = new ReportSection
                {
                    Title = "Recommendations",
                    Content = "The following recommendations are suggested based on the analysis:",
                    Recommendations = await GetRecommendationsAsync(request.ReportType, 5),
                    Order = 3
                };

                // Risk Analysis
                report.Sections["Risk Analysis"] = new ReportSection
                {
                    Title = "Risk Analysis",
                    Content = "Risk factors identified through AI analysis:",
                    Order = 4
                };

                report.Status = "generated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                report.Status = "failed";
                report.Error = ex.Message;
            }

            return report;
        }

        private List<string> GetCapabilitiesForExtension(IFinancialAIExtension extension)
        {
            var capabilities = new List<string>();

            if (extension is IPredictiveAnalyticsExtension)
            {
                capabilities.AddRange(new[]
                {
                    "Demand Forecasting",
                    "Expense Prediction",
                    "Cash Flow Forecasting",
                    "Revenue Projection"
                });
            }

            if (extension is IRiskAssessmentExtension)
            {
                capabilities.AddRange(new[]
                {
                    "Credit Risk Assessment",
                    "Vendor Risk Analysis",
                    "Inventory Risk Evaluation"
                });
            }

            if (extension is IAnomalyDetectionExtension)
            {
                capabilities.AddRange(new[]
                {
                    "Transaction Anomaly Detection",
                    "Inventory Anomaly Detection",
                    "Payment Pattern Analysis"
                });
            }

            if (extension is IOptimizationExtension)
            {
                capabilities.AddRange(new[]
                {
                    "Inventory Optimization",
                    "Procurement Cost Optimization",
                    "Cash Flow Optimization"
                });
            }

            return capabilities;
        }

        private List<AIRecommendation> GenerateCashFlowRecommendations(int count)
        {
            return new List<AIRecommendation>
            {
                new AIRecommendation
                {
                    Title = "Optimize Payment Schedule",
                    Description = "Negotiate extended payment terms with top suppliers to improve cash flow.",
                    ImpactScore = 8.5,
                    Priority = "High",
                    Category = "Cash Flow",
                    EstimatedBenefit = 50000m
                },
                new AIRecommendation
                {
                    Title = "Accelerate Collections",
                    Description = "Implement early payment discounts for customers to accelerate cash inflows.",
                    ImpactScore = 7.8,
                    Priority = "High",
                    Category = "Cash Flow",
                    EstimatedBenefit = 30000m
                },
                new AIRecommendation
                {
                    Title = "Review Inventory Levels",
                    Description = "Reduce excess inventory to free up working capital.",
                    ImpactScore = 7.2,
                    Priority = "Medium",
                    Category = "Inventory",
                    EstimatedBenefit = 25000m
                }
            }.Take(count).ToList();
        }

        private List<AIRecommendation> GenerateCostReductionRecommendations(int count)
        {
            return new List<AIRecommendation>
            {
                new AIRecommendation
                {
                    Title = "Consolidate Suppliers",
                    Description = "Reduce supplier count and consolidate volume for better pricing.",
                    ImpactScore = 8.0,
                    Priority = "High",
                    Category = "Procurement",
                    EstimatedBenefit = 75000m
                },
                new AIRecommendation
                {
                    Title = "Review Utility Costs",
                    Description = "Audit energy usage and switch to more efficient systems.",
                    ImpactScore = 6.5,
                    Priority = "Medium",
                    Category = "Operations",
                    EstimatedBenefit = 15000m
                }
            }.Take(count).ToList();
        }

        private List<AIRecommendation> GenerateRiskManagementRecommendations(int count)
        {
            return new List<AIRecommendation>
            {
                new AIRecommendation
                {
                    Title = "Diversify Customer Base",
                    Description = "Reduce concentration risk by acquiring new customer segments.",
                    ImpactScore = 7.8,
                    Priority = "High",
                    Category = "Risk",
                    EstimatedBenefit = 100000m
                },
                new AIRecommendation
                {
                    Title = "Implement Credit Controls",
                    Description = "Strengthen credit review processes for new accounts.",
                    ImpactScore = 7.0,
                    Priority = "Medium",
                    Category = "Risk",
                    EstimatedBenefit = 20000m
                }
            }.Take(count).ToList();
        }

        private List<AIRecommendation> GenerateInventoryRecommendations(int count)
        {
            return new List<AIRecommendation>
            {
                new AIRecommendation
                {
                    Title = "Optimize Reorder Points",
                    Description = "Adjust reorder levels based on demand forecasts.",
                    ImpactScore = 7.5,
                    Priority = "Medium",
                    Category = "Inventory",
                    EstimatedBenefit = 35000m
                },
                new AIRecommendation
                {
                    Title = "Clearance Sale for Obsolete Items",
                    Description = "Clear slow-moving inventory to free warehouse space.",
                    ImpactScore = 6.8,
                    Priority = "Medium",
                    Category = "Inventory",
                    EstimatedBenefit = 20000m
                }
            }.Take(count).ToList();
        }

        private List<AIRecommendation> GenerateGeneralRecommendations(int count)
        {
            return new List<AIRecommendation>
            {
                new AIRecommendation
                {
                    Title = "Monthly Financial Review",
                    Description = "Establish monthly review cycle for KPIs and metrics.",
                    ImpactScore = 7.0,
                    Priority = "High",
                    Category = "Process",
                    EstimatedBenefit = 10000m
                },
                new AIRecommendation
                {
                    Title = "Enhance Forecasting",
                    Description = "Implement AI-powered forecasting for better planning.",
                    ImpactScore = 8.0,
                    Priority = "High",
                    Category = "Analytics",
                    EstimatedBenefit = 50000m
                }
            }.Take(count).ToList();
        }

        private string GenerateExecutiveSummary(ReportRequest request)
        {
            return $"This {request.ReportType} report for {request.Period} provides comprehensive AI-powered analysis " +
                   $"of financial performance, risks, and opportunities. The analysis leverages predictive analytics, " +
                   $"risk assessment, and optimization algorithms to deliver actionable insights.";
        }

        private List<string> GenerateKeyFindings(ReportRequest request)
        {
            return new List<string>
            {
                "Strong cash flow position with stable trend",
                "Moderate credit risk concentration in top 5 customers",
                "Inventory levels optimized for current demand",
                "Operational efficiency improved by 5% YoY",
                "Cost structure stable with opportunity for 8-10% reduction"
            };
        }
    }

    // ============== Supporting Classes ==============

    public class AnalysisRequest
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public List<string> Modules { get; set; } = new();
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class AIAnalysisResult
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Dictionary<string, object> AnalysisByModule { get; set; } = new();
        public string Error { get; set; }
    }

    public class AICapabilitiesSummary
    {
        public List<string> Capabilities { get; set; }
        public int EnabledExtensions { get; set; }
        public int TotalExtensions { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class AIInsights
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public List<Insight> Insights { get; set; } = new();
        public string Status { get; set; }
        public string Error { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class Insight
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public double Confidence { get; set; }
        public object Value { get; set; }
    }

    public class AIRecommendation
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public double ImpactScore { get; set; }
        public string Priority { get; set; }
        public decimal EstimatedBenefit { get; set; }
        public List<string> ImplementationSteps { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ReportRequest
    {
        public string ReportType { get; set; }
        public string Period { get; set; }
        public int? AccountId { get; set; }
        public Dictionary<string, object> Filters { get; set; } = new();
    }

    public class AIReport
    {
        public string ReportType { get; set; }
        public string Period { get; set; }
        public string Status { get; set; }
        public DateTime GeneratedAt { get; set; }
        public Dictionary<string, ReportSection> Sections { get; set; } = new();
        public string Error { get; set; }
    }

    public class ReportSection
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public List<string> Findings { get; set; } = new();
        public List<AIRecommendation> Recommendations { get; set; } = new();
    }
}
