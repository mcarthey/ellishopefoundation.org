using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Models;

/// <summary>
/// ViewModel for editing a user's responsibilities.
/// </summary>
public class EditResponsibilitiesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole UserRole { get; set; }

    /// <summary>
    /// List of all possible responsibilities with their current assignment state.
    /// </summary>
    public List<ResponsibilityAssignment> Responsibilities { get; set; } = new();
}

/// <summary>
/// Represents a single responsibility assignment for a user.
/// </summary>
public class ResponsibilityAssignment
{
    public Responsibility Responsibility { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
    public bool AutoApprove { get; set; }
}
