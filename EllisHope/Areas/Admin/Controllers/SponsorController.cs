using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Sponsor")]
public class SponsorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SponsorController> _logger;

    public SponsorController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<SponsorController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Admin/Sponsor/Dashboard
    /// <summary>
    /// sponsor dashboard.
    /// </summary>
    [SwaggerOperation(Summary = "sponsor dashboard.")]
    public async Task<IActionResult> Dashboard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        // Get sponsored clients
        var sponsoredClients = await _context.Users
            .Where(u => u.SponsorId == currentUser.Id)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        // Calculate statistics
        var totalClients = sponsoredClients.Count;
        var activeClients = sponsoredClients.Count(c => c.Status == MembershipStatus.Active);
        var pendingClients = sponsoredClients.Count(c => c.Status == MembershipStatus.Pending);
        var totalMonthlyCommitment = sponsoredClients
            .Where(c => c.Status == MembershipStatus.Active)
            .Sum(c => c.MonthlyFee ?? 0);

        var viewModel = new SponsorDashboardViewModel
        {
            SponsorName = currentUser.FullName,
            SponsorEmail = currentUser.Email ?? string.Empty,
            TotalClients = totalClients,
            ActiveClients = activeClients,
            PendingClients = pendingClients,
            TotalMonthlyCommitment = totalMonthlyCommitment,
            SponsoredClients = sponsoredClients.Select(c => new SponsoredClientViewModel
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                PhoneNumber = c.PhoneNumber,
                Status = c.Status,
                JoinedDate = c.JoinedDate,
                MonthlyFee = c.MonthlyFee ?? 0,
                LastLoginDate = c.LastLoginDate,
                Age = c.Age
            }).ToList()
        };

        return View(viewModel);
    }

    // GET: Admin/Sponsor/ClientDetails/{id}
    /// <summary>
    /// view client details assigned to sponsor.
    /// </summary>
    [SwaggerOperation(Summary = "view client details assigned to sponsor.")]
    public async Task<IActionResult> ClientDetails(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        var client = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.SponsorId == currentUser.Id);

        if (client == null)
        {
            _logger.LogWarning("Sponsor {SponsorId} attempted to access client {ClientId} they don't sponsor", 
                currentUser.Id, id);
            return NotFound("Client not found or you are not the sponsor for this client.");
        }

        var viewModel = new ClientDetailsViewModel
        {
            Id = client.Id,
            FullName = client.FullName,
            Email = client.Email ?? string.Empty,
            PhoneNumber = client.PhoneNumber,
            DateOfBirth = client.DateOfBirth,
            Age = client.Age,
            Address = client.Address,
            City = client.City,
            State = client.State,
            ZipCode = client.ZipCode,
            EmergencyContactName = client.EmergencyContactName,
            EmergencyContactPhone = client.EmergencyContactPhone,
            Status = client.Status,
            JoinedDate = client.JoinedDate,
            LastLoginDate = client.LastLoginDate,
            MonthlyFee = client.MonthlyFee ?? 0,
            MembershipStartDate = client.MembershipStartDate,
            MembershipEndDate = client.MembershipEndDate
        };

        return View(viewModel);
    }

    // GET: Admin/Sponsor/MyProfile
    /// <summary>
    /// sponsor profile.
    /// </summary>
    [SwaggerOperation(Summary = "sponsor profile.")]
    public async Task<IActionResult> MyProfile()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        // Redirect to the standard profile page
        return RedirectToAction("Index", "Profile");
    }
}

// View Models
public class SponsorDashboardViewModel
{
    public string SponsorName { get; set; } = string.Empty;
    public string SponsorEmail { get; set; } = string.Empty;
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int PendingClients { get; set; }
    public decimal TotalMonthlyCommitment { get; set; }
    public List<SponsoredClientViewModel> SponsoredClients { get; set; } = new();
}

public class SponsoredClientViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public MembershipStatus Status { get; set; }
    public DateTime JoinedDate { get; set; }
    public decimal MonthlyFee { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int? Age { get; set; }
}

public class ClientDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public MembershipStatus Status { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public decimal MonthlyFee { get; set; }
    public DateTime? MembershipStartDate { get; set; }
    public DateTime? MembershipEndDate { get; set; }
}
