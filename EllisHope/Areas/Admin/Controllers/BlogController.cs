using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
public class BlogController : Controller
{
    private readonly IBlogService _blogService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public BlogController(IBlogService blogService, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _blogService = blogService;
        _environment = environment;
        _configuration = configuration;
    }

    // GET: Admin/Blog
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

            // Handle featured image - prioritize FeaturedImageUrl (from Media Library) over file upload
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl))
            {
                // Image selected from Media Library
                post.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // Legacy: File uploaded directly (bypassing Media Library)
                post.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile);
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

            // Handle featured image - prioritize FeaturedImageUrl (from Media Library) over file upload
            if (!string.IsNullOrWhiteSpace(model.FeaturedImageUrl) && model.FeaturedImageUrl != post.FeaturedImageUrl)
            {
                // New image selected from Media Library
                // Only delete old if it was from local uploads, not Media Library
                if (!string.IsNullOrEmpty(post.FeaturedImageUrl) && 
                    (post.FeaturedImageUrl.Contains("/uploads/causes/") || 
                     post.FeaturedImageUrl.Contains("/uploads/blog/") || 
                     post.FeaturedImageUrl.Contains("/uploads/events/")))
                {
                    DeleteFeaturedImage(post.FeaturedImageUrl);
                }
                post.FeaturedImageUrl = model.FeaturedImageUrl;
            }
            else if (model.FeaturedImageFile != null)
            {
                // Legacy: New file uploaded directly (bypassing Media Library)
                // Delete old image if exists and was from local uploads
                if (!string.IsNullOrEmpty(post.FeaturedImageUrl) && 
                    (post.FeaturedImageUrl.Contains("/uploads/causes/") || 
                     post.FeaturedImageUrl.Contains("/uploads/blog/") || 
                     post.FeaturedImageUrl.Contains("/uploads/events/")))
                {
                    DeleteFeaturedImage(post.FeaturedImageUrl);
                }
                post.FeaturedImageUrl = await SaveFeaturedImageAsync(model.FeaturedImageFile);
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
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _blogService.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        // Delete featured image if exists
        if (!string.IsNullOrEmpty(post.FeaturedImageUrl))
        {
            DeleteFeaturedImage(post.FeaturedImageUrl);
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

    private async Task<string> SaveFeaturedImageAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "blog");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/blog/{uniqueFileName}";
    }

    private void DeleteFeaturedImage(string imageUrl)
    {
        var imagePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }
    }
}
