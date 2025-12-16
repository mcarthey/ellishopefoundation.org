using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service interface for managing client applications and approval workflow
/// </summary>
public interface IClientApplicationService
{
    #region Application CRUD Operations
    
    /// <summary>
    /// Get all applications with optional filtering
    /// </summary>
    Task<IEnumerable<ClientApplication>> GetAllApplicationsAsync(
        ApplicationStatus? status = null,
        string? applicantId = null,
        bool includeVotes = false,
        bool includeComments = false);
    
    /// <summary>
    /// Get applications that need review by a specific board member
    /// </summary>
    Task<IEnumerable<ClientApplication>> GetApplicationsNeedingReviewAsync(string boardMemberId);
    
    /// <summary>
    /// Get application by ID with related data
    /// </summary>
    Task<ClientApplication?> GetApplicationByIdAsync(int id, bool includeVotes = true, bool includeComments = true);
    
    /// <summary>
    /// Get applications for a specific applicant
    /// </summary>
    Task<IEnumerable<ClientApplication>> GetApplicationsByApplicantAsync(string applicantId);
    
    /// <summary>
    /// Create new application (draft)
    /// </summary>
    Task<(bool Succeeded, string[] Errors, ClientApplication? Application)> CreateApplicationAsync(
        ClientApplication application);
    
    /// <summary>
    /// Update existing application
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> UpdateApplicationAsync(ClientApplication application);
    
    /// <summary>
    /// Delete application (only allowed for drafts)
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> DeleteApplicationAsync(int id);
    
    /// <summary>
    /// Submit application for review
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> SubmitApplicationAsync(int id, string applicantId);
    
    /// <summary>
    /// Withdraw application
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> WithdrawApplicationAsync(int id, string applicantId, string reason);
    
    #endregion
    
    #region Voting Operations
    
    /// <summary>
    /// Cast or update vote on an application
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> CastVoteAsync(
        int applicationId, 
        string voterId, 
        VoteDecision decision, 
        string reasoning,
        int confidenceLevel = 3);
    
    /// <summary>
    /// Get vote for specific application and voter
    /// </summary>
    Task<ApplicationVote?> GetVoteAsync(int applicationId, string voterId);
    
    /// <summary>
    /// Get all votes for an application
    /// </summary>
    Task<IEnumerable<ApplicationVote>> GetApplicationVotesAsync(int applicationId);
    
    /// <summary>
    /// Check if board member has voted on application
    /// </summary>
    Task<bool> HasVotedAsync(int applicationId, string boardMemberId);
    
    /// <summary>
    /// Get voting summary (approve count, reject count, etc.)
    /// </summary>
    Task<VotingSummary> GetVotingSummaryAsync(int applicationId);
    
    /// <summary>
    /// Check if application has enough votes for decision
    /// </summary>
    Task<bool> HasSufficientVotesAsync(int applicationId);
    
    /// <summary>
    /// Process application decision based on votes
    /// </summary>
    Task<(bool Succeeded, string[] Errors, DecisionOutcome? Decision)> ProcessApplicationDecisionAsync(
        int applicationId, 
        string decisionMakerId);
    
    #endregion
    
    #region Comment Operations
    
    /// <summary>
    /// Add comment to application
    /// </summary>
    Task<(bool Succeeded, string[] Errors, ApplicationComment? Comment)> AddCommentAsync(
        int applicationId,
        string authorId,
        string content,
        bool isPrivate = true,
        bool isInformationRequest = false,
        int? parentCommentId = null);
    
    /// <summary>
    /// Get all comments for an application
    /// </summary>
    Task<IEnumerable<ApplicationComment>> GetApplicationCommentsAsync(
        int applicationId, 
        bool includePrivate = true,
        bool includeReplies = true);
    
    /// <summary>
    /// Update comment
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> UpdateCommentAsync(
        int commentId, 
        string authorId, 
        string newContent);
    
    /// <summary>
    /// Delete comment (soft delete)
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> DeleteCommentAsync(int commentId, string authorId);
    
