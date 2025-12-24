using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EllisHope.Services;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly IBlogService _blogService;
    private readonly IEventService _eventService;
    private readonly ICauseService _causeService;
    private readonly IMediaService _mediaService;
    private readonly IPageService _pageService;

    public DashboardController(
        ILogger<DashboardController> logger,
        IBlogService blogService,
        IEventService eventService,
        ICauseService causeService,
        IMediaService mediaService,
        IPageService pageService)
    {
        _logger = logger;
        _blogService = blogService;
        _eventService = eventService;
        _causeService = causeService;
        _mediaService = mediaService;
        _pageService = pageService;
    }

    /// <summary>
    /// Displays the admin dashboard with statistics for blog posts, events, causes, media, and pages
    /// </summary>
    /// <remarks>Requires Admin role authorization</remarks>
    [SwaggerOperation(Summary = "Displays the admin dashboard with statistics for blog posts, events, causes, media, and pages")]
    public async Task<IActionResult> Index()
    {
        ViewData["UserName"] = User.Identity?.Name ?? "Admin";
        
        // Get statistics
        var blogCount = (await _blogService.GetAllPostsAsync(includeUnpublished: true)).Count();
        var eventCount = (await _eventService.GetAllEventsAsync(includeUnpublished: true)).Count();
        var causeCount = (await _causeService.GetAllCausesAsync(includeUnpublished: true)).Count();
        var mediaCount = await _mediaService.GetTotalMediaCountAsync();
        var pageCount = (await _pageService.GetAllPagesAsync()).Count();

        ViewData["BlogCount"] = blogCount;
        ViewData["EventCount"] = eventCount;
        ViewData["CauseCount"] = causeCount;
        ViewData["MediaCount"] = mediaCount;
        ViewData["PageCount"] = pageCount;

        return View();
    }
}
