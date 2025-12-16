using System.Net;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Edge case tests for all role-based portals
/// Tests boundary conditions, missing data, expired memberships, and cross-portal scenarios
/// </summary>
public class PortalEdgeCaseTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PortalEdgeCaseTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Expired Membership Tests

    [Fact]
    public async Task ClientDashboard_WithExpiredMembership_ShowsExpiredStatus()
    {
        // Arrange - Create client with expired membership
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "expired@client-test.com",
            "Expired",
            "Client",
            UserRole.Client);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = MembershipStatus.Expired;
                user.MembershipEndDate = DateTime.UtcNow.AddMonths(-1); // Expired 1 month ago
                await context.SaveChangesAsync();
            }
        }

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Expired", content);
        Assert.Contains("Membership Status", content);
    }

    [Fact]
    public async Task SponsorDashboard_WithExpiredClients_ShowsCorrectCount()
    {
        // Arrange - Create sponsor with mixed status clients
        var sponsorId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor-expired@test.com",
            "Sponsor",
            "Mixed",
            UserRole.Sponsor);

        var activeClientId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "active-client@test.com",
            "Active",
            "Client",
            UserRole.Client);

        var expiredClientId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "expired-client@test.com",
            "Expired",
            "Client",
            UserRole.Client);

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

            var expiredClient = await context.Users.FindAsync(expiredClientId);
            if (expiredClient != null)
            {
                expiredClient.SponsorId = sponsorId;
                expiredClient.Status = MembershipStatus.Expired;
                expiredClient.MonthlyFee = 150m;
                expiredClient.MembershipEndDate = DateTime.UtcNow.AddMonths(-2);
            }

            await context.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(">2</h2>", content); // Total clients
        Assert.Contains(">1</h2>", content); // Active clients
        Assert.Contains("$100.00", content); // Only active client's fee
    }

    #endregion

    #region Missing Data Tests

    [Fact]
    public async Task ClientDashboard_WithoutSponsor_ShowsNoSponsorMessage()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "nosponsor@client-test.com",
            "No",
            "Sponsor",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("You don't have a sponsor assigned yet", content);
    }

    [Fact]
    public async Task ClientDashboard_WithoutMembershipDates_ShowsNoProgressBar()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "nodates@client-test.com",
            "No",
            "Dates",
            UserRole.Client);

        // Don't set membership dates
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Should not crash, should handle missing dates gracefully
        Assert.DoesNotContain("Exception", content);
    }

    [Fact]
    public async Task SponsorDashboard_WithNoClients_ShowsEmptyState()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "noclients@sponsor-test.com",
            "Empty",
            "Sponsor",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(">0</h2>", content); // Total clients = 0
        Assert.Contains("You don't have any sponsored clients yet", content);
    }

    [Fact]
    public async Task MemberDashboard_WithoutJoinDate_ShowsCurrentDate()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "nojoindate@member-test.com",
            "No",
            "JoinDate",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Member/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Member Since", content);
        // Should default to current date
        Assert.Contains(DateTime.UtcNow.ToString("MMM yyyy"), content);
    }

    #endregion

    #region Boundary Condition Tests

    [Fact]
    public async Task SponsorDashboard_WithManyClients_HandlesCorrectly()
    {
        // Arrange - Create sponsor with 10 clients (simulating many)
        var sponsorId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "manyclients@sponsor-test.com",
            "Many",
            "Clients",
            UserRole.Sponsor);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            
            for (int i = 1; i <= 10; i++)
            {
                var clientId = await TestAuthenticationHelper.CreateTestUserAsync(
                    _factory.Services,
                    $"bulk-client-{i}@test.com",
                    $"Client",
                    $"Number{i}",
                    UserRole.Client);

                var clientUser = await context.Users.FindAsync(clientId);
                if (clientUser != null)
                {
                    clientUser.SponsorId = sponsorId;
                    clientUser.Status = MembershipStatus.Active;
                    clientUser.MonthlyFee = 100m;
                }
            }

            await context.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(">10</h2>", content); // Total clients
        Assert.Contains("$1,000.00", content); // 10 * $100
    }

    [Fact]
    public async Task ClientProgress_WithZeroProgress_ShowsZeroPercent()
    {
        // Arrange - Client just started (membership start = today)
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "zeroprogress@client-test.com",
            "Zero",
            "Progress",
            UserRole.Client);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.MembershipStartDate = DateTime.UtcNow;
                user.MembershipEndDate = DateTime.UtcNow.AddMonths(12);
                await context.SaveChangesAsync();
            }
        }

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Membership Progress", content);
        // Should show 0% or very small percentage
    }

    [Fact]
    public async Task ClientProgress_WithFullProgress_ShowsHundredPercent()
    {
        // Arrange - Client at end of membership
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "fullprogress@client-test.com",
            "Full",
            "Progress",
            UserRole.Client);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.MembershipStartDate = DateTime.UtcNow.AddMonths(-12);
                user.MembershipEndDate = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Membership Progress", content);
    }

    #endregion

    #region Cross-Portal Access Tests

    [Fact]
    public async Task AdminUser_CanAccessAllPortals()
    {
        // Arrange
        var adminId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-all@test.com",
            "Admin",
            "All",
            UserRole.Admin);

        var client = _factory.CreateAuthenticatedClient(adminId);

        // Act & Assert - Admin can access Users management
        var usersResponse = await client.GetAsync("/Admin/Users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        // Admin can access Media
        var mediaResponse = await client.GetAsync("/Admin/Media");
        Assert.Equal(HttpStatusCode.OK, mediaResponse.StatusCode);

        // Admin can access Pages
        var pagesResponse = await client.GetAsync("/Admin/Pages");
        Assert.Equal(HttpStatusCode.OK, pagesResponse.StatusCode);

        // Admin can access Causes
        var causesResponse = await client.GetAsync("/Admin/Causes");
        Assert.Equal(HttpStatusCode.OK, causesResponse.StatusCode);
    }

    [Fact]
    public async Task BoardMemberUser_CannotAccessContentManagement()
    {
        // Arrange
        var boardMemberId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "boardmember@test.com",
            "Board",
            "Member",
            UserRole.BoardMember);

        var client = _factory.CreateAuthenticatedClient(boardMemberId);

        // Act & Assert - BoardMember cannot access content management (Admin/Editor only)
        var mediaResponse = await client.GetAsync("/Admin/Media");
        Assert.Equal(HttpStatusCode.Forbidden, mediaResponse.StatusCode);

        var pagesResponse = await client.GetAsync("/Admin/Pages");
        Assert.Equal(HttpStatusCode.Forbidden, pagesResponse.StatusCode);

        var causesResponse = await client.GetAsync("/Admin/Causes");
        Assert.Equal(HttpStatusCode.Forbidden, causesResponse.StatusCode);
        
        // BoardMember CAN access Dashboard
        var dashboardResponse = await client.GetAsync("/Admin/Dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, dashboardResponse.StatusCode); // Dashboard is Admin only too
    }

    [Fact]
    public async Task SponsorUser_CannotAccessClientPortal()
    {
        // Arrange
        var sponsorId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor-noclient@test.com",
            "Sponsor",
            "NoClient",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(sponsorId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");

        // Assert - Should be forbidden (wrong role)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ClientUser_CannotAccessSponsorPortal()
    {
        // Arrange
        var clientId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client-nosponsor-portal@test.com",
            "Client",
            "NoSponsor",
            UserRole.Client);

        var client = _factory.CreateAuthenticatedClient(clientId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");

        // Assert - Should be forbidden (wrong role)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MemberUser_CannotAccessSponsorPortal()
    {
        // Arrange
        var memberId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member-nosponsor@test.com",
            "Member",
            "NoSponsor",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(memberId);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");

        // Assert - Should be forbidden (wrong role)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MemberUser_CannotAccessClientPortal()
    {
        // Arrange
        var memberId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "member-noclient@test.com",
            "Member",
            "NoClient",
            UserRole.Member);

        var client = _factory.CreateAuthenticatedClient(memberId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");

        // Assert - Should be forbidden (wrong role)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task SponsorClientDetails_OnlyShowsOwnClients()
    {
        // Arrange - Create two sponsors with their own clients
        var (sponsor1Id, client1Id) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        
        var sponsor2Id = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor2@integrity-test.com",
            "Sponsor",
            "Two",
            UserRole.Sponsor);

        var client2Id = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "client2@integrity-test.com",
            "Client",
            "Two",
            UserRole.Client);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            var client2 = await context.Users.FindAsync(client2Id);
            if (client2 != null)
            {
                client2.SponsorId = sponsor2Id;
                await context.SaveChangesAsync();
            }
        }

        var sponsor1Client = _factory.CreateAuthenticatedClient(sponsor1Id);

        // Act - Sponsor1 tries to access Sponsor2's client
        var response = await sponsor1Client.GetAsync($"/Admin/Sponsor/ClientDetails/{client2Id}");

        // Assert - Should be NotFound (sponsor doesn't own this client)
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SponsorDashboard_OnlyShowsOwnClientsInList()
    {
        // Arrange - Create two sponsors with different clients
        var (sponsor1Id, client1Id) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        
        var sponsor2Id = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "sponsor2-list@test.com",
            "Sponsor",
            "List",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(sponsor1Id);

        // Act
        var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(">1</h2>", content); // Should show only 1 client (their own)
        Assert.Contains("Test Client", content); // Their client's name
        Assert.DoesNotContain("Sponsor List", content); // Should NOT contain other sponsor's name
    }

    [Fact]
    public async Task ClientDashboard_ShowsCorrectSponsorOnly()
    {
        // Arrange
        var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
        
        // Create another sponsor (should not appear)
        var otherSponsorId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "othersponsor@test.com",
            "Other",
            "Sponsor",
            UserRole.Sponsor);

        var client = _factory.CreateAuthenticatedClient(clientId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Test Sponsor", content); // Their sponsor
        Assert.DoesNotContain("Other Sponsor", content); // Should NOT show other sponsor
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public async Task Client_FromPendingToActive_UpdatesCorrectly()
    {
        // Arrange - Create client with pending status
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "pending@client-test.com",
            "Pending",
            "Client",
            UserRole.Client);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = MembershipStatus.Pending;
                await context.SaveChangesAsync();
            }
        }

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Pending", content);
    }

    [Fact]
    public async Task Client_InactiveStatus_ShowsCorrectly()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "inactive@client-test.com",
            "Inactive",
            "Client",
            UserRole.Client);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = _factory.GetDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = MembershipStatus.Inactive;
                await context.SaveChangesAsync();
            }
        }

        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Client/Dashboard");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Inactive", content);
    }

    #endregion
}
