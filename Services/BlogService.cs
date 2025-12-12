using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace EllisHope.Services;

public class BlogService : IBlogService
{
    private readonly ApplicationDbContext _context;
    private readonly SlugHelper _slugHelper;

    public BlogService(ApplicationDbContext context)
    {
        _context = context;
        _slugHelper = new SlugHelper();
    }

    public async Task<IEnumerable<BlogPost>> GetAllPostsAsync(bool includeUnpublished = false)
    {
        var query = _context.BlogPosts
            .Include(p => p.BlogPostCategories)
            .ThenInclude(pc => pc.Category)
            .OrderByDescending(p => p.PublishedDate);

        if (!includeUnpublished)
        {
            return await query.Where(p => p.IsPublished).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<BlogPost?> GetPostByIdAsync(int id)
    {
        return await _context.BlogPosts
            .Include(p => p.BlogPostCategories)
            .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<BlogPost?> GetPostBySlugAsync(string slug)
    {
        return await _context.BlogPosts
            .Include(p => p.BlogPostCategories)
            .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
    }

    public async Task<IEnumerable<BlogPost>> SearchPostsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllPostsAsync();
        }

        return await _context.BlogPosts
            .Include(p => p.BlogPostCategories)
            .ThenInclude(pc => pc.Category)
            .Where(p => p.IsPublished &&
                   (p.Title.Contains(searchTerm) ||
                    p.Content.Contains(searchTerm) ||
                    (p.Summary != null && p.Summary.Contains(searchTerm))))
            .OrderByDescending(p => p.PublishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BlogPost>> GetPostsByCategoryAsync(int categoryId)
    {
        return await _context.BlogPosts
            .Include(p => p.BlogPostCategories)
            .ThenInclude(pc => pc.Category)
            .Where(p => p.IsPublished &&
                   p.BlogPostCategories.Any(pc => pc.CategoryId == categoryId))
            .OrderByDescending(p => p.PublishedDate)
            .ToListAsync();
    }

    public async Task<BlogPost> CreatePostAsync(BlogPost post)
    {
        // Ensure slug is unique
        if (string.IsNullOrWhiteSpace(post.Slug))
        {
            post.Slug = GenerateSlug(post.Title);
        }

        post.Slug = await EnsureUniqueSlugAsync(post.Slug);
        post.CreatedDate = DateTime.UtcNow;
        post.ModifiedDate = DateTime.UtcNow;

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<BlogPost> UpdatePostAsync(BlogPost post)
    {
        post.ModifiedDate = DateTime.UtcNow;
        _context.BlogPosts.Update(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task DeletePostAsync(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post != null)
        {
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync()
    {
        return await _context.BlogCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<BlogCategory> CreateCategoryAsync(BlogCategory category)
    {
        category.Slug = GenerateSlug(category.Name);
        _context.BlogCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.BlogCategories.FindAsync(id);
        if (category != null)
        {
            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludePostId = null)
    {
        var query = _context.BlogPosts.Where(p => p.Slug == slug);

        if (excludePostId.HasValue)
        {
            query = query.Where(p => p.Id != excludePostId.Value);
        }

        return await query.AnyAsync();
    }

    public string GenerateSlug(string title)
    {
        return _slugHelper.GenerateSlug(title);
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug)
    {
        var slug = baseSlug;
        var counter = 1;

        while (await SlugExistsAsync(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
