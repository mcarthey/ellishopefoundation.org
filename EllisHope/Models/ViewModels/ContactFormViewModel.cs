using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.ViewModels;

/// <summary>
/// ViewModel for the contact form submission
/// </summary>
public class ContactFormViewModel
{
    [Required(ErrorMessage = "Please enter your name")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your message")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 5000 characters")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Google reCAPTCHA token - populated by the client-side reCAPTCHA widget
    /// </summary>
    [Required(ErrorMessage = "Please complete the reCAPTCHA verification")]
    public string RecaptchaToken { get; set; } = string.Empty;

    /// <summary>
    /// Honeypot field - should be empty for legitimate submissions
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Success message to display after submission
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Error message to display if submission fails
    /// </summary>
    public string? ErrorMessage { get; set; }
}
