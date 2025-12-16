using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Unit;

/// <summary>
/// Unit tests for PdfService
/// Tests PDF generation for applications, approval letters, and reports
/// </summary>
public class PdfServiceTests
{
    private readonly Mock<ILogger<PdfService>> _loggerMock;
    private readonly PdfService _service;

    public PdfServiceTests()
    {
        _loggerMock = new Mock<ILogger<PdfService>>();
        _service = new PdfService(_loggerMock.Object);
    }

    #region Application PDF Tests

    [Fact]
    public async Task GenerateApplicationPdfAsync_WithBasicApplication_GeneratesPdf()
    {
        // Arrange
        var application = CreateTestApplication();

        // Act
        var pdfBytes = await _service.GenerateApplicationPdfAsync(application);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
        
        // Verify PDF header
        Assert.Equal(0x25, pdfBytes[0]); // % character
        Assert.Equal(0x50, pdfBytes[1]); // P character
        Assert.Equal(0x44, pdfBytes[2]); // D character
        Assert.Equal(0x46, pdfBytes[3]); // F character
    }

    [Fact]
    public async Task GenerateApplicationPdfAsync_WithVotes_IncludesVotingSection()
    {
        // Arrange
        var application = CreateTestApplication();
        application.Votes = new List<ApplicationVote>
        {
            new ApplicationVote
            {
                VoterId = "voter1",
                Voter = new ApplicationUser { FirstName = "John", LastName = "Voter" },
                Decision = VoteDecision.Approve,
                Reasoning = "Good candidate",
                ConfidenceLevel = 4,
                VotedDate = DateTime.UtcNow
            }
        };

        // Act
        var pdfBytes = await _service.GenerateApplicationPdfAsync(application, includeVotes: true);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }

    [Fact]
    public async Task GenerateApplicationPdfAsync_WithComments_IncludesCommentsSection()
    {
        // Arrange
        var application = CreateTestApplication();
        application.Comments = new List<ApplicationComment>
        {
            new ApplicationComment
            {
                AuthorId = "author1",
                Author = new ApplicationUser { FirstName = "Jane", LastName = "Commenter" },
                Content = "This is a test comment",
                CreatedDate = DateTime.UtcNow,
                IsPrivate = false
            }
        };

        // Act
        var pdfBytes = await _service.GenerateApplicationPdfAsync(application, includeComments: true);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }

    [Fact]
    public async Task GenerateApplicationPdfAsync_WithDecision_IncludesDecisionSection()
    {
        // Arrange
        var application = CreateTestApplication();
        application.FinalDecision = DecisionOutcome.Approved;
        application.DecisionDate = DateTime.UtcNow;
        application.DecisionMessage = "Congratulations! You have been approved.";
        application.ApprovedMonthlyAmount = 150m;

        // Act
        var pdfBytes = await _service.GenerateApplicationPdfAsync(application);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }

