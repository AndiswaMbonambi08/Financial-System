using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialSystemApp.Services;
using FinancialSystemApp.AI.Extensions;
using FinancialSystemApp.Models.ERP;


namespace FinancialSystemApp.Controllers
{
    /// <summary>
    /// Sales Module Controller - Handles customers, invoices, and sales orders
    /// Integrated with AI-powered credit risk assessment and revenue forecasting
    /// </summary>
    [ApiController]
    [Route("api/sales")]
    [Produces("application/json")]
    public class SalesController : ControllerBase
    {
        private readonly ILogger<SalesController> _logger;
        private readonly IRiskAssessmentExtension _riskAssessment;
        private readonly IPredictiveAnalyticsExtension _predictiveAnalytics;
        private readonly IAIOrchestrator _aiOrchestrator;

        public SalesController(
            ILogger<SalesController> logger,
            IRiskAssessmentExtension riskAssessment,
            IPredictiveAnalyticsExtension predictiveAnalytics,
            IAIOrchestrator aiOrchestrator)
        {
            _logger = logger;
            _riskAssessment = riskAssessment;
            _predictiveAnalytics = predictiveAnalytics;
            _aiOrchestrator = aiOrchestrator;
        }

        /// <summary>
        /// Get all customers with risk assessments
        /// </summary>
        [HttpGet("customers")]
        [ProducesResponseType(typeof(List<CustomerWithRisk>), 200)]
        public async Task<ActionResult<List<CustomerWithRisk>>> GetCustomers()
        {
            try
            {
                _logger.LogInformation("Fetching all customers");
                
                // In a real system, this would query the database
                var customers = new List<CustomerWithRisk>
                {
                    new CustomerWithRisk
                    {
                        CustomerId = 1,
                        CustomerName = "ABC Corporation",
                        Email = "contact@abc.com",
                        CreditLimit = 100000,
                        CurrentBalance = 45000,
                        Status = "active"
                    }
                };

                // Get risk assessment for each customer
                foreach (var customer in customers)
                {
                    try
                    {
                        var risk = await _riskAssessment.AssessCustomerCreditRiskAsync(customer.CustomerId);
                        customer.RiskScore = risk.RiskScore;
                        customer.RiskLevel = risk.RiskLevel;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Risk assessment failed for customer {customer.CustomerId}: {ex.Message}");
                    }
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customers");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get customer details with AI insights
        /// </summary>
        [HttpGet("customers/{id}/insights")]
        [ProducesResponseType(typeof(CustomerInsights), 200)]
        public async Task<ActionResult<CustomerInsights>> GetCustomerInsights(int id)
        {
            try
            {
                _logger.LogInformation($"Getting insights for customer {id}");

                var insights = await _aiOrchestrator.GetEntityInsightsAsync("Customer", id);

                return Ok(new CustomerInsights
                {
                    CustomerId = id,
                    Insights = insights.Insights,
                    RiskAssessment = await _riskAssessment.AssessCustomerCreditRiskAsync(id),
                    RevenueProjection = await _predictiveAnalytics.ProjectRevenueAsync(id, 12),
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer insights: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create new sales order with AI validation
        /// </summary>
        [HttpPost("orders")]
        [ProducesResponseType(typeof(SalesOrderResponse), 201)]
        public async Task<ActionResult<SalesOrderResponse>> CreateOrder([FromBody] CreateSalesOrderRequest request)
        {
            try
            {
                _logger.LogInformation($"Creating sales order for customer {request.CustomerId}");

                // Validate customer credit risk
                var riskAssessment = await _riskAssessment.AssessCustomerCreditRiskAsync(request.CustomerId);
                
                if (riskAssessment.RiskLevel == "Critical")
                {
                    return BadRequest(new
                    {
                        error = "Cannot create order",
                        reason = "Customer credit risk is critical",
                        riskScore = riskAssessment.RiskScore
                    });
                }

                // Create order
                var orderId = new Random().Next(10000, 99999);
                var response = new SalesOrderResponse
                {
                    OrderId = orderId,
                    CustomerId = request.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = request.OrderItems.Count * 1000,
                    Status = "created",
                    RiskAssessment = new
                    {
                        riskScore = riskAssessment.RiskScore,
                        riskLevel = riskAssessment.RiskLevel,
                        approved = riskAssessment.RiskLevel != "High"
                    }
                };

                _logger.LogInformation($"Order {orderId} created successfully");
                return CreatedAtAction(nameof(GetOrder), new { id = orderId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sales order");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get sales order details
        /// </summary>
        [HttpGet("orders/{id}")]
        [ProducesResponseType(typeof(SalesOrderResponse), 200)]
        public ActionResult<SalesOrderResponse> GetOrder(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching order {id}");
                return Ok(new SalesOrderResponse
                {
                    OrderId = id,
                    CustomerId = 1,
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    TotalAmount = 50000,
                    Status = "confirmed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get AI recommendations for sales optimization
        /// </summary>
        [HttpGet("recommendations")]
        [ProducesResponseType(typeof(List<SalesRecommendation>), 200)]
        public async Task<ActionResult<List<SalesRecommendation>>> GetSalesRecommendations()
        {
            try
            {
                _logger.LogInformation("Getting sales recommendations");

                var aiRecommendations = await _aiOrchestrator.GetRecommendationsAsync("Revenue Optimization", 5);

                var recommendations = new List<SalesRecommendation>();
                foreach (var rec in aiRecommendations)
                {
                    recommendations.Add(new SalesRecommendation
                    {
                        Title = rec.Title,
                        Description = rec.Description,
                        ImpactScore = rec.ImpactScore,
                        Priority = rec.Priority,
                        EstimatedRevenueLift = rec.EstimatedBenefit
                    });
                }

                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Generate sales dashboard with AI insights
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(SalesDashboard), 200)]
        public async Task<ActionResult<SalesDashboard>> GetSalesDashboard()
        {
            try
            {
                _logger.LogInformation("Generating sales dashboard");

                var capabilities = await _aiOrchestrator.GetAvailableCapabilitiesAsync();

                return Ok(new SalesDashboard
                {
                    TotalCustomers = 12,
                    ActiveOrders = 8,
                    PendingInvoices = 15,
                    TotalRevenue = 1250000,
                    AvgOrderValue = 78125,
                    AiCapabilities = capabilities.Capabilities,
                    TopRisks = new List<string>
                    {
                        "High concentration in 3 customers",
                        "Extended payment terms increasing",
                        "Seasonal demand volatility"
                    },
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // ============== Sales Request/Response Models ==============

    public class CustomerWithRisk
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Status { get; set; }
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; }
    }

    public class CustomerInsights
    {
        public int CustomerId { get; set; }
        public List<FinancialSystem.AI.Extensions.Insight> Insights { get; set; }
        public RiskAssessment RiskAssessment { get; set; }
        public RevenueProjection RevenueProjection { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CreateSalesOrderRequest
    {
        public int CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public string DeliveryAddress { get; set; }
    }

    public class OrderItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class SalesOrderResponse
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public object RiskAssessment { get; set; }
    }

    public class SalesRecommendation
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double ImpactScore { get; set; }
        public string Priority { get; set; }
        public decimal EstimatedRevenueLift { get; set; }
    }

    public class SalesDashboard
    {
        public int TotalCustomers { get; set; }
        public int ActiveOrders { get; set; }
        public int PendingInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public List<string> AiCapabilities { get; set; }
        public List<string> TopRisks { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
