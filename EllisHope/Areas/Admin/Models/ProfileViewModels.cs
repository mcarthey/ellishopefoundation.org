using System.ComponentModel.DataAnnotations;
using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

public class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole UserRole { get; set; }
    public MembershipStatus Status { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    public string? ProfilePictureUrl { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}

public class EditProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(200)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [StringLength(2)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [StringLength(10)]
    [Display(Name = "Zip Code")]
    public string? ZipCode { get; set; }

    [StringLength(100)]
    [Display(Name = "Emergency Contact Name")]
    public string? EmergencyContactName { get; set; }

    [Phone]
    [Display(Name = "Emergency Contact Phone")]
    public string? EmergencyContactPhone { get; set; }
}

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
