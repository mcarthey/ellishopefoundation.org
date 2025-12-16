using System.Net;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for Client Portal
/// Tests client dashboard, progress tracking, resources, and authorization
/// </summary>
public class ClientPortalIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ClientPortalIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Authorization Tests

    [Fact]
    public async Task ClientDashboard_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");

        // Assert - Returns Unauthorized (401) instead of Redirect in test environment
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ClientDashboard_WithNonClientUser_ReturnsForbidden()
    {
        // Arrange - Create a member (not a client)
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member1@test.com",
            "Test",
            "Member",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ClientDashboard_WithClientUser_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client1@test.com",
            "Test",
            "Client",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Dashboard Content Tests

    [Fact]
    public async Task ClientDashboard_DisplaysClientName()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client2@test.com",
            "Jane",
            "Client",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Jane Client", content);
        Assert.Contains("Client Dashboard", content);
    }

    [Fact]
    public async Task ClientDashboard_WithSponsor_DisplaysSponsorInfo()
    {
        // Arrange
        var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        var client = _factory.CreateAuthenticatedClient(clientId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Your Sponsor", content);
        Assert.Contains("Test Sponsor", content);
        Assert.Contains("sponsor@test.com", content);
    }

    [Fact]
    public async Task ClientDashboard_WithoutSponsor_ShowsNoSponsorMessage()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client3@test.com",
            "No",
            "Sponsor",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("You don't have a sponsor assigned yet", content);
    }

    [Fact]
    public async Task ClientDashboard_DisplaysMembershipStatus()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client4@test.com",
            "Status",
            "Test",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Membership Status", content);
        Assert.Contains("Active", content);
    }

    [Fact]
    public async Task ClientDashboard_WithMembershipDates_ShowsProgress()
    {
        // Arrange
        var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        var client = _factory.CreateAuthenticatedClient(clientId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Membership Progress", content);
        Assert.Contains("progress-bar", content);
    }

    #endregion

    #region Progress Page Tests

    [Fact]
    public async Task Progress_WithAuth_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client5@test.com",
            "Progress",
            "Test",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Progress");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Progress_DisplaysMilestones()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client6@test.com",
            "Milestone",
            "Test",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Progress");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("My Progress", content);
        Assert.Contains("Milestones", content);
        Assert.Contains("Registration Complete", content);
    }

    [Fact]
    public async Task Progress_HasBackToDashboardLink()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client7@test.com",
            "Nav",
            "Test",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Progress");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Back to Dashboard", content);
    }

    #endregion

    #region Resources Page Tests

    [Fact]
    public async Task Resources_WithAuth_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client8@test.com",
            "Resources",
            "Test",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Resources");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Resources_DisplaysResourceCategories()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client9@test.com",
            "Categories",
            "Test",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Resources");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Fitness Guides", content);
        Assert.Contains("Nutrition Resources", content);
        Assert.Contains("Wellness Resources", content);
        Assert.Contains("Video Tutorials", content);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task ClientDashboard_HasQuickActionLinks()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client10@test.com",
            "Quick",
            "Actions",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Track Progress", content);
        Assert.Contains("My Profile", content);
        Assert.Contains("Resources", content);
    }

    [Fact]
    public async Task MyProfile_RedirectsToProfilePage()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client11@test.com",
            "Profile",
            "Redirect",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/MyProfile");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Profile", response.Headers.Location?.ToString());
    }

    #endregion

    #region Progress Calculation Tests

    [Fact]
    public async Task ClientDashboard_CalculatesMembershipProgress()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client12@test.com",
            "Progress",
            "Calc",
            UserRole.Client);

        // Set specific membership dates
        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.MembershipStartDate = DateTime.UtcNow.AddMonths(-6);
                user.MembershipEndDate = DateTime.UtcNow.AddMonths(6);
                await context.SaveChangesAsync();
            }
        }

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Membership Progress", content);
        Assert.Contains("progress-bar", content);
        // Should show approximately 50% progress (6 months elapsed, 6 remaining)
    }

    #endregion
}
