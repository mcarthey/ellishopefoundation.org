using Microsoft.AspNetCore.Mvc;

namespace Kuki.Controllers
{
    public class AboutController : Controller
    {
        // GET: About
        public IActionResult Index()
        {
            return View();
        }
    }
}