using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Services;

/// <summary>
/// Service for managing user responsibilities (content area permissions).
/// </summary>
public class ResponsibilityService : IResponsibilityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResponsibilityService> _logger;

    public ResponsibilityService(
        ApplicationDbContext context,
        ILogger<ResponsibilityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasResponsibilityAsync(string userId, Responsibility responsibility)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        return await _context.UserResponsibilities
            .AnyAsync(ur => ur.UserId == userId && ur.Responsibility == responsibility);
    }

    public async Task<bool> HasAnyResponsibilityAsync(string userId, params Responsibility[] responsibilities)
    {
        if (string.IsNullOrEmpty(userId) || responsibilities.Length == 0)
            return false;

        return await _context.UserResponsibilities
            .AnyAsync(ur => ur.UserId == userId && responsibilities.Contains(ur.Responsibility));
    }

    public async Task<bool> CanAutoApproveAsync(string userId, Responsibility responsibility)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        var userResponsibility = await _context.UserResponsibilities
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Responsibility == responsibility);

        return userResponsibility?.AutoApprove ?? false;
    }

    public async Task<IEnumerable<UserResponsibility>> GetUserResponsibilitiesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Enumerable.Empty<UserResponsibility>();

        return await _context.UserResponsibilities
            .Include(ur => ur.AssignedBy)
            .Where(ur => ur.UserId == userId)
            .OrderBy(ur => ur.Responsibility)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersWithResponsibilityAsync(Responsibility responsibility)
    {
        return await _context.UserResponsibilities
            .Where(ur => ur.Responsibility == responsibility)
            .Include(ur => ur.User)
            .Select(ur => ur.User!)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string[] Errors)> AssignResponsibilityAsync(
        string userId,
        Responsibility responsibility,
        bool autoApprove,
        string assignedById)
    {
        try
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found." });
            }

            // Check if responsibility already assigned
            var existing = await _context.UserResponsibilities
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Responsibility == responsibility);

            if (existing != null)
            {
                return (false, new[] { $"User already has the {responsibility} responsibility." });
            }

            var userResponsibility = new UserResponsibility
            {
                UserId = userId,
                Responsibility = responsibility,
                AutoApprove = autoApprove,
                AssignedById = assignedById,
                AssignedDate = DateTime.UtcNow
            };

            _context.UserResponsibilities.Add(userResponsibility);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Assigned {Responsibility} responsibility to user {UserId} by {AssignedById}. AutoApprove: {AutoApprove}",
                responsibility, userId, assignedById, autoApprove);

            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning {Responsibility} to user {UserId}", responsibility, userId);
            return (false, new[] { "An error occurred while assigning the responsibility." });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateAutoApproveAsync(
        string userId,
        Responsibility responsibility,
        bool autoApprove)
    {
        try
        {
            var userResponsibility = await _context.UserResponsibilities
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Responsibility == responsibility);

            if (userResponsibility == null)
            {
                return (false, new[] { $"User does not have the {responsibility} responsibility." });
            }

            userResponsibility.AutoApprove = autoApprove;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Updated AutoApprove to {AutoApprove} for {Responsibility} responsibility of user {UserId}",
                autoApprove, responsibility, userId);

            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AutoApprove for {Responsibility} of user {UserId}", responsibility, userId);
            return (false, new[] { "An error occurred while updating the responsibility." });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> RemoveResponsibilityAsync(
        string userId,
        Responsibility responsibility)
    {
        try
        {
            var userResponsibility = await _context.UserResponsibilities
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.Responsibility == responsibility);

            if (userResponsibility == null)
            {
                return (false, new[] { $"User does not have the {responsibility} responsibility." });
            }

            _context.UserResponsibilities.Remove(userResponsibility);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Removed {Responsibility} responsibility from user {UserId}",
                responsibility, userId);

            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing {Responsibility} from user {UserId}", responsibility, userId);
            return (false, new[] { "An error occurred while removing the responsibility." });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> RemoveAllResponsibilitiesAsync(string userId)
    {
        try
        {
            var responsibilities = await _context.UserResponsibilities
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (!responsibilities.Any())
            {
                return (true, Array.Empty<string>());
            }

            _context.UserResponsibilities.RemoveRange(responsibilities);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Removed all {Count} responsibilities from user {UserId}",
                responsibilities.Count, userId);

            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing all responsibilities from user {UserId}", userId);
            return (false, new[] { "An error occurred while removing responsibilities." });
        }
    }
}
