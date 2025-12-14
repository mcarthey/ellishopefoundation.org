using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers.Admin;

public class AdminCausesControllerTests
{
    private readonly Mock<ICauseService> _mockCauseService;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<CausesController>> _mockLogger;
    private readonly CausesController _controller;

    public AdminCausesControllerTests()
    {
        _mockCauseService = new Mock<ICauseService>();
        _mockMediaService = new Mock<IMediaService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<CausesController>>();

        _controller = new CausesController(
            _mockCauseService.Object,
            _mockMediaService.Object,
            _mockEnvironment.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Setup TempData
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Edit_Post_WithValidModel_AndNoImageChange_UpdatesSuccessfully()
    {
        // Arrange
        var existing = new Cause
        {
            Id = 1,
            Title = "Old",
            GoalAmount = 1000,
            FeaturedImageUrl = "/uploads/media/old.jpg"
        };

        var model = new CauseViewModel
        {
            Id = 1,
            Title = "Updated",
            Description = "Updated Desc",
            GoalAmount = 2000,
            FeaturedImageUrl = "/uploads/media/old.jpg"
        };

        _mockCauseService.Setup(s => s.GetCauseByIdAsync(1)).ReturnsAsync(existing);
        _mockCauseService.Setup(s => s.UpdateCauseAsync(It.IsAny<Cause>())).ReturnsAsync((Cause c) => c);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirect.ActionName);
        Assert.Equal("Cause updated successfully!", _controller.TempData["SuccessMessage"]);

        _mockCauseService.Verify(s => s.UpdateCauseAsync(It.Is<Cause>(c =>
            c.Title == "Updated" &&
            c.GoalAmount == 2000 &&
            c.FeaturedImageUrl == "/uploads/media/old.jpg")), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_WithNewImageFromLibrary_UpdatesImageUrl()
    {
        // Arrange
        var existing = new Cause { Id = 1, Title = "Test", GoalAmount = 1000, FeaturedImageUrl = "/uploads/media/old.jpg" };
        var model = new CauseViewModel { Id = 1, Title = "Test", GoalAmount = 1000, FeaturedImageUrl = "/uploads/media/new.jpg" };

        _mockCauseService.Setup(s => s.GetCauseByIdAsync(1)).ReturnsAsync(existing);
        _mockCauseService.Setup(s => s.UpdateCauseAsync(It.IsAny<Cause>())).ReturnsAsync((Cause c) => c);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        _mockCauseService.Verify(s => s.UpdateCauseAsync(It.Is<Cause>(c =>
            c.FeaturedImageUrl == "/uploads/media/new.jpg")), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_WithInvalidModel_ReturnsViewWithErrorMessage()
    {
        // Arrange
        var model = new CauseViewModel { Id = 1 };
        _controller.ModelState.AddModelError("Title", "Required");

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(_controller.TempData["ErrorMessage"]);
        Assert.Contains("Validation failed", _controller.TempData["ErrorMessage"]?.ToString());
    }
}
