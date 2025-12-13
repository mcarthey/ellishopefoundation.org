using System.ComponentModel.DataAnnotations;

namespace EllisHope.Areas.Admin.Models;

public class EventViewModel
{
    public int Id { get; set;}

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Slug cannot exceed 300 characters")]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    public string? FeaturedImageUrl { get; set; }

    [Display(Name = "Featured Image")]
    public IFormFile? FeaturedImageFile { get; set; }

    [Required(ErrorMessage = "Event date is required")]
    [Display(Name = "Event Date")]
    public DateTime EventDate { get; set; }

    [Display(Name = "Start Time")]
    public TimeSpan? StartTime { get; set; }

    [Display(Name = "End Time")]
    public TimeSpan? EndTime { get; set; }

    [Required(ErrorMessage = "Location is required")]
    [StringLength(500)]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "Organizer Name")]
    [StringLength(100)]
    public string? OrganizerName { get; set; }

    [Display(Name = "Organizer Email")]
    [EmailAddress]
    [StringLength(100)]
    public string? OrganizerEmail { get; set; }

    [Display(Name = "Organizer Phone")]
    [Phone]
    [StringLength(20)]
    public string? OrganizerPhone { get; set; }

    [Display(Name = "Registration URL")]
    [Url]
    [StringLength(500)]
    public string? RegistrationUrl { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; }

    [Display(Name = "Tags (comma-separated)")]
    public string? Tags { get; set; }

    [Display(Name = "Max Attendees")]
    public int? MaxAttendees { get; set; }
}
