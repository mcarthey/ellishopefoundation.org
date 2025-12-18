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
/// Unit tests for Draft Application Save functionality
/// Tests the critical "Save & Exit" feature that allows partial saves
/// </summary>
public class MyApplicationsDraftSaveTests
{
    private readonly Mock<IClientApplicationService> _mockApplicationService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<MyApplicationsController>> _mockLogger;
    private readonly MyApplicationsController _controller;
    private readonly ApplicationUser _testUser;

    public MyApplicationsDraftSaveTests()
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

        var httpContext = new DefaultHttpContext();
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

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

    #region Save & Exit from Step 1 Tests

    [Fact]
    public async Task SaveAndExit_FromStep1_OnlyUpdatesStep1Fields()
    {
        // With hidden fields, ALL data is posted in the model
        var existingApplication = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Draft,
            FirstName = "Original",
            LastName = "User",
            Email = "original@test.com",
            PhoneNumber = "555-0000",
            // Step 2 data
            FundingTypesRequested = "GymMembership",
            EstimatedMonthlyCost = 100,
            // Step 3 data
            PersonalStatement = "Original statement that is long enough to pass validation requirements for this field.",
            ExpectedBenefits = "Original benefits that are also long enough to meet the minimum requirements.",
            CommitmentStatement = "Original commitment that meets the minimum character requirement."
        };

        // User edits Step 1, but hidden fields carry Step 2-3 data
        var editModel = new ApplicationEditViewModel
        {
            Id = 1,
            CurrentStep = 1,
            // Step 1 - updated
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = "555-1234",
            Email = "updated@test.com",
            // Step 2-3 - preserved via hidden fields
            FundingTypesRequested = new List<FundingType> { FundingType.GymMembership },
            EstimatedMonthlyCost = 100,
            PersonalStatement = "Original statement that is long enough to pass validation requirements for this field.",
            ExpectedBenefits = "Original benefits that are also long enough to meet the minimum requirements.",
            CommitmentStatement = "Original commitment that meets the minimum character requirement."
        };

        ClientApplication? capturedApplication = null;

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(existingApplication);

