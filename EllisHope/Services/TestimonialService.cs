using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Services;

public class TestimonialService : ITestimonialService
{
    private readonly ApplicationDbContext _context;

    public TestimonialService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Public Queries

    public async Task<IEnumerable<Testimonial>> GetFeaturedTestimonialsAsync(int count = 5)
    {
        return await _context.Testimonials
            .Include(t => t.AuthorPhoto)
            .Where(t => t.IsPublished && t.IsFeatured && !t.RequiresApproval)
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Testimonial>> GetPublishedTestimonialsAsync()
    {
        return await _context.Testimonials
            .Include(t => t.AuthorPhoto)
            .Where(t => t.IsPublished && !t.RequiresApproval)
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    #endregion

    #region Admin Queries

    public async Task<IEnumerable<Testimonial>> GetAllTestimonialsAsync(bool includeUnpublished = true)
    {
        var query = _context.Testimonials
            .Include(t => t.AuthorPhoto)
            .Include(t => t.CreatedBy)
            .Include(t => t.ApprovedBy)
            .AsQueryable();

        if (!includeUnpublished)
        {
            query = query.Where(t => t.IsPublished);
        }

        return await query
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Testimonial>> GetPendingApprovalAsync()
    {
        return await _context.Testimonials
            .Include(t => t.AuthorPhoto)
            .Include(t => t.CreatedBy)
            .Where(t => t.RequiresApproval)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    public async Task<Testimonial?> GetByIdAsync(int id)
    {
        return await _context.Testimonials
            .Include(t => t.AuthorPhoto)
            .Include(t => t.CreatedBy)
            .Include(t => t.ApprovedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    #endregion

    #region CRUD Operations

    public async Task<Testimonial> CreateAsync(Testimonial testimonial, string userId, bool canAutoApprove)
    {
        testimonial.CreatedById = userId;
        testimonial.CreatedDate = DateTime.UtcNow;

        // If user can auto-approve, content doesn't require approval
        // Otherwise, set RequiresApproval to true
        if (canAutoApprove)
        {
            testimonial.RequiresApproval = false;
        }
        else
        {
            testimonial.RequiresApproval = true;
            testimonial.IsPublished = false; // Ensure unpublished until approved
        }

        _context.Testimonials.Add(testimonial);
        await _context.SaveChangesAsync();

        return testimonial;
    }

    public async Task<Testimonial> UpdateAsync(Testimonial testimonial)
    {
        testimonial.ModifiedDate = DateTime.UtcNow;
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();

        return testimonial;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
        {
            return false;
        }

        _context.Testimonials.Remove(testimonial);
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Actions

    public async Task<bool> PublishAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
        {
            return false;
        }

        testimonial.IsPublished = true;
        testimonial.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnpublishAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
        {
            return false;
        }

        testimonial.IsPublished = false;
        testimonial.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> FeatureAsync(int id, bool featured)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
        {
            return false;
        }

        testimonial.IsFeatured = featured;
        testimonial.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ApproveAsync(int id, string approverId)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
        {
            return false;
        }

        testimonial.RequiresApproval = false;
        testimonial.ApprovedById = approverId;
        testimonial.ApprovedDate = DateTime.UtcNow;
        testimonial.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateDisplayOrderAsync(int id, int newOrder)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
        {
            return false;
        }

        testimonial.DisplayOrder = newOrder;
        testimonial.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion
}
