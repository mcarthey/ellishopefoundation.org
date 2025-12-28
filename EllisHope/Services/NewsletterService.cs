using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

namespace EllisHope.Services;

/// <summary>
/// Implementation of newsletter subscription and sending service
/// </summary>
public class NewsletterService : INewsletterService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(
        ApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<NewsletterService> logger)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    #region Subscription Management

    public async Task<(bool Success, string Message)> SubscribeAsync(string email, string? name = null, string? source = null, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Email address is required.");
        }

        email = email.Trim().ToLowerInvariant();

        // Check if already subscribed
        var existingSubscriber = await _context.Subscribers
            .FirstOrDefaultAsync(s => s.Email == email);

        if (existingSubscriber != null)
        {
            if (existingSubscriber.IsActive)
            {
                return (false, "This email is already subscribed to our newsletter.");
            }
            else
            {
                // Re-subscribe
                existingSubscriber.UnsubscribedAt = null;
                existingSubscriber.SubscribedAt = DateTime.UtcNow;
                existingSubscriber.UnsubscribeToken = Guid.NewGuid().ToString("N");
                existingSubscriber.Name = name ?? existingSubscriber.Name;
                existingSubscriber.Source = source;
                existingSubscriber.IpAddress = ipAddress;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Subscriber re-subscribed: {Email}", email);
                return (true, "Welcome back! You've been re-subscribed to our newsletter.");
            }
        }

        // Create new subscriber
        var subscriber = new Subscriber
        {
            Email = email,
            Name = name,
            Source = source,
            IpAddress = ipAddress,
            SubscribedAt = DateTime.UtcNow,
            UnsubscribeToken = Guid.NewGuid().ToString("N")
        };

        _context.Subscribers.Add(subscriber);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New subscriber added: {Email} from {Source}", email, source);
        return (true, "Thank you for subscribing to our newsletter!");
    }

    public async Task<(bool Success, string Message)> UnsubscribeAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "Invalid unsubscribe token.");
        }

        var subscriber = await _context.Subscribers
            .FirstOrDefaultAsync(s => s.UnsubscribeToken == token);

        if (subscriber == null)
        {
            return (false, "Invalid or expired unsubscribe link.");
        }

        if (!subscriber.IsActive)
        {
            return (false, "This email has already been unsubscribed.");
        }

        subscriber.UnsubscribedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Subscriber unsubscribed: {Email}", subscriber.Email);
        return (true, "You have been successfully unsubscribed from our newsletter.");
    }

    public async Task<Subscriber?> GetSubscriberByTokenAsync(string token)
    {
        return await _context.Subscribers
            .FirstOrDefaultAsync(s => s.UnsubscribeToken == token);
    }

    public async Task<List<Subscriber>> GetActiveSubscribersAsync()
    {
        return await _context.Subscribers
            .Where(s => s.UnsubscribedAt == null)
            .OrderByDescending(s => s.SubscribedAt)
            .ToListAsync();
    }

    public async Task<List<Subscriber>> GetAllSubscribersAsync()
    {
        return await _context.Subscribers
            .OrderByDescending(s => s.SubscribedAt)
            .ToListAsync();
    }

    public async Task<int> GetActiveSubscriberCountAsync()
    {
        return await _context.Subscribers
            .CountAsync(s => s.UnsubscribedAt == null);
    }

    public async Task<bool> IsSubscribedAsync(string email)
    {
        email = email.Trim().ToLowerInvariant();
        return await _context.Subscribers
            .AnyAsync(s => s.Email == email && s.UnsubscribedAt == null);
    }

    public async Task<bool> DeleteSubscriberAsync(int subscriberId)
    {
        var subscriber = await _context.Subscribers.FindAsync(subscriberId);
        if (subscriber == null)
        {
            return false;
        }

        _context.Subscribers.Remove(subscriber);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Subscriber deleted: {Email}", subscriber.Email);
        return true;
    }

    #endregion

    #region Newsletter Management

    public async Task<Newsletter> CreateDraftAsync(string subject, string htmlContent, string? plainTextContent = null)
    {
        var newsletter = new Newsletter
        {
            Subject = subject,
            HtmlContent = htmlContent,
            PlainTextContent = plainTextContent,
            CreatedAt = DateTime.UtcNow
        };

        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Newsletter draft created: {Subject}", subject);
        return newsletter;
    }

    public async Task<bool> UpdateDraftAsync(int newsletterId, string subject, string htmlContent, string? plainTextContent = null)
    {
        var newsletter = await _context.Newsletters.FindAsync(newsletterId);
        if (newsletter == null || newsletter.IsSent)
        {
            return false;
        }

        newsletter.Subject = subject;
        newsletter.HtmlContent = htmlContent;
        newsletter.PlainTextContent = plainTextContent;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Newsletter?> GetNewsletterByIdAsync(int id)
    {
        return await _context.Newsletters
            .Include(n => n.SentBy)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<Newsletter>> GetDraftNewslettersAsync()
    {
        return await _context.Newsletters
            .Where(n => n.SentAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Newsletter>> GetSentNewslettersAsync()
    {
        return await _context.Newsletters
            .Include(n => n.SentBy)
            .Where(n => n.SentAt != null)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    public async Task<(bool Success, int RecipientCount, string Message)> SendNewsletterAsync(int newsletterId, string senderId)
    {
        var newsletter = await _context.Newsletters.FindAsync(newsletterId);
        if (newsletter == null)
        {
            return (false, 0, "Newsletter not found.");
        }

        if (newsletter.IsSent)
        {
            return (false, 0, "This newsletter has already been sent.");
        }

        var subscribers = await GetActiveSubscribersAsync();
        if (!subscribers.Any())
        {
            return (false, 0, "No active subscribers to send to.");
        }

        var baseUrl = _configuration["App:BaseUrl"] ?? "https://ellishopefoundation.org";
        var foundationName = _configuration["Foundation:Name"] ?? "Ellis Hope Foundation";

        // Send to each subscriber
        int successCount = 0;
        foreach (var subscriber in subscribers)
        {
            try
            {
                var unsubscribeUrl = GenerateUnsubscribeUrl(baseUrl, subscriber.UnsubscribeToken);
                var personalizedContent = WrapWithEmailTemplate(
                    newsletter.HtmlContent,
                    newsletter.Subject,
                    unsubscribeUrl,
                    foundationName);

                await _emailService.SendEmailAsync(
                    subscriber.Email,
                    newsletter.Subject,
                    personalizedContent,
                    isHtml: true);

                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send newsletter to {Email}", subscriber.Email);
            }
        }

        // Update newsletter as sent
        newsletter.SentAt = DateTime.UtcNow;
        newsletter.RecipientCount = successCount;
        newsletter.SentByUserId = senderId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Newsletter sent: {Subject} to {Count} recipients", newsletter.Subject, successCount);
        return (true, successCount, $"Newsletter sent successfully to {successCount} subscribers.");
    }

    public async Task<bool> DeleteDraftAsync(int newsletterId)
    {
        var newsletter = await _context.Newsletters.FindAsync(newsletterId);
        if (newsletter == null || newsletter.IsSent)
        {
            return false;
        }

        _context.Newsletters.Remove(newsletter);
        await _context.SaveChangesAsync();
        return true;
    }

    public string GenerateUnsubscribeUrl(string baseUrl, string unsubscribeToken)
    {
        return $"{baseUrl.TrimEnd('/')}/Newsletter/Unsubscribe?token={unsubscribeToken}";
    }

    #endregion

    #region Export

    public async Task<string> ExportSubscribersAsCsvAsync()
    {
        var subscribers = await GetActiveSubscribersAsync();
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Email,Name,SubscribedAt,Source");

        // Data
        foreach (var subscriber in subscribers)
        {
            sb.AppendLine($"\"{subscriber.Email}\",\"{subscriber.Name ?? ""}\",\"{subscriber.SubscribedAt:yyyy-MM-dd HH:mm:ss}\",\"{subscriber.Source ?? ""}\"");
        }

        return sb.ToString();
    }

    #endregion

    #region Private Helpers

    private string WrapWithEmailTemplate(string content, string subject, string unsubscribeUrl, string foundationName)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{subject}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            text-align: center;
            padding: 20px 0;
            border-bottom: 2px solid #f0784b;
        }}
        .header h1 {{
            color: #f0784b;
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px 0;
            border-top: 1px solid #eee;
            font-size: 12px;
            color: #666;
        }}
        .footer a {{
            color: #f0784b;
        }}
        .unsubscribe {{
            margin-top: 15px;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>{foundationName}</h1>
    </div>
    <div class=""content"">
        {content}
    </div>
    <div class=""footer"">
        <p>You received this email because you subscribed to our newsletter.</p>
        <p class=""unsubscribe"">
            <a href=""{unsubscribeUrl}"">Unsubscribe from future emails</a>
        </p>
        <p>&copy; {DateTime.UtcNow.Year} {foundationName}. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    #endregion
}
