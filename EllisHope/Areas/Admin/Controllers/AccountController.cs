using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "Admin")]
[SwaggerTag("Account management endpoints for admin area (login, registration, password reset, etc.)")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountEmailService _accountEmailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAccountEmailService accountEmailService,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accountEmailService = accountEmailService;
        _logger = logger;
    }

    /// <summary>
    /// Show the login page.
    /// </summary>
    /// <param name="returnUrl">Optional return URL after login</param>
    [HttpGet]
    /// <summary>
    /// sign in (fields: `Email`, `Password`, `RememberMe`). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "sign in (fields: `Email`, `Password`, `RememberMe`). Anti-forgery required.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Sign in a user.
    /// </summary>
    /// <param name="model">Login form data</param>
    /// <param name="returnUrl">Optional return URL after login</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// sign in (fields: `Email`, `Password`, `RememberMe`). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "sign in (fields: `Email`, `Password`, `RememberMe`). Anti-forgery required.")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    /// <summary>
    /// Log out the current user.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// sign out. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "sign out. Anti-forgery required.")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    /// <summary>
    /// Show the registration page.
    /// </summary>
    /// <param name="returnUrl">Optional return URL after registration</param>
    [HttpGet]
    /// <summary>
    /// register new user (RegisterViewModel fields). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "register new user (RegisterViewModel fields). Anti-forgery required.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="model">Registration form data</param>
    /// <param name="returnUrl">Optional return URL after registration</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// register new user (RegisterViewModel fields). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "register new user (RegisterViewModel fields). Anti-forgery required.")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
                await _accountEmailService.SendWelcomeEmailAsync(user);

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

    /// <summary>
    /// Show the access denied view.
    /// </summary>
    [HttpGet]
    /// <summary>
    /// access denied view.
    /// </summary>
    [SwaggerOperation(Summary = "access denied view.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Show the lockout view.
    /// </summary>
    [HttpGet]
    /// <summary>
    /// lockout view.
    /// </summary>
    [SwaggerOperation(Summary = "lockout view.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Lockout()
    {
        return View();
    }

    #region Password Reset

    /// <summary>
    /// Show the forgot password page.
    /// </summary>
    [HttpGet]
    /// <summary>
    /// TODO: Describe GET /Admin/Account/ForgotPassword
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Admin/Account/ForgotPassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    /// <summary>
    /// Request a password reset email.
    /// </summary>
    /// <param name="model">Forgot password form data</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// TODO: Describe POST /Admin/Account/ForgotPassword
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe POST /Admin/Account/ForgotPassword")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Always redirect to confirmation page to prevent user enumeration
        // Even if user doesn't exist, we show the same message
        if (user == null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", model.Email);
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // Generate password reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Build reset URL
        var resetUrl = Url.Action(
            nameof(ResetPassword),
            "Account",
            new { area = "Admin", email = user.Email, token },
            Request.Scheme);

        // Send password reset email
        await _accountEmailService.SendPasswordResetEmailAsync(user, token, resetUrl!);

        _logger.LogInformation("Password reset email sent to {Email}", model.Email);

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    /// <summary>
    /// Show the forgot password confirmation page.
    /// </summary>
    [HttpGet]
    /// <summary>
    /// TODO: Describe GET /Admin/Account/ForgotPasswordConfirmation
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Admin/Account/ForgotPasswordConfirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    /// <summary>
    /// Show the reset password form.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="token">Password reset token</param>
    [HttpGet]
    /// <summary>
    /// TODO: Describe GET /Admin/Account/ResetPassword
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Admin/Account/ResetPassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ResetPassword(string? email = null, string? token = null)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid password reset link.");
        }

        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };

        return View(model);
    }

    /// <summary>
    /// Reset a user's password.
    /// </summary>
    /// <param name="model">Reset password form data</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// TODO: Describe POST /Admin/Account/ResetPassword
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe POST /Admin/Account/ResetPassword")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset successful for {Email}", model.Email);

            // Send confirmation email
            await _accountEmailService.SendPasswordChangedConfirmationAsync(user);

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        // Add errors to ModelState
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    /// <summary>
    /// Show the reset password confirmation page.
    /// </summary>
    [HttpGet]
    /// <summary>
    /// TODO: Describe GET /Admin/Account/ResetPasswordConfirmation
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Admin/Account/ResetPasswordConfirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    #endregion
}
