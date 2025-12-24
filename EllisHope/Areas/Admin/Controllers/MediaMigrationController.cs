using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MediaMigrationController : Controller
{
    private readonly IMediaMigrationService _migrationService;
    private readonly ILogger<MediaMigrationController> _logger;

    public MediaMigrationController(
        IMediaMigrationService migrationService,
        ILogger<MediaMigrationController> logger)
    {
        _migrationService = migrationService;
        _logger = logger;
    }

    // GET: Admin/MediaMigration
    /// <summary>
    /// Displays media migration analysis report for legacy images in the system
    /// </summary>
    /// <remarks>Requires Admin role authorization. Analyzes legacy image paths and provides migration recommendations.</remarks>
    [SwaggerOperation(Summary = "Displays media migration analysis report for legacy images in the system")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var report = await _migrationService.AnalyzeLegacyImagesAsync();
            return View(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing legacy images");
            TempData["ErrorMessage"] = $"Error analyzing images: {ex.Message}";
            return View();
        }
    }

    // GET: Admin/MediaMigration/BrokenReferences
    /// <summary>
    /// list broken media refs.
    /// </summary>
    [SwaggerOperation(Summary = "list broken media refs.")]
    public async Task<IActionResult> BrokenReferences()
    {
        try
        {
            var brokenRefs = await _migrationService.FindBrokenReferencesAsync();
            return View(brokenRefs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding broken references");
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Admin/MediaMigration/Migrate
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// run migration tool.
    /// </summary>
    [SwaggerOperation(Summary = "run migration tool.")]
    public async Task<IActionResult> Migrate(bool updateDatabaseReferences = true, bool deleteOldFiles = false)
    {
        try
        {
            _logger.LogInformation("Starting media migration (UpdateDB: {UpdateDB}, DeleteOld: {DeleteOld})",
                updateDatabaseReferences, deleteOldFiles);

            var result = await _migrationService.MigrateLegacyImagesAsync(updateDatabaseReferences, deleteOldFiles);

            if (result.SuccessfullyMigrated > 0)
            {
                TempData["SuccessMessage"] = $"Migration completed! " +
                    $"Migrated: {result.SuccessfullyMigrated}, " +
                    $"Already in library: {result.AlreadyInMediaLibrary}, " +
                    $"Failed: {result.Failed}, " +
                    $"DB references updated: {result.DatabaseReferencesUpdated}, " +
                    $"Files deleted: {result.FilesDeleted}";
            }
            else
            {
                TempData["WarningMessage"] = "No images were migrated. " +
                    $"Already in library: {result.AlreadyInMediaLibrary}";
            }

            if (result.Errors.Any())
            {
                TempData["ErrorMessage"] = $"Some errors occurred: {string.Join(", ", result.Errors.Take(5))}";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during migration");
            TempData["ErrorMessage"] = $"Migration failed: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Admin/MediaMigration/RemoveDuplicates
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// remove duplicates discovered during migration.
    /// </summary>
    [SwaggerOperation(Summary = "remove duplicates discovered during migration.")]
    public async Task<IActionResult> RemoveDuplicates()
    {
        try
        {
            var removed = await _migrationService.RemoveDuplicateImagesAsync();

            if (removed > 0)
            {
                TempData["SuccessMessage"] = $"Removed {removed} duplicate images";
            }
            else
            {
                TempData["InfoMessage"] = "No duplicate images found";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing duplicates");
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
