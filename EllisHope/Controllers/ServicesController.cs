using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

public class ServicesController : Controller
{
    /// <summary>
    /// Displays the services offered by Ellis Hope Foundation
    /// </summary>
    [SwaggerOperation(Summary = "Displays the services offered by Ellis Hope Foundation")]
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
