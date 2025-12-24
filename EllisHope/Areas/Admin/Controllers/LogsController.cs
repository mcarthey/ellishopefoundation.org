using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(GroupName = "Logs")]
[SwaggerTag("Application log management and monitoring. Admin-only access to view, search, filter, and analyze application logs for debugging, auditing, and security monitoring.")]
public class LogsController : Controller
{
    private readonly IDatabaseLoggerService _loggerService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LogsController> _logger;

    public LogsController(
        IDatabaseLoggerService loggerService,
        UserManager<ApplicationUser> userManager,
        ILogger<LogsController> logger)
    {
        _loggerService = loggerService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Admin/Logs
    /// <summary>
    /// Displays paginated application logs with comprehensive filtering, search, and statistics
    /// </summary>
    /// <param name="minLevel">Minimum log level to display (Trace, Debug, Information, Warning, Error, Critical)</param>
    /// <param name="maxLevel">Maximum log level to display</param>
    /// <param name="category">Filter by logger category (e.g., "EllisHope.Controllers.AccountController")</param>
    /// <param name="searchTerm">Search term to filter log messages, exception details, and correlation IDs</param>
    /// <param name="fromDate">Start date for log entries (default: 7 days ago)</param>
    /// <param name="toDate">End date for log entries (default: today)</param>
    /// <param name="isReviewed">Filter by review status (true: reviewed, false: unreviewed, null: all)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of log entries per page (default: 50, max recommended: 100)</param>
    /// <returns>View displaying filtered log entries with pagination and statistics dashboard</returns>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /Admin/Logs
    ///     GET /Admin/Logs?minLevel=Warning&amp;isReviewed=false
    ///     GET /Admin/Logs?searchTerm=ApplicationId&amp;fromDate=2024-01-01&amp;page=2
    ///     GET /Admin/Logs?category=EllisHope.Controllers.AccountController&amp;maxLevel=Error
    ///
    /// Returns comprehensive log management view featuring:
    /// - Paginated log entries with level, timestamp, category, message, exception details
    /// - Statistics dashboard showing log counts by level for date range
    /// - Advanced filtering options (level range, category, date range, review status)
    /// - Full-text search across message, exception, and correlation ID fields
    /// - Review status tracking (mark logs as reviewed for audit trail)
    /// - Links to view detailed log entries
    ///
    /// **Log Levels:**
    /// - Trace (0): Most verbose, detailed diagnostic information
    /// - Debug (1): Debugging information for development
    /// - Information (2): General informational messages
    /// - Warning (3): Warning messages for unusual but handled situations
    /// - Error (4): Error messages for failures
    /// - Critical (5): Critical failures requiring immediate attention
    ///
    /// **Common Use Cases:**
    /// - Monitor application errors: `minLevel=Error`
    /// - Review unreviewed warnings: `minLevel=Warning&isReviewed=false`
    /// - Debug specific request: `searchTerm=correlation-id-abc123`
    /// - Audit specific controller: `category=EllisHope.Controllers.ApplicationsController`
    ///
    /// **Authorization:**
    /// - Requires Admin role
    /// - Only admins can view application logs for security/privacy
    /// </remarks>
    /// <response code="200">Successfully retrieved and displayed log entries with statistics</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User lacks Admin role</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays paginated application logs with filtering, search, and statistics",
        Description = "Returns log management view with advanced filtering (level, category, date, review status), full-text search, pagination, and statistics dashboard. Admin-only access.",
        OperationId = "GetLogs",
        Tags = new[] { "Logs" }
    )]
    [ProducesResponseType(typeof(LogsIndexViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Index(
        AppLogLevel? minLevel,
        AppLogLevel? maxLevel,
        string? category,
        string? searchTerm,
        DateTime? fromDate,
        DateTime? toDate,
        bool? isReviewed,
        int page = 1,
        int pageSize = 50)
    {
        var (logs, totalCount) = await _loggerService.GetLogsAsync(
            minLevel, maxLevel, category, searchTerm, fromDate, toDate, isReviewed, page, pageSize);

        var statistics = await _loggerService.GetLogStatisticsAsync(
            fromDate ?? DateTime.UtcNow.AddDays(-7),
            toDate ?? DateTime.UtcNow);

        var viewModel = new LogsIndexViewModel
        {
            Logs = logs,
            Statistics = statistics,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            MinLevel = minLevel,
            MaxLevel = maxLevel,
            Category = category,
            SearchTerm = searchTerm,
            FromDate = fromDate,
            ToDate = toDate,
            IsReviewed = isReviewed
        };

        return View(viewModel);
    }

    // GET: Admin/Logs/Details/5
    /// <summary>
    /// Displays comprehensive details for a specific log entry including exception stack trace and review history
    /// </summary>
    /// <param name="id">Log entry ID</param>
    /// <returns>View displaying full log entry details with exception information and review metadata</returns>
    /// <remarks>
    /// Returns detailed view of single log entry including:
    /// - Full log message
    /// - Log level and category
    /// - Timestamp and correlation ID
    /// - Complete exception details (type, message, stack trace, inner exceptions)
    /// - Review status and reviewer information
    /// - Review notes if marked as reviewed
    ///
    /// Useful for investigating specific errors, debugging issues, and auditing log reviews.
    /// </remarks>
    /// <response code="200">Successfully retrieved log entry details</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User lacks Admin role</response>
    /// <response code="404">Log entry not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays comprehensive details for a specific log entry",
        Description = "Returns full log entry with message, exception stack trace, correlation ID, and review history. Admin-only access.",
        OperationId = "GetLogDetails",
        Tags = new[] { "Logs" }
    )]
    [ProducesResponseType(typeof(AppLog), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Details(int id)
    {
        var log = await _loggerService.GetLogByIdAsync(id);
        if (log == null)
        {
            return NotFound();
        }

        return View(log);
    }

    // POST: Admin/Logs/MarkAsReviewed/5
    /// <summary>
    /// Marks a log entry as reviewed by the current admin with optional review notes for audit trail
    /// </summary>
    /// <param name="id">Log entry ID to mark as reviewed</param>
    /// <param name="reviewNotes">Optional notes documenting the review (e.g., "Investigated - fixed in PR #123")</param>
    /// <returns>Redirects to log details page with success message</returns>
    /// <remarks>
    /// Records admin review of log entry for audit and tracking purposes. Updates:
    /// - IsReviewed flag to true
    /// - ReviewedById to current admin's ID
    /// - ReviewedAt timestamp to current UTC time
    /// - ReviewNotes with provided notes (if any)
    ///
    /// Useful for tracking which errors/warnings have been investigated and resolved.
    /// Helps prevent duplicate investigation of the same issues.
    ///
    /// **Authorization:** Requires Admin role and anti-forgery token.
    /// </remarks>
    /// <response code="302">Redirects to log details after marking as reviewed</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User lacks Admin role</response>
    /// <response code="404">Log entry not found</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Marks log entry as reviewed by current admin with optional notes",
        Description = "Records admin review of log entry for audit trail. Updates review status, reviewer ID, timestamp, and optional notes.",
        OperationId = "PostMarkLogAsReviewed",
        Tags = new[] { "Logs" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsReviewed(int id, string? reviewNotes)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        await _loggerService.MarkAsReviewedAsync(id, currentUser.Id, reviewNotes);
        TempData["SuccessMessage"] = "Log marked as reviewed.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Admin/Logs/Purge
    /// <summary>
    /// Purges old log entries to maintain database performance while preserving critical logs
    /// </summary>
    /// <param name="daysToKeep">Number of days of logs to retain (default: 90, minimum: 7)</param>
    /// <param name="minLevelToKeep">Minimum log level to keep regardless of age (e.g., Warning, Error, Critical)</param>
    /// <returns>Redirects to logs index with count of purged entries</returns>
    /// <remarks>
    /// Deletes old log entries to prevent unbounded database growth while preserving important logs.
    ///
    /// **Purge Strategy:**
    /// - Deletes logs older than specified retention period (daysToKeep)
    /// - Preserves logs at or above minLevelToKeep regardless of age (if specified)
    /// - Example: `daysToKeep=90, minLevelToKeep=Warning` deletes:
    ///   - All Trace/Debug/Information logs older than 90 days
    ///   - Keeps ALL Warning/Error/Critical logs forever
    ///
    /// **Common Retention Policies:**
    /// - Development: 30 days, keep Warning+
    /// - Staging: 60 days, keep Error+
    /// - Production: 90-180 days, keep Error+
    ///
    /// **Best Practices:**
    /// - Run periodically (e.g., monthly) to maintain database performance
    /// - Always preserve Error and Critical logs for long-term audit
    /// - Export logs to external storage before purging if needed for compliance
    /// - Review log statistics before purging to ensure appropriate retention
    ///
    /// **Authorization:** Requires Admin role and anti-forgery token.
    /// </remarks>
    /// <response code="302">Redirects to logs index with purge count message</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User lacks Admin role</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Purges old log entries with configurable retention and level preservation",
        Description = "Deletes logs older than retention period while preserving critical logs by level. Maintains database performance. Requires admin authorization.",
        OperationId = "PostPurgeLogs",
        Tags = new[] { "Logs" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Purge(int daysToKeep = 90, AppLogLevel? minLevelToKeep = null)
    {
        var deletedCount = await _loggerService.PurgeOldLogsAsync(daysToKeep, minLevelToKeep);
        TempData["SuccessMessage"] = $"Purged {deletedCount} old log entries.";

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Logs/Search
    /// <summary>
    /// Searches for all log entries sharing a correlation ID to trace a complete request workflow
    /// </summary>
    /// <param name="correlationId">Correlation ID to search for (typically a GUID linking related log entries)</param>
    /// <returns>View displaying all log entries matching the correlation ID in chronological order</returns>
    /// <remarks>
    /// Traces a complete request workflow by finding all log entries with the same correlation ID.
    ///
    /// **Use Cases:**
    /// - Debug a specific failed request by finding all related log entries
    /// - Trace complete workflow from request start to completion/error
    /// - Investigate multi-step process (e.g., application submission → review → decision)
    /// - Correlate logs across multiple controllers/services for single user action
    ///
    /// **Correlation IDs:**
    /// - Automatically generated for each HTTP request
    /// - Propagated through entire request pipeline
    /// - Included in all log entries for that request
    /// - Format: GUID (e.g., "a1b2c3d4-e5f6-7890-abcd-ef1234567890")
    ///
    /// Returns up to 100 log entries sorted chronologically to show request flow.
    ///
    /// **Authorization:** Requires Admin role.
    /// </remarks>
    /// <response code="200">Successfully retrieved log entries for correlation ID</response>
    /// <response code="302">Redirects to Index if correlation ID not provided</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User lacks Admin role</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Searches for all log entries by correlation ID to trace request workflow",
        Description = "Returns all log entries sharing a correlation ID to debug and trace complete request flow from start to finish. Admin-only access.",
        OperationId = "SearchLogsByCorrelationId",
        Tags = new[] { "Logs" }
    )]
    [ProducesResponseType(typeof(LogsIndexViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Search(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return RedirectToAction(nameof(Index));
        }

        var (logs, _) = await _loggerService.GetLogsAsync(
            searchTerm: correlationId,
            page: 1,
            pageSize: 100);

        var viewModel = new LogsIndexViewModel
        {
            Logs = logs,
            SearchTerm = correlationId,
            CurrentPage = 1,
            PageSize = 100,
            TotalCount = logs.Count()
        };

        return View("Index", viewModel);
    }

    // GET: Admin/Logs/Statistics
    /// <summary>
    /// Displays comprehensive log statistics, analytics, and trends for monitoring application health
    /// </summary>
    /// <param name="fromDate">Start date for statistics analysis (default: 30 days ago)</param>
    /// <param name="toDate">End date for statistics analysis (default: today)</param>
    /// <returns>View displaying log statistics dashboard with charts and metrics</returns>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /Admin/Logs/Statistics
    ///     GET /Admin/Logs/Statistics?fromDate=2024-01-01&amp;toDate=2024-01-31
    ///
    /// Returns comprehensive log analytics dashboard featuring:
    /// - Log volume by level (Trace, Debug, Information, Warning, Error, Critical)
    /// - Log count trends over time (daily/hourly breakdown)
    /// - Top categories generating logs
    /// - Error rate trends
    /// - Warning/Error ratio analysis
    /// - Most common exception types
    ///
    /// **Use Cases:**
    /// - Monitor application health trends
    /// - Identify increasing error rates
    /// - Spot unusual log patterns (spikes, anomalies)
    /// - Capacity planning based on log volume
    /// - Identify frequently failing components
    /// - Compliance and audit reporting
    ///
    /// **Metrics Included:**
    /// - Total log count by level
    /// - Average logs per day/hour
    /// - Error percentage vs. total logs
    /// - Top 10 categories by volume
    /// - Time series data for trend visualization
    ///
    /// Defaults to last 30 days if no date range specified.
    ///
    /// **Authorization:** Requires Admin role.
    /// </remarks>
    /// <response code="200">Successfully retrieved log statistics and analytics</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User lacks Admin role</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays comprehensive log statistics and health monitoring dashboard",
        Description = "Returns analytics dashboard with log volume trends, error rates, top categories, and health metrics for specified date range. Admin-only access.",
        OperationId = "GetLogStatistics",
        Tags = new[] { "Logs" }
    )]
    [ProducesResponseType(typeof(LogStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Statistics(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var statistics = await _loggerService.GetLogStatisticsAsync(from, to);

        return View(statistics);
    }
}
