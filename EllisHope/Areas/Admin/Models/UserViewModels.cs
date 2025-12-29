using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EllisHope.Areas.Admin.Models;

public class UserListViewModel
{
    public IEnumerable<UserSummaryViewModel> Users { get; set; } = new List<UserSummaryViewModel>();
    public string? SearchTerm { get; set; }
    public UserRole? RoleFilter { get; set; }
    public MembershipStatus? StatusFilter { get; set; }
    public bool? ActiveFilter { get; set; }
    
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingUsers { get; set; }
}

public class UserSummaryViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole UserRole { get; set; }
    public MembershipStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? SponsorName { get; set; }
    public int SponsoredClientsCount { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class UserCreateViewModel
{
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

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "User Role")]
    public UserRole UserRole { get; set; } = UserRole.Member;

    [Display(Name = "Status")]
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

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

    [Display(Name = "Sponsor")]
    public string? SponsorId { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "Monthly Fee")]
    public decimal? MonthlyFee { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Membership Start Date")]
    public DateTime? MembershipStartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Membership End Date")]
    public DateTime? MembershipEndDate { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "Admin Notes")]
    public string? AdminNotes { get; set; }

    [Display(Name = "Send Welcome Email")]
    public bool SendWelcomeEmail { get; set; } = true;

    [Display(Name = "Profile Photo")]
    public IFormFile? ProfilePhoto { get; set; }

    [Display(Name = "Selected Avatar")]
    public string? SelectedAvatarUrl { get; set; }
}

public class UserEditViewModel
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

    [Display(Name = "User Role")]
    public UserRole UserRole { get; set; }

    [Display(Name = "Status")]
    public MembershipStatus Status { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

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

    [Display(Name = "Sponsor")]
    public string? SponsorId { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "Monthly Fee")]
    public decimal? MonthlyFee { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Membership Start Date")]
    public DateTime? MembershipStartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Membership End Date")]
    public DateTime? MembershipEndDate { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "Admin Notes")]
    public string? AdminNotes { get; set; }

    public string? ProfilePictureUrl { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }

    [Display(Name = "Profile Photo")]
    public IFormFile? ProfilePhoto { get; set; }

    [Display(Name = "Selected Avatar")]
    public string? SelectedAvatarUrl { get; set; }

    // Available sponsors for dropdown
    public IEnumerable<UserSelectItem> AvailableSponsors { get; set; } = new List<UserSelectItem>();
}

public class UserDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole UserRole { get; set; }
    public MembershipStatus Status { get; set; }
    public bool IsActive { get; set; }
    public int? Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string FullAddress => string.Join(", ", new[] { Address, City, State, ZipCode }.Where(s => !string.IsNullOrEmpty(s)));
    
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    public string? SponsorId { get; set; }
    public string? SponsorName { get; set; }
    public IEnumerable<UserSummaryViewModel> SponsoredClients { get; set; } = new List<UserSummaryViewModel>();
    
    public decimal? MonthlyFee { get; set; }
    public DateTime? MembershipStartDate { get; set; }
    public DateTime? MembershipEndDate { get; set; }
    
    public string? AdminNotes { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    public DateTime JoinedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}

public class UserSelectItem
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class UserDeleteViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole UserRole { get; set; }
    public int SponsoredClientsCount { get; set; }
    public bool HasSponsoredClients => SponsoredClientsCount > 0;
}
