using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EllisHope.Models.Domain;

/// <summary>
/// Board member vote on a client application
/// Each board member must vote (approve/reject) with reasoning
/// </summary>
public class ApplicationVote
{
    public int Id { get; set; }
    
    // Application Reference
    [Required]
    public int ApplicationId { get; set; }
    
    [ForeignKey(nameof(ApplicationId))]
    public virtual ClientApplication Application { get; set; } = null!;
    
    // Voter Reference
    [Required]
    public string VoterId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(VoterId))]
    public virtual ApplicationUser Voter { get; set; } = null!;
    
    // Vote Details
    [Required]
    public VoteDecision Decision { get; set; }
    
    /// <summary>
    /// Required explanation/reasoning for the vote
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string Reasoning { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence level in this decision (1-5 scale)
    /// </summary>
    public int ConfidenceLevel { get; set; } = 3;
    
    /// <summary>
    /// Whether this vote can be changed
    /// </summary>
    public bool IsLocked { get; set; } = false;
    
    /// <summary>
    /// Date vote was cast
    /// </summary>
    public DateTime VotedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date vote was last modified (if changed)
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
    
    /// <summary>
    /// IP address of voter (for audit trail)
    /// </summary>
    [StringLength(50)]
    public string? VoterIpAddress { get; set; }
    
    #region Computed Properties
    
    /// <summary>
    /// Whether vote is an approval
    /// </summary>
    [NotMapped]
    public bool IsApproval => Decision == VoteDecision.Approve;
    
    /// <summary>
    /// Whether vote is a rejection
    /// </summary>
    [NotMapped]
    public bool IsRejection => Decision == VoteDecision.Reject;
    
    /// <summary>
    /// Whether voter needs more information
    /// </summary>
    [NotMapped]
    public bool NeedsMoreInfo => Decision == VoteDecision.NeedsMoreInfo;
    
    /// <summary>
    /// Days since vote was cast
    /// </summary>
    [NotMapped]
    public int DaysSinceVote => (DateTime.UtcNow - VotedDate).Days;
    
    #endregion
}

/// <summary>
/// Vote decision options
/// </summary>
public enum VoteDecision
{
    /// <summary>
    /// Approve the application
    /// </summary>
    Approve = 1,
    
    /// <summary>
    /// Reject the application
    /// </summary>
    Reject = 2,
    
    /// <summary>
    /// Request more information from applicant
    /// </summary>
    NeedsMoreInfo = 3,
    
    /// <summary>
    /// Abstain from voting (does not count toward quorum)
    /// </summary>
    Abstain = 4
}

/// <summary>
/// Discussion comment on an application
/// Board members can discuss and ask questions
/// </summary>
public class ApplicationComment
{
    public int Id { get; set; }
    
    // Application Reference
    [Required]
    public int ApplicationId { get; set; }
    
    [ForeignKey(nameof(ApplicationId))]
    public virtual ClientApplication Application { get; set; } = null!;
    
    // Author Reference
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(AuthorId))]
    public virtual ApplicationUser Author { get; set; } = null!;
    
    // Comment Content
    [Required]
    [StringLength(5000)]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether comment is only visible to board members
    /// If false, comment may be shared with applicant
    /// </summary>
    public bool IsPrivate { get; set; } = true;
    
    /// <summary>
    /// Whether this is a request for information from applicant
    /// </summary>
    public bool IsInformationRequest { get; set; } = false;
    
    /// <summary>
    /// Whether applicant has responded to this comment
    /// </summary>
    public bool HasResponse { get; set; } = false;
    
    /// <summary>
    /// Parent comment ID (for threaded discussions)
    /// </summary>
    public int? ParentCommentId { get; set; }
    
    [ForeignKey(nameof(ParentCommentId))]
    public virtual ApplicationComment? ParentComment { get; set; }
    
    /// <summary>
    /// Child comments (replies)
    /// </summary>
    public virtual ICollection<ApplicationComment> Replies { get; set; } = new List<ApplicationComment>();
    
    // Audit Fields
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    
    #region Computed Properties
    
    /// <summary>
    /// Whether this is a top-level comment
    /// </summary>
    [NotMapped]
    public bool IsTopLevel => !ParentCommentId.HasValue;
    
    /// <summary>
    /// Number of replies to this comment
    /// </summary>
    [NotMapped]
    public int ReplyCount => Replies?.Count ?? 0;
    
    /// <summary>
    /// Days since comment was created
    /// </summary>
    [NotMapped]
    public int DaysSinceCreated => (DateTime.UtcNow - CreatedDate).Days;
    
    #endregion
}

/// <summary>
/// Notification for application-related events
/// </summary>
public class ApplicationNotification
{
    public int Id { get; set; }
    
    // Recipient
    [Required]
    public string RecipientId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(RecipientId))]
    public virtual ApplicationUser Recipient { get; set; } = null!;
    
    // Related Application (optional - some notifications may be general)
    public int? ApplicationId { get; set; }
    
    [ForeignKey(nameof(ApplicationId))]
    public virtual ClientApplication? Application { get; set; }
    
    // Notification Details
    [Required]
    public NotificationType Type { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// URL to navigate to when notification is clicked
    /// </summary>
    [StringLength(500)]
    public string? ActionUrl { get; set; }
    
    // Status
    public bool IsRead { get; set; } = false;
    public DateTime? ReadDate { get; set; }
    
    public bool IsSent { get; set; } = false;
    public DateTime? SentDate { get; set; }
    
    /// <summary>
    /// Whether email notification was also sent
    /// </summary>
    public bool EmailSent { get; set; } = false;
    public DateTime? EmailSentDate { get; set; }
    
    // Audit
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresDate { get; set; }
    
    #region Computed Properties
    
    /// <summary>
    /// Whether notification has expired
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresDate.HasValue && ExpiresDate.Value < DateTime.UtcNow;
    
    /// <summary>
    /// Days since notification was created
    /// </summary>
    [NotMapped]
    public int DaysOld => (DateTime.UtcNow - CreatedDate).Days;
    
    #endregion
}

/// <summary>
/// Types of notifications
/// </summary>
public enum NotificationType
{
    // Applicant notifications
    ApplicationSubmitted,
    ApplicationUnderReview,
    InformationRequested,
    ApplicationApproved,
    ApplicationRejected,
    SponsorAssigned,
    ProgramStarting,
    
    // Board/Admin notifications
    NewApplicationReceived,
    VoteRequired,
    QuorumReached,
    DiscussionCommentAdded,
    ApplicationExpiringSoon,
    
    // General
    SystemAnnouncement,
    MessageReceived
}
