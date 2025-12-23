using EllisHope.Models.Domain;
using EllisHope.Services;

namespace EllisHope.Areas.Admin.Models;

/// <summary>
/// View model for the logs index page
/// </summary>
public class LogsIndexViewModel
{
    public IEnumerable<ApplicationLog> Logs { get; set; } = new List<ApplicationLog>();
    public LogStatistics? Statistics { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    // Filters
    public AppLogLevel? MinLevel { get; set; }
    public AppLogLevel? MaxLevel { get; set; }
    public string? Category { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsReviewed { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

/// <summary>
/// View model for log statistics
/// </summary>
public class LogStatisticsViewModel
{
    public LogStatistics Statistics { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
