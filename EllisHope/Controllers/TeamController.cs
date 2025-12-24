using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Controllers;

public class TeamController : Controller
{
    private readonly ApplicationDbContext _context;

    public TeamController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Team/v1 - List all board members
    /// <summary>
    /// TODO: Describe GET /Team/v1
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Team/v1")]
    public async Task<IActionResult> v1()
    {
        var boardMembers = await _context.Users
            .Where(u => u.UserRole == UserRole.BoardMember && u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        return View(boardMembers);
    }

    // GET: Team/details/{id} - Individual board member details
    /// <summary>
    /// team member details
    /// </summary>
    [SwaggerOperation(Summary = "team member details")]
    public async Task<IActionResult> details(string id)
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

    // Keeping v2 for potential future use
    /// <summary>
    /// TODO: Describe GET /Team/v2
    /// </summary>
    [SwaggerOperation(Summary = "TODO: Describe GET /Team/v2")]
    public IActionResult v2()
    {
        return View();
    }
}
