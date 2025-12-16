using System.Net;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for Member Portal
/// Tests member dashboard, events, volunteer opportunities, and authorization
/// </summary>
public class MemberPortalIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MemberPortalIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Authorization Tests

    [Fact]
    public async Task MemberDashboard_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");

        // Assert - Returns Unauthorized (401) instead of Redirect in test environment
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MemberDashboard_WithNonMemberUser_ReturnsForbidden()
    {
        // Arrange - Create a client (not a member)
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client@member-test.com",
            "Test",
            "Client",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MemberDashboard_WithMemberUser_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member1@test.com",
            "Test",
            "Member",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Dashboard Content Tests

    [Fact]
    public async Task MemberDashboard_DisplaysMemberName()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member2@test.com",
            "John",
            "Member",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("John Member", content);
        Assert.Contains("Member Dashboard", content);
    }

    [Fact]
    public async Task MemberDashboard_ShowsWelcomeBanner()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member3@test.com",
            "Welcome",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Welcome to Ellis Hope Foundation", content);
        Assert.Contains("part of our community", content);
    }

    [Fact]
    public async Task MemberDashboard_DisplaysMembershipStatus()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member4@test.com",
            "Status",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Membership Status", content);
        Assert.Contains("Active", content);
        Assert.Contains("Member Since", content);
    }

    [Fact]
    public async Task MemberDashboard_ShowsQuickActions()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member5@test.com",
            "Quick",
            "Actions",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Upcoming Events", content);
        Assert.Contains("Volunteer Opportunities", content);
        Assert.Contains("My Profile", content);
    }

    #endregion

    #region Events Page Tests

    [Fact]
    public async Task Events_WithAuth_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member6@test.com",
            "Events",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Events");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Events_DisplaysEventsContent()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member7@test.com",
            "Event",
            "Content",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Events");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Upcoming Events", content);
        Assert.Contains("View All Events", content);
        Assert.Contains("Event Registration", content);
        Assert.Contains("Coming Soon", content);
    }

    [Fact]
    public async Task Events_HasBackToDashboardLink()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member8@test.com",
            "Nav",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Events");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Back to Dashboard", content);
    }

    #endregion

    #region Volunteer Page Tests

    [Fact]
    public async Task Volunteer_WithAuth_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member9@test.com",
            "Volunteer",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Volunteer");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Volunteer_DisplaysOpportunityCategories()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member10@test.com",
            "Categories",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Volunteer");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Volunteer Opportunities", content);
        Assert.Contains("Event Support", content);
        Assert.Contains("Mentorship", content);
        Assert.Contains("Outreach", content);
        Assert.Contains("Administrative Support", content);
    }

    [Fact]
    public async Task Volunteer_ShowsBenefitsSection()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member11@test.com",
            "Benefits",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Volunteer");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Why Volunteer?", content);
        Assert.Contains("Make an Impact", content);
        Assert.Contains("Build Community", content);
        Assert.Contains("Personal Growth", content);
    }

    [Fact]
    public async Task Volunteer_HasGetStartedSection()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member12@test.com",
            "GetStarted",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Volunteer");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Ready to Get Started?", content);
        Assert.Contains("Contact Us", content);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task MemberDashboard_HasCommunityFeaturesSection()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member13@test.com",
            "Community",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Community Features", content);
        Assert.Contains("Latest from the Blog", content);
        Assert.Contains("Support Our Causes", content);
    }

    [Fact]
    public async Task MemberDashboard_HasGetInvolvedSection()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member14@test.com",
            "Involved",
            "Test",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Get More Involved", content);
    }

    [Fact]
    public async Task MyProfile_RedirectsToProfilePage()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member15@test.com",
            "Profile",
            "Redirect",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/MyProfile");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Profile", response.Headers.Location?.ToString());
    }

    #endregion

    #region Last Login Display Tests

    [Fact]
    public async Task MemberDashboard_WithFirstVisit_ShowsFirstVisitMessage()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member16@test.com",
            "First",
            "Visit",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Last Login", content);
    }

    [Fact]
    public async Task MemberDashboard_DisplaysJoinDate()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member17@test.com",
            "Join",
            "Date",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Member Since", content);
        Assert.Contains(DateTime.UtcNow.ToString("MMM yyyy"), content);
    }

    #endregion
}
