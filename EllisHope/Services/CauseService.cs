using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace EllisHope.Services;

public class CauseService : ICauseService
{
    private readonly ApplicationDbContext _context;
    private readonly SlugHelper _slugHelper;

    public CauseService(ApplicationDbContext context)
    {
        _context = context;
        _slugHelper = new SlugHelper();
    }

    public async Task<IEnumerable<Cause>> GetAllCausesAsync(bool includeUnpublished = false)
    {
        var query = _context.Causes.OrderByDescending(c => c.CreatedDate);

        if (!includeUnpublished)
        {
            return await query.Where(c => c.IsPublished).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Cause>> GetFeaturedCausesAsync(int count = 4)
    {
        return await _context.Causes
            .Where(c => c.IsPublished && c.IsFeatured)
            .OrderByDescending(c => c.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cause>> GetActiveCausesAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Causes
            .Where(c => c.IsPublished &&
                   (!c.EndDate.HasValue || c.EndDate.Value >= now))
            .OrderByDescending(c => c.IsFeatured)
            .ThenByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<Cause?> GetCauseByIdAsync(int id)
    {
        return await _context.Causes.FindAsync(id);
    }

    public async Task<Cause?> GetCauseBySlugAsync(string slug)
    {
        return await _context.Causes
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsPublished);
    }

    public async Task<IEnumerable<Cause>> SearchCausesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllCausesAsync();
        }

        searchTerm = searchTerm.ToLower();

        return await _context.Causes
            .Where(c => c.IsPublished &&
                   (c.Title.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)) ||
                    (c.ShortDescription != null && c.ShortDescription.ToLower().Contains(searchTerm)) ||
                    (c.Category != null && c.Category.ToLower().Contains(searchTerm))))
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cause>> GetCausesByCategoryAsync(string category)
    {
        return await _context.Causes
            .Where(c => c.IsPublished && c.Category == category)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cause>> GetSimilarCausesAsync(int causeId, int count = 4)
    {
        var currentCause = await GetCauseByIdAsync(causeId);
        if (currentCause == null)
            return Enumerable.Empty<Cause>();

        // Get causes with same category or similar tags
        var similarCauses = await _context.Causes
            .Where(c => c.IsPublished && c.Id != causeId &&
                   (c.Category == currentCause.Category))
            .OrderByDescending(c => c.CreatedDate)
            .Take(count)
            .ToListAsync();

        return similarCauses;
    }

    public async Task<Cause> CreateCauseAsync(Cause cause)
    {
        if (string.IsNullOrWhiteSpace(cause.Slug))
        {
            cause.Slug = GenerateSlug(cause.Title);
        }

        cause.Slug = await EnsureUniqueSlugAsync(cause.Slug);
        cause.CreatedDate = DateTime.UtcNow;
        cause.UpdatedDate = DateTime.UtcNow;

        _context.Causes.Add(cause);
        await _context.SaveChangesAsync();
        return cause;
    }

    public async Task<Cause> UpdateCauseAsync(Cause cause)
    {
        cause.UpdatedDate = DateTime.UtcNow;
        _context.Causes.Update(cause);
        await _context.SaveChangesAsync();
        return cause;
    }

    public async Task DeleteCauseAsync(int id)
    {
        var cause = await _context.Causes.FindAsync(id);
        if (cause != null)
        {
            _context.Causes.Remove(cause);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeCauseId = null)
    {
        var query = _context.Causes.Where(c => c.Slug == slug);

        if (excludeCauseId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCauseId.Value);
        }

        return await query.AnyAsync();
    }

    public string GenerateSlug(string title)
    {
        return _slugHelper.GenerateSlug(title);
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug)
    {
        var slug = baseSlug;
        var counter = 1;

        while (await SlugExistsAsync(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
