using Microsoft.AspNetCore.Mvc;
using EllisHope.Services;
using Swashbuckle.AspNetCore.Annotations;
using EllisHope.Models.Domain;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class BlogController : Controller
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    // GET: Blog/Classic
    /// <summary>
    /// Displays blog posts in classic list layout with optional search and category filtering
    /// </summary>
    /// <param name="search">Optional keyword to search in blog post titles and content</param>
    /// <param name="category">Optional category ID to filter posts by category</param>
    /// <returns>View displaying blog posts in classic list format with sidebar categories</returns>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /Blog/Classic
    ///     GET /Blog/Classic?search=fundraising
    ///     GET /Blog/Classic?category=3
    ///     GET /Blog/Classic?search=community&amp;category=5
    ///
    /// Returns only published blog posts. Includes category list in sidebar for navigation.
    /// Search performs full-text search across title and content fields.
    /// </remarks>
    /// <response code="200">Successfully retrieved and displayed blog posts</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves blog posts with search and category filtering (classic layout)",
        Description = "Displays published blog posts in a traditional list format. Supports keyword search and category-based filtering. Returns posts with categories for sidebar navigation.",
        OperationId = "GetBlogClassic",
        Tags = new[] { "Blog" }
    )]
    [ProducesResponseType(typeof(IEnumerable<BlogPost>), StatusCodes.Status200OK)]
    public async Task<IActionResult> classic(string? search, int? category)
    {
        IEnumerable<Models.Domain.BlogPost> posts;

        if (!string.IsNullOrWhiteSpace(search))
        {
            posts = await _blogService.SearchPostsAsync(search);
        }
        else if (category.HasValue)
        {
            posts = await _blogService.GetPostsByCategoryAsync(category.Value);
        }
        else
        {
            posts = await _blogService.GetAllPostsAsync();
        }

        ViewBag.Categories = await _blogService.GetAllCategoriesAsync();
        ViewBag.SearchTerm = search;
        ViewBag.SelectedCategory = category;

        return View(posts);
    }

    // GET: Blog/Details/slug
    /// <summary>
    /// Displays detailed view of a single blog post identified by URL slug
    /// </summary>
    /// <param name="slug">URL-friendly slug identifying the blog post (e.g., "community-fundraiser-2024")</param>
    /// <returns>View displaying full blog post content with related posts, categories, and tags</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Blog/Details/our-mission-statement
    ///
    /// Returns the full blog post including:
    /// - Complete post content with HTML formatting
    /// - Post metadata (author, publish date, category, tags)
    /// - Sidebar with categories, recent posts (excluding current), and popular tags
    /// - Featured image if available
    ///
    /// Route also accessible via custom route: /blog/details/{slug}
    /// </remarks>
    /// <response code="200">Successfully retrieved blog post details</response>
    /// <response code="404">Blog post with specified slug not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves individual blog post by URL slug",
        Description = "Returns complete blog post content with metadata, related posts, and navigation elements. Includes categories, recent posts, and tags in sidebar.",
        OperationId = "GetBlogDetails",
        Tags = new[] { "Blog" }
    )]
    [ProducesResponseType(typeof(BlogPost), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var post = await _blogService.GetPostBySlugAsync(slug);
        if (post == null)
        {
            return NotFound();
        }

        // Populate sidebar data
        ViewBag.Categories = await _blogService.GetAllCategoriesAsync();
        
        // Get recent posts (excluding current post)
        var allPosts = await _blogService.GetAllPostsAsync();
        ViewBag.RecentPosts = allPosts
            .Where(p => p.Id != post.Id)
            .OrderByDescending(p => p.PublishedDate)
            .Take(3);
        
        // Get all tags from all posts
        var allTags = allPosts
            .Where(p => !string.IsNullOrEmpty(p.Tags))
            .SelectMany(p => p.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct()
            .Take(10);
        ViewBag.AllTags = allTags;

        return View(post);
    }

    /// <summary>
    /// Displays blog posts in grid/card layout format
    /// </summary>
    /// <returns>View displaying blog posts in responsive grid layout</returns>
    /// <remarks>
    /// Alternative visual layout for blog post listing. Displays posts as cards in a responsive grid
    /// format, optimized for visual browsing with featured images.
    /// </remarks>
    /// <response code="200">Successfully displayed blog grid view</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays blog posts in grid/card layout",
        Description = "Alternative blog listing view using card-based grid layout. Optimized for visual content browsing with featured images.",
        OperationId = "GetBlogGrid",
        Tags = new[] { "Blog" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult grid()
    {
        return View();
    }
}
