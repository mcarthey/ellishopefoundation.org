using Microsoft.AspNetCore.Mvc;

namespace Kuki.Controllers
{
    public class FaqController : Controller
    {
        // GET: Faq
        public IActionResult Index()
        {
            return View();
        }
    }
}