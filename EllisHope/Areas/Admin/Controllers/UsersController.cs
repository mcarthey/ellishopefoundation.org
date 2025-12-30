using EllisHope.Areas.Admin.Models;
using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(IgnoreApi = true)]
public class UsersController : Controller
{
    private readonly IUserManagementService _userService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IMediaService _mediaService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserManagementService userService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IMediaService mediaService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _userManager = userManager;
        _context = context;
        _mediaService = mediaService;
        _logger = logger;
    }

    // GET: Admin/Users
    /// <summary>
    /// list users. Roles: Admin.
    /// </summary>
    [SwaggerOperation(Summary = "list users. Roles: Admin.")]
    public async Task<IActionResult> Index(string? searchTerm, UserRole? roleFilter, MembershipStatus? statusFilter, bool? activeFilter)
    {
        var users = await _userService.GetAllUsersAsync();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            users = await _userService.SearchUsersAsync(searchTerm);
        }
        else
        {
            if (roleFilter.HasValue)
            {
                users = users.Where(u => u.UserRole == roleFilter.Value);
            }

            if (statusFilter.HasValue)
            {
                users = users.Where(u => u.Status == statusFilter.Value);
            }

            if (activeFilter.HasValue)
            {
                users = users.Where(u => u.IsActive == activeFilter.Value);
            }
        }

        var viewModel = new UserListViewModel
        {
            Users = users.Select(u => new UserSummaryViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                UserRole = u.UserRole,
                Status = u.Status,
                IsActive = u.IsActive,
                JoinedDate = u.JoinedDate,
                LastLoginDate = u.LastLoginDate,
                SponsorName = u.Sponsor?.FullName,
                SponsoredClientsCount = u.SponsoredClients?.Count ?? 0,
                ProfilePictureUrl = u.ProfilePictureUrl
            }),
            SearchTerm = searchTerm,
            RoleFilter = roleFilter,
            StatusFilter = statusFilter,
            ActiveFilter = activeFilter,
            TotalUsers = await _userService.GetTotalUsersCountAsync(),
            ActiveUsers = await _userService.GetActiveUsersCountAsync(),
            PendingUsers = await _userService.GetPendingUsersCountAsync()
        };

        return View(viewModel);
    }

    // GET: Admin/Users/Details/5
    /// <summary>
    /// view user details.
    /// </summary>
    [SwaggerOperation(Summary = "view user details.")]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var viewModel = new UserDetailsViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserRole = user.UserRole,
            Status = user.Status,
            IsActive = user.IsActive,
            Age = user.Age,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            City = user.City,
            State = user.State,
            ZipCode = user.ZipCode,
            EmergencyContactName = user.EmergencyContactName,
            EmergencyContactPhone = user.EmergencyContactPhone,
            SponsorId = user.SponsorId,
            SponsorName = user.Sponsor?.FullName,
            SponsoredClients = user.SponsoredClients.Select(c => new UserSummaryViewModel
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                PhoneNumber = c.PhoneNumber,
                UserRole = c.UserRole,
                Status = c.Status,
                IsActive = c.IsActive,
                JoinedDate = c.JoinedDate,
                LastLoginDate = c.LastLoginDate,
                ProfilePictureUrl = c.ProfilePictureUrl
            }),
            MonthlyFee = user.MonthlyFee,
            MembershipStartDate = user.MembershipStartDate,
            MembershipEndDate = user.MembershipEndDate,
            AdminNotes = user.AdminNotes,
            ProfilePictureUrl = user.ProfilePictureUrl,
            JoinedDate = user.JoinedDate,
            LastLoginDate = user.LastLoginDate,
            Roles = roles,
            // Sponsor company fields
            CompanyName = user.CompanyName,
            CompanyLogoUrl = user.CompanyLogoUrl,
            SponsorQuote = user.SponsorQuote,
            SponsorRating = user.SponsorRating,
            SponsorQuoteApproved = user.SponsorQuoteApproved,
            SponsorQuoteSubmittedDate = user.SponsorQuoteSubmittedDate,
            ShowInSponsorSection = user.ShowInSponsorSection
        };

        return View(viewModel);
    }

    // GET: Admin/Users/Create
    /// <summary>
    /// create user, assign role. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "create user, assign role. Anti-forgery required.")]
    public async Task<IActionResult> Create()
    {
        var viewModel = new UserCreateViewModel();
        return View(viewModel);
    }

    // POST: Admin/Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// create user, assign role. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "create user, assign role. Anti-forgery required.")]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        // Validate that board members must have a profile photo
        if (model.UserRole == UserRole.BoardMember)
        {
            var hasPhoto = model.ProfilePhoto?.Length > 0 || !string.IsNullOrEmpty(model.SelectedAvatarUrl);
            if (!hasPhoto)
            {
                ModelState.AddModelError("ProfilePhoto", "Board members must have a profile photo. Please upload a photo or select an avatar.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            UserRole = model.UserRole,
            Status = model.Status,
            IsActive = model.IsActive,
            DateOfBirth = model.DateOfBirth,
            Address = model.Address,
            City = model.City,
            State = model.State,
            ZipCode = model.ZipCode,
            EmergencyContactName = model.EmergencyContactName,
            EmergencyContactPhone = model.EmergencyContactPhone,
            SponsorId = model.SponsorId,
            MonthlyFee = model.MonthlyFee,
            MembershipStartDate = model.MembershipStartDate,
            MembershipEndDate = model.MembershipEndDate,
            AdminNotes = model.AdminNotes
        };

        // Handle profile photo upload or avatar selection
        if (model.ProfilePhoto?.Length > 0)
        {
            try
            {
                var media = await _mediaService.UploadLocalImageAsync(
                    model.ProfilePhoto,
                    $"{model.FirstName} {model.LastName} profile photo",
                    $"{model.FirstName} {model.LastName}",
                    MediaCategory.Team,
                    "profile,team",
                    User.Identity?.Name);
                user.ProfilePictureUrl = media.FilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload profile photo for new user {Email}", model.Email);
                ModelState.AddModelError("ProfilePhoto", "Failed to upload profile photo. Please try again.");
                return View(model);
            }
        }
        else if (!string.IsNullOrEmpty(model.SelectedAvatarUrl))
        {
            user.ProfilePictureUrl = model.SelectedAvatarUrl;
        }

        var (succeeded, errors) = await _userService.CreateUserAsync(user, model.Password);

        if (!succeeded)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        TempData["SuccessMessage"] = $"User '{user.FullName}' created successfully!";

        // TODO: Send welcome email if model.SendWelcomeEmail is true

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Users/Edit/5
    /// <summary>
    /// Displays the user edit form with all user details and available sponsors
    /// </summary>
    /// <param name="id">User ID to edit</param>
    [SwaggerOperation(Summary = "Displays the user edit form with all user details and available sponsors")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var sponsors = await _userService.GetSponsorsAsync();

        var viewModel = new UserEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserRole = user.UserRole,
            Status = user.Status,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            City = user.City,
            State = user.State,
            ZipCode = user.ZipCode,
            EmergencyContactName = user.EmergencyContactName,
            EmergencyContactPhone = user.EmergencyContactPhone,
            SponsorId = user.SponsorId,
            MonthlyFee = user.MonthlyFee,
            MembershipStartDate = user.MembershipStartDate,
            MembershipEndDate = user.MembershipEndDate,
            AdminNotes = user.AdminNotes,
            ProfilePictureUrl = user.ProfilePictureUrl,
            JoinedDate = user.JoinedDate,
            LastLoginDate = user.LastLoginDate,
            AvailableSponsors = sponsors.Select(s => new UserSelectItem
            {
                Id = s.Id,
                DisplayName = s.FullName
            }),
            // Sponsor company fields
            CompanyName = user.CompanyName,
            CompanyLogoUrl = user.CompanyLogoUrl,
            SponsorQuote = user.SponsorQuote,
            SponsorRating = user.SponsorRating,
            ShowInSponsorSection = user.ShowInSponsorSection,
            SponsorQuoteApproved = user.SponsorQuoteApproved,
            SponsorQuoteSubmittedDate = user.SponsorQuoteSubmittedDate
        };

        return View(viewModel);
    }

    // POST: Admin/Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Processes user edit form submission, updating user details, role, status, and sponsor assignment. Anti-forgery required.
    /// </summary>
    /// <param name="id">User ID to edit</param>
    /// <param name="model">Updated user information</param>
    [SwaggerOperation(Summary = "Processes user edit form submission, updating user details, role, status, and sponsor assignment. Anti-forgery required.")]
    public async Task<IActionResult> Edit(string id, UserEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            // Reload sponsors for dropdown
            var sponsors = await _userService.GetSponsorsAsync();
            model.AvailableSponsors = sponsors.Select(s => new UserSelectItem
            {
                Id = s.Id,
                DisplayName = s.FullName
            });
            return View(model);
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Update user properties
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.DateOfBirth = model.DateOfBirth;
        user.Address = model.Address;
        user.City = model.City;
        user.State = model.State;
        user.ZipCode = model.ZipCode;
        user.EmergencyContactName = model.EmergencyContactName;
        user.EmergencyContactPhone = model.EmergencyContactPhone;
        user.MonthlyFee = model.MonthlyFee;
        user.MembershipStartDate = model.MembershipStartDate;
        user.MembershipEndDate = model.MembershipEndDate;
        user.AdminNotes = model.AdminNotes;

        // Handle profile photo upload, avatar selection, or removal
        if (model.ProfilePhoto?.Length > 0)
        {
            try
            {
                var media = await _mediaService.UploadLocalImageAsync(
                    model.ProfilePhoto,
                    $"{model.FirstName} {model.LastName} profile photo",
                    $"{model.FirstName} {model.LastName}",
                    MediaCategory.Team,
                    "profile,team",
                    User.Identity?.Name);
                user.ProfilePictureUrl = media.FilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload profile photo for user {UserId}", id);
                ModelState.AddModelError("ProfilePhoto", "Failed to upload profile photo. Please try again.");
                var sponsors = await _userService.GetSponsorsAsync();
                model.AvailableSponsors = sponsors.Select(s => new UserSelectItem
                {
                    Id = s.Id,
                    DisplayName = s.FullName
                });
                return View(model);
            }
        }
        else if (!string.IsNullOrEmpty(model.SelectedAvatarUrl))
        {
            if (model.SelectedAvatarUrl == "__REMOVE__")
            {
                user.ProfilePictureUrl = null;
            }
            else
            {
                user.ProfilePictureUrl = model.SelectedAvatarUrl;
            }
        }

        // Handle sponsor company fields (for sponsor users)
        if (model.UserRole == UserRole.Sponsor || user.UserRole == UserRole.Sponsor)
        {
            user.CompanyName = model.CompanyName;
            user.SponsorQuote = model.SponsorQuote;
            user.SponsorRating = model.SponsorRating;
            user.ShowInSponsorSection = model.ShowInSponsorSection;

            // Handle company logo upload
            if (model.CompanyLogo?.Length > 0)
            {
                try
                {
                    var media = await _mediaService.UploadLocalImageAsync(
                        model.CompanyLogo,
                        $"{model.CompanyName ?? model.FirstName} company logo",
                        model.CompanyName ?? $"{model.FirstName} {model.LastName}",
                        MediaCategory.Logo,
                        "logo,sponsor",
                        User.Identity?.Name);
                    user.CompanyLogoUrl = media.FilePath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload company logo for user {UserId}", id);
                    ModelState.AddModelError("CompanyLogo", "Failed to upload company logo. Please try again.");
                    var sponsors = await _userService.GetSponsorsAsync();
                    model.AvailableSponsors = sponsors.Select(s => new UserSelectItem
                    {
                        Id = s.Id,
                        DisplayName = s.FullName
                    });
                    return View(model);
                }
            }
        }

        // Handle role change
        if (user.UserRole != model.UserRole)
        {
            var (roleSucceeded, roleErrors) = await _userService.UpdateUserRoleAsync(user.Id, model.UserRole);
            if (!roleSucceeded)
            {
                foreach (var error in roleErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }
        }

        // Handle status change
        if (user.Status != model.Status)
        {
            user.Status = model.Status;
        }

        // Handle active status change
        if (user.IsActive != model.IsActive)
        {
            user.IsActive = model.IsActive;
        }

        // Handle sponsor assignment
        if (user.SponsorId != model.SponsorId)
        {
            if (!string.IsNullOrEmpty(model.SponsorId))
            {
                await _userService.AssignSponsorAsync(user.Id, model.SponsorId);
            }
            else if (user.SponsorId != null)
            {
                await _userService.RemoveSponsorAsync(user.Id);
            }
        }

        var (succeeded, errors) = await _userService.UpdateUserAsync(user);

        if (!succeeded)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        TempData["SuccessMessage"] = $"User '{user.FullName}' updated successfully!";
        return RedirectToAction(nameof(Details), new { id = user.Id });
    }

    // GET: Admin/Users/Delete/5
    /// <summary>
    /// delete user (confirmation flow).
    /// </summary>
    [SwaggerOperation(Summary = "delete user (confirmation flow).")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new UserDeleteViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            UserRole = user.UserRole,
            SponsoredClientsCount = user.SponsoredClients?.Count ?? 0
        };

        return View(viewModel);
    }

    // POST: Admin/Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// delete confirmed action.
    /// </summary>
    [SwaggerOperation(Summary = "delete confirmed action.")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var (succeeded, errors) = await _userService.DeleteUserAsync(id);

        if (!succeeded)
        {
            TempData["ErrorMessage"] = string.Join(", ", errors);
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["SuccessMessage"] = "User deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Users/PendingSponsorQuotes
    /// <summary>
    /// List sponsors with pending quotes awaiting approval.
    /// </summary>
    [SwaggerOperation(Summary = "List sponsors with pending quotes awaiting approval.")]
    public async Task<IActionResult> PendingSponsorQuotes()
    {
        var pendingQuotes = await _context.Users
            .Where(u => u.UserRole == UserRole.Sponsor
                && u.IsActive
                && !string.IsNullOrEmpty(u.SponsorQuote)
                && !u.SponsorQuoteApproved
                && string.IsNullOrEmpty(u.SponsorQuoteRejectionReason))
            .OrderBy(u => u.SponsorQuoteSubmittedDate)
            .Select(u => new PendingSponsorQuoteViewModel
            {
                UserId = u.Id,
                SponsorName = u.FirstName + " " + u.LastName,
                CompanyName = u.CompanyName,
                CompanyLogoUrl = u.CompanyLogoUrl,
                Quote = u.SponsorQuote,
                Rating = u.SponsorRating,
                SubmittedDate = u.SponsorQuoteSubmittedDate,
                Email = u.Email
            })
            .ToListAsync();

        return View(pendingQuotes);
    }

    // POST: Admin/Users/ApproveQuote
    /// <summary>
    /// Approve a sponsor's quote for display on the About page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Approve a sponsor's quote.")]
    public async Task<IActionResult> ApproveQuote(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }

        var sponsor = await _context.Users.FindAsync(userId);
        if (sponsor == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);

        sponsor.SponsorQuoteApproved = true;
        sponsor.SponsorQuoteApprovedDate = DateTime.UtcNow;
        sponsor.SponsorQuoteApprovedById = currentUser?.Id;
        sponsor.SponsorQuoteRejectionReason = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} approved quote for sponsor {SponsorId}",
            currentUser?.Id, userId);

        TempData["SuccessMessage"] = $"Quote for {sponsor.FullName} has been approved.";
        return RedirectToAction(nameof(PendingSponsorQuotes));
    }

    // GET: Admin/Users/RejectQuote/5
    /// <summary>
    /// Display form to reject a sponsor's quote with feedback.
    /// </summary>
    [SwaggerOperation(Summary = "Display form to reject a sponsor's quote.")]
    public async Task<IActionResult> RejectQuote(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var sponsor = await _context.Users.FindAsync(id);
        if (sponsor == null)
        {
            return NotFound();
        }

        var viewModel = new RejectQuoteViewModel
        {
            UserId = sponsor.Id,
            SponsorName = sponsor.FullName,
            CompanyName = sponsor.CompanyName,
            Quote = sponsor.SponsorQuote
        };

        return View(viewModel);
    }

    // POST: Admin/Users/RejectQuote
    /// <summary>
    /// Reject a sponsor's quote with feedback.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Reject a sponsor's quote with feedback.")]
    public async Task<IActionResult> RejectQuote(RejectQuoteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sponsor = await _context.Users.FindAsync(model.UserId);
        if (sponsor == null)
        {
            return NotFound();
        }

        sponsor.SponsorQuoteApproved = false;
        sponsor.SponsorQuoteRejectionReason = model.Reason;

        await _context.SaveChangesAsync();

        var currentUser = await _userManager.GetUserAsync(User);
        _logger.LogInformation("Admin {AdminId} rejected quote for sponsor {SponsorId} with reason: {Reason}",
            currentUser?.Id, model.UserId, model.Reason);

        TempData["SuccessMessage"] = $"Quote for {sponsor.FullName} has been rejected with feedback.";
        return RedirectToAction(nameof(PendingSponsorQuotes));
    }

    /// <summary>
    /// Get count of pending sponsor quotes for navigation badge.
    /// </summary>
    public async Task<int> GetPendingQuotesCount()
    {
        return await _context.Users
            .CountAsync(u => u.UserRole == UserRole.Sponsor
                && u.IsActive
                && !string.IsNullOrEmpty(u.SponsorQuote)
                && !u.SponsorQuoteApproved
                && string.IsNullOrEmpty(u.SponsorQuoteRejectionReason));
    }
}
