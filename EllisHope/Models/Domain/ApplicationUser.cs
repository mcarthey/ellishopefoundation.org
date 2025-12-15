using Microsoft.AspNetCore.Identity;

namespace EllisHope.Models.Domain;

/// <summary>
/// Extended user model with profile and membership information
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Basic Info
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Contact Information
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    // Account Status
    public UserRole UserRole { get; set; } = UserRole.Member;
    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Client-Specific Fields
    public string? SponsorId { get; set; }
    public virtual ApplicationUser? Sponsor { get; set; }
    public virtual ICollection<ApplicationUser> SponsoredClients { get; set; } = new List<ApplicationUser>();
    
    public decimal? MonthlyFee { get; set; }
    public DateTime? MembershipStartDate { get; set; }
    public DateTime? MembershipEndDate { get; set; }
    
    // Notes (Admin only)
    public string? AdminNotes { get; set; }
    
    // Computed Properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int? Age
    {
        get
        {
            if (!DateOfBirth.HasValue) return null;
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
    
    public bool IsSponsor => SponsoredClients?.Any() ?? false;
    public bool HasSponsor => !string.IsNullOrEmpty(SponsorId);
}

/// <summary>
/// Primary user role - determines portal access level
/// </summary>
public enum UserRole
{
    Member = 0,         // Basic account holder
    Client = 1,         // Active client with programs
    Sponsor = 2,        // Sponsors clients
    BoardMember = 3,    // Board member with oversight
    Admin = 4           // Full system access
}

/// <summary>
/// Membership/account status
/// </summary>
public enum MembershipStatus
{
    Pending = 0,        // Application submitted, awaiting approval
    Active = 1,         // Current member/client in good standing
    Inactive = 2,       // Temporarily paused (on hold)
    Expired = 3,        // Membership lapsed
    Cancelled = 4       // Account closed
}
