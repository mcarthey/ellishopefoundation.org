using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

public class EventListViewModel
{
    public IEnumerable<Event> Events { get; set; } = new List<Event>();
    public string? SearchTerm { get; set; }
    public bool ShowUnpublished { get; set; } = true;
    public bool ShowUpcomingOnly { get; set; } = false;
}
