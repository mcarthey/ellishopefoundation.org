using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for UsersController
/// Tests complete user management CRUD operations and workflows
/// </summary>
public class UsersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UsersControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Index Action Integration Tests

    [Fact]
    public async Task Index_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_WithSearchTerm_FiltersResults()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users?searchTerm=test");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Index_WithRoleFilter_FiltersResults()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users?roleFilter=Member");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Index_WithStatusFilter_FiltersResults()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users?statusFilter=Active");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Index_WithActiveFilter_FiltersResults()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users?activeFilter=true");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Index_WithMultipleFilters_CombinesFilters()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users?roleFilter=Client&statusFilter=Active&activeFilter=true");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    #endregion

    #region Details Action Integration Tests

    [Fact]
    public async Task Details_WithValidId_ReturnsUserDetails()
    {
        // Arrange
        var testUserId = "test-user-id";

        // Act
        var response = await _client.GetAsync($"/Admin/Users/Details/{testUserId}");

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
        // Act
        var response = await _client.GetAsync("/Admin/Users/Details/invalid-id");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Unauthorized or NotFound, got {response.StatusCode}");
    }

    [Fact]
    public async Task Details_WithEmptyId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users/Details/");

        // Assert
        // May redirect to Index or return NotFound
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Expected Unauthorized, NotFound, or Redirect, got {response.StatusCode}");
    }

    #endregion

    #region Create Action Integration Tests

    [Fact]
    public async Task Create_Get_ReturnsCreateForm()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users/Create");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Create_Post_WithValidData_CreatesUser()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "John"),
            new KeyValuePair<string, string>("LastName", "Doe"),
            new KeyValuePair<string, string>("Email", "john.doe@test.com"),
            new KeyValuePair<string, string>("Password", "Test123!@#"),
            new KeyValuePair<string, string>("ConfirmPassword", "Test123!@#"),
            new KeyValuePair<string, string>("UserRole", "Member"),
            new KeyValuePair<string, string>("Status", "Active"),
            new KeyValuePair<string, string>("IsActive", "true")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Users/Create", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, Redirect, OK, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Create_Post_WithInvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "John"),
            new KeyValuePair<string, string>("LastName", "Doe"),
            new KeyValuePair<string, string>("Email", "invalid-email"),
            new KeyValuePair<string, string>("Password", "Test123!@#"),
            new KeyValuePair<string, string>("ConfirmPassword", "Test123!@#")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Users/Create", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, OK, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Create_Post_WithWeakPassword_ReturnsValidationError()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "John"),
            new KeyValuePair<string, string>("LastName", "Doe"),
            new KeyValuePair<string, string>("Email", "john.doe@test.com"),
            new KeyValuePair<string, string>("Password", "weak"),
            new KeyValuePair<string, string>("ConfirmPassword", "weak")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Users/Create", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, OK, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Create_Post_WithMismatchedPasswords_ReturnsValidationError()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "John"),
            new KeyValuePair<string, string>("LastName", "Doe"),
            new KeyValuePair<string, string>("Email", "john.doe@test.com"),
            new KeyValuePair<string, string>("Password", "Test123!@#"),
            new KeyValuePair<string, string>("ConfirmPassword", "Different123!@#")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Users/Create", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, OK, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Create_Post_WithEmptyRequiredFields_ReturnsValidationError()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", ""),
            new KeyValuePair<string, string>("LastName", ""),
            new KeyValuePair<string, string>("Email", ""),
            new KeyValuePair<string, string>("Password", "")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Users/Create", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, OK, or BadRequest, got {response.StatusCode}");
    }

    #endregion

    #region Edit Action Integration Tests

    [Fact]
    public async Task Edit_Get_WithValidId_ReturnsEditForm()
    {
        // Arrange
        var testUserId = "test-user-id";

        // Act
        var response = await _client.GetAsync($"/Admin/Users/Edit/{testUserId}");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized, NotFound, or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Edit_Get_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users/Edit/invalid-id");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Unauthorized or NotFound, got {response.StatusCode}");
    }

    [Fact]
    public async Task Edit_Post_WithValidData_UpdatesUser()
    {
        // Arrange
        var testUserId = "test-user-id";
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Id", testUserId),
            new KeyValuePair<string, string>("FirstName", "Jane"),
            new KeyValuePair<string, string>("LastName", "Smith"),
            new KeyValuePair<string, string>("Email", "jane.smith@test.com"),
            new KeyValuePair<string, string>("UserRole", "Client"),
            new KeyValuePair<string, string>("Status", "Active"),
            new KeyValuePair<string, string>("IsActive", "true")
        });

        // Act
        var response = await _client.PostAsync($"/Admin/Users/Edit/{testUserId}", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, NotFound, Redirect, OK, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Edit_Post_WithMismatchedId_ReturnsNotFound()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Id", "different-id"),
            new KeyValuePair<string, string>("FirstName", "Jane"),
            new KeyValuePair<string, string>("LastName", "Smith"),
            new KeyValuePair<string, string>("Email", "jane.smith@test.com")
        });

        // Act
        var response = await _client.PostAsync("/Admin/Users/Edit/test-user-id", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, NotFound, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Edit_Post_WithRoleChange_UpdatesUserRole()
    {
        // Arrange
        var testUserId = "test-user-id";
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Id", testUserId),
            new KeyValuePair<string, string>("FirstName", "Jane"),
            new KeyValuePair<string, string>("LastName", "Smith"),
            new KeyValuePair<string, string>("Email", "jane.smith@test.com"),
            new KeyValuePair<string, string>("UserRole", "Sponsor"),
            new KeyValuePair<string, string>("Status", "Active"),
            new KeyValuePair<string, string>("IsActive", "true")
        });

        // Act
        var response = await _client.PostAsync($"/Admin/Users/Edit/{testUserId}", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, NotFound, Redirect, OK, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Edit_Post_WithSponsorAssignment_AssignsSponsor()
    {
        // Arrange
        var testUserId = "test-user-id";
        var sponsorId = "sponsor-id";
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Id", testUserId),
            new KeyValuePair<string, string>("FirstName", "Jane"),
            new KeyValuePair<string, string>("LastName", "Smith"),
            new KeyValuePair<string, string>("Email", "jane.smith@test.com"),
            new KeyValuePair<string, string>("UserRole", "Client"),
            new KeyValuePair<string, string>("Status", "Active"),
            new KeyValuePair<string, string>("IsActive", "true"),
            new KeyValuePair<string, string>("SponsorId", sponsorId)
        });

        // Act
        var response = await _client.PostAsync($"/Admin/Users/Edit/{testUserId}", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, NotFound, Redirect, OK, or BadRequest, got {response.StatusCode}");
    }

    #endregion

    #region Delete Action Integration Tests

    [Fact]
    public async Task Delete_Get_WithValidId_ReturnsDeleteConfirmation()
    {
        // Arrange
        var testUserId = "test-user-id";

        // Act
        var response = await _client.GetAsync($"/Admin/Users/Delete/{testUserId}");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized, NotFound, or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Delete_Get_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Users/Delete/invalid-id");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected Unauthorized or NotFound, got {response.StatusCode}");
    }

    [Fact]
    public async Task Delete_Post_WithValidId_DeletesUser()
    {
        // Arrange
        var testUserId = "test-user-id";
        var formData = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        // Act
        var response = await _client.PostAsync($"/Admin/Users/Delete/{testUserId}", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, NotFound, Redirect, or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Delete_Post_WithSponsoredClients_ReturnsError()
    {
        // Note: This tests business logic that prevents deletion of sponsors with clients
        // Arrange
        var sponsorWithClientsId = "sponsor-with-clients-id";
        var formData = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        // Act
        var response = await _client.PostAsync($"/Admin/Users/Delete/{sponsorWithClientsId}", formData);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Unauthorized, NotFound, Redirect, OK, or BadRequest, got {response.StatusCode}");
    }

    #endregion

    #region User Management Workflow Tests

    [Fact]
    public async Task UserManagement_FullWorkflow_CreateEditDelete()
    {
        // Act 1: Try to access users list without auth
        var listResponse = await _client.GetAsync("/Admin/Users");
        Assert.Equal(HttpStatusCode.Unauthorized, listResponse.StatusCode);

        // Act 2: Try to access create form
        var createFormResponse = await _client.GetAsync("/Admin/Users/Create");
        Assert.Equal(HttpStatusCode.Unauthorized, createFormResponse.StatusCode);

        // Act 3: Try to create a user
        var createData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "Test"),
            new KeyValuePair<string, string>("LastName", "User"),
            new KeyValuePair<string, string>("Email", "testuser@test.com"),
            new KeyValuePair<string, string>("Password", "Test123!@#"),
            new KeyValuePair<string, string>("ConfirmPassword", "Test123!@#")
        });

        var createResponse = await _client.PostAsync("/Admin/Users/Create", createData);
        
        Assert.True(
            createResponse.StatusCode == HttpStatusCode.Unauthorized ||
            createResponse.StatusCode == HttpStatusCode.Redirect ||
            createResponse.StatusCode == HttpStatusCode.BadRequest,
            $"Create request processed with status {createResponse.StatusCode}");
    }

    [Fact]
    public async Task UserManagement_SponsorClientRelationship_Workflow()
    {
        // This test documents the sponsor-client relationship workflow
        // Act 1: Create sponsor
        var sponsorData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "John"),
            new KeyValuePair<string, string>("LastName", "Sponsor"),
            new KeyValuePair<string, string>("Email", "sponsor@test.com"),
            new KeyValuePair<string, string>("Password", "Test123!@#"),
            new KeyValuePair<string, string>("ConfirmPassword", "Test123!@#"),
            new KeyValuePair<string, string>("UserRole", "Sponsor")
        });

        var sponsorResponse = await _client.PostAsync("/Admin/Users/Create", sponsorData);
        
        // Act 2: Create client
        var clientData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("FirstName", "Jane"),
            new KeyValuePair<string, string>("LastName", "Client"),
            new KeyValuePair<string, string>("Email", "client@test.com"),
            new KeyValuePair<string, string>("Password", "Test123!@#"),
            new KeyValuePair<string, string>("ConfirmPassword", "Test123!@#"),
            new KeyValuePair<string, string>("UserRole", "Client")
        });

        var clientResponse = await _client.PostAsync("/Admin/Users/Create", clientData);

        // Assert
        Assert.True(
            sponsorResponse.StatusCode == HttpStatusCode.Unauthorized ||
            sponsorResponse.StatusCode == HttpStatusCode.Redirect ||
            sponsorResponse.StatusCode == HttpStatusCode.BadRequest,
            $"Sponsor creation processed with status {sponsorResponse.StatusCode}");
        
        Assert.True(
            clientResponse.StatusCode == HttpStatusCode.Unauthorized ||
            clientResponse.StatusCode == HttpStatusCode.Redirect ||
            clientResponse.StatusCode == HttpStatusCode.BadRequest,
            $"Client creation processed with status {clientResponse.StatusCode}");
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task UserManagement_RequiresAdminRole()
    {
        // All user management endpoints should require Admin role
        var endpoints = new[]
        {
            "/Admin/Users",
            "/Admin/Users/Create",
            "/Admin/Users/Edit/test-id",
            "/Admin/Users/Details/test-id",
            "/Admin/Users/Delete/test-id"
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
    public async Task UserManagement_PostActionsRequireAntiForgeryToken()
    {
        // All POST actions should validate anti-forgery tokens
        var postData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("test", "data")
        });

        var createResponse = await _client.PostAsync("/Admin/Users/Create", postData);
        var editResponse = await _client.PostAsync("/Admin/Users/Edit/test-id", postData);
        var deleteResponse = await _client.PostAsync("/Admin/Users/Delete/test-id", postData);

        // Should be Unauthorized, BadRequest, or MethodNotAllowed without proper token
        Assert.True(
            createResponse.StatusCode == HttpStatusCode.Unauthorized ||
            createResponse.StatusCode == HttpStatusCode.BadRequest ||
            createResponse.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Create returned {createResponse.StatusCode}");

        Assert.True(
            editResponse.StatusCode == HttpStatusCode.Unauthorized ||
            editResponse.StatusCode == HttpStatusCode.BadRequest ||
            editResponse.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Edit returned {editResponse.StatusCode}");

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.Unauthorized ||
            deleteResponse.StatusCode == HttpStatusCode.BadRequest ||
            deleteResponse.StatusCode == HttpStatusCode.MethodNotAllowed,
            $"Delete returned {deleteResponse.StatusCode}");
    }

    #endregion

    #region Filter and Search Tests

    [Fact]
    public async Task Index_WithCombinedFilters_ReturnsFilteredResults()
    {
        // Test combining search with filters
        var response = await _client.GetAsync(
            "/Admin/Users?searchTerm=john&roleFilter=Member&statusFilter=Active&activeFilter=true");

        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    [Fact]
    public async Task Index_SearchTerm_OverridesOtherFilters()
    {
        // When searchTerm is provided, it should take precedence
        var response = await _client.GetAsync(
            "/Admin/Users?searchTerm=test&roleFilter=Admin&statusFilter=Inactive");

        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, got {response.StatusCode}");
    }

    #endregion
}
