using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Purge(int daysToKeep = 90, AppLogLevel? minLevelToKeep = null)
    {
        var deletedCount = await _loggerService.PurgeOldLogsAsync(daysToKeep, minLevelToKeep);
        TempData["SuccessMessage"] = $"Purged {deletedCount} old log entries.";

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Logs/Search
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
    public async Task<IActionResult> Statistics(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var statistics = await _loggerService.GetLogStatisticsAsync(from, to);

        return View(statistics);
    }
}
