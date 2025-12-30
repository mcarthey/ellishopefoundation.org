using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EllisHope.Tests.Controllers.Admin;

public class UsersControllerTests
{
    private readonly Mock<IUserManagementService> _mockUserService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly ApplicationDbContext _context;
    private readonly UsersController _controller;
    private readonly DefaultHttpContext _httpContext;
    private readonly ApplicationUser _testAdmin;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserManagementService>();
        _mockUserManager = MockHelpers.MockUserManager<ApplicationUser>();
        _mockMediaService = new Mock<IMediaService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _mockUrlHelper = new Mock<IUrlHelper>();

        // Create in-memory database for context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Create controller using standard constructor
        _controller = new UsersController(
            _mockUserService.Object,
            _mockUserManager.Object,
            _context,
            _mockMediaService.Object,
            _mockLogger.Object
        );

        // Setup HttpContext with real ServiceCollection for IUrlHelperFactory
        _httpContext = new DefaultHttpContext();

        // Use real ServiceCollection instead of mocking IServiceProvider
        var services = new ServiceCollection();
        var urlHelperFactory = new Mock<IUrlHelperFactory>();
        urlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(_mockUrlHelper.Object);
        services.AddSingleton<IUrlHelperFactory>(urlHelperFactory.Object);
        services.AddSingleton<IUrlHelper>(_mockUrlHelper.Object);
        var serviceProvider = services.BuildServiceProvider();

        _httpContext.RequestServices = serviceProvider;

        // Setup TempData
        var tempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        // Setup ControllerContext
        _controller.ControllerContext = new ControllerContext { HttpContext = _httpContext };

        // Create test admin user
        _testAdmin = new ApplicationUser
        {
            Id = "admin-1",
            Email = "admin@test.com",
            UserName = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            UserRole = UserRole.Admin
        };

