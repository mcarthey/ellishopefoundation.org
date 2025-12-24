using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class FaqController : Controller
{
    // GET: Faq
    /// <summary>
    /// Displays the Frequently Asked Questions page
    /// </summary>
    [SwaggerOperation(Summary = "Displays the Frequently Asked Questions page")]
    public IActionResult Index()
    {
        return View();
    }
}