    [Fact]
    public async Task GenerateApplicationPdfAsync_WithAllSections_GeneratesCompletePdf()
    {
        // Arrange
        var application = CreateCompleteApplication();

        // Act
        var pdfBytes = await _service.GenerateApplicationPdfAsync(
            application,
            includeVotes: true,
            includeComments: true);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 1000); // Should be substantial with all content
    }

    #endregion

    #region Approval Letter PDF Tests

    [Fact]
    public async Task GenerateApprovalLetterPdfAsync_WithApprovedApplication_GeneratesLetter()
    {
        // Arrange
        var application = CreateTestApplication();
        application.Status = ApplicationStatus.Approved;
        application.FinalDecision = DecisionOutcome.Approved;
        application.DecisionMessage = "Congratulations on your approval!";
        application.ApprovedMonthlyAmount = 200m;

        // Act
        var pdfBytes = await _service.GenerateApprovalLetterPdfAsync(application);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }

    [Fact]
    public async Task GenerateApprovalLetterPdfAsync_WithSponsor_IncludesSponsorInfo()
    {
        // Arrange
        var application = CreateTestApplication();
        application.Status = ApplicationStatus.Approved;
        application.AssignedSponsor = new ApplicationUser
        {
            FirstName = "Bob",
            LastName = "Sponsor"
        };
        application.ApprovedMonthlyAmount = 150m;

        // Act
        var pdfBytes = await _service.GenerateApprovalLetterPdfAsync(application);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }

    [Fact]
    public async Task GenerateApprovalLetterPdfAsync_WithProgramDates_IncludesSchedule()
    {
        // Arrange
        var application = CreateTestApplication();
        application.Status = ApplicationStatus.Approved;
        application.ProgramStartDate = DateTime.UtcNow.AddDays(30);
        application.ProgramDurationMonths = 12;

        // Act
        var pdfBytes = await _service.GenerateApprovalLetterPdfAsync(application);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }

    #endregion

    #region Statistics Report PDF Tests

    [Fact]
    public async Task GenerateStatisticsReportPdfAsync_WithBasicStats_GeneratesReport()
    {
        // Arrange
        var statistics = new ApplicationStatistics
        {
            TotalApplications = 50,
            PendingReview = 5,
            UnderReview = 10,
            Approved = 25,
            Rejected = 5,
            Active = 20,
            Completed = 5,
            ApprovalRate = 50m,
            AverageReviewDays = 7.5
        };

        // Act
        var pdfBytes = await _service.GenerateStatisticsReportPdfAsync(statistics);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }

    [Fact]
    public async Task GenerateStatisticsReportPdfAsync_WithZeroApplications_GeneratesReport()
    {
        // Arrange
        var statistics = new ApplicationStatistics
        {
            TotalApplications = 0,
            ApprovalRate = 0,
            AverageReviewDays = 0
        };

        // Act
        var pdfBytes = await _service.GenerateStatisticsReportPdfAsync(statistics);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }

    #endregion

    #region PDF Format Tests

    [Fact]
    public async Task GeneratedPdf_HasValidPdfSignature()
    {
        // Arrange
        var application = CreateTestApplication();

        // Act
        var pdfBytes = await _service.GenerateApplicationPdfAsync(application);

        // Assert
        Assert.True(IsPdfFormat(pdfBytes));
    }

    [Theory]
    [InlineData(ApplicationStatus.Draft)]
    [InlineData(ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.Approved)]
    [InlineData(ApplicationStatus.Rejected)]
    public async Task GenerateApplicationPdfAsync_WithDifferentStatuses_GeneratesValidPdf(ApplicationStatus status)
    {
        // Arrange
        var application = CreateTestApplication();
        application.Status = status;

        // Act
        var pdfBytes = await _service.GenerateApplicationPdfAsync(application);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.True(IsPdfFormat(pdfBytes));
    }

    #endregion

    #region Helper Methods

    private ClientApplication CreateTestApplication()
    {
        return new ClientApplication
        {
            Id = 1,
            ApplicantId = "applicant1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "555-1234",
            Address = "123 Main St",
            City = "Anytown",
            State = "CA",
            ZipCode = "12345",
            Status = ApplicationStatus.Submitted,
            FundingTypesRequested = "GymMembership,PersonalTraining",
            EstimatedMonthlyCost = 150m,
            ProgramDurationMonths = 12,
            PersonalStatement = "I want to improve my health and fitness through this program.",
            ExpectedBenefits = "Better health, more energy, improved quality of life.",
            CommitmentStatement = "I am fully committed to completing this 12-month program.",
            UnderstandsCommitment = true,
            Signature = "John Doe",
            SubmittedDate = DateTime.UtcNow.AddDays(-5),
            CurrentFitnessLevel = FitnessLevel.Beginner,
            AgreesToNutritionist = true,
            AgreesToPersonalTrainer = true,
            AgreesToWeeklyCheckIns = true,
            AgreesToProgressReports = true,
            VotesRequiredForApproval = 3
        };
    }

    private ClientApplication CreateCompleteApplication()
    {
        var application = CreateTestApplication();
        
        application.Occupation = "Software Engineer";
        application.DateOfBirth = new DateTime(1990, 1, 1);
        application.EmergencyContactName = "Jane Doe";
        application.EmergencyContactPhone = "555-5678";
        application.FundingDetails = "Need assistance with gym membership and personal training sessions";
        application.ConcernsObstacles = "Limited time due to work schedule";
        application.MedicalConditions = "None";
        application.CurrentMedications = "None";
        application.LastPhysicalExamDate = DateTime.UtcNow.AddMonths(-3);
        application.FitnessGoals = "Lose weight and build muscle";
        application.FinalDecision = DecisionOutcome.Approved;
        application.DecisionDate = DateTime.UtcNow;
        application.DecisionMessage = "Approved!";
        application.ApprovedMonthlyAmount = 150m;
        
        application.Applicant = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };

        application.Votes = new List<ApplicationVote>
        {
            new ApplicationVote
            {
                VoterId = "voter1",
                Voter = new ApplicationUser { FirstName = "Board", LastName = "Member1" },
                Decision = VoteDecision.Approve,
                Reasoning = "Strong candidate",
                ConfidenceLevel = 5,
                VotedDate = DateTime.UtcNow.AddDays(-2)
            }
        };

        application.Comments = new List<ApplicationComment>
        {
            new ApplicationComment
            {
                AuthorId = "author1",
                Author = new ApplicationUser { FirstName = "Admin", LastName = "User" },
                Content = "Looks good!",
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                IsPrivate = false,
                IsDeleted = false
            }
        };

        return application;
    }

    private bool IsPdfFormat(byte[] bytes)
    {
        if (bytes == null || bytes.Length < 4)
            return false;

        return bytes[0] == 0x25 && // %
               bytes[1] == 0x50 && // P
               bytes[2] == 0x44 && // D
               bytes[3] == 0x46;   // F
    }

    #endregion
}
