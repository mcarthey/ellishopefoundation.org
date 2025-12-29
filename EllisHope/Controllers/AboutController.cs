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

        var viewModel = new AboutPageViewModel
        {
            BoardMembers = boardMembers,
            SponsorsWithQuotes = sponsorsWithQuotes,
            AllSponsors = allSponsors
        };

        return View(viewModel);
    }
}
