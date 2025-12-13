using EllisHope.Controllers;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class BlogControllerTests
{
    private readonly Mock<IBlogService> _mockBlogService;
    private readonly BlogController _controller;

    public BlogControllerTests()
    {
        _mockBlogService = new Mock<IBlogService>();
        _controller = new BlogController(_mockBlogService.Object);
    }

    #region Classic Action Tests

    [Fact]
    public async Task Classic_ReturnsViewWithAllPosts_WhenNoFilters()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost { Id = 1, Title = "Post 1", Slug = "post-1", IsPublished = true },
            new BlogPost { Id = 2, Title = "Post 2", Slug = "post-2", IsPublished = true }
        };
        var categories = new List<BlogCategory>
        {
            new BlogCategory { Id = 1, Name = "Tech", Slug = "tech" }
        };

        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(posts);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.classic(null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<BlogPost>>(viewResult.Model);
        Assert.Equal(2, model.Count());
        Assert.Equal(categories, _controller.ViewBag.Categories);
    }

    [Fact]
    public async Task Classic_ReturnsSearchResults_WhenSearchTermProvided()
    {
        // Arrange
        var searchTerm = "ASP.NET";
        var posts = new List<BlogPost>
        {
            new BlogPost { Id = 1, Title = "ASP.NET Tutorial", Slug = "aspnet-tutorial", IsPublished = true }
        };

        _mockBlogService.Setup(s => s.SearchPostsAsync(searchTerm)).ReturnsAsync(posts);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());

        // Act
        var result = await _controller.classic(searchTerm, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<BlogPost>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal(searchTerm, _controller.ViewBag.SearchTerm);
    }

    [Fact]
    public async Task Classic_ReturnsCategoryPosts_WhenCategoryProvided()
    {
        // Arrange
        var categoryId = 1;
        var posts = new List<BlogPost>
        {
            new BlogPost { Id = 1, Title = "Tech Post", Slug = "tech-post", IsPublished = true }
        };

        _mockBlogService.Setup(s => s.GetPostsByCategoryAsync(categoryId)).ReturnsAsync(posts);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());

        // Act
        var result = await _controller.classic(null, categoryId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<BlogPost>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal(categoryId, _controller.ViewBag.SelectedCategory);
    }

    [Fact]
    public async Task Classic_SetsViewBagCorrectly()
    {
        // Arrange
        var searchTerm = "test";
        var categoryId = 1;
        var categories = new List<BlogCategory>
        {
            new BlogCategory { Id = 1, Name = "Tech", Slug = "tech" }
        };

        _mockBlogService.Setup(s => s.SearchPostsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BlogPost>());
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        await _controller.classic(searchTerm, categoryId);

        // Assert
        Assert.Equal(searchTerm, _controller.ViewBag.SearchTerm);
        Assert.Equal(categoryId, _controller.ViewBag.SelectedCategory);
        Assert.Equal(categories, _controller.ViewBag.Categories);
    }

    #endregion

    #region Details Action Tests

    [Fact]
    public async Task Details_ReturnsNotFound_WhenSlugIsNull()
    {
        // Act
        var result = await _controller.details(null!);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenSlugIsEmpty()
    {
        // Act
        var result = await _controller.details("");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var slug = "non-existent-post";
        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync((BlogPost?)null);

        // Act
        var result = await _controller.details(slug);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithPost_WhenPostExists()
    {
        // Arrange
        var slug = "test-post";
        var post = new BlogPost
        {
            Id = 1,
            Title = "Test Post",
            Slug = slug,
            IsPublished = true,
            Content = "Test content"
        };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(new List<BlogPost> { post });

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogPost>(viewResult.Model);
        Assert.Equal(post.Id, model.Id);
        Assert.Equal(post.Title, model.Title);
        Assert.Equal(post.Slug, model.Slug);
    }

    [Fact]
    public async Task Details_PopulatesViewBagWithCategories()
    {
        // Arrange
        var slug = "test-post";
        var post = new BlogPost { Id = 1, Title = "Test", Slug = slug, IsPublished = true };
        var categories = new List<BlogCategory>
        {
            new BlogCategory { Id = 1, Name = "Tech", Slug = "tech" },
            new BlogCategory { Id = 2, Name = "News", Slug = "news" }
        };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(new List<BlogPost> { post });

        // Act
        await _controller.details(slug);

        // Assert
        Assert.Equal(categories, _controller.ViewBag.Categories);
    }

    [Fact]
    public async Task Details_PopulatesViewBagWithRecentPosts()
    {
        // Arrange
        var slug = "test-post";
        var currentPost = new BlogPost
        {
            Id = 1,
            Title = "Current Post",
            Slug = slug,
            IsPublished = true,
            PublishedDate = DateTime.UtcNow
        };
        var allPosts = new List<BlogPost>
        {
            currentPost,
            new BlogPost { Id = 2, Title = "Recent 1", Slug = "recent-1", IsPublished = true, PublishedDate = DateTime.UtcNow.AddDays(-1) },
            new BlogPost { Id = 3, Title = "Recent 2", Slug = "recent-2", IsPublished = true, PublishedDate = DateTime.UtcNow.AddDays(-2) },
            new BlogPost { Id = 4, Title = "Recent 3", Slug = "recent-3", IsPublished = true, PublishedDate = DateTime.UtcNow.AddDays(-3) },
            new BlogPost { Id = 5, Title = "Recent 4", Slug = "recent-4", IsPublished = true, PublishedDate = DateTime.UtcNow.AddDays(-4) }
        };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(currentPost);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(allPosts);

        // Act
        await _controller.details(slug);

        // Assert
        var recentPosts = _controller.ViewBag.RecentPosts as IEnumerable<BlogPost>;
        Assert.NotNull(recentPosts);
        Assert.Equal(3, recentPosts.Count());
        Assert.DoesNotContain(recentPosts, p => p.Id == currentPost.Id); // Current post should be excluded
    }

    [Fact]
    public async Task Details_PopulatesViewBagWithTags()
    {
        // Arrange
        var slug = "test-post";
        var post = new BlogPost { Id = 1, Title = "Test", Slug = slug, IsPublished = true, Tags = "tag1,tag2" };
        var allPosts = new List<BlogPost>
        {
            post,
            new BlogPost { Id = 2, Title = "Post 2", Slug = "post-2", IsPublished = true, Tags = "tag2,tag3,tag4" },
            new BlogPost { Id = 3, Title = "Post 3", Slug = "post-3", IsPublished = true, Tags = "tag5" }
        };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(allPosts);

        // Act
        await _controller.details(slug);

        // Assert
        var tags = _controller.ViewBag.AllTags as IEnumerable<string>;
        Assert.NotNull(tags);
        Assert.True(tags.Count() <= 10); // Should limit to 10 tags
        Assert.Contains("tag1", tags);
        Assert.Contains("tag2", tags);
    }

    [Fact]
    public async Task Details_CallsGetPostBySlugAsync_WithCorrectSlug()
    {
        // Arrange
        var slug = "my-test-post";
        var post = new BlogPost { Id = 1, Title = "Test", Slug = slug, IsPublished = true };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(new List<BlogPost> { post });

        // Act
        await _controller.details(slug);

        // Assert
        _mockBlogService.Verify(s => s.GetPostBySlugAsync(slug), Times.Once);
    }

    [Fact]
    public async Task Details_HandlesPostWithNoTags()
    {
        // Arrange
        var slug = "test-post";
        var post = new BlogPost { Id = 1, Title = "Test", Slug = slug, IsPublished = true, Tags = null };
        var allPosts = new List<BlogPost> { post };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(allPosts);

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    [Fact]
    public async Task Details_HandlesEmptyRecentPosts()
    {
        // Arrange
        var slug = "only-post";
        var post = new BlogPost { Id = 1, Title = "Only Post", Slug = slug, IsPublished = true };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<BlogCategory>());
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(new List<BlogPost> { post });

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var recentPosts = _controller.ViewBag.RecentPosts as IEnumerable<BlogPost>;
        Assert.NotNull(recentPosts);
        Assert.Empty(recentPosts);
    }

    #endregion

    #region Grid Action Tests

    [Fact]
    public void Grid_ReturnsView()
    {
        // Act
        var result = _controller.grid();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region Integration-like Tests

    [Fact]
    public async Task Details_IntegrationTest_CompleteWorkflow()
    {
        // Arrange
        var slug = "complete-test-post";
        var currentPost = new BlogPost
        {
            Id = 1,
            Title = "Complete Test Post",
            Slug = slug,
            Content = "<p>This is a complete test</p>",
            Excerpt = "Test excerpt",
            AuthorName = "Test Author",
            IsPublished = true,
            PublishedDate = DateTime.UtcNow,
            Tags = "test,integration,complete"
        };

        var otherPosts = new List<BlogPost>
        {
            new BlogPost { Id = 2, Title = "Other 1", Slug = "other-1", IsPublished = true, PublishedDate = DateTime.UtcNow.AddDays(-1), Tags = "test" },
            new BlogPost { Id = 3, Title = "Other 2", Slug = "other-2", IsPublished = true, PublishedDate = DateTime.UtcNow.AddDays(-2) },
            new BlogPost { Id = 4, Title = "Other 3", Slug = "other-3", IsPublished = true, PublishedDate = DateTime.UtcNow.AddDays(-3) }
        };

        var allPosts = new List<BlogPost> { currentPost }.Concat(otherPosts).ToList();

        var categories = new List<BlogCategory>
        {
            new BlogCategory { Id = 1, Name = "Test Category", Slug = "test-category" }
        };

        _mockBlogService.Setup(s => s.GetPostBySlugAsync(slug)).ReturnsAsync(currentPost);
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(allPosts);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogPost>(viewResult.Model);
        
        // Verify model
        Assert.Equal(currentPost.Id, model.Id);
        Assert.Equal(currentPost.Title, model.Title);
        Assert.Equal(currentPost.Content, model.Content);
        
        // Verify ViewBag
        Assert.Equal(categories, _controller.ViewBag.Categories);
        
        var recentPosts = _controller.ViewBag.RecentPosts as IEnumerable<BlogPost>;
        Assert.NotNull(recentPosts);
        Assert.Equal(3, recentPosts.Count());
        Assert.DoesNotContain(recentPosts, p => p.Id == currentPost.Id);
        
        var tags = _controller.ViewBag.AllTags as IEnumerable<string>;
        Assert.NotNull(tags);
        Assert.Contains("test", tags);
        Assert.Contains("integration", tags);
        Assert.Contains("complete", tags);
    }

    #endregion
}
