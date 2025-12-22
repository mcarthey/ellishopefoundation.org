using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
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

        // Handle profile photo upload
        if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
        {
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
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST: Admin/Profile/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
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
