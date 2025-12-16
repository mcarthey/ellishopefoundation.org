using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EllisHope.Services;

/// <summary>
/// Service for managing client applications and approval workflow
/// </summary>
public class ClientApplicationService : IClientApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientApplicationService> _logger;
    private readonly IEmailService? _emailService;

    public ClientApplicationService(
        ApplicationDbContext context,
        ILogger<ClientApplicationService> logger,
        IEmailService? emailService = null)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    #region Application CRUD Operations

    public async Task<IEnumerable<ClientApplication>> GetAllApplicationsAsync(
        ApplicationStatus? status = null,
        string? applicantId = null,
        bool includeVotes = false,
        bool includeComments = false)
    {
        var query = _context.ClientApplications
            .Include(ca => ca.Applicant)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(ca => ca.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(applicantId))
        {
            query = query.Where(ca => ca.ApplicantId == applicantId);
        }

        if (includeVotes)
        {
            query = query.Include(ca => ca.Votes)
                .ThenInclude(v => v.Voter);
        }

        if (includeComments)
        {
            query = query.Include(ca => ca.Comments)
                .ThenInclude(c => c.Author);
        }

        return await query
            .OrderByDescending(ca => ca.SubmittedDate ?? ca.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClientApplication>> GetApplicationsNeedingReviewAsync(string boardMemberId)
    {
        return await _context.ClientApplications
            .Include(ca => ca.Applicant)
            .Include(ca => ca.Votes)
            .Where(ca => ca.Status == ApplicationStatus.UnderReview || ca.Status == ApplicationStatus.InDiscussion)
            .Where(ca => !ca.Votes.Any(v => v.VoterId == boardMemberId))
            .OrderBy(ca => ca.SubmittedDate)
            .ToListAsync();
    }

    public async Task<ClientApplication?> GetApplicationByIdAsync(int id, bool includeVotes = true, bool includeComments = true)
    {
        var query = _context.ClientApplications
            .Include(ca => ca.Applicant)
            .Include(ca => ca.AssignedSponsor)
            .Include(ca => ca.DecisionMadeBy)
            .AsQueryable();

        if (includeVotes)
        {
            query = query.Include(ca => ca.Votes)
                .ThenInclude(v => v.Voter);
        }

        if (includeComments)
        {
            query = query.Include(ca => ca.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Author);
        }

        return await query.FirstOrDefaultAsync(ca => ca.Id == id);
    }

    public async Task<IEnumerable<ClientApplication>> GetApplicationsByApplicantAsync(string applicantId)
    {
        return await _context.ClientApplications
            .Include(ca => ca.AssignedSponsor)
            .Where(ca => ca.ApplicantId == applicantId)
            .OrderByDescending(ca => ca.SubmittedDate ?? ca.CreatedDate)
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string[] Errors, ClientApplication? Application)> CreateApplicationAsync(
        ClientApplication application)
    {
        try
        {
            application.Status = ApplicationStatus.Draft;
            application.CreatedDate = DateTime.UtcNow;
            application.ModifiedDate = DateTime.UtcNow;

            _context.ClientApplications.Add(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Application created: {application.Id} by {application.ApplicantId}");
            return (true, Array.Empty<string>(), application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating application for {application.ApplicantId}");
            return (false, new[] { ex.Message }, null);
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateApplicationAsync(ClientApplication application)
    {
        try
        {
            application.ModifiedDate = DateTime.UtcNow;
            _context.ClientApplications.Update(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Application updated: {application.Id}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating application {application.Id}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> DeleteApplicationAsync(int id)
    {
        try
        {
            var application = await _context.ClientApplications.FindAsync(id);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            if (application.Status != ApplicationStatus.Draft)
            {
                return (false, new[] { "Only draft applications can be deleted" });
            }

            _context.ClientApplications.Remove(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Application deleted: {id}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting application {id}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> SubmitApplicationAsync(int id, string applicantId)
    {
        try
        {
            var application = await _context.ClientApplications.FindAsync(id);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            if (application.ApplicantId != applicantId)
            {
                return (false, new[] { "Unauthorized" });
            }

            if (application.Status != ApplicationStatus.Draft)
            {
                return (false, new[] { "Application has already been submitted" });
            }

            application.Status = ApplicationStatus.Submitted;
            application.SubmittedDate = DateTime.UtcNow;
            application.SignedDate = DateTime.UtcNow;
            
            // Set votes required based on active board members
            var activeBoardMembers = await _context.Users
                .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
                .CountAsync();
            application.VotesRequiredForApproval = activeBoardMembers;

            await _context.SaveChangesAsync();

            // Notify applicant
            await SendNotificationAsync(
                applicantId,
                NotificationType.ApplicationSubmitted,
                "Application Submitted",
                "Your application has been successfully submitted and is pending review.",
                id,
                sendEmail: true);

            _logger.LogInformation($"Application submitted: {id}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error submitting application {id}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> WithdrawApplicationAsync(int id, string applicantId, string reason)
    {
        try
        {
            var application = await _context.ClientApplications.FindAsync(id);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            if (application.ApplicantId != applicantId)
            {
                return (false, new[] { "Unauthorized" });
            }

            application.Status = ApplicationStatus.Withdrawn;
            application.DecisionMessage = $"Withdrawn by applicant: {reason}";
            application.DecisionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Application withdrawn: {id}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error withdrawing application {id}");
            return (false, new[] { ex.Message });
        }
    }

    #endregion

    #region Voting Operations

    public async Task<(bool Succeeded, string[] Errors)> CastVoteAsync(
        int applicationId,
        string voterId,
        VoteDecision decision,
        string reasoning,
        int confidenceLevel = 3)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            if (application.Status != ApplicationStatus.UnderReview && 
                application.Status != ApplicationStatus.InDiscussion)
            {
                return (false, new[] { "Application is not open for voting" });
            }

            var existingVote = await _context.ApplicationVotes
                .FirstOrDefaultAsync(v => v.ApplicationId == applicationId && v.VoterId == voterId);

            if (existingVote != null)
            {
                if (existingVote.IsLocked)
                {
                    return (false, new[] { "Vote is locked and cannot be changed" });
                }

                existingVote.Decision = decision;
                existingVote.Reasoning = reasoning;
                existingVote.ConfidenceLevel = confidenceLevel;
                existingVote.ModifiedDate = DateTime.UtcNow;
                _context.ApplicationVotes.Update(existingVote);
            }
            else
            {
                var vote = new ApplicationVote
                {
                    ApplicationId = applicationId,
                    VoterId = voterId,
                    Decision = decision,
                    Reasoning = reasoning,
                    ConfidenceLevel = confidenceLevel,
                    VotedDate = DateTime.UtcNow
                };
                _context.ApplicationVotes.Add(vote);
            }

            // Update application status if moving to discussion
            if (application.Status == ApplicationStatus.UnderReview)
            {
                application.Status = ApplicationStatus.InDiscussion;
            }

            await _context.SaveChangesAsync();

            // Check if we have enough votes for decision
            var summary = await GetVotingSummaryAsync(applicationId);
            if (summary.HasSufficientVotes)
            {
                // Notify board that quorum is reached
                var boardMembers = await _context.Users
                    .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync();

                await SendBulkNotificationAsync(
                    boardMembers,
                    NotificationType.QuorumReached,
                    "Quorum Reached",
                    $"Application #{applicationId} has received all required votes.",
                    applicationId);
            }

            _logger.LogInformation($"Vote cast on application {applicationId} by {voterId}: {decision}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error casting vote on application {applicationId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<ApplicationVote?> GetVoteAsync(int applicationId, string voterId)
    {
        return await _context.ApplicationVotes
            .Include(v => v.Voter)
            .FirstOrDefaultAsync(v => v.ApplicationId == applicationId && v.VoterId == voterId);
    }

    public async Task<IEnumerable<ApplicationVote>> GetApplicationVotesAsync(int applicationId)
    {
        return await _context.ApplicationVotes
            .Include(v => v.Voter)
            .Where(v => v.ApplicationId == applicationId)
            .OrderBy(v => v.VotedDate)
            .ToListAsync();
    }

    public async Task<bool> HasVotedAsync(int applicationId, string boardMemberId)
    {
        return await _context.ApplicationVotes
            .AnyAsync(v => v.ApplicationId == applicationId && v.VoterId == boardMemberId);
    }

    public async Task<VotingSummary> GetVotingSummaryAsync(int applicationId)
    {
        var application = await GetApplicationByIdAsync(applicationId);
        if (application == null)
        {
            return new VotingSummary();
        }

        var votes = await GetApplicationVotesAsync(applicationId);
        var boardMembers = await _context.Users
            .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
            .ToListAsync();

        var votedMemberIds = votes.Select(v => v.VoterId).ToHashSet();
        var pendingVoters = boardMembers
            .Where(bm => !votedMemberIds.Contains(bm.Id))
            .Select(bm => bm.FullName)
            .ToList();

        return new VotingSummary
        {
            TotalVotesCast = votes.Count(v => v.Decision != VoteDecision.Abstain),
            ApprovalVotes = votes.Count(v => v.Decision == VoteDecision.Approve),
            RejectionVotes = votes.Count(v => v.Decision == VoteDecision.Reject),
            NeedsInfoVotes = votes.Count(v => v.Decision == VoteDecision.NeedsMoreInfo),
            AbstainVotes = votes.Count(v => v.Decision == VoteDecision.Abstain),
            VotesRequired = application.VotesRequiredForApproval,
            HasSufficientVotes = votes.Count() >= application.VotesRequiredForApproval,
            IsApproved = votes.Count(v => v.Decision == VoteDecision.Approve) >= application.VotesRequiredForApproval,
            HasAnyRejection = votes.Any(v => v.Decision == VoteDecision.Reject),
            PendingVoters = pendingVoters
        };
    }

    public async Task<bool> HasSufficientVotesAsync(int applicationId)
    {
        var summary = await GetVotingSummaryAsync(applicationId);
        return summary.HasSufficientVotes;
    }

    public async Task<(bool Succeeded, string[] Errors, DecisionOutcome? Decision)> ProcessApplicationDecisionAsync(
        int applicationId,
        string decisionMakerId)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" }, null);
            }

            var summary = await GetVotingSummaryAsync(applicationId);
            
            if (!summary.HasSufficientVotes)
            {
                return (false, new[] { "Not all board members have voted yet" }, null);
            }

            DecisionOutcome outcome;
            
            if (summary.HasAnyRejection)
            {
                outcome = DecisionOutcome.Rejected;
                application.Status = ApplicationStatus.Rejected;
            }
            else if (summary.IsApproved)
            {
                outcome = DecisionOutcome.Approved;
                application.Status = ApplicationStatus.Approved;
            }
            else if (summary.NeedsInfoVotes > 0)
            {
                outcome = DecisionOutcome.NeedsMoreInformation;
                application.Status = ApplicationStatus.NeedsInformation;
            }
            else
            {
                outcome = DecisionOutcome.Deferred;
            }

            application.FinalDecision = outcome;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionMadeById = decisionMakerId;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Application {applicationId} decision: {outcome}");
            return (true, Array.Empty<string>(), outcome);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing decision for application {applicationId}");
            return (false, new[] { ex.Message }, null);
        }
    }

    #endregion

    #region Comment Operations

    public async Task<(bool Succeeded, string[] Errors, ApplicationComment? Comment)> AddCommentAsync(
        int applicationId,
        string authorId,
        string content,
        bool isPrivate = true,
        bool isInformationRequest = false,
        int? parentCommentId = null)
    {
        try
        {
            var comment = new ApplicationComment
            {
                ApplicationId = applicationId,
                AuthorId = authorId,
                Content = content,
                IsPrivate = isPrivate,
                IsInformationRequest = isInformationRequest,
                ParentCommentId = parentCommentId,
                CreatedDate = DateTime.UtcNow
            };

            _context.ApplicationComments.Add(comment);
            await _context.SaveChangesAsync();

            // Notify board members of new comment
            if (!isPrivate || isInformationRequest)
            {
                var boardMembers = await _context.Users
                    .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive && u.Id != authorId)
                    .Select(u => u.Id)
                    .ToListAsync();

                await SendBulkNotificationAsync(
                    boardMembers,
                    NotificationType.DiscussionCommentAdded,
                    "New Comment Added",
                    $"A new comment was added to application #{applicationId}",
                    applicationId);
            }

            _logger.LogInformation($"Comment added to application {applicationId} by {authorId}");
            return (true, Array.Empty<string>(), comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding comment to application {applicationId}");
            return (false, new[] { ex.Message }, null);
        }
    }

    public async Task<IEnumerable<ApplicationComment>> GetApplicationCommentsAsync(
        int applicationId,
        bool includePrivate = true,
        bool includeReplies = true)
    {
        var query = _context.ApplicationComments
            .Include(c => c.Author)
            .Where(c => c.ApplicationId == applicationId && !c.IsDeleted);

        if (!includePrivate)
        {
            query = query.Where(c => !c.IsPrivate);
        }

        if (includeReplies)
        {
            query = query.Include(c => c.Replies.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Author);
        }
        else
        {
            query = query.Where(c => c.ParentCommentId == null);
        }

        return await query
            .OrderBy(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateCommentAsync(
        int commentId,
        string authorId,
        string newContent)
    {
        try
        {
            var comment = await _context.ApplicationComments.FindAsync(commentId);
            if (comment == null)
            {
                return (false, new[] { "Comment not found" });
            }

            if (comment.AuthorId != authorId)
            {
                return (false, new[] { "Unauthorized" });
            }

            comment.Content = newContent;
            comment.ModifiedDate = DateTime.UtcNow;
            comment.IsEdited = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Comment {commentId} updated");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating comment {commentId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> DeleteCommentAsync(int commentId, string authorId)
    {
        try
        {
            var comment = await _context.ApplicationComments.FindAsync(commentId);
            if (comment == null)
            {
                return (false, new[] { "Comment not found" });
            }

            if (comment.AuthorId != authorId)
            {
                return (false, new[] { "Unauthorized" });
            }

            comment.IsDeleted = true;
            comment.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Comment {commentId} deleted");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting comment {commentId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task MarkInformationRequestRespondedAsync(int commentId)
    {
        var comment = await _context.ApplicationComments.FindAsync(commentId);
        if (comment != null)
        {
            comment.HasResponse = true;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Notification Operations

    public async Task SendNotificationAsync(
        string recipientId,
        NotificationType type,
        string title,
        string message,
        int? applicationId = null,
        string? actionUrl = null,
        bool sendEmail = false)
    {
        try
        {
            var notification = new ApplicationNotification
            {
                RecipientId = recipientId,
                ApplicationId = applicationId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                CreatedDate = DateTime.UtcNow,
                IsSent = true,
                SentDate = DateTime.UtcNow
            };

            _context.ApplicationNotifications.Add(notification);
            await _context.SaveChangesAsync();

            if (sendEmail && _emailService != null)
            {
                try
                {
                    var recipient = await _context.Users.FindAsync(recipientId);
                    if (recipient != null && !string.IsNullOrEmpty(recipient.Email))
                    {
                        await _emailService.SendEmailAsync(recipient.Email, title, message);
                        notification.EmailSent = true;
                        notification.EmailSentDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email notification to {recipientId}");
                }
            }

            _logger.LogInformation($"Notification sent to {recipientId}: {type}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification to {recipientId}");
        }
    }

    public async Task SendBulkNotificationAsync(
        IEnumerable<string> recipientIds,
        NotificationType type,
        string title,
        string message,
        int? applicationId = null,
        string? actionUrl = null,
        bool sendEmail = false)
    {
        foreach (var recipientId in recipientIds)
        {
            await SendNotificationAsync(recipientId, type, title, message, applicationId, actionUrl, sendEmail);
        }
    }

    public async Task<IEnumerable<ApplicationNotification>> GetUnreadNotificationsAsync(string userId)
    {
        return await _context.ApplicationNotifications
            .Include(n => n.Application)
            .Where(n => n.RecipientId == userId && !n.IsRead && !n.IsExpired)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task MarkNotificationAsReadAsync(int notificationId, string userId)
    {
        var notification = await _context.ApplicationNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllNotificationsAsReadAsync(string userId)
    {
        var notifications = await _context.ApplicationNotifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        return await _context.ApplicationNotifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead && !n.IsExpired);
    }

    #endregion

    #region Workflow Operations

    public async Task<(bool Succeeded, string[] Errors)> StartReviewProcessAsync(int applicationId)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            if (application.Status != ApplicationStatus.Submitted)
            {
                return (false, new[] { "Application is not in submitted status" });
            }

            application.Status = ApplicationStatus.UnderReview;
            application.ReviewStartedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify applicant
            await SendNotificationAsync(
                application.ApplicantId,
                NotificationType.ApplicationUnderReview,
                "Application Under Review",
                "Your application is now being reviewed by our board members.",
                applicationId,
                sendEmail: true);

            // Notify all board members
            var boardMembers = await _context.Users
                .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            await SendBulkNotificationAsync(
                boardMembers,
                NotificationType.NewApplicationReceived,
                "New Application Received",
                $"A new application from {application.FullName} is ready for review.",
                applicationId,
                actionUrl: $"/Admin/Applications/Review/{applicationId}",
                sendEmail: true);

            _logger.LogInformation($"Review process started for application {applicationId}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting review for application {applicationId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> RequestAdditionalInformationAsync(
        int applicationId,
        string requesterId,
        string requestDetails)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            application.Status = ApplicationStatus.NeedsInformation;
            await _context.SaveChangesAsync();

            // Add comment with information request
            await AddCommentAsync(
                applicationId,
                requesterId,
                requestDetails,
                isPrivate: false,
                isInformationRequest: true);

            // Notify applicant
            await SendNotificationAsync(
                application.ApplicantId,
                NotificationType.InformationRequested,
                "Additional Information Requested",
                "The board has requested additional information regarding your application.",
                applicationId,
                sendEmail: true);

            _logger.LogInformation($"Additional information requested for application {applicationId}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error requesting information for application {applicationId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> ApproveApplicationAsync(
        int applicationId,
        string approverId,
        decimal? approvedAmount = null,
        string? sponsorId = null,
        string? decisionMessage = null)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            application.Status = ApplicationStatus.Approved;
            application.FinalDecision = DecisionOutcome.Approved;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionMadeById = approverId;
            application.ApprovedMonthlyAmount = approvedAmount;
            application.AssignedSponsorId = sponsorId;
            application.DecisionMessage = decisionMessage ?? "Your application has been approved!";

            // Lock all votes
            var votes = await _context.ApplicationVotes
                .Where(v => v.ApplicationId == applicationId)
                .ToListAsync();
            
            foreach (var vote in votes)
            {
                vote.IsLocked = true;
            }

            await _context.SaveChangesAsync();

            // Notify applicant
            await SendNotificationAsync(
                application.ApplicantId,
                NotificationType.ApplicationApproved,
                "Application Approved!",
                application.DecisionMessage,
                applicationId,
                sendEmail: true);

            // Notify sponsor if assigned
            if (!string.IsNullOrEmpty(sponsorId))
            {
                await SendNotificationAsync(
                    sponsorId,
                    NotificationType.SponsorAssigned,
                    "New Client Assigned",
                    $"You have been assigned to support {application.FullName}.",
                    applicationId,
                    sendEmail: true);
            }

            _logger.LogInformation($"Application {applicationId} approved");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error approving application {applicationId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> RejectApplicationAsync(
        int applicationId,
        string rejectorId,
        string rejectionReason)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            application.Status = ApplicationStatus.Rejected;
            application.FinalDecision = DecisionOutcome.Rejected;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionMadeById = rejectorId;
            application.DecisionMessage = rejectionReason;

            // Lock all votes
            var votes = await _context.ApplicationVotes
                .Where(v => v.ApplicationId == applicationId)
                .ToListAsync();
            
            foreach (var vote in votes)
            {
                vote.IsLocked = true;
            }

            await _context.SaveChangesAsync();

            // Notify applicant
            await SendNotificationAsync(
                application.ApplicantId,
                NotificationType.ApplicationRejected,
                "Application Decision",
                "We regret to inform you that your application was not approved at this time.",
                applicationId,
                sendEmail: true);

            _logger.LogInformation($"Application {applicationId} rejected");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting application {applicationId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> StartProgramAsync(
        int applicationId,
        DateTime startDate,
        int durationMonths = 12)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            if (application.Status != ApplicationStatus.Approved)
            {
                return (false, new[] { "Application must be approved first" });
            }

            application.Status = ApplicationStatus.Active;
            application.ProgramStartDate = startDate;
            application.ProgramEndDate = startDate.AddMonths(durationMonths);

            await _context.SaveChangesAsync();

            // Notify applicant
            await SendNotificationAsync(
                application.ApplicantId,
                NotificationType.ProgramStarting,
                "Program Starting",
                $"Your program will start on {startDate:MMMM dd, yyyy}.",
                applicationId,
                sendEmail: true);

            _logger.LogInformation($"Program started for application {applicationId}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting program for application {applicationId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> CompleteProgramAsync(int applicationId)
    {
        try
        {
            var application = await GetApplicationByIdAsync(applicationId);
            if (application == null)
            {
                return (false, new[] { "Application not found" });
            }

            application.Status = ApplicationStatus.Completed;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Program completed for application {applicationId}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing program for application {applicationId}");
            return (false, new[] { ex.Message });
        }
    }

    #endregion

    #region Statistics & Reporting

    public async Task<ApplicationStatistics> GetApplicationStatisticsAsync()
    {
        var applications = await _context.ClientApplications.ToListAsync();

        var total = applications.Count;
        var approved = applications.Count(a => a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Active);

        return new ApplicationStatistics
        {
            TotalApplications = total,
            PendingReview = applications.Count(a => a.Status == ApplicationStatus.Submitted),
            UnderReview = applications.Count(a => a.Status == ApplicationStatus.UnderReview || a.Status == ApplicationStatus.InDiscussion),
            Approved = approved,
            Rejected = applications.Count(a => a.Status == ApplicationStatus.Rejected),
            Active = applications.Count(a => a.Status == ApplicationStatus.Active),
            Completed = applications.Count(a => a.Status == ApplicationStatus.Completed),
            ApprovalRate = total > 0 ? (decimal)approved / total * 100 : 0,
            AverageReviewDays = applications
                .Where(a => a.SubmittedDate.HasValue && a.DecisionDate.HasValue)
                .Average(a => (a.DecisionDate!.Value - a.SubmittedDate!.Value).TotalDays)
        };
    }

    public async Task<BoardMemberStatistics> GetBoardMemberStatisticsAsync(string boardMemberId)
    {
        var votes = await _context.ApplicationVotes
            .Where(v => v.VoterId == boardMemberId)
            .ToListAsync();

        var pendingApplications = await GetApplicationsNeedingReviewAsync(boardMemberId);
        var totalApplications = await _context.ClientApplications
            .CountAsync(a => a.Status == ApplicationStatus.UnderReview || a.Status == ApplicationStatus.InDiscussion);

        return new BoardMemberStatistics
        {
            TotalVotesCast = votes.Count,
            ApprovalsGiven = votes.Count(v => v.Decision == VoteDecision.Approve),
            RejectionsGiven = votes.Count(v => v.Decision == VoteDecision.Reject),
            PendingVotes = pendingApplications.Count(),
            ParticipationRate = totalApplications > 0 ? (double)votes.Count / totalApplications * 100 : 0,
            AverageConfidenceLevel = votes.Any() ? votes.Average(v => v.ConfidenceLevel) : 0
        };
    }

    public async Task<IEnumerable<ClientApplication>> GetApplicationsExpiringSoonAsync(int daysThreshold = 30)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-daysThreshold);

        return await _context.ClientApplications
            .Include(ca => ca.Applicant)
            .Where(ca => (ca.Status == ApplicationStatus.UnderReview || ca.Status == ApplicationStatus.InDiscussion) 
                        && ca.SubmittedDate.HasValue 
                        && ca.SubmittedDate.Value <= thresholdDate)
            .OrderBy(ca => ca.SubmittedDate)
            .ToListAsync();
    }

    #endregion
}
