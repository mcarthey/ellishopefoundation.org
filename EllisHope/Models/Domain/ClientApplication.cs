using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EllisHope.Models.Domain;

/// <summary>
/// Client application for program sponsorship and funding
/// Represents a formal request for charitable assistance
/// </summary>
public class ClientApplication
{
    public int Id { get; set; }
    
    // Applicant Reference
    [Required]
    public string ApplicantId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(ApplicantId))]
    public virtual ApplicationUser Applicant { get; set; } = null!;
    
    // Application Status
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
    
    #region Personal Information
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(2)]
    public string? State { get; set; }
    
    [StringLength(10)]
    public string? ZipCode { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Occupation { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(100)]
    public string? EmergencyContactName { get; set; }
    
    [Phone]
    public string? EmergencyContactPhone { get; set; }
    
    #endregion
    
    #region Program Interest & Funding
    
    /// <summary>
    /// Types of funding/support requested (can be multiple)
    /// Stored as comma-separated values
    /// </summary>
    [Required]
    public string FundingTypesRequested { get; set; } = string.Empty;
    
    /// <summary>
    /// Estimated monthly cost for requested services
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? EstimatedMonthlyCost { get; set; }
    
    /// <summary>
    /// Requested program duration in months
    /// </summary>
    public int ProgramDurationMonths { get; set; } = 12;
    
    /// <summary>
    /// Specific details about funding needs
    /// </summary>
    [StringLength(1000)]
    public string? FundingDetails { get; set; }
    
    #endregion
    
    #region Motivation & Commitment
    
    /// <summary>
    /// Personal statement - why applicant wants to join program
    /// </summary>
    [Required]
    [StringLength(5000)]
    public string PersonalStatement { get; set; } = string.Empty;
    
    /// <summary>
    /// How the program will benefit the applicant
    /// </summary>
    [Required]
    [StringLength(3000)]
    public string ExpectedBenefits { get; set; } = string.Empty;
    
    /// <summary>
    /// Commitment level explanation - schedule, availability, concerns
    /// </summary>
    [Required]
    [StringLength(3000)]
    public string CommitmentStatement { get; set; } = string.Empty;
    
    /// <summary>
    /// Any obstacles or concerns about participation
    /// </summary>
    [StringLength(2000)]
    public string? ConcernsObstacles { get; set; }
    
    #endregion
    
    #region Health & Fitness
    
    /// <summary>
    /// Pre-existing medical conditions
    /// </summary>
    [StringLength(2000)]
    public string? MedicalConditions { get; set; }
    
    /// <summary>
    /// Current medications being taken
    /// </summary>
    [StringLength(2000)]
    public string? CurrentMedications { get; set; }
    
    /// <summary>
    /// Date of last physical examination
    /// </summary>
    public DateTime? LastPhysicalExamDate { get; set; }
    
    /// <summary>
    /// Specific fitness goals
    /// </summary>
    [StringLength(1000)]
    public string? FitnessGoals { get; set; }
    
    /// <summary>
    /// Current fitness level assessment
    /// </summary>
    public FitnessLevel CurrentFitnessLevel { get; set; } = FitnessLevel.Beginner;
    
    #endregion
    
    #region Program Requirements Agreement
    
    /// <summary>
    /// Agrees to work with nutritionist
    /// </summary>
    public bool AgreesToNutritionist { get; set; }
    
    /// <summary>
    /// Agrees to work with personal trainer
    /// </summary>
    public bool AgreesToPersonalTrainer { get; set; }
    
    /// <summary>
    /// Agrees to weekly check-in calls
    /// </summary>
    public bool AgreesToWeeklyCheckIns { get; set; }
    
    /// <summary>
    /// Agrees to monthly progress reports
    /// </summary>
    public bool AgreesToProgressReports { get; set; }
    
    /// <summary>
    /// Understands 12-month commitment requirement
    /// </summary>
    public bool UnderstandsCommitment { get; set; }
    
    #endregion
    
    #region Supporting Documents
    
    /// <summary>
    /// URL/path to medical clearance letter (if uploaded)
    /// </summary>
    public string? MedicalClearanceDocumentUrl { get; set; }
    
    /// <summary>
    /// URL/path to reference letters (if uploaded)
    /// </summary>
    public string? ReferenceLettersDocumentUrl { get; set; }
    
    /// <summary>
    /// URL/path to income verification (if uploaded)
    /// </summary>
    public string? IncomeVerificationDocumentUrl { get; set; }
    
    /// <summary>
    /// Other supporting documents
    /// </summary>
    public string? OtherDocumentsUrl { get; set; }
    
    #endregion
    
    #region Signature & Consent
    
    /// <summary>
    /// Digital signature (applicant's name)
    /// </summary>
    [StringLength(200)]
    public string? Signature { get; set; }
    
    /// <summary>
    /// Date application was signed/submitted
    /// </summary>
    public DateTime? SignedDate { get; set; }
    
    /// <summary>
    /// IP address at time of submission (for verification)
    /// </summary>
    [StringLength(50)]
    public string? SubmissionIpAddress { get; set; }
    
    #endregion
    
    #region Review & Decision
    
    /// <summary>
    /// Board members who must review this application
    /// </summary>
    public virtual ICollection<ApplicationVote> Votes { get; set; } = new List<ApplicationVote>();
    
    /// <summary>
    /// Discussion comments from board members
    /// </summary>
    public virtual ICollection<ApplicationComment> Comments { get; set; } = new List<ApplicationComment>();
    
    /// <summary>
    /// Number of votes required for approval (set based on active board members)
    /// </summary>
    public int VotesRequiredForApproval { get; set; }
    
    /// <summary>
    /// Date application was submitted for review
    /// </summary>
    public DateTime? SubmittedDate { get; set; }
    
    /// <summary>
    /// Date review process started
    /// </summary>
    public DateTime? ReviewStartedDate { get; set; }
    
    /// <summary>
    /// Date final decision was made
    /// </summary>
    public DateTime? DecisionDate { get; set; }
    
    /// <summary>
    /// Final decision outcome
    /// </summary>
    public DecisionOutcome? FinalDecision { get; set; }
    
    /// <summary>
    /// Official decision message sent to applicant
    /// </summary>
    [StringLength(2000)]
    public string? DecisionMessage { get; set; }
    
    /// <summary>
    /// ID of board member who finalized the decision
    /// </summary>
    public string? DecisionMadeById { get; set; }
    
    [ForeignKey(nameof(DecisionMadeById))]
    public virtual ApplicationUser? DecisionMadeBy { get; set; }
    
    #endregion
    
    #region Post-Approval
    
    /// <summary>
    /// Assigned sponsor (if applicable)
    /// </summary>
    public string? AssignedSponsorId { get; set; }
    
    [ForeignKey(nameof(AssignedSponsorId))]
    public virtual ApplicationUser? AssignedSponsor { get; set; }
    
    /// <summary>
    /// Date sponsorship/program started
    /// </summary>
    public DateTime? ProgramStartDate { get; set; }
    
    /// <summary>
    /// Expected program end date
    /// </summary>
    public DateTime? ProgramEndDate { get; set; }
    
    /// <summary>
    /// Monthly funding amount approved
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? ApprovedMonthlyAmount { get; set; }
    
    #endregion
    
    #region Audit Fields
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(450)]
    public string? LastModifiedById { get; set; }
    
    [ForeignKey(nameof(LastModifiedById))]
    public virtual ApplicationUser? LastModifiedBy { get; set; }
    
    #endregion
    
    #region Computed Properties

    /// <summary>
    /// Whether the application is in a state where voting is applicable
    /// </summary>
    [NotMapped]
    public bool IsInReviewableState => Status == ApplicationStatus.UnderReview
                                    || Status == ApplicationStatus.InDiscussion;

    /// <summary>
    /// Full name of applicant
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    /// <summary>
    /// Full address formatted
    /// </summary>
    [NotMapped]
    public string FullAddress => string.Join(", ", new[] { Address, City, State, ZipCode }
        .Where(s => !string.IsNullOrWhiteSpace(s)));
    
    /// <summary>
    /// Days since application was submitted
    /// </summary>
    [NotMapped]
    public int? DaysSinceSubmission => SubmittedDate.HasValue 
        ? (DateTime.UtcNow - SubmittedDate.Value).Days 
        : null;
    
    /// <summary>
    /// Count of approval votes received
    /// </summary>
    [NotMapped]
    public int ApprovalVoteCount => Votes?.Count(v => v.Decision == VoteDecision.Approve) ?? 0;
    
    /// <summary>
    /// Count of rejection votes received
    /// </summary>
    [NotMapped]
    public int RejectionVoteCount => Votes?.Count(v => v.Decision == VoteDecision.Reject) ?? 0;
    
    /// <summary>
    /// Total votes cast
    /// </summary>
    [NotMapped]
    public int TotalVotesCast => Votes?.Count(v => v.Decision != VoteDecision.Abstain) ?? 0;
    
    /// <summary>
    /// Whether application has enough votes for decision.
    /// Only returns true when in a reviewable state to avoid false positives for Draft applications.
    /// </summary>
    [NotMapped]
    public bool HasSufficientVotes => IsInReviewableState && TotalVotesCast >= VotesRequiredForApproval;

    /// <summary>
    /// Whether application is approved based on votes.
    /// Only returns true when in a reviewable state to avoid false positives.
    /// </summary>
    [NotMapped]
    public bool IsApprovedByVotes => IsInReviewableState && ApprovalVoteCount >= VotesRequiredForApproval;
    
    /// <summary>
    /// Whether any board member rejected
    /// </summary>
    [NotMapped]
    public bool HasAnyRejection => RejectionVoteCount > 0;
    
    /// <summary>
    /// List of funding types as array
    /// </summary>
    [NotMapped]
    public IEnumerable<FundingType> FundingTypesList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FundingTypesRequested))
                return Enumerable.Empty<FundingType>();
            
            return FundingTypesRequested
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.Parse<FundingType>(s.Trim()));
        }
    }
    
    #endregion
}

