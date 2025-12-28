using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

/// <summary>
/// Represents a newsletter that has been composed and optionally sent
/// </summary>
public class Newsletter
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML content of the newsletter
    /// </summary>
    [Required]
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Plain text version of the content (for email clients that don't support HTML)
    /// </summary>
    public string? PlainTextContent { get; set; }

    /// <summary>
    /// When the newsletter was created/drafted
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the newsletter was sent (null if still draft)
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Number of recipients when sent
    /// </summary>
    public int RecipientCount { get; set; }

    /// <summary>
    /// User who sent the newsletter
    /// </summary>
    [MaxLength(450)]
    public string? SentByUserId { get; set; }
    public ApplicationUser? SentBy { get; set; }

    /// <summary>
    /// Whether this newsletter has been sent
    /// </summary>
    public bool IsSent => SentAt.HasValue;

    /// <summary>
    /// Whether this is still a draft
    /// </summary>
    public bool IsDraft => !SentAt.HasValue;
}
