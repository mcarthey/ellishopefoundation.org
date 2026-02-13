using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class SiteSetting
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Value { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}
