using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class Page
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string PageName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
    public bool IsPublished { get; set; } = true;

    // Navigation properties
    public ICollection<ContentSection> ContentSections { get; set; } = new List<ContentSection>();
    public ICollection<PageImage> PageImages { get; set; } = new List<PageImage>();
}
