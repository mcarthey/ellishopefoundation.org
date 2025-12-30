using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

public class Event
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    [Required]
    [MaxLength(500)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }

    [MaxLength(100)]
    public string? OrganizerName { get; set; }

    [MaxLength(100)]
    public string? OrganizerEmail { get; set; }

    [MaxLength(20)]
    public string? OrganizerPhone { get; set; }

    [MaxLength(500)]
    public string? RegistrationUrl { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public int? MaxAttendees { get; set; }

    public bool IsPublished { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    // Approval workflow (for users without auto-approve)
    public bool RequiresApproval { get; set; } = false;

    [MaxLength(450)]
    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    [MaxLength(450)]
    public string? ApprovedById { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
}
