using EllisHope.Areas.Admin.Models;
using EllisHope.Controllers;
using EllisHope.Models.Domain;
using EllisHope.Services;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

/// <summary>
/// Unit tests for MyApplicationsController
/// Tests user-facing application management (view drafts, edit, submit)
/// </summary>
public class MyApplicationsControllerTests
{
    private readonly Mock<IClientApplicationService> _mockApplicationService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<MyApplicationsController>> _mockLogger;
    private readonly MyApplicationsController _controller;
    private readonly ApplicationUser _testUser;

    public MyApplicationsControllerTests()
    {
        _mockApplicationService = new Mock<IClientApplicationService>();
        
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _mockLogger = new Mock<ILogger<MyApplicationsController>>();

        _controller = new MyApplicationsController(
            _mockApplicationService.Object,
            _mockUserManager.Object,
            _mockLogger.Object);

        // Setup TempData
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());

        // Setup test user
        _testUser = new ApplicationUser
        {
            Id = "user123",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(_testUser);
    }

    #region Index Tests

    [Fact]
    public async Task Index_ReturnsViewWithApplications()
    {
        // Arrange
        var applications = new List<ClientApplication>
        {
            new ClientApplication { Id = 1, ApplicantId = _testUser.Id, Status = ApplicationStatus.Draft },
            new ClientApplication { Id = 2, ApplicantId = _testUser.Id, Status = ApplicationStatus.Submitted }
        };

        _mockApplicationService.Setup(s => s.GetApplicationsByApplicantAsync(_testUser.Id))
            .ReturnsAsync(applications);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyApplicationsViewModel>(viewResult.Model);
        Assert.Equal(2, model.Applications.Count());
    }

    [Fact]
    public async Task Index_WhenUserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    #endregion

    #region Details Tests

    [Fact]
    public async Task Details_WithValidId_ReturnsView()
    {
        // Arrange
        var application = new ClientApplication 
        { 
            Id = 1, 
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Submitted
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false))
            .ReturnsAsync(application);

        _mockApplicationService.Setup(s => s.GetApplicationCommentsAsync(1, false))
            .ReturnsAsync(new List<ApplicationComment>());

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationDetailsViewModel>(viewResult.Model);
        Assert.Equal(1, model.Application.Id);
    }

    [Fact]
    public async Task Details_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(999, false))
            .ReturnsAsync((ClientApplication)null!);

        // Act
        var result = await _controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_OtherUsersApplication_ReturnsForbidden()
    {
        // Arrange
        var application = new ClientApplication 
        { 
            Id = 1, 
            ApplicantId = "other-user-id", // Different user
            Status = ApplicationStatus.Submitted
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false))
            .ReturnsAsync(application);

        // Act
        var result = await _controller.Details(1);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    #endregion

    #region Edit Tests

    [Fact]
    public async Task Edit_Get_WithDraftApplication_ReturnsView()
    {
        // Arrange
        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Draft,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com"
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(application);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationEditViewModel>(viewResult.Model);
        Assert.Equal(1, model.Id);
        Assert.Equal("Test", model.FirstName);
    }

    [Fact]
    public async Task Edit_Get_WithSubmittedApplication_RedirectsWithError()
    {
        // Arrange
        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Submitted // Not a draft!
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(application);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal("Only draft applications can be edited.", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Edit_Get_OtherUsersApplication_ReturnsForbidden()
    {
        // Arrange
        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = "other-user-id", // Different user
            Status = ApplicationStatus.Draft
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(application);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidData_UpdatesApplication()
    {
        // Arrange
        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Draft
        };

        var model = new ApplicationEditViewModel
        {
            Id = 1,
            CurrentStep = 6, // Final step
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@test.com",
            PhoneNumber = "555-1234",
            FundingTypesRequested = new List<FundingType> { FundingType.GymMembership },
            PersonalStatement = "This is a test personal statement that is long enough to pass validation requirements.",
            ExpectedBenefits = "This is a test expected benefits statement that is long enough to meet requirements.",
            CommitmentStatement = "This is a test commitment statement that is long enough for validation.",
            UnderstandsCommitment = true,
            Signature = "Updated User"
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(application);

        _mockApplicationService.Setup(s => s.UpdateApplicationAsync(It.IsAny<ClientApplication>()))
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>()));

        // Act
        var result = await _controller.Edit(1, model, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal("Application updated successfully.", _controller.TempData["SuccessMessage"]);
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_Get_ReturnsView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationCreateViewModel>(viewResult.Model);
        Assert.Equal(1, model.CurrentStep);
    }

    [Fact]
    public async Task Create_Post_SaveAsDraft_CreatesDraft()
    {
        // Arrange
        var model = new ApplicationCreateViewModel
        {
            CurrentStep = 1,
            SaveAsDraft = true,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "555-1234"
        };

        _mockApplicationService.Setup(s => s.CreateApplicationAsync(It.IsAny<ClientApplication>()))
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>(), Application: new ClientApplication { Id = 1 }));

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("Application saved as draft.", _controller.TempData["SuccessMessage"]);
    }

    #endregion

    #region Withdraw Tests

    [Fact]
    public async Task Withdraw_ValidApplication_WithdrawsSuccessfully()
    {
        // Arrange
        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Submitted
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(application);

        _mockApplicationService.Setup(s => s.WithdrawApplicationAsync(1, _testUser.Id, It.IsAny<string>()))
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>()));

        // Act
        var result = await _controller.Withdraw(1, "Changed my mind");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("Application withdrawn.", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Withdraw_OtherUsersApplication_ReturnsForbidden()
    {
        // Arrange
        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = "other-user-id",
            Status = ApplicationStatus.Submitted
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(application);

        // Act
        var result = await _controller.Withdraw(1, "test");

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    #endregion
}
