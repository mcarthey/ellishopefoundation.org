using EllisHope.Areas.Admin.Controllers;
using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EllisHope.Tests.Controllers.Admin;

public class ClientControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<ClientController>> _mockLogger;
    private readonly ClientController _controller;
    private readonly ApplicationUser _testClient;
    private readonly ApplicationUser _testSponsor;

    public ClientControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _mockUserManager = MockHelpers.MockUserManager<ApplicationUser>();
        _mockLogger = new Mock<ILogger<ClientController>>();

        _controller = new ClientController(
            _context,
            _mockUserManager.Object,
            _mockLogger.Object
        );

        // Create test sponsor
        _testSponsor = new ApplicationUser
        {
            Id = "sponsor-1",
            Email = "sponsor@test.com",
            UserName = "sponsor@test.com",
            FirstName = "John",
            LastName = "Sponsor",
            PhoneNumber = "555-0100",
            UserRole = UserRole.Sponsor
        };

        // Create test client
        _testClient = new ApplicationUser
        {
            Id = "client-1",
            Email = "client@test.com",
            UserName = "client@test.com",
            FirstName = "Jane",
            LastName = "Client",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            JoinedDate = DateTime.UtcNow.AddMonths(-2),
            MonthlyFee = 150.00m,
            MembershipStartDate = DateTime.UtcNow.AddMonths(-2),
            MembershipEndDate = DateTime.UtcNow.AddMonths(10),
            SponsorId = "sponsor-1"
        };

        _context.Users.Add(_testSponsor);
        _context.Users.Add(_testClient);
        _context.SaveChanges();

        // Setup User identity (Client role)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testClient.Id),
            new Claim(ClaimTypes.Name, _testClient.Email!),
            new Claim(ClaimTypes.Role, "Client")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    #region Dashboard Tests

    [Fact]
    public async Task Dashboard_ReturnsViewWithClientData()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testClient);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientDashboardViewModel>(viewResult.Model);
        Assert.Equal("Jane Client", model.ClientName);
        Assert.Equal("client@test.com", model.ClientEmail);
        Assert.Equal(MembershipStatus.Active, model.Status);
        Assert.Equal(150.00m, model.MonthlyFee);
    }

    [Fact]
    public async Task Dashboard_DisplaysSponsorInformation_WhenSponsorAssigned()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testClient);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientDashboardViewModel>(viewResult.Model);
        Assert.True(model.HasSponsor);
        Assert.Equal("John Sponsor", model.SponsorName);
        Assert.Equal("sponsor@test.com", model.SponsorEmail);
        Assert.Equal("555-0100", model.SponsorPhone);
    }

    [Fact]
    public async Task Dashboard_CalculatesMembershipProgress()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testClient);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientDashboardViewModel>(viewResult.Model);
        Assert.NotNull(model.MembershipDays);
        Assert.NotNull(model.DaysRemaining);
        Assert.NotNull(model.MembershipProgress);
        Assert.True(model.MembershipProgress >= 0 && model.MembershipProgress <= 100);
    }

    [Fact]
    public async Task Dashboard_HandlesClientWithoutSponsor()
    {
        // Arrange
        var clientWithoutSponsor = new ApplicationUser
        {
            Id = "client-2",
            Email = "client2@test.com",
            UserName = "client2@test.com",
            FirstName = "Solo",
            LastName = "Client",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            JoinedDate = DateTime.UtcNow,
            MonthlyFee = 100.00m,
            SponsorId = null
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(clientWithoutSponsor);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientDashboardViewModel>(viewResult.Model);
        Assert.False(model.HasSponsor);
        Assert.Null(model.SponsorName);
        Assert.Null(model.SponsorEmail);
    }

    [Fact]
    public async Task Dashboard_HandlesClientWithoutMembershipDates()
    {
        // Arrange
        var clientWithoutDates = new ApplicationUser
        {
            Id = "client-3",
            Email = "client3@test.com",
            UserName = "client3@test.com",
            FirstName = "New",
            LastName = "Client",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Pending,
            JoinedDate = DateTime.UtcNow,
            MembershipStartDate = null,
            MembershipEndDate = null
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(clientWithoutDates);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientDashboardViewModel>(viewResult.Model);
        Assert.Null(model.MembershipDays);
        Assert.Null(model.DaysRemaining);
        Assert.Null(model.MembershipProgress);
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

    #endregion

    #region Progress Tests

    [Fact]
    public async Task Progress_ReturnsViewWithMilestones()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testClient);

        // Act
        var result = await _controller.Progress();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientProgressViewModel>(viewResult.Model);
        Assert.Equal("Jane Client", model.ClientName);
        Assert.NotEmpty(model.Milestones);
        Assert.Equal(5, model.Milestones.Count);
        Assert.True(model.Milestones.First().IsCompleted); // Registration Complete
        Assert.False(model.Milestones.Last().IsCompleted); // Future milestones
    }

    [Fact]
    public async Task Progress_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Progress();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Resources Tests

    [Fact]
    public void Resources_ReturnsView()
    {
        // Act
        var result = _controller.Resources();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    #endregion

    #region MyProfile Tests

    [Fact]
    public async Task MyProfile_RedirectsToProfileIndex()
    {
        // Act
        var result = await _controller.MyProfile();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Profile", redirectResult.ControllerName);
    }

    #endregion
}
