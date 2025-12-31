using System.ComponentModel.DataAnnotations;
using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

public class TestimonialListViewModel
{
    public IEnumerable<Testimonial> Testimonials { get; set; } = [];
    public int PendingApprovalCount { get; set; }
}

public class TestimonialCreateViewModel
{
    [Required(ErrorMessage = "Quote is required")]
    [StringLength(1000, ErrorMessage = "Quote cannot exceed 1000 characters")]
    [Display(Name = "Quote")]
    public string Quote { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author name is required")]
    [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
    [Display(Name = "Author Name")]
    public string AuthorName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Author role cannot exceed 100 characters")]
    [Display(Name = "Author Role/Title")]
    public string? AuthorRole { get; set; }

    [Display(Name = "Author Photo")]
    public int? AuthorPhotoId { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; } = false;

    [Display(Name = "Featured on Home Page")]
    public bool IsFeatured { get; set; } = false;

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 0;
}

public class TestimonialEditViewModel : TestimonialCreateViewModel
{
    public int Id { get; set; }

    public bool RequiresApproval { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ApprovedByName { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public Media? CurrentPhoto { get; set; }
}
