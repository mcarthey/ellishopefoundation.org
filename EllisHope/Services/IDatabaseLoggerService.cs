using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service interface for database logging operations
/// </summary>
public interface IDatabaseLoggerService
{
    /// <summary>
    /// Log an exception with full context
    /// </summary>
    Task LogExceptionAsync(
        Exception exception,
        string? requestPath = null,
        string? httpMethod = null,
        string? queryString = null,
        string? userId = null,
        string? userName = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        string? additionalData = null);

    /// <summary>
    /// Log a message with the specified level
    /// </summary>
    Task LogAsync(
        AppLogLevel level,
        string category,
        string message,
        string? additionalData = null,
        string? correlationId = null);

    /// <summary>
    /// Get logs with filtering and pagination
    /// </summary>
    Task<(IEnumerable<ApplicationLog> Logs, int TotalCount)> GetLogsAsync(
        AppLogLevel? minLevel = null,
        AppLogLevel? maxLevel = null,
        string? category = null,
        string? searchTerm = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool? isReviewed = null,
        int page = 1,
        int pageSize = 50);

    /// <summary>
    /// Get a single log entry by ID
    /// </summary>
    Task<ApplicationLog?> GetLogByIdAsync(int id);

    /// <summary>
    /// Mark a log as reviewed
    /// </summary>
    Task MarkAsReviewedAsync(int logId, string reviewedById, string? reviewNotes = null);

    /// <summary>
    /// Get log statistics
    /// </summary>
    Task<LogStatistics> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Delete old logs based on retention policy
    /// </summary>
    Task<int> PurgeOldLogsAsync(int daysToKeep = 90, AppLogLevel? minLevelToKeep = null);

    /// <summary>
    /// Get recent error count for dashboard
    /// </summary>
    Task<int> GetRecentErrorCountAsync(int hours = 24);
}

/// <summary>
/// Log statistics for dashboard display
/// </summary>
public class LogStatistics
{
    public int TotalLogs { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int CriticalCount { get; set; }
    public int UnreviewedCount { get; set; }
    public Dictionary<string, int> LogsByCategory { get; set; } = new();
    public Dictionary<AppLogLevel, int> LogsByLevel { get; set; } = new();
    public DateTime? LastErrorTime { get; set; }
}
