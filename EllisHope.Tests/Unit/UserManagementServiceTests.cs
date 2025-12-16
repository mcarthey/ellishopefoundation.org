using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Unit;

/// <summary>
/// Unit tests for UserManagementService
/// Tests user management business logic in isolation
/// </summary>
public class UserManagementServiceTests : IDisposable
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<UserManagementService>> _mockLogger;
    private readonly ApplicationDbContext _context;
    private readonly UserManagementService _service;

    public UserManagementServiceTests()
    {
        // Setup in-memory database with unique name for test isolation
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup UserManager mock
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _mockLogger = new Mock<ILogger<UserManagementService>>();

        _service = new UserManagementService(
            _mockUserManager.Object,
            _context,
            _mockLogger.Object);
    }

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers_OrderedByName()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserName = "john@test.com" },
            new ApplicationUser { Id = "2", FirstName = "Alice", LastName = "Smith", Email = "alice@test.com", UserName = "alice@test.com" },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com", UserName = "bob@test.com" }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllUsersAsync();
        var resultList = result.ToList();

        // Assert
        Assert.Equal(3, resultList.Count);
        // Should be ordered by LastName then FirstName: Doe, Johnson, Smith
        Assert.Equal("John", resultList[0].FirstName);
        Assert.Equal("Bob", resultList[1].FirstName);
        Assert.Equal("Alice", resultList[2].FirstName);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetUsersByRoleAsync Tests

    [Fact]
    public async Task GetUsersByRoleAsync_FiltersByRole()
    {
        // Arrange
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserRole = UserRole.Member },
            new ApplicationUser { Id = "2", FirstName = "Alice", LastName = "Smith", Email = "alice@test.com", UserRole = UserRole.Client },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com", UserRole = UserRole.Sponsor }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUsersByRoleAsync(UserRole.Client);

        // Assert
        Assert.Single(result);
        Assert.Equal("Alice", result.First().FirstName);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_WithNoMatchingRole_ReturnsEmptyList()
    {
        // Arrange
        var user = new ApplicationUser 
        { 
            Id = "1", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@test.com", 
            UserRole = UserRole.Member 
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUsersByRoleAsync(UserRole.Admin);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetUsersByStatusAsync Tests

    [Fact]
    public async Task GetUsersByStatusAsync_FiltersByStatus()
    {
        // Arrange
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", Status = MembershipStatus.Active },
            new ApplicationUser { Id = "2", FirstName = "Alice", LastName = "Smith", Email = "alice@test.com", Status = MembershipStatus.Pending },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com", Status = MembershipStatus.Active }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUsersByStatusAsync(MembershipStatus.Active);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var user = new ApplicationUser 
        { 
            Id = "test-id", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@test.com" 
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserByIdAsync("test-id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserByIdAsync("invalid-id");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SearchUsersAsync Tests

    [Fact]
    public async Task SearchUsersAsync_SearchesByFirstName()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "Alice", LastName = "Williams", Email = "alice@test.com", UserName = "alice@test.com" },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", UserName = "jane@test.com" },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Anderson", Email = "bob@test.com", UserName = "bob@test.com" }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SearchUsersAsync("Alice");

        // Assert
        Assert.Single(result);
        Assert.Equal("Alice", result.First().FirstName);
    }

    [Fact]
    public async Task SearchUsersAsync_SearchesByLastName()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserName = "john@test.com" },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", UserName = "jane@test.com" }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SearchUsersAsync("Smith");

        // Assert
        Assert.Single(result);
        Assert.Equal("Jane", result.First().FirstName);
    }

    [Fact]
    public async Task SearchUsersAsync_SearchesByEmail()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserName = "john@test.com" },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", UserName = "jane@example.com" }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SearchUsersAsync("example");

        // Assert
        Assert.Single(result);
        Assert.Equal("Jane", result.First().FirstName);
    }

    [Fact]
    public async Task SearchUsersAsync_IsCaseInsensitive()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var user = new ApplicationUser 
        { 
            Id = "1", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@test.com",
            UserName = "john@test.com"
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SearchUsersAsync("JOHN");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchUsersAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        
        var user = new ApplicationUser 
        { 
            Id = "1", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@test.com",
            UserName = "john@test.com"
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SearchUsersAsync("NonExistent");

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetSponsorsAsync Tests

    [Fact]
    public async Task GetSponsorsAsync_ReturnsOnlySponsors()
    {
        // Arrange
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserRole = UserRole.Sponsor },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", UserRole = UserRole.Client },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com", UserRole = UserRole.Sponsor }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetSponsorsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.Equal(UserRole.Sponsor, u.UserRole));
    }

    #endregion

    #region GetClientsAsync Tests

    [Fact]
    public async Task GetClientsAsync_ReturnsOnlyClients()
    {
        // Arrange
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserRole = UserRole.Client },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", UserRole = UserRole.Sponsor },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com", UserRole = UserRole.Client }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetClientsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.Equal(UserRole.Client, u.UserRole));
    }

    #endregion

    #region AssignSponsorAsync Tests

    [Fact]
    public async Task AssignSponsorAsync_WithValidIds_AssignsSponsor()
    {
        // Arrange
        var sponsor = new ApplicationUser 
        { 
            Id = "sponsor-id", 
            FirstName = "John", 
            LastName = "Sponsor", 
            Email = "sponsor@test.com",
            UserRole = UserRole.Sponsor 
        };
        var client = new ApplicationUser 
        { 
            Id = "client-id", 
            FirstName = "Jane", 
            LastName = "Client", 
            Email = "client@test.com",
            UserRole = UserRole.Client 
        };

        await _context.Users.AddRangeAsync(sponsor, client);
        await _context.SaveChangesAsync();

        // Act
        var (succeeded, errors) = await _service.AssignSponsorAsync("client-id", "sponsor-id");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        
        var updatedClient = await _context.Users.FindAsync("client-id");
        Assert.Equal("sponsor-id", updatedClient?.SponsorId);
    }

    [Fact]
    public async Task AssignSponsorAsync_WithInvalidClientId_ReturnsFalse()
    {
        // Arrange
        var sponsor = new ApplicationUser 
        { 
            Id = "sponsor-id", 
            FirstName = "John", 
            LastName = "Sponsor", 
            Email = "sponsor@test.com" 
        };

        await _context.Users.AddAsync(sponsor);
        await _context.SaveChangesAsync();

        // Act
        var (succeeded, errors) = await _service.AssignSponsorAsync("invalid-client-id", "sponsor-id");

        // Assert
        Assert.False(succeeded);
        Assert.NotEmpty(errors);
    }

    #endregion

    #region RemoveSponsorAsync Tests

    [Fact]
    public async Task RemoveSponsorAsync_WithValidClientId_RemovesSponsor()
    {
        // Arrange
        var sponsor = new ApplicationUser 
        { 
            Id = "sponsor-id", 
            FirstName = "John", 
            LastName = "Sponsor", 
            Email = "sponsor@test.com" 
        };
        var client = new ApplicationUser 
        { 
            Id = "client-id", 
            FirstName = "Jane", 
            LastName = "Client", 
            Email = "client@test.com",
            SponsorId = "sponsor-id"
        };

        await _context.Users.AddRangeAsync(sponsor, client);
        await _context.SaveChangesAsync();

        // Act
        var (succeeded, errors) = await _service.RemoveSponsorAsync("client-id");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        
        var updatedClient = await _context.Users.FindAsync("client-id");
        Assert.Null(updatedClient?.SponsorId);
    }

    [Fact]
    public async Task RemoveSponsorAsync_WithInvalidClientId_ReturnsFalse()
    {
        // Act
        var (succeeded, errors) = await _service.RemoveSponsorAsync("invalid-client-id");

        // Assert
        Assert.False(succeeded);
        Assert.NotEmpty(errors);
    }

    #endregion

    #region Count Methods Tests

    [Fact]
    public async Task GetTotalUsersCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com" }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GetTotalUsersCountAsync();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task GetActiveUsersCountAsync_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", IsActive = true, Status = MembershipStatus.Active },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", IsActive = false, Status = MembershipStatus.Active },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com", IsActive = true, Status = MembershipStatus.Inactive }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GetActiveUsersCountAsync();

        // Assert
        Assert.Equal(1, count); // Only user 1 is both active and has Active status
    }

    [Fact]
    public async Task GetPendingUsersCountAsync_ReturnsOnlyPendingUsers()
    {
        // Arrange
        var users = new[]
        {
            new ApplicationUser { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@test.com", Status = MembershipStatus.Pending },
            new ApplicationUser { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", Status = MembershipStatus.Active },
            new ApplicationUser { Id = "3", FirstName = "Bob", LastName = "Johnson", Email = "bob@test.com", Status = MembershipStatus.Pending }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GetPendingUsersCountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    #endregion

    #region UpdateUserStatusAsync Tests

    [Fact]
    public async Task UpdateUserStatusAsync_WithValidId_UpdatesStatus()
    {
        // Arrange
        var user = new ApplicationUser 
        { 
            Id = "test-id", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@test.com",
            Status = MembershipStatus.Pending
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var (succeeded, errors) = await _service.UpdateUserStatusAsync("test-id", MembershipStatus.Active);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        
        var updatedUser = await _context.Users.FindAsync("test-id");
        Assert.Equal(MembershipStatus.Active, updatedUser?.Status);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var (succeeded, errors) = await _service.UpdateUserStatusAsync("invalid-id", MembershipStatus.Active);

        // Assert
        Assert.False(succeeded);
        Assert.NotEmpty(errors);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
