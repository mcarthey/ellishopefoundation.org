using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMediaService _mediaService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMediaService mediaService,
        ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mediaService = mediaService;
        _logger = logger;
    }

    // GET: Admin/Profile
    /// <summary>
    /// Displays the authenticated user's complete profile with personal information, role details, and account status
    /// </summary>
    /// <returns>View displaying user profile with all personal data, roles, and account metrics</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Admin/Profile
    ///     GET /Admin/Profile/Index
    ///
    /// Returns comprehensive profile information including:
    /// - Personal details (name, email, phone, address, date of birth)
    /// - Role and status information
    /// - Emergency contact details
    /// - Profile photo
    /// - Account metrics (join date, last login)
    /// - Assigned roles (Admin, BoardMember, Sponsor, etc.)
    ///
    /// Profile information is read-only on this page. Use Edit link to modify profile details.
    ///
    /// **Authorization:**
    /// - Requires authentication
    /// - Users can only view their own profile
    ///
    /// **User Roles Displayed:**
    /// - Admin, BoardMember, Member, Sponsor, Client, Editor
    /// </remarks>
    /// <response code="200">Successfully displayed user profile</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">User profile not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays authenticated user's complete profile information",
        Description = "Returns user profile view with personal details, roles, status, account metrics, and profile photo. Read-only display with edit link.",
        OperationId = "GetUserProfile",
        Tags = new[] { "Profile" }
    )]
    [ProducesResponseType(typeof(ProfileViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new ProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserRole = user.UserRole,
            Status = user.Status,
            DateOfBirth = user.DateOfBirth,
            Age = user.Age,
            Address = user.Address,
            City = user.City,
            State = user.State,
            ZipCode = user.ZipCode,
            EmergencyContactName = user.EmergencyContactName,
            EmergencyContactPhone = user.EmergencyContactPhone,
            ProfilePictureUrl = user.ProfilePictureUrl,
            JoinedDate = user.JoinedDate,
            LastLoginDate = user.LastLoginDate,
            Roles = roles
        };

        return View(model);
    }

    // GET: Admin/Profile/Edit
    /// <summary>
    /// Displays the profile edit form pre-populated with the authenticated user's current information
    /// </summary>
    /// <returns>View displaying edit form with user's current profile data</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Admin/Profile/Edit
    ///
    /// Returns edit form pre-filled with user's current profile information:
    /// - Name (First/Last)
    /// - Email address
    /// - Phone number
    /// - Date of birth
    /// - Address (street, city, state, zip)
    /// - Emergency contact details
    /// - Current profile photo (with option to upload new photo)
    ///
    /// **Editable Fields:**
    /// - All personal information fields
    /// - Profile photo upload (jpg, png, gif - max 5MB)
    /// - Email (triggers username update to match)
    ///
    /// **Non-Editable:**
    /// - User role (requires admin intervention)
    /// - Account status (requires admin intervention)
    /// - Join date and last login (system-managed)
    ///
    /// **Authorization:**
    /// - Requires authentication
    /// - Users can only edit their own profile
    /// </remarks>
    /// <response code="200">Successfully displayed edit form</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">User profile not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays profile edit form with current user information",
        Description = "Returns edit form pre-populated with user's profile data. Allows editing personal info, contact details, and profile photo upload.",
        OperationId = "GetEditProfile",
        Tags = new[] { "Profile" }
    )]
    [ProducesResponseType(typeof(EditProfileViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new EditProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            City = user.City,
            State = user.State,
            ZipCode = user.ZipCode,
            EmergencyContactName = user.EmergencyContactName,
            EmergencyContactPhone = user.EmergencyContactPhone,
            CurrentProfilePictureUrl = user.ProfilePictureUrl
        };

        return View(model);
    }

    // POST: Admin/Profile/Edit
    /// <summary>
    /// Processes profile update with personal information changes and optional profile photo upload
    /// </summary>
    /// <param name="model">Updated profile data including optional profile photo file</param>
    /// <returns>Redirects to profile view on success; returns form with errors on failure</returns>
    /// <remarks>
    /// Sample form submission:
    ///
    ///     POST /Admin/Profile/Edit
    ///     Content-Type: multipart/form-data
    ///
    ///     FirstName=John&amp;LastName=Doe&amp;Email=john.doe@example.com&amp;...&amp;ProfilePhoto=[file]
    ///
    /// **Update Process:**
    /// 1. Validates all form fields (required fields, formats, etc.)
    /// 2. If profile photo uploaded:
    ///    - Validates file type (jpg, png, gif) and size (max 5MB)
    ///    - Uploads to media service with automatic thumbnail generation
    ///    - Updates user's ProfilePictureUrl
    /// 3. Updates all personal information fields
    /// 4. If email changed:
    ///    - Updates email via UserManager
    ///    - Updates username to match new email (username = email)
    ///    - Validates email uniqueness
    /// 5. Saves changes to database
    /// 6. Redirects to profile view with success message
    ///
    /// **Validation:**
    /// - First/Last name required
    /// - Email required and must be valid format
    /// - Phone number optional but must be valid format if provided
    /// - Profile photo must be image file type and under size limit
    ///
    /// **Error Handling:**
    /// - Photo upload errors shown with specific error message
    /// - Email uniqueness conflicts displayed
    /// - Identity update errors from UserManager shown to user
    /// - Current profile photo preserved if new upload fails
    ///
    /// **Authorization:**
    /// - Requires authentication
    /// - Users can only update their own profile
    /// - Anti-forgery token required
    /// </remarks>
    /// <response code="302">Redirects to profile view after successful update</response>
    /// <response code="200">Returns form with validation errors</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">User profile not found</response>
    /// <response code="400">Validation failed or upload error</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Processes profile updates with photo upload and email change handling",
        Description = "Updates user profile information, handles photo upload with validation, updates email/username if changed. Validates all fields and displays errors.",
        OperationId = "PostEditProfile",
        Tags = new[] { "Profile" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(EditProfileViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Edit(EditProfileViewModel model, string? SelectedAvatarUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Always repopulate current profile picture URL for potential re-render
        model.CurrentProfilePictureUrl = user.ProfilePictureUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Handle profile photo: Upload takes priority, then avatar selection, then removal
        if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
        {
            // User uploaded a new photo
            try
            {
                var altText = $"{model.FirstName} {model.LastName} profile photo";
                var media = await _mediaService.UploadLocalImageAsync(
                    model.ProfilePhoto,
                    altText,
                    $"{model.FirstName} {model.LastName}",
                    MediaCategory.Team,
                    "profile,team",
                    User.Identity?.Name);

                user.ProfilePictureUrl = media.FilePath;
                _logger.LogInformation("User {UserId} uploaded a new profile photo: {FilePath}", user.Id, media.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile photo for user {UserId}", user.Id);
                ModelState.AddModelError("ProfilePhoto", $"Error uploading photo: {ex.Message}");
                return View(model);
            }
        }
        else if (!string.IsNullOrEmpty(SelectedAvatarUrl))
        {
            if (SelectedAvatarUrl == "__REMOVE__")
            {
                // User wants to remove their profile photo
                user.ProfilePictureUrl = null;
                _logger.LogInformation("User {UserId} removed their profile photo", user.Id);
            }
            else if (SelectedAvatarUrl.StartsWith("/assets/img/avatars/"))
            {
                // User selected a default avatar
                user.ProfilePictureUrl = SelectedAvatarUrl;
                _logger.LogInformation("User {UserId} selected default avatar: {AvatarUrl}", user.Id, SelectedAvatarUrl);
            }
        }

        // Update user properties
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.DateOfBirth = model.DateOfBirth;
        user.Address = model.Address;
        user.City = model.City;
        user.State = model.State;
        user.ZipCode = model.ZipCode;
        user.EmergencyContactName = model.EmergencyContactName;
        user.EmergencyContactPhone = model.EmergencyContactPhone;

        // Handle email change
        if (user.Email != model.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var error in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        _logger.LogInformation("User {UserId} updated their profile", user.Id);
        TempData["SuccessMessage"] = "Your profile has been updated successfully!";

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Profile/ChangePassword
    /// <summary>
    /// Displays the change password form for the authenticated user
    /// </summary>
    /// <returns>View displaying password change form with current/new password fields</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Admin/Profile/ChangePassword
    ///
    /// Returns password change form with:
    /// - Current password field (required for verification)
    /// - New password field (with strength requirements)
    /// - Confirm new password field (must match new password)
    ///
    /// **Password Requirements:**
    /// - Minimum 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// - At least one special character (@$!%*?&amp;)
    ///
    /// **Authorization:**
    /// - Requires authentication
    /// - Users can only change their own password
    /// </remarks>
    /// <response code="200">Successfully displayed password change form</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays password change form for authenticated user",
        Description = "Returns form with current password, new password, and confirmation fields. Enforces password strength requirements.",
        OperationId = "GetChangePassword",
        Tags = new[] { "Profile" }
    )]
    [ProducesResponseType(typeof(ChangePasswordViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST: Admin/Profile/ChangePassword
    /// <summary>
    /// Processes password change request with current password verification and new password validation
    /// </summary>
    /// <param name="model">Password change data including current password, new password, and confirmation</param>
    /// <returns>Redirects to profile view on success; returns form with errors on failure</returns>
    /// <remarks>
    /// Sample form submission:
    ///
    ///     POST /Admin/Profile/ChangePassword
    ///     Content-Type: application/x-www-form-urlencoded
    ///
    ///     CurrentPassword=OldPass123!&amp;NewPassword=NewSecurePass456!&amp;ConfirmPassword=NewSecurePass456!
    ///
    /// **Change Process:**
    /// 1. Validates current password is correct
    /// 2. Validates new password meets strength requirements:
    ///    - Minimum 8 characters
    ///    - Contains uppercase, lowercase, digit, and special character
    /// 3. Validates new password matches confirmation
    /// 4. Updates password via UserManager.ChangePasswordAsync
    /// 5. Refreshes user's sign-in to prevent logout
    /// 6. Logs password change event
    /// 7. Redirects to profile with success message
    ///
    /// **Validation Errors:**
    /// - Current password incorrect
    /// - New password doesn't meet strength requirements
    /// - New password doesn't match confirmation
    /// - New password same as current password
    ///
    /// **Security:**
    /// - Current password must be verified before change
    /// - New password strength validated by Identity configuration
    /// - User remains logged in after password change (sign-in refreshed)
    /// - Password change logged for audit trail
    ///
    /// **Authorization:**
    /// - Requires authentication
    /// - Users can only change their own password
    /// - Anti-forgery token required
    /// </remarks>
    /// <response code="302">Redirects to profile view after successful password change</response>
    /// <response code="200">Returns form with validation errors</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Validation failed or current password incorrect</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Processes password change with verification and validation",
        Description = "Verifies current password, validates new password strength, updates password, and refreshes sign-in. Requires anti-forgery token.",
        OperationId = "PostChangePassword",
        Tags = new[] { "Profile" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ChangePasswordViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("User {UserId} changed their password successfully", user.Id);
        
        TempData["SuccessMessage"] = "Your password has been changed successfully!";
        return RedirectToAction(nameof(Index));
    }
}
