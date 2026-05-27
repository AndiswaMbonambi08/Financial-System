using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinancialSystemApp.Models;
using FinancialSystemApp.Services;

namespace FinancialSystemApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<List<Transaction>>> GetTransactionsByAccount(int accountId)
        {
            var transactions = await _transactionService.GetTransactionsByAccountAsync(accountId);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _transactionService.GetTransactionAsync(id);
            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [HttpPost]
        public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetTransaction), new { id = createdTransaction.Id }, createdTransaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, Transaction transaction)
        {
            if (id != transaction.Id)
                return BadRequest();

            var existingTransaction = await _transactionService.GetTransactionAsync(id);
            if (existingTransaction == null)
                return NotFound();

            var updatedTransaction = await _transactionService.UpdateTransactionAsync(transaction);
            return Ok(updatedTransaction);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var success = await _transactionService.DeleteTransactionAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("account/{accountId}/category/{category}")]
        public async Task<ActionResult<List<Transaction>>> GetTransactionsByCategory(int accountId, TransactionCategory category)
        {
            var transactions = await _transactionService.GetTransactionsByCategoryAsync(accountId, category);
            return Ok(transactions);
        }

        [HttpGet("account/{accountId}/date-range")]
        public async Task<ActionResult<List<Transaction>>> GetTransactionsByDateRange(
            int accountId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
            return Ok(transactions);
        }

        [HttpPost("process-recurring")]
        public async Task<IActionResult> ProcessRecurringTransactions()
        {
            await _transactionService.ProcessRecurringTransactionsAsync();
            return Ok("Recurring transactions processed");
        }
    }
}
