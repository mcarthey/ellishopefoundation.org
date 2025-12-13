using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class ContactController : Controller
{
    // GET: Contact
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult v2()
    {
        return View();
    }
}
