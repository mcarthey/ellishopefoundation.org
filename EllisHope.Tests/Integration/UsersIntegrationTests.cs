using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Identity;
using EllisHope.Tests.Helpers;

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
    public async Task UsersIndex_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UsersIndex_WithAuth_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UsersIndex_RendersWithoutLayoutErrors()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-layout@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Users", content);
        Assert.DoesNotContain("Exception", content);
        Assert.DoesNotContain("Error", content);
    }

    [Fact]
    public async Task UsersIndex_RendersStatisticsCards()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-stats@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Total Users", content);
        Assert.Contains("Active Users", content);
        Assert.Contains("Pending", content);
    }

    [Fact]
    public async Task UsersIndex_RendersFilterForm()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-filter@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Search", content);
        Assert.Contains("Filter", content);
    }

    [Fact]
    public async Task UsersCreate_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-create@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users/Create");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Create", content);
    }

    [Fact]
    public async Task UsersDetails_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-details@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act - Use the user ID we just created
        var response = await client.GetAsync($"/Admin/Users/Details/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Admin User", content);
    }

    [Fact]
    public async Task UsersEdit_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-edit@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act - Use the user ID we just created
        var response = await client.GetAsync($"/Admin/Users/Edit/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Edit User", content);
    }

    [Fact]
    public async Task UsersDelete_WithValidId_ReturnsSuccess()
    {
        // Arrange - Create admin user
        var adminId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-delete@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        
        // Create a user to delete
        var deleteUserId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "todelete@users-test.com",
            "To",
            "Delete",
            UserRole.Member);
        
        var client = _factory.CreateAuthenticatedClient(adminId);

        // Act
        var response = await client.GetAsync($"/Admin/Users/Delete/{deleteUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Delete", content);
        Assert.Contains("To Delete", content);
    }

    #endregion

    #region User CRUD Flow Tests

    [Fact]
    public async Task UserCreationFlow_CreatesUserSuccessfully()
    {
        // Arrange
        var adminId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-crud@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(adminId);

        var formData = new FormUrlEncodedContent(new []
        {
            new KeyValuePair<string, string>("FirstName", "John"),
            new KeyValuePair<string, string>("LastName", "Doe"),
            new KeyValuePair<string, string>("Email", "john.doe@test.com"),
            new KeyValuePair<string, string>("Password", "Test@123456"),
            new KeyValuePair<string, string>("ConfirmPassword", "Test@123456"),
            new KeyValuePair<string, string>("UserRole", "4"), // Admin
            new KeyValuePair<string, string>("Status", "1"), // Active
            new KeyValuePair<string, string>("IsActive", "true")
        });

        // Act
        var response = await client.PostAsync("/Admin/Users/Create", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Users", response.Headers.Location?.ToString());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task UsersDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-invalid@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users/Details/invalid-id-12345");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UsersEdit_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-edit-invalid@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users/Edit/invalid-id-12345");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Filter and Search Tests

    [Fact]
    public async Task UsersIndex_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-search@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users?searchTerm=admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UsersIndex_WithRoleFilter_ReturnsFilteredResults()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-role-filter@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users?roleFilter=4"); // Admin role

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UsersIndex_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-status-filter@users-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Users?statusFilter=1"); // Active status

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}
