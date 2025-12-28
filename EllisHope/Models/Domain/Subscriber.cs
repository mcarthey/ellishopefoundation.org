using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

/// <summary>
/// Represents a newsletter subscriber
/// </summary>
public class Subscriber
{
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UnsubscribedAt { get; set; }

    /// <summary>
    /// Unique token for secure unsubscribe links
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Optional name if provided during subscription
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Source of subscription (e.g., "Contact Page", "Footer", "Popup")
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }

    /// <summary>
    /// IP address at time of subscription (for spam prevention)
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Whether the subscriber is currently active (not unsubscribed)
    /// </summary>
    public bool IsActive => UnsubscribedAt == null;
}
