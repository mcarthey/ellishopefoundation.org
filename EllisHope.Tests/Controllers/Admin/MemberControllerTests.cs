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

public class MemberControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<MemberController>> _mockLogger;
    private readonly MemberController _controller;
    private readonly ApplicationUser _testMember;

    public MemberControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _mockUserManager = MockHelpers.MockUserManager<ApplicationUser>();
        _mockLogger = new Mock<ILogger<MemberController>>();

        _controller = new MemberController(
            _context,
            _mockUserManager.Object,
            _mockLogger.Object
        );

        // Create test member
        _testMember = new ApplicationUser
        {
            Id = "member-1",
            Email = "member@test.com",
            UserName = "member@test.com",
            FirstName = "Test",
            LastName = "Member",
            UserRole = UserRole.Member,
            Status = MembershipStatus.Active,
            JoinedDate = DateTime.UtcNow.AddYears(-1),
            LastLoginDate = DateTime.UtcNow.AddDays(-1)
        };

        _context.Users.Add(_testMember);
        _context.SaveChanges();

        // Setup User identity (Member role)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testMember.Id),
            new Claim(ClaimTypes.Name, _testMember.Email!),
            new Claim(ClaimTypes.Role, "Member")
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
    public async Task Dashboard_ReturnsViewWithMemberData()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_testMember);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MemberDashboardViewModel>(viewResult.Model);
        Assert.Equal("Test Member", model.MemberName);
        Assert.Equal("member@test.com", model.MemberEmail);
        Assert.Equal(MembershipStatus.Active, model.Status);
        Assert.Equal(_testMember.JoinedDate, model.JoinedDate);
        Assert.Equal(_testMember.LastLoginDate, model.LastLoginDate);
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
    public async Task Dashboard_HandlesNewMemberWithoutLastLogin()
    {
        // Arrange
        var newMember = new ApplicationUser
        {
            Id = "member-2",
            Email = "newmember@test.com",
            UserName = "newmember@test.com",
            FirstName = "New",
            LastName = "Member",
            UserRole = UserRole.Member,
            Status = MembershipStatus.Pending,
            JoinedDate = DateTime.UtcNow,
            LastLoginDate = null
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(newMember);

        // Act
        var result = await _controller.Dashboard();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MemberDashboardViewModel>(viewResult.Model);
        Assert.Equal("New Member", model.MemberName);
        Assert.Null(model.LastLoginDate);
        Assert.Equal(MembershipStatus.Pending, model.Status);
    }

    #endregion

    #region Events Tests

    [Fact]
    public void Events_ReturnsView()
    {
        // Act
        var result = _controller.Events();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    #endregion

    #region Volunteer Tests

    [Fact]
    public void Volunteer_ReturnsView()
    {
        // Act
        var result = _controller.Volunteer();

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
