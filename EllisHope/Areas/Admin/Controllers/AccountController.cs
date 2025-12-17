using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailService = emailService;
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

                if (user != null)
                {
                    // Redirect based on user role
                    if (await _userManager.IsInRoleAsync(user, "Admin") || 
                        await _userManager.IsInRoleAsync(user, "BoardMember") || 
                        await _userManager.IsInRoleAsync(user, "Editor"))
                    {
                        // Admin, BoardMember, or Editor - go to admin dashboard
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Sponsor"))
                    {
                        // Sponsor - go to sponsor portal
                        return RedirectToAction("Dashboard", "Sponsor");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Client"))
                    {
                        // Client - go to client portal
                        return RedirectToAction("Dashboard", "Client");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Member"))
                    {
                        // Member - go to member portal (redirects to MyApplications)
                        return RedirectToAction("Dashboard", "Member");
                    }
                    else
                    {
                        // Default - go to applications
                        return RedirectToAction("Index", "MyApplications", new { area = "" });
                    }
                }

                // Fallback
                return RedirectToAction("Index", "Home", new { area = "" });
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
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserRole = UserRole.Member, // New users start as Members
                Status = MembershipStatus.Active,
                IsActive = true,
                JoinedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Add to Member role
                await _userManager.AddToRoleAsync(user, "Member");

                // Send welcome email
                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Welcome to Ellis Hope Foundation!",
                        $@"<html>
                            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                                <h2 style='color: #c53040;'>Welcome to Ellis Hope Foundation!</h2>
                                <p>Hi {user.FirstName},</p>
                                <p>Thank you for creating an account with us. We're excited to have you as part of our community!</p>
                                <p>You can now:</p>
                                <ul>
                                    <li>Apply for support programs</li>
                                    <li>View upcoming events</li>
                                    <li>Explore volunteer opportunities</li>
                                    <li>Stay updated with our latest news</li>
                                </ul>
                                <p><strong>Your Account Details:</strong></p>
                                <ul>
                                    <li>Email: {user.Email}</li>
                                    <li>Member Since: {user.JoinedDate:MMMM dd, yyyy}</li>
                                </ul>
                                <p>If you have any questions, please don't hesitate to contact us.</p>
                                <p>Best regards,<br/>The Ellis Hope Foundation Team</p>
                            </body>
                        </html>");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                    // Don't fail registration if email fails
                }

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redirect to member portal (which redirects to MyApplications)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Dashboard", "Member");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
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
