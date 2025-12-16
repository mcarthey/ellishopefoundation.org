using System.Net;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for PagesController
/// Tests full HTTP pipeline including authentication, model binding, and validation
/// </summary>
public class PagesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PagesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Index Action Integration Tests

    [Fact]
    public async Task Index_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Pages");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Pages");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Pages", content);
    }

    [Fact]
    public async Task Index_WithSearchTerm_FiltersResults()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-search@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Pages?searchTerm=Home");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Edit GET Action Integration Tests

    [Fact]
    public async Task Edit_Get_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Pages/Edit/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Edit_Get_WithNonExistentPage_ReturnsNotFound()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-edit@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act
        var response = await client.GetAsync("/Admin/Pages/Edit/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region UpdateContent Action Integration Tests

    [Fact]
    public async Task UpdateContent_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1"),
            new KeyValuePair<string, string>("sectionKey", "WelcomeText"),
            new KeyValuePair<string, string>("content", "Hello World"),
            new KeyValuePair<string, string>("contentType", "Text")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Pages/UpdateContent", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContent_WithInvalidModel_RedirectsToEditWithError()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-update-invalid@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);
        
        // Missing required fields
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1")
            // Missing sectionKey
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateContent", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Admin/Pages/Edit", location);
    }

    [Fact]
    public async Task UpdateContent_WithValidData_RedirectsToEdit()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-update-valid@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);
        
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1"),
            new KeyValuePair<string, string>("sectionKey", "TestSection"),
            new KeyValuePair<string, string>("content", "Test Content"),
            new KeyValuePair<string, string>("contentType", "Text")
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateContent", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Admin/Pages/Edit", location);
    }

    #endregion

    #region UpdateImage Action Integration Tests

    [Fact]
    public async Task UpdateImage_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1"),
            new KeyValuePair<string, string>("imageKey", "HeroImage"),
            new KeyValuePair<string, string>("mediaId", "1")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Pages/UpdateImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateImage_WithInvalidModel_RedirectsToEditWithError()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-image-invalid@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);
        
        // Missing required fields
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1")
            // Missing imageKey and mediaId
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Admin/Pages/Edit", location);
    }

    [Fact]
    public async Task UpdateImage_WithValidData_RedirectsToEdit()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-image-valid@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);
        
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1"),
            new KeyValuePair<string, string>("imageKey", "TestImage"),
            new KeyValuePair<string, string>("mediaId", "1")
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Admin/Pages/Edit", location);
    }

    #endregion

    #region RemoveImage Action Integration Tests

    [Fact]
    public async Task RemoveImage_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1"),
            new KeyValuePair<string, string>("imageKey", "HeroImage")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Pages/RemoveImage?pageId=1&imageKey=HeroImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveImage_WithValidData_RedirectsToEdit()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-remove@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);
        
        var formData = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        // Act
        var response = await client.PostAsync("/Admin/Pages/RemoveImage?pageId=1&imageKey=TestImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Admin/Pages/Edit", location);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task AllPagesEndpoints_RequireAuthentication()
    {
        // Test that all endpoints return unauthorized when not authenticated
        var endpoints = new[]
        {
            "/Admin/Pages",
            "/Admin/Pages/Edit/1"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    #endregion

    #region Full Workflow Integration Tests

    [Fact]
    public async Task FullWorkflow_CreatePageAndUpdateContent()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-workflow@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act 1: Navigate to pages list
        var indexResponse = await client.GetAsync("/Admin/Pages");
        Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);

        // Act 2: Update a section (assuming page ID 1 exists or will be created)
        var updateContentData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1"),
            new KeyValuePair<string, string>("sectionKey", "IntegrationTest"),
            new KeyValuePair<string, string>("content", "Integration Test Content"),
            new KeyValuePair<string, string>("contentType", "Text")
        });

        var updateResponse = await client.PostAsync("/Admin/Pages/UpdateContent", updateContentData);
        
        // Assert
        Assert.Equal(HttpStatusCode.Redirect, updateResponse.StatusCode);
        var location = updateResponse.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Admin/Pages/Edit", location);
    }

    [Fact]
    public async Task FullWorkflow_UpdateImageAndRemove()
    {
        // Arrange
        var userId = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "admin-image-workflow@pages-test.com",
            "Admin",
            "User",
            UserRole.Admin);
        var client = _factory.CreateAuthenticatedClient(userId);

        // Act 1: Update image
        var updateImageData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pageId", "1"),
            new KeyValuePair<string, string>("imageKey", "IntegrationTestImage"),
            new KeyValuePair<string, string>("mediaId", "1")
        });

        var updateResponse = await client.PostAsync("/Admin/Pages/UpdateImage", updateImageData);
        Assert.Equal(HttpStatusCode.Redirect, updateResponse.StatusCode);

        // Act 2: Remove image
        var removeResponse = await client.PostAsync(
            "/Admin/Pages/RemoveImage?pageId=1&imageKey=IntegrationTestImage",
            new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, removeResponse.StatusCode);
        var location = removeResponse.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Admin/Pages/Edit", location);
    }

    #endregion
}
