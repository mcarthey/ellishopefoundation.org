using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service for sending account-related emails (welcome, password reset, etc.)
/// Follows single responsibility principle - separates account emails from application emails.
/// </summary>
public interface IAccountEmailService
{
    /// <summary>
    /// Send welcome email to newly registered user
    /// </summary>
    Task SendWelcomeEmailAsync(ApplicationUser user);

    /// <summary>
    /// Send password reset email with reset link
    /// </summary>
    Task SendPasswordResetEmailAsync(ApplicationUser user, string resetToken, string resetUrl);

    /// <summary>
    /// Send confirmation email after password has been changed
    /// </summary>
    Task SendPasswordChangedConfirmationAsync(ApplicationUser user);
}
