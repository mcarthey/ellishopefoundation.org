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

public class SponsorControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<SponsorController>> _mockLogger;
    private readonly SponsorController _controller;
    private readonly ApplicationUser _testSponsor;
    private readonly ApplicationUser _testClient1;
    private readonly ApplicationUser _testClient2;
    private readonly ApplicationUser _otherSponsor;
    private readonly ApplicationUser _otherClient;

    public SponsorControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _mockUserManager = MockHelpers.MockUserManager<ApplicationUser>();
        _mockLogger = new Mock<ILogger<SponsorController>>();

        _controller = new SponsorController(
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
            FirstName = "Main",
            LastName = "Sponsor",
            UserRole = UserRole.Sponsor
        };

        // Create test clients for main sponsor
        _testClient1 = new ApplicationUser
        {
            Id = "client-1",
            Email = "client1@test.com",
            UserName = "client1@test.com",
            FirstName = "John",
            LastName = "Doe",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            JoinedDate = DateTime.UtcNow.AddMonths(-2),
            MonthlyFee = 150.00m,
            SponsorId = "sponsor-1"
        };

        _testClient2 = new ApplicationUser
        {
            Id = "client-2",
            Email = "client2@test.com",
            UserName = "client2@test.com",
            FirstName = "Jane",
            LastName = "Smith",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Pending,
            JoinedDate = DateTime.UtcNow.AddDays(-5),
            MonthlyFee = 100.00m,
            SponsorId = "sponsor-1"
        };

        // Create another sponsor and client to test authorization
        _otherSponsor = new ApplicationUser
        {
            Id = "sponsor-2",
            Email = "other@test.com",
            UserName = "other@test.com",
            FirstName = "Other",
            LastName = "Sponsor",
            UserRole = UserRole.Sponsor
        };

        _otherClient = new ApplicationUser
        {
            Id = "client-3",
            Email = "otherclient@test.com",
            UserName = "otherclient@test.com",
            FirstName = "Other",
            LastName = "Client",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            SponsorId = "sponsor-2"
        };

        _context.Users.AddRange(_testSponsor, _testClient1, _testClient2, _otherSponsor, _otherClient);
        _context.SaveChanges();

        // Setup User identity (Sponsor role)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testSponsor.Id),
            new Claim(ClaimTypes.Name, _testSponsor.Email!),
            new Claim(ClaimTypes.Role, "Sponsor")
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
    public async Task Dashboard_ReturnsViewWithSponsorData()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SponsorDashboardViewModel>(viewResult.Model);
        Assert.Equal("Main Sponsor", model.SponsorName);
        Assert.Equal("sponsor@test.com", model.SponsorEmail);
        Assert.Equal(2, model.TotalClients);
        Assert.Equal(1, model.ActiveClients);
        Assert.Equal(1, model.PendingClients);
    }

    [Fact]
    public async Task Dashboard_CalculatesTotalMonthlyCommitment()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SponsorDashboardViewModel>(viewResult.Model);
        // Only active clients count toward commitment
        Assert.Equal(150.00m, model.TotalMonthlyCommitment);
    }

    [Fact]
    public async Task Dashboard_ListsSponsoredClients()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SponsorDashboardViewModel>(viewResult.Model);
        Assert.Equal(2, model.SponsoredClients.Count);

        // Check sorted by last name
        Assert.Equal("Doe", model.SponsoredClients[0].FullName.Split(' ')[1]);
        Assert.Equal("Smith", model.SponsoredClients[1].FullName.Split(' ')[1]);
    }

    [Fact]
    public async Task Dashboard_HandlesSponsorWithNoClients()
    {
        // Arrange
        var sponsorNoClients = new ApplicationUser
        {
            Id = "sponsor-3",
            Email = "noclient@test.com",
            UserName = "noclient@test.com",
            FirstName = "No",
            LastName = "Clients",
            UserRole = UserRole.Sponsor
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(sponsorNoClients);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SponsorDashboardViewModel>(viewResult.Model);
        Assert.Equal(0, model.TotalClients);
        Assert.Equal(0, model.ActiveClients);
        Assert.Equal(0, model.PendingClients);
        Assert.Equal(0, model.TotalMonthlyCommitment);
        Assert.Empty(model.SponsoredClients);
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

    #region ClientDetails Tests

    [Fact]
    public async Task ClientDetails_ReturnsViewWithClientData_WhenSponsorOwnsClient()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.ClientDetails("client-1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientDetailsViewModel>(viewResult.Model);
        Assert.Equal("client-1", model.Id);
        Assert.Equal("John Doe", model.FullName);
        Assert.Equal("client1@test.com", model.Email);
        Assert.Equal(MembershipStatus.Active, model.Status);
        Assert.Equal(150.00m, model.MonthlyFee);
    }

    [Fact]
    public async Task ClientDetails_ReturnsNotFound_WhenSponsorDoesNotOwnClient()
    {
        // Arrange - Try to access another sponsor's client
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.ClientDetails("client-3"); // Belongs to sponsor-2

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Client not found or you are not the sponsor for this client.", notFoundResult.Value);
    }

    [Fact]
    public async Task ClientDetails_ReturnsNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.ClientDetails("nonexistent-client");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ClientDetails_ReturnsNotFound_WhenSponsorNotFound()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.ClientDetails("client-1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ClientDetails_IncludesAllClientInformation()
    {
        // Arrange - Set up a client with full details
        var detailedClient = new ApplicationUser
        {
            Id = "client-detailed",
            Email = "detailed@test.com",
            UserName = "detailed@test.com",
            FirstName = "Detailed",
            LastName = "Client",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            DateOfBirth = new DateTime(1990, 5, 15),
            Address = "123 Main St",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            EmergencyContactName = "Emergency Contact",
            EmergencyContactPhone = "555-0199",
            JoinedDate = DateTime.UtcNow.AddMonths(-6),
            MonthlyFee = 200.00m,
            MembershipStartDate = DateTime.UtcNow.AddMonths(-6),
            MembershipEndDate = DateTime.UtcNow.AddMonths(6),
            SponsorId = "sponsor-1"
        };

        _context.Users.Add(detailedClient);
        _context.SaveChanges();

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.ClientDetails("client-detailed");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientDetailsViewModel>(viewResult.Model);
        Assert.Equal("123 Main St", model.Address);
        Assert.Equal("Springfield", model.City);
        Assert.Equal("IL", model.State);
        Assert.Equal("62701", model.ZipCode);
        Assert.Equal("Emergency Contact", model.EmergencyContactName);
        Assert.Equal("555-0199", model.EmergencyContactPhone);
        Assert.NotNull(model.Age);
    }

    #endregion

    #region MyProfile Tests

    [Fact]
    public async Task MyProfile_RedirectsToProfileIndex()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testSponsor);

        // Act
        var result = await _controller.MyProfile();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Profile", redirectResult.ControllerName);
    }

    [Fact]
    public async Task MyProfile_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.MyProfile();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}
