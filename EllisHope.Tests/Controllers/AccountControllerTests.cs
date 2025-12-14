using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<SignInManager<IdentityUser>> _mockSignInManager;
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<ILogger<AccountController>> _mockLogger;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        // Setup UserManager mock
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup SignInManager mock
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _mockSignInManager = new Mock<SignInManager<IdentityUser>>(
            _mockUserManager.Object,
            contextAccessor.Object,
            claimsPrincipalFactory.Object,
            null!, null!, null!, null!);

        _mockLogger = new Mock<ILogger<AccountController>>();

        _controller = new AccountController(
            _mockSignInManager.Object,
            _mockUserManager.Object,
            _mockLogger.Object);

        // Setup TempData
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    #region Login GET Action Tests

    [Fact]
    public void Login_Get_ReturnsView()
    {
        // Act
        var result = _controller.Login();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Login_Get_SetsReturnUrlInViewData()
    {
        // Arrange
        var returnUrl = "/admin/pages";

        // Act
        var result = _controller.Login(returnUrl);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(returnUrl, _controller.ViewData["ReturnUrl"]);
    }

    #endregion

    #region Login POST Action Tests

    [Fact]
    public async Task Login_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        var model = new LoginViewModel();
        _controller.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _controller.Login(model, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<LoginViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_AdminUser_RedirectsToDashboard()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Email = "admin@test.com",
            Password = "Password123!",
            RememberMe = false
        };

        var user = new IdentityUser { Email = "admin@test.com", UserName = "admin@test.com" };

        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _mockUserManager.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(u => u.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Login(model, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Dashboard", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_WithReturnUrl_RedirectsToReturnUrl()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Email = "admin@test.com",
            Password = "Password123!",
            RememberMe = false
        };

        var user = new IdentityUser { Email = "admin@test.com", UserName = "admin@test.com" };
        var returnUrl = "/admin/pages";

        // Setup URL helper
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);
        _controller.Url = urlHelperMock.Object;

        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _mockUserManager.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(u => u.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Login(model, returnUrl);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(returnUrl, redirectResult.Url);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_NotAdminUser_SignsOutAndReturnsError()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Email = "user@test.com",
            Password = "Password123!",
            RememberMe = false
        };

        var user = new IdentityUser { Email = "user@test.com", UserName = "user@test.com" };

        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _mockUserManager.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(u => u.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(false);

        _mockSignInManager.Setup(s => s.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Login(model, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task Login_Post_LockedOut_ReturnsLockoutView()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Email = "locked@test.com",
            Password = "Password123!",
            RememberMe = false
        };

        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        // Act
        var result = await _controller.Login(model, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Lockout", viewResult.ViewName);
    }

    [Fact]
    public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Email = "wrong@test.com",
            Password = "WrongPassword!",
            RememberMe = false
        };

        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _controller.Login(model, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region Logout Action Tests

    [Fact]
    public async Task Logout_SignsOutUser_RedirectsToHome()
    {
        // Arrange
        _mockSignInManager.Setup(s => s.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
        Assert.Equal("", redirectResult.RouteValues!["area"]);
        _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
    }

    #endregion

    #region AccessDenied Action Tests

    [Fact]
    public void AccessDenied_ReturnsView()
    {
        // Act
        var result = _controller.AccessDenied();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region Lockout Action Tests

    [Fact]
    public void Lockout_ReturnsView()
    {
        // Act
        var result = _controller.Lockout();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    #endregion
}
