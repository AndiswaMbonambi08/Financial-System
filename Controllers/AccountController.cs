using Microsoft.AspNetCore.Mvc;
using Financial_System.Services;
using System.Threading.Tasks;
using Financial_System.Models;


namespace Financial_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;

        public AccountController(
            IAccountService accountService,
            ITransactionService transactionService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return View(accounts);
        }

        public IActionResult Create()
        {
            ViewBag.AccountTypes = new[]
            {
                "Checking",
                "Savings",
                "Investment",
                "Credit Card",
                "Loan"
            };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Account account)
        {
            try
            {
                account.CreatedDate = DateTime.Now;
                account.IsActive = true;
                await _accountService.AddAccountAsync(account);
                TempData["Success"] = "Account created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating account: {ex.Message}";
                return View(account);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();

            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(id);
            var viewModel = new AccountDetailsViewModel
            {
                Account = account,
                Transactions = transactions.OrderByDescending(t => t.TransactionDate).Take(10).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();

            ViewBag.AccountTypes = new[]
            {
                "Checking",
                "Savings",
                "Investment",
                "Credit Card",
                "Loan"
            };

            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Account account)
        {
            try
            {
                account.AccountId = id;
                await _accountService.UpdateAccountAsync(account);
                TempData["Success"] = "Account updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating account: {ex.Message}";
                return View(account);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();

            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _accountService.DeleteAccountAsync(id);
                TempData["Success"] = "Account deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting account: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(int fromAccountId, int toAccountId, decimal amount)
        {
            try
            {
                var fromAccount = await _accountService.GetAccountByIdAsync(fromAccountId);
                var toAccount = await _accountService.GetAccountByIdAsync(toAccountId);

                if (fromAccount == null || toAccount == null)
                    return BadRequest("Invalid account");

                if (fromAccount.CurrentBalance < amount)
                    return BadRequest("Insufficient balance");

                fromAccount.CurrentBalance -= amount;
                toAccount.CurrentBalance += amount;

                await _accountService.UpdateAccountAsync(fromAccount);
                await _accountService.UpdateAccountAsync(toAccount);

                TempData["Success"] = $"Transferred {amount:C} successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    public class AccountDetailsViewModel
    {
        public Account Account { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
