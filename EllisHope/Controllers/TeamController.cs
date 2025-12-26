using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class TeamController : Controller
{
    private readonly ApplicationDbContext _context;

    public TeamController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Team - List all board members
    /// <summary>
    /// Displays a list of all active board members ordered by last name
    /// </summary>
    [SwaggerOperation(Summary = "Displays a list of all active board members ordered by last name")]
    public async Task<IActionResult> Index()
    {
        var boardMembers = await _context.Users
            .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        return View(boardMembers);
    }

    // GET: Team/Details/{id} - Individual board member details
    /// <summary>
    /// Displays detailed information for a specific team member
    /// </summary>
    /// <param name="id">The user ID of the team member</param>
    [SwaggerOperation(Summary = "Displays detailed information for a specific team member")]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var boardMember = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserRole == UserRole.BoardMember && u.IsActive);

        if (boardMember == null)
        {
            return NotFound();
        }

        return View(boardMember);
    }
}
