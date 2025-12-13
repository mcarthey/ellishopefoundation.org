using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class ImageSize
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Hero", "Blog Featured", "Thumbnail"

    [MaxLength(200)]
    public string? Description { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public MediaCategory Category { get; set; }

    public bool IsActive { get; set; } = true;
}
