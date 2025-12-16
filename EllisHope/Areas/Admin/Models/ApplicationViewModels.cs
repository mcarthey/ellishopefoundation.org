using EllisHope.Models.Domain;
using EllisHope.Services;
using System.ComponentModel.DataAnnotations;

namespace EllisHope.Areas.Admin.Models;

#region List/Index View Models

/// <summary>
/// View model for applications list (Admin view)
/// </summary>
public class ApplicationListViewModel
{
    public IEnumerable<ApplicationSummaryViewModel> Applications { get; set; } = new List<ApplicationSummaryViewModel>();
    
    // Filters
    public ApplicationStatus? StatusFilter { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? SubmittedAfter { get; set; }
    public DateTime? SubmittedBefore { get; set; }
    
    // Statistics
    public int TotalApplications { get; set; }
    public int PendingReview { get; set; }
    public int UnderReview { get; set; }
    public int NeedingMyVote { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}

/// <summary>
/// Summary view for application in list
/// </summary>
public class ApplicationSummaryViewModel
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public int? DaysSinceSubmission { get; set; }
    public string FundingTypes { get; set; } = string.Empty;
    public decimal? EstimatedMonthlyCost { get; set; }
    
    // Voting info
    public int TotalVotes { get; set; }
    public int VotesRequired { get; set; }
    public int ApprovalVotes { get; set; }
    public int RejectionVotes { get; set; }
    public bool HasUserVoted { get; set; }
    
    // Display helpers
    public string StatusBadgeClass => Status switch
    {
        ApplicationStatus.Draft => "badge-secondary",
        ApplicationStatus.Submitted => "badge-info",
        ApplicationStatus.UnderReview => "badge-primary",
        ApplicationStatus.InDiscussion => "badge-warning",
        ApplicationStatus.NeedsInformation => "badge-warning",
        ApplicationStatus.Approved => "badge-success",
        ApplicationStatus.Rejected => "badge-danger",
        ApplicationStatus.Active => "badge-success",
        ApplicationStatus.Completed => "badge-dark",
        _ => "badge-secondary"
    };
}

#endregion

#region Details/Review View Models

/// <summary>
/// Complete application details for review
/// </summary>
public class ApplicationDetailsViewModel
{
    public ClientApplication Application { get; set; } = null!;
    public VotingSummary VotingSummary { get; set; } = null!;
    public IEnumerable<ApplicationVote> Votes { get; set; } = new List<ApplicationVote>();
    public IEnumerable<ApplicationComment> Comments { get; set; } = new List<ApplicationComment>();
    
    // User context
    public bool CanUserVote { get; set; }
    public bool HasUserVoted { get; set; }
    public ApplicationVote? UserVote { get; set; }
    public bool CanApproveReject { get; set; }
    public bool CanEdit { get; set; }
    
    // Form models
    public VoteFormViewModel VoteForm { get; set; } = new();
    public CommentFormViewModel CommentForm { get; set; } = new();
}

#endregion

#region Create/Edit View Models

/// <summary>
/// Create new application (multi-step form)
/// </summary>
public class ApplicationCreateViewModel
{
    // Step 1: Personal Information
    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Street Address")]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [StringLength(2)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [StringLength(10)]
    [Display(Name = "ZIP Code")]
    public string? ZipCode { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Occupation")]
    public string? Occupation { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(100)]
    [Display(Name = "Emergency Contact Name")]
    public string? EmergencyContactName { get; set; }

    [Phone]
    [Display(Name = "Emergency Contact Phone")]
    public string? EmergencyContactPhone { get; set; }

    // Step 2: Program Interest & Funding
    [Required]
    [Display(Name = "Types of Funding/Support Requested")]
    public List<FundingType> FundingTypesRequested { get; set; } = new();

    [Display(Name = "Estimated Monthly Cost")]
    [DataType(DataType.Currency)]
    public decimal? EstimatedMonthlyCost { get; set; }

    [Display(Name = "Program Duration (months)")]
    [Range(1, 24)]
    public int ProgramDurationMonths { get; set; } = 12;

    [StringLength(1000)]
    [Display(Name = "Please provide specific details about your funding needs")]
    [DataType(DataType.MultilineText)]
    public string? FundingDetails { get; set; }

    // Step 3: Motivation & Commitment
    [Required]
    [StringLength(5000, MinimumLength = 50)]
    [Display(Name = "Describe yourself, why you want to be part of this program, and how you could benefit from it")]
    [DataType(DataType.MultilineText)]
    public string PersonalStatement { get; set; } = string.Empty;

    [Required]
    [StringLength(3000, MinimumLength = 50)]
    [Display(Name = "How will this program benefit you?")]
    [DataType(DataType.MultilineText)]
    public string ExpectedBenefits { get; set; } = string.Empty;

    [Required]
    [StringLength(3000, MinimumLength = 50)]
    [Display(Name = "Consider your schedule and travel requirements. Are you willing and able to devote the time and energy necessary? List any concerns.")]
    [DataType(DataType.MultilineText)]
    public string CommitmentStatement { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "List any concerns or obstacles that might affect your participation")]
    [DataType(DataType.MultilineText)]
    public string? ConcernsObstacles { get; set; }

    // Step 4: Health & Fitness
    [StringLength(2000)]
    [Display(Name = "Describe any pre-existing medical conditions you may have")]
    [DataType(DataType.MultilineText)]
    public string? MedicalConditions { get; set; }

    [StringLength(2000)]
    [Display(Name = "List any medications you are currently taking")]
    [DataType(DataType.MultilineText)]
    public string? CurrentMedications { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "When was your last yearly physical?")]
    public DateTime? LastPhysicalExamDate { get; set; }

