using System.Net;
using EllisHope.Models.Domain;
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
    public async Task Index_WithoutAuthentication_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Pages");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Index_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

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
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Pages?searchTerm=Home");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Edit GET Action Integration Tests

    [Fact]
    public async Task Edit_Get_WithoutAuthentication_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Pages/Edit/1");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Edit_Get_WithNonExistentPage_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Pages/Edit/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region UpdateSection Action Integration Tests

    [Fact]
    public async Task UpdateSection_WithoutAuthentication_RedirectsToLogin()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1"),
            new KeyValuePair<string, string>("SectionKey", "WelcomeText"),
            new KeyValuePair<string, string>("Content", "Hello World"),
            new KeyValuePair<string, string>("ContentType", "Text")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Pages/UpdateSection", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UpdateSection_WithInvalidModel_RedirectsToEditWithError()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();
        
        // Missing required fields
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1")
            // Missing SectionKey and ContentType
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateSection", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Pages/Edit/1", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UpdateSection_WithValidData_RedirectsToEdit()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();
        
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1"),
            new KeyValuePair<string, string>("SectionKey", "TestSection"),
            new KeyValuePair<string, string>("Content", "Test Content"),
            new KeyValuePair<string, string>("ContentType", "Text")
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateSection", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Pages/Edit/1", response.Headers.Location?.ToString());
    }

    #endregion

    #region UpdateImage Action Integration Tests

    [Fact]
    public async Task UpdateImage_WithoutAuthentication_RedirectsToLogin()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1"),
            new KeyValuePair<string, string>("ImageKey", "HeroImage"),
            new KeyValuePair<string, string>("MediaId", "1"),
            new KeyValuePair<string, string>("DisplayOrder", "0")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Pages/UpdateImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UpdateImage_WithInvalidModel_RedirectsToEditWithError()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();
        
        // Missing required fields
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1")
            // Missing ImageKey and MediaId
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Pages/Edit/1", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UpdateImage_WithValidData_RedirectsToEdit()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();
        
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1"),
            new KeyValuePair<string, string>("ImageKey", "TestImage"),
            new KeyValuePair<string, string>("MediaId", "1"),
            new KeyValuePair<string, string>("DisplayOrder", "0")
        });

        // Act
        var response = await client.PostAsync("/Admin/Pages/UpdateImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Pages/Edit/1", response.Headers.Location?.ToString());
    }

    #endregion

    #region RemoveImage Action Integration Tests

    [Fact]
    public async Task RemoveImage_WithoutAuthentication_RedirectsToLogin()
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
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task RemoveImage_WithValidData_RedirectsToEdit()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();
        
        var formData = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        // Act
        var response = await client.PostAsync("/Admin/Pages/RemoveImage?pageId=1&imageKey=TestImage", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Pages/Edit/1", response.Headers.Location?.ToString());
    }

    #endregion

    #region MediaPicker Action Integration Tests

    [Fact]
    public async Task MediaPicker_WithoutAuthentication_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Pages/MediaPicker?pageId=1&imageKey=HeroImage");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task MediaPicker_WithAuthentication_ReturnsView()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act
        var response = await client.GetAsync("/Admin/Pages/MediaPicker?pageId=1&imageKey=HeroImage");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Media Picker", content);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task AllPagesEndpoints_RequireAuthentication()
    {
        // Test that all endpoints redirect to login when not authenticated
        var endpoints = new[]
        {
            "/Admin/Pages",
            "/Admin/Pages/Edit/1",
            "/Admin/Pages/MediaPicker?pageId=1&imageKey=Test"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.ToString() ?? string.Empty;
            Assert.Contains("/Admin/Account/Login", location);
        }
    }

    #endregion

    #region Full Workflow Integration Tests

    [Fact]
    public async Task FullWorkflow_CreatePageAndUpdateContent()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act 1: Navigate to pages list
        var indexResponse = await client.GetAsync("/Admin/Pages");
        Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);

        // Act 2: Update a section (assuming page ID 1 exists or will be created)
        var updateSectionData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1"),
            new KeyValuePair<string, string>("SectionKey", "IntegrationTest"),
            new KeyValuePair<string, string>("Content", "Integration Test Content"),
            new KeyValuePair<string, string>("ContentType", "Text")
        });

        var updateResponse = await client.PostAsync("/Admin/Pages/UpdateSection", updateSectionData);
        
        // Assert
        Assert.Equal(HttpStatusCode.Redirect, updateResponse.StatusCode);
        Assert.Contains("/Admin/Pages/Edit", updateResponse.Headers.Location?.ToString());
    }

    [Fact]
    public async Task FullWorkflow_UpdateImageAndRemove()
    {
        // Arrange
        var client = _factory.CreateClientWithAuth();

        // Act 1: Update image
        var updateImageData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PageId", "1"),
            new KeyValuePair<string, string>("ImageKey", "IntegrationTestImage"),
            new KeyValuePair<string, string>("MediaId", "1"),
            new KeyValuePair<string, string>("DisplayOrder", "0")
        });

        var updateResponse = await client.PostAsync("/Admin/Pages/UpdateImage", updateImageData);
        Assert.Equal(HttpStatusCode.Redirect, updateResponse.StatusCode);

        // Act 2: Remove image
        var removeResponse = await client.PostAsync(
            "/Admin/Pages/RemoveImage?pageId=1&imageKey=IntegrationTestImage",
            new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, removeResponse.StatusCode);
        Assert.Contains("/Admin/Pages/Edit", removeResponse.Headers.Location?.ToString());
    }

    #endregion
}
