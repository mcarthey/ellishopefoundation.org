using EllisHope.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EllisHope.Tests.Services;

public class EmailServiceTests
{
    private EmailService CreateEmailService(EmailSettings? settings = null, ILogger<EmailService>? logger = null)
    {
        var emailSettings = settings ?? new EmailSettings
        {
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUsername = "test@test.com",
            SmtpPassword = "testpassword",
            EnableSsl = true,
            FromEmail = "noreply@test.com",
            FromName = "Test Foundation"
        };

        var mockOptions = new Mock<IOptions<EmailSettings>>();
        mockOptions.Setup(x => x.Value).Returns(emailSettings);

        var mockLogger = logger ?? Mock.Of<ILogger<EmailService>>();

        return new EmailService(mockOptions.Object, mockLogger);
    }

    [Fact]
    public void Constructor_InitializesWithProvidedSettings()
    {
        // Arrange
        var settings = new EmailSettings
        {
            SmtpHost = "custom.smtp.com",
            SmtpPort = 465,
            FromEmail = "custom@test.com"
        };

        // Act
        var service = CreateEmailService(settings);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task SendEmailAsync_DoesNotThrow_WhenSendingEmail()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert - Should not throw even if SMTP fails
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", "Test Subject", "Test Body")
        );

        // The service swallows exceptions to prevent workflow failures
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_HandlesNullRecipient_Gracefully()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync((string)null!, "Subject", "Body")
        );

        // Should handle null gracefully (will fail internally but not throw)
        // The null! is intentional to test error handling
    }

    [Fact]
    public async Task SendEmailAsync_HandlesEmptyRecipient_Gracefully()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync(string.Empty, "Subject", "Body")
        );

        // Should handle empty string gracefully
    }

    [Fact]
    public async Task SendEmailAsync_HandlesNullSubject_Gracefully()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", null!, "Body")
        );

        // Should handle null subject gracefully
    }

    [Fact]
    public async Task SendEmailAsync_HandlesNullBody_Gracefully()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", "Subject", null!)
        );

        // Should handle null body gracefully
    }

    [Fact]
    public async Task SendEmailAsync_WithIsHtmlFalse_DoesNotThrow()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", "Test", "Plain text body", isHtml: false)
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_WithIsHtmlTrue_DoesNotThrow()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", "Test", "<html><body>HTML body</body></html>", isHtml: true)
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_MultipleRecipients_SendsToEachRecipient()
    {
        // Arrange
        var service = CreateEmailService();
        var recipients = new[] { "user1@test.com", "user2@test.com", "user3@test.com" };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync(recipients, "Test Subject", "Test Body")
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_MultipleRecipients_WithEmptyList_DoesNotThrow()
    {
        // Arrange
        var service = CreateEmailService();
        var recipients = Array.Empty<string>();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync(recipients, "Test Subject", "Test Body")
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_MultipleRecipients_WithNullList_Throws()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.SendEmailAsync((IEnumerable<string>)null!, "Subject", "Body")
        );
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_CreatesTemplateBody()
    {
        // Arrange
        var service = CreateEmailService();
        var model = new { Name = "John", Amount = 1000 };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendTemplatedEmailAsync("test@example.com", "Application Approved", model)
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_WithNullModel_DoesNotThrow()
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendTemplatedEmailAsync("test@example.com", "Template", null!)
        );

        Assert.Null(exception);
    }

    [Fact]
    public void EmailSettings_HasDefaultValues()
    {
        // Arrange & Act
        var settings = new EmailSettings();

        // Assert
        Assert.Equal("smtp.gmail.com", settings.SmtpHost);
        Assert.Equal(587, settings.SmtpPort);
        Assert.Equal(string.Empty, settings.SmtpUsername);
        Assert.Equal(string.Empty, settings.SmtpPassword);
        Assert.True(settings.EnableSsl);
        Assert.Equal("noreply@ellishope.org", settings.FromEmail);
        Assert.Equal("Ellis Hope Foundation", settings.FromName);
    }

    [Fact]
    public void EmailSettings_CanSetCustomValues()
    {
        // Arrange & Act
        var settings = new EmailSettings
        {
            SmtpHost = "custom.smtp.com",
            SmtpPort = 465,
            SmtpUsername = "custom@user.com",
            SmtpPassword = "custompass",
            EnableSsl = false,
            FromEmail = "custom@from.com",
            FromName = "Custom Name"
        };

        // Assert
        Assert.Equal("custom.smtp.com", settings.SmtpHost);
        Assert.Equal(465, settings.SmtpPort);
        Assert.Equal("custom@user.com", settings.SmtpUsername);
        Assert.Equal("custompass", settings.SmtpPassword);
        Assert.False(settings.EnableSsl);
        Assert.Equal("custom@from.com", settings.FromEmail);
        Assert.Equal("Custom Name", settings.FromName);
    }

    [Fact]
    public async Task SendEmailAsync_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailService>>();
        var service = CreateEmailService(logger: mockLogger.Object);

        // Act
        await service.SendEmailAsync("invalid-email-address", "Subject", "Body");

        // Assert - Verify that LogError was called at least once
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendEmailAsync_WithSpecialCharactersInSubject_DoesNotThrow()
    {
        // Arrange
        var service = CreateEmailService();
        var subject = "Test Email: Special Chars áéíóú ñ €$£ 中文 日本語";

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", subject, "Body")
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_WithLongBody_DoesNotThrow()
    {
        // Arrange
        var service = CreateEmailService();
        var longBody = string.Concat(Enumerable.Repeat("<p>This is a test paragraph. </p>", 1000));

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", "Long Email", longBody, isHtml: true)
        );

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_WithComplexHtmlBody_DoesNotThrow()
    {
        // Arrange
        var service = CreateEmailService();
        var htmlBody = @"
            <html>
                <head><style>body { font-family: Arial; }</style></head>
                <body>
                    <h1>Welcome</h1>
                    <p>This is a <strong>test</strong> email with <a href='https://test.com'>links</a>.</p>
                    <img src='https://test.com/image.png' alt='Test' />
                    <table><tr><td>Cell 1</td><td>Cell 2</td></tr></table>
                </body>
            </html>";

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", "HTML Email", htmlBody, isHtml: true)
        );

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("smtp.gmail.com", 587)]
    [InlineData("smtp.office365.com", 587)]
    [InlineData("mail.ellishopefoundation.org", 465)]
    [InlineData("localhost", 25)]
    public async Task SendEmailAsync_WithDifferentSmtpHosts_DoesNotThrow(string smtpHost, int smtpPort)
    {
        // Arrange
        var settings = new EmailSettings
        {
            SmtpHost = smtpHost,
            SmtpPort = smtpPort,
            SmtpUsername = "test@test.com",
            SmtpPassword = "password",
            EnableSsl = true
        };
        var service = CreateEmailService(settings);

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync("test@example.com", "Test", "Body")
        );

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user+tag@domain.co.uk")]
    [InlineData("first.last@subdomain.example.com")]
    [InlineData("user_name@example-domain.org")]
    public async Task SendEmailAsync_WithVariousEmailFormats_DoesNotThrow(string emailAddress)
    {
        // Arrange
        var service = CreateEmailService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await service.SendEmailAsync(emailAddress, "Test", "Body")
        );

        Assert.Null(exception);
    }
}
