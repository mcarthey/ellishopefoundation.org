using System.ComponentModel.DataAnnotations;
using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

/// <summary>
/// ViewModel for sponsor company profile management
/// </summary>
public class SponsorProfileViewModel
{
    // Company Info
    [Display(Name = "Company Name")]
    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Display(Name = "Company Logo")]
    public IFormFile? CompanyLogo { get; set; }

    public string? CurrentCompanyLogoUrl { get; set; }

    // Testimonial
    [Display(Name = "Your Quote/Testimonial")]
    [StringLength(500, MinimumLength = 20, ErrorMessage = "Quote must be between 20 and 500 characters")]
    public string? SponsorQuote { get; set; }

    [Display(Name = "Rating (Optional)")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
    public int? SponsorRating { get; set; }

    // Visibility
    [Display(Name = "Show my company on the About page")]
    public bool ShowInSponsorSection { get; set; } = true;

    // Status (read-only display)
    public bool QuoteApproved { get; set; }
    public DateTime? QuoteSubmittedDate { get; set; }
    public string? QuoteStatus { get; set; }  // "Pending Approval", "Approved", "Not Submitted", "Rejected"
    public string? RejectionReason { get; set; }  // Show admin feedback if rejected
}

/// <summary>
/// ViewModel for admin to view pending sponsor quotes
/// </summary>
public class PendingSponsorQuoteViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? Quote { get; set; }
    public int? Rating { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// ViewModel for rejecting a sponsor quote with feedback
/// </summary>
public class RejectQuoteViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? SponsorName { get; set; }
    public string? CompanyName { get; set; }
    public string? Quote { get; set; }

    [Required(ErrorMessage = "Please provide a reason for rejection")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 1000 characters")]
    [Display(Name = "Rejection Reason / Feedback")]
    public string Reason { get; set; } = string.Empty;
}
