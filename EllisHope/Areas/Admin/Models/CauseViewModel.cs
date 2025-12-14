using System.ComponentModel.DataAnnotations;

namespace EllisHope.Areas.Admin.Models;

public class CauseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Slug cannot exceed 300 characters")]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Short description cannot exceed 500 characters")]
    [Display(Name = "Short Description")]
    public string? ShortDescription { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public string? FeaturedImageUrl { get; set; }

    [Display(Name = "Featured Image")]
    public IFormFile? FeaturedImageFile { get; set; }

    [Required(ErrorMessage = "Goal amount is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Goal amount must be greater than 0")]
    [Display(Name = "Goal Amount ($)")]
    public decimal GoalAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Raised amount cannot be negative")]
    [Display(Name = "Raised Amount ($)")]
    public decimal RaisedAmount { get; set; }

    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Donation URL")]
    [Url]
    [StringLength(500)]
    public string? DonationUrl { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; }

    [Display(Name = "Featured")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Tags (comma-separated)")]
    public string? Tags { get; set; }
}
