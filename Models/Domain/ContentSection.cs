using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class ContentSection
{
    public int Id { get; set; }

    public int PageId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SectionKey { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(20)]
    public string ContentType { get; set; } = "Text"; // Text, HTML, RichText

    public int DisplayOrder { get; set; }

    // Navigation property
    public Page Page { get; set; } = null!;
}
