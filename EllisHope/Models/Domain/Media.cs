using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class Media
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AltText { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    public int? Width { get; set; }
    public int? Height { get; set; }

    public DateTime UploadedDate { get; set; } = DateTime.Now;

    [MaxLength(256)]
    public string? UploadedBy { get; set; }
}
