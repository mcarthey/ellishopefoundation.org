namespace EllisHope.Services;

/// <summary>
/// Service interface for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email to recipient
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <param name="isHtml">Whether body is HTML formatted</param>
    /// <param name="replyTo">Optional reply-to email address</param>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, string? replyTo = null);
    
    /// <summary>
    /// Send email to multiple recipients
    /// </summary>
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true);
    
    /// <summary>
    /// Send email using template
    /// </summary>
    Task SendTemplatedEmailAsync(string to, string templateName, object model);
}
