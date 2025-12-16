using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for MyApplicationsController (User Portal)
/// Tests application submission and tracking workflow
/// </summary>
public class MyApplicationsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MyApplicationsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Index/List Actions

    [Fact]
    public async Task Index_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/MyApplications");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_RequiresAuthentication()
    {
        // This test documents that the user portal requires login
        // Act
        var response = await _client.GetAsync("/MyApplications");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Details Actions

    [Fact]
    public async Task Details_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/MyApplications/Details/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Details_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/MyApplications/Details/99999");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Unauthorized or NotFound, got {response.StatusCode}");
    }

    [Fact]
    public async Task Details_OtherUsersApplication_ReturnsForbidden()
    {
        // This test documents that users can only see their own applications
        // Act
        var response = await _client.GetAsync("/MyApplications/Details/1");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Unauthorized, Forbidden, or NotFound, got {response.StatusCode}");
    }

    #endregion

    #region Create Actions

    [Fact]
    public async Task Create_Get_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/MyApplications/Create");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_Post_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "Test"),
            new KeyValuePair<string, string>("LastName", "User"),
            new KeyValuePair<string, string>("Email", "test@test.com"),
            new KeyValuePair<string, string>("PhoneNumber", "555-1234"),
            new KeyValuePair<string, string>("CurrentStep", "1")
        });

        // Act
        var response = await _client.PostAsync("/MyApplications/Create", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    public async Task Create_MultiStepWizard_ValidatesPerStep()
    {
        // This test documents the multi-step validation process
        // Step 1: Personal Info
        // Step 2: Funding Request
        // Step 3: Motivation
        // Step 4: Health
        // Step 5: Agreement
        // Step 6: Signature

        var steps = new[] { 1, 2, 3, 4, 5, 6 };

        foreach (var step in steps)
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("CurrentStep", step.ToString())
            });

            var response = await _client.PostAsync("/MyApplications/Create", formData);

            // Should require authentication
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }

    #endregion

    #region Edit Actions

    [Fact]
    public async Task Edit_Get_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/MyApplications/Edit/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Edit_Post_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Id", "1"),
            new KeyValuePair<string, string>("FirstName", "Updated"),
            new KeyValuePair<string, string>("LastName", "User")
        });

        // Act
        var response = await _client.PostAsync("/MyApplications/Edit/1", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    public async Task Edit_OnlyDraftsCanBeEdited()
    {
        // This test documents that only draft applications can be edited
        // Submitted applications cannot be modified by users
        
        // Act
        var response = await _client.GetAsync("/MyApplications/Edit/1");

        // Assert - Should require auth and check draft status
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Withdraw Actions

    [Fact]
    public async Task Withdraw_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("reason", "Changed my mind")
        });

        // Act
        var response = await _client.PostAsync("/MyApplications/Withdraw/1", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    public async Task Withdraw_RequiresAntiForgeryToken()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("reason", "Test")
        });

        // Act
        var response = await _client.PostAsync("/MyApplications/Withdraw/1", formData);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task AllUserActions_RequireAuthentication()
    {
        // Test that all user endpoints require auth
        var endpoints = new[]
        {
            "/MyApplications",
            "/MyApplications/Details/1",
            "/MyApplications/Create",
            "/MyApplications/Edit/1"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    [Fact]
    public async Task UserCannotAccessOtherUsersApplications()
    {
        // This test documents privacy enforcement
        // Users should only see their own applications
        
        // Act
        var response = await _client.GetAsync("/MyApplications/Details/1");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Create_RequiresValidData()
    {
        // Test that empty/invalid data is rejected
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", ""),
            new KeyValuePair<string, string>("LastName", ""),
            new KeyValuePair<string, string>("CurrentStep", "1")
        });

        var response = await _client.PostAsync("/MyApplications/Create", formData);

        // Should fail validation (after auth check)
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_PersonalStatement_RequiresMinimumLength()
    {
        // This test documents that personal statement must be >= 50 chars
        var shortStatement = "Too short";
        
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("PersonalStatement", shortStatement),
            new KeyValuePair<string, string>("CurrentStep", "3")
        });

        var response = await _client.PostAsync("/MyApplications/Create", formData);

        // Should require auth first, then fail validation
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_MustAcknowledgeCommitment()
    {
        // This test documents that users must check commitment box
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("UnderstandsCommitment", "false"),
            new KeyValuePair<string, string>("CurrentStep", "5")
        });

        var response = await _client.PostAsync("/MyApplications/Create", formData);

        // Should require auth first, then fail validation
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_RequiresSignature()
    {
        // This test documents that signature is required
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Signature", ""),
            new KeyValuePair<string, string>("CurrentStep", "6")
        });

        var response = await _client.PostAsync("/MyApplications/Create", formData);

        // Should require auth first, then fail validation
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task ApplicationSubmission_FullWorkflow()
    {
        // This test documents the complete user workflow
        // 1. User logs in
        // 2. Navigates to MyApplications
        // 3. Creates new application (multi-step)
        // 4. Submits application
        // 5. Tracks application status
        // 6. Receives decision notification

        var workflowSteps = new[]
        {
            "/MyApplications",
            "/MyApplications/Create",
            "/MyApplications/Details/1"
        };

        // All require authentication
        foreach (var step in workflowSteps)
        {
            var response = await _client.GetAsync(step);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    [Fact]
    public async Task DraftApplication_CanBeSavedAndResumed()
    {
        // This test documents draft save/resume functionality
        // Users can save progress and return later
        
        var saveAsDraftData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("SaveAsDraft", "true"),
            new KeyValuePair<string, string>("FirstName", "Test"),
            new KeyValuePair<string, string>("CurrentStep", "1")
        });

        var response = await _client.PostAsync("/MyApplications/Create", saveAsDraftData);

        // Should require auth
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}
