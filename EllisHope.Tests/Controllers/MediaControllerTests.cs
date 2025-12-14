using EllisHope.Areas.Admin.Controllers;
using EllisHope.Models.Domain;
using EllisHope.Models.ViewModels;
using EllisHope.Models;
using EllisHope.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class MediaControllerTests
{
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IUnsplashService> _mockUnsplashService;
    private readonly Mock<ILogger<MediaController>> _mockLogger;
    private readonly MediaController _controller;

    public MediaControllerTests()
    {
        _mockMediaService = new Mock<IMediaService>();
        _mockUnsplashService = new Mock<IUnsplashService>();
        _mockLogger = new Mock<ILogger<MediaController>>();

        _controller = new MediaController(
            _mockMediaService.Object,
            _mockUnsplashService.Object,
            _mockLogger.Object);

        // Setup TempData
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    #region Index Action Tests

    [Fact]
    public async Task Index_ReturnsViewWithAllMedia()
    {
        // Arrange
        var media = new List<Media>
        {
            new Media { Id = 1, FileName = "test1.jpg", FilePath = "/test1.jpg", MimeType = "image/jpeg", FileSize = 1024, Source = MediaSource.Local, Category = MediaCategory.Hero },
            new Media { Id = 2, FileName = "test2.jpg", FilePath = "/test2.jpg", MimeType = "image/jpeg", FileSize = 2048, Source = MediaSource.Unsplash, Category = MediaCategory.Blog }
        };

        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(media);
        _mockMediaService.Setup(s => s.GetTotalMediaCountAsync())
            .ReturnsAsync(2);
        _mockMediaService.Setup(s => s.GetTotalStorageSizeAsync())
            .ReturnsAsync(3072);
        _mockMediaService.Setup(s => s.GetMediaCountByCategoryAsync())
            .ReturnsAsync(new Dictionary<MediaCategory, int>());

        // Act
        var result = await _controller.Index(null, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MediaLibraryViewModel>(viewResult.Model);
        Assert.Equal(2, model.Media.Count());
    }

    [Fact]
    public async Task Index_FiltersMediaBySearchTerm()
    {
        // Arrange
        var searchResults = new List<Media>
        {
            new Media { Id = 1, FileName = "sunset.jpg", FilePath = "/sunset.jpg", MimeType = "image/jpeg", FileSize = 1024, Source = MediaSource.Local, Category = MediaCategory.Hero }
        };

        _mockMediaService.Setup(s => s.SearchMediaAsync("sunset", null))
            .ReturnsAsync(searchResults);
        _mockMediaService.Setup(s => s.GetTotalMediaCountAsync())
            .ReturnsAsync(1);
        _mockMediaService.Setup(s => s.GetTotalStorageSizeAsync())
            .ReturnsAsync(1024);
        _mockMediaService.Setup(s => s.GetMediaCountByCategoryAsync())
            .ReturnsAsync(new Dictionary<MediaCategory, int>());

        // Act
        var result = await _controller.Index("sunset", null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MediaLibraryViewModel>(viewResult.Model);
        Assert.Single(model.Media);
        Assert.Equal("sunset", model.SearchTerm);
    }

    [Fact]
    public async Task Index_FiltersMediaByCategory()
    {
        // Arrange
        var heroMedia = new List<Media>
        {
            new Media { Id = 1, FileName = "hero.jpg", FilePath = "/hero.jpg", MimeType = "image/jpeg", FileSize = 1024, Source = MediaSource.Local, Category = MediaCategory.Hero }
        };

        _mockMediaService.Setup(s => s.GetAllMediaAsync(MediaCategory.Hero, null))
            .ReturnsAsync(heroMedia);
        _mockMediaService.Setup(s => s.GetTotalMediaCountAsync())
            .ReturnsAsync(1);
        _mockMediaService.Setup(s => s.GetTotalStorageSizeAsync())
            .ReturnsAsync(1024);
        _mockMediaService.Setup(s => s.GetMediaCountByCategoryAsync())
            .ReturnsAsync(new Dictionary<MediaCategory, int>());

        // Act
        var result = await _controller.Index(null, MediaCategory.Hero, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MediaLibraryViewModel>(viewResult.Model);
        Assert.Single(model.Media);
        Assert.Equal(MediaCategory.Hero, model.FilterCategory);
    }

    [Fact]
    public async Task Index_FiltersMediaBySource()
    {
        // Arrange
        var unsplashMedia = new List<Media>
        {
            new Media { Id = 1, FileName = "unsplash.jpg", FilePath = "/unsplash.jpg", MimeType = "image/jpeg", FileSize = 1024, Source = MediaSource.Unsplash, Category = MediaCategory.Hero }
        };

        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, MediaSource.Unsplash))
            .ReturnsAsync(unsplashMedia);
        _mockMediaService.Setup(s => s.GetTotalMediaCountAsync())
            .ReturnsAsync(1);
        _mockMediaService.Setup(s => s.GetTotalStorageSizeAsync())
            .ReturnsAsync(1024);
        _mockMediaService.Setup(s => s.GetMediaCountByCategoryAsync())
            .ReturnsAsync(new Dictionary<MediaCategory, int>());

        // Act
        var result = await _controller.Index(null, null, MediaSource.Unsplash);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MediaLibraryViewModel>(viewResult.Model);
        Assert.Equal(MediaSource.Unsplash, model.FilterSource);
    }

    #endregion

    #region Upload GET Action Tests

    [Fact]
    public void Upload_Get_ReturnsViewWithEmptyModel()
    {
        // Act
        var result = _controller.Upload();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MediaUploadViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    #endregion

    #region Upload POST Action Tests

    [Fact]
    public async Task Upload_Post_ReturnsView_WhenModelInvalid()
    {
        // Arrange
        var model = new MediaUploadViewModel();
        _controller.ModelState.AddModelError("File", "Required");

        // Act
        var result = await _controller.Upload(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<MediaUploadViewModel>(viewResult.Model);
    }

    [Fact(Skip = "ModelState validation in tests requires manual setup - covered by integration tests")]
    public async Task Upload_Post_ValidModel_UploadedSuccessfully_RedirectsToIndex()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var model = new MediaUploadViewModel
        {
            File = fileMock.Object,
            AltText = "Test image",
            Category = MediaCategory.Page
        };

        var uploadedMedia = new Media
        {
            Id = 1,
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };

        _mockMediaService.Setup(s => s.UploadLocalImageAsync(
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<MediaCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(uploadedMedia);

        // Manually validate and ensure model is valid
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationContext, validationResults, true);
        
        Assert.True(isValid, "Model should be valid for this test");
        _controller.ModelState.Clear(); // Clear any pre-existing errors

        // Act
        var result = await _controller.Upload(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Contains("test.jpg", _controller.TempData["SuccessMessage"]!.ToString()!);
        Assert.Contains("successfully", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_Post_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);

        var model = new MediaUploadViewModel
        {
            File = fileMock.Object,
            AltText = "Test alt",
            Title = "Test title",
            Tags = "test,image",
            Category = MediaCategory.Hero
        };

        _mockMediaService.Setup(s => s.UploadLocalImageAsync(
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<MediaCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(new Media { Id = 1, FileName = "test.jpg", FilePath = "/test.jpg", MimeType = "image/jpeg", FileSize = 1024, Source = MediaSource.Local, Category = MediaCategory.Hero });

        _controller.ModelState.Clear();

        // Act
        await _controller.Upload(model);

        // Assert
        _mockMediaService.Verify(s => s.UploadLocalImageAsync(
            fileMock.Object,
            "Test alt",
            "Test title",
            MediaCategory.Hero,
            "test,image",
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Upload_Post_HandlesException_ReturnsViewWithError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var model = new MediaUploadViewModel { File = fileMock.Object };

        _mockMediaService.Setup(s => s.UploadLocalImageAsync(
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<MediaCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ThrowsAsync(new Exception("Upload failed"));

        // Act
        var result = await _controller.Upload(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region UnsplashSearch GET Action Tests

    [Fact]
    public void UnsplashSearch_Get_ReturnsViewWithEmptyModel()
    {
        // Act
        var result = _controller.UnsplashSearch();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UnsplashSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    #endregion

    #region UnsplashSearch POST Action Tests

    [Fact]
    public async Task UnsplashSearch_Post_EmptyQuery_ReturnsViewWithEmptyModel()
    {
        // Act
        var result = await _controller.UnsplashSearch("", 1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UnsplashSearchViewModel>(viewResult.Model);
        Assert.Null(model.Query);
    }

    [Fact]
    public async Task UnsplashSearch_Post_ValidQuery_ReturnsViewWithResults()
    {
        // Arrange
        var searchResults = new UnsplashSearchResult
        {
            Total = 10,
            TotalPages = 1,
            Results = new List<UnsplashPhoto>()
        };

        _mockUnsplashService.Setup(s => s.SearchPhotosAsync("nature", 1, 30))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.UnsplashSearch("nature", 1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UnsplashSearchViewModel>(viewResult.Model);
        Assert.Equal("nature", model.Query);
        Assert.Equal(1, model.Page);
        Assert.Equal(searchResults, model.Results);
    }

    [Fact]
    public async Task UnsplashSearch_Post_HandlesException_ReturnsViewWithError()
    {
        // Arrange
        _mockUnsplashService.Setup(s => s.SearchPhotosAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _controller.UnsplashSearch("test", 1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region ImportFromUnsplash Action Tests

    [Fact]
    public async Task ImportFromUnsplash_InvalidModel_RedirectsWithError()
    {
        // Arrange
        var model = new ImportUnsplashPhotoViewModel();
        _controller.ModelState.AddModelError("UnsplashPhotoId", "Required");

        // Act
        var result = await _controller.ImportFromUnsplash(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("UnsplashSearch", redirectResult.ActionName);
        Assert.Equal("Invalid photo data", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task ImportFromUnsplash_ValidModel_ImportSuccessfully()
    {
        // Arrange
        var model = new ImportUnsplashPhotoViewModel
        {
            UnsplashPhotoId = "abc123",
            AltText = "Beautiful photo",
            Category = MediaCategory.Hero
        };

        var importedMedia = new Media
        {
            Id = 1,
            FileName = "abc123.jpg",
            FilePath = "/uploads/unsplash/abc123.jpg",
            MimeType = "image/jpeg",
            FileSize = 2048,
            Source = MediaSource.Unsplash,
            Category = MediaCategory.Hero
        };

        _mockMediaService.Setup(s => s.ImportFromUnsplashAsync(
            "abc123",
            "Beautiful photo",
            MediaCategory.Hero,
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(importedMedia);

        // Manually validate model
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationContext, validationResults, true);
        
        Assert.True(isValid, "Model should be valid for this test");
        _controller.ModelState.Clear();

        // Act
        var result = await _controller.ImportFromUnsplash(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromUnsplash_HandlesException_RedirectsWithError()
    {
        // Arrange
        var model = new ImportUnsplashPhotoViewModel
        {
            UnsplashPhotoId = "abc123",
            Category = MediaCategory.Hero
        };

        _mockMediaService.Setup(s => s.ImportFromUnsplashAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<MediaCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ThrowsAsync(new Exception("Import failed"));

        // Act
        var result = await _controller.ImportFromUnsplash(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("UnsplashSearch", redirectResult.ActionName);
        Assert.Contains("error", _controller.TempData["ErrorMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Edit GET Action Tests

    [Fact]
    public async Task Edit_Get_MediaNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockMediaService.Setup(s => s.GetMediaByIdAsync(999))
            .ReturnsAsync((Media?)null);

        // Act
        var result = await _controller.Edit(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_MediaFound_ReturnsViewWithModel()
    {
        // Arrange
        var media = new Media
        {
            Id = 1,
            FileName = "test.jpg",
            FilePath = "/test.jpg",
            AltText = "Test image",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };

        _mockMediaService.Setup(s => s.GetMediaByIdAsync(1))
            .ReturnsAsync(media);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MediaEditViewModel>(viewResult.Model);
        Assert.Equal(1, model.Id);
        Assert.Equal("test.jpg", model.FileName);
    }

    #endregion

    #region Edit POST Action Tests

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        // Arrange
        var model = new MediaEditViewModel { Id = 1 };

        // Act
        var result = await _controller.Edit(2, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        var model = new MediaEditViewModel { Id = 1 };
        _controller.ModelState.AddModelError("AltText", "Required");

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<MediaEditViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Edit_Post_MediaNotFound_ReturnsNotFound()
    {
        // Arrange
        var model = new MediaEditViewModel { Id = 1, AltText = "Updated" };

        _mockMediaService.Setup(s => s.GetMediaByIdAsync(1))
            .ReturnsAsync((Media?)null);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
    {
        // Arrange
        var media = new Media
        {
            Id = 1,
            FileName = "test.jpg",
            FilePath = "/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };

        var model = new MediaEditViewModel
        {
            Id = 1,
            AltText = "Updated alt text",
            Title = "Updated title"
        };

        _mockMediaService.Setup(s => s.GetMediaByIdAsync(1))
            .ReturnsAsync(media);
        _mockMediaService.Setup(s => s.UpdateMediaAsync(It.IsAny<Media>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Edit_Post_HandlesException_ReturnsViewWithError()
    {
        // Arrange
        var media = new Media { Id = 1, FileName = "test.jpg", FilePath = "/test.jpg", MimeType = "image/jpeg", FileSize = 1024, Source = MediaSource.Local, Category = MediaCategory.Page };
        var model = new MediaEditViewModel { Id = 1, AltText = "Updated" };

        _mockMediaService.Setup(s => s.GetMediaByIdAsync(1))
            .ReturnsAsync(media);
        _mockMediaService.Setup(s => s.UpdateMediaAsync(It.IsAny<Media>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region Delete Action Tests

    [Fact]
    public async Task Delete_SuccessfulDelete_RedirectsWithSuccess()
    {
        // Arrange
        _mockMediaService.Setup(s => s.DeleteMediaAsync(1, false))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, false);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Delete_MediaInUse_RedirectsWithError()
    {
        // Arrange
        _mockMediaService.Setup(s => s.DeleteMediaAsync(1, false))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(1, false);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("Cannot delete media", _controller.TempData["ErrorMessage"]!.ToString()!);
        Assert.Contains("currently in use", _controller.TempData["ErrorMessage"]!.ToString()!);
    }

    [Fact]
    public async Task Delete_ForceDelete_DeletesSuccessfully()
    {
        // Arrange
        _mockMediaService.Setup(s => s.DeleteMediaAsync(1, true))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, true);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("success", _controller.TempData["SuccessMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Delete_HandlesException_RedirectsWithError()
    {
        // Arrange
        _mockMediaService.Setup(s => s.DeleteMediaAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.Delete(1, false);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("error", _controller.TempData["ErrorMessage"]!.ToString()!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Usages Action Tests

    [Fact]
    public async Task Usages_MediaNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockMediaService.Setup(s => s.GetMediaByIdAsync(999))
            .ReturnsAsync((Media?)null);

        // Act
        var result = await _controller.Usages(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Usages_MediaFound_ReturnsViewWithUsages()
    {
        // Arrange
        var media = new Media
        {
            Id = 1,
            FileName = "test.jpg",
            FilePath = "/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };

        var usages = new List<MediaUsage>
        {
            new MediaUsage { Id = 1, MediaId = 1, EntityType = "BlogPost", EntityId = 5, UsageType = UsageType.Featured }
        };

        _mockMediaService.Setup(s => s.GetMediaByIdAsync(1))
            .ReturnsAsync(media);
        _mockMediaService.Setup(s => s.GetMediaUsagesAsync(1))
            .ReturnsAsync(usages);

        // Act
        var result = await _controller.Usages(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<MediaUsage>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal(media, _controller.ViewBag.Media);
    }

    #endregion

    #region GetMediaJson Action Tests

    [Fact]
    public async Task GetMediaJson_ReturnsJsonWithAllMedia()
    {
        // Arrange
        var media = new List<Media>
        {
            new Media
            {
                Id = 1,
                FileName = "test.jpg",
                FilePath = "/test.jpg",
                AltText = "Test",
                Width = 800,
                Height = 600,
                Category = MediaCategory.Hero,
                Source = MediaSource.Local,
                MimeType = "image/jpeg",
                FileSize = 1024
            }
        };

        _mockMediaService.Setup(s => s.GetAllMediaAsync(null, null))
            .ReturnsAsync(media);

        // Act
        var result = await _controller.GetMediaJson(null);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public async Task GetMediaJson_FiltersByCategory()
    {
        // Arrange
        var heroMedia = new List<Media>
        {
            new Media
            {
                Id = 1,
                FileName = "hero.jpg",
                FilePath = "/hero.jpg",
                Category = MediaCategory.Hero,
                MimeType = "image/jpeg",
                FileSize = 1024,
                Source = MediaSource.Local
            }
        };

        _mockMediaService.Setup(s => s.GetAllMediaAsync(MediaCategory.Hero, null))
            .ReturnsAsync(heroMedia);

        // Act
        var result = await _controller.GetMediaJson(MediaCategory.Hero);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    #endregion
}
