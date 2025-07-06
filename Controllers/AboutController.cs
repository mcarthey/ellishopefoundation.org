using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class AboutController : Controller
{
    // GET: About
    public IActionResult Index()
    {
        return View();
    }
}
