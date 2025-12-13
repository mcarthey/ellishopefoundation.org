using EllisHope.Models.Domain;

namespace EllisHope.Services;

public interface IPageService
{
    // Page CRUD
    Task<IEnumerable<Page>> GetAllPagesAsync();
    Task<Page?> GetPageByIdAsync(int id);
    Task<Page?> GetPageByNameAsync(string pageName);
    Task<Page> CreatePageAsync(Page page);
    Task<Page> UpdatePageAsync(Page page);
    Task DeletePageAsync(int id);
    
    // Content Sections
    Task<ContentSection?> GetContentSectionAsync(int pageId, string sectionKey);
    Task UpdateContentSectionAsync(int pageId, string sectionKey, string content, string contentType = "RichText");
    Task<IEnumerable<ContentSection>> GetPageContentSectionsAsync(int pageId);
    
    // Page Images
    Task<PageImage?> GetPageImageAsync(int pageId, string imageKey);
    Task SetPageImageAsync(int pageId, string imageKey, int mediaId, int displayOrder = 0);
    Task RemovePageImageAsync(int pageId, string imageKey);
    Task<IEnumerable<PageImage>> GetPageImagesAsync(int pageId);
    
    // Helper methods
    Task<bool> PageExistsAsync(string pageName);
    Task InitializeDefaultPagesAsync();
}
