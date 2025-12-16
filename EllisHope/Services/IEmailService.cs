namespace EllisHope.Services;

/// <summary>
/// Service interface for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email to recipient
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    
    /// <summary>
    /// Send email to multiple recipients
    /// </summary>
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true);
    
    /// <summary>
    /// Send email using template
    /// </summary>
    Task SendTemplatedEmailAsync(string to, string templateName, object model);
}
