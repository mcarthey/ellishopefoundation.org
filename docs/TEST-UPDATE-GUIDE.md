# Quick Fix Guide: Update Existing Integration Tests

## Overview
This guide provides step-by-step instructions for updating existing integration tests to use the new test authentication system.

---

## Step 1: Update Test File Imports

Add the new helper namespace to your test file:

```csharp
using EllisHope.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection; // If using scope creation
```

---

## Step 2: Replace Skipped Tests

### Before:
```csharp
[Fact(Skip = "Authentication in integration tests needs to be refactored")]
public async Task SomeTest()
{
    Assert.True(true); // Placeholder
}
```

### After:
```csharp
[Fact]
public async Task SomeTest()
{
    // Arrange
    var userId = await TestAuthenticationHelper.CreateTestUserAsync(
        _factory.Services,
        "admin@test.com",
        "Admin",
        "User",
        UserRole.Admin);
    
    var client = _factory.CreateAuthenticatedClient(userId);
    
    // Act
    var response = await client.GetAsync("/Admin/YourController");
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

---

## Step 3: Fix Unauthenticated Test Expectations

### Before:
```csharp
[Fact]
public async Task Action_WithoutAuth_RedirectsToLogin()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/Admin/Action");
    
    // Assert
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
}
```

### After:
```csharp
[Fact]
public async Task Action_WithoutAuth_ReturnsUnauthorized()
{
    // Arrange
    var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });
    
    // Act
    var response = await client.GetAsync("/Admin/Action");
    
    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

---

## Step 4: Replace Helper Methods

### Remove Old Helper Methods:
```csharp
// DELETE THESE:
private async Task<HttpClient> GetAuthenticatedClient() { }
private async Task CreateAuthenticatedAdmin() { }
private async Task<string> GetAdminUserId() { }
```

### Use New Helper Instead:
```csharp
// Use directly in tests:
var userId = await TestAuthenticationHelper.CreateTestUserAsync(
    _factory.Services,
    "user@test.com",
    "First",
    "Last",
    UserRole.Admin);

var client = _factory.CreateAuthenticatedClient(userId);
```

---

## Step 5: Handle Different User Roles

### Admin User:
```csharp
var adminId = await TestAuthenticationHelper.CreateTestUserAsync(
    _factory.Services,
    "admin@test.com",
    "Admin",
    "User",
    UserRole.Admin);
```

### Board Member:
```csharp
var boardId = await TestAuthenticationHelper.CreateTestUserAsync(
    _factory.Services,
    "board@test.com",
    "Board",
    "Member",
    UserRole.BoardMember);
```

### Editor:
```csharp
var editorId = await TestAuthenticationHelper.CreateTestUserAsync(
    _factory.Services,
    "editor@test.com",
    "Editor",
    "User",
    UserRole.Editor); // Note: Editor role might need special handling
```

---

## Step 6: Test Data Setup

### Creating Test Entities:
```csharp
[Fact]
public async Task Create_WithValidData_RedirectsToIndex()
{
    // Arrange - Create authenticated user
    var userId = await TestAuthenticationHelper.CreateTestUserAsync(
        _factory.Services,
        "admin@test.com",
        "Admin",
        "User",
        UserRole.Admin);
    
    var client = _factory.CreateAuthenticatedClient(userId);
    
    // Arrange - Create test data
    var formData = new Dictionary<string, string>
    {
        ["Title"] = "Test Item",
        ["Description"] = "Test Description",
        // ... other fields
    };
    
    // Act
    var response = await client.PostAsync(
        "/Admin/YourController/Create",
        new FormUrlEncodedContent(formData));
    
    // Assert
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
}
```

---

## Step 7: Handle Complex Scenarios

### Sponsor with Clients:
```csharp
[Fact]
public async Task SponsorAction_WithClients_ShowsData()
{
    // Arrange - Create sponsor and client relationship
    var (sponsorId, clientId) = await TestAuthenticationHelper.CreateSponsorWithClientAsync(_factory.Services);
    
    var client = _factory.CreateAuthenticatedClient(sponsorId);
    
    // Act
    var response = await client.GetAsync("/Admin/Sponsor/Dashboard");
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("Test Client", content);
}
```

### Custom User Properties:
```csharp
[Fact]
public async Task Action_WithCustomUserData_Works()
{
    // Arrange - Create user
    var userId = await TestAuthenticationHelper.CreateTestUserAsync(
        _factory.Services,
        "custom@test.com",
        "Custom",
        "User",
        UserRole.Client);
    
    // Arrange - Update user with custom data
    using (var scope = _factory.Services.CreateScope())
    {
        var context = _factory.GetDbContext();
        var user = await context.Users.FindAsync(userId);
        if (user != null)
        {
            user.MonthlyFee = 150m;
            user.MembershipStartDate = DateTime.UtcNow.AddMonths(-3);
            user.MembershipEndDate = DateTime.UtcNow.AddMonths(9);
            await context.SaveChangesAsync();
        }
    }
    
    var client = _factory.CreateAuthenticatedClient(userId);
    
    // Act & Assert
    // ... test logic
}
```

