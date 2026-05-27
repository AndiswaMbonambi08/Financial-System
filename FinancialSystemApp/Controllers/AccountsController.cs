using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinancialSystem.Models;
using FinancialSystemApp.Services;
using FinancialSystemApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystemApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly FinancialDbContext _context;
        private readonly ITransactionService _transactionService;
        private readonly IReportService _reportService;

        public AccountsController(
            FinancialDbContext context,
            ITransactionService transactionService,
            IReportService reportService)
        {
            _context = context;
            _transactionService = transactionService;
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Account>>> GetAccounts()
        {
            var accounts = await _context.Accounts
                .Where(a => a.Status == AccountStatus.Active)
                .ToListAsync();

            // Calculate summaries
            foreach (var account in accounts)
            {
                var (totalIncome, totalExpenses) = await _transactionService.GetAccountSummaryAsync(account.Id);
                account.TotalIncome = totalIncome;
                account.TotalExpenses = totalExpenses;
                account.NetBalance = totalIncome - totalExpenses;
                account.BudgetUtilizationPercentage = account.BudgetLimit > 0
                    ? ((account.BudgetLimit - account.CurrentBalance) / account.BudgetLimit) * 100
                    : 0;
            }

            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
                return NotFound();

            var (totalIncome, totalExpenses) = await _transactionService.GetAccountSummaryAsync(account.Id);
            account.TotalIncome = totalIncome;
            account.TotalExpenses = totalExpenses;
            account.NetBalance = totalIncome - totalExpenses;
            account.BudgetUtilizationPercentage = account.BudgetLimit > 0
                ? ((account.BudgetLimit - account.CurrentBalance) / account.BudgetLimit) * 100
                : 0;

            return Ok(account);
        }

        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount(Account account)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            account.CreatedDate = DateTime.UtcNow;
            account.Status = AccountStatus.Active;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, Account account)
        {
            if (id != account.Id)
                return BadRequest();

            var existingAccount = await _context.Accounts.FindAsync(id);
            if (existingAccount == null)
                return NotFound();

            existingAccount.Name = account.Name;
            existingAccount.Description = account.Description;
            existingAccount.BudgetLimit = account.BudgetLimit;
            existingAccount.ModifiedDate = DateTime.UtcNow;

            _context.Accounts.Update(existingAccount);
            await _context.SaveChangesAsync();

            return Ok(existingAccount);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            account.Status = AccountStatus.Archived;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/transactions")]
        public async Task<ActionResult<List<Transaction>>> GetAccountTransactions(int id, 
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            List<Transaction> transactions;

            if (startDate.HasValue && endDate.HasValue)
            {
                transactions = await _transactionService.GetTransactionsByDateRangeAsync(id, startDate.Value, endDate.Value);
            }
            else
            {
                transactions = await _transactionService.GetTransactionsByAccountAsync(id);
            }

            return Ok(transactions);
        }

        [HttpGet("{id}/summary")]
        public async Task<ActionResult<dynamic>> GetAccountSummary(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            var (totalIncome, totalExpenses) = await _transactionService.GetAccountSummaryAsync(id);
            var alerts = await _context.BudgetAlerts
                .Where(a => a.AccountId == id && a.IsActive)
                .ToListAsync();

            return Ok(new
            {
                Account = new { account.Id, account.Name, account.CurrentBalance, account.BudgetLimit },
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetBalance = totalIncome - totalExpenses,
                BudgetUtilization = ((account.BudgetLimit - account.CurrentBalance) / account.BudgetLimit) * 100,
                ActiveAlerts = alerts.Count
            });
        }

        [HttpGet("{id}/reports")]
        public async Task<ActionResult<List<Report>>> GetAccountReports(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            var reports = await _reportService.GetAccountReportsAsync(id);
            return Ok(reports);
        }

        [HttpPost("{id}/generate-report")]
        public async Task<ActionResult<Report>> GenerateReport(int id, [FromQuery] ReportType type,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            Report report = type switch
            {
                ReportType.IncomeStatement => await _reportService.GenerateIncomeStatementAsync(id, start, end),
                ReportType.ExpenseBreakdown => await _reportService.GenerateExpenseBreakdownAsync(id, start, end),
                ReportType.CashFlow => await _reportService.GenerateCashFlowReportAsync(id, start, end),
                ReportType.BudgetComparison => await _reportService.GenerateBudgetComparisonAsync(id, start, end),
                _ => null
            };

            if (report == null)
                return BadRequest("Invalid report type");

            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }

        [HttpGet("reports/{reportId}")]
        public async Task<ActionResult<Report>> GetReport(int reportId)
        {
            var report = await _reportService.GetReportAsync(reportId);
            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpDelete("reports/{reportId}")]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            var success = await _reportService.DeleteReportAsync(reportId);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
