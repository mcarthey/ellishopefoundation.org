using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public enum UsageType
{
    Featured,
    Gallery,
    Inline,
    Thumbnail,
    Hero,
    Background
}

public class MediaUsage
{
    public int Id { get; set; }

    public int MediaId { get; set; }
    public Media Media { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // "Blog", "Event", "Page"

    public int EntityId { get; set; }

    public UsageType UsageType { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
