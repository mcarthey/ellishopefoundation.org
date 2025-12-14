using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class Cause
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }

    [Required]
    public decimal GoalAmount { get; set; }

    public decimal RaisedAmount { get; set; } = 0;

    public int ProgressPercentage
    {
        get
        {
            if (GoalAmount == 0) return 0;
            return (int)((RaisedAmount / GoalAmount) * 100);
        }
    }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? DonationUrl { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}
