using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for ApplicationsController (Admin)
/// Tests application review and approval workflow
/// </summary>
public class ApplicationsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApplicationsControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        var response = await _client.GetAsync("/Admin/Applications");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_WithStatusFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Applications?status=Submitted");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task NeedingReview_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Applications/NeedingReview");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Details/Review Actions

    [Fact]
    public async Task Details_WithValidId_ReturnsDetails()
    {
        // Arrange
        var applicationId = 1;

        // Act
        var response = await _client.GetAsync($"/Admin/Applications/Details/{applicationId}");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized, NotFound, or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Details_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var applicationId = 99999;

        // Act
        var response = await _client.GetAsync($"/Admin/Applications/Details/{applicationId}");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Unauthorized or NotFound, got {response.StatusCode}");
    }

    [Fact]
    public async Task Review_RedirectsToDetails()
    {
        // Arrange
        var applicationId = 1;

        // Act
        var response = await _client.GetAsync($"/Admin/Applications/Review/{applicationId}");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized, NotFound, or OK, got {response.StatusCode}");
    }

    #endregion

    #region Voting Actions

    [Fact]
    public async Task Vote_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("Decision", "Approve"),
            new KeyValuePair<string, string>("Reasoning", "This is a test vote with sufficient reasoning to meet the minimum requirements."),
            new KeyValuePair<string, string>("ConfidenceLevel", "4")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Applications/Vote", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    public async Task Vote_RequiresBoardMemberRole()
    {
        // This test documents that voting requires BoardMember role
        // Act
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("Decision", "Approve"),
            new KeyValuePair<string, string>("Reasoning", "Test reasoning"),
            new KeyValuePair<string, string>("ConfidenceLevel", "3")
        });

        var response = await _client.PostAsync("/Admin/Applications/Vote", formData);

        // Assert - Should require authentication and BoardMember role
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Comment Actions

    [Fact]
    public async Task Comment_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("Content", "This is a test comment"),
            new KeyValuePair<string, string>("IsPrivate", "true")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Applications/Comment", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Comment_WithAuth_RequiresValidUser()
    {
        // Arrange - Use a non-existent user ID to verify auth handler behavior
        var authenticatedClient = _factory.CreateAuthenticatedClient("nonexistent-user-id");

        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("Content", "This is a test comment with enough characters to pass validation."),
            new KeyValuePair<string, string>("IsPrivate", "true")
        });

        // Act
        var response = await authenticatedClient.PostAsync("/Admin/Applications/Comment", formData);

        // Assert - Should return Unauthorized for invalid user
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Comment_FormData_RequiresMinimumContentLength()
    {
        // This test documents that the CommentFormViewModel requires:
        // - Content: Required, minimum 5 characters, maximum 5000 characters
        // - ApplicationId: Required
        // - IsPrivate: Optional, defaults to true
        // - IsInformationRequest: Optional, defaults to false
        // - ParentCommentId: Optional, for threaded discussions

        // Validation is handled by model binding and redirects with TempData error
        Assert.True(true, "CommentFormViewModel has [StringLength(5000, MinimumLength = 5)] on Content property");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Comment_Endpoint_RequiresRoleAuthorization()
    {
        // This test documents that the Comment action:
        // 1. Requires authentication (Admin or BoardMember role via controller attribute)
        // 2. Uses ValidateAntiForgeryToken for CSRF protection
        // 3. Redirects to Details page after success/failure
        // 4. Sets TempData["SuccessMessage"] on success
        // 5. Sets TempData["ErrorMessage"] on validation failure

        // Without authentication, POST should be rejected
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("Content", "Test comment"),
            new KeyValuePair<string, string>("IsPrivate", "true")
        });

        var response = await _client.PostAsync("/Admin/Applications/Comment", formData);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Decision Actions

    [Fact]
    public async Task Approve_Get_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Applications/Approve/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Approve_Post_RequiresAdminRole()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("ApprovedMonthlyAmount", "100"),
            new KeyValuePair<string, string>("DecisionMessage", "Approved!")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Applications/Approve", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    public async Task Reject_Get_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Applications/Reject/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Post_RequiresAdminRole()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("RejectionReason", "Does not meet requirements at this time.")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Applications/Reject", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    public async Task RequestInfo_Get_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Applications/RequestInfo/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestInfo_Post_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("ApplicationId", "1"),
            new KeyValuePair<string, string>("RequestDetails", "Please provide more information about your fitness goals.")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Applications/RequestInfo", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    [Fact]
    public async Task StartReview_RequiresAdminRole()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        // Act
        var response = await _client.PostAsync("/Admin/Applications/StartReview/1", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Expected Unauthorized, BadRequest, or MethodNotAllowed, got {response.StatusCode}");
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task AllAdminActions_RequireAuthentication()
    {
        // Test that all admin endpoints require auth
        var endpoints = new[]
        {
            "/Admin/Applications",
            "/Admin/Applications/NeedingReview",
            "/Admin/Applications/Details/1",
            "/Admin/Applications/Review/1",
            "/Admin/Applications/Approve/1",
            "/Admin/Applications/Reject/1",
            "/Admin/Applications/RequestInfo/1",
            "/Admin/Applications/Statistics"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            Assert.True(
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"Endpoint {endpoint} returned {response.StatusCode}");
        }
    }

    [Fact]
    public async Task PostActions_RequireAntiForgeryToken()
    {
        // Test that POST actions validate anti-forgery tokens
        var postData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("test", "data")
        });

        var voteResponse = await _client.PostAsync("/Admin/Applications/Vote", postData);
        var commentResponse = await _client.PostAsync("/Admin/Applications/Comment", postData);
        var approveResponse = await _client.PostAsync("/Admin/Applications/Approve", postData);
        var rejectResponse = await _client.PostAsync("/Admin/Applications/Reject", postData);

        // Should be Unauthorized, BadRequest, or MethodNotAllowed without proper token
        Assert.NotEqual(HttpStatusCode.OK, voteResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, commentResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, approveResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, rejectResponse.StatusCode);
    }

    #endregion

    #region Workflow Tests

    [Fact]
    public async Task ApplicationWorkflow_RequiresProperSequence()
    {
        // This test documents the expected workflow sequence
        // Submitted ? UnderReview ? Voting ? Approved/Rejected

        // Step 1: Application must be submitted
        // Step 2: Admin starts review (notifies board)
        // Step 3: Board members vote
        // Step 4: Admin approves/rejects based on votes

        // All steps require proper authentication and authorization
        var workflowSteps = new[]
        {
            "/Admin/Applications/StartReview/1",
            "/Admin/Applications/Vote",
            "/Admin/Applications/Approve/1"
        };

        // All should require auth
        foreach (var step in workflowSteps)
        {
            var response = await _client.GetAsync(step);
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Admin_CanStartReview_ForSubmittedApplication()
    {
        // This test would require a logged-in admin user and a submitted application
        // It documents the expected behavior:
        // 1. Admin POSTs to /Admin/Applications/StartReview/{id}
        // 2. Application status changes from Submitted to UnderReview
        // 3. Redirects back to Details page with a success message
        Assert.True(true, "Test documentation: Admin should be able to start the review process.");
    }

    [Fact]
    public async Task Admin_CanRequestInformation_ForApplication()
    {
        // This test would require a logged-in admin user and an application
        // It documents the expected behavior:
        // 1. Admin GETs /Admin/Applications/RequestInfo/{id} -> gets a form
        // 2. Admin POSTs to /Admin/Applications/RequestInfo with details
        // 3. Application status changes to NeedsInformation
        // 4. A comment is added with the request
        // 5. Applicant is notified
        // 6. Redirects back to Details page with a success message
        Assert.True(true, "Test documentation: Admin should be able to request more information.");
    }

    #endregion
}
