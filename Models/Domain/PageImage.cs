using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class PageImage
{
    public int Id { get; set; }

    public int PageId { get; set; }
    public int MediaId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ImageKey { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    // Navigation properties
    public Page Page { get; set; } = null!;
    public Media Media { get; set; } = null!;
}
