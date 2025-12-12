using EllisHope.Models.Domain;

namespace EllisHope.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync(bool includeUnpublished = false);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int count = 10);
    Task<IEnumerable<Event>> GetPastEventsAsync();
    Task<Event?> GetEventByIdAsync(int id);
    Task<Event?> GetEventBySlugAsync(string slug);
    Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
    Task<IEnumerable<Event>> GetSimilarEventsAsync(int eventId, int count = 4);
    Task<Event> CreateEventAsync(Event eventItem);
    Task<Event> UpdateEventAsync(Event eventItem);
    Task DeleteEventAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeEventId = null);
    string GenerateSlug(string title);
}
