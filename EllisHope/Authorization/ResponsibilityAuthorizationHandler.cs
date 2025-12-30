using System.Security.Claims;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;

namespace EllisHope.Authorization;

/// <summary>
/// Authorization handler that checks if a user has the required responsibility.
/// Admins always pass this check.
/// </summary>
public class ResponsibilityAuthorizationHandler : AuthorizationHandler<ResponsibilityRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ResponsibilityAuthorizationHandler> _logger;

    public ResponsibilityAuthorizationHandler(
        IServiceProvider serviceProvider,
        ILogger<ResponsibilityAuthorizationHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResponsibilityRequirement requirement)
    {
        var user = context.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        // Admins always have access
        if (user.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        // Use service scope to get the responsibility service
        using var scope = _serviceProvider.CreateScope();
        var responsibilityService = scope.ServiceProvider.GetRequiredService<IResponsibilityService>();

        // Check if user has any of the allowed responsibilities
        var hasResponsibility = await responsibilityService.HasAnyResponsibilityAsync(
            userId,
            requirement.AllowedResponsibilities);

        if (hasResponsibility)
        {
            _logger.LogDebug(
                "User {UserId} authorized via responsibility for requirements: {Responsibilities}",
                userId,
                string.Join(", ", requirement.AllowedResponsibilities));

            context.Succeed(requirement);
        }
    }
}
