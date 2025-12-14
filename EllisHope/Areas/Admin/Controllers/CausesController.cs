using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class CausesController : Controller
{
    private readonly ICauseService _causeService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CausesController> _logger;

    public CausesController(
        ICauseService causeService,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<CausesController> logger)
    {
        _causeService = causeService;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: Admin/Causes
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
    public async Task<IActionResult> Create(CauseViewModel model)
    {
        if (ModelState.IsValid)
        {
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

            // Handle featured image - prioritize FeaturedImageUrl (from Media Library) over file upload
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
            {
                // Image selected from Media Library
                cause.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // Legacy: File uploaded directly (bypassing Media Library)
                cause.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile);
            }

            await _causeService.CreateCauseAsync(cause);

            TempData["SuccessMessage"] = "Cause created successfully!";
            return RedirectToAction(nameof(Index));
        }

        SetTinyMceApiKey();
        return View(model);
    }

    // GET: Admin/Causes/Edit/5
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
    public async Task<IActionResult> Edit(int id, CauseViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
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

            // Handle featured image - prioritize FeaturedImageUrl (from Media Library) over file upload
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl) && model.FeaturedImageUrl != cause.FeaturedImageUrl)
            {
                // New image selected from Media Library
                // Only delete old if it was from local uploads, not Media Library
                if (!string.IsNullOrEmpty(cause.FeaturedImageUrl) && 
                    (cause.FeaturedImageUrl.Contains("/uploads/causes/") || 
                     cause.FeaturedImageUrl.Contains("/uploads/blog/") || 
                     cause.FeaturedImageUrl.Contains("/uploads/events/")))
                {
                    DeleteFeaturedImage(cause.FeaturedImageUrl);
                }
                cause.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // Legacy: New file uploaded directly (bypassing Media Library)
                // Delete old image if exists and was from local uploads
                if (!string.IsNullOrEmpty(cause.FeaturedImageUrl) && 
                    (cause.FeaturedImageUrl.Contains("/uploads/causes/") || 
                     cause.FeaturedImageUrl.Contains("/uploads/blog/") || 
                     cause.FeaturedImageUrl.Contains("/uploads/events/")))
                {
                    DeleteFeaturedImage(cause.FeaturedImageUrl);
                }
                cause.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile);
            }

            await _causeService.UpdateCauseAsync(cause);

            TempData["SuccessMessage"] = "Cause updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        SetTinyMceApiKey();
        return View(model);
    }

    // POST: Admin/Causes/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var cause = await _causeService.GetCauseByIdAsync(id);
        if (cause == null)
        {
            return NotFound();
        }

        // Delete featured image if exists
        if (!string.IsNullOrEmpty(cause.FeaturedImageUrl))
        {
            DeleteFeaturedImage(cause.FeaturedImageUrl);
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

    private async Task<string> SaveFeaturedImageAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "causes");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/causes/{uniqueFileName}";
    }

    private void DeleteFeaturedImage(string imageUrl)
    {
        var imagePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }
    }
}
