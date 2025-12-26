using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Caching.Memory;

namespace EllisHope.Services;

/// <summary>
/// Provides easy access to dynamic page content for use in Razor views.
/// Inject this service into views using @inject IPageContentHelper Content
/// </summary>
public interface IPageContentHelper
{
    /// <summary>
    /// Gets text content for a page section. Returns plain text (HTML-encoded when rendered).
    /// </summary>
    /// <param name="pageName">The page name (e.g., "About", "Home")</param>
    /// <param name="key">The content section key (e.g., "HeroTitle", "MissionStatement")</param>
    /// <returns>The content text, or template default if not customized</returns>
    string Text(string pageName, string key);

    /// <summary>
    /// Gets HTML content for a page section. Returns IHtmlContent that renders without encoding.
    /// Use this for rich text content that may contain HTML formatting.
    /// </summary>
    /// <param name="pageName">The page name (e.g., "About", "Home")</param>
    /// <param name="key">The content section key (e.g., "AboutSummary", "ServicesIntro")</param>
    /// <returns>The HTML content, or template default if not customized</returns>
    IHtmlContent Html(string pageName, string key);

    /// <summary>
    /// Gets the image URL for a page image slot.
    /// </summary>
    /// <param name="pageName">The page name (e.g., "About", "Home")</param>
    /// <param name="key">The image key (e.g., "HeaderBanner", "MissionImage")</param>
    /// <returns>The image URL from Media Library, or template fallback path</returns>
    string Image(string pageName, string key);

    /// <summary>
    /// Preloads all content for a page into cache. Call this at the start of a view
    /// to minimize database queries when rendering multiple content sections.
    /// </summary>
    /// <param name="pageName">The page name to preload</param>
    Task PreloadAsync(string pageName);
}

public class PageContentHelper : IPageContentHelper
{
    private readonly IPageService _pageService;
    private readonly IPageTemplateService _templateService;
    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PageContentHelper> _logger;

    // Cache keys
    private static string GetPageCacheKey(string pageName) => $"PageContent_{pageName}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    // Templates folder for custom template images
    private const string TemplatesFolder = "assets/img/templates";

    public PageContentHelper(
        IPageService pageService,
        IPageTemplateService templateService,
        IMemoryCache cache,
        IWebHostEnvironment environment,
        ILogger<PageContentHelper> logger)
    {
        _pageService = pageService;
        _templateService = templateService;
        _cache = cache;
        _environment = environment;
        _logger = logger;
    }

    public string Text(string pageName, string key)
    {
        var pageData = GetOrLoadPageData(pageName);
        return GetContentValue(pageData, pageName, key);
    }

    public IHtmlContent Html(string pageName, string key)
    {
        var content = Text(pageName, key);
        return new HtmlString(content);
    }

    public string Image(string pageName, string key)
    {
        var pageData = GetOrLoadPageData(pageName);
        return GetImageUrl(pageData, pageName, key);
    }

    public async Task PreloadAsync(string pageName)
    {
        var cacheKey = GetPageCacheKey(pageName);

        if (!_cache.TryGetValue(cacheKey, out PageCacheData? _))
        {
            var pageData = await LoadPageDataAsync(pageName);
            _cache.Set(cacheKey, pageData, CacheDuration);
        }
    }

    private PageCacheData GetOrLoadPageData(string pageName)
    {
        var cacheKey = GetPageCacheKey(pageName);

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            // Load synchronously (we're in a view context)
            // This is acceptable because:
            // 1. Data is cached after first load
            // 2. PreloadAsync can be called at view start for better performance
            return LoadPageDataAsync(pageName).GetAwaiter().GetResult();
        })!;
    }

    private async Task<PageCacheData> LoadPageDataAsync(string pageName)
    {
        var page = await _pageService.GetPageByNameAsync(pageName);
        var template = _templateService.GetPageTemplate(pageName);

        var cacheData = new PageCacheData
        {
            PageName = pageName,
            Template = template
        };

        if (page != null)
        {
            // Load content sections into dictionary for fast lookup
            foreach (var section in page.ContentSections)
            {
                cacheData.ContentSections[section.SectionKey] = section.Content ?? string.Empty;
            }

            // Load images into dictionary for fast lookup
            foreach (var pageImage in page.PageImages)
            {
                if (pageImage.Media != null)
                {
                    cacheData.ImagePaths[pageImage.ImageKey] = pageImage.Media.FilePath;
                }
            }

            _logger.LogDebug("Loaded page data for {PageName}: {ContentCount} content sections, {ImageCount} images",
                pageName, cacheData.ContentSections.Count, cacheData.ImagePaths.Count);
        }
        else
        {
            _logger.LogDebug("Page {PageName} not found in database, using template defaults", pageName);
        }

        return cacheData;
    }

    private string GetContentValue(PageCacheData pageData, string pageName, string key)
    {
        // First check if there's database content
        if (pageData.ContentSections.TryGetValue(key, out var dbContent) && !string.IsNullOrEmpty(dbContent))
        {
            return dbContent;
        }

        // Fall back to template default
        var templateContent = pageData.Template?.ContentAreas.FirstOrDefault(c => c.Key == key);
        if (templateContent != null && !string.IsNullOrEmpty(templateContent.CurrentTemplateValue))
        {
            return templateContent.CurrentTemplateValue;
        }

        // No content found
        _logger.LogWarning("No content found for page {PageName}, key {Key}", pageName, key);
        return string.Empty;
    }

    private string GetImageUrl(PageCacheData pageData, string pageName, string key)
    {
        // First check if there's a database image (from Media Library)
        if (pageData.ImagePaths.TryGetValue(key, out var dbImagePath) && !string.IsNullOrEmpty(dbImagePath))
        {
            return dbImagePath;
        }

        // Check for custom template image in templates folder
        var customTemplatePath = FindCustomTemplateImage(pageName, key);
        if (customTemplatePath != null)
        {
            return customTemplatePath;
        }

        // Fall back to template default
        var templateImage = pageData.Template?.Images.FirstOrDefault(i => i.Key == key);
        if (templateImage != null)
        {
            return templateImage.CurrentTemplatePath ?? templateImage.FallbackPath ?? "/assets/img/default.jpg";
        }

        // No image found
        _logger.LogWarning("No image found for page {PageName}, key {Key}", pageName, key);
        return "/assets/img/default.jpg";
    }

    /// <summary>
    /// Checks if a custom template image exists in the templates folder.
    /// Looks for files matching the pattern {pagename}-{key}.*
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
            // Return the web path for the first matching file
            var fileName = Path.GetFileName(matchingFiles[0]);
            return $"/{TemplatesFolder}/{fileName}";
        }

        return null;
    }

    /// <summary>
    /// Internal cache data structure for a page
    /// </summary>
    private class PageCacheData
    {
        public string PageName { get; set; } = string.Empty;
        public PageTemplate? Template { get; set; }
        public Dictionary<string, string> ContentSections { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ImagePaths { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
