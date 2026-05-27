using Microsoft.AspNetCore.Mvc;

namespace FinancialSystemApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
