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
using Xunit;

namespace EllisHope.Tests.Controllers;

public class EventsControllerTests
{
    private readonly Mock<IEventService> _mockEventService;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _mockEventService = new Mock<IEventService>();
        _mockMediaService = new Mock<IMediaService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockConfiguration = new Mock<IConfiguration>();

        _controller = new EventsController(
            _mockEventService.Object,
            _mockMediaService.Object,
            _mockEnvironment.Object,
            _mockConfiguration.Object);

        // Setup TempData
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    #region Edit Tests

    [Fact]
    public async Task Edit_Get_ReturnsViewWithModel()
    {
        // Arrange
        var eventItem = new Event
        {
            Id = 1,
            Title = "Test Event",
            Slug = "test-event",
            Description = "Test Description",
            Location = "Test Location",
            EventDate = DateTime.Now.AddDays(7),
            IsPublished = true
        };

        _mockEventService.Setup(s => s.GetEventByIdAsync(1))
            .ReturnsAsync(eventItem);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<EventViewModel>(viewResult.Model);
        Assert.Equal(1, viewModel.Id);
        Assert.Equal("Test Event", viewModel.Title);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_NoImageChange_UpdatesSuccessfully()
    {
        // Arrange
        var existingEvent = new Event
        {
            Id = 1,
            Title = "Old Title",
            Slug = "old-title",
            Location = "Old Location",
            EventDate = DateTime.Now,
            FeaturedImageUrl = "/uploads/media/existing.jpg"
        };

        var model = new EventViewModel
        {
            Id = 1,
            Title = "Updated Title",
            Slug = "updated-title",
            Location = "Updated Location",
            EventDate = DateTime.Now.AddDays(1),
            FeaturedImageUrl = "/uploads/media/existing.jpg", // Same image
            FeaturedImageFile = null // No new file
        };

        _mockEventService.Setup(s => s.GetEventByIdAsync(1))
            .ReturnsAsync(existingEvent);

        _mockEventService.Setup(s => s.UpdateEventAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => e);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("Event updated successfully!", _controller.TempData["SuccessMessage"]);

        // Verify the service was called
        _mockEventService.Verify(s => s.UpdateEventAsync(It.Is<Event>(e =>
            e.Id == 1 &&
            e.Title == "Updated Title" &&
            e.FeaturedImageUrl == "/uploads/media/existing.jpg")), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_NewImageFromLibrary_UpdatesSuccessfully()
    {
        // Arrange
        var existingEvent = new Event
        {
            Id = 1,
            Title = "Test Event",
            Slug = "test-event",
            Location = "Test Location",
            EventDate = DateTime.Now,
            FeaturedImageUrl = "/uploads/media/old.jpg"
        };

        var model = new EventViewModel
        {
            Id = 1,
            Title = "Test Event",
            Slug = "test-event",
            Location = "Test Location",
            EventDate = DateTime.Now,
            FeaturedImageUrl = "/uploads/media/new.jpg", // Different image
            FeaturedImageFile = null
        };

        _mockEventService.Setup(s => s.GetEventByIdAsync(1))
            .ReturnsAsync(existingEvent);

        _mockEventService.Setup(s => s.UpdateEventAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => e);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);

        // Verify the image URL was updated
        _mockEventService.Verify(s => s.UpdateEventAsync(It.Is<Event>(e =>
            e.FeaturedImageUrl == "/uploads/media/new.jpg")), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_NewFileUpload_UpdatesSuccessfully()
    {
        // Arrange
        var existingEvent = new Event
        {
            Id = 1,
            Title = "Test Event",
            Slug = "test-event",
            Location = "Test Location",
            EventDate = DateTime.Now,
            FeaturedImageUrl = "/uploads/media/old.jpg"
        };

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.Length).Returns(1024);

        var model = new EventViewModel
        {
            Id = 1,
            Title = "Test Event",
            Slug = "test-event",
            Location = "Test Location",
            EventDate = DateTime.Now,
            FeaturedImageUrl = "/uploads/media/old.jpg", // Same as existing
            FeaturedImageFile = mockFile.Object // New file
        };

        var uploadedMedia = new Media
        {
            Id = 10,
            FilePath = "/uploads/media/new-uploaded.jpg"
        };

        _mockEventService.Setup(s => s.GetEventByIdAsync(1))
            .ReturnsAsync(existingEvent);

        _mockMediaService.Setup(s => s.UploadLocalImageAsync(
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            MediaCategory.Event,
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(uploadedMedia);

        _mockMediaService.Setup(s => s.TrackMediaUsageAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<UsageType>()))
            .Returns(Task.CompletedTask);

        _mockEventService.Setup(s => s.UpdateEventAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => e);

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);

        // Verify file was uploaded
        _mockMediaService.Verify(s => s.UploadLocalImageAsync(
            mockFile.Object,
            "Event: Test Event",
            "Test Event",
            MediaCategory.Event,
            null,
            null), Times.Once);

        // Verify image URL was updated to new upload
        _mockEventService.Verify(s => s.UpdateEventAsync(It.Is<Event>(e =>
            e.FeaturedImageUrl == "/uploads/media/new-uploaded.jpg")), Times.Once);

        // Verify usage tracking
        _mockMediaService.Verify(s => s.TrackMediaUsageAsync(
            10, "Event", 1, UsageType.Featured), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsViewWithErrors()
    {
        // Arrange
        var model = new EventViewModel
        {
            Id = 1
            // Missing required fields
        };

        _controller.ModelState.AddModelError("Title", "Title is required");

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<EventViewModel>(viewResult.Model);
        Assert.True(_controller.ModelState.ErrorCount > 0);
        Assert.NotNull(_controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        // Arrange
        var model = new EventViewModel { Id = 1 };

        // Act
        var result = await _controller.Edit(2, model); // Different ID

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_EventNotFound_ReturnsNotFound()
    {
        // Arrange
        var model = new EventViewModel
        {
            Id = 999,
            Title = "Test",
            Location = "Test",
            EventDate = DateTime.Now
        };

        _mockEventService.Setup(s => s.GetEventByIdAsync(999))
            .ReturnsAsync((Event?)null);

        // Act
        var result = await _controller.Edit(999, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_Post_ValidModel_NoImage_CreatesSuccessfully()
    {
        // Arrange
        var model = new EventViewModel
        {
            Title = "New Event",
            Slug = "new-event",
            Description = "Description",
            Location = "Location",
            EventDate = DateTime.Now.AddDays(7)
        };

        _mockEventService.Setup(s => s.CreateEventAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => { e.Id = 1; return e; });

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("Event created successfully!", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Create_Post_ValidModel_WithImageFromLibrary_CreatesSuccessfully()
    {
        // Arrange
        var model = new EventViewModel
        {
            Title = "New Event",
            Description = "Description",
            Location = "Location",
            EventDate = DateTime.Now.AddDays(7),
            FeaturedImageUrl = "/uploads/media/existing.jpg"
        };

        _mockEventService.Setup(s => s.CreateEventAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => { e.Id = 1; return e; });

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        // Verify the image was set
        _mockEventService.Verify(s => s.CreateEventAsync(It.Is<Event>(e =>
            e.FeaturedImageUrl == "/uploads/media/existing.jpg")), Times.Once);
    }

    #endregion
}
