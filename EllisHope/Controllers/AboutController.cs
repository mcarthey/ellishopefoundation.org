using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

/// <summary>
/// About page and board member listing.
/// </summary>
[ApiExplorerSettings(GroupName = "Public")]
[SwaggerTag("About page and board member listing.")]
public class AboutController : Controller
{
    private readonly ApplicationDbContext _context;

    public AboutController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Show the About page with board members.
    /// </summary>
    [HttpGet]
    /// <summary>
    /// about page (alias: `/About/Index`).
    /// </summary>
    [SwaggerOperation(Summary = "about page (alias: `/About/Index`).")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Index()
    {
        var boardMembers = await _context.Users
            .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        return View(boardMembers);
    }
}
