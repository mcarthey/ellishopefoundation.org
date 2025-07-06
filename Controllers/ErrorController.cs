using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class ErrorController : Controller
{
    // GET: Error
    public IActionResult Index()
    {
        return View();
    }
}
