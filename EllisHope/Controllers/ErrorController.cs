using Microsoft.AspNetCore.Mvc;
using EllisHope.Models.ViewModels;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ErrorController(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    // GET: Error (generic)
    /// <summary>
    /// Displays a generic error page when an unhandled exception occurs
    /// </summary>
    [Route("Error")]
    [SwaggerOperation(Summary = "Display generic error page for unhandled exceptions.")]
    public IActionResult Index()
    {
        return View("Error", CreateErrorViewModel(500, "An error occurred"));
    }

    // GET: Error/404
    /// <summary>
    /// Displays a 404 Not Found error page with optional correlation ID for tracking
    /// </summary>
    /// <param name="correlationId">Optional tracking identifier for the error</param>
    [Route("Error/404")]
    [SwaggerOperation(Summary = "Display 404 Not Found error page.")]
    public IActionResult NotFound404(string? correlationId)
    {
        Response.StatusCode = 404;
        return View("NotFound", CreateErrorViewModel(404, "Page Not Found", correlationId));
    }

    // GET: Error/500
    /// <summary>
    /// Displays a 500 Server Error page with exception details in development mode
    /// </summary>
    /// <param name="correlationId">Optional tracking identifier for the error</param>
    [Route("Error/500")]
    [SwaggerOperation(Summary = "Display 500 Server Error page.")]
    public IActionResult ServerError(string? correlationId)
    {
        Response.StatusCode = 500;
        var viewModel = CreateErrorViewModel(500, "Server Error", correlationId);

        // In development, show more details
        if (_environment.IsDevelopment() && HttpContext.Items.ContainsKey("Exception"))
        {
            viewModel.Exception = HttpContext.Items["Exception"] as Exception;
            viewModel.ShowDetails = true;
        }

        return View("ServerError", viewModel);
    }

    // GET: Error/403
    /// <summary>
    /// Displays a 403 Forbidden error page when user lacks permissions
    /// </summary>
    /// <param name="correlationId">Optional tracking identifier for the error</param>
    [Route("Error/403")]
    [SwaggerOperation(Summary = "Display 403 Forbidden error page.")]
    public IActionResult Forbidden(string? correlationId)
    {
        Response.StatusCode = 403;
        return View("Forbidden", CreateErrorViewModel(403, "Access Denied", correlationId));
    }

    // GET: Error/{statusCode}
    /// <summary>
    /// Generic HTTP status code handler that routes to appropriate error view based on status code
    /// </summary>
    /// <param name="statusCode">The HTTP status code to handle (404, 403, 401, 400, etc.)</param>
    /// <param name="correlationId">Optional tracking identifier for the error</param>
    [Route("Error/{statusCode:int}")]
    [SwaggerOperation(Summary = "Generic HTTP status code handler.")]
    public IActionResult HttpStatusCodeHandler(int statusCode, string? correlationId)
    {
        Response.StatusCode = statusCode;

        return statusCode switch
        {
            404 => View("NotFound", CreateErrorViewModel(404, "Page Not Found", correlationId)),
            403 => View("Forbidden", CreateErrorViewModel(403, "Access Denied", correlationId)),
            401 => View("Unauthorized", CreateErrorViewModel(401, "Unauthorized", correlationId)),
            400 => View("BadRequest", CreateErrorViewModel(400, "Bad Request", correlationId)),
            _ => View("Error", CreateErrorViewModel(statusCode, "An error occurred", correlationId))
        };
    }

    private ErrorViewModel CreateErrorViewModel(int statusCode, string title, string? correlationId = null)
    {
        return new ErrorViewModel
        {
            StatusCode = statusCode,
            Title = title,
            CorrelationId = correlationId ?? HttpContext.TraceIdentifier,
            RequestPath = HttpContext.Request.Path,
            SupportEmail = _configuration["Foundation:Email"] ?? "admin@ellishopefoundation.org",
            ShowDetails = _environment.IsDevelopment()
        };
    }
}
