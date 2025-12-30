using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "CanManageCauses")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CausesController : Controller
{
    private readonly ICauseService _causeService;
    private readonly IMediaService _mediaService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CausesController> _logger;

    public CausesController(
        ICauseService causeService,
        IMediaService mediaService,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<CausesController> logger)
    {
        _causeService = causeService;
        _mediaService = mediaService;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: Admin/Causes
    /// <summary>
    /// list causes. Roles: Admin, Editor.
    /// </summary>
    [SwaggerOperation(Summary = "list causes. Roles: Admin, Editor.")]
    public async Task<IActionResult> Index(string? searchTerm, bool showUnpublished = true, bool showActiveOnly = false, string? categoryFilter = null)
    {
        IEnumerable<Cause> causes;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            causes = await _causeService.SearchCausesAsync(searchTerm);
        }
        else if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            causes = await _causeService.GetCausesByCategoryAsync(categoryFilter);
        }
        else if (showActiveOnly)
        {
            causes = await _causeService.GetActiveCausesAsync();
        }
        else
        {
            causes = await _causeService.GetAllCausesAsync(showUnpublished);
        }

        var viewModel = new CauseListViewModel
        {
            Causes = causes,
            SearchTerm = searchTerm,
            ShowUnpublished = showUnpublished,
            ShowActiveOnly = showActiveOnly,
            CategoryFilter = categoryFilter
        };

        return View(viewModel);
    }

    // GET: Admin/Causes/Create
    /// <summary>
    /// create cause. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "create cause. Anti-forgery required.")]
    public IActionResult Create()
    {
        SetTinyMceApiKey();
        
        var viewModel = new CauseViewModel
        {
            StartDate = DateTime.Now,
            IsPublished = false,
            IsFeatured = false
        };

        return View(viewModel);
    }

    // POST: Admin/Causes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// create cause. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "create cause. Anti-forgery required.")]
    public async Task<IActionResult> Create(CauseViewModel model)
    {
        // Log all ModelState errors for debugging
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Errors = x.Value?.Errors.Select(e => e.ErrorMessage ?? e.Exception?.Message).ToList()
                })
                .ToList();

            var errorMessage = string.Join("; ", errors.Select(e => 
                $"{e.Field}: {string.Join(", ", e.Errors ?? new List<string>())}"));

            TempData["ErrorMessage"] = $"Validation failed: {errorMessage}";
            SetTinyMceApiKey();
            return View(model);
        }

        var cause = new Cause
        {
            Title = model.Title,
            Slug = string.IsNullOrWhiteSpace(model.Slug)
                ? _causeService.GenerateSlug(model.Title)
                : model.Slug,
            Description = model.Description,
            ShortDescription = model.ShortDescription,
            Category = model.Category,
            GoalAmount = model.GoalAmount,
            RaisedAmount = model.RaisedAmount,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            DonationUrl = model.DonationUrl,
            IsPublished = model.IsPublished,
            IsFeatured = model.IsFeatured,
            Tags = model.Tags
        };

        // Handle featured image - prioritize Media Library
        if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
        {
            cause.FeaturedImageUrl = model.FeaturedImageUrl;
        }
        else if (model.FeaturedImageFile != null)
        {
            // Upload to centralized Media Manager
            var uploadedBy = User.Identity?.Name ?? "System";
            var media = await _mediaService.UploadLocalImageAsync(
                model.FeaturedImageFile,
                $"Cause: {model.Title}",
                model.Title,
                MediaCategory.Cause,
                model.Tags,
                uploadedBy);
            
            if (media != null)
            {
                cause.FeaturedImageUrl = media.FilePath;
                
                // Track usage
                await _causeService.CreateCauseAsync(cause);
                await _mediaService.TrackMediaUsageAsync(media.Id, "Cause", cause.Id, UsageType.Featured);
                
                TempData["SuccessMessage"] = "Cause created successfully!";
                return RedirectToAction(nameof(Index));
            }
        }

        await _causeService.CreateCauseAsync(cause);

        TempData["SuccessMessage"] = "Cause created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Causes/Edit/5
    /// <summary>
    /// update cause. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update cause. Anti-forgery required.")]
    public async Task<IActionResult> Edit(int id)
    {
        SetTinyMceApiKey();
        
        var cause = await _causeService.GetCauseByIdAsync(id);
        if (cause == null)
        {
            return NotFound();
        }

        var viewModel = new CauseViewModel
        {
            Id = cause.Id,
            Title = cause.Title,
            Slug = cause.Slug,
            Description = cause.Description,
            ShortDescription = cause.ShortDescription,
            Category = cause.Category,
            FeaturedImageUrl = cause.FeaturedImageUrl,
            GoalAmount = cause.GoalAmount,
            RaisedAmount = cause.RaisedAmount,
            StartDate = cause.StartDate,
            EndDate = cause.EndDate,
            DonationUrl = cause.DonationUrl,
            IsPublished = cause.IsPublished,
            IsFeatured = cause.IsFeatured,
            Tags = cause.Tags
        };

        return View(viewModel);
    }

    // POST: Admin/Causes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// update cause. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update cause. Anti-forgery required.")]
    public async Task<IActionResult> Edit(int id, CauseViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        // Log all ModelState errors for debugging
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Errors = x.Value?.Errors.Select(e => e.ErrorMessage ?? e.Exception?.Message).ToList()
                })
                .ToList();

            var errorMessage = string.Join("; ", errors.Select(e => 
                $"{e.Field}: {string.Join(", ", e.Errors ?? new List<string>())}"));

            TempData["ErrorMessage"] = $"Validation failed: {errorMessage}";
            SetTinyMceApiKey();
            return View(model);
        }

        var cause = await _causeService.GetCauseByIdAsync(id);
        if (cause == null)
        {
            return NotFound();
        }

        cause.Title = model.Title;
        cause.Slug = string.IsNullOrWhiteSpace(model.Slug)
            ? _causeService.GenerateSlug(model.Title)
            : model.Slug;
        cause.Description = model.Description;
        cause.ShortDescription = model.ShortDescription;
        cause.Category = model.Category;
        cause.GoalAmount = model.GoalAmount;
        cause.RaisedAmount = model.RaisedAmount;
        cause.StartDate = model.StartDate;
        cause.EndDate = model.EndDate;
        cause.DonationUrl = model.DonationUrl;
        cause.IsPublished = model.IsPublished;
        cause.IsFeatured = model.IsFeatured;
        cause.Tags = model.Tags;

        // Handle featured image update
        if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl) && model.FeaturedImageUrl != cause.FeaturedImageUrl)
        {
            // New image from Media Library - just update URL
            cause.FeaturedImageUrl = model.FeaturedImageUrl;
        }
        else if (model.FeaturedImageFile != null)
        {
            // New file upload - use Media Manager
            var uploadedBy = User.Identity?.Name ?? "System";
            var media = await _mediaService.UploadLocalImageAsync(
                model.FeaturedImageFile,
                $"Cause: {model.Title}",
                model.Title,
                MediaCategory.Cause,
                model.Tags,
                uploadedBy);
            
            if (media != null)
            {
                cause.FeaturedImageUrl = media.FilePath;
                
                // Save first to get the ID
                await _causeService.UpdateCauseAsync(cause);
                
                // Then track usage
                await _mediaService.TrackMediaUsageAsync(media.Id, "Cause", cause.Id, UsageType.Featured);
                
                TempData["SuccessMessage"] = "Cause updated successfully!";
                return RedirectToAction(nameof(Index));
            }
        }
        // If neither condition is met, image stays the same (which is correct)

        await _causeService.UpdateCauseAsync(cause);

        TempData["SuccessMessage"] = "Cause updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Causes/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// delete cause. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "delete cause. Anti-forgery required.")]
    public async Task<IActionResult> Delete(int id)
    {
        var cause = await _causeService.GetCauseByIdAsync(id);
        if (cause == null)
        {
            return NotFound();
        }

        // Remove media usage tracking
        if (!string.IsNullOrEmpty(cause.FeaturedImageUrl))
        {
            // Find media by path
            var allMedia = await _mediaService.GetAllMediaAsync();
            var media = allMedia.FirstOrDefault(m => m.FilePath == cause.FeaturedImageUrl);
            if (media != null)
            {
                await _mediaService.RemoveMediaUsageAsync(media.Id, "Cause", id);
            }
        }

        await _causeService.DeleteCauseAsync(id);

        TempData["SuccessMessage"] = "Cause deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // Helper methods
    private void SetTinyMceApiKey()
    {
        ViewData["TinyMceApiKey"] = _configuration["TinyMCE:ApiKey"] ?? "no-api-key";
    }
}