    /// <summary>
    /// Mark information request as responded
    /// </summary>
    Task MarkInformationRequestRespondedAsync(int commentId);
    
    #endregion
    
    #region Notification Operations
    
    /// <summary>
    /// Send notification to user(s)
    /// </summary>
    Task SendNotificationAsync(
        string recipientId,
        NotificationType type,
        string title,
        string message,
        int? applicationId = null,
        string? actionUrl = null,
        bool sendEmail = false);
    
    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    Task SendBulkNotificationAsync(
        IEnumerable<string> recipientIds,
        NotificationType type,
        string title,
        string message,
        int? applicationId = null,
        string? actionUrl = null,
        bool sendEmail = false);
    
    /// <summary>
    /// Get unread notifications for user
    /// </summary>
    Task<IEnumerable<ApplicationNotification>> GetUnreadNotificationsAsync(string userId);
    
    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task MarkNotificationAsReadAsync(int notificationId, string userId);
    
    /// <summary>
    /// Mark all notifications as read for user
    /// </summary>
    Task MarkAllNotificationsAsReadAsync(string userId);
    
    /// <summary>
    /// Get unread notification count for user
    /// </summary>
    Task<int> GetUnreadNotificationCountAsync(string userId);
    
    #endregion
    
    #region Workflow Operations
    
    /// <summary>
    /// Move application to review status and notify board members
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> StartReviewProcessAsync(int applicationId);
    
    /// <summary>
    /// Request additional information from applicant
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> RequestAdditionalInformationAsync(
        int applicationId,
        string requesterId,
        string requestDetails);
    
    /// <summary>
    /// Approve application and assign sponsor (if applicable)
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> ApproveApplicationAsync(
        int applicationId,
        string approverId,
        decimal? approvedAmount = null,
        string? sponsorId = null,
        string? decisionMessage = null);
    
    /// <summary>
    /// Reject application
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> RejectApplicationAsync(
        int applicationId,
        string rejectorId,
        string rejectionReason);
    
    /// <summary>
    /// Start program for approved application
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> StartProgramAsync(
        int applicationId,
        DateTime startDate,
        int durationMonths = 12);
    
    /// <summary>
    /// Complete program
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> CompleteProgramAsync(int applicationId);
    
    #endregion
    
    #region Statistics & Reporting
    
    /// <summary>
    /// Get application statistics
    /// </summary>
    Task<ApplicationStatistics> GetApplicationStatisticsAsync();
    
    /// <summary>
    /// Get board member voting statistics
    /// </summary>
    Task<BoardMemberStatistics> GetBoardMemberStatisticsAsync(string boardMemberId);
    
    /// <summary>
    /// Get applications pending review (expired soon)
    /// </summary>
    Task<IEnumerable<ClientApplication>> GetApplicationsExpiringSoonAsync(int daysThreshold = 30);
    
    #endregion
}

/// <summary>
/// Voting summary for an application
/// </summary>
public class VotingSummary
{
    public int TotalVotesCast { get; set; }
    public int ApprovalVotes { get; set; }
    public int RejectionVotes { get; set; }
    public int NeedsInfoVotes { get; set; }
    public int AbstainVotes { get; set; }
    public int VotesRequired { get; set; }
    public bool HasSufficientVotes { get; set; }
    public bool IsApproved { get; set; }
    public bool HasAnyRejection { get; set; }
    public List<string> PendingVoters { get; set; } = new();
}

/// <summary>
/// Application statistics
/// </summary>
public class ApplicationStatistics
{
    public int TotalApplications { get; set; }
    public int PendingReview { get; set; }
    public int UnderReview { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Active { get; set; }
    public int Completed { get; set; }
    public decimal ApprovalRate { get; set; }
    public double AverageReviewDays { get; set; }
}

/// <summary>
/// Board member voting statistics
/// </summary>
public class BoardMemberStatistics
{
    public int TotalVotesCast { get; set; }
    public int ApprovalsGiven { get; set; }
    public int RejectionsGiven { get; set; }
    public int PendingVotes { get; set; }
    public double ParticipationRate { get; set; }
    public double AverageConfidenceLevel { get; set; }
}
