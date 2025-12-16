using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EllisHope.Services;

/// <summary>
/// SMTP-based email service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);
            client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
            client.EnableSsl = _settings.EnableSsl;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation($"Email sent successfully to {to}: {subject}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {to}: {subject}");
            // Don't throw - just log the error to prevent notification failures from breaking workflows
        }
    }

    public async Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true)
    {
        foreach (var recipient in recipients)
        {
            await SendEmailAsync(recipient, subject, body, isHtml);
        }
    }

    public async Task SendTemplatedEmailAsync(string to, string templateName, object model)
    {
        // Simple template implementation - in production, use a proper templating engine
        var body = $"<html><body><h1>{templateName}</h1><p>{model}</p></body></html>";
        await SendEmailAsync(to, templateName, body, true);
    }
}

/// <summary>
/// Email configuration settings
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@ellishope.org";
    public string FromName { get; set; } = "Ellis Hope Foundation";
}
