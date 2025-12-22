using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Services;

public class AccountEmailServiceTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IEmailTemplateService> _mockTemplateService;
    private readonly Mock<ILogger<AccountEmailService>> _mockLogger;
    private readonly AccountEmailService _service;

    public AccountEmailServiceTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockTemplateService = new Mock<IEmailTemplateService>();
        _mockLogger = new Mock<ILogger<AccountEmailService>>();

        _mockTemplateService.Setup(t => t.GenerateWelcomeEmail(It.IsAny<string>()))
            .Returns("<html>Welcome</html>");
        _mockTemplateService.Setup(t => t.GeneratePasswordResetEmail(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("<html>Reset</html>");
        _mockTemplateService.Setup(t => t.GeneratePasswordChangedEmail(It.IsAny<string>()))
            .Returns("<html>Changed</html>");

        _service = new AccountEmailService(
            _mockEmailService.Object,
            _mockTemplateService.Object,
            _mockLogger.Object);
    }

    private ApplicationUser CreateTestUser(string? email = "test@example.com")
    {
        return new ApplicationUser
        {
            Id = "test-user-id",
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            UserName = email
        };
    }

    #region SendWelcomeEmailAsync Tests

    [Fact]
    public async Task SendWelcomeEmailAsync_SendsEmail_WithCorrectParameters()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        await _service.SendWelcomeEmailAsync(user);

        // Assert
        _mockTemplateService.Verify(t => t.GenerateWelcomeEmail(user.FirstName), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync(
            user.Email,
            "Welcome to Ellis Hope Foundation!",
            It.IsAny<string>(),
            true), Times.Once);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_DoesNotSendEmail_WhenEmailIsNull()
    {
        // Arrange
        var user = CreateTestUser(email: null);

        // Act
        await _service.SendWelcomeEmailAsync(user);

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_DoesNotSendEmail_WhenEmailIsEmpty()
    {
        // Arrange
        var user = CreateTestUser(email: "");

        // Act
        await _service.SendWelcomeEmailAsync(user);

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_DoesNotThrow_WhenEmailServiceThrows()
    {
        // Arrange
        var user = CreateTestUser();
        _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() => _service.SendWelcomeEmailAsync(user));
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_LogsError_WhenEmailServiceThrows()
    {
        // Arrange
        var user = CreateTestUser();
        _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act
        await _service.SendWelcomeEmailAsync(user);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SendPasswordResetEmailAsync Tests

    [Fact]
    public async Task SendPasswordResetEmailAsync_SendsEmail_WithCorrectParameters()
    {
        // Arrange
        var user = CreateTestUser();
        var resetToken = "test-reset-token";
        var resetUrl = "https://ellishope.org/Admin/Account/ResetPassword?token=test";

        // Act
        await _service.SendPasswordResetEmailAsync(user, resetToken, resetUrl);

        // Assert
        _mockTemplateService.Verify(t => t.GeneratePasswordResetEmail(user.FirstName, resetUrl), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync(
            user.Email,
            "Reset Your Password - Ellis Hope Foundation",
            It.IsAny<string>(),
            true), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_DoesNotSendEmail_WhenEmailIsNull()
    {
        // Arrange
        var user = CreateTestUser(email: null);

        // Act
        await _service.SendPasswordResetEmailAsync(user, "token", "url");

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_DoesNotSendEmail_WhenEmailIsEmpty()
    {
        // Arrange
        var user = CreateTestUser(email: "");

        // Act
        await _service.SendPasswordResetEmailAsync(user, "token", "url");

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_DoesNotThrow_WhenEmailServiceThrows()
    {
        // Arrange
        var user = CreateTestUser();
        _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() =>
            _service.SendPasswordResetEmailAsync(user, "token", "url"));
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_LogsError_WhenEmailServiceThrows()
    {
        // Arrange
        var user = CreateTestUser();
        _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act
        await _service.SendPasswordResetEmailAsync(user, "token", "url");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SendPasswordChangedConfirmationAsync Tests

    [Fact]
    public async Task SendPasswordChangedConfirmationAsync_SendsEmail_WithCorrectParameters()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        await _service.SendPasswordChangedConfirmationAsync(user);

        // Assert
        _mockTemplateService.Verify(t => t.GeneratePasswordChangedEmail(user.FirstName), Times.Once);
        _mockEmailService.Verify(e => e.SendEmailAsync(
            user.Email,
            "Password Changed - Ellis Hope Foundation",
            It.IsAny<string>(),
            true), Times.Once);
    }

    [Fact]
    public async Task SendPasswordChangedConfirmationAsync_DoesNotSendEmail_WhenEmailIsNull()
    {
        // Arrange
        var user = CreateTestUser(email: null);

        // Act
        await _service.SendPasswordChangedConfirmationAsync(user);

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SendPasswordChangedConfirmationAsync_DoesNotSendEmail_WhenEmailIsEmpty()
    {
        // Arrange
        var user = CreateTestUser(email: "");

        // Act
        await _service.SendPasswordChangedConfirmationAsync(user);

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SendPasswordChangedConfirmationAsync_DoesNotThrow_WhenEmailServiceThrows()
    {
        // Arrange
        var user = CreateTestUser();
        _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() =>
            _service.SendPasswordChangedConfirmationAsync(user));
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendPasswordChangedConfirmationAsync_LogsError_WhenEmailServiceThrows()
    {
        // Arrange
        var user = CreateTestUser();
        _mockEmailService.Setup(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()))
            .ThrowsAsync(new Exception("SMTP error"));

        // Act
        await _service.SendPasswordChangedConfirmationAsync(user);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Integration Tests (verifying template is used)

    [Fact]
    public async Task SendWelcomeEmailAsync_UsesTemplateBody()
    {
        // Arrange
        var user = CreateTestUser();
        var expectedBody = "<html>Custom Welcome Template</html>";
        _mockTemplateService.Setup(t => t.GenerateWelcomeEmail(It.IsAny<string>()))
            .Returns(expectedBody);

        // Act
        await _service.SendWelcomeEmailAsync(user);

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            expectedBody,
            It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_UsesTemplateBody()
    {
        // Arrange
        var user = CreateTestUser();
        var resetUrl = "https://test.com/reset";
        var expectedBody = "<html>Reset Link: " + resetUrl + "</html>";
        _mockTemplateService.Setup(t => t.GeneratePasswordResetEmail(It.IsAny<string>(), resetUrl))
            .Returns(expectedBody);

        // Act
        await _service.SendPasswordResetEmailAsync(user, "token", resetUrl);

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            expectedBody,
            It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task SendPasswordChangedConfirmationAsync_UsesTemplateBody()
    {
        // Arrange
        var user = CreateTestUser();
        var expectedBody = "<html>Password Changed Confirmation</html>";
        _mockTemplateService.Setup(t => t.GeneratePasswordChangedEmail(It.IsAny<string>()))
            .Returns(expectedBody);

        // Act
        await _service.SendPasswordChangedConfirmationAsync(user);

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            expectedBody,
            It.IsAny<bool>()), Times.Once);
    }

    #endregion
}
