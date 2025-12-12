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

        return View(post);
    }

    public IActionResult grid()
    {
        return View();
    }
}
