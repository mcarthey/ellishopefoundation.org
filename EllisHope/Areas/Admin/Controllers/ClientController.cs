using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Client")]
public class ClientController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ClientController> _logger;

    public ClientController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ClientController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Admin/Client/Dashboard
    /// <summary>
    /// client admin dashboard.
    /// </summary>
    [SwaggerOperation(Summary = "client admin dashboard.")]
    public async Task<IActionResult> Dashboard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        // Get sponsor information if assigned
        ApplicationUser? sponsor = null;
        if (!string.IsNullOrEmpty(currentUser.SponsorId))
        {
            sponsor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUser.SponsorId);
        }

        // Calculate membership progress
        int? membershipDays = null;
        int? daysRemaining = null;
        double? membershipProgress = null;

        if (currentUser.MembershipStartDate.HasValue && currentUser.MembershipEndDate.HasValue)
        {
            var totalDays = (currentUser.MembershipEndDate.Value - currentUser.MembershipStartDate.Value).Days;
            var elapsedDays = (DateTime.UtcNow.Date - currentUser.MembershipStartDate.Value.Date).Days;
            daysRemaining = (currentUser.MembershipEndDate.Value.Date - DateTime.UtcNow.Date).Days;
            membershipDays = elapsedDays;
            
            if (totalDays > 0)
            {
                membershipProgress = Math.Min(100, (double)elapsedDays / totalDays * 100);
            }
        }

        var viewModel = new ClientDashboardViewModel
        {
            ClientName = currentUser.FullName,
            ClientEmail = currentUser.Email ?? string.Empty,
            Status = currentUser.Status,
            JoinedDate = currentUser.JoinedDate,
            MonthlyFee = currentUser.MonthlyFee ?? 0,
            MembershipStartDate = currentUser.MembershipStartDate,
            MembershipEndDate = currentUser.MembershipEndDate,
            MembershipDays = membershipDays,
            DaysRemaining = daysRemaining,
            MembershipProgress = membershipProgress,
            HasSponsor = sponsor != null,
            SponsorName = sponsor?.FullName,
            SponsorEmail = sponsor?.Email,
            SponsorPhone = sponsor?.PhoneNumber
        };

        return View(viewModel);
    }

    // GET: Admin/Client/Progress
    /// <summary>
    /// client progress view.
    /// </summary>
    [SwaggerOperation(Summary = "client progress view.")]
    public async Task<IActionResult> Progress()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        var viewModel = new ClientProgressViewModel
        {
            ClientName = currentUser.FullName,
            // TODO: Add actual progress tracking data when implemented
            // This is a placeholder structure
            Milestones = new List<ClientMilestone>
            {
                new() { Title = "Registration Complete", Date = currentUser.JoinedDate, IsCompleted = true },
                new() { Title = "Initial Assessment", IsCompleted = false },
                new() { Title = "First Training Session", IsCompleted = false },
                new() { Title = "30-Day Check-in", IsCompleted = false },
                new() { Title = "90-Day Review", IsCompleted = false }
            }
        };

        return View(viewModel);
    }

    // GET: Admin/Client/Resources
    /// <summary>
    /// client resources.
    /// </summary>
    [SwaggerOperation(Summary = "client resources.")]
    public IActionResult Resources()
    {
        return View();
    }

    // GET: Admin/Client/MyProfile
    /// <summary>
    /// view/edit client profile.
    /// </summary>
    [SwaggerOperation(Summary = "view/edit client profile.")]
    public async Task<IActionResult> MyProfile()
    {
        // Redirect to the standard profile page
        return RedirectToAction("Index", "Profile");
    }
}

// View Models
public class ClientDashboardViewModel
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public MembershipStatus Status { get; set; }
    public DateTime JoinedDate { get; set; }
    public decimal MonthlyFee { get; set; }
    public DateTime? MembershipStartDate { get; set; }
    public DateTime? MembershipEndDate { get; set; }
    public int? MembershipDays { get; set; }
    public int? DaysRemaining { get; set; }
    public double? MembershipProgress { get; set; }
    public bool HasSponsor { get; set; }
    public string? SponsorName { get; set; }
    public string? SponsorEmail { get; set; }
    public string? SponsorPhone { get; set; }
}

public class ClientProgressViewModel
{
    public string ClientName { get; set; } = string.Empty;
    public List<ClientMilestone> Milestones { get; set; } = new();
}

public class ClientMilestone
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public bool IsCompleted { get; set; }
}
