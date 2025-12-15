using EllisHope.Models.Domain;
using EllisHope.Areas.Admin.Models;

namespace EllisHope.Services;

public interface IUserManagementService
{
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(UserRole role);
    Task<IEnumerable<ApplicationUser>> GetUsersByStatusAsync(MembershipStatus status);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm);
    Task<IEnumerable<ApplicationUser>> GetSponsorsAsync();
    Task<IEnumerable<ApplicationUser>> GetClientsAsync();
    Task<IEnumerable<ApplicationUser>> GetSponsoredClientsAsync(string sponsorId);
    Task<(bool Succeeded, string[] Errors)> CreateUserAsync(ApplicationUser user, string password, string? roleName = null);
    Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(ApplicationUser user);
    Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId);
    Task<(bool Succeeded, string[] Errors)> AssignSponsorAsync(string clientId, string sponsorId);
    Task<(bool Succeeded, string[] Errors)> RemoveSponsorAsync(string clientId);
    Task<(bool Succeeded, string[] Errors)> UpdateUserRoleAsync(string userId, UserRole newRole);
    Task<(bool Succeeded, string[] Errors)> UpdateUserStatusAsync(string userId, MembershipStatus newStatus);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetPendingUsersCountAsync();
}
