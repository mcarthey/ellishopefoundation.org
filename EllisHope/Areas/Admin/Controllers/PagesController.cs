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
    private readonly IPageTemplateService _templateService;
    private readonly IMediaService _mediaService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PagesController> _logger;

    public PagesController(
        IPageService pageService,
        IPageTemplateService templateService,
        IMediaService mediaService,
        IConfiguration configuration,
        ILogger<PagesController> logger)
    {
        _pageService = pageService;
        _templateService = templateService;
        _mediaService = mediaService;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: Admin/Pages
    /// <summary>
    /// list pages (Admin, Editor).
    /// </summary>
    [SwaggerOperation(Summary = "list pages (Admin, Editor).")]
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
    /// <summary>
    /// edit page (sections, images).
    /// </summary>
    [SwaggerOperation(Summary = "edit page (sections, images).")]
    public async Task<IActionResult> Edit(int id)
    {
        SetTinyMceApiKey();
        
        var page = await _pageService.GetPageByIdAsync(id);
        if (page == null)
        {
            return NotFound();
        }

        // Get the template for this page type
        var template = _templateService.GetPageTemplate(page.PageName);
        
        // Populate current values from database
        foreach (var img in template.Images)
        {
            var currentImg = page.PageImages.FirstOrDefault(pi => pi.ImageKey == img.Key);
            if (currentImg != null)
            {
                img.CurrentImagePath = currentImg.Media?.FilePath;
                img.CurrentMediaId = currentImg.MediaId;
            }
        }

        foreach (var content in template.ContentAreas)
        {
            var currentContent = page.ContentSections.FirstOrDefault(cs => cs.SectionKey == content.Key);
            if (currentContent != null)
            {
                content.CurrentContent = currentContent.Content;
            }
        }

        ViewBag.PageId = page.Id;
        ViewBag.PageName = page.PageName;
        ViewBag.PageTemplate = template;
        ViewBag.AvailableMedia = await _mediaService.GetAllMediaAsync();

        return View(template);
    }

    // POST: Admin/Pages/UpdateSection
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// update section content. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update section content. Anti-forgery required.")]
    public async Task<IActionResult> UpdateContent(int pageId, string sectionKey, string content, string contentType = "RichText")
    {
        try
        {
            await _pageService.UpdateContentSectionAsync(pageId, sectionKey, content ?? string.Empty, contentType);
            
            TempData["SuccessMessage"] = "Content updated successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating content section");
            TempData["ErrorMessage"] = $"Error updating content: {ex.Message}";
        }

        return RedirectToAction(nameof(Edit), new { id = pageId });
    }

    // POST: Admin/Pages/UpdateImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// update image reference. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update image reference. Anti-forgery required.")]
    public async Task<IActionResult> UpdateImage(int pageId, string imageKey, int mediaId)
    {
        try
        {
            await _pageService.SetPageImageAsync(pageId, imageKey, mediaId, 0);
            
            TempData["SuccessMessage"] = "Image updated successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page image");
            TempData["ErrorMessage"] = $"Error updating image: {ex.Message}";
        }

        return RedirectToAction(nameof(Edit), new { id = pageId });
    }

    // POST: Admin/Pages/RemoveImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// remove image from page. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "remove image from page. Anti-forgery required.")]
    public async Task<IActionResult> RemoveImage(int pageId, string imageKey)
    {
        try
        {
            await _pageService.RemovePageImageAsync(pageId, imageKey);
            TempData["SuccessMessage"] = "Image removed successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing page image");
            TempData["ErrorMessage"] = $"Error removing image: {ex.Message}";
        }

        return RedirectToAction(nameof(Edit), new { id = pageId });
    }

    // Helper methods
    private void SetTinyMceApiKey()
    {
        ViewData["TinyMceApiKey"] = _configuration["TinyMCE:ApiKey"] ?? "no-api-key";
    }
}
