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
    /// Displays application logs with filtering options for level, category, date range, and review status
    /// </summary>
    /// <remarks>Requires Admin role authorization. Supports pagination and search.</remarks>
    [SwaggerOperation(Summary = "Displays application logs with filtering options for level, category, date range, and review status")]
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
    /// Displays detailed information for a specific log entry
    /// </summary>
    /// <param name="id">Log entry ID</param>
    [SwaggerOperation(Summary = "Displays detailed information for a specific log entry")]
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Marks a log entry as reviewed by the current admin with optional notes. Anti-forgery required.
    /// </summary>
    /// <param name="id">Log entry ID</param>
    /// <param name="reviewNotes">Optional notes about the review</param>
    [SwaggerOperation(Summary = "Marks a log entry as reviewed by the current admin with optional notes. Anti-forgery required.")]
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Purges old log entries while keeping logs within specified retention period and level. Anti-forgery required.
    /// </summary>
    /// <param name="daysToKeep">Number of days of logs to retain (default: 90)</param>
    /// <param name="minLevelToKeep">Minimum log level to keep regardless of age</param>
    [SwaggerOperation(Summary = "Purges old log entries while keeping logs within specified retention period and level. Anti-forgery required.")]
    public async Task<IActionResult> Purge(int daysToKeep = 90, AppLogLevel? minLevelToKeep = null)
    {
        var deletedCount = await _loggerService.PurgeOldLogsAsync(daysToKeep, minLevelToKeep);
        TempData["SuccessMessage"] = $"Purged {deletedCount} old log entries.";

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Logs/Search
    /// <summary>
    /// Searches for log entries by correlation ID to track related log events
    /// </summary>
    /// <param name="correlationId">Correlation ID to search for</param>
    [SwaggerOperation(Summary = "Searches for log entries by correlation ID to track related log events")]
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
    /// Displays log statistics and analytics for a specified date range
    /// </summary>
    /// <param name="fromDate">Start date for statistics (default: 7 days ago)</param>
    /// <param name="toDate">End date for statistics (default: today)</param>
    [SwaggerOperation(Summary = "Displays log statistics and analytics for a specified date range")]
    public async Task<IActionResult> Statistics(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var statistics = await _loggerService.GetLogStatisticsAsync(from, to);

        return View(statistics);
    }
}
