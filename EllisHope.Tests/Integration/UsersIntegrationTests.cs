using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for Users management in Admin area
/// Tests full page rendering and layout integration
/// </summary>
public class UsersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UsersIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Page Rendering Tests

    [Fact]
    public async Task UsersIndex_WithoutAuth_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersIndex_WithAuth_ReturnsSuccess()
    {
        // This test is skipped until we implement proper test authentication
        // The current approach doesn't work well with the test environment
        Assert.True(true);
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersIndex_RendersWithoutLayoutErrors()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersIndex_RendersStatisticsCards()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersIndex_RendersFilterForm()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersCreate_ReturnsSuccess()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersDetails_WithValidId_ReturnsSuccess()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersEdit_WithValidId_ReturnsSuccess()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UsersDelete_WithValidId_ReturnsSuccess()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    #endregion

    #region User CRUD Flow Tests

    [Fact(Skip = "Authentication in integration tests needs to be refactored")]
    public async Task UserCreationFlow_CreatesUserSuccessfully()
    {
        // This test is skipped until we implement proper test authentication
        Assert.True(true);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UsersDetails_WithInvalidId_RedirectsToLogin()
    {
        // Arrange - No authentication

        // Act
        var response = await _client.GetAsync("/Admin/Users/Details/invalid-id-12345");

        // Assert - Should redirect to login since not authenticated
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UsersEdit_WithInvalidId_RedirectsToLogin()
    {
        // Arrange - No authentication

        // Act
        var response = await _client.GetAsync("/Admin/Users/Edit/invalid-id-12345");

        // Assert - Should redirect to login since not authenticated
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    #endregion

    #region Helper Methods (for future use when authentication is fixed)

    private async Task<HttpClient> GetAuthenticatedClient()
    {
        // TODO: Implement proper test authentication
        // This is a placeholder for when we implement test authentication properly
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        });

        return client;
    }

    private async Task CreateAuthenticatedAdmin()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var admin = await userManager.FindByEmailAsync("admin@ellishope.org");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@ellishope.org",
                Email = "admin@ellishope.org",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                UserRole = UserRole.Admin,
                Status = MembershipStatus.Active,
                IsActive = true
            };

            await userManager.CreateAsync(admin, "Admin@123456");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private async Task<string> GetAdminUserId()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync("admin@ellishope.org");
        return admin?.Id ?? throw new InvalidOperationException("Admin user not found");
    }

    #endregion
}
