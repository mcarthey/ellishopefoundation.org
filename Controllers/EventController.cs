using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class EventController : Controller
{
    // GET: Event
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
