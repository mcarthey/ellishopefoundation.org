using EllisHope.Models.Domain;
using EllisHope.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Sponsor)
            .Include(u => u.SponsoredClients)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Include(u => u.Sponsor)
            .Include(u => u.SponsoredClients)
            .Where(u => u.UserRole == role)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersByStatusAsync(MembershipStatus status)
    {
        return await _context.Users
            .Include(u => u.Sponsor)
            .Include(u => u.SponsoredClients)
            .Where(u => u.Status == status)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _context.Users
            .Include(u => u.Sponsor)
            .Include(u => u.SponsoredClients)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _context.Users
            .Include(u => u.Sponsor)
            .Include(u => u.SponsoredClients)
            .Where(u => 
                u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                u.LastName.ToLower().Contains(lowerSearchTerm) ||
                u.Email.ToLower().Contains(lowerSearchTerm) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetSponsorsAsync()
    {
        return await _context.Users
            .Include(u => u.SponsoredClients)
            .Where(u => u.UserRole == UserRole.Sponsor || u.SponsoredClients.Any())
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetClientsAsync()
    {
        return await _context.Users
            .Include(u => u.Sponsor)
            .Where(u => u.UserRole == UserRole.Client)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetSponsoredClientsAsync(string sponsorId)
    {
        return await _context.Users
            .Where(u => u.SponsorId == sponsorId)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string[] Errors)> CreateUserAsync(ApplicationUser user, string password, string? roleName = null)
    {
        try
        {
            user.JoinedDate = DateTime.UtcNow;
            user.UserName = user.Email; // Use email as username
            
            var result = await _userManager.CreateAsync(user, password);
            
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            // Assign role based on UserRole enum or explicit role name
            var roleToAssign = roleName ?? user.UserRole.ToString();
            var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
            
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning($"User created but role assignment failed for {user.Email}");
            }

            _logger.LogInformation($"User created: {user.Email} with role {roleToAssign}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating user: {user.Email}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(ApplicationUser user)
    {
        try
        {
            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            _logger.LogInformation($"User updated: {user.Email}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user: {user.Email}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            // Check if user has sponsored clients
            if (user.SponsoredClients.Any())
            {
                return (false, new[] { $"Cannot delete user: {user.SponsoredClients.Count()} sponsored client(s) must be reassigned first" });
            }

            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            _logger.LogInformation($"User deleted: {user.Email}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user: {userId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> AssignSponsorAsync(string clientId, string sponsorId)
    {
        try
        {
            var client = await GetUserByIdAsync(clientId);
            var sponsor = await GetUserByIdAsync(sponsorId);

            if (client == null || sponsor == null)
            {
                return (false, new[] { "Client or sponsor not found" });
            }

            client.SponsorId = sponsorId;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Sponsor assigned: {sponsor.FullName} ? {client.FullName}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning sponsor: {sponsorId} to client: {clientId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> RemoveSponsorAsync(string clientId)
    {
        try
        {
            var client = await GetUserByIdAsync(clientId);
            if (client == null)
            {
                return (false, new[] { "Client not found" });
            }

            client.SponsorId = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Sponsor removed from client: {client.FullName}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing sponsor from client: {clientId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateUserRoleAsync(string userId, UserRole newRole)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            var oldRole = user.UserRole;
            user.UserRole = newRole;

            // Update Identity role
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, newRole.ToString());

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User role updated: {user.Email} from {oldRole} to {newRole}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user role: {userId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateUserStatusAsync(string userId, MembershipStatus newStatus)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            user.Status = newStatus;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User status updated: {user.Email} to {newStatus}");
            return (true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user status: {userId}");
            return (false, new[] { ex.Message });
        }
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsActive && u.Status == MembershipStatus.Active);
    }

    public async Task<int> GetPendingUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.Status == MembershipStatus.Pending);
    }
}
