using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Authorization;

namespace EllisHope.Authorization;

/// <summary>
/// Authorization requirement that checks if a user has one of the specified responsibilities.
/// </summary>
public class ResponsibilityRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The responsibilities that satisfy this requirement (any one of these is sufficient).
    /// </summary>
    public Responsibility[] AllowedResponsibilities { get; }

    /// <summary>
    /// Creates a new responsibility requirement.
    /// </summary>
    /// <param name="responsibilities">One or more responsibilities that satisfy this requirement.</param>
    public ResponsibilityRequirement(params Responsibility[] responsibilities)
    {
        AllowedResponsibilities = responsibilities ?? throw new ArgumentNullException(nameof(responsibilities));

        if (AllowedResponsibilities.Length == 0)
        {
            throw new ArgumentException("At least one responsibility must be specified.", nameof(responsibilities));
        }
    }
}
