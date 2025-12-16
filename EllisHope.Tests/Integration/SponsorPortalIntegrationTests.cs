using System.Net;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for Sponsor Portal
/// Tests sponsor dashboard, client viewing, and authorization
/// </summary>
public class SponsorPortalIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SponsorPortalIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Authorization Tests

    [Fact]
    public async Task SponsorDashboard_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");

        // Assert - Returns Unauthorized (401) instead of Redirect in test environment
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SponsorDashboard_WithNonSponsorUser_ReturnsForbidden()
    {
        // Arrange - Create a regular member (not a sponsor)
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member@test.com",
            "Test",
            "Member",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SponsorDashboard_WithSponsorUser_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor1@test.com",
            "Test",
            "Sponsor",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Dashboard Content Tests

    [Fact]
    public async Task SponsorDashboard_DisplaysSponsorName()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor2@test.com",
            "John",
            "Sponsor",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("John Sponsor", content);
        Assert.Contains("Sponsor Dashboard", content);
    }

    [Fact]
    public async Task SponsorDashboard_WithNoClients_ShowsZeroStatistics()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor3@test.com",
            "Empty",
            "Sponsor",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Total Clients", content);
        Assert.Contains(">0</h2>", content); // Zero clients
        Assert.Contains("You don't have any sponsored clients yet", content);
    }

    [Fact]
    public async Task SponsorDashboard_WithClients_DisplaysStatistics()
    {
        // Arrange
        var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Total Clients", content);
        Assert.Contains(">1</h2>", content); // One client
        Assert.Contains("Test Client", content); // Client name
        Assert.Contains("$100.00", content); // Monthly fee
    }

    #endregion

    #region Client Details Tests

    [Fact]
    public async Task ClientDetails_WithSponsoredClient_ReturnsSuccess()
    {
        // Arrange
        var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync($"/Admin/Sponsor/ClientDetails/{clientId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ClientDetails_WithNonSponsoredClient_ReturnsNotFound()
    {
        // Arrange
        var sponsorId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor4@test.com",
            "Other",
            "Sponsor",
            UserRole.Sponsor);

        var (_, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        
        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync($"/Admin/Sponsor/ClientDetails/{clientId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ClientDetails_DisplaysClientInformation()
    {
        // Arrange
        var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync($"/Admin/Sponsor/ClientDetails/{clientId}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Test Client", content);
        Assert.Contains("client@test.com", content);
        Assert.Contains("$100.00", content); // Monthly fee
        Assert.Contains("Client Details", content);
    }

    [Fact]
    public async Task ClientDetails_ShowsContactInformation()
    {
        // Arrange
        var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync($"/Admin/Sponsor/ClientDetails/{clientId}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Quick Actions", content);
        Assert.Contains("Send Email", content);
        Assert.Contains("mailto:client@test.com", content);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task SponsorDashboard_HasProfileLink()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor5@test.com",
            "Nav",
            "Test",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("My Profile", content);
    }

    [Fact]
    public async Task MyProfile_RedirectsToProfilePage()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor6@test.com",
            "Profile",
            "Test",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/MyProfile");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Profile", response.Headers.Location?.ToString());
    }

    #endregion

    #region Data Calculation Tests

    [Fact]
    public async Task SponsorDashboard_CalculatesActiveClientsCorrectly()
    {
        // Arrange
        var sponsorId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor7@test.com",
            "Multi",
            "Sponsor",
            UserRole.Sponsor);

        // Create multiple clients with different statuses
        var activeClientId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "active@test.com",
            "Active",
            "Client",
            UserRole.Client);

        var pendingClientId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "pending@test.com",
            "Pending",
            "Client",
            UserRole.Client);

        // Assign sponsor and set statuses
        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            
            var activeClient = await context.Users.FindAsync(activeClientId);
            if (activeClient != null)
            {
                activeClient.SponsorId = sponsorId;
                activeClient.Status = MembershipStatus.Active;
                activeClient.MonthlyFee = 100m;
            }

            var pendingClient = await context.Users.FindAsync(pendingClientId);
            if (pendingClient != null)
            {
                pendingClient.SponsorId = sponsorId;
                pendingClient.Status = MembershipStatus.Pending;
                pendingClient.MonthlyFee = 50m;
            }

            await context.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains(">2</h2>", content); // Total clients
        Assert.Contains("Active Clients", content);
        Assert.Contains("Pending", content);
    }

    #endregion
}
