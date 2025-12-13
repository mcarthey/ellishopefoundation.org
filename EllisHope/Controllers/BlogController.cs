using Microsoft.AspNetCore.Mvc;
using EllisHope.Services;

namespace EllisHope.Controllers;

public class BlogController : Controller
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    // GET: Blog/Classic
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

    public IActionResult grid()
    {
        return View();
    }
}
