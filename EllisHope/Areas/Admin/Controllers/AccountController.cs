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
[ApiExplorerSettings(IgnoreApi = true)]
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
    /// Displays the admin login page
    /// </summary>
    /// <param name="returnUrl">Optional URL to redirect to after successful login</param>
    /// <returns>Login view with form for email and password</returns>
    /// <remarks>
    /// Renders the login form for admin area authentication. After successful login,
    /// users are redirected based on their role (Admin, BoardMember, Editor, Sponsor, Client, or Member).
    /// </remarks>
    /// <response code="200">Successfully displayed login page</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays the admin login page",
        Description = "Renders the login form where users enter email and password credentials to access the admin area.",
        OperationId = "GetLogin",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Processes admin login credentials and authenticates the user
    /// </summary>
    /// <param name="model">Login credentials containing Email, Password, and RememberMe options</param>
    /// <param name="returnUrl">Optional URL to redirect to after successful authentication</param>
    /// <returns>Redirects to appropriate dashboard based on user role, or returns to login with errors</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /Admin/Account/Login
    ///     {
    ///        "email": "admin@example.com",
    ///        "password": "SecurePassword123!",
    ///        "rememberMe": true
    ///     }
    ///
    /// On successful login, updates user's LastLoginDate and redirects based on role:
    /// - Admin → Admin Dashboard
    /// - BoardMember → BoardMember Dashboard
    /// - Editor → Blog Management
    /// - Sponsor → Sponsor Portal
    /// - Client → Client Portal
    /// - Member → Member Portal
    ///
    /// Implements account lockout after 5 failed attempts (15-minute lockout period).
    /// </remarks>
    /// <response code="302">Successful login - redirects to role-appropriate dashboard</response>
    /// <response code="200">Login failed - returns view with validation errors</response>
    /// <response code="400">Invalid model state</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Authenticates user credentials and creates login session",
        Description = "Validates email/password, updates last login timestamp, and redirects to role-based dashboard. Enforces lockout policy after multiple failed attempts.",
        OperationId = "PostLogin",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(LoginViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        // Admin - go to admin dashboard
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "BoardMember"))
                    {
                        // BoardMember - go to board member dashboard
                        return RedirectToAction("Dashboard", "BoardMember");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Editor"))
                    {
                        // Editor - go to blog management (primary editor area)
                        return RedirectToAction("Index", "Blog");
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
    [SwaggerOperation(Summary = "Sign out current user. Requires anti-forgery token.")]
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
    [SwaggerOperation(Summary = "Display registration form for new user.")]
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
    [SwaggerOperation(Summary = "Register new user with form data. Requires anti-forgery token.")]
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
    [SwaggerOperation(Summary = "Display access denied view.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Show the lockout view.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Display account lockout view.")]
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
    [SwaggerOperation(Summary = "Display forgot password form for password reset requests.")]
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
    [SwaggerOperation(Summary = "Process forgot password request and send reset email.")]
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
    [SwaggerOperation(Summary = "Display confirmation after forgot password request submitted.")]
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
    [SwaggerOperation(Summary = "Display password reset form with email and token from reset link.")]
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
    [SwaggerOperation(Summary = "Process password reset with new password.")]
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
    [SwaggerOperation(Summary = "Display confirmation after password successfully reset.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    #endregion
}
