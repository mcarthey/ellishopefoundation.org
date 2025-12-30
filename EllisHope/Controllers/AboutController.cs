using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AboutController : Controller
{
    private readonly ApplicationDbContext _context;

    public AboutController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Displays the About page with organization mission, values, and active board member directory
    /// </summary>
    /// <returns>View displaying organization information with board member profiles ordered alphabetically by last name</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /About
    ///     GET /About/Index
    ///
    /// Returns the About page featuring:
    /// - Organization mission statement and history
    /// - Core values and guiding principles
    /// - Impact statistics and success stories
    /// - Directory of active board members with profiles
    ///
    /// **Board Member Data:**
    /// - Only active board members are displayed (IsActive = true)
    /// - Members are ordered alphabetically by last name, then first name
    /// - Each profile may include: name, title, bio, photo, contact information
    ///
    /// Accessible to all users (no authentication required).
    /// </remarks>
    /// <response code="200">Successfully displayed About page with board member directory</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays About page with mission, values, board members, and sponsor showcase",
        Description = "Returns organization information page featuring mission statement, values, alphabetically-ordered list of active board members, and sponsor testimonials/logos. Public access.",
        OperationId = "GetAboutPage",
        Tags = new[] { "About" }
    )]
    [ProducesResponseType(typeof(AboutPageViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index()
    {
        // Get active board members
        var boardMembers = await _context.Users
            .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        // Get sponsors with approved quotes for testimonial carousel
        var sponsorsWithQuotes = await _context.Users
            .Where(u => u.UserRole == UserRole.Sponsor
                && u.IsActive
                && u.SponsorQuoteApproved
                && !string.IsNullOrEmpty(u.SponsorQuote)
                && u.ShowInSponsorSection)
            .OrderBy(u => u.SponsorQuoteApprovedDate)
            .ToListAsync();

        // Get all active sponsors for logo section (includes those without quotes)
        var allSponsors = await _context.Users
            .Where(u => u.UserRole == UserRole.Sponsor
                && u.IsActive
                && u.ShowInSponsorSection)
            .OrderBy(u => u.CompanyName ?? u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        // Build statistics from causes and fallback data
        var statistics = await BuildStatisticsAsync();

        var viewModel = new AboutPageViewModel
        {
            BoardMembers = boardMembers,
            SponsorsWithQuotes = sponsorsWithQuotes,
            AllSponsors = allSponsors,
            Statistics = statistics
        };

        return View(viewModel);
    }

    /// <summary>
    /// Builds statistics list from published causes (up to 3) and fills remaining slots with user/event counts
    /// </summary>
    private async Task<List<AboutStatistic>> BuildStatisticsAsync()
    {
        var statistics = new List<AboutStatistic>();

        // Get all published causes with fundraising goals
        var allCauses = await _context.Causes
            .Where(c => c.IsPublished && c.GoalAmount > 0)
            .ToListAsync();

        // Select up to 3 causes: featured first, then random selection from the rest
        var selectedCauses = SelectCausesWithPreference(allCauses, 3);

        // Add cause statistics
        foreach (var cause in selectedCauses)
        {
            var percentage = cause.GoalAmount > 0
                ? (int)Math.Min(Math.Round(cause.RaisedAmount / cause.GoalAmount * 100), 100)
                : 0;

            statistics.Add(new AboutStatistic
            {
                DisplayValue = $"{percentage}%",
                Label = cause.Title,
                Percentage = percentage,
                IsPercentage = true
            });
        }

        // If we need more stats, add fallback statistics
        if (statistics.Count < 3)
        {
            var fallbackStats = await GetFallbackStatisticsAsync();

            foreach (var stat in fallbackStats)
            {
                if (statistics.Count >= 3) break;
                statistics.Add(stat);
            }
        }

        return statistics;
    }

    /// <summary>
    /// Selects causes with preference for featured causes, then random selection from the rest
    /// </summary>
    private static List<Cause> SelectCausesWithPreference(List<Cause> allCauses, int count)
    {
        if (allCauses.Count <= count)
            return allCauses;

        var selected = new List<Cause>();
        var random = new Random();

        // First, add featured causes (randomized if more than needed)
        var featuredCauses = allCauses.Where(c => c.IsFeatured).ToList();
        if (featuredCauses.Count > 0)
        {
            var shuffledFeatured = featuredCauses.OrderBy(_ => random.Next()).ToList();
            selected.AddRange(shuffledFeatured.Take(count));
        }

        // If we still need more, randomly select from non-featured causes
        if (selected.Count < count)
        {
            var nonFeatured = allCauses.Where(c => !c.IsFeatured).ToList();
            var shuffledNonFeatured = nonFeatured.OrderBy(_ => random.Next()).ToList();
            selected.AddRange(shuffledNonFeatured.Take(count - selected.Count));
        }

        // Shuffle the final selection so featured aren't always first
        return selected.OrderBy(_ => random.Next()).ToList();
    }

    /// <summary>
    /// Gets fallback statistics when there aren't enough causes
    /// </summary>
    private async Task<List<AboutStatistic>> GetFallbackStatisticsAsync()
    {
        var fallbacks = new List<AboutStatistic>();

        // Count active members
        var memberCount = await _context.Users
            .CountAsync(u => u.UserRole == UserRole.Member && u.IsActive);
        if (memberCount > 0)
        {
            fallbacks.Add(new AboutStatistic
            {
                DisplayValue = FormatCount(memberCount),
                Label = "Active Members",
                Percentage = Math.Min(memberCount * 10, 100), // Scale for visual appeal
                IsPercentage = false
            });
        }

        // Count active clients
        var clientCount = await _context.Users
            .CountAsync(u => u.UserRole == UserRole.Client && u.IsActive);
        if (clientCount > 0)
        {
            fallbacks.Add(new AboutStatistic
            {
                DisplayValue = FormatCount(clientCount),
                Label = "Clients Served",
                Percentage = Math.Min(clientCount * 10, 100),
                IsPercentage = false
            });
        }

        // Count active sponsors
        var sponsorCount = await _context.Users
            .CountAsync(u => u.UserRole == UserRole.Sponsor && u.IsActive);
        if (sponsorCount > 0)
        {
            fallbacks.Add(new AboutStatistic
            {
                DisplayValue = FormatCount(sponsorCount),
                Label = "Sponsors",
                Percentage = Math.Min(sponsorCount * 15, 100),
                IsPercentage = false
            });
        }

        // Count published events
        var eventCount = await _context.Events
            .CountAsync(e => e.IsPublished);
        if (eventCount > 0)
        {
            fallbacks.Add(new AboutStatistic
            {
                DisplayValue = FormatCount(eventCount),
                Label = "Events Hosted",
                Percentage = Math.Min(eventCount * 10, 100),
                IsPercentage = false
            });
        }

        // Count board members
        var boardCount = await _context.Users
            .CountAsync(u => u.UserRole == UserRole.BoardMember && u.IsActive);
        if (boardCount > 0)
        {
            fallbacks.Add(new AboutStatistic
            {
                DisplayValue = FormatCount(boardCount),
                Label = "Board Members",
                Percentage = Math.Min(boardCount * 15, 100),
                IsPercentage = false
            });
        }

        return fallbacks;
    }

    /// <summary>
    /// Formats a count for display (e.g., 1500 -> "1.5k")
    /// </summary>
    private static string FormatCount(int count)
    {
        if (count >= 1000)
            return $"{count / 1000.0:0.#}k";
        return count.ToString();
    }
}
