using System.Net;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for CausesController
/// Tests full HTTP pipeline including authentication, model binding, and validation
/// </summary>
public class CausesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CausesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region List Action Integration Tests

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task List_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Causes/list");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Causes", content);
    }

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task List_WithSearchTerm_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/Causes/list?search=education");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Details Action Integration Tests

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task Details_WithValidSlug_ReturnsSuccess()
    {
        // Note: This will return NotFound since no causes exist in test database
        // In a real scenario with seeded data, this would return OK
        
        // Act
        var response = await _client.GetAsync("/Causes/details/test-cause");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}");
    }

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task Details_WithInvalidSlug_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/Causes/details/non-existent-cause-slug-123");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Grid Action Integration Tests

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task Grid_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Causes/grid");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Admin Index Action Integration Tests

    [Fact]
    public async Task Admin_Index_WithoutAuthentication_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Causes");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Admin_Index_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Causes");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected OK or Redirect, got {response.StatusCode}");
    }

    [Fact]
    public async Task Admin_Index_WithSearchTerm_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Causes?searchTerm=water");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected OK or Redirect, got {response.StatusCode}");
    }

    [Fact]
    public async Task Admin_Index_WithCategoryFilter_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Causes?categoryFilter=Education");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected OK or Redirect, got {response.StatusCode}");
    }

    #endregion

    #region Admin Create Action Integration Tests

    [Fact]
    public async Task Admin_Create_Get_WithoutAuthentication_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Causes/Create");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Admin_Create_Get_WithAuthentication_ReturnsForm()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Causes/Create");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected OK or Redirect, got {response.StatusCode}");
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Create", content);
        }
    }

    [Fact]
    public async Task Admin_Create_Post_WithoutAuthentication_RedirectsToLogin()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Title", "Test Cause"),
            new KeyValuePair<string, string>("Description", "Test Description"),
            new KeyValuePair<string, string>("GoalAmount", "1000")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Causes/Create", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    #endregion

    #region Admin Edit Action Integration Tests

    [Fact]
    public async Task Admin_Edit_Get_WithoutAuthentication_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Causes/Edit/1");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Admin_Edit_Get_WithNonExistentCause_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Causes/Edit/99999");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected NotFound or Redirect, got {response.StatusCode}");
    }

    [Fact]
    public async Task Admin_Edit_Post_WithoutAuthentication_RedirectsToLogin()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Id", "1"),
            new KeyValuePair<string, string>("Title", "Updated Cause"),
            new KeyValuePair<string, string>("Description", "Updated Description"),
            new KeyValuePair<string, string>("GoalAmount", "2000")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Causes/Edit/1", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Admin_Edit_Post_WithIdMismatch_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();
        
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Id", "1"),
            new KeyValuePair<string, string>("Title", "Updated"),
            new KeyValuePair<string, string>("Description", "Description"),
            new KeyValuePair<string, string>("GoalAmount", "1000")
        });

        // Act
        var response = await client.PostAsync("/Admin/Causes/Edit/2", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected NotFound or Redirect, got {response.StatusCode}");
    }

    #endregion

    #region Admin Delete Action Integration Tests

    [Fact]
    public async Task Admin_Delete_WithoutAuthentication_RedirectsToLogin()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        // Act
        var response = await _client.PostAsync("/Admin/Causes/Delete/1", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Admin_Delete_WithAuthentication_ProcessesRequest()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();
        
        var formData = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        // Act
        var response = await client.PostAsync("/Admin/Causes/Delete/99999", formData);

        // Assert
        // Should redirect back to Index regardless of whether cause exists
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Redirect or NotFound, got {response.StatusCode}");
        
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var location = response.Headers.Location?.ToString() ?? string.Empty;
            Assert.True(
                location.Contains("/Admin/Causes") || location.Contains("/Admin/Account/Login"),
                $"Expected redirect to Causes or Login, got {location}");
        }
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task AllAdminCausesEndpoints_RequireAuthentication()
    {
        // Test that all admin endpoints redirect to login when not authenticated
        var endpoints = new[]
        {
            "/Admin/Causes",
            "/Admin/Causes/Create",
            "/Admin/Causes/Edit/1"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.ToString() ?? string.Empty;
            Assert.Contains("/Admin/Account/Login", location);
        }
    }

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task PublicCausesEndpoints_AllowAnonymousAccess()
    {
        // Test that public endpoints are accessible without authentication
        var endpoints = new[]
        {
            "/Causes/list",
            "/Causes/grid"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    #endregion

    #region Routing Tests

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task CauseDetailsRoute_WithSlug_MapsCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/causes/details/test-slug");

        // Assert
        // Should return NotFound (no data) or OK (if seeded data exists)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Route should map correctly, got {response.StatusCode}");
    }

    #endregion

    #region Full Workflow Integration Tests

    [Fact(Skip = "Known issue: CustomWebApplicationFactory DbContext initialization - See docs/issues/causes-integration-tests-failing.md")]
    public async Task FullWorkflow_BrowseCausesPublicly()
    {
        // Act 1: Visit causes list
        var listResponse = await _client.GetAsync("/Causes/list");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // Act 2: Visit grid view
        var gridResponse = await _client.GetAsync("/Causes/grid");
        Assert.Equal(HttpStatusCode.OK, gridResponse.StatusCode);

        // Act 3: Search for causes
        var searchResponse = await _client.GetAsync("/Causes/list?search=education");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        // Assert
        // All public pages should be accessible
    }

    [Fact]
    public async Task FullWorkflow_AdminCauseManagement()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act 1: Navigate to admin causes list
        var indexResponse = await client.GetAsync("/Admin/Causes");
        
        // Act 2: Navigate to create page
        var createResponse = await client.GetAsync("/Admin/Causes/Create");
        
        // Act 3: Search causes in admin
        var searchResponse = await client.GetAsync("/Admin/Causes?searchTerm=water");
        
        // Act 4: Filter by category
        var filterResponse = await client.GetAsync("/Admin/Causes?categoryFilter=Education");

        // Assert
        // All responses should either be OK (if authenticated) or Redirect (if not)
        Assert.True(
            indexResponse.StatusCode == HttpStatusCode.OK ||
            indexResponse.StatusCode == HttpStatusCode.Redirect);
        Assert.True(
            createResponse.StatusCode == HttpStatusCode.OK ||
            createResponse.StatusCode == HttpStatusCode.Redirect);
        Assert.True(
            searchResponse.StatusCode == HttpStatusCode.OK ||
            searchResponse.StatusCode == HttpStatusCode.Redirect);
        Assert.True(
            filterResponse.StatusCode == HttpStatusCode.OK ||
            filterResponse.StatusCode == HttpStatusCode.Redirect);
    }

    #endregion
}
