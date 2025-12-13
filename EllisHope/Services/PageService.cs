using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Services;

public class PageService : IPageService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PageService> _logger;

    public PageService(ApplicationDbContext context, ILogger<PageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Page>> GetAllPagesAsync()
    {
        return await _context.Pages
            .Include(p => p.ContentSections)
            .Include(p => p.PageImages)
                .ThenInclude(pi => pi.Media)
            .OrderBy(p => p.PageName)
            .ToListAsync();
    }

    public async Task<Page?> GetPageByIdAsync(int id)
    {
        return await _context.Pages
            .Include(p => p.ContentSections)
            .Include(p => p.PageImages)
                .ThenInclude(pi => pi.Media)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Page?> GetPageByNameAsync(string pageName)
    {
        return await _context.Pages
            .Include(p => p.ContentSections)
            .Include(p => p.PageImages)
                .ThenInclude(pi => pi.Media)
            .FirstOrDefaultAsync(p => p.PageName == pageName);
    }

    public async Task<Page> CreatePageAsync(Page page)
    {
        page.CreatedDate = DateTime.UtcNow;
        page.ModifiedDate = DateTime.UtcNow;
        
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();
        
        return page;
    }

    public async Task<Page> UpdatePageAsync(Page page)
    {
        page.ModifiedDate = DateTime.UtcNow;
        
        _context.Pages.Update(page);
        await _context.SaveChangesAsync();
        
        return page;
    }

    public async Task DeletePageAsync(int id)
    {
        var page = await _context.Pages
            .Include(p => p.ContentSections)
            .Include(p => p.PageImages)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (page != null)
        {
            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ContentSection?> GetContentSectionAsync(int pageId, string sectionKey)
    {
        return await _context.ContentSections
            .FirstOrDefaultAsync(cs => cs.PageId == pageId && cs.SectionKey == sectionKey);
    }

    public async Task UpdateContentSectionAsync(int pageId, string sectionKey, string content, string contentType = "RichText")
    {
        var section = await GetContentSectionAsync(pageId, sectionKey);
        
        if (section == null)
        {
            // Create new section
            section = new ContentSection
            {
                PageId = pageId,
                SectionKey = sectionKey,
                Content = content,
                ContentType = contentType,
                DisplayOrder = await GetNextDisplayOrderAsync(pageId)
            };
            _context.ContentSections.Add(section);
        }
        else
        {
            // Update existing section
            section.Content = content;
            section.ContentType = contentType;
            _context.ContentSections.Update(section);
        }
        
        await _context.SaveChangesAsync();
        
        // Update page modified date
        var page = await _context.Pages.FindAsync(pageId);
        if (page != null)
        {
            page.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ContentSection>> GetPageContentSectionsAsync(int pageId)
    {
        return await _context.ContentSections
            .Where(cs => cs.PageId == pageId)
            .OrderBy(cs => cs.DisplayOrder)
            .ToListAsync();
    }

    public async Task<PageImage?> GetPageImageAsync(int pageId, string imageKey)
    {
        return await _context.PageImages
            .Include(pi => pi.Media)
            .FirstOrDefaultAsync(pi => pi.PageId == pageId && pi.ImageKey == imageKey);
    }

    public async Task SetPageImageAsync(int pageId, string imageKey, int mediaId, int displayOrder = 0)
    {
        var pageImage = await GetPageImageAsync(pageId, imageKey);
        
        if (pageImage == null)
        {
            // Create new page image
            pageImage = new PageImage
            {
                PageId = pageId,
                MediaId = mediaId,
                ImageKey = imageKey,
                DisplayOrder = displayOrder
            };
            _context.PageImages.Add(pageImage);
        }
        else
        {
            // Update existing page image
            pageImage.MediaId = mediaId;
            pageImage.DisplayOrder = displayOrder;
            _context.PageImages.Update(pageImage);
        }
        
        await _context.SaveChangesAsync();
        
        // Update page modified date
        var page = await _context.Pages.FindAsync(pageId);
        if (page != null)
        {
            page.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemovePageImageAsync(int pageId, string imageKey)
    {
        var pageImage = await GetPageImageAsync(pageId, imageKey);
        
        if (pageImage != null)
        {
            _context.PageImages.Remove(pageImage);
            await _context.SaveChangesAsync();
            
            // Update page modified date
            var page = await _context.Pages.FindAsync(pageId);
            if (page != null)
            {
                page.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<IEnumerable<PageImage>> GetPageImagesAsync(int pageId)
    {
        return await _context.PageImages
            .Include(pi => pi.Media)
            .Where(pi => pi.PageId == pageId)
            .OrderBy(pi => pi.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> PageExistsAsync(string pageName)
    {
        return await _context.Pages.AnyAsync(p => p.PageName == pageName);
    }

    public async Task InitializeDefaultPagesAsync()
    {
        var defaultPages = new[]
        {
            "Home",
            "About",
            "Team",
            "Services",
            "Contact"
        };

        foreach (var pageName in defaultPages)
        {
            if (!await PageExistsAsync(pageName))
            {
                var page = new Page
                {
                    PageName = pageName,
                    Title = pageName,
                    IsPublished = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
                
                await CreatePageAsync(page);
                _logger.LogInformation("Created default page: {PageName}", pageName);
            }
        }
    }

    private async Task<int> GetNextDisplayOrderAsync(int pageId)
    {
        var maxOrder = await _context.ContentSections
            .Where(cs => cs.PageId == pageId)
            .MaxAsync(cs => (int?)cs.DisplayOrder);
            
        return (maxOrder ?? 0) + 1;
    }
}
