using System.ComponentModel.DataAnnotations;
using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

/// <summary>
/// View model for the newsletter subscribers list
/// </summary>
public class NewsletterIndexViewModel
{
    public List<Subscriber> Subscribers { get; set; } = new();
    public int ActiveCount { get; set; }
    public int TotalCount { get; set; }
    public int UnsubscribedCount { get; set; }
    public List<Newsletter> RecentNewsletters { get; set; } = new();
    public List<Newsletter> DraftNewsletters { get; set; } = new();
}

/// <summary>
/// View model for composing/editing a newsletter
/// </summary>
public class ComposeNewsletterViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Subject is required")]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string HtmlContent { get; set; } = string.Empty;

    public string? PlainTextContent { get; set; }

    public int SubscriberCount { get; set; }
}

/// <summary>
/// View model for newsletter preview
/// </summary>
public class PreviewNewsletterViewModel
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public int SubscriberCount { get; set; }
}

/// <summary>
/// View model for newsletter history
/// </summary>
public class NewsletterHistoryViewModel
{
    public List<Newsletter> SentNewsletters { get; set; } = new();
}
