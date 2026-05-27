using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialSystemApp.AI.Extensions;
using FinancialSystemApp.Models.ERP;

namespace FinancialSystemApp.Controllers
{
    /// <summary>
    /// General Ledger Controller - Core accounting module
    /// Handles chart of accounts, journal entries, posting, trial balance, and GL analysis
    /// Integrated with AI-powered anomaly detection and compliance checking
    /// </summary>
    [ApiController]
    [Route("api/gl")]
    [Produces("application/json")]
    public class GeneralLedgerController : ControllerBase
    {
        private readonly ILogger<GeneralLedgerController> _logger;
        private readonly IAIOrchestrator _aiOrchestrator;

        public GeneralLedgerController(
            ILogger<GeneralLedgerController> logger,
            IAIOrchestrator aiOrchestrator)
        {
            _logger = logger;
            _aiOrchestrator = aiOrchestrator;
        }

        /// <summary>
        /// Get chart of accounts with balances
        /// </summary>
        [HttpGet("accounts")]
        [ProducesResponseType(typeof(List<GLAccountDetail>), 200)]
        public ActionResult<List<GLAccountDetail>> GetChartOfAccounts([FromQuery] string accountType = null)
        {
            try
            {
                _logger.LogInformation("Fetching chart of accounts");

                var accounts = new List<GLAccountDetail>
                {
                    new GLAccountDetail
                    {
                        AccountNumber = "1000",
                        AccountName = "Cash",
                        AccountType = "Asset",
                        Balance = 250000,
                        Currency = "USD",
                        Active = true
                    },
                    new GLAccountDetail
                    {
                        AccountNumber = "2000",
                        AccountName = "Accounts Payable",
                        AccountType = "Liability",
                        Balance = -75000,
                        Currency = "USD",
                        Active = true
                    },
                    new GLAccountDetail
                    {
                        AccountNumber = "4000",
                        AccountName = "Sales Revenue",
                        AccountType = "Revenue",
                        Balance = 500000,
                        Currency = "USD",
                        Active = true
                    },
                    new GLAccountDetail
                    {
                        AccountNumber = "5000",
                        AccountName = "Cost of Goods Sold",
                        AccountType = "Expense",
                        Balance = -300000,
                        Currency = "USD",
                        Active = true
                    }
                };

                if (!string.IsNullOrEmpty(accountType))
                {
                    accounts = accounts.FindAll(a => a.AccountType == accountType);
                }

                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chart of accounts");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get account details with historical balances
        /// </summary>
        [HttpGet("accounts/{accountNumber}")]
        [ProducesResponseType(typeof(GLAccountDetail), 200)]
        public ActionResult<GLAccountDetail> GetAccountDetail(string accountNumber)
        {
            try
            {
                _logger.LogInformation($"Fetching account {accountNumber}");

                return Ok(new GLAccountDetail
                {
                    AccountNumber = accountNumber,
                    AccountName = $"Account {accountNumber}",
                    AccountType = "Asset",
                    Balance = 150000,
                    Currency = "USD",
                    Active = true,
                    HistoricalBalances = new List<HistoricalBalance>
                    {
                        new HistoricalBalance { Period = "2024-01", Balance = 120000 },
                        new HistoricalBalance { Period = "2024-02", Balance = 135000 },
                        new HistoricalBalance { Period = "2024-03", Balance = 150000 }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching account {accountNumber}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Post journal entry
        /// </summary>
        [HttpPost("journal-entries")]
        [ProducesResponseType(typeof(JournalEntryResponse), 201)]
        public async Task<ActionResult<JournalEntryResponse>> PostJournalEntry([FromBody] CreateJournalEntryRequest request)
        {
            try
            {
                _logger.LogInformation($"Posting journal entry: {request.Description}");

                // Validate debit/credit balance
                decimal totalDebits = 0;
                decimal totalCredits = 0;

                foreach (var line in request.Lines)
                {
                    if (line.DebitAmount > 0)
                        totalDebits += line.DebitAmount;
                    if (line.CreditAmount > 0)
                        totalCredits += line.CreditAmount;
                }

                if (totalDebits != totalCredits)
                {
                    return BadRequest(new
                    {
                        error = "Journal entry out of balance",
                        debits = totalDebits,
                        credits = totalCredits,
                        difference = Math.Abs(totalDebits - totalCredits)
                    });
                }

                var entryId = new Random().Next(10000, 99999);
                var response = new JournalEntryResponse
                {
                    EntryId = entryId,
                    EntryDate = DateTime.UtcNow,
                    Description = request.Description,
                    Reference = request.Reference,
                    TotalAmount = totalDebits,
                    Status = "posted",
                    LineItems = request.Lines.Count
                };

                _logger.LogInformation($"Journal entry {entryId} posted successfully");
                return CreatedAtAction(nameof(GetJournalEntry), new { id = entryId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting journal entry");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get journal entry details
        /// </summary>
        [HttpGet("journal-entries/{id}")]
        [ProducesResponseType(typeof(JournalEntryResponse), 200)]
        public ActionResult<JournalEntryResponse> GetJournalEntry(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching journal entry {id}");

                return Ok(new JournalEntryResponse
                {
                    EntryId = id,
                    EntryDate = DateTime.UtcNow.AddDays(-1),
                    Description = "Sales transaction",
                    Reference = "SO-12345",
                    TotalAmount = 50000,
                    Status = "posted",
                    LineItems = 2
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching journal entry: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Generate trial balance
        /// </summary>
        [HttpGet("trial-balance")]
        [ProducesResponseType(typeof(TrialBalance), 200)]
        public ActionResult<TrialBalance> GetTrialBalance([FromQuery] string period = "current")
        {
            try
            {
                _logger.LogInformation($"Generating trial balance for period: {period}");

                var accounts = new List<TrialBalanceAccount>
                {
                    new TrialBalanceAccount
                    {
                        AccountNumber = "1000",
                        AccountName = "Cash",
                        Debit = 250000,
                        Credit = 0
                    },
                    new TrialBalanceAccount
                    {
                        AccountNumber = "2000",
                        AccountName = "Accounts Payable",
                        Debit = 0,
                        Credit = 75000
                    },
                    new TrialBalanceAccount
                    {
                        AccountNumber = "4000",
                        AccountName = "Sales Revenue",
                        Debit = 0,
                        Credit = 500000
                    },
                    new TrialBalanceAccount
                    {
                        AccountNumber = "5000",
                        AccountName = "COGS",
                        Debit = 300000,
                        Credit = 0
                    }
                };

                decimal totalDebits = 0;
                decimal totalCredits = 0;

                foreach (var account in accounts)
                {
                    totalDebits += account.Debit;
                    totalCredits += account.Credit;
                }

                return Ok(new TrialBalance
                {
                    Period = period,
                    GeneratedDate = DateTime.UtcNow,
                    Accounts = accounts,
                    TotalDebits = totalDebits,
                    TotalCredits = totalCredits,
                    IsBalanced = Math.Abs(totalDebits - totalCredits) < 0.01m,
                    AccountCount = accounts.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trial balance");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Generate financial statements (Income Statement, Balance Sheet)
        /// </summary>
        [HttpGet("financial-statements")]
        [ProducesResponseType(typeof(FinancialStatements), 200)]
        public async Task<ActionResult<FinancialStatements>> GetFinancialStatements([FromQuery] string period = "2024-Q1")
        {
            try
            {
                _logger.LogInformation($"Generating financial statements for period: {period}");

                var statements = new FinancialStatements
                {
                    Period = period,
                    GeneratedDate = DateTime.UtcNow,
                    IncomeStatement = new IncomeStatement
                    {
                        TotalRevenue = 500000,
                        CostOfGoodsSold = 300000,
                        GrossProfit = 200000,
                        OperatingExpenses = 75000,
                        OperatingIncome = 125000,
                        TaxExpense = 25000,
                        NetIncome = 100000
                    },
                    BalanceSheet = new BalanceSheet
                    {
                        TotalAssets = 500000,
                        TotalLiabilities = 200000,
                        TotalEquity = 300000
                    }
                };

                // Get AI-powered analysis
                var report = await _aiOrchestrator.GenerateReportAsync(new ReportRequest
                {
                    ReportType = "Financial Analysis",
                    Period = period
                });

                statements.AIAnalysis = new
                {
                    Status = report.Status,
                    GeneratedAt = report.GeneratedAt,
                    Sections = report.Sections.Keys
                };

                return Ok(statements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial statements");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get GL anomalies detected by AI
        /// </summary>
        [HttpGet("anomalies")]
        [ProducesResponseType(typeof(GLAnomalies), 200)]
        public ActionResult<GLAnomalies> GetGLAnomalies([FromQuery] int days = 30)
        {
            try
            {
                _logger.LogInformation($"Analyzing GL for anomalies in last {days} days");

                var anomalies = new GLAnomalies
                {
                    Period = $"Last {days} days",
                    AnalysisDate = DateTime.UtcNow,
                    AnomaliesFound = 2,
                    Details = new List<AnomalyDetail>
                    {
                        new AnomalyDetail
                        {
                            Type = "Large Unusual Amount",
                            AccountNumber = "4000",
                            AccountName = "Sales Revenue",
                            Amount = 500000,
                            Date = DateTime.UtcNow.AddDays(-5),
                            AnomalyScore = 0.75,
                            Description = "Transaction amount 3x average",
                            SuggestedAction = "Review transaction source documentation"
                        },
                        new AnomalyDetail
                        {
                            Type = "Unusual Account Pairing",
                            AccountNumber = "1000 - 2000",
                            AccountName = "Cash to AP",
                            Amount = 75000,
                            Date = DateTime.UtcNow.AddDays(-2),
                            AnomalyScore = 0.45,
                            Description = "Unusual account combination",
                            SuggestedAction = "Verify business purpose"
                        }
                    }
                };

                return Ok(anomalies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing anomalies");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get GL reconciliation status and AI recommendations
        /// </summary>
        [HttpGet("reconciliation-status")]
        [ProducesResponseType(typeof(ReconciliationStatus), 200)]
        public async Task<ActionResult<ReconciliationStatus>> GetReconciliationStatus()
        {
            try
            {
                _logger.LogInformation("Checking reconciliation status");

                var status = new ReconciliationStatus
                {
                    CheckDate = DateTime.UtcNow,
                    Status = "In Progress",
                    CompletionPercentage = 85,
                    AccountsReconciled = 18,
                    AccountsPending = 3,
                    OpenItems = new List<OpenReconciliationItem>
                    {
                        new OpenReconciliationItem
                        {
                            ItemId = 1,
                            Description = "Bank reconciliation - Pending deposits",
                            Amount = 5000,
                            DaysOpen = 3,
                            Priority = "High"
                        }
                    }
                };

                // Get AI recommendations
                var recommendations = await _aiOrchestrator.GetRecommendationsAsync("GL Reconciliation", 3);
                status.Recommendations = recommendations;

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reconciliation status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get GL dashboard with KPIs and AI insights
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(GLDashboard), 200)]
        public async Task<ActionResult<GLDashboard>> GetGLDashboard()
        {
            try
            {
                _logger.LogInformation("Generating GL dashboard");

                var dashboard = new GLDashboard
                {
                    GeneratedAt = DateTime.UtcNow,
                    TotalAccounts = 50,
                    AccountsReconciled = 48,
                    PendingAdjustments = 2,
                    TotalDebits = 1000000,
                    TotalCredits = 1000000,
                    IsBalanced = true,
                    KeyMetrics = new Dictionary<string, object>
                    {
                        { "Current Ratio", 1.5 },
                        { "Working Capital", 150000 },
                        { "Debt to Equity", 0.67 },
                        { "ROA", 0.22 }
                    },
                    RecentEntries = 25,
                    UnpostedEntries = 3,
                    ComplianceStatus = "90% compliant"
                };

                // Get AI insights
                var insights = await _aiOrchestrator.GetEntityInsightsAsync("GeneralLedger", 1);
                dashboard.AIInsights = insights.Insights;

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating GL dashboard");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // ============== GL Request/Response Models ==============

    public class GLAccountDetail
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public bool Active { get; set; }
        public List<HistoricalBalance> HistoricalBalances { get; set; } = new();
    }

    public class HistoricalBalance
    {
        public string Period { get; set; }
        public decimal Balance { get; set; }
    }

    public class CreateJournalEntryRequest
    {
        public string Description { get; set; }
        public string Reference { get; set; }
        public List<JournalEntryLine> Lines { get; set; }
    }

    public class JournalEntryLine
    {
        public string AccountNumber { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public class JournalEntryResponse
    {
        public int EntryId { get; set; }
        public DateTime EntryDate { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int LineItems { get; set; }
    }

    public class TrialBalance
    {
        public string Period { get; set; }
        public DateTime GeneratedDate { get; set; }
        public List<TrialBalanceAccount> Accounts { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced { get; set; }
        public int AccountCount { get; set; }
    }

    public class TrialBalanceAccount
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class FinancialStatements
    {
        public string Period { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IncomeStatement IncomeStatement { get; set; }
        public BalanceSheet BalanceSheet { get; set; }
        public object AIAnalysis { get; set; }
    }

    public class IncomeStatement
    {
        public decimal TotalRevenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal OperatingIncome { get; set; }
        public decimal TaxExpense { get; set; }
        public decimal NetIncome { get; set; }
    }

    public class BalanceSheet
    {
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
    }

    public class GLAnomalies
    {
        public string Period { get; set; }
        public DateTime AnalysisDate { get; set; }
        public int AnomaliesFound { get; set; }
        public List<AnomalyDetail> Details { get; set; }
    }

    public class AnomalyDetail
    {
        public string Type { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public double AnomalyScore { get; set; }
        public string Description { get; set; }
        public string SuggestedAction { get; set; }
    }

    public class ReconciliationStatus
    {
        public DateTime CheckDate { get; set; }
        public string Status { get; set; }
        public int CompletionPercentage { get; set; }
        public int AccountsReconciled { get; set; }
        public int AccountsPending { get; set; }
        public List<OpenReconciliationItem> OpenItems { get; set; }
        public List<FinancialSystem.AI.Extensions.AIRecommendation> Recommendations { get; set; }
    }

    public class OpenReconciliationItem
    {
        public int ItemId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int DaysOpen { get; set; }
        public string Priority { get; set; }
    }

    public class GLDashboard
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalAccounts { get; set; }
        public int AccountsReconciled { get; set; }
        public int PendingAdjustments { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced { get; set; }
        public Dictionary<string, object> KeyMetrics { get; set; }
        public int RecentEntries { get; set; }
        public int UnpostedEntries { get; set; }
        public string ComplianceStatus { get; set; }
        public List<FinancialSystem.AI.Extensions.Insight> AIInsights { get; set; }
    }
}
