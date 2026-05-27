using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FinancialSystem.Models;
using HttpClient = System.Net.Http.HttpClient;

namespace FinancialSystemApp.Services
{
    /// <summary>
    /// Service for AI-powered financial insights and analysis
    /// Integrates with Anthropic Claude API
    /// </summary>
    public interface IFinancialAIService
    {
        Task<string> GenerateTransactionInsight(Transaction transaction, List<Transaction> historicalData);
        Task<string> GenerateAccountAnalysis(Account account, List<Transaction> transactions);
        Task<List<string>> GenerateRecommendations(Account account, List<Transaction> transactions, decimal budgetUtilization);
        Task<string> PredictExpenseTrends(List<Transaction> transactions, int forecastMonths = 3);
        Task<string> GenerateBudgetAlert(Account account, decimal currentSpending);
    }

    public class FinancialAIService : IFinancialAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";

        public FinancialAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Anthropic:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }

        public async Task<string> GenerateTransactionInsight(Transaction transaction, List<Transaction> historicalData)
        {
            var prompt = BuildTransactionInsightPrompt(transaction, historicalData);
            return await CallClaudeAPI(prompt);
        }

        public async Task<string> GenerateAccountAnalysis(Account account, List<Transaction> transactions)
        {
            var summary = GenerateTransactionSummary(transactions);
            var prompt = $@"
Analyze this financial account data and provide key insights:

Account: {account.Name}
Budget Limit: ${account.BudgetLimit:F2}
Current Balance: ${account.CurrentBalance:F2}
Budget Utilization: {((account.BudgetLimit - account.CurrentBalance) / account.BudgetLimit * 100):F1}%

Transaction Summary:
{summary}

Provide a concise analysis (2-3 sentences) covering:
1. Overall financial health
2. Spending patterns
3. Budget adherence

Be professional and actionable.";

            return await CallClaudeAPI(prompt);
        }

        public async Task<List<string>> GenerateRecommendations(Account account, List<Transaction> transactions, decimal budgetUtilization)
        {
            var topExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .OrderByDescending(g => g.Sum(t => t.Amount))
                .Take(5)
                .Select(g => $"{g.Key}: ${g.Sum(t => t.Amount):F2}")
                .ToList();

            var prompt = $@"
Generate 3-4 specific, actionable financial recommendations for this account:

Account: {account.Name}
Budget: ${account.BudgetLimit:F2}
Spending: {budgetUtilization:F1}% of budget
Budget Remaining: ${account.BudgetLimit - account.CurrentBalance:F2}

Top Expense Categories:
{string.Join("\n", topExpenses)}

Provide recommendations as a JSON array of strings, each 1-2 sentences.
Format: [""Recommendation 1"", ""Recommendation 2"", ""Recommendation 3""]

Focus on:
- Cost optimization
- Budget allocation
- Spending reduction opportunities";

            var response = await CallClaudeAPI(prompt);
            return ParseRecommendationsJson(response);
        }

        public async Task<string> PredictExpenseTrends(List<Transaction> transactions, int forecastMonths = 3)
        {
            var monthlyExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:D2}", Total = g.Sum(t => t.Amount) })
                .OrderBy(x => x.Month)
                .ToList();

            var prompt = $@"
Based on this historical expense data, predict trends for the next {forecastMonths} months:

Historical Monthly Expenses:
{string.Join("\n", monthlyExpenses.Select(m => $"{m.Month}: ${m.Total:F2}"))}

Provide:
1. Trend analysis (increasing/decreasing/stable)
2. Predicted average monthly expense
3. Factors influencing the trend
4. Recommendation (1-2 sentences)

Be concise and data-driven.";

            return await CallClaudeAPI(prompt);
        }

        public async Task<string> GenerateBudgetAlert(Account account, decimal currentSpending)
        {
            var utilizationPercent = (currentSpending / account.BudgetLimit) * 100;
            var remaining = account.BudgetLimit - currentSpending;

            var prompt = $@"
Generate a brief, professional budget alert message:

Account: {account.Name}
Budget: ${account.BudgetLimit:F2}
Current Spending: ${currentSpending:F2}
Utilization: {utilizationPercent:F1}%
Remaining: ${remaining:F2}

Create an alert message (1-2 sentences) that is:
- Clear and professional
- Action-oriented
- Specific to the threshold

Alert only if utilization is above 75%.";

            return await CallClaudeAPI(prompt);
        }

        private async Task<string> CallClaudeAPI(string prompt)
        {
            try
            {
                var request = new
                {
                    model = "claude-opus-4-1",
                    max_tokens = 500,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(ApiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

                return jsonResponse
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch (Exception ex)
            {
                // Fallback response if API fails
                return $"Analysis unavailable: {ex.Message}";
            }
        }

        private string BuildTransactionInsightPrompt(Transaction transaction, List<Transaction> historicalData)
        {
            var similarTransactions = historicalData
                .Where(t => t.Category == transaction.Category && t.Type == transaction.Type)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToList();

            var avgAmount = similarTransactions.Any() 
                ? similarTransactions.Average(t => t.Amount) 
                : transaction.Amount;

            var prompt = $@"
Analyze this financial transaction:

Transaction: {transaction.Description}
Amount: ${transaction.Amount:F2}
Type: {transaction.Type}
Category: {transaction.Category}
Date: {transaction.Date:yyyy-MM-dd}

Similar Transactions (avg): ${avgAmount:F2}
Historical Count: {similarTransactions.Count}

Provide a brief insight (1-2 sentences) about this transaction.
Consider if the amount is unusual, typical, or noteworthy.";

            return prompt;
        }

        private string GenerateTransactionSummary(List<Transaction> transactions)
        {
            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            
            var topCategories = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .OrderByDescending(g => g.Sum(t => t.Amount))
                .Take(3)
                .Select(g => $"{g.Key}: ${g.Sum(t => t.Amount):F2}")
                .ToList();

            return $@"Total Income: ${income:F2}
Total Expenses: ${expenses:F2}
Net: ${income - expenses:F2}
Transaction Count: {transactions.Count}

Top Expense Categories:
{string.Join("\n", topCategories)}";
        }

        private List<string> ParseRecommendationsJson(string jsonResponse)
        {
            try
            {
                var jsonArray = JsonSerializer.Deserialize<List<string>>(jsonResponse);
                return jsonArray ?? new List<string> { "Recommendations unavailable" };
            }
            catch
            {
                return new List<string> { jsonResponse };
            }
        }
    }
}