        _mockApplicationService.Setup(s => s.UpdateApplicationAsync(It.IsAny<ClientApplication>()))
            .Callback<ClientApplication>(app => capturedApplication = app)
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>()));

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "SaveAndExit", "true" }
        });
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;

        // Act
        await _controller.Edit(1, editModel, null);

        // Assert
        Assert.NotNull(capturedApplication);
        
        // Step 1 fields should be updated
        Assert.Equal("Updated", capturedApplication.FirstName);
        Assert.Equal("Name", capturedApplication.LastName);
        Assert.Equal("555-1234", capturedApplication.PhoneNumber);
        Assert.Equal("updated@test.com", capturedApplication.Email);
        
        // Step 2-3 fields preserved via hidden fields
        Assert.Equal("GymMembership", capturedApplication.FundingTypesRequested);
        Assert.Equal(100, capturedApplication.EstimatedMonthlyCost);
        Assert.Equal("Original statement that is long enough to pass validation requirements for this field.", 
            capturedApplication.PersonalStatement);
        Assert.Equal("Original benefits that are also long enough to meet the minimum requirements.", 
            capturedApplication.ExpectedBenefits);
    }

    #endregion

    #region Save & Exit from Step 2 Tests

    [Fact]
    public async Task SaveAndExit_FromStep2_PreservesStep1RequiredFields()
    {
        // Arrange - This is the bug scenario!
        // With hidden fields, model should have all data from all steps
        var existingApplication = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Draft,
            // Step 1 - required fields
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com", // REQUIRED - must not be overwritten!
            PhoneNumber = "555-0000"
        };

        // User is on Step 2, but hidden fields carry Step 1 data
        var editModel = new ApplicationEditViewModel
        {
            Id = 1,
            CurrentStep = 2,
            // Step 1 fields - carried via hidden fields!
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com", // Hidden field preserves this!
            PhoneNumber = "555-0000",
            // Step 2 fields
            FundingTypesRequested = new List<FundingType> { FundingType.GymMembership },
            EstimatedMonthlyCost = 150
        };

        ClientApplication? capturedApplication = null;

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(existingApplication);

        _mockApplicationService.Setup(s => s.UpdateApplicationAsync(It.IsAny<ClientApplication>()))
            .Callback<ClientApplication>(app => capturedApplication = app)
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>()));

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "SaveAndExit", "true" }
        });
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;

        // Act
        await _controller.Edit(1, editModel, null);

        // Assert
        Assert.NotNull(capturedApplication);
        
        // Step 1 required fields must be preserved (via hidden fields)
        Assert.Equal("Test", capturedApplication.FirstName);
        Assert.Equal("User", capturedApplication.LastName);
        Assert.Equal("test@test.com", capturedApplication.Email); // THIS IS THE KEY TEST!
        Assert.Equal("555-0000", capturedApplication.PhoneNumber);
        
        // Step 2 fields should be updated
        Assert.Contains("GymMembership", capturedApplication.FundingTypesRequested);
        Assert.Equal(150, capturedApplication.EstimatedMonthlyCost);
    }

    #endregion

    #region Save & Exit from Step 3 Tests

    [Fact]
    public async Task SaveAndExit_FromStep3_PreservesStep1And2Data()
    {
        // Arrange
        var existingApplication = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Draft,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "555-0000",
            FundingTypesRequested = "GymMembership",
            EstimatedMonthlyCost = 100
        };

        // Model has ALL data (via hidden fields)
        var editModel = new ApplicationEditViewModel
        {
            Id = 1,
            CurrentStep = 3,
            // Step 1-2 preserved via hidden fields
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "555-0000",
            FundingTypesRequested = new List<FundingType> { FundingType.GymMembership },
            EstimatedMonthlyCost = 100,
            // Step 3 data - new
            PersonalStatement = "New statement that is definitely long enough to meet requirements.",
            ExpectedBenefits = "New benefits that are also long enough for this field.",
            CommitmentStatement = "New commitment statement meeting the character requirement."
        };

        ClientApplication? capturedApplication = null;

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(existingApplication);

        _mockApplicationService.Setup(s => s.UpdateApplicationAsync(It.IsAny<ClientApplication>()))
            .Callback<ClientApplication>(app => capturedApplication = app)
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>()));

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "SaveAndExit", "true" }
        });
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;

        // Act
        await _controller.Edit(1, editModel, null);

        // Assert
        Assert.NotNull(capturedApplication);
        
        // Step 1-2 data preserved
        Assert.Equal("test@test.com", capturedApplication.Email);
        Assert.Equal("GymMembership", capturedApplication.FundingTypesRequested);
        
        // Step 3 data updated
        Assert.Equal("New statement that is definitely long enough to meet requirements.", 
            capturedApplication.PersonalStatement);
    }

    #endregion

    #region Full Update vs Partial Update Tests

    [Fact]
    public async Task Edit_FinalStepSubmit_UsesFullUpdate()
    {
        // When submitting the final step (not SaveAndExit), all fields should be validated and updated
        var existingApplication = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Draft,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "555-0000"
        };

        var editModel = new ApplicationEditViewModel
        {
            Id = 1,
            CurrentStep = 6,
            FirstName = "Final",
            LastName = "Update",
            Email = "final@test.com",
            PhoneNumber = "555-9999",
            FundingTypesRequested = new List<FundingType> { FundingType.GymMembership },
            PersonalStatement = "Complete personal statement meeting all requirements.",
            ExpectedBenefits = "Complete benefits statement meeting all requirements.",
            CommitmentStatement = "Complete commitment statement meeting requirements.",
            UnderstandsCommitment = true,
            Signature = "Final Update"
        };

        ClientApplication? capturedApplication = null;

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(existingApplication);

        _mockApplicationService.Setup(s => s.UpdateApplicationAsync(It.IsAny<ClientApplication>()))
            .Callback<ClientApplication>(app => capturedApplication = app)
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>()));

        // No SaveAndExit - regular submit
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;

        // Act
        await _controller.Edit(1, editModel, null);

        // Assert
        Assert.NotNull(capturedApplication);
        Assert.Equal("Final", capturedApplication.FirstName);
        Assert.Equal("final@test.com", capturedApplication.Email);
        Assert.Equal("Final Update", capturedApplication.Signature);
    }

    #endregion

    #region Save & Exit Success Message Tests

    [Fact]
    public async Task SaveAndExit_ShowsSuccessMessage()
    {
        // Arrange
        var existingApplication = new ClientApplication
        {
            Id = 1,
            ApplicantId = _testUser.Id,
            Status = ApplicationStatus.Draft,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "555-0000"
        };

        var editModel = new ApplicationEditViewModel
        {
            Id = 1,
            CurrentStep = 1,
            FirstName = "Updated",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "555-1234"
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, false, false))
            .ReturnsAsync(existingApplication);

        _mockApplicationService.Setup(s => s.UpdateApplicationAsync(It.IsAny<ClientApplication>()))
            .ReturnsAsync((Succeeded: true, Errors: Array.Empty<string>()));

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "SaveAndExit", "true" }
        });
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;

        // Act
        var result = await _controller.Edit(1, editModel, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("Draft saved successfully.", _controller.TempData["SuccessMessage"]);
    }

    #endregion
}
