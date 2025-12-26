using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EllisHope.Areas.Admin.Models;
using EllisHope.Services;

namespace EllisHope.Areas.Admin.Controllers;

/// <summary>
/// Controller for managing template images - the default images used on public pages
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class TemplatesController : Controller
{
    private readonly IPageTemplateService _templateService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TemplatesController> _logger;

    private const string TemplatesFolder = "assets/img/templates";

    public TemplatesController(
        IPageTemplateService templateService,
        IWebHostEnvironment environment,
        ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Display all template images across all pages
    /// </summary>
    public IActionResult Images()
    {
        var viewModel = new TemplateImagesViewModel();
        var pages = _templateService.GetAvailablePages();

        foreach (var pageName in pages)
        {
            var template = _templateService.GetPageTemplate(pageName);
            if (template.Images == null || !template.Images.Any())
                continue;

            var group = new TemplateImageGroup
            {
                PageName = pageName,
                PageDisplayName = template.DisplayName
            };

            foreach (var image in template.Images)
            {
                // Check for custom template image first
                var customTemplatePath = FindCustomTemplateImage(pageName, image.Key);
                var effectivePath = customTemplatePath ?? image.CurrentTemplatePath ?? image.FallbackPath ?? "/assets/img/default.jpg";

                var slot = new TemplateImageSlot
                {
                    PageName = pageName,
                    Key = image.Key,
                    Label = image.Label,
                    Description = image.Description ?? string.Empty,
                    CurrentPath = effectivePath,
                    TemplatePath = image.CurrentTemplatePath ?? image.FallbackPath ?? string.Empty,
                    RecommendedWidth = image.Requirements?.RecommendedWidth,
                    RecommendedHeight = image.Requirements?.RecommendedHeight,
                    AspectRatio = image.Requirements?.AspectRatio
                };

                // Check if file exists and get size
                var physicalPath = GetPhysicalPath(slot.CurrentPath);
                if (System.IO.File.Exists(physicalPath))
                {
                    slot.FileExists = true;
                    slot.FileSizeBytes = new FileInfo(physicalPath).Length;
                }

                group.ImageSlots.Add(slot);
            }

            if (group.ImageSlots.Any())
            {
                viewModel.PageGroups.Add(group);
            }
        }

        // Check for TempData messages
        if (TempData["SuccessMessage"] != null)
            viewModel.SuccessMessage = TempData["SuccessMessage"]?.ToString();
        if (TempData["ErrorMessage"] != null)
            viewModel.ErrorMessage = TempData["ErrorMessage"]?.ToString();

        return View(viewModel);
    }

    /// <summary>
    /// Upload a new template image to replace an existing one
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(string pageName, string key, IFormFile file)
    {
        if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(key))
        {
            TempData["ErrorMessage"] = "Invalid page name or image key.";
            return RedirectToAction(nameof(Images));
        }

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a file to upload.";
            return RedirectToAction(nameof(Images));
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            TempData["ErrorMessage"] = $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}";
            return RedirectToAction(nameof(Images));
        }

        // Validate file size (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "File size exceeds 10MB limit.";
            return RedirectToAction(nameof(Images));
        }

        try
        {
            // Get the template to find the target path
            var template = _templateService.GetPageTemplate(pageName);
            var imageSlot = template.Images?.FirstOrDefault(i => i.Key == key);

            if (imageSlot == null)
            {
                TempData["ErrorMessage"] = $"Image slot '{key}' not found for page '{pageName}'.";
                return RedirectToAction(nameof(Images));
            }

            // Determine target path - use template path or generate new one in templates folder
            string targetPath;
            string targetPhysicalPath;

            // Check if the current template path is in the templates folder
            if (imageSlot.CurrentTemplatePath?.Contains("/templates/") == true)
            {
                // Use existing templates folder path
                targetPath = imageSlot.CurrentTemplatePath;
            }
            else
            {
                // Create new path in templates folder with consistent naming
                var fileName = $"{pageName.ToLower()}-{key.ToLower()}{extension}";
                targetPath = $"/{TemplatesFolder}/{fileName}";
            }

            targetPhysicalPath = GetPhysicalPath(targetPath);

            // Ensure templates directory exists
            var templatesDir = Path.GetDirectoryName(targetPhysicalPath);
            if (!string.IsNullOrEmpty(templatesDir) && !Directory.Exists(templatesDir))
            {
                Directory.CreateDirectory(templatesDir);
            }

            // Save the file
            using (var stream = new FileStream(targetPhysicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Template image uploaded: {Path} for {Page}/{Key}", targetPath, pageName, key);

            TempData["SuccessMessage"] = $"Successfully uploaded image for {pageName} - {imageSlot.Label}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading template image for {Page}/{Key}", pageName, key);
            TempData["ErrorMessage"] = "An error occurred while uploading the image.";
        }

        return RedirectToAction(nameof(Images));
    }

    /// <summary>
    /// Get information about a specific template image slot
    /// </summary>
    [HttpGet]
    public IActionResult GetSlotInfo(string pageName, string key)
    {
        var template = _templateService.GetPageTemplate(pageName);
        var imageSlot = template.Images?.FirstOrDefault(i => i.Key == key);

        if (imageSlot == null)
        {
            return NotFound();
        }

        var physicalPath = GetPhysicalPath(imageSlot.EffectiveImagePath);
        var fileExists = System.IO.File.Exists(physicalPath);
        long? fileSize = fileExists ? new FileInfo(physicalPath).Length : null;

        return Json(new
        {
            pageName,
            key = imageSlot.Key,
            label = imageSlot.Label,
            description = imageSlot.Description,
            currentPath = imageSlot.EffectiveImagePath,
            templatePath = imageSlot.CurrentTemplatePath,
            fallbackPath = imageSlot.FallbackPath,
            recommendedWidth = imageSlot.Requirements?.RecommendedWidth,
            recommendedHeight = imageSlot.Requirements?.RecommendedHeight,
            aspectRatio = imageSlot.Requirements?.AspectRatio,
            fileExists,
            fileSize
        });
    }

    /// <summary>
    /// Reset a template image to its original fallback
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reset(string pageName, string key)
    {
        if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(key))
        {
            TempData["ErrorMessage"] = "Invalid page name or image key.";
            return RedirectToAction(nameof(Images));
        }

        try
        {
            // Get the template image slot
            var template = _templateService.GetPageTemplate(pageName);
            var imageSlot = template.Images?.FirstOrDefault(i => i.Key == key);

            if (imageSlot == null)
            {
                TempData["ErrorMessage"] = $"Image slot '{key}' not found for page '{pageName}'.";
                return RedirectToAction(nameof(Images));
            }

            // Check if there's a custom template file in the templates folder
            var customFileName = $"{pageName.ToLower()}-{key.ToLower()}";
            var templatesPhysicalPath = Path.Combine(_environment.WebRootPath, TemplatesFolder);

            if (Directory.Exists(templatesPhysicalPath))
            {
                // Find and delete any custom template files for this slot
                var customFiles = Directory.GetFiles(templatesPhysicalPath, $"{customFileName}.*");
                foreach (var file in customFiles)
                {
                    System.IO.File.Delete(file);
                    _logger.LogInformation("Deleted custom template image: {Path}", file);
                }
            }

            TempData["SuccessMessage"] = $"Reset image for {pageName} - {imageSlot.Label} to default.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting template image for {Page}/{Key}", pageName, key);
            TempData["ErrorMessage"] = "An error occurred while resetting the image.";
        }

        return RedirectToAction(nameof(Images));
    }

    private string GetPhysicalPath(string webPath)
    {
        // Remove leading slash and convert to physical path
        var relativePath = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_environment.WebRootPath, relativePath);
    }

    /// <summary>
    /// Checks if a custom template image exists in the templates folder.
    /// </summary>
    private string? FindCustomTemplateImage(string pageName, string key)
    {
        var templatesPath = Path.Combine(_environment.WebRootPath, TemplatesFolder);
        if (!Directory.Exists(templatesPath))
        {
            return null;
        }

        var pattern = $"{pageName.ToLower()}-{key.ToLower()}.*";
        var matchingFiles = Directory.GetFiles(templatesPath, pattern);

        if (matchingFiles.Length > 0)
        {
            var fileName = Path.GetFileName(matchingFiles[0]);
            return $"/{TemplatesFolder}/{fileName}";
        }

        return null;
    }
}
