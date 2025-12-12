using EllisHope.Models.Domain;

namespace EllisHope.Services;

public interface IBlogService
{
    Task<IEnumerable<BlogPost>> GetAllPostsAsync(bool includeUnpublished = false);
    Task<BlogPost?> GetPostByIdAsync(int id);
    Task<BlogPost?> GetPostBySlugAsync(string slug);
    Task<IEnumerable<BlogPost>> SearchPostsAsync(string searchTerm);
    Task<IEnumerable<BlogPost>> GetPostsByCategoryAsync(int categoryId);
    Task<BlogPost> CreatePostAsync(BlogPost post);
    Task<BlogPost> UpdatePostAsync(BlogPost post);
    Task DeletePostAsync(int id);
    Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync();
    Task<BlogCategory> CreateCategoryAsync(BlogCategory category);
    Task DeleteCategoryAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludePostId = null);
    string GenerateSlug(string title);
}