        // Setup User identity (Admin)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testAdmin.Id),
            new Claim(ClaimTypes.Name, _testAdmin.Email!),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _httpContext.User = principal;
    }

    #region Index Action Tests

    [Fact]
    public async Task Index_ReturnsViewWithAllUsers_WhenNoFilters()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user-1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserRole = UserRole.Client, Status = MembershipStatus.Active, IsActive = true },
            new ApplicationUser { Id = "user-2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", UserRole = UserRole.Sponsor, Status = MembershipStatus.Active, IsActive = true }
        }.AsEnumerable();

        _mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);
        _mockUserService.Setup(s => s.GetTotalUsersCountAsync()).ReturnsAsync(2);
        _mockUserService.Setup(s => s.GetActiveUsersCountAsync()).ReturnsAsync(2);
        _mockUserService.Setup(s => s.GetPendingUsersCountAsync()).ReturnsAsync(0);

        // Act
        var result = await _controller.Index(null, null, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListViewModel>(viewResult.Model);
        Assert.Equal(2, model.Users.Count());
        Assert.Equal(2, model.TotalUsers);
        Assert.Equal(2, model.ActiveUsers);
    }

    [Fact]
    public async Task Index_FiltersUsersByRole()
    {
        // Arrange
        var allUsers = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user-1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserRole = UserRole.Client, Status = MembershipStatus.Active, IsActive = true },
            new ApplicationUser { Id = "user-2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", UserRole = UserRole.Sponsor, Status = MembershipStatus.Active, IsActive = true }
        }.AsEnumerable();

        _mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(allUsers);
        _mockUserService.Setup(s => s.GetTotalUsersCountAsync()).ReturnsAsync(2);
        _mockUserService.Setup(s => s.GetActiveUsersCountAsync()).ReturnsAsync(2);
        _mockUserService.Setup(s => s.GetPendingUsersCountAsync()).ReturnsAsync(0);

        // Act
        var result = await _controller.Index(null, UserRole.Client, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListViewModel>(viewResult.Model);
        Assert.Single(model.Users);
        Assert.Equal(UserRole.Client, model.Users.First().UserRole);
        Assert.Equal(UserRole.Client, model.RoleFilter);
    }

    [Fact]
    public async Task Index_FiltersUsersByStatus()
    {
        // Arrange
        var allUsers = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user-1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserRole = UserRole.Client, Status = MembershipStatus.Active, IsActive = true },
            new ApplicationUser { Id = "user-2", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", UserRole = UserRole.Client, Status = MembershipStatus.Pending, IsActive = false }
        }.AsEnumerable();

        _mockUserService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(allUsers);
        _mockUserService.Setup(s => s.GetTotalUsersCountAsync()).ReturnsAsync(2);
        _mockUserService.Setup(s => s.GetActiveUsersCountAsync()).ReturnsAsync(1);
        _mockUserService.Setup(s => s.GetPendingUsersCountAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.Index(null, null, MembershipStatus.Active, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListViewModel>(viewResult.Model);
        Assert.Single(model.Users);
        Assert.Equal(MembershipStatus.Active, model.Users.First().Status);
    }

    [Fact]
    public async Task Index_SearchesUsersByTerm()
    {
        // Arrange
        var searchResults = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user-1", FirstName = "John", LastName = "Doe", Email = "john@test.com", UserRole = UserRole.Client, Status = MembershipStatus.Active, IsActive = true }
        }.AsEnumerable();

        _mockUserService.Setup(s => s.SearchUsersAsync("john")).ReturnsAsync(searchResults);
        _mockUserService.Setup(s => s.GetTotalUsersCountAsync()).ReturnsAsync(1);
        _mockUserService.Setup(s => s.GetActiveUsersCountAsync()).ReturnsAsync(1);
        _mockUserService.Setup(s => s.GetPendingUsersCountAsync()).ReturnsAsync(0);

        // Act
        var result = await _controller.Index("john", null, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListViewModel>(viewResult.Model);
        Assert.Single(model.Users);
        Assert.Equal("john", model.SearchTerm);
        Assert.Contains("john", model.Users.First().Email, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Details Action Tests

    [Fact]
    public async Task Details_ReturnsViewWithUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true,
            SponsoredClients = new List<ApplicationUser>()
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Client" });

        // Act
        var result = await _controller.Details("user-1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserDetailsViewModel>(viewResult.Model);
        Assert.Equal("user-1", model.Id);
        Assert.Equal("John Doe", model.FullName);
        Assert.Equal("john@test.com", model.Email);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenUserIdIsNull()
    {
        // Act
        var result = await _controller.Details(null!);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserByIdAsync("nonexistent")).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Details("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Create Action Tests

    [Fact]
    public async Task Create_GET_ReturnsView()
    {
        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserCreateViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task Create_POST_CreatesUser_WithValidModel()
    {
        // Arrange
        var createModel = new UserCreateViewModel
        {
            FirstName = "New",
            LastName = "User",
            Email = "newuser@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Pending,
            IsActive = true
        };

        _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<ApplicationUser>(), createModel.Password))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Create(createModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Contains("created successfully", _controller.TempData["SuccessMessage"]?.ToString());
    }

    [Fact]
    public async Task Create_POST_ReturnsView_WhenModelStateInvalid()
    {
        // Arrange
        var createModel = new UserCreateViewModel
        {
            FirstName = "",  // Invalid
            LastName = "User",
            Email = "newuser@test.com",
            Password = "Test@123456"
        };

        _controller.ModelState.AddModelError("FirstName", "First name is required");

        // Act
        var result = await _controller.Create(createModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_POST_ReturnsView_WhenUserCreationFails()
    {
        // Arrange
        var createModel = new UserCreateViewModel
        {
            FirstName = "New",
            LastName = "User",
            Email = "existing@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            UserRole = UserRole.Client
        };

        _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<ApplicationUser>(), createModel.Password))
            .ReturnsAsync((false, new[] { "Email already exists" }));

        // Act
        var result = await _controller.Create(createModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Contains("Email already exists", _controller.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task Create_POST_CreatesUserWithAllRoles()
    {
        // Test creating users with different roles
        await TestCreateUserWithRole(UserRole.Admin);
        await TestCreateUserWithRole(UserRole.BoardMember);
        await TestCreateUserWithRole(UserRole.Client);
        await TestCreateUserWithRole(UserRole.Sponsor);
        await TestCreateUserWithRole(UserRole.Member);
    }

    private async Task TestCreateUserWithRole(UserRole role)
    {
        var createModel = new UserCreateViewModel
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"{role.ToString().ToLower()}@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            UserRole = role,
            Status = MembershipStatus.Active,
            IsActive = true,
            // Board members require a profile photo - provide an avatar URL
            SelectedAvatarUrl = role == UserRole.BoardMember ? "/assets/img/avatars/avatar-01.png" : null
        };

        _mockUserService.Setup(s => s.CreateUserAsync(It.Is<ApplicationUser>(u => u.UserRole == role), createModel.Password))
            .ReturnsAsync((true, Array.Empty<string>()));

        var result = await _controller.Create(createModel);

        Assert.IsType<RedirectToActionResult>(result);
    }

    #endregion

    #region Edit Action Tests

    [Fact]
    public async Task Edit_GET_ReturnsViewWithUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true
        };

        var sponsors = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "sponsor-1", FirstName = "Sponsor", LastName = "One", UserRole = UserRole.Sponsor }
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.GetSponsorsAsync()).ReturnsAsync(sponsors);

        // Act
        var result = await _controller.Edit("user-1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserEditViewModel>(viewResult.Model);
        Assert.Equal("user-1", model.Id);
        Assert.Equal("John", model.FirstName);
        Assert.Single(model.AvailableSponsors);
    }

    [Fact]
    public async Task Edit_GET_ReturnsNotFound_WhenUserIdIsNull()
    {
        // Act
        var result = await _controller.Edit(null!);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_GET_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserByIdAsync("nonexistent")).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Edit("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_UpdatesUser_WithValidModel()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true
        };

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John Updated",
            LastName = "Doe Updated",
            Email = "john.updated@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal("user-1", redirectResult.RouteValues!["id"]);
        Assert.Contains("updated successfully", _controller.TempData["SuccessMessage"]?.ToString());
    }

    [Fact]
    public async Task Edit_POST_ReturnsNotFound_WhenIdMismatch()
    {
        // Arrange
        var editModel = new UserEditViewModel
        {
            Id = "user-2",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_UpdatesUserRole_WhenRoleChanged()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active
        };

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Sponsor,  // Changed role
            Status = MembershipStatus.Active,
            IsActive = true
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.UpdateUserRoleAsync("user-1", UserRole.Sponsor))
            .ReturnsAsync((true, Array.Empty<string>()));
        _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        _mockUserService.Verify(s => s.UpdateUserRoleAsync("user-1", UserRole.Sponsor), Times.Once);
    }

    [Fact]
    public async Task Edit_POST_AssignsSponsor_WhenSponsorChanged()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            SponsorId = null
        };

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            SponsorId = "sponsor-1",  // Assign sponsor
            Status = MembershipStatus.Active,
            IsActive = true
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.AssignSponsorAsync("user-1", "sponsor-1"))
            .Returns(Task.FromResult((true, Array.Empty<string>())));
        _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        _mockUserService.Verify(s => s.AssignSponsorAsync("user-1", "sponsor-1"), Times.Once);
    }

    [Fact]
    public async Task Edit_POST_RemovesSponsor_WhenSponsorCleared()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            SponsorId = "sponsor-1"
        };

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            SponsorId = null,  // Remove sponsor
            Status = MembershipStatus.Active,
            IsActive = true
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.RemoveSponsorAsync("user-1"))
            .Returns(Task.FromResult((true, Array.Empty<string>())));
        _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        _mockUserService.Verify(s => s.RemoveSponsorAsync("user-1"), Times.Once);
    }

    #endregion

    #region Delete Action Tests

    [Fact]
    public async Task Delete_GET_ReturnsViewWithUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            SponsoredClients = new List<ApplicationUser>()
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);

        // Act
        var result = await _controller.Delete("user-1");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserDeleteViewModel>(viewResult.Model);
        Assert.Equal("user-1", model.Id);
        Assert.Equal("John Doe", model.FullName);
        Assert.Equal(0, model.SponsoredClientsCount);
    }

    [Fact]
    public async Task Delete_GET_ReturnsNotFound_WhenUserIdIsNull()
    {
        // Act
        var result = await _controller.Delete(null!);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_GET_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserByIdAsync("nonexistent")).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Delete("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_POST_DeletesUser_Successfully()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeleteUserAsync("user-1"))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.DeleteConfirmed("user-1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("User deleted successfully!", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task DeleteConfirmed_POST_ReturnsError_WhenDeletionFails()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeleteUserAsync("user-1"))
            .ReturnsAsync((false, new[] { "Cannot delete user with active sponsored clients" }));

        // Act
        var result = await _controller.DeleteConfirmed("user-1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Delete), redirectResult.ActionName);
        Assert.Equal("user-1", redirectResult.RouteValues!["id"]);
        Assert.Contains("Cannot delete user", _controller.TempData["ErrorMessage"]?.ToString());
    }

    [Fact]
    public async Task DeleteConfirmed_POST_PreventsDeletionOfUserWithSponsoredClients()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeleteUserAsync("sponsor-1"))
            .ReturnsAsync((false, new[] { "Cannot delete sponsor with active clients. Please reassign clients first." }));

        // Act
        var result = await _controller.DeleteConfirmed("sponsor-1");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("reassign clients", _controller.TempData["ErrorMessage"]?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Board Member Photo Requirement Tests

    [Fact]
    public async Task Create_POST_RequiresPhoto_ForBoardMember_WithoutPhoto()
    {
        // Arrange
        var createModel = new UserCreateViewModel
        {
            FirstName = "Board",
            LastName = "Member",
            Email = "boardmember@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            UserRole = UserRole.BoardMember,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = null,
            SelectedAvatarUrl = null
        };

        // Act
        var result = await _controller.Create(createModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey("ProfilePhoto"));
        Assert.Contains("Board members must have a profile photo",
            _controller.ModelState["ProfilePhoto"]!.Errors.First().ErrorMessage);
    }

    [Fact]
    public async Task Create_POST_Succeeds_ForBoardMember_WithPhotoUpload()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("profile.jpg");

        var createModel = new UserCreateViewModel
        {
            FirstName = "Board",
            LastName = "Member",
            Email = "boardmember@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            UserRole = UserRole.BoardMember,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = fileMock.Object,
            SelectedAvatarUrl = null
        };

        var uploadedMedia = new Media
        {
            Id = 1,
            FilePath = "/uploads/media/profile.jpg",
            FileName = "profile.jpg"
        };

        _mockMediaService.Setup(ms => ms.UploadLocalImageAsync(
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            MediaCategory.Team,
            "profile,team",
            It.IsAny<string>()))
            .ReturnsAsync(uploadedMedia);

        _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<ApplicationUser>(), createModel.Password))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Create(createModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockMediaService.Verify(ms => ms.UploadLocalImageAsync(
            fileMock.Object,
            "Board Member profile photo",
            "Board Member",
            MediaCategory.Team,
            "profile,team",
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Create_POST_Succeeds_ForBoardMember_WithAvatarSelection()
    {
        // Arrange
        var createModel = new UserCreateViewModel
        {
            FirstName = "Board",
            LastName = "Member",
            Email = "boardmember@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            UserRole = UserRole.BoardMember,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = null,
            SelectedAvatarUrl = "/assets/img/avatars/avatar-01.png"
        };

        _mockUserService.Setup(s => s.CreateUserAsync(It.Is<ApplicationUser>(u =>
            u.ProfilePictureUrl == "/assets/img/avatars/avatar-01.png"), createModel.Password))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Create(createModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockUserService.Verify(s => s.CreateUserAsync(
            It.Is<ApplicationUser>(u => u.ProfilePictureUrl == "/assets/img/avatars/avatar-01.png"),
            createModel.Password), Times.Once);
    }

    [Fact]
    public async Task Create_POST_DoesNotRequirePhoto_ForNonBoardMember()
    {
        // Arrange
        var createModel = new UserCreateViewModel
        {
            FirstName = "Regular",
            LastName = "Member",
            Email = "member@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            UserRole = UserRole.Member,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = null,
            SelectedAvatarUrl = null
        };

        _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<ApplicationUser>(), createModel.Password))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Create(createModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_POST_UploadsProfilePhoto_WhenPhotoProvided()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true
        };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("profile.jpg");

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = fileMock.Object
        };

        var uploadedMedia = new Media
        {
            Id = 1,
            FilePath = "/uploads/media/profile.jpg",
            FileName = "profile.jpg"
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockMediaService.Setup(ms => ms.UploadLocalImageAsync(
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            MediaCategory.Team,
            "profile,team",
            It.IsAny<string>()))
            .ReturnsAsync(uploadedMedia);
        _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("/uploads/media/profile.jpg", user.ProfilePictureUrl);
    }

    [Fact]
    public async Task Edit_POST_SelectsAvatar_WhenAvatarSelected()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePictureUrl = "/old/photo.jpg"
        };

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = null,
            SelectedAvatarUrl = "/assets/img/avatars/avatar-05.png"
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("/assets/img/avatars/avatar-05.png", user.ProfilePictureUrl);
    }

    [Fact]
    public async Task Edit_POST_RemovesProfilePhoto_WhenRemoveRequested()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePictureUrl = "/uploads/media/existing-photo.jpg"
        };

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = null,
            SelectedAvatarUrl = "__REMOVE__"  // Sentinel value for removal
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Null(user.ProfilePictureUrl);
    }

    [Fact]
    public async Task Edit_POST_ReturnsView_WhenPhotoUploadFails()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true
        };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("profile.jpg");

        var editModel = new UserEditViewModel
        {
            Id = "user-1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Client,
            Status = MembershipStatus.Active,
            IsActive = true,
            ProfilePhoto = fileMock.Object
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _mockUserService.Setup(s => s.GetSponsorsAsync()).ReturnsAsync(new List<ApplicationUser>());
        _mockMediaService.Setup(ms => ms.UploadLocalImageAsync(
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<MediaCategory>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ThrowsAsync(new Exception("Upload failed"));

        // Act
        var result = await _controller.Edit("user-1", editModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey("ProfilePhoto"));
    }

    #endregion
}

