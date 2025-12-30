using System.ComponentModel.DataAnnotations;

namespace EllisHope.Models.Domain;

/// <summary>
/// Represents a responsibility assignment for a user.
/// Responsibilities grant users the ability to manage specific content areas.
/// </summary>
public class UserResponsibility
{
    public int Id { get; set; }

    /// <summary>
    /// The user this responsibility is assigned to
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// The type of responsibility assigned
    /// </summary>
    [Required]
    public Responsibility Responsibility { get; set; }

    /// <summary>
    /// If true, content created by this user is published immediately.
    /// If false, content requires admin approval before publishing.
    /// </summary>
    public bool AutoApprove { get; set; } = false;

    /// <summary>
    /// When this responsibility was assigned
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The admin who assigned this responsibility
    /// </summary>
    [MaxLength(450)]
    public string? AssignedById { get; set; }
    public ApplicationUser? AssignedBy { get; set; }
}
