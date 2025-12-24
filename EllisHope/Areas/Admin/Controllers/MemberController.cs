using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Member")]
[ApiExplorerSettings(IgnoreApi = true)]
public class MemberController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<MemberController> _logger;

    public MemberController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<MemberController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Admin/Member/Dashboard
    /// <summary>
    /// member admin dashboard.
    /// </summary>
    [SwaggerOperation(Summary = "member admin dashboard.")]
    public async Task<IActionResult> Dashboard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        var viewModel = new MemberDashboardViewModel
        {
            MemberName = currentUser.FullName,
            MemberEmail = currentUser.Email ?? string.Empty,
            Status = currentUser.Status,
            JoinedDate = currentUser.JoinedDate,
            LastLoginDate = currentUser.LastLoginDate
        };

        return View(viewModel);
    }

    // GET: Admin/Member/Events
    /// <summary>
    /// member events listing.
    /// </summary>
    [SwaggerOperation(Summary = "member events listing.")]
    public IActionResult Events()
    {
        return View();
    }

    // GET: Admin/Member/Volunteer
    /// <summary>
    /// volunteer opportunities and management.
    /// </summary>
    [SwaggerOperation(Summary = "volunteer opportunities and management.")]
    public IActionResult Volunteer()
    {
        return View();
    }

    // GET: Admin/Member/MyProfile
    /// <summary>
    /// member profile.
    /// </summary>
    [SwaggerOperation(Summary = "member profile.")]
    public async Task<IActionResult> MyProfile()
    {
        // Redirect to the standard profile page
        return RedirectToAction("Index", "Profile");
    }
}

// View Models
public class MemberDashboardViewModel
{
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public MembershipStatus Status { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
}
