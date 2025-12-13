using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace EllisHope.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;
    private readonly SlugHelper _slugHelper;

    public EventService(ApplicationDbContext context)
    {
        _context = context;
        _slugHelper = new SlugHelper();
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(bool includeUnpublished = false)
    {
        var query = _context.Events.OrderByDescending(e => e.StartDate);

        if (!includeUnpublished)
        {
            return await query.Where(e => e.IsPublished).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int count = 10)
    {
        var now = DateTime.UtcNow;
        return await _context.Events
            .Where(e => e.IsPublished && e.StartDate >= now)
            .OrderBy(e => e.StartDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetPastEventsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Events
            .Where(e => e.IsPublished && e.StartDate < now)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await _context.Events.FindAsync(id);
    }

    public async Task<Event?> GetEventBySlugAsync(string slug)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Slug == slug && e.IsPublished);
    }

    public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllEventsAsync();
        }

        return await _context.Events
            .Where(e => e.IsPublished &&
                   (e.Title.Contains(searchTerm) ||
                    e.Description.Contains(searchTerm) ||
                    e.Location.Contains(searchTerm)))
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetSimilarEventsAsync(int eventId, int count = 4)
    {
        var currentEvent = await GetEventByIdAsync(eventId);
        if (currentEvent == null)
            return Enumerable.Empty<Event>();

        var now = DateTime.UtcNow;

        // Get events with similar tags or upcoming events
        var similarEvents = await _context.Events
            .Where(e => e.IsPublished && e.Id != eventId && e.StartDate >= now)
            .OrderBy(e => e.StartDate)
            .Take(count)
            .ToListAsync();

        return similarEvents;
    }

    public async Task<Event> CreateEventAsync(Event eventItem)
    {
        if (string.IsNullOrWhiteSpace(eventItem.Slug))
        {
            eventItem.Slug = GenerateSlug(eventItem.Title);
        }

        eventItem.Slug = await EnsureUniqueSlugAsync(eventItem.Slug);
        eventItem.CreatedDate = DateTime.UtcNow;
        eventItem.ModifiedDate = DateTime.UtcNow;

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();
        return eventItem;
    }

    public async Task<Event> UpdateEventAsync(Event eventItem)
    {
        eventItem.ModifiedDate = DateTime.UtcNow;
        _context.Events.Update(eventItem);
        await _context.SaveChangesAsync();
        return eventItem;
    }

    public async Task DeleteEventAsync(int id)
    {
        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem != null)
        {
            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeEventId = null)
    {
        var query = _context.Events.Where(e => e.Slug == slug);

        if (excludeEventId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEventId.Value);
        }

        return await query.AnyAsync();
    }

    public string GenerateSlug(string title)
    {
        return _slugHelper.GenerateSlug(title);
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug)
    {
        var slug = baseSlug;
        var counter = 1;

        while (await SlugExistsAsync(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
