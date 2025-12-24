using EllisHope.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using EllisHope.Models.Domain;

namespace EllisHope.Controllers;

[ApiExplorerSettings(GroupName = "Causes")]
[SwaggerTag("Public causes and initiatives. Browse active charitable causes, donation campaigns, and community initiatives supported by Ellis Hope Foundation.")]
public class CausesController : Controller
{
    private readonly ICauseService _causeService;
    private readonly ILogger<CausesController> _logger;

    public CausesController(ICauseService causeService, ILogger<CausesController> logger)
    {
        _causeService = causeService;
        _logger = logger;
    }

    // GET: Causes/list
    /// <summary>
    /// Displays active causes and initiatives in list format with optional keyword search
    /// </summary>
    /// <param name="search">Optional keyword to search in cause titles and descriptions</param>
    /// <returns>View displaying active causes in list format</returns>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /Causes/List
    ///     GET /Causes/List?search=education
    ///     GET /Causes/List?search=healthcare
    ///
    /// Returns only active, published causes. Search performs full-text search across cause title,
    /// description, and goal information. Includes fundraising progress and donation details where applicable.
    /// </remarks>
    /// <response code="200">Successfully retrieved causes list</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves active causes with optional search filtering",
        Description = "Displays published active causes in list format. Supports keyword search across title and description. Shows fundraising goals and progress where available.",
        OperationId = "GetCausesList",
        Tags = new[] { "Causes" }
    )]
    [ProducesResponseType(typeof(IEnumerable<Cause>), StatusCodes.Status200OK)]
    public async Task<IActionResult> list(string? search)
    {
        IEnumerable<Models.Domain.Cause> causes;

        if (!string.IsNullOrWhiteSpace(search))
        {
            causes = await _causeService.SearchCausesAsync(search);
            ViewBag.SearchTerm = search;
        }
        else
        {
            causes = await _causeService.GetActiveCausesAsync();
        }

        return View(causes);
    }

    // GET: Causes/details/{slug}
    /// <summary>
    /// Displays detailed view of a single cause identified by URL slug
    /// </summary>
    /// <param name="slug">URL-friendly slug identifying the cause (e.g., "education-scholarship-fund")</param>
    /// <returns>View displaying complete cause details with similar causes and donation information</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Causes/Details/community-health-initiative
    ///
    /// Returns full cause details including:
    /// - Cause title, description, and mission statement
    /// - Fundraising goal and current progress (if applicable)
    /// - Impact metrics and beneficiary information
    /// - How to donate or get involved
    /// - Featured image and media gallery
    /// - Similar/related causes (up to 4)
    /// - Featured causes in sidebar
    ///
    /// Route also accessible via custom route: /causes/details/{slug}
    /// </remarks>
    /// <response code="200">Successfully retrieved cause details</response>
    /// <response code="404">Cause with specified slug not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves individual cause by URL slug",
        Description = "Returns complete cause information including description, fundraising goals/progress, impact metrics, and related causes. Includes similar causes and donation details.",
        OperationId = "GetCauseDetails",
        Tags = new[] { "Causes" }
    )]
    [ProducesResponseType(typeof(Cause), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> details(string? slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var cause = await _causeService.GetCauseBySlugAsync(slug);

        if (cause == null)
        {
            return NotFound();
        }

        // Get similar causes for sidebar
        var similarCauses = await _causeService.GetSimilarCausesAsync(cause.Id, 4);
        ViewBag.SimilarCauses = similarCauses;

        // Get recent/featured causes
        var featuredCauses = await _causeService.GetFeaturedCausesAsync(3);
        ViewBag.FeaturedCauses = featuredCauses;

        return View(cause);
    }

    // GET: Causes/grid
    /// <summary>
    /// Displays causes in grid/card layout format
    /// </summary>
    /// <returns>View displaying active causes in responsive grid layout</returns>
    /// <remarks>
    /// Alternative visual layout for cause listing. Displays causes as cards in a responsive grid
    /// format, optimized for visual browsing with featured images, fundraising progress bars,
    /// and key impact metrics.
    /// </remarks>
    /// <response code="200">Successfully displayed causes grid view</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays causes in grid/card layout",
        Description = "Alternative causes listing view using card-based grid layout. Optimized for visual content browsing with featured images, progress indicators, and donation CTAs.",
        OperationId = "GetCausesGrid",
        Tags = new[] { "Causes" }
    )]
    [ProducesResponseType(typeof(IEnumerable<Cause>), StatusCodes.Status200OK)]
    public async Task<IActionResult> grid()
    {
        var causes = await _causeService.GetActiveCausesAsync();
        return View(causes);
    }
}
