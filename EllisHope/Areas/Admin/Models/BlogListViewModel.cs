using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

public class BlogListViewModel
{
    public IEnumerable<BlogPost> Posts { get; set; } = new List<BlogPost>();
    public string? SearchTerm { get; set; }
    public int? CategoryFilter { get; set; }
    public bool ShowUnpublished { get; set; } = true;
}
