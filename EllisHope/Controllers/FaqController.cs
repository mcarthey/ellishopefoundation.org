using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class FaqController : Controller
{
    // GET: Faq
    /// <summary>
    /// TODO: Describe GET /Faq
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Faq")]
    public IActionResult Index()
    {
        return View();
    }
}
