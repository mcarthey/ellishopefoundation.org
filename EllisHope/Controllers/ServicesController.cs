using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class ServicesController : Controller
{
    /// <summary>
    /// TODO: Describe GET /Services
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Services")]
    public IActionResult Index()
    {
        return View();
    }

    // GET: Services
    /// <summary>
    /// alternate services view
    /// </summary>
    [SwaggerOperation(Summary = "alternate services view")]
    public IActionResult v2()
    {
        return View();
    }
}
