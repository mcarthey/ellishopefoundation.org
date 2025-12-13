using EllisHope.Models.Domain;
using EllisHope.Models.ViewModels;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class MediaController : Controller
{
    private readonly IMediaService _mediaService;
    private readonly IUnsplashService _unsplashService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(
        IMediaService mediaService,
        IUnsplashService unsplashService,
        ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _unsplashService = unsplashService;
        _logger = logger;
    }

    // GET: Admin/Media
    public async Task<IActionResult> Index(string? searchTerm, MediaCategory? category, MediaSource? source)
    {
        IEnumerable<Media> media;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            media = await _mediaService.SearchMediaAsync(searchTerm, category);
        }
        else
        {
            media = await _mediaService.GetAllMediaAsync(category, source);
        }

        var viewModel = new MediaLibraryViewModel
        {
            Media = media,
            SearchTerm = searchTerm,
            FilterCategory = category,
            FilterSource = source,
            TotalMediaCount = await _mediaService.GetTotalMediaCountAsync(),
            TotalStorageSize = await _mediaService.GetTotalStorageSizeAsync(),
            MediaCountByCategory = await _mediaService.GetMediaCountByCategoryAsync()
        };

        return View(viewModel);
    }

    // GET: Admin/Media/Upload
    public IActionResult Upload()
    {
        return View(new MediaUploadViewModel());
    }

    // POST: Admin/Media/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(MediaUploadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var uploadedBy = User.Identity?.Name;
            var media = await _mediaService.UploadLocalImageAsync(
                model.File!,
                model.AltText,
                model.Title,
                model.Category,
                model.Tags,
                uploadedBy);

            TempData["SuccessMessage"] = $"Image '{media.FileName}' uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media");
            ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
            return View(model);
        }
    }

    // GET: Admin/Media/UnsplashSearch
    public IActionResult UnsplashSearch()
    {
        return View(new UnsplashSearchViewModel());
    }

    // POST: Admin/Media/UnsplashSearch
    [HttpPost]
    public async Task<IActionResult> UnsplashSearch(string query, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return View(new UnsplashSearchViewModel());
        }

        try
        {
            var results = await _unsplashService.SearchPhotosAsync(query, page);

            var viewModel = new UnsplashSearchViewModel
            {
                Query = query,
                Page = page,
                Results = results
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Unsplash");
            ModelState.AddModelError("", $"Error searching Unsplash: {ex.Message}");
            return View(new UnsplashSearchViewModel { Query = query });
        }
    }

    // POST: Admin/Media/ImportFromUnsplash
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromUnsplash(ImportUnsplashPhotoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid photo data";
            return RedirectToAction(nameof(UnsplashSearch));
        }

        try
        {
            var uploadedBy = User.Identity?.Name;
            var media = await _mediaService.ImportFromUnsplashAsync(
                model.UnsplashPhotoId,
                model.AltText,
                model.Category,
                model.Tags,
                uploadedBy);

            TempData["SuccessMessage"] = $"Image from Unsplash imported successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from Unsplash");
            TempData["ErrorMessage"] = $"Error importing image: {ex.Message}";
            return RedirectToAction(nameof(UnsplashSearch));
        }
    }

    // GET: Admin/Media/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var media = await _mediaService.GetMediaByIdAsync(id);
        if (media == null)
        {
            return NotFound();
        }

        var viewModel = MediaEditViewModel.FromMedia(media);
        return View(viewModel);
    }

    // POST: Admin/Media/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MediaEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var media = await _mediaService.GetMediaByIdAsync(id);
            if (media == null)
            {
                return NotFound();
            }

            model.ApplyTo(media);
            await _mediaService.UpdateMediaAsync(media);

            TempData["SuccessMessage"] = "Media updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media {MediaId}", id);
            ModelState.AddModelError("", $"Error updating media: {ex.Message}");
            return View(model);
        }
    }

    // POST: Admin/Media/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, bool force = false)
    {
        try
        {
            var success = await _mediaService.DeleteMediaAsync(id, force);

            if (success)
            {
                TempData["SuccessMessage"] = "Media deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot delete media - it is currently in use. Check 'Force Delete' to delete anyway.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media {MediaId}", id);
            TempData["ErrorMessage"] = $"Error deleting media: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Media/Usages/5
    public async Task<IActionResult> Usages(int id)
    {
        var media = await _mediaService.GetMediaByIdAsync(id);
        if (media == null)
        {
            return NotFound();
        }

        var usages = await _mediaService.GetMediaUsagesAsync(id);

        ViewBag.Media = media;
        return View(usages);
    }

    // API endpoint for AJAX requests
    [HttpGet]
    public async Task<IActionResult> GetMediaJson(MediaCategory? category)
    {
        var media = await _mediaService.GetAllMediaAsync(category);
        return Json(media.Select(m => new
        {
            m.Id,
            m.FileName,
            m.FilePath,
            m.AltText,
            m.Width,
            m.Height,
            m.Category,
            m.Source
        }));
    }
}
