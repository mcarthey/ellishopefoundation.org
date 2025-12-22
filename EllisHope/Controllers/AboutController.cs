using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Controllers;

public class AboutController : Controller
{
    private readonly ApplicationDbContext _context;

    public AboutController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: About
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
