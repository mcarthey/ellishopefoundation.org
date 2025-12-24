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
    /// <summary>
    /// media index/list. Roles: Admin, Editor.
    /// </summary>
    [SwaggerOperation(Summary = "media index/list. Roles: Admin, Editor.")]
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
    /// <summary>
    /// upload file (multipart form). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "upload file (multipart form). Anti-forgery required.")]
    public IActionResult Upload()
    {
        return View(new MediaUploadViewModel());
    }

    // POST: Admin/Media/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// upload file (multipart form). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "upload file (multipart form). Anti-forgery required.")]
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
    /// <summary>
    /// perform Unsplash search.
    /// </summary>
    [SwaggerOperation(Summary = "perform Unsplash search.")]
    public IActionResult UnsplashSearch()
    {
        return View(new UnsplashSearchViewModel());
    }

    // POST: Admin/Media/UnsplashSearch
    [HttpPost]
    /// <summary>
    /// perform Unsplash search.
    /// </summary>
    [SwaggerOperation(Summary = "perform Unsplash search.")]
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
    /// <summary>
    /// import image from Unsplash.
    /// </summary>
    [SwaggerOperation(Summary = "import image from Unsplash.")]
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
    /// <summary>
    /// update metadata. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update metadata. Anti-forgery required.")]
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
    /// <summary>
    /// update metadata. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update metadata. Anti-forgery required.")]
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
    /// <summary>
    /// delete media. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "delete media. Anti-forgery required.")]
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
    /// <summary>
    /// show usages for a media item.
    /// </summary>
    [SwaggerOperation(Summary = "show usages for a media item.")]
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
    /// <summary>
    /// JSON API for media listing (may require auth).
    /// </summary>
    [SwaggerOperation(Summary = "JSON API for media listing (may require auth).")]
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

    // GET: Admin/Media/GetUnusedMedia
    [HttpGet]
    /// <summary>
    /// list unused media (Admin tool).
    /// </summary>
    [SwaggerOperation(Summary = "list unused media (Admin tool).")]
    public async Task<IActionResult> GetUnusedMedia()
    {
        var allMedia = await _mediaService.GetAllMediaAsync();
        var unused = new List<object>();

        foreach (var media in allMedia)
        {
            if (!await _mediaService.IsMediaInUseAsync(media.Id))
            {
                unused.Add(new
                {
                    media.Id,
                    media.FileName,
                    media.FilePath,
                    media.FileSize,
                    media.UploadedDate
                });
            }
        }

        return Json(unused);
    }

    // GET: Admin/Media/GetDuplicates
    [HttpGet]
    /// <summary>
    /// list duplicate media (Admin tool).
    /// </summary>
    [SwaggerOperation(Summary = "list duplicate media (Admin tool).")]
    public async Task<IActionResult> GetDuplicates()
    {
        var allMedia = await _mediaService.GetAllMediaAsync();
        
        // Group by FileHash
        var duplicateGroups = allMedia
            .Where(m => !string.IsNullOrEmpty(m.FileHash))
            .GroupBy(m => m.FileHash)
            .Where(g => g.Count() > 1)
            .Select(g => g.OrderBy(m => m.UploadedDate).Select(m => new
            {
                m.Id,
                m.FileName,
                m.FilePath,
                m.FileHash,
                m.UploadedDate
            }).ToList())
            .ToList();

        return Json(duplicateGroups);
    }

    // POST: Admin/Media/RemoveDuplicates
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// remove duplicates.
    /// </summary>
    [SwaggerOperation(Summary = "remove duplicates.")]
    public async Task<IActionResult> RemoveDuplicates()
    {
        try
        {
            var allMedia = await _mediaService.GetAllMediaAsync();
            var removed = 0;

            var duplicateGroups = allMedia
                .Where(m => !string.IsNullOrEmpty(m.FileHash))
                .GroupBy(m => m.FileHash)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateGroups)
            {
                // Keep oldest, delete rest
                var toKeep = group.OrderBy(m => m.UploadedDate).First();
                var toDelete = group.Where(m => m.Id != toKeep.Id);

                foreach (var duplicate in toDelete)
                {
                    if (await _mediaService.DeleteMediaAsync(duplicate.Id, force: true))
                    {
                        removed++;
                    }
                }
            }

            return Json(new { removed });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing duplicates");
            return Json(new { error = ex.Message });
        }
    }

    // POST: Admin/Media/DeleteAllUnused
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// remove unused media (Admin tool).
    /// </summary>
    [SwaggerOperation(Summary = "remove unused media (Admin tool).")]
    public async Task<IActionResult> DeleteAllUnused()
    {
        try
        {
            var allMedia = await _mediaService.GetAllMediaAsync();
            var deleted = 0;

            foreach (var media in allMedia)
            {
                if (!await _mediaService.IsMediaInUseAsync(media.Id))
                {
                    if (await _mediaService.DeleteMediaAsync(media.Id, force: false))
                    {
                        deleted++;
                    }
                }
            }

            return Json(new { deleted });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unused media");
            return Json(new { error = ex.Message });
        }
    }
}
