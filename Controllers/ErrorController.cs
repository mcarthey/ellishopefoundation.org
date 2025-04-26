using Microsoft.AspNetCore.Mvc;

namespace Kuki.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public IActionResult Index()
        {
            return View();
        }
    }
}