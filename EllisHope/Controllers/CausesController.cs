using EllisHope.Services;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

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
    public async Task<IActionResult> grid()
    {
        var causes = await _causeService.GetActiveCausesAsync();
        return View(causes);
    }
}
