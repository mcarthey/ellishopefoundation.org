using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EllisHope.Tests.Services;

public class BlogServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllPostsAsync_ReturnsOnlyPublishedPosts_WhenIncludeUnpublishedIsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.AddRange(
            new BlogPost { Id = 1, Title = "Published Post", Slug = "published-post", IsPublished = true, PublishedDate = DateTime.UtcNow, Summary = "" },
            new BlogPost { Id = 2, Title = "Draft Post", Slug = "draft-post", IsPublished = false, Summary = "" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPostsAsync(includeUnpublished: false);

        // Assert
        Assert.Single(result);
        Assert.Equal("Published Post", result.First().Title);
    }

    [Fact]
    public async Task GetAllPostsAsync_ReturnsAllPosts_WhenIncludeUnpublishedIsTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.AddRange(
            new BlogPost { Id = 1, Title = "Published Post", Slug = "published-post", IsPublished = true, PublishedDate = DateTime.UtcNow, Summary = "" },
            new BlogPost { Id = 2, Title = "Draft Post", Slug = "draft-post", IsPublished = false, Summary = "" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPostsAsync(includeUnpublished: true);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetPostByIdAsync_ReturnsCorrectPost()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var post = new BlogPost { Id = 1, Title = "Test Post", Slug = "test-post", Summary = "" };
        context.BlogPosts.Add(post);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPostByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Post", result.Title);
    }

    [Fact]
    public async Task GetPostByIdAsync_ReturnsNull_WhenPostNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        // Act
        var result = await service.GetPostByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ReturnsPublishedPost()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.Add(new BlogPost
        {
            Id = 1,
            Title = "Test Post",
            Slug = "test-post",
            IsPublished = true,
            Summary = ""
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPostBySlugAsync("test-post");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Post", result.Title);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ReturnsNull_WhenPostIsUnpublished()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.Add(new BlogPost
        {
            Id = 1,
            Title = "Draft Post",
            Slug = "draft-post",
            IsPublished = false,
            Summary = ""
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPostBySlugAsync("draft-post");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchPostsAsync_ReturnsMatchingPosts()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.AddRange(
            new BlogPost { Id = 1, Title = "ASP.NET Tutorial", Slug = "aspnet-tutorial", Content = "Learn ASP.NET", IsPublished = true, Summary = "Tutorial" },
            new BlogPost { Id = 2, Title = "C# Guide", Slug = "csharp-guide", Content = "Learn C#", IsPublished = true, Summary = "Guide" },
            new BlogPost { Id = 3, Title = "JavaScript Basics", Slug = "js-basics", Content = "Learn JavaScript", IsPublished = true, Summary = "Basics" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchPostsAsync("ASP.NET");

        // Assert
        Assert.Single(result);
        Assert.Equal("ASP.NET Tutorial", result.First().Title);
    }

    [Fact]
    public async Task SearchPostsAsync_ReturnsAllPublishedPosts_WhenSearchTermIsEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.AddRange(
            new BlogPost { Id = 1, Title = "Post 1", Slug = "post-1", IsPublished = true, Summary = "" },
            new BlogPost { Id = 2, Title = "Post 2", Slug = "post-2", IsPublished = false, Summary = "" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchPostsAsync("");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetPostsByCategoryAsync_ReturnsPostsInCategory()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var category = new BlogCategory { Id = 1, Name = "Tech", Slug = "tech" };
        var post1 = new BlogPost { Id = 1, Title = "Tech Post", Slug = "tech-post", IsPublished = true, Summary = "" };
        var post2 = new BlogPost { Id = 2, Title = "Other Post", Slug = "other-post", IsPublished = true, Summary = "" };

        context.BlogCategories.Add(category);
        context.BlogPosts.AddRange(post1, post2);
        context.BlogPostCategories.Add(new BlogPostCategory { BlogPostId = 1, CategoryId = 1 });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPostsByCategoryAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Tech Post", result.First().Title);
    }

    [Fact]
    public async Task CreatePostAsync_GeneratesSlugFromTitle()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var post = new BlogPost
        {
            Title = "My New Post",
            Slug = "",
            Summary = ""
        };

        // Act
        var result = await service.CreatePostAsync(post);

        // Assert
        Assert.NotEmpty(result.Slug);
        Assert.Equal("my-new-post", result.Slug);
    }

    [Fact]
    public async Task CreatePostAsync_EnsuresUniqueSlug()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.Add(new BlogPost
        {
            Title = "Test Post",
            Slug = "test-post",
            Summary = ""
        });
        await context.SaveChangesAsync();

        var newPost = new BlogPost
        {
            Title = "Test Post",
            Slug = "",
            Summary = ""
        };

        // Act
        var result = await service.CreatePostAsync(newPost);

        // Assert
        Assert.Equal("test-post-1", result.Slug);
    }

    [Fact]
    public async Task CreatePostAsync_SetsCreatedAndModifiedDates()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var post = new BlogPost
        {
            Title = "Test Post",
            Slug = "test-post",
            Summary = ""
        };

        // Act
        var result = await service.CreatePostAsync(post);

        // Assert
        Assert.NotEqual(default, result.CreatedDate);
        Assert.NotEqual(default, result.ModifiedDate);
    }

    [Fact]
    public async Task UpdatePostAsync_UpdatesModifiedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var post = new BlogPost
        {
            Title = "Test Post",
            Slug = "test-post",
            Summary = "",
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.BlogPosts.Add(post);
        await context.SaveChangesAsync();

        var oldUpdateDate = post.ModifiedDate;

        // Detach the entity to avoid tracking conflicts
        context.Entry(post).State = EntityState.Detached;

        // Act
        post.Title = "Updated Post";
        await Task.Delay(10); // Small delay to ensure time difference
        var result = await service.UpdatePostAsync(post);

        // Assert
        Assert.True(result.ModifiedDate > oldUpdateDate);
    }

    [Fact]
    public async Task DeletePostAsync_RemovesPost()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var post = new BlogPost { Title = "Test Post", Slug = "test-post", Summary = "" };
        context.BlogPosts.Add(post);
        await context.SaveChangesAsync();

        // Act
        await service.DeletePostAsync(post.Id);

        // Assert
        var deletedPost = await context.BlogPosts.FindAsync(post.Id);
        Assert.Null(deletedPost);
    }

    [Fact]
    public async Task DeletePostAsync_DoesNotThrow_WhenPostNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.DeletePostAsync(999));
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogCategories.AddRange(
            new BlogCategory { Name = "Tech", Slug = "tech" },
            new BlogCategory { Name = "News", Slug = "news" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateCategoryAsync_GeneratesSlug()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var category = new BlogCategory { Name = "Tech News", Slug = "" };

        // Act
        var result = await service.CreateCategoryAsync(category);

        // Assert
        Assert.Equal("tech-news", result.Slug);
    }

    [Fact]
    public async Task DeleteCategoryAsync_RemovesCategory()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        var category = new BlogCategory { Name = "Tech", Slug = "tech" };
        context.BlogCategories.Add(category);
        await context.SaveChangesAsync();

        // Act
        await service.DeleteCategoryAsync(category.Id);

        // Assert
        var deletedCategory = await context.BlogCategories.FindAsync(category.Id);
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task SlugExistsAsync_ReturnsTrue_WhenSlugExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.Add(new BlogPost { Title = "Test", Slug = "test-slug", Summary = "" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.SlugExistsAsync("test-slug");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SlugExistsAsync_ReturnsFalse_WhenSlugDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        // Act
        var result = await service.SlugExistsAsync("non-existent-slug");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SlugExistsAsync_ExcludesSpecifiedPost()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        context.BlogPosts.Add(new BlogPost { Id = 1, Title = "Test", Slug = "test-slug", Summary = "" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.SlugExistsAsync("test-slug", excludePostId: 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateSlug_CreatesValidSlug()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new BlogService(context);

        // Act
        var result = service.GenerateSlug("My Test Post!");

        // Assert
        Assert.Equal("my-test-post", result);
    }
}