    [StringLength(1000)]
    [Display(Name = "What are your specific fitness goals?")]
    [DataType(DataType.MultilineText)]
    public string? FitnessGoals { get; set; }

    [Display(Name = "Current Fitness Level")]
    public FitnessLevel CurrentFitnessLevel { get; set; } = FitnessLevel.Beginner;

    // Step 5: Program Requirements Agreement
    [Display(Name = "Would you agree to a nutritionist helping you with a program?")]
    public bool AgreesToNutritionist { get; set; }

    [Display(Name = "Would you agree to a personal trainer helping you with a program?")]
    public bool AgreesToPersonalTrainer { get; set; }

    [Display(Name = "I agree to participate in weekly check-in calls")]
    public bool AgreesToWeeklyCheckIns { get; set; }

    [Display(Name = "I agree to provide monthly progress reports")]
    public bool AgreesToProgressReports { get; set; }

    [Required(ErrorMessage = "You must acknowledge the 12-month commitment")]
    [Display(Name = "I understand this is a 12-month commitment")]
    public bool UnderstandsCommitment { get; set; }

    // Step 6: Signature & Consent
    [Required]
    [StringLength(200)]
    [Display(Name = "Full Name (Digital Signature)")]
    public string Signature { get; set; } = string.Empty;

    // Hidden fields
    public int CurrentStep { get; set; } = 1;
    public bool SaveAsDraft { get; set; }
}

/// <summary>
/// Edit existing application (limited to drafts)
/// </summary>
public class ApplicationEditViewModel : ApplicationCreateViewModel
{
    public int Id { get; set; }
    public ApplicationStatus Status { get; set; }
}

#endregion

#region Voting View Models

/// <summary>
/// Cast or update vote
/// </summary>
public class VoteFormViewModel
{
    public int ApplicationId { get; set; }

    [Required]
    [Display(Name = "Decision")]
    public VoteDecision Decision { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    [Display(Name = "Reasoning (required)")]
    [DataType(DataType.MultilineText)]
    public string Reasoning { get; set; } = string.Empty;

    [Range(1, 5)]
    [Display(Name = "Confidence Level (1-5)")]
    public int ConfidenceLevel { get; set; } = 3;
}

#endregion

#region Comment View Models

/// <summary>
/// Add comment to application
/// </summary>
public class CommentFormViewModel
{
    public int ApplicationId { get; set; }

    [Required]
    [StringLength(5000, MinimumLength = 5)]
    [Display(Name = "Comment")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Private (board members only)")]
    public bool IsPrivate { get; set; } = true;

    [Display(Name = "Request information from applicant")]
    public bool IsInformationRequest { get; set; }

    public int? ParentCommentId { get; set; }
}

#endregion

#region Decision View Models

/// <summary>
/// Approve application
/// </summary>
public class ApproveApplicationViewModel
{
    public int ApplicationId { get; set; }

    [Display(Name = "Approved Monthly Amount")]
    [DataType(DataType.Currency)]
    public decimal? ApprovedMonthlyAmount { get; set; }

    [Display(Name = "Assign Sponsor (optional)")]
    public string? SponsorId { get; set; }

    [StringLength(2000)]
    [Display(Name = "Decision Message to Applicant")]
    [DataType(DataType.MultilineText)]
    public string? DecisionMessage { get; set; }

    public IEnumerable<SponsorSelectItem> AvailableSponsors { get; set; } = new List<SponsorSelectItem>();
}

/// <summary>
/// Reject application
/// </summary>
public class RejectApplicationViewModel
{
    public int ApplicationId { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    [Display(Name = "Reason for Rejection")]
    [DataType(DataType.MultilineText)]
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// Request additional information
/// </summary>
public class RequestInformationViewModel
{
    public int ApplicationId { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 20)]
    [Display(Name = "Information Requested")]
    [DataType(DataType.MultilineText)]
    public string RequestDetails { get; set; } = string.Empty;
}

#endregion

#region User Portal View Models

/// <summary>
/// User's application list (portal view)
/// </summary>
public class MyApplicationsViewModel
{
    public IEnumerable<MyApplicationSummary> Applications { get; set; } = new List<MyApplicationSummary>();
    public bool CanCreateNew { get; set; } = true;
}

/// <summary>
/// User's application summary
/// </summary>
public class MyApplicationSummary
{
    public int Id { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string FundingTypes { get; set; } = string.Empty;
    public decimal? EstimatedMonthlyCost { get; set; }
    public DecisionOutcome? FinalDecision { get; set; }
    public string? DecisionMessage { get; set; }
    public bool CanEdit { get; set; }
    public bool CanWithdraw { get; set; }
    
    public string StatusDisplay => Status switch
    {
        ApplicationStatus.Draft => "Draft - Not Submitted",
        ApplicationStatus.Submitted => "Submitted - Awaiting Review",
        ApplicationStatus.UnderReview => "Under Review by Board",
        ApplicationStatus.InDiscussion => "Under Discussion",
        ApplicationStatus.NeedsInformation => "Additional Information Requested",
        ApplicationStatus.Approved => "Approved!",
        ApplicationStatus.Rejected => "Not Approved",
        ApplicationStatus.Active => "Active Program",
        ApplicationStatus.Completed => "Program Completed",
        ApplicationStatus.Withdrawn => "Withdrawn",
        _ => Status.ToString()
    };
}

#endregion

#region Helper Classes

public class SponsorSelectItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CurrentClientCount { get; set; }
}

#endregion
