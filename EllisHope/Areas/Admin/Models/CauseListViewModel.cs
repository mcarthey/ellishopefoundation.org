using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

public class CauseListViewModel
{
    public IEnumerable<Cause> Causes { get; set; } = new List<Cause>();
    public string? SearchTerm { get; set; }
    public bool ShowUnpublished { get; set; }
    public bool ShowActiveOnly { get; set; }
    public string? CategoryFilter { get; set; }
}
