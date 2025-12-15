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
    private readonly Mock<IPageTemplateService> _mockTemplateService;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<PagesController>> _mockLogger;
    private readonly PagesController _controller;

    public PagesControllerTests()
    {
        _mockPageService = new Mock<IPageService>();
        _mockTemplateService = new Mock<IPageTemplateService>();
        _mockMediaService = new Mock<IMediaService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PagesController>>();

        _controller = new PagesController(
            _mockPageService.Object,
            _mockTemplateService.Object,
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
    public async Task Edit_ReturnsViewWithTemplate_WhenPageExists()
    {
        // Arrange
        var page = new Page
        {
            Id = 1,
            PageName = "Home",
            Title = "Home Page",
            ContentSections = new List<ContentSection>(),
            PageImages = new List<PageImage>()
        };

        var template = new PageTemplate
        {
            PageName = "Home",
            DisplayName = "Home Page",
            Images = new List<EditableImage>(),
            ContentAreas = new List<EditableContent>()
        };

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockTemplateService.Setup(s => s.GetPageTemplate("Home"))
            .Returns(template);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageTemplate>(viewResult.Model);
        Assert.Equal("Home", model.PageName);
    }

    #endregion

    #region UpdateContent Action Tests

    [Fact]
    public async Task UpdateContent_RedirectsToEdit_WithSuccessMessage()
    {
        // Arrange
        _mockPageService.Setup(s => s.UpdateContentSectionAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateContent(1, "WelcomeText", "Hello", "RichText");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirectResult.ActionName);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region UpdateImage Action Tests

    [Fact]
    public async Task UpdateImage_RedirectsToEdit_WithSuccessMessage()
    {
        // Arrange
        _mockPageService.Setup(s => s.SetPageImageAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateImage(1, "HeroImage", 1);

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
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
