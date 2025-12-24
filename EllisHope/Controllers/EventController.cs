using Microsoft.AspNetCore.Mvc;
using EllisHope.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

public class EventController : Controller
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // GET: Event/List
    /// <summary>
    /// list layout
    /// </summary>
    [SwaggerOperation(Summary = "list layout")]
    public async Task<IActionResult> list(string? search)
    {
        IEnumerable<Models.Domain.Event> events;

        if (!string.IsNullOrWhiteSpace(search))
        {
            events = await _eventService.SearchEventsAsync(search);
        }
        else
        {
            events = await _eventService.GetUpcomingEventsAsync(100);
        }

        ViewBag.SearchTerm = search;
        return View(events);
    }

    // GET: Event/Details/slug
    /// <summary>
    /// details by slug
    /// </summary>
    [SwaggerOperation(Summary = "details by slug")]
    public async Task<IActionResult> details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var eventItem = await _eventService.GetEventBySlugAsync(slug);
        if (eventItem == null)
        {
            return NotFound();
        }

        // Get similar/upcoming events
        var similarEvents = await _eventService.GetSimilarEventsAsync(eventItem.Id, 4);
        ViewBag.SimilarEvents = similarEvents;

        // Get recent events for sidebar
        var recentEvents = await _eventService.GetUpcomingEventsAsync(3);
        ViewBag.RecentEvents = recentEvents;

        return View(eventItem);
    }

    /// <summary>
    /// grid layout
    /// </summary>
    [SwaggerOperation(Summary = "grid layout")]
    public IActionResult grid()
    {
        return View();
    }
}
