using Microsoft.AspNetCore.Mvc;

namespace Kuki.Controllers
{
    public class TeamController : Controller
    {
        // GET: Team
        public IActionResult details()
        {
            return View();
        }
        public IActionResult v1()
        {
            return View();
        }
        public IActionResult v2()
        {
            return View();
        }
    }
}