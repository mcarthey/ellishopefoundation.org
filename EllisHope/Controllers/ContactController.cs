using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

public class ContactController : Controller
{
    // GET: Contact
    /// <summary>
    /// send message (anti-forgery)
    /// </summary>
    [SwaggerOperation(Summary = "send message (anti-forgery)")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// alternate contact API/version
    /// </summary>
    [SwaggerOperation(Summary = "alternate contact API/version")]
    public IActionResult v2()
    {
        return View();
    }
}
