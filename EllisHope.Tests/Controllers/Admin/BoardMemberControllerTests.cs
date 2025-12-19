using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EllisHope.Tests.Controllers.Admin;

public class BoardMemberControllerTests
{
    private readonly Mock<IClientApplicationService> _mockApplicationService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<BoardMemberController>> _mockLogger;
    private readonly BoardMemberController _controller;

    public BoardMemberControllerTests()
    {
        _mockApplicationService = new Mock<IClientApplicationService>();
        _mockLogger = new Mock<ILogger<BoardMemberController>>();

        // Setup UserManager mock
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _controller = new BoardMemberController(
            _mockApplicationService.Object,
            _mockUserManager.Object,
            _mockLogger.Object
        );

        // Setup HttpContext and User
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Dashboard_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Dashboard_ReturnsViewWithStats_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User"
        };

        var memberStats = new BoardMemberStatistics
        {
            PendingVotes = 5,
            TotalVotesCast = 20,
            ParticipationRate = 85.5m
        };

        var appStats = new ApplicationStatistics
        {
            UnderReview = 10,
            AverageReviewDays = 7.5,
            ApprovalRate = 75.0m
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockApplicationService.Setup(s => s.GetBoardMemberStatisticsAsync("user-123"))
            .ReturnsAsync(memberStats);
        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync())
            .ReturnsAsync(appStats);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BoardMemberDashboardViewModel>(viewResult.Model);

        Assert.Equal("Test User", model.MemberName);
        Assert.Equal(5, model.PendingVotes);
        Assert.Equal(20, model.TotalVotesCast);
        Assert.Equal(85.5m, model.ParticipationRate);
        Assert.Equal(10, model.ApplicationsUnderReview);
        Assert.Equal(7.5, model.AverageReviewDays);
        Assert.Equal(75.0m, model.ApprovalRate);
    }

    [Fact]
    public async Task Dashboard_CallsGetBoardMemberStatistics_WithCorrectUserId()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123", FirstName = "Test", LastName = "User" };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockApplicationService.Setup(s => s.GetBoardMemberStatisticsAsync("user-123"))
            .ReturnsAsync(new BoardMemberStatistics());
        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync())
            .ReturnsAsync(new ApplicationStatistics());

        // Act
        await _controller.Dashboard();

        // Assert
        _mockApplicationService.Verify(
            s => s.GetBoardMemberStatisticsAsync("user-123"),
            Times.Once
        );
    }

    [Fact]
    public async Task Dashboard_CallsGetApplicationStatistics()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123", FirstName = "Test", LastName = "User" };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockApplicationService.Setup(s => s.GetBoardMemberStatisticsAsync(It.IsAny<string>()))
            .ReturnsAsync(new BoardMemberStatistics());
        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync())
            .ReturnsAsync(new ApplicationStatistics());

        // Act
        await _controller.Dashboard();

        // Assert
        _mockApplicationService.Verify(
            s => s.GetApplicationStatisticsAsync(),
            Times.Once
        );
    }

    [Fact]
    public async Task Dashboard_HandlesZeroParticipationRate()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123", FirstName = "New", LastName = "Member" };

        var memberStats = new BoardMemberStatistics
        {
            PendingVotes = 10,
            TotalVotesCast = 0,
            ParticipationRate = 0m
        };

        var appStats = new ApplicationStatistics();

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockApplicationService.Setup(s => s.GetBoardMemberStatisticsAsync("user-123"))
            .ReturnsAsync(memberStats);
        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync())
            .ReturnsAsync(appStats);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BoardMemberDashboardViewModel>(viewResult.Model);
        Assert.Equal(0m, model.ParticipationRate);
        Assert.Equal(0, model.TotalVotesCast);
    }

    [Fact]
    public async Task Dashboard_HandlesHighVolumeStats()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123", FirstName = "Active", LastName = "Member" };

        var memberStats = new BoardMemberStatistics
        {
            PendingVotes = 50,
            TotalVotesCast = 500,
            ParticipationRate = 98.5m
        };

        var appStats = new ApplicationStatistics
        {
            UnderReview = 100,
            AverageReviewDays = 3.5,
            ApprovalRate = 65.0m
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockApplicationService.Setup(s => s.GetBoardMemberStatisticsAsync("user-123"))
            .ReturnsAsync(memberStats);
        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync())
            .ReturnsAsync(appStats);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BoardMemberDashboardViewModel>(viewResult.Model);
        Assert.Equal(50, model.PendingVotes);
        Assert.Equal(500, model.TotalVotesCast);
        Assert.Equal(98.5m, model.ParticipationRate);
    }
}
