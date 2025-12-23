using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EllisHope.Services;

/// <summary>
/// Service for logging to the database
/// </summary>
public class DatabaseLoggerService : IDatabaseLoggerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseLoggerService> _logger;

    public DatabaseLoggerService(
        ApplicationDbContext context,
        ILogger<DatabaseLoggerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogExceptionAsync(
        Exception exception,
        string? requestPath = null,
        string? httpMethod = null,
        string? queryString = null,
        string? userId = null,
        string? userName = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        string? additionalData = null)
    {
        try
        {
            var log = new ApplicationLog
            {
                Level = AppLogLevel.Error,
                Category = exception.Source ?? "Unknown",
                Message = exception.Message,
                ExceptionDetails = exception.ToString(),
                StackTrace = exception.StackTrace,
                ExceptionType = exception.GetType().FullName,
                InnerException = exception.InnerException?.ToString(),
                RequestPath = requestPath,
                HttpMethod = httpMethod,
                QueryString = queryString,
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = correlationId ?? Guid.NewGuid().ToString("N"),
                AdditionalData = additionalData,
                MachineName = Environment.MachineName,
                CreatedAt = DateTime.UtcNow
            };

            // Determine severity
            if (exception is OutOfMemoryException or StackOverflowException)
            {
                log.Level = AppLogLevel.Critical;
            }

            _context.ApplicationLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Fallback to standard logging if database logging fails
            _logger.LogError(ex, "Failed to log exception to database. Original exception: {OriginalException}", exception.Message);
        }
    }

    public async Task LogAsync(
        AppLogLevel level,
        string category,
        string message,
        string? additionalData = null,
        string? correlationId = null)
    {
        try
        {
            var log = new ApplicationLog
            {
                Level = level,
                Category = category,
                Message = message,
                AdditionalData = additionalData,
                CorrelationId = correlationId,
                MachineName = Environment.MachineName,
                CreatedAt = DateTime.UtcNow
            };

            _context.ApplicationLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log message to database: {Message}", message);
        }
    }

    public async Task<(IEnumerable<ApplicationLog> Logs, int TotalCount)> GetLogsAsync(
        AppLogLevel? minLevel = null,
        AppLogLevel? maxLevel = null,
        string? category = null,
        string? searchTerm = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool? isReviewed = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.ApplicationLogs.AsQueryable();

        if (minLevel.HasValue)
        {
            query = query.Where(l => l.Level >= minLevel.Value);
        }

        if (maxLevel.HasValue)
        {
            query = query.Where(l => l.Level <= maxLevel.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(l => l.Category.Contains(category));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(l =>
                l.Message.Contains(searchTerm) ||
                (l.ExceptionDetails != null && l.ExceptionDetails.Contains(searchTerm)) ||
                (l.RequestPath != null && l.RequestPath.Contains(searchTerm)));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= toDate.Value);
        }

        if (isReviewed.HasValue)
        {
            query = query.Where(l => l.IsReviewed == isReviewed.Value);
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }

    public async Task<ApplicationLog?> GetLogByIdAsync(int id)
    {
        return await _context.ApplicationLogs.FindAsync(id);
    }

    public async Task MarkAsReviewedAsync(int logId, string reviewedById, string? reviewNotes = null)
    {
        var log = await _context.ApplicationLogs.FindAsync(logId);
        if (log != null)
        {
            log.IsReviewed = true;
            log.ReviewedAt = DateTime.UtcNow;
            log.ReviewedById = reviewedById;
            log.ReviewNotes = reviewNotes;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<LogStatistics> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.ApplicationLogs.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= toDate.Value);
        }

        var stats = new LogStatistics
        {
            TotalLogs = await query.CountAsync(),
            ErrorCount = await query.CountAsync(l => l.Level == AppLogLevel.Error),
            WarningCount = await query.CountAsync(l => l.Level == AppLogLevel.Warning),
            CriticalCount = await query.CountAsync(l => l.Level == AppLogLevel.Critical),
            UnreviewedCount = await query.CountAsync(l => !l.IsReviewed && l.Level >= AppLogLevel.Warning),
            LastErrorTime = await query
                .Where(l => l.Level >= AppLogLevel.Error)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => (DateTime?)l.CreatedAt)
                .FirstOrDefaultAsync()
        };

        // Get logs by category (top 10)
        var categoryGroups = await query
            .GroupBy(l => l.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        stats.LogsByCategory = categoryGroups.ToDictionary(x => x.Category, x => x.Count);

        // Get logs by level
        var levelGroups = await query
            .GroupBy(l => l.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.LogsByLevel = levelGroups.ToDictionary(x => x.Level, x => x.Count);

        return stats;
    }

    public async Task<int> PurgeOldLogsAsync(int daysToKeep = 90, AppLogLevel? minLevelToKeep = null)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var query = _context.ApplicationLogs
            .Where(l => l.CreatedAt < cutoffDate);

        // Optionally keep logs above a certain level
        if (minLevelToKeep.HasValue)
        {
            query = query.Where(l => l.Level < minLevelToKeep.Value);
        }

        var logsToDelete = await query.ToListAsync();
        var count = logsToDelete.Count;

        _context.ApplicationLogs.RemoveRange(logsToDelete);
        await _context.SaveChangesAsync();

        return count;
    }

    public async Task<int> GetRecentErrorCountAsync(int hours = 24)
    {
        var cutoff = DateTime.UtcNow.AddHours(-hours);
        return await _context.ApplicationLogs
            .CountAsync(l => l.Level >= AppLogLevel.Error && l.CreatedAt >= cutoff);
    }
}
