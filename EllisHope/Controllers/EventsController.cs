using Microsoft.AspNetCore.Mvc;
using EllisHope.Services;
using Swashbuckle.AspNetCore.Annotations;
using EllisHope.Models.Domain;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class EventsController : Controller
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // GET: Events
    /// <summary>
    /// Displays upcoming events in list format with optional keyword search
    /// </summary>
    /// <param name="search">Optional keyword to search in event titles and descriptions</param>
    /// <returns>View displaying upcoming events in list format</returns>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /Events
    ///     GET /Events?search=fundraiser
    ///     GET /Events?search=community
    ///
    /// Returns upcoming published events sorted by event date. Search performs full-text search
    /// across event title, description, and location fields. Limited to 100 upcoming events.
    /// </remarks>
    /// <response code="200">Successfully retrieved event list</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves upcoming events with optional search filtering",
        Description = "Displays published upcoming events. Supports keyword search across title, description, and location. Returns maximum 100 events sorted by event date.",
        OperationId = "GetEvents",
        Tags = new[] { "Events" }
    )]
    [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(string? search)
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
    /// Displays detailed view of a single event identified by URL slug
    /// </summary>
    /// <param name="slug">URL-friendly slug identifying the event (e.g., "annual-gala-2024")</param>
    /// <returns>View displaying complete event details with similar/upcoming events</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Event/Details/community-fundraiser-march
    ///
    /// Returns full event details including:
    /// - Event title, description, and full details
    /// - Event date, time, and location information
    /// - Registration/RSVP details if applicable
    /// - Featured image and event media
    /// - Similar/related events (up to 4)
    /// - Recent upcoming events in sidebar
    ///
    /// Route also accessible via custom route: /event/details/{slug}
    /// </remarks>
    /// <response code="200">Successfully retrieved event details</response>
    /// <response code="404">Event with specified slug not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves individual event by URL slug",
        Description = "Returns complete event information including date/time, location, description, and related events. Includes similar events and sidebar navigation.",
        OperationId = "GetEventDetails",
        Tags = new[] { "Events" }
    )]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Details(string slug)
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
    /// Displays events in grid/card layout format
    /// </summary>
    /// <returns>View displaying events in responsive grid layout</returns>
    /// <remarks>
    /// Alternative visual layout for event listing. Displays events as cards in a responsive grid
    /// format, optimized for visual browsing with event images and key details.
    /// </remarks>
    /// <response code="200">Successfully displayed events grid view</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays events in grid/card layout",
        Description = "Alternative event listing view using card-based grid layout. Optimized for visual content browsing with featured images and event highlights.",
        OperationId = "GetEventsGrid",
        Tags = new[] { "Events" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Grid()
    {
        return View();
    }
}
