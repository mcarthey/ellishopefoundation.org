using EllisHope.Controllers;
using EllisHope.Models.ViewModels;
using EllisHope.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class ContactControllerTests
{
    private readonly Mock<IRecaptchaService> _recaptchaServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ContactController>> _loggerMock;
    private readonly IOptions<ContactFormSettings> _contactSettings;
    private readonly IOptions<RecaptchaSettings> _recaptchaSettings;
    private readonly ContactController _controller;

    public ContactControllerTests()
    {
        _recaptchaServiceMock = new Mock<IRecaptchaService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ContactController>>();
        _contactSettings = Options.Create(new ContactFormSettings
        {
            RecipientEmail = "admin@test.com",
            SubjectPrefix = "[Test Contact]"
        });
        _recaptchaSettings = Options.Create(new RecaptchaSettings
        {
            SiteKey = "test-site-key",
            SecretKey = "test-secret-key",
            MinimumScore = 0.5f
        });

        _controller = new ContactController(
            _recaptchaServiceMock.Object,
            _emailServiceMock.Object,
            _contactSettings,
            _recaptchaSettings,
            _loggerMock.Object);

        // Setup ViewData/TempData for controller
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.TempData = new TempDataDictionary(
            _controller.ControllerContext.HttpContext,
            Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public void Index_Get_ReturnsViewResultWithEmptyModel()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ContactFormViewModel>(viewResult.Model);
        Assert.Equal(string.Empty, model.Name);
        Assert.Equal(string.Empty, model.Email);
        Assert.Equal(string.Empty, model.Message);
    }

    [Fact]
    public void Index_Get_SetsRecaptchaSiteKeyInViewBag()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.Equal("test-site-key", _controller.ViewBag.RecaptchaSiteKey);
    }

    [Fact]
    public async Task Index_Post_HoneypotFilled_ReturnsSuccessWithoutSendingEmail()
    {
        // Arrange
        var model = new ContactFormViewModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Message = "Test message",
            RecaptchaToken = "test-token",
            Website = "http://spam.com" // Honeypot filled - this is a bot
        };

        // Act
        var result = await _controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<ContactFormViewModel>(viewResult.Model);
        Assert.NotNull(returnedModel.SuccessMessage);

        // Verify email was NOT sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Index_Post_RecaptchaFails_ReturnsViewWithError()
    {
        // Arrange
        var model = new ContactFormViewModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Message = "Test message",
            RecaptchaToken = "invalid-token"
        };

        _recaptchaServiceMock.Setup(x => x.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(new RecaptchaValidationResult
            {
                Success = false,
                ErrorMessage = "reCAPTCHA validation failed"
            });

        // Act
        var result = await _controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Index_Post_ValidSubmission_SendsEmailAndReturnsSuccess()
    {
        // Arrange
        var model = new ContactFormViewModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Message = "This is a test message from the contact form.",
            RecaptchaToken = "valid-token"
        };

        _recaptchaServiceMock.Setup(x => x.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(new RecaptchaValidationResult
            {
                Success = true,
                Score = 0.9f
            });

        _emailServiceMock.Setup(x => x.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<ContactFormViewModel>(viewResult.Model);
        Assert.NotNull(returnedModel.SuccessMessage);

        // Verify email was sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync("admin@test.com", It.IsAny<string>(), It.IsAny<string>(), true, "test@example.com"),
            Times.Once);
    }

    [Fact]
    public async Task Index_Post_EmailServiceThrows_ReturnsViewWithError()
    {
        // Arrange
        var model = new ContactFormViewModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Message = "This is a test message from the contact form.",
            RecaptchaToken = "valid-token"
        };

        _recaptchaServiceMock.Setup(x => x.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(new RecaptchaValidationResult { Success = true, Score = 0.9f });

        _emailServiceMock.Setup(x => x.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act
        var result = await _controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<ContactFormViewModel>(viewResult.Model);
        Assert.NotNull(returnedModel.ErrorMessage);
    }

    [Fact]
    public void v2_ReturnsViewResult()
    {
        // Act
        var result = _controller.v2();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }
}
