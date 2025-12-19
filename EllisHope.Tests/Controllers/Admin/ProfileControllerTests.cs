using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EllisHope.Tests.Controllers.Admin;

public class ProfileControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<ILogger<ProfileController>> _mockLogger;
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            contextAccessor.Object,
            userPrincipalFactory.Object,
            null, null, null, null);

        _mockLogger = new Mock<ILogger<ProfileController>>();

        _controller = new ProfileController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockLogger.Object
        );

        // Setup HttpContext and TempData
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "testuser@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    #region Index Action Tests

    [Fact]
    public async Task Index_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ReturnsViewWithProfile_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "555-1234",
            UserRole = UserRole.Client,
            Status = UserStatus.Active,
            JoinedDate = DateTime.UtcNow.AddDays(-30)
        };

        var roles = new List<string> { "Client" };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(viewResult.Model);
        Assert.Equal(user.Id, model.Id);
        Assert.Equal("John Doe", model.FullName);
        Assert.Equal(user.Email, model.Email);
        Assert.Single(model.Roles);
    }

    #endregion

    #region Edit GET Action Tests

    [Fact]
    public async Task Edit_GET_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Edit();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_GET_ReturnsViewWithModel_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "555-1234",
            Address = "123 Main St",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701"
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Edit();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditProfileViewModel>(viewResult.Model);
        Assert.Equal(user.FirstName, model.FirstName);
        Assert.Equal(user.LastName, model.LastName);
        Assert.Equal(user.Email, model.Email);
    }

    #endregion

    #region Edit POST Action Tests

    [Fact]
    public async Task Edit_POST_ReturnsView_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("FirstName", "Required");
        var model = new EditProfileViewModel();

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task Edit_POST_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var model = new EditProfileViewModel { FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_POST_UpdatesUser_WithValidModel()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };

        var model = new EditProfileViewModel
        {
            Id = "user-123",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "john@test.com",
            PhoneNumber = "555-5678",
            Address = "456 Elm St",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601"
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockUserManager.Verify(um => um.UpdateAsync(It.Is<ApplicationUser>(u =>
            u.FirstName == "Jane" &&
            u.LastName == "Smith" &&
            u.PhoneNumber == "555-5678"
        )), Times.Once);
    }

    [Fact]
    public async Task Edit_POST_UpdatesEmail_WhenEmailChanged()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "old@test.com",
            UserName = "old@test.com"
        };

        var model = new EditProfileViewModel
        {
            Id = "user-123",
            FirstName = "John",
            LastName = "Doe",
            Email = "new@test.com"
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.SetEmailAsync(user, "new@test.com"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.SetUserNameAsync(user, "new@test.com"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockUserManager.Verify(um => um.SetEmailAsync(user, "new@test.com"), Times.Once);
        _mockUserManager.Verify(um => um.SetUserNameAsync(user, "new@test.com"), Times.Once);
    }

    [Fact]
    public async Task Edit_POST_ReturnsView_WhenEmailUpdateFails()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123", Email = "old@test.com" };
        var model = new EditProfileViewModel { Id = "user-123", FirstName = "John", LastName = "Doe", Email = "new@test.com" };

        var errors = new[]
        {
            new IdentityError { Description = "Email already in use" }
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.SetEmailAsync(user, "new@test.com"))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Edit_POST_SetsTempDataSuccess_OnSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123", Email = "test@test.com" };
        var model = new EditProfileViewModel { Id = "user-123", FirstName = "John", LastName = "Doe", Email = "test@test.com" };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _controller.Edit(model);

        // Assert
        Assert.Contains("updated", _controller.TempData["SuccessMessage"]?.ToString());
    }

    #endregion

    #region ChangePassword GET Action Tests

    [Fact]
    public void ChangePassword_GET_ReturnsView()
    {
        // Act
        var result = _controller.ChangePassword();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region ChangePassword POST Action Tests

    [Fact]
    public async Task ChangePassword_POST_ReturnsView_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("CurrentPassword", "Required");
        var model = new ChangePasswordViewModel();

        // Act
        var result = await _controller.ChangePassword(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task ChangePassword_POST_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var model = new ChangePasswordViewModel
        {
            CurrentPassword = "Old123!",
            NewPassword = "New123!",
            ConfirmPassword = "New123!"
        };
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.ChangePassword(model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ChangePassword_POST_ChangesPassword_WithValidModel()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123" };
        var model = new ChangePasswordViewModel
        {
            CurrentPassword = "Old123!",
            NewPassword = "New123!",
            ConfirmPassword = "New123!"
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.ChangePasswordAsync(user, "Old123!", "New123!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockSignInManager.Setup(sm => sm.RefreshSignInAsync(user))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ChangePassword(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        _mockUserManager.Verify(um => um.ChangePasswordAsync(user, "Old123!", "New123!"), Times.Once);
        _mockSignInManager.Verify(sm => sm.RefreshSignInAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_POST_ReturnsView_WhenPasswordChangeFails()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123" };
        var model = new ChangePasswordViewModel
        {
            CurrentPassword = "Wrong123!",
            NewPassword = "New123!",
            ConfirmPassword = "New123!"
        };

        var errors = new[] { new IdentityError { Description = "Incorrect password" } };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.ChangePasswordAsync(user, "Wrong123!", "New123!"))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.ChangePassword(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task ChangePassword_POST_SetsTempDataSuccess_OnSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123" };
        var model = new ChangePasswordViewModel
        {
            CurrentPassword = "Old123!",
            NewPassword = "New123!",
            ConfirmPassword = "New123!"
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.ChangePasswordAsync(user, "Old123!", "New123!"))
            .ReturnsAsync(IdentityResult.Success);
        _mockSignInManager.Setup(sm => sm.RefreshSignInAsync(user))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.ChangePassword(model);

        // Assert
        Assert.Contains("changed", _controller.TempData["SuccessMessage"]?.ToString());
    }

    #endregion
}
