using EllisHope.Controllers;
using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class AboutControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AboutController _controller;
    private readonly List<ApplicationUser> _testUsers;

    public AboutControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"AboutControllerTest_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _controller = new AboutController(_context);

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
                ProfilePictureUrl = "/assets/img/board/john.jpg"
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
    public async Task Index_ReturnsViewWithActiveBoardMembers()
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
    public async Task Index_OrdersBoardMembersByLastNameThenFirstName()
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
    public async Task Index_ExcludesInactiveBoardMembers()
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
    public async Task Index_ExcludesNonBoardMemberRoles()
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
    public async Task Index_ReturnsEmptyListWhenNoBoardMembers()
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
    public async Task Index_ReturnsDefaultView()
    {
        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }
}
