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
[Authorize(Roles = "Sponsor")]
[ApiExplorerSettings(IgnoreApi = true)]
public class SponsorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMediaService _mediaService;
    private readonly ILogger<SponsorController> _logger;

    public SponsorController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IMediaService mediaService,
        ILogger<SponsorController> logger)
    {
        _context = context;
        _userManager = userManager;
        _mediaService = mediaService;
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

    // GET: Admin/Sponsor/MyCompanyProfile
    /// <summary>
    /// Edit company profile and testimonial for About page showcase.
    /// </summary>
    [SwaggerOperation(Summary = "Edit company profile and testimonial.")]
    public async Task<IActionResult> MyCompanyProfile()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        var viewModel = new SponsorProfileViewModel
        {
            CompanyName = currentUser.CompanyName,
            CurrentCompanyLogoUrl = currentUser.CompanyLogoUrl,
            SponsorQuote = currentUser.SponsorQuote,
            SponsorRating = currentUser.SponsorRating,
            ShowInSponsorSection = currentUser.ShowInSponsorSection,
            QuoteApproved = currentUser.SponsorQuoteApproved,
            QuoteSubmittedDate = currentUser.SponsorQuoteSubmittedDate,
            RejectionReason = currentUser.SponsorQuoteRejectionReason,
            QuoteStatus = GetQuoteStatus(currentUser)
        };

        return View(viewModel);
    }

    // POST: Admin/Sponsor/MyCompanyProfile
    /// <summary>
    /// Save company profile and testimonial changes.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(Summary = "Save company profile and testimonial.")]
    public async Task<IActionResult> MyCompanyProfile(SponsorProfileViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return NotFound();
        }

        // For validation: quote is optional, but if provided must meet length requirements
        if (!string.IsNullOrWhiteSpace(model.SponsorQuote) && model.SponsorQuote.Length < 20)
        {
            ModelState.AddModelError(nameof(model.SponsorQuote), "Quote must be at least 20 characters if provided.");
        }

        if (!ModelState.IsValid)
        {
            model.CurrentCompanyLogoUrl = currentUser.CompanyLogoUrl;
            model.QuoteApproved = currentUser.SponsorQuoteApproved;
            model.QuoteSubmittedDate = currentUser.SponsorQuoteSubmittedDate;
            model.QuoteStatus = GetQuoteStatus(currentUser);
            return View(model);
        }

        try
        {
            // Track if quote was changed (for re-approval logic)
            var quoteChanged = currentUser.SponsorQuote != model.SponsorQuote;
            var wasApproved = currentUser.SponsorQuoteApproved;

            // Handle company logo upload
            if (model.CompanyLogo?.Length > 0)
            {
                try
                {
                    var media = await _mediaService.UploadLocalImageAsync(
                        model.CompanyLogo,
                        $"{model.CompanyName ?? currentUser.FullName} company logo",
                        model.CompanyName ?? currentUser.FullName,
                        MediaCategory.Logo,
                        "sponsor,company,logo",
                        User.Identity?.Name);
                    currentUser.CompanyLogoUrl = media.FilePath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload company logo for sponsor {UserId}", currentUser.Id);
                    ModelState.AddModelError("CompanyLogo", "Failed to upload company logo. Please try again.");
                    model.CurrentCompanyLogoUrl = currentUser.CompanyLogoUrl;
                    model.QuoteApproved = currentUser.SponsorQuoteApproved;
                    model.QuoteSubmittedDate = currentUser.SponsorQuoteSubmittedDate;
                    model.QuoteStatus = GetQuoteStatus(currentUser);
                    return View(model);
                }
            }

            // Update company info
            currentUser.CompanyName = model.CompanyName;
            currentUser.ShowInSponsorSection = model.ShowInSponsorSection;

            // Update quote and rating
            currentUser.SponsorQuote = model.SponsorQuote;
            currentUser.SponsorRating = model.SponsorRating;

            // If quote changed and was previously approved, reset approval status
            if (quoteChanged && wasApproved && !string.IsNullOrWhiteSpace(model.SponsorQuote))
            {
                currentUser.SponsorQuoteApproved = false;
                currentUser.SponsorQuoteApprovedDate = null;
                currentUser.SponsorQuoteApprovedById = null;
                currentUser.SponsorQuoteSubmittedDate = DateTime.UtcNow;
                currentUser.SponsorQuoteRejectionReason = null;

                _logger.LogInformation("Sponsor {UserId} updated their quote, resetting approval status", currentUser.Id);
                TempData["InfoMessage"] = "Your quote has been updated and is pending approval.";
            }
            else if (quoteChanged && !string.IsNullOrWhiteSpace(model.SponsorQuote))
            {
                // New quote submitted or quote updated after rejection
                currentUser.SponsorQuoteSubmittedDate = DateTime.UtcNow;
                currentUser.SponsorQuoteRejectionReason = null;

                _logger.LogInformation("Sponsor {UserId} submitted a new quote for approval", currentUser.Id);
                TempData["InfoMessage"] = "Your quote has been submitted for approval.";
            }
            else if (string.IsNullOrWhiteSpace(model.SponsorQuote))
            {
                // Quote removed
                currentUser.SponsorQuoteApproved = false;
                currentUser.SponsorQuoteApprovedDate = null;
                currentUser.SponsorQuoteApprovedById = null;
                currentUser.SponsorQuoteSubmittedDate = null;
                currentUser.SponsorQuoteRejectionReason = null;
            }

            await _context.SaveChangesAsync();

            if (TempData["InfoMessage"] == null)
            {
                TempData["SuccessMessage"] = "Your company profile has been updated successfully.";
            }

            return RedirectToAction(nameof(MyCompanyProfile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company profile for sponsor {UserId}", currentUser.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while saving your profile. Please try again.");
            model.CurrentCompanyLogoUrl = currentUser.CompanyLogoUrl;
            model.QuoteApproved = currentUser.SponsorQuoteApproved;
            model.QuoteSubmittedDate = currentUser.SponsorQuoteSubmittedDate;
            model.QuoteStatus = GetQuoteStatus(currentUser);
            return View(model);
        }
    }

    private static string GetQuoteStatus(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(user.SponsorQuote))
            return "Not Submitted";

        if (user.SponsorQuoteApproved)
            return "Approved";

        if (!string.IsNullOrWhiteSpace(user.SponsorQuoteRejectionReason))
            return "Rejected";

        return "Pending Approval";
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
