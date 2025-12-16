using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EllisHope.Tests.Helpers;

/// <summary>
/// Helper class for managing authentication in integration tests
/// </summary>
public static class TestAuthenticationHelper
{
    public const string TestScheme = "TestScheme";
    
    /// <summary>
    /// Creates a test user with specified role and returns their ID
    /// </summary>
    public static async Task<string> CreateTestUserAsync(
        IServiceProvider services,
        string email,
        string firstName,
        string lastName,
        UserRole userRole,
        string password = "Test@123456")
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure role exists
        var roleName = userRole.ToString();
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return existingUser.Id;
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            UserRole = userRole,
            Status = MembershipStatus.Active,
            IsActive = true,
            JoinedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, roleName);
        
        return user.Id;
    }

    /// <summary>
    /// Creates a sponsor with a sponsored client for testing
    /// </summary>
    public static async Task<(string sponsorId, string clientId)> CreateSponsorWithClientAsync(IServiceProvider services)
    {
        var sponsorId = await CreateTestUserAsync(
            services,
            "sponsor@test.com",
            "Test",
            "Sponsor",
            UserRole.Sponsor);

        var clientId = await CreateTestUserAsync(
            services,
            "client@test.com",
            "Test",
            "Client",
            UserRole.Client);

        // Assign sponsor to client
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = await context.Users.FindAsync(clientId);
        if (client != null)
        {
            client.SponsorId = sponsorId;
            client.MonthlyFee = 100m;
            client.MembershipStartDate = DateTime.UtcNow.AddMonths(-1);
            client.MembershipEndDate = DateTime.UtcNow.AddMonths(11);
            await context.SaveChangesAsync();
        }

        return (sponsorId, clientId);
    }

    /// <summary>
    /// Gets authentication cookie for a test user
    /// </summary>
    public static async Task<string> GetAuthCookieAsync(IServiceProvider services, string userId)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        var roles = await userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // In a real scenario, you'd generate a proper authentication cookie
        // For testing, we'll use a simplified approach
        return $"test-auth-{userId}";
    }
}

/// <summary>
/// Test authentication handler for integration tests
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IServiceProvider serviceProvider)
        : base(options, logger, encoder)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the request has a test authentication header
        if (!Request.Headers.ContainsKey("X-Test-User-Id"))
        {
            return AuthenticateResult.NoResult();
        }

        var userId = Request.Headers["X-Test-User-Id"].ToString();
        
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return AuthenticateResult.Fail("Invalid test user");
        }

        var roles = await userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, TestAuthenticationHelper.TestScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthenticationHelper.TestScheme);

        return AuthenticateResult.Success(ticket);
    }
}