/// <summary>
/// Application lifecycle status
/// </summary>
public enum ApplicationStatus
{
    Draft = 0,              // Being created by applicant
    Submitted = 1,          // Submitted and awaiting review
    UnderReview = 2,        // Board members reviewing
    NeedsInformation = 3,   // Additional info requested from applicant
    InDiscussion = 4,       // Board members discussing/voting
    Approved = 5,           // Approved for program
    Rejected = 6,           // Not approved
    Active = 7,             // Program started
    Completed = 8,          // Program finished successfully
    Withdrawn = 9,          // Applicant withdrew application
    Expired = 10            // Application expired (no decision made)
}

/// <summary>
/// Types of funding/support available
/// </summary>
public enum FundingType
{
    GymMembership,
    PersonalTraining,
    NutritionistConsultation,
    FitnessApparel,
    FitnessEquipment,
    NutritionSupplements,
    GroupClasses,
    OnlinePrograms,
    Other
}

/// <summary>
/// Applicant's current fitness level
/// </summary>
public enum FitnessLevel
{
    Beginner,
    Intermediate,
    Advanced
}

/// <summary>
/// Final decision on application
/// </summary>
public enum DecisionOutcome
{
    Approved,
    Rejected,
    NeedsMoreInformation,
    Deferred
}
