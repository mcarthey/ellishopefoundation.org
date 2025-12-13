using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class FaqController : Controller
{
    // GET: Faq
    public IActionResult Index()
    {
        return View();
    }
}
