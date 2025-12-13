using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public enum MediaSource
{
    Local,
    Unsplash
}

public enum MediaCategory
{
    Uncategorized,
    Hero,
    Blog,
    Event,
    Team,
    Gallery,
    Page,
    Icon,
    Logo
}

public class Media
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    // For Unsplash images
    [MaxLength(100)]
    public string? UnsplashId { get; set; }

    [MaxLength(200)]
    public string? UnsplashPhotographer { get; set; }

    [MaxLength(200)]
    public string? UnsplashPhotographerUsername { get; set; }

    public MediaSource Source { get; set; } = MediaSource.Local;

    public long FileSize { get; set; }

    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AltText { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Caption { get; set; }

    // Comma-separated tags
    [MaxLength(500)]
    public string? Tags { get; set; }

    public MediaCategory Category { get; set; } = MediaCategory.Uncategorized;

    public int? Width { get; set; }
    public int? Height { get; set; }

    // JSON storage for different size URLs/paths
    [MaxLength(2000)]
    public string? Thumbnails { get; set; }

    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string? UploadedBy { get; set; }

    // Navigation property
    public ICollection<MediaUsage> Usages { get; set; } = new List<MediaUsage>();
}
