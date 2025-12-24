using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

public class HomeController : Controller
{
    // GET: Home
    /// <summary>
    /// Displays the main landing page of the Ellis Hope Foundation website
    /// </summary>
    [SwaggerOperation(Summary = "Displays the main landing page of the Ellis Hope Foundation website")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// alternate home/demo page.
    /// </summary>
    [SwaggerOperation(Summary = "alternate home/demo page.")]
    public IActionResult Index2()
    {
        return View();
    }

    /// <summary>
    /// alternate home/demo page.
    /// </summary>
    [SwaggerOperation(Summary = "alternate home/demo page.")]
    public IActionResult Index3()
    {
        return View();
    }

    /// <summary>
    /// alternate home/demo page.
    /// </summary>
    [SwaggerOperation(Summary = "alternate home/demo page.")]
    public IActionResult Index4()
    {
        return View();
    }

    /// <summary>
    /// alternate home/demo page.
    /// </summary>
    [SwaggerOperation(Summary = "alternate home/demo page.")]
    public IActionResult Index5()
    {
        return View();
    }

    /// <summary>
    /// alternate home/demo page.
    /// </summary>
    [SwaggerOperation(Summary = "alternate home/demo page.")]
    public IActionResult Index6()
    {
        return View();
    }

    /// <summary>
    /// alternate home/demo page.
    /// </summary>
    [SwaggerOperation(Summary = "alternate home/demo page.")]
    public IActionResult Index7()
    {
        return View();
    }

    /// <summary>
    /// alternate home/demo page.
    /// </summary>
    [SwaggerOperation(Summary = "alternate home/demo page.")]
    public IActionResult Index8()
    {
        return View();
    }
}
