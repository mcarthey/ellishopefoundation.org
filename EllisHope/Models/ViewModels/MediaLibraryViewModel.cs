using EllisHope.Models.Domain;

namespace EllisHope.Models.ViewModels;

public class MediaLibraryViewModel
{
    public IEnumerable<Media> Media { get; set; } = new List<Media>();
    public MediaCategory? FilterCategory { get; set; }
    public MediaSource? FilterSource { get; set; }
    public string? SearchTerm { get; set; }

    // Statistics
    public int TotalMediaCount { get; set; }
    public long TotalStorageSize { get; set; }
    public Dictionary<MediaCategory, int> MediaCountByCategory { get; set; } = new();

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    public int TotalPages { get; set; }
}