---

## Common Patterns

### Pattern 1: Simple GET Request
```csharp
[Fact]
public async Task Index_WithAuth_ReturnsSuccess()
{
    var userId = await TestAuthenticationHelper.CreateTestUserAsync(
        _factory.Services, "test@test.com", "Test", "User", UserRole.Admin);
    var client = _factory.CreateAuthenticatedClient(userId);
    
    var response = await client.GetAsync("/Admin/Controller");
    
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

### Pattern 2: POST Request
```csharp
[Fact]
public async Task Create_WithValidData_Succeeds()
{
    var userId = await TestAuthenticationHelper.CreateTestUserAsync(
        _factory.Services, "test@test.com", "Test", "User", UserRole.Admin);
    var client = _factory.CreateAuthenticatedClient(userId);
    
    var formData = new Dictionary<string, string> { /* ... */ };
    var response = await client.PostAsync("/Admin/Controller/Create", 
        new FormUrlEncodedContent(formData));
    
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
}
```

### Pattern 3: Authorization Test
```csharp
[Fact]
public async Task AdminAction_WithNonAdminUser_ReturnsForbidden()
{
    var userId = await TestAuthenticationHelper.CreateTestUserAsync(
        _factory.Services, "member@test.com", "Member", "User", UserRole.Member);
    var client = _factory.CreateAuthenticatedClient(userId);
    
    var response = await client.GetAsync("/Admin/AdminOnlyController");
    
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

---

## Checklist for Each Test File

- [ ] Add `using EllisHope.Tests.Helpers;`
- [ ] Remove `[Fact(Skip = "...")]` attributes
- [ ] Replace old authentication methods
- [ ] Update "Redirect" expectations to "Unauthorized" for unauth tests
- [ ] Use `TestAuthenticationHelper.CreateTestUserAsync()` for user creation
- [ ] Use `_factory.CreateAuthenticatedClient(userId)` for authenticated requests
- [ ] Test still passes with new authentication
- [ ] Test actually validates the behavior (not just placeholder)

---

## Example: Complete File Update

**Before (UsersIntegrationTests.cs):**
```csharp
[Fact(Skip = "Authentication in integration tests needs to be refactored")]
public async Task UsersIndex_WithAuth_ReturnsSuccess()
{
    Assert.True(true);
}

[Fact]
public async Task UsersIndex_WithoutAuth_RedirectsToLogin()
{
    var response = await _client.GetAsync("/Admin/Users");
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.Contains("/Admin/Account/Login", response.Headers.Location?.ToString());
}
```

**After (UsersIntegrationTests.cs):**
```csharp
using EllisHope.Tests.Helpers;
// ... other usings

[Fact]
public async Task UsersIndex_WithAuth_ReturnsSuccess()
{
    // Arrange
    var userId = await TestAuthenticationHelper.CreateTestUserAsync(
        _factory.Services,
        "admin@test.com",
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
public async Task UsersIndex_WithoutAuth_ReturnsUnauthorized()
{
    // Arrange
    var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });
    
    // Act
    var response = await client.GetAsync("/Admin/Users");
    
    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

---

## Priority Order for Updating

1. ? **Portal Tests** - Already done
2. **AccountControllerIntegrationTests** - Authentication is critical
3. **UsersIntegrationTests** - Already has TODOs
4. **CausesControllerIntegrationTests** - Most failing tests (61)
5. **BlogControllerIntegrationTests**
6. **EventsControllerIntegrationTests**
7. **MediaControllerIntegrationTests**
8. **PagesControllerIntegrationTests**

---

## Estimated Time

- **Per test file:** 30-60 minutes
- **Total for all files:** 3-5 hours
- **Verification:** 30 minutes

---

## Verification

After updating each file, run:

```bash
# Test specific file
dotnet test --filter "FullyQualifiedName~YourTestFileName"

# Check for improvements
dotnet test --verbosity minimal
```

Expected result: Decreasing number of failed tests with each file updated.

---

**Need Help?**
- See `docs/TESTING-IMPLEMENTATION.md` for detailed explanation
- Check `EllisHope.Tests/Integration/SponsorPortalIntegrationTests.cs` for reference
- Review `EllisHope.Tests/Helpers/TestAuthenticationHelper.cs` for available methods
