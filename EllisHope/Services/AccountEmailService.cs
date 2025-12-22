using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service for sending account-related emails using templates.
/// Follows single responsibility principle - separates account emails from application workflow emails.
/// </summary>
public class AccountEmailService : IAccountEmailService
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<AccountEmailService> _logger;

    public AccountEmailService(
        IEmailService emailService,
        IEmailTemplateService templateService,
        ILogger<AccountEmailService> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(ApplicationUser user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("Cannot send welcome email - user {UserId} has no email address", user.Id);
            return;
        }

        try
        {
            var body = _templateService.GenerateWelcomeEmail(user.FirstName);
            await _emailService.SendEmailAsync(
                user.Email,
                "Welcome to Ellis Hope Foundation!",
                body,
                isHtml: true);

            _logger.LogInformation("Welcome email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            // Don't rethrow - email failures shouldn't break registration flow
        }
    }

    public async Task SendPasswordResetEmailAsync(ApplicationUser user, string resetToken, string resetUrl)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("Cannot send password reset email - user {UserId} has no email address", user.Id);
            return;
        }

        try
        {
            var body = _templateService.GeneratePasswordResetEmail(user.FirstName, resetUrl);
            await _emailService.SendEmailAsync(
                user.Email,
                "Reset Your Password - Ellis Hope Foundation",
                body,
                isHtml: true);

            _logger.LogInformation("Password reset email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            // Don't rethrow - we don't want to reveal if the email exists or not
        }
    }

    public async Task SendPasswordChangedConfirmationAsync(ApplicationUser user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.LogWarning("Cannot send password changed email - user {UserId} has no email address", user.Id);
            return;
        }

        try
        {
            var body = _templateService.GeneratePasswordChangedEmail(user.FirstName);
            await _emailService.SendEmailAsync(
                user.Email,
                "Password Changed - Ellis Hope Foundation",
                body,
                isHtml: true);

            _logger.LogInformation("Password changed confirmation email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password changed email to {Email}", user.Email);
            // Don't rethrow - email failures shouldn't break password change flow
        }
    }
}
