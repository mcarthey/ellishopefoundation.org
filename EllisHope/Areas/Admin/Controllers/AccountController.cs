using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            // Update last login date before sign in attempt
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                // Ensure user is in Admin role
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "You do not have admin access.");
                    return View(model);
                }
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }
}
