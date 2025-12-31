using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class Testimonial
{
    public int Id { get; set; }

    // Content
    [Required]
    [StringLength(1000)]
    public string Quote { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? AuthorRole { get; set; }

    // Optional Media
    public int? AuthorPhotoId { get; set; }
    public Media? AuthorPhoto { get; set; }

    // Publishing
    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Approval Workflow (matches BlogPost, Event, Cause pattern)
    public bool RequiresApproval { get; set; } = false;
    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public string? ApprovedById { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    // Audit
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
}
