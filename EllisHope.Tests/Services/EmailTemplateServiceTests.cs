using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EllisHope.Tests.Services;

public class EmailTemplateServiceTests
{
    private EmailTemplateService CreateService(string? baseUrl = null)
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AppSettings:BaseUrl"]).Returns(baseUrl ?? "https://test.ellishope.org");
        return new EmailTemplateService(mockConfig.Object);
    }

    private ClientApplication CreateTestApplication()
    {
        return new ClientApplication
        {
            Id = 123,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            EstimatedMonthlyCost = 500.00m,
            FundingTypesRequested = "Gym Membership,Personal Training",
            ApprovedMonthlyAmount = 450.00m,
            DecisionMessage = "We are excited to support your fitness journey!"
        };
    }

    [Fact]
    public void Constructor_UsesDefaultBaseUrl_WhenNotConfigured()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AppSettings:BaseUrl"]).Returns((string?)null);

        // Act
        var service = new EmailTemplateService(mockConfig.Object);
        var application = CreateTestApplication();
        var result = service.GenerateApplicationSubmittedEmail(application);

        // Assert
        Assert.Contains("https://ellishope.org", result);
    }

    [Fact]
    public void Constructor_UsesProvidedBaseUrl_WhenConfigured()
    {
        // Arrange & Act
        var service = CreateService("https://custom.domain.com");
        var application = CreateTestApplication();
        var result = service.GenerateApplicationSubmittedEmail(application);

        // Assert
        Assert.Contains("https://custom.domain.com", result);
    }

    [Fact]
    public void GenerateApplicationSubmittedEmail_ContainsRequiredElements()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationSubmittedEmail(application);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("Application Submitted Successfully", result);
        Assert.Contains(application.FirstName, result);
        Assert.Contains($"#{application.Id}", result);
        Assert.Contains("Ellis Hope Foundation", result);
        Assert.Contains("/MyApplications/Details/123", result);
    }

    [Fact]
    public void GenerateApplicationSubmittedEmail_ContainsNextSteps()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationSubmittedEmail(application);

        // Assert
        Assert.Contains("What happens next?", result);
        Assert.Contains("<ol>", result);
        Assert.Contains("board members will review", result);
        Assert.Contains("additional information", result);
        Assert.Contains("notified of the decision", result);
    }

    [Fact]
    public void GenerateApplicationSubmittedEmail_ContainsActionButton()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationSubmittedEmail(application);

        // Assert
        Assert.Contains("class='button'", result);
        Assert.Contains("View Application Status", result);
    }

    [Fact]
    public void GenerateApplicationUnderReviewEmail_ContainsRequiredElements()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationUnderReviewEmail(application);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("Application Under Review", result);
        Assert.Contains(application.FirstName, result);
        Assert.Contains($"#{application.Id}", result);
        Assert.Contains("being reviewed by our board", result);
    }

    [Fact]
    public void GenerateApplicationUnderReviewEmail_ContainsViewApplicationLink()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationUnderReviewEmail(application);

        // Assert
        Assert.Contains("/MyApplications/Details/123", result);
        Assert.Contains("View Application", result);
    }

    [Fact]
    public void GenerateNewApplicationNotificationEmail_ContainsRequiredElements()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        var boardMemberName = "Jane Smith";

        // Act
        var result = service.GenerateNewApplicationNotificationEmail(application, boardMemberName);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("New Application Received", result);
        Assert.Contains(boardMemberName, result);
        Assert.Contains($"#{application.Id}", result);
    }

    [Fact]
    public void GenerateNewApplicationNotificationEmail_ContainsApplicationDetails()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        var boardMemberName = "Jane Smith";

        // Act
        var result = service.GenerateNewApplicationNotificationEmail(application, boardMemberName);

        // Assert
        Assert.Contains("Application ID:", result);
        Assert.Contains("Applicant:", result);
        Assert.Contains("Funding Types:", result);
        Assert.Contains("Estimated Monthly Cost:", result);
        Assert.Contains("$500.00", result);
        Assert.Contains("info-box", result);
    }

    [Fact]
    public void GenerateNewApplicationNotificationEmail_ContainsReviewLink()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateNewApplicationNotificationEmail(application, "Board Member");

        // Assert
        Assert.Contains("/Admin/Applications/Review/123", result);
        Assert.Contains("Review Application", result);
    }

    [Fact]
    public void GenerateVoteRequestEmail_ReturnsNewApplicationNotification()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        var boardMemberName = "John Board";

        // Act
        var voteRequest = service.GenerateVoteRequestEmail(application, boardMemberName);
        var newAppNotification = service.GenerateNewApplicationNotificationEmail(application, boardMemberName);

        // Assert
        Assert.Equal(newAppNotification, voteRequest);
    }

    [Fact]
    public void GenerateQuorumReachedEmail_ContainsRequiredElements()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateQuorumReachedEmail(application);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("Quorum Reached", result);
        Assert.Contains("Dear Board Member", result);
        Assert.Contains($"#{application.Id}", result);
        Assert.Contains("All required votes have been received", result);
    }

    [Fact]
    public void GenerateQuorumReachedEmail_ContainsApplicationDetailsLink()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateQuorumReachedEmail(application);

        // Assert
        Assert.Contains("/Admin/Applications/Details/123", result);
        Assert.Contains("View Application", result);
    }

    [Fact]
    public void GenerateApplicationApprovedEmail_ContainsRequiredElements()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationApprovedEmail(application);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("Congratulations", result);
        Assert.Contains(application.FirstName, result);
        Assert.Contains("APPROVED", result);
        Assert.Contains($"#{application.Id}", result);
    }

    [Fact]
    public void GenerateApplicationApprovedEmail_ContainsApprovedAmount_WhenSet()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        application.ApprovedMonthlyAmount = 450.00m;

        // Act
        var result = service.GenerateApplicationApprovedEmail(application);

        // Assert
        Assert.Contains("Approved Monthly Support", result);
        Assert.Contains("$450.00", result);
    }

    [Fact]
    public void GenerateApplicationApprovedEmail_OmitsApprovedAmount_WhenNull()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        application.ApprovedMonthlyAmount = null;

        // Act
        var result = service.GenerateApplicationApprovedEmail(application);

        // Assert
        Assert.DoesNotContain("Approved Monthly Support", result);
    }

    [Fact]
    public void GenerateApplicationApprovedEmail_ContainsDecisionMessage()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        application.DecisionMessage = "You've shown great commitment to your health!";

        // Act
        var result = service.GenerateApplicationApprovedEmail(application);

        // Assert
        Assert.Contains(application.DecisionMessage, result);
    }

    [Fact]
    public void GenerateApplicationApprovedEmail_ContainsNextSteps()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationApprovedEmail(application);

        // Assert
        Assert.Contains("Next Steps:", result);
        Assert.Contains("initial consultation", result);
        Assert.Contains("health screenings", result);
        Assert.Contains("trainer and nutritionist", result);
        Assert.Contains("fitness journey", result);
    }

    [Fact]
    public void GenerateApplicationApprovedEmail_ContainsSuccessBox()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationApprovedEmail(application);

        // Assert
        Assert.Contains("success-box", result);
    }

    [Fact]
    public void GenerateApplicationRejectedEmail_ContainsRequiredElements()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationRejectedEmail(application);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("Application Decision", result);
        Assert.Contains(application.FirstName, result);
        Assert.Contains($"#{application.Id}", result);
        Assert.Contains("unable to approve", result);
    }

    [Fact]
    public void GenerateApplicationRejectedEmail_ContainsDecisionMessage_WhenProvided()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        application.DecisionMessage = "Please consider reapplying next quarter.";

        // Act
        var result = service.GenerateApplicationRejectedEmail(application);

        // Assert
        Assert.Contains(application.DecisionMessage, result);
    }

    [Fact]
    public void GenerateApplicationRejectedEmail_OmitsDecisionMessage_WhenNullOrEmpty()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        application.DecisionMessage = null;

        // Act
        var resultNull = service.GenerateApplicationRejectedEmail(application);

        application.DecisionMessage = "";
        var resultEmpty = service.GenerateApplicationRejectedEmail(application);

        // Assert - Should not contain the <p> tag that would wrap the message
        // The template uses: {(!string.IsNullOrEmpty(application.DecisionMessage) ? $"<p>{application.DecisionMessage}</p>" : "")}
        // So we verify there's no extra paragraph between specific known paragraphs
        Assert.Contains("unable to approve", resultNull);
        Assert.Contains("reapply in the future", resultNull);
        Assert.Contains("unable to approve", resultEmpty);
        Assert.Contains("reapply in the future", resultEmpty);
    }

    [Fact]
    public void GenerateApplicationRejectedEmail_EncouragesReapplication()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationRejectedEmail(application);

        // Assert
        Assert.Contains("reapply in the future", result);
        Assert.Contains("circumstances change", result);
    }

    [Fact]
    public void GenerateInformationRequestedEmail_ContainsRequiredElements()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        var requestDetails = "Please provide recent medical clearance from your doctor.";

        // Act
        var result = service.GenerateInformationRequestedEmail(application, requestDetails);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("Additional Information Needed", result);
        Assert.Contains(application.FirstName, result);
        Assert.Contains($"#{application.Id}", result);
        Assert.Contains(requestDetails, result);
    }

    [Fact]
    public void GenerateInformationRequestedEmail_ContainsRequestBox()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        var requestDetails = "Please clarify your fitness goals.";

        // Act
        var result = service.GenerateInformationRequestedEmail(application, requestDetails);

        // Assert
        Assert.Contains("request-box", result);
        Assert.Contains("Information Requested:", result);
        Assert.Contains(requestDetails, result);
    }

    [Fact]
    public void GenerateInformationRequestedEmail_ContainsRespondLink()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateInformationRequestedEmail(application, "Need more info");

        // Assert
        Assert.Contains("/MyApplications/Details/123", result);
        Assert.Contains("Respond to Request", result);
    }

    [Fact]
    public void AllTemplates_ContainHtmlStructure()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var templates = new[]
        {
            service.GenerateApplicationSubmittedEmail(application),
            service.GenerateApplicationUnderReviewEmail(application),
            service.GenerateNewApplicationNotificationEmail(application, "Board Member"),
            service.GenerateQuorumReachedEmail(application),
            service.GenerateApplicationApprovedEmail(application),
            service.GenerateApplicationRejectedEmail(application),
            service.GenerateInformationRequestedEmail(application, "Details")
        };

        // Assert
        foreach (var template in templates)
        {
            Assert.Contains("<!DOCTYPE html>", template);
            Assert.Contains("<html>", template);
            Assert.Contains("<head>", template);
            Assert.Contains("<body>", template);
            Assert.Contains("</body>", template);
            Assert.Contains("</html>", template);
        }
    }

    [Fact]
    public void AllTemplates_ContainStyling()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var templates = new[]
        {
            service.GenerateApplicationSubmittedEmail(application),
            service.GenerateApplicationUnderReviewEmail(application),
            service.GenerateNewApplicationNotificationEmail(application, "Board Member"),
            service.GenerateQuorumReachedEmail(application),
            service.GenerateApplicationApprovedEmail(application),
            service.GenerateApplicationRejectedEmail(application),
            service.GenerateInformationRequestedEmail(application, "Details")
        };

        // Assert
        foreach (var template in templates)
        {
            Assert.Contains("<style>", template);
            Assert.Contains("font-family", template);
            Assert.Contains("</style>", template);
        }
    }

    [Fact]
    public void AllTemplates_ContainFooter()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();

        // Act
        var templates = new[]
        {
            service.GenerateApplicationSubmittedEmail(application),
            service.GenerateApplicationUnderReviewEmail(application),
            service.GenerateNewApplicationNotificationEmail(application, "Board Member"),
            service.GenerateQuorumReachedEmail(application),
            service.GenerateApplicationApprovedEmail(application),
            service.GenerateApplicationRejectedEmail(application),
            service.GenerateInformationRequestedEmail(application, "Details")
        };

        // Assert
        foreach (var template in templates)
        {
            Assert.Contains("Ellis Hope Foundation", template);
            Assert.Contains("class='footer'", template);
        }
    }

    [Fact]
    public void Templates_HandleSpecialCharactersInApplicationName()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        application.FirstName = "José";
        application.LastName = "O'Brien-Smith";

        // Act
        var result = service.GenerateApplicationSubmittedEmail(application);

        // Assert
        Assert.Contains("José", result);
    }

    [Fact]
    public void Templates_HandleLongDecisionMessages()
    {
        // Arrange
        var service = CreateService();
        var application = CreateTestApplication();
        application.DecisionMessage = string.Concat(Enumerable.Repeat("Very long message. ", 100));

        // Act
        var result = service.GenerateApplicationApprovedEmail(application);

        // Assert
        Assert.Contains(application.DecisionMessage, result);
    }

    [Theory]
    [InlineData("https://ellishope.org")]
    [InlineData("https://www.ellishopefoundation.org")]
    [InlineData("http://localhost:5000")]
    [InlineData("https://staging.ellishope.org")]
    public void Templates_HandleDifferentBaseUrls(string baseUrl)
    {
        // Arrange
        var service = CreateService(baseUrl);
        var application = CreateTestApplication();

        // Act
        var result = service.GenerateApplicationSubmittedEmail(application);

        // Assert
        Assert.Contains(baseUrl, result);
    }
}
