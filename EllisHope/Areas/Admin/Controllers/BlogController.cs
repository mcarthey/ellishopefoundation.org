using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class BlogController : Controller
{
    private readonly IBlogService _blogService;
    private readonly IMediaService _mediaService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public BlogController(
        IBlogService blogService, 
        IMediaService mediaService,
        IWebHostEnvironment environment, 
        IConfiguration configuration)
    {
        _blogService = blogService;
        _mediaService = mediaService;
        _environment = environment;
        _configuration = configuration;
    }

    // GET: Admin/Blog
    /// <summary>
    /// list/filter posts (`searchTerm`, `categoryFilter`, `showUnpublished`). Roles: Admin, Editor.
    /// </summary>
    [SwaggerOperation(Summary = "list/filter posts (`searchTerm`, `categoryFilter`, `showUnpublished`). Roles: Admin, Editor.")]
    public async Task<IActionResult> Index(string? searchTerm, int? categoryFilter, bool showUnpublished = true)
    {
        IEnumerable<BlogPost> posts;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            posts = await _blogService.SearchPostsAsync(searchTerm);
        }
        else if (categoryFilter.HasValue)
        {
            posts = await _blogService.GetPostsByCategoryAsync(categoryFilter.Value);
        }
        else
        {
            posts = await _blogService.GetAllPostsAsync(showUnpublished);
        }

        var viewModel = new BlogListViewModel
        {
            Posts = posts,
            SearchTerm = searchTerm,
            CategoryFilter = categoryFilter,
            ShowUnpublished = showUnpublished
        };

        return View(viewModel);
    }

    // GET: Admin/Blog/Create
    /// <summary>
    /// create post (BlogPostViewModel). Anti-forgery required. Roles: Admin, Editor.
    /// </summary>
    [SwaggerOperation(Summary = "create post (BlogPostViewModel). Anti-forgery required. Roles: Admin, Editor.")]
    public async Task<IActionResult> Create()
    {
        SetTinyMceApiKey();
        
        var viewModel = new BlogPostViewModel
        {
            PublishedDate = DateTime.Now,
            AvailableCategories = await GetCategorySelectListAsync()
        };

        return View(viewModel);
    }

    // POST: Admin/Blog/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// create post (BlogPostViewModel). Anti-forgery required. Roles: Admin, Editor.
    /// </summary>
    [SwaggerOperation(Summary = "create post (BlogPostViewModel). Anti-forgery required. Roles: Admin, Editor.")]
    public async Task<IActionResult> Create(BlogPostViewModel model)
    {
        if (ModelState.IsValid)
        {
            var post = new BlogPost
            {
                Title = model.Title,
                Slug = string.IsNullOrWhiteSpace(model.Slug)
                    ? _blogService.GenerateSlug(model.Title)
                    : model.Slug,
                Content = model.Content,
                Excerpt = model.Excerpt,
                AuthorName = model.AuthorName,
                IsPublished = model.IsPublished,
                PublishedDate = model.IsPublished ? (model.PublishedDate ?? DateTime.UtcNow) : null,
                MetaDescription = model.MetaDescription,
                Tags = model.Tags
            };

            // Handle featured image - prioritize Media Library
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
            {
                post.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // Upload to centralized Media Manager
                var uploadedBy = User.Identity?.Name;
                var media = await _mediaService.UploadLocalImageAsync(
                    model.FeaturedImageFile,
                    $"Blog: {model.Title}",
                    model.Title,
                    MediaCategory.Blog,
                    model.Tags,
                    uploadedBy);
                
                post.FeaturedImageUrl = media.FilePath;
                
                // Track usage
                await _blogService.CreatePostAsync(post);
                await _mediaService.TrackMediaUsageAsync(media.Id, "BlogPost", post.Id, UsageType.Featured);

                // Handle categories
                if (model.SelectedCategoryIds.Any())
                {
                    foreach (var categoryId in model.SelectedCategoryIds)
                    {
                        post.BlogPostCategories.Add(new BlogPostCategory
                        {
                            BlogPostId = post.Id,
                            CategoryId = categoryId
                        });
                    }
                    await _blogService.UpdatePostAsync(post);
                }

                TempData["SuccessMessage"] = "Blog post created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await _blogService.CreatePostAsync(post);

            // Handle categories
            if (model.SelectedCategoryIds.Any())
            {
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    post.BlogPostCategories.Add(new BlogPostCategory
                    {
                        BlogPostId = post.Id,
                        CategoryId = categoryId
                    });
                }
                await _blogService.UpdatePostAsync(post);
            }

            TempData["SuccessMessage"] = "Blog post created successfully!";
            return RedirectToAction(nameof(Index));
        }

        SetTinyMceApiKey();
        model.AvailableCategories = await GetCategorySelectListAsync();
        return View(model);
    }

    // GET: Admin/Blog/Edit/5
    /// <summary>
    /// update post. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update post. Anti-forgery required.")]
    public async Task<IActionResult> Edit(int id)
    {
        SetTinyMceApiKey();
        
        var post = await _blogService.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        var viewModel = new BlogPostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Excerpt = post.Excerpt,
            FeaturedImageUrl = post.FeaturedImageUrl,
            AuthorName = post.AuthorName,
            IsPublished = post.IsPublished,
            PublishedDate = post.PublishedDate,
            MetaDescription = post.MetaDescription,
            Tags = post.Tags,
            SelectedCategoryIds = post.BlogPostCategories.Select(pc => pc.CategoryId).ToList(),
            AvailableCategories = await GetCategorySelectListAsync()
        };

        return View(viewModel);
    }

    // POST: Admin/Blog/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// update post. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "update post. Anti-forgery required.")]
    public async Task<IActionResult> Edit(int id, BlogPostViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var post = await _blogService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Title = model.Title;
            post.Slug = string.IsNullOrWhiteSpace(model.Slug)
                ? _blogService.GenerateSlug(model.Title)
                : model.Slug;
            post.Content = model.Content;
            post.Excerpt = model.Excerpt;
            post.AuthorName = model.AuthorName;
            post.IsPublished = model.IsPublished;
            post.PublishedDate = model.IsPublished ? (model.PublishedDate ?? DateTime.UtcNow) : null;
            post.MetaDescription = model.MetaDescription;
            post.Tags = model.Tags;

            // Handle featured image update
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl) && model.FeaturedImageUrl != post.FeaturedImageUrl)
            {
                // New image from Media Library - just update URL
                post.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // New file upload - use Media Manager
                var uploadedBy = User.Identity?.Name;
                var media = await _mediaService.UploadLocalImageAsync(
                    model.FeaturedImageFile,
                    $"Blog: {model.Title}",
                    model.Title,
                    MediaCategory.Blog,
                    model.Tags,
                    uploadedBy);
                
                post.FeaturedImageUrl = media.FilePath;
                
                // Track new usage
                await _mediaService.TrackMediaUsageAsync(media.Id, "BlogPost", post.Id, UsageType.Featured);
            }

            // Update categories
            post.BlogPostCategories.Clear();
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                post.BlogPostCategories.Add(new BlogPostCategory
                {
                    BlogPostId = post.Id,
                    CategoryId = categoryId
                });
            }

            await _blogService.UpdatePostAsync(post);

            TempData["SuccessMessage"] = "Blog post updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        SetTinyMceApiKey();
        model.AvailableCategories = await GetCategorySelectListAsync();
        return View(model);
    }

    // POST: Admin/Blog/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// delete post. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "delete post. Anti-forgery required.")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _blogService.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        // Remove media usage tracking
        if (!string.IsNullOrEmpty(post.FeaturedImageUrl))
        {
            // Find media by path
            var allMedia = await _mediaService.GetAllMediaAsync();
            var media = allMedia.FirstOrDefault(m => m.FilePath == post.FeaturedImageUrl);
            if (media != null)
            {
                await _mediaService.RemoveMediaUsageAsync(media.Id, "BlogPost", id);
            }
        }

        await _blogService.DeletePostAsync(id);

        TempData["SuccessMessage"] = "Blog post deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // Helper methods
    private void SetTinyMceApiKey()
    {
        ViewData["TinyMceApiKey"] = _configuration["TinyMCE:ApiKey"] ?? "no-api-key";
    }

    private async Task<List<SelectListItem>> GetCategorySelectListAsync()
    {
        var categories = await _blogService.GetAllCategoriesAsync();
        return categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();
    }
}
