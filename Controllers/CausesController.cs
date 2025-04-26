using Microsoft.AspNetCore.Mvc;

namespace Kuki.Controllers
{
    public class CausesController : Controller
    {
        // GET: Causes
        public IActionResult details()
        {
            return View();
        }
        public IActionResult grid()
        {
            return View();
        }
        public IActionResult list()
        {
            return View();
        }
    }
}