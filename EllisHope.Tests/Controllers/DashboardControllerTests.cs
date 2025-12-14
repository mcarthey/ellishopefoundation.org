using EllisHope.Areas.Admin.Controllers;
using EllisHope.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<ILogger<DashboardController>> _mockLogger;
    private readonly Mock<IBlogService> _mockBlogService;
    private readonly Mock<IEventService> _mockEventService;
    private readonly Mock<ICauseService> _mockCauseService;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IPageService> _mockPageService;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockLogger = new Mock<ILogger<DashboardController>>();
        _mockBlogService = new Mock<IBlogService>();
        _mockEventService = new Mock<IEventService>();
        _mockCauseService = new Mock<ICauseService>();
        _mockMediaService = new Mock<IMediaService>();
        _mockPageService = new Mock<IPageService>();

        _controller = new DashboardController(
            _mockLogger.Object,
            _mockBlogService.Object,
            _mockEventService.Object,
            _mockCauseService.Object,
            _mockMediaService.Object,
            _mockPageService.Object);

        // Setup default empty returns
        _mockBlogService.Setup(s => s.GetAllPostsAsync(It.IsAny<bool>())).ReturnsAsync(new List<Models.Domain.BlogPost>());
        _mockEventService.Setup(s => s.GetAllEventsAsync(It.IsAny<bool>())).ReturnsAsync(new List<Models.Domain.Event>());
        _mockCauseService.Setup(s => s.GetAllCausesAsync(It.IsAny<bool>())).ReturnsAsync(new List<Models.Domain.Cause>());
        _mockMediaService.Setup(s => s.GetTotalMediaCountAsync()).ReturnsAsync(0);
        _mockPageService.Setup(s => s.GetAllPagesAsync()).ReturnsAsync(new List<Models.Domain.Page>());
    }

    [Fact]
    public async Task Index_ReturnsView()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "admin@test.com")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Index_SetsUserNameInViewData_WhenUserAuthenticated()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "admin@test.com"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("admin@test.com", _controller.ViewData["UserName"]);
    }

    [Fact]
    public async Task Index_SetsDefaultUserName_WhenUserNotAuthenticated()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Admin", _controller.ViewData["UserName"]);
    }

    [Fact]
    public async Task Index_SetsStatistics_InViewData()
    {
        // Arrange
        _mockBlogService.Setup(s => s.GetAllPostsAsync(true))
            .ReturnsAsync(new List<Models.Domain.BlogPost> { new(), new(), new() });
        _mockEventService.Setup(s => s.GetAllEventsAsync(true))
            .ReturnsAsync(new List<Models.Domain.Event> { new(), new() });
        _mockMediaService.Setup(s => s.GetTotalMediaCountAsync())
            .ReturnsAsync(10);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.Equal(3, _controller.ViewData["BlogCount"]);
        Assert.Equal(2, _controller.ViewData["EventCount"]);
        Assert.Equal(10, _controller.ViewData["MediaCount"]);
    }
}
