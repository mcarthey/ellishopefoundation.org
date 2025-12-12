using Microsoft.AspNetCore.Mvc;
using EllisHope.Services;

namespace EllisHope.Controllers;

public class EventController : Controller
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // GET: Event/List
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

    public IActionResult grid()
    {
        return View();
    }
}
