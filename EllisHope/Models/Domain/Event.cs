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
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    // Additional date/time properties
    public DateTime EventDate { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public int? FeaturedImageId { get; set; }
    public Media? FeaturedImage { get; set; }

    [MaxLength(500)]
    public string? FeaturedImageUrl { get; set; }

    [MaxLength(500)]
    public string? RegistrationUrl { get; set; }

    public int? MaxAttendees { get; set; }
    public int CurrentAttendees { get; set; } = 0;

    // Organizer information
    [MaxLength(200)]
    public string? OrganizerName { get; set; }

    [MaxLength(200)]
    public string? OrganizerEmail { get; set; }

    [MaxLength(50)]
    public string? OrganizerPhone { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public bool IsPublished { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
}
