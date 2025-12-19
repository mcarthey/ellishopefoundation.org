using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EllisHope.Tests.Controllers.Admin;

public class AdminBlogControllerTests
{
    private readonly Mock<IBlogService> _mockBlogService;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly BlogController _controller;

    public AdminBlogControllerTests()
    {
        _mockBlogService = new Mock<IBlogService>();
        _mockMediaService = new Mock<IMediaService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(c => c["TinyMCE:ApiKey"]).Returns("test-api-key");

        _controller = new BlogController(
            _mockBlogService.Object,
            _mockMediaService.Object,
            _mockEnvironment.Object,
            _mockConfiguration.Object
        );

        // Setup TempData
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        // Setup User identity
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    #region Index Action Tests

    [Fact]
    public async Task Index_ReturnsAllPosts_WhenNoFilters()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost { Id = 1, Title = "Post 1", Slug = "post-1", Summary = "" },
            new BlogPost { Id = 2, Title = "Post 2", Slug = "post-2", Summary = "" }
        };
        _mockBlogService.Setup(s => s.GetAllPostsAsync(true)).ReturnsAsync(posts);

        // Act
        var result = await _controller.Index(null, null, true);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogListViewModel>(viewResult.Model);
        Assert.Equal(2, model.Posts.Count());
    }

    [Fact]
    public async Task Index_ReturnsSearchResults_WhenSearchTermProvided()
    {
        // Arrange
        var searchTerm = "test";
        var posts = new List<BlogPost>
        {
            new BlogPost { Id = 1, Title = "Test Post", Slug = "test-post", Summary = "" }
        };
        _mockBlogService.Setup(s => s.SearchPostsAsync(searchTerm)).ReturnsAsync(posts);

        // Act
        var result = await _controller.Index(searchTerm, null, true);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogListViewModel>(viewResult.Model);
        Assert.Single(model.Posts);
        Assert.Equal(searchTerm, model.SearchTerm);
    }

    [Fact]
    public async Task Index_ReturnsFilteredPosts_WhenCategoryFilterProvided()
    {
        // Arrange
        var categoryId = 5;
        var posts = new List<BlogPost>
        {
            new BlogPost { Id = 1, Title = "Tech Post", Slug = "tech-post", Summary = "" }
        };
        _mockBlogService.Setup(s => s.GetPostsByCategoryAsync(categoryId)).ReturnsAsync(posts);

        // Act
        var result = await _controller.Index(null, categoryId, true);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogListViewModel>(viewResult.Model);
        Assert.Single(model.Posts);
        Assert.Equal(categoryId, model.CategoryFilter);
    }

    [Fact]
    public async Task Index_SetsShowUnpublished_InViewModel()
    {
        // Arrange
        _mockBlogService.Setup(s => s.GetAllPostsAsync(false)).ReturnsAsync(new List<BlogPost>());

        // Act
        var result = await _controller.Index(null, null, false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogListViewModel>(viewResult.Model);
        Assert.False(model.ShowUnpublished);
    }

    #endregion

    #region Create Action Tests

    [Fact]
    public async Task Create_GET_ReturnsViewWithViewModel()
    {
        // Arrange
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(new List<BlogCategory>());

        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogPostViewModel>(viewResult.Model);
        Assert.NotNull(model.AvailableCategories);
    }

    [Fact]
    public async Task Create_GET_SetsTinyMceApiKey()
    {
        // Arrange
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(new List<BlogCategory>());

        // Act
        await _controller.Create();

        // Assert
        Assert.Equal("test-api-key", _controller.ViewData["TinyMceApiKey"]);
    }

    [Fact]
    public async Task Create_POST_CreatesPost_WithValidModel()
    {
        // Arrange
        var model = new BlogPostViewModel
        {
            Title = "New Post",
            Content = "Content",
            AuthorName = "Author",
            IsPublished = true,
            SelectedCategoryIds = new List<int>()
        };

        var createdPost = new BlogPost { Id = 1, Title = model.Title, Slug = "new-post", Summary = "" };
        _mockBlogService.Setup(s => s.GenerateSlug(model.Title)).Returns("new-post");
        _mockBlogService.Setup(s => s.CreatePostAsync(It.IsAny<BlogPost>())).ReturnsAsync(createdPost);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockBlogService.Verify(s => s.CreatePostAsync(It.IsAny<BlogPost>()), Times.Once);
    }

    [Fact]
    public async Task Create_POST_GeneratesSlug_WhenNotProvided()
    {
        // Arrange
        var model = new BlogPostViewModel
        {
            Title = "New Post",
            Slug = "",
            Content = "Content",
            AuthorName = "Author",
            SelectedCategoryIds = new List<int>()
        };

        _mockBlogService.Setup(s => s.GenerateSlug(model.Title)).Returns("new-post");
        _mockBlogService.Setup(s => s.CreatePostAsync(It.IsAny<BlogPost>()))
            .ReturnsAsync(new BlogPost { Id = 1, Slug = "new-post", Summary = "" });

        // Act
        await _controller.Create(model);

        // Assert
        _mockBlogService.Verify(s => s.GenerateSlug(model.Title), Times.Once);
    }

    [Fact]
    public async Task Create_POST_ReturnsViewWithModel_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Required");
        var model = new BlogPostViewModel();
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(new List<BlogCategory>());

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        _mockBlogService.Verify(s => s.CreatePostAsync(It.IsAny<BlogPost>()), Times.Never);
    }

    [Fact]
    public async Task Create_POST_SetsTempDataSuccess_OnSuccess()
    {
        // Arrange
        var model = new BlogPostViewModel
        {
            Title = "Test",
            Content = "Content",
            AuthorName = "Author",
            SelectedCategoryIds = new List<int>()
        };

        _mockBlogService.Setup(s => s.GenerateSlug(It.IsAny<string>())).Returns("test");
        _mockBlogService.Setup(s => s.CreatePostAsync(It.IsAny<BlogPost>()))
            .ReturnsAsync(new BlogPost { Id = 1, Summary = "" });

        // Act
        await _controller.Create(model);

        // Assert
        Assert.Contains("successfully", _controller.TempData["SuccessMessage"]?.ToString());
    }

    #endregion

    #region Edit Action Tests

    [Fact]
    public async Task Edit_GET_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        _mockBlogService.Setup(s => s.GetPostByIdAsync(999)).ReturnsAsync((BlogPost?)null);

        // Act
        var result = await _controller.Edit(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_GET_ReturnsViewWithPost_WhenPostExists()
    {
        // Arrange
        var post = new BlogPost
        {
            Id = 1,
            Title = "Test Post",
            Slug = "test-post",
            Content = "Content",
            Summary = "",
            BlogPostCategories = new List<BlogPostCategory>()
        };

        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(new List<BlogCategory>());

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogPostViewModel>(viewResult.Model);
        Assert.Equal(post.Title, model.Title);
        Assert.Equal(post.Id, model.Id);
    }

    [Fact]
    public async Task Edit_POST_ReturnsNotFound_WhenIdMismatch()
    {
        // Arrange
        var model = new BlogPostViewModel { Id = 1 };

        // Act
        var result = await _controller.Edit(2, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var model = new BlogPostViewModel { Id = 1, Title = "Test" };
        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync((BlogPost?)null);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_UpdatesPost_WithValidModel()
    {
        // Arrange
        var existingPost = new BlogPost
        {
            Id = 1,
            Title = "Old Title",
            Slug = "old-title",
            Summary = "",
            BlogPostCategories = new List<BlogPostCategory>()
        };

        var model = new BlogPostViewModel
        {
            Id = 1,
            Title = "Updated Title",
            Content = "Updated Content",
            AuthorName = "Author",
            IsPublished = true,
            SelectedCategoryIds = new List<int> { 1, 2 }
        };

        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(existingPost);
        _mockBlogService.Setup(s => s.GenerateSlug(model.Title)).Returns("updated-title");
        _mockBlogService.Setup(s => s.UpdatePostAsync(It.IsAny<BlogPost>())).ReturnsAsync(existingPost);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockBlogService.Verify(s => s.UpdatePostAsync(It.IsAny<BlogPost>()), Times.Once);
    }

    [Fact]
    public async Task Edit_POST_UpdatesCategories()
    {
        // Arrange
        var post = new BlogPost
        {
            Id = 1,
            Title = "Test",
            Slug = "test",
            Summary = "",
            BlogPostCategories = new List<BlogPostCategory>
            {
                new BlogPostCategory { BlogPostId = 1, CategoryId = 1 }
            }
        };

        var model = new BlogPostViewModel
        {
            Id = 1,
            Title = "Test",
            Content = "Content",
            AuthorName = "Author",
            SelectedCategoryIds = new List<int> { 2, 3 }
        };

        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GenerateSlug(It.IsAny<string>())).Returns("test");
        _mockBlogService.Setup(s => s.UpdatePostAsync(It.IsAny<BlogPost>())).ReturnsAsync(post);

        // Act
        await _controller.Edit(1, model);

        // Assert
        _mockBlogService.Verify(s => s.UpdatePostAsync(It.Is<BlogPost>(p =>
            p.BlogPostCategories.Any(pc => pc.CategoryId == 2) &&
            p.BlogPostCategories.Any(pc => pc.CategoryId == 3)
        )), Times.Once);
    }

    #endregion

    #region Delete Action Tests

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        _mockBlogService.Setup(s => s.GetPostByIdAsync(999)).ReturnsAsync((BlogPost?)null);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_DeletesPost_WhenPostExists()
    {
        // Arrange
        var post = new BlogPost
        {
            Id = 1,
            Title = "Test",
            Slug = "test",
            Summary = "",
            FeaturedImageUrl = null
        };

        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.DeletePostAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockBlogService.Verify(s => s.DeletePostAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_RemovesMediaUsage_WhenFeaturedImageExists()
    {
        // Arrange
        var post = new BlogPost
        {
            Id = 1,
            Title = "Test",
            Slug = "test",
            Summary = "",
            FeaturedImageUrl = "/uploads/test.jpg"
        };

        var media = new Media { Id = 10, FilePath = "/uploads/test.jpg" };
        var allMedia = new List<Media> { media };

        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);
        _mockMediaService.Setup(s => s.GetAllMediaAsync()).ReturnsAsync(allMedia);
        _mockMediaService.Setup(s => s.RemoveMediaUsageAsync(10, "BlogPost", 1)).Returns(Task.CompletedTask);
        _mockBlogService.Setup(s => s.DeletePostAsync(1)).Returns(Task.CompletedTask);

        // Act
        await _controller.Delete(1);

        // Assert
        _mockMediaService.Verify(s => s.RemoveMediaUsageAsync(10, "BlogPost", 1), Times.Once);
    }

    [Fact]
    public async Task Delete_SetsTempDataSuccess_OnSuccess()
    {
        // Arrange
        var post = new BlogPost { Id = 1, Title = "Test", Slug = "test", Summary = "" };
        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.DeletePostAsync(1)).Returns(Task.CompletedTask);

        // Act
        await _controller.Delete(1);

        // Assert
        Assert.Contains("deleted", _controller.TempData["SuccessMessage"]?.ToString());
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public async Task Create_LoadsCategories_ForSelectList()
    {
        // Arrange
        var categories = new List<BlogCategory>
        {
            new BlogCategory { Id = 1, Name = "Tech", Slug = "tech" },
            new BlogCategory { Id = 2, Name = "News", Slug = "news" }
        };

        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogPostViewModel>(viewResult.Model);
        Assert.Equal(2, model.AvailableCategories.Count);
    }

    [Fact]
    public async Task Edit_LoadsCategories_ForSelectList()
    {
        // Arrange
        var post = new BlogPost
        {
            Id = 1,
            Title = "Test",
            Slug = "test",
            Summary = "",
            BlogPostCategories = new List<BlogPostCategory>()
        };

        var categories = new List<BlogCategory>
        {
            new BlogCategory { Id = 1, Name = "Tech", Slug = "tech" }
        };

        _mockBlogService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);
        _mockBlogService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogPostViewModel>(viewResult.Model);
        Assert.Single(model.AvailableCategories);
    }

    #endregion
}
