using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class PagesControllerTests
{
    private readonly Mock<IPageService> _mockPageService;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<PagesController>> _mockLogger;
    private readonly PagesController _controller;

    public PagesControllerTests()
    {
        _mockPageService = new Mock<IPageService>();
        _mockMediaService = new Mock<IMediaService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PagesController>>();

        _controller = new PagesController(
            _mockPageService.Object,
            _mockMediaService.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Setup TempData
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    #region Index Action Tests

    [Fact]
    public async Task Index_ReturnsViewWithAllPages()
    {
        // Arrange
        var pages = new List<Page>
        {
            new Page { Id = 1, PageName = "Home", Title = "Home Page" },
            new Page { Id = 2, PageName = "About", Title = "About Us" }
        };

        _mockPageService.Setup(s => s.GetAllPagesAsync())
            .ReturnsAsync(pages);

        // Act
        var result = await _controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageListViewModel>(viewResult.Model);
        Assert.Equal(2, model.Pages.Count());
    }

    [Fact]
    public async Task Index_FiltersPagesBySearchTerm()
    {
        // Arrange
        var pages = new List<Page>
        {
            new Page { Id = 1, PageName = "Home", Title = "Home Page" },
            new Page { Id = 2, PageName = "About", Title = "About Us" },
            new Page { Id = 3, PageName = "Contact", Title = "Contact" }
        };

        _mockPageService.Setup(s => s.GetAllPagesAsync())
            .ReturnsAsync(pages);

        // Act
        var result = await _controller.Index("About");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageListViewModel>(viewResult.Model);
        Assert.Single(model.Pages);
        Assert.Equal("About", model.Pages.First().PageName);
    }

    [Fact]
    public async Task Index_SearchIsCaseInsensitive()
    {
        // Arrange
        var pages = new List<Page>
        {
            new Page { Id = 1, PageName = "Home", Title = "Welcome Home" }
        };

        _mockPageService.Setup(s => s.GetAllPagesAsync())
            .ReturnsAsync(pages);

        // Act
        var result = await _controller.Index("welcome");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageListViewModel>(viewResult.Model);
        Assert.Single(model.Pages);
    }

    #endregion

    #region Edit Action Tests

    [Fact]
    public async Task Edit_ReturnsNotFound_WhenPageDoesNotExist()
    {
        // Arrange
        _mockPageService.Setup(s => s.GetPageByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Page?)null);

        // Act
        var result = await _controller.Edit(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_ReturnsViewWithModel_WhenPageExists()
    {
        // Arrange
        var page = new Page
        {
            Id = 1,
            PageName = "Home",
            Title = "Home Page",
            MetaDescription = "Home meta",
            IsPublished = true,
            ContentSections = new List<ContentSection>(),
            PageImages = new List<PageImage>()
        };

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());
        _mockConfiguration.Setup(c => c["TinyMCE:ApiKey"])
            .Returns("test-key");

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageEditViewModel>(viewResult.Model);
        Assert.Equal(1, model.Id);
        Assert.Equal("Home", model.PageName);
        Assert.Equal("Home Page", model.Title);
    }

    #endregion

    #region UpdateSection Action Tests

    [Fact]
    public async Task UpdateSection_RedirectsToEdit_WithSuccessMessage()
    {
        // Arrange
        var model = new QuickEditSectionViewModel
        {
            PageId = 1,
            SectionKey = "WelcomeText",
            Content = "Hello World",
            ContentType = "RichText"
        };

        _mockPageService.Setup(s => s.UpdateContentSectionAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateSection(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues!["id"]);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateSection_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var model = new QuickEditSectionViewModel
        {
            PageId = 1,
            SectionKey = "WelcomeText",
            Content = "Hello World",
            ContentType = "RichText"
        };

        // Act
        await _controller.UpdateSection(model);

        // Assert
        _mockPageService.Verify(s => s.UpdateContentSectionAsync(
            1,
            "WelcomeText",
            "Hello World",
            "RichText"), Times.Once);
    }

    #endregion

    #region UpdateImage Action Tests

    [Fact]
    public async Task UpdateImage_RedirectsToEdit_WithSuccessMessage()
    {
        // Arrange
        var model = new QuickEditImageViewModel
        {
            PageId = 1,
            ImageKey = "HeroImage",
            MediaId = 1,
            DisplayOrder = 0
        };

        _mockPageService.Setup(s => s.SetPageImageAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateImage(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirectResult.ActionName);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region RemoveImage Action Tests

    [Fact]
    public async Task RemoveImage_RedirectsToEdit_WithSuccessMessage()
    {
        // Arrange
        _mockPageService.Setup(s => s.RemovePageImageAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveImage(1, "HeroImage");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues!["id"]);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RemoveImage_CallsServiceWithCorrectParameters()
    {
        // Arrange & Act
        await _controller.RemoveImage(1, "HeroImage");

        // Assert
        _mockPageService.Verify(s => s.RemovePageImageAsync(1, "HeroImage"), Times.Once);
    }

    #endregion

    #region MediaPicker Action Tests

    [Fact]
    public async Task MediaPicker_ReturnsView_WithModel()
    {
        // Arrange
        var page = new Page { Id = 1, PageName = "Home", Title = "Home" };
        var mediaList = new List<Media>();

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockPageService.Setup(s => s.GetPageImageAsync(1, "HeroImage"))
            .ReturnsAsync((PageImage?)null);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(mediaList);

        // Act
        var result = await _controller.MediaPicker(1, "HeroImage");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuickEditImageViewModel>(viewResult.Model);
        Assert.Equal(1, model.PageId);
        Assert.Equal("HeroImage", model.ImageKey);
    }

    #endregion
}
