using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class PagesController : Controller
{
    private readonly IPageService _pageService;
    private readonly IMediaService _mediaService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PagesController> _logger;

    public PagesController(
        IPageService pageService,
        IMediaService mediaService,
        IConfiguration configuration,
        ILogger<PagesController> logger)
    {
        _pageService = pageService;
        _mediaService = mediaService;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: Admin/Pages
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var pages = await _pageService.GetAllPagesAsync();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            pages = pages.Where(p =>
                p.PageName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (p.Title != null && p.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        var viewModel = new PageListViewModel
        {
            Pages = pages,
            SearchTerm = searchTerm
        };

        return View(viewModel);
    }

    // GET: Admin/Pages/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        SetTinyMceApiKey();
        
        var page = await _pageService.GetPageByIdAsync(id);
        if (page == null)
        {
            return NotFound();
        }

        var viewModel = new PageEditViewModel
        {
            Id = page.Id,
            PageName = page.PageName,
            Title = page.Title,
            MetaDescription = page.MetaDescription,
            IsPublished = page.IsPublished,
            ContentSections = page.ContentSections
                .Select(ContentSectionViewModel.FromContentSection)
                .ToList(),
            PageImages = page.PageImages
                .Select(PageImageViewModel.FromPageImage)
                .ToList()
        };

        // Get available media for selection
        ViewBag.AvailableMedia = await _mediaService.GetAllMediaAsync();

        return View(viewModel);
    }

    // POST: Admin/Pages/UpdateSection
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSection(QuickEditSectionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid section data";
            return RedirectToAction(nameof(Edit), new { id = model.PageId });
        }

        try
        {
            await _pageService.UpdateContentSectionAsync(
                model.PageId,
                model.SectionKey,
                model.Content ?? string.Empty,
                model.ContentType);

            TempData["SuccessMessage"] = $"Section '{model.SectionKey}' updated successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating content section");
            TempData["ErrorMessage"] = $"Error updating section: {ex.Message}";
        }

        return RedirectToAction(nameof(Edit), new { id = model.PageId });
    }

    // POST: Admin/Pages/UpdateImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateImage(QuickEditImageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid image data";
            return RedirectToAction(nameof(Edit), new { id = model.PageId });
        }

        try
        {
            await _pageService.SetPageImageAsync(
                model.PageId,
                model.ImageKey,
                model.MediaId,
                model.DisplayOrder);

            TempData["SuccessMessage"] = $"Image '{model.ImageKey}' updated successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page image");
            TempData["ErrorMessage"] = $"Error updating image: {ex.Message}";
        }

        return RedirectToAction(nameof(Edit), new { id = model.PageId });
    }

    // POST: Admin/Pages/RemoveImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveImage(int pageId, string imageKey)
    {
        try
        {
            await _pageService.RemovePageImageAsync(pageId, imageKey);
            TempData["SuccessMessage"] = $"Image '{imageKey}' removed successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing page image");
            TempData["ErrorMessage"] = $"Error removing image: {ex.Message}";
        }

        return RedirectToAction(nameof(Edit), new { id = pageId });
    }

    // GET: Admin/Pages/MediaPicker
    public async Task<IActionResult> MediaPicker(int pageId, string imageKey)
    {
        var media = await _mediaService.GetAllMediaAsync();
        var page = await _pageService.GetPageByIdAsync(pageId);
        var currentImage = await _pageService.GetPageImageAsync(pageId, imageKey);

        var viewModel = new QuickEditImageViewModel
        {
            PageId = pageId,
            PageName = page?.PageName ?? "Unknown",
            ImageKey = imageKey,
            CurrentMediaId = currentImage?.MediaId,
            CurrentMediaPath = currentImage?.Media?.FilePath,
            DisplayOrder = currentImage?.DisplayOrder ?? 0
        };

        ViewBag.AvailableMedia = media;
        return View(viewModel);
    }

    // Helper methods
    private void SetTinyMceApiKey()
    {
        ViewData["TinyMceApiKey"] = _configuration["TinyMCE:ApiKey"] ?? "no-api-key";
    }
}
