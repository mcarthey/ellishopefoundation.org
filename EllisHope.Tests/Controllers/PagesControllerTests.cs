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

    [Fact]
    public async Task UpdateSection_ReturnsToEdit_WithError_WhenModelInvalid()
    {
        // Arrange
        var model = new QuickEditSectionViewModel { PageId = 1 };
        _controller.ModelState.AddModelError("SectionKey", "Required");

        // Act
        var result = await _controller.UpdateSection(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Invalid section data", _controller.TempData["ErrorMessage"]);
        _mockPageService.Verify(s => s.UpdateContentSectionAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSection_HandlesException_ReturnsErrorMessage()
    {
        // Arrange
        var model = new QuickEditSectionViewModel
        {
            PageId = 1,
            SectionKey = "WelcomeText",
            Content = "Content",
            ContentType = "Text"
        };

        _mockPageService.Setup(s => s.UpdateContentSectionAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateSection(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("error", _controller.TempData["ErrorMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database error", _controller.TempData["ErrorMessage"]!.ToString()!);
    }

    [Fact]
    public async Task UpdateSection_HandlesNullContent()
    {
        // Arrange
        var model = new QuickEditSectionViewModel
        {
            PageId = 1,
            SectionKey = "WelcomeText",
            Content = null,
            ContentType = "Text"
        };

        // Act
        await _controller.UpdateSection(model);

        // Assert
        _mockPageService.Verify(s => s.UpdateContentSectionAsync(
            1,
            "WelcomeText",
            string.Empty,
            "Text"), Times.Once);
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

    [Fact]
    public async Task UpdateImage_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var model = new QuickEditImageViewModel
        {
            PageId = 2,
            ImageKey = "HeaderImage",
            MediaId = 5,
            DisplayOrder = 3
        };

        // Act
        await _controller.UpdateImage(model);

        // Assert
        _mockPageService.Verify(s => s.SetPageImageAsync(2, "HeaderImage", 5, 3), Times.Once);
    }

    [Fact]
    public async Task UpdateImage_ReturnsToEdit_WithError_WhenModelInvalid()
    {
        // Arrange
        var model = new QuickEditImageViewModel { PageId = 1 };
        _controller.ModelState.AddModelError("ImageKey", "Required");

        // Act
        var result = await _controller.UpdateImage(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Invalid image data", _controller.TempData["ErrorMessage"]);
        _mockPageService.Verify(s => s.SetPageImageAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateImage_HandlesException_ReturnsErrorMessage()
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
            .ThrowsAsync(new InvalidOperationException("Media not found"));

        // Act
        var result = await _controller.UpdateImage(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("error", _controller.TempData["ErrorMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Media not found", _controller.TempData["ErrorMessage"]!.ToString()!);
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

    [Fact]
    public async Task RemoveImage_HandlesException_ReturnsErrorMessage()
    {
        // Arrange
        _mockPageService.Setup(s => s.RemovePageImageAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.RemoveImage(1, "HeroImage");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("error", _controller.TempData["ErrorMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public async Task MediaPicker_SetsAvailableMediaInViewBag()
    {
        // Arrange
        var page = new Page { Id = 1, PageName = "Home", Title = "Home" };
        var mediaList = new List<Media>
        {
            new Media { Id = 1, FileName = "test.jpg", FilePath = "/test.jpg", MimeType = "image/jpeg", FileSize = 1024, Source = MediaSource.Local, Category = MediaCategory.Hero }
        };

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
        Assert.Equal(mediaList, _controller.ViewBag.AvailableMedia);
    }

    [Fact]
    public async Task MediaPicker_HandlesExistingImage()
    {
        // Arrange
        var page = new Page { Id = 1, PageName = "Home", Title = "Home" };
        var media = new Media
        {
            Id = 5,
            FileName = "existing.jpg",
            FilePath = "/uploads/existing.jpg",
            MimeType = "image/jpeg",
            FileSize = 2048,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        var currentImage = new PageImage
        {
            PageId = 1,
            MediaId = 5,
            ImageKey = "HeroImage",
            DisplayOrder = 2,
            Media = media
        };

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockPageService.Setup(s => s.GetPageImageAsync(1, "HeroImage"))
            .ReturnsAsync(currentImage);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());

        // Act
        var result = await _controller.MediaPicker(1, "HeroImage");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuickEditImageViewModel>(viewResult.Model);
        Assert.Equal(5, model.CurrentMediaId);
        Assert.Equal("/uploads/existing.jpg", model.CurrentMediaPath);
        Assert.Equal(2, model.DisplayOrder);
    }

    [Fact]
    public async Task MediaPicker_HandlesNullPage()
    {
        // Arrange
        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync((Page?)null);
        _mockPageService.Setup(s => s.GetPageImageAsync(1, "HeroImage"))
            .ReturnsAsync((PageImage?)null);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());

        // Act
        var result = await _controller.MediaPicker(1, "HeroImage");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuickEditImageViewModel>(viewResult.Model);
        Assert.Equal("Unknown", model.PageName);
    }

    #endregion

    #region Edit Action - Additional Tests

    [Fact]
    public async Task Edit_PopulatesContentSections()
    {
        // Arrange
        var page = new Page
        {
            Id = 1,
            PageName = "Home",
            Title = "Home",
            ContentSections = new List<ContentSection>
            {
                new ContentSection { Id = 1, SectionKey = "WelcomeText", Content = "Welcome!", ContentType = "Text", DisplayOrder = 0 },
                new ContentSection { Id = 2, SectionKey = "AboutText", Content = "About us", ContentType = "RichText", DisplayOrder = 1 }
            },
            PageImages = new List<PageImage>()
        };

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageEditViewModel>(viewResult.Model);
        Assert.Equal(2, model.ContentSections.Count);
        Assert.Equal("WelcomeText", model.ContentSections[0].SectionKey);
        Assert.Equal("AboutText", model.ContentSections[1].SectionKey);
    }

    [Fact]
    public async Task Edit_PopulatesPageImages()
    {
        // Arrange
        var media = new Media
        {
            Id = 1,
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            AltText = "Test image",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };

        var page = new Page
        {
            Id = 1,
            PageName = "Home",
            Title = "Home",
            ContentSections = new List<ContentSection>(),
            PageImages = new List<PageImage>
            {
                new PageImage { Id = 1, ImageKey = "HeroImage", MediaId = 1, Media = media, DisplayOrder = 0 }
            }
        };

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageEditViewModel>(viewResult.Model);
        Assert.Single(model.PageImages);
        Assert.Equal("HeroImage", model.PageImages.First().ImageKey);
        Assert.Equal("test.jpg", model.PageImages.First().MediaFileName);
    }

    [Fact]
    public async Task Edit_SetsTinyMceApiKeyInViewData()
    {
        // Arrange
        var page = new Page
        {
            Id = 1,
            PageName = "Home",
            Title = "Home",
            ContentSections = new List<ContentSection>(),
            PageImages = new List<PageImage>()
        };

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());
        _mockConfiguration.Setup(c => c["TinyMCE:ApiKey"])
            .Returns("my-api-key");

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("my-api-key", _controller.ViewData["TinyMceApiKey"]);
    }

    [Fact]
    public async Task Edit_UsesDefaultApiKey_WhenConfigMissing()
    {
        // Arrange
        var page = new Page
        {
            Id = 1,
            PageName = "Home",
            Title = "Home",
            ContentSections = new List<ContentSection>(),
            PageImages = new List<PageImage>()
        };

        _mockPageService.Setup(s => s.GetPageByIdAsync(1))
            .ReturnsAsync(page);
        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(new List<Media>());
        _mockConfiguration.Setup(c => c["TinyMCE:ApiKey"])
            .Returns((string?)null);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("no-api-key", _controller.ViewData["TinyMceApiKey"]);
    }

    #endregion
}
