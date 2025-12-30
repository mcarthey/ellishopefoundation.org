using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service for managing user responsibilities (content area permissions).
/// </summary>
public interface IResponsibilityService
{
    /// <summary>
    /// Checks if a user has a specific responsibility.
    /// </summary>
    Task<bool> HasResponsibilityAsync(string userId, Responsibility responsibility);

    /// <summary>
    /// Checks if a user has any of the specified responsibilities.
    /// </summary>
    Task<bool> HasAnyResponsibilityAsync(string userId, params Responsibility[] responsibilities);

    /// <summary>
    /// Checks if a user can auto-approve content for a specific responsibility.
    /// Returns false if user doesn't have the responsibility.
    /// </summary>
    Task<bool> CanAutoApproveAsync(string userId, Responsibility responsibility);

    /// <summary>
    /// Gets all responsibility assignments for a user.
    /// </summary>
    Task<IEnumerable<UserResponsibility>> GetUserResponsibilitiesAsync(string userId);

    /// <summary>
    /// Gets all users who have a specific responsibility.
    /// </summary>
    Task<IEnumerable<ApplicationUser>> GetUsersWithResponsibilityAsync(Responsibility responsibility);

    /// <summary>
    /// Assigns a responsibility to a user.
    /// </summary>
    /// <param name="userId">The user to assign the responsibility to</param>
    /// <param name="responsibility">The responsibility type to assign</param>
    /// <param name="autoApprove">Whether the user can auto-approve content</param>
    /// <param name="assignedById">The admin user who is assigning this responsibility</param>
    /// <returns>Success status and any error messages</returns>
    Task<(bool Succeeded, string[] Errors)> AssignResponsibilityAsync(
        string userId,
        Responsibility responsibility,
        bool autoApprove,
        string assignedById);

    /// <summary>
    /// Updates the auto-approve setting for an existing responsibility assignment.
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> UpdateAutoApproveAsync(
        string userId,
        Responsibility responsibility,
        bool autoApprove);

    /// <summary>
    /// Removes a responsibility from a user.
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> RemoveResponsibilityAsync(
        string userId,
        Responsibility responsibility);

    /// <summary>
    /// Removes all responsibilities from a user.
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> RemoveAllResponsibilitiesAsync(string userId);
}
