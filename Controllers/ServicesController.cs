using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class ServicesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // GET: Services
    public IActionResult v2()
    {
        return View();
    }
}
