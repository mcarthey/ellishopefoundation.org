using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Unit;

/// <summary>
/// Unit tests for ClientApplicationService
/// Tests application management business logic
/// </summary>
public class ClientApplicationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<ClientApplicationService>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ClientApplicationService _service;

    public ClientApplicationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<ClientApplicationService>>();
        _emailServiceMock = new Mock<IEmailService>();
        _service = new ClientApplicationService(_context, _loggerMock.Object, _emailServiceMock.Object);

        SeedTestData();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedTestData()
    {
        // Seed test users
        var applicant = new ApplicationUser
        {
            Id = "applicant1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRole = UserRole.Member,
            IsActive = true
        };

        var boardMember1 = new ApplicationUser
        {
            Id = "board1",
            FirstName = "Board",
            LastName = "Member1",
            Email = "board1@test.com",
            UserRole = UserRole.BoardMember,
            IsActive = true
        };

        var boardMember2 = new ApplicationUser
        {
            Id = "board2",
            FirstName = "Board",
            LastName = "Member2",
            Email = "board2@test.com",
            UserRole = UserRole.BoardMember,
            IsActive = true
        };

        var admin = new ApplicationUser
        {
            Id = "admin1",
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@test.com",
            UserRole = UserRole.Admin,
            IsActive = true
        };

        _context.Users.AddRange(applicant, boardMember1, boardMember2, admin);

        // Seed test application
        var application = new ClientApplication
        {
            Id = 1,
            ApplicantId = "applicant1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "555-1234",
            Status = ApplicationStatus.Submitted,
            FundingTypesRequested = "GymMembership,PersonalTraining",
            EstimatedMonthlyCost = 150m,
            PersonalStatement = "I want to improve my health and fitness through this program.",
            ExpectedBenefits = "Better health, more energy, improved quality of life.",
            CommitmentStatement = "I am fully committed to completing this 12-month program.",
            UnderstandsCommitment = true,
            Signature = "John Doe",
            SubmittedDate = DateTime.UtcNow.AddDays(-5),
            VotesRequiredForApproval = 2,
            CreatedDate = DateTime.UtcNow.AddDays(-6)
        };

        _context.ClientApplications.Add(application);
        _context.SaveChanges();
    }

    #region Application CRUD Tests

    [Fact]
    public async Task GetAllApplicationsAsync_ReturnsAllApplications()
    {
        // Act
        var applications = await _service.GetAllApplicationsAsync();

        // Assert
        Assert.NotEmpty(applications);
        Assert.Single(applications);
    }

    [Fact]
    public async Task GetAllApplicationsAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        // Act
        var submitted = await _service.GetAllApplicationsAsync(status: ApplicationStatus.Submitted);
        var approved = await _service.GetAllApplicationsAsync(status: ApplicationStatus.Approved);

        // Assert
        Assert.Single(submitted);
        Assert.Empty(approved);
    }

    [Fact]
    public async Task GetApplicationByIdAsync_WithValidId_ReturnsApplication()
    {
        // Act
        var application = await _service.GetApplicationByIdAsync(1);

        // Assert
        Assert.NotNull(application);
        Assert.Equal("John", application.FirstName);
        Assert.Equal("Doe", application.LastName);
    }

    [Fact]
    public async Task GetApplicationByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var application = await _service.GetApplicationByIdAsync(99999);

        // Assert
        Assert.Null(application);
    }

    [Fact]
    public async Task CreateApplicationAsync_CreatesNewApplication()
    {
        // Arrange
        var newApplication = new ClientApplication
        {
            ApplicantId = "applicant1",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com",
            PhoneNumber = "555-5678",
            FundingTypesRequested = "GymMembership",
            PersonalStatement = "Looking to start my fitness journey",
            ExpectedBenefits = "Better health",
            CommitmentStatement = "Fully committed",
            UnderstandsCommitment = true,
            Signature = "Jane Smith"
        };

        // Act
        var (succeeded, errors, application) = await _service.CreateApplicationAsync(newApplication);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.Draft, application.Status);
    }

    [Fact]
    public async Task UpdateApplicationAsync_UpdatesExistingApplication()
    {
        // Arrange
        var application = await _service.GetApplicationByIdAsync(1);
        Assert.NotNull(application);
        application.EstimatedMonthlyCost = 200m;

        // Act
        var (succeeded, errors) = await _service.UpdateApplicationAsync(application);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var updated = await _service.GetApplicationByIdAsync(1);
        Assert.Equal(200m, updated?.EstimatedMonthlyCost);
    }

    [Fact]
    public async Task SubmitApplicationAsync_ChangesStatusToSubmitted()
    {
        // Arrange
        var draftApp = new ClientApplication
        {
            ApplicantId = "applicant1",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "555-0000",
            FundingTypesRequested = "GymMembership",
            PersonalStatement = "Test statement",
            ExpectedBenefits = "Test benefits",
            CommitmentStatement = "Test commitment",
            UnderstandsCommitment = true,
            Signature = "Test User",
            Status = ApplicationStatus.Draft
        };

        var (_, _, created) = await _service.CreateApplicationAsync(draftApp);
        Assert.NotNull(created);

        // Act
        var (succeeded, errors) = await _service.SubmitApplicationAsync(created.Id, "applicant1");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var submitted = await _service.GetApplicationByIdAsync(created.Id);
        Assert.Equal(ApplicationStatus.Submitted, submitted?.Status);
        Assert.NotNull(submitted?.SubmittedDate);
    }

    #endregion

    #region Voting Tests

    [Fact]
    public async Task CastVoteAsync_RecordsVote()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);

        // Act
        var (succeeded, errors) = await _service.CastVoteAsync(
            1,
            "board1",
            VoteDecision.Approve,
            "This applicant shows strong commitment and clear goals.",
            4);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var votes = await _service.GetApplicationVotesAsync(1);
        Assert.Single(votes);
        Assert.Equal(VoteDecision.Approve, votes.First().Decision);
    }

    [Fact]
    public async Task CastVoteAsync_CanUpdateExistingVote()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);
        await _service.CastVoteAsync(1, "board1", VoteDecision.Approve, "Initial vote", 3);

        // Act - Update vote
        var (succeeded, errors) = await _service.CastVoteAsync(
            1,
            "board1",
            VoteDecision.Reject,
            "Changed my mind after further review",
            2);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var vote = await _service.GetVoteAsync(1, "board1");
        Assert.NotNull(vote);
        Assert.Equal(VoteDecision.Reject, vote.Decision);
    }

    [Fact]
    public async Task HasVotedAsync_ReturnsTrueIfVoted()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);
        await _service.CastVoteAsync(1, "board1", VoteDecision.Approve, "Good candidate", 4);

        // Act
        var hasVoted = await _service.HasVotedAsync(1, "board1");
        var hasNotVoted = await _service.HasVotedAsync(1, "board2");

        // Assert
        Assert.True(hasVoted);
        Assert.False(hasNotVoted);
    }

    [Fact]
    public async Task GetVotingSummaryAsync_CalculatesCorrectly()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);
        await _service.CastVoteAsync(1, "board1", VoteDecision.Approve, "Approve reason", 4);
        await _service.CastVoteAsync(1, "board2", VoteDecision.Approve, "Also approve", 5);

        // Act
        var summary = await _service.GetVotingSummaryAsync(1);

        // Assert
        Assert.Equal(2, summary.TotalVotesCast);
        Assert.Equal(2, summary.ApprovalVotes);
        Assert.Equal(0, summary.RejectionVotes);
        Assert.Equal(2, summary.VotesRequired);
        Assert.True(summary.HasSufficientVotes);
        Assert.True(summary.IsApproved);
        Assert.False(summary.HasAnyRejection);
    }

    [Fact]
    public async Task GetVotingSummaryAsync_DetectsRejections()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);
        await _service.CastVoteAsync(1, "board1", VoteDecision.Approve, "Approve", 4);
        await _service.CastVoteAsync(1, "board2", VoteDecision.Reject, "Reject", 3);

        // Act
        var summary = await _service.GetVotingSummaryAsync(1);

        // Assert
        Assert.Equal(2, summary.TotalVotesCast);
        Assert.Equal(1, summary.ApprovalVotes);
        Assert.Equal(1, summary.RejectionVotes);
        Assert.True(summary.HasAnyRejection);
        Assert.False(summary.IsApproved); // One rejection prevents approval
    }

    #endregion

    #region Comment Tests

    [Fact]
    public async Task AddCommentAsync_AddsComment()
    {
        // Act
        var (succeeded, errors, comment) = await _service.AddCommentAsync(
            1,
            "board1",
            "This is a test comment",
            isPrivate: true);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        Assert.NotNull(comment);
        Assert.Equal("This is a test comment", comment.Content);
    }

    [Fact]
    public async Task AddCommentAsync_SupportsThreadedReplies()
    {
        // Arrange
        var (_, _, parentComment) = await _service.AddCommentAsync(
            1, "board1", "Parent comment", true);
        Assert.NotNull(parentComment);

        // Act
        var (succeeded, errors, replyComment) = await _service.AddCommentAsync(
            1,
            "board2",
            "Reply to comment",
            isPrivate: true,
            parentCommentId: parentComment.Id);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
        Assert.NotNull(replyComment);
        Assert.Equal(parentComment.Id, replyComment.ParentCommentId);
    }

    [Fact]
    public async Task GetApplicationCommentsAsync_ReturnsComments()
    {
        // Arrange
        await _service.AddCommentAsync(1, "board1", "Comment 1", true);
        await _service.AddCommentAsync(1, "board2", "Comment 2", true);

        // Act
        var comments = await _service.GetApplicationCommentsAsync(1);

        // Assert
        Assert.Equal(2, comments.Count());
    }

    [Fact]
    public async Task UpdateCommentAsync_UpdatesContent()
    {
        // Arrange
        var (_, _, comment) = await _service.AddCommentAsync(1, "board1", "Original", true);
        Assert.NotNull(comment);

        // Act
        var (succeeded, errors) = await _service.UpdateCommentAsync(
            comment.Id,
            "board1",
            "Updated content");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task StartReviewProcessAsync_ChangesStatusAndNotifies()
    {
        // Act
        var (succeeded, errors) = await _service.StartReviewProcessAsync(1);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var application = await _service.GetApplicationByIdAsync(1);
        Assert.Equal(ApplicationStatus.UnderReview, application?.Status);
        Assert.NotNull(application?.ReviewStartedDate);
    }

    [Fact]
    public async Task ApproveApplicationAsync_SetsApprovedStatus()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);
        await _service.CastVoteAsync(1, "board1", VoteDecision.Approve, "Good", 4);
        await _service.CastVoteAsync(1, "board2", VoteDecision.Approve, "Great", 5);

        // Act
        var (succeeded, errors) = await _service.ApproveApplicationAsync(
            1,
            "admin1",
            approvedAmount: 150m,
            decisionMessage: "Congratulations!");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var application = await _service.GetApplicationByIdAsync(1);
        Assert.Equal(ApplicationStatus.Approved, application?.Status);
        Assert.Equal(DecisionOutcome.Approved, application?.FinalDecision);
        Assert.Equal(150m, application?.ApprovedMonthlyAmount);
    }

    [Fact]
    public async Task RejectApplicationAsync_SetsRejectedStatus()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);
        await _service.CastVoteAsync(1, "board1", VoteDecision.Reject, "No", 2);
        await _service.CastVoteAsync(1, "board2", VoteDecision.Reject, "No", 1);

        // Act
        var (succeeded, errors) = await _service.RejectApplicationAsync(
            1,
            "admin1",
            "Does not meet requirements");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var application = await _service.GetApplicationByIdAsync(1);
        Assert.Equal(ApplicationStatus.Rejected, application?.Status);
        Assert.Equal(DecisionOutcome.Rejected, application?.FinalDecision);
    }

    [Fact]
    public async Task RequestAdditionalInformationAsync_ChangesStatus()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);

        // Act
        var (succeeded, errors) = await _service.RequestAdditionalInformationAsync(
            1,
            "board1",
            "Please provide more details about your fitness goals.");

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);

        var application = await _service.GetApplicationByIdAsync(1);
        Assert.Equal(ApplicationStatus.NeedsInformation, application?.Status);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetApplicationStatisticsAsync_CalculatesCorrectly()
    {
        // Act
        var stats = await _service.GetApplicationStatisticsAsync();

        // Assert
        Assert.Equal(1, stats.TotalApplications);
        Assert.Equal(1, stats.PendingReview);
        Assert.Equal(0, stats.Approved);
        Assert.Equal(0, stats.Rejected);
    }

    [Fact]
    public async Task GetBoardMemberStatisticsAsync_CalculatesParticipation()
    {
        // Arrange
        await _service.StartReviewProcessAsync(1);
        await _service.CastVoteAsync(1, "board1", VoteDecision.Approve, "Good", 4);

        // Act
        var stats = await _service.GetBoardMemberStatisticsAsync("board1");

        // Assert
        Assert.Equal(1, stats.TotalVotesCast);
        Assert.Equal(1, stats.ApprovalsGiven);
        Assert.Equal(0, stats.RejectionsGiven);
        Assert.Equal(4.0, stats.AverageConfidenceLevel);
    }

    #endregion

    #region Notification Tests

    [Fact]
    public async Task SendNotificationAsync_CreatesNotification()
    {
        // Act
        await _service.SendNotificationAsync(
            "applicant1",
            NotificationType.ApplicationSubmitted,
            "Application Submitted",
            "Your application has been submitted.",
            applicationId: 1);

        // Assert
        var notifications = await _service.GetUnreadNotificationsAsync("applicant1");
        Assert.Single(notifications);
        Assert.Equal("Application Submitted", notifications.First().Title);
    }

    [Fact]
    public async Task GetUnreadNotificationCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _service.SendNotificationAsync("applicant1", NotificationType.ApplicationSubmitted, "Test 1", "Message 1", 1);
        await _service.SendNotificationAsync("applicant1", NotificationType.ApplicationUnderReview, "Test 2", "Message 2", 1);

        // Act
        var count = await _service.GetUnreadNotificationCountAsync("applicant1");

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_UpdatesReadStatus()
    {
        // Arrange
        await _service.SendNotificationAsync("applicant1", NotificationType.ApplicationSubmitted, "Test", "Message", 1);
        var notifications = await _service.GetUnreadNotificationsAsync("applicant1");
        var notification = notifications.First();

        // Act
        await _service.MarkNotificationAsReadAsync(notification.Id, "applicant1");

        // Assert
        var unreadCount = await _service.GetUnreadNotificationCountAsync("applicant1");
        Assert.Equal(0, unreadCount);
    }

    #endregion
}
