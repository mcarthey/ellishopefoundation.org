using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class BlogController : Controller
{
    // GET: Blog
    public IActionResult classic()
    {
        return View();
    }

    public IActionResult details()
    {
        return View();
    }

    public IActionResult grid()
    {
        return View();
    }
}
