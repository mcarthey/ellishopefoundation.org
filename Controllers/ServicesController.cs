using Microsoft.AspNetCore.Mvc;

namespace Kuki.Controllers
{
    public class ServicesController : Controller
    {
        // GET: Services
        public IActionResult v2()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}