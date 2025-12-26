using EllisHope.Controllers;
using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class TeamControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TeamController _controller;
    private readonly List<ApplicationUser> _testUsers;

    public TeamControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TeamControllerTest_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _controller = new TeamController(_context);

        // Seed test data
        _testUsers = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "board-1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@ellishope.org",
                PhoneNumber = "262-555-0001",
                UserRole = UserRole.BoardMember,
                IsActive = true,
                JoinedDate = DateTime.UtcNow.AddYears(-2),
                ProfilePictureUrl = "/assets/img/board/john.jpg",
                AdminNotes = "Founding board member with extensive nonprofit experience."
            },
            new ApplicationUser
            {
                Id = "board-2",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@ellishope.org",
                PhoneNumber = "262-555-0002",
                UserRole = UserRole.BoardMember,
                IsActive = true,
                JoinedDate = DateTime.UtcNow.AddYears(-1),
                ProfilePictureUrl = "/assets/img/board/jane.jpg"
            },
            new ApplicationUser
            {
                Id = "board-inactive",
                FirstName = "Bob",
                LastName = "Inactive",
                Email = "bob@ellishope.org",
                UserRole = UserRole.BoardMember,
                IsActive = false, // Inactive - should not appear
                JoinedDate = DateTime.UtcNow.AddYears(-3)
            },
            new ApplicationUser
            {
                Id = "sponsor-1",
                FirstName = "Alice",
                LastName = "Sponsor",
                Email = "alice@ellishope.org",
                UserRole = UserRole.Sponsor, // Not a board member
                IsActive = true,
                JoinedDate = DateTime.UtcNow.AddMonths(-6)
            }
        };

        _context.Users.AddRange(_testUsers);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task V1_ReturnsViewWithActiveBoardMembers()
    {
        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.Model);

        var boardMembers = model.ToList();
        Assert.Equal(2, boardMembers.Count); // Only active board members
        Assert.All(boardMembers, member => Assert.Equal(UserRole.BoardMember, member.UserRole));
        Assert.All(boardMembers, member => Assert.True(member.IsActive));
    }

    [Fact]
    public async Task V1_OrdersBoardMembersByLastNameThenFirstName()
    {
        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.Model);

        var boardMembers = model.ToList();
        Assert.Equal("Doe", boardMembers[0].LastName);
        Assert.Equal("Smith", boardMembers[1].LastName);
    }

    [Fact]
    public async Task V1_ExcludesInactiveBoardMembers()
    {
        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.Model);

        var boardMembers = model.ToList();
        Assert.DoesNotContain(boardMembers, m => m.Id == "board-inactive");
    }

    [Fact]
    public async Task V1_ExcludesNonBoardMemberRoles()
    {
        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.Model);

        var boardMembers = model.ToList();
        Assert.DoesNotContain(boardMembers, m => m.UserRole != UserRole.BoardMember);
    }

    [Fact]
    public async Task V1_ReturnsEmptyListWhenNoBoardMembers()
    {
        // Arrange
        _context.Users.RemoveRange(_context.Users.Where(u => u.UserRole == UserRole.BoardMember));
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Details_WithValidId_ReturnsViewWithBoardMember()
    {
        // Act
        var result = await _controller.Details("board-1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationUser>(viewResult.Model);
        Assert.Equal("board-1", model.Id);
        Assert.Equal("John", model.FirstName);
        Assert.Equal("Doe", model.LastName);
    }

    [Fact]
    public async Task Details_WithNullId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details(null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithEmptyId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details("");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details("nonexistent-id");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithInactiveBoardMember_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details("board-inactive");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithNonBoardMemberUser_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details("sponsor-1");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsUserWithAllProperties()
    {
        // Act
        var result = await _controller.Details("board-1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationUser>(viewResult.Model);

        Assert.Equal("John", model.FirstName);
        Assert.Equal("Doe", model.LastName);
        Assert.Equal("john.doe@ellishope.org", model.Email);
        Assert.Equal("262-555-0001", model.PhoneNumber);
        Assert.Equal("/assets/img/board/john.jpg", model.ProfilePictureUrl);
        Assert.Equal("Founding board member with extensive nonprofit experience.", model.AdminNotes);
        Assert.Equal(UserRole.BoardMember, model.UserRole);
        Assert.True(model.IsActive);
    }

}
