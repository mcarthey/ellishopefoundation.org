using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Http;

namespace EllisHope.Services;

public interface IMediaService
{
    // Get operations
    Task<IEnumerable<Media>> GetAllMediaAsync(MediaCategory? category = null, MediaSource? source = null);
    Task<Media?> GetMediaByIdAsync(int id);
    Task<IEnumerable<Media>> SearchMediaAsync(string searchTerm, MediaCategory? category = null);
    Task<IEnumerable<Media>> GetMediaByTagsAsync(string tags);

    // Upload operations
    Task<Media> UploadLocalImageAsync(IFormFile file, string? altText = null, string? title = null,
        MediaCategory category = MediaCategory.Uncategorized, string? tags = null, string? uploadedBy = null);

    // Unsplash operations
    Task<Media> ImportFromUnsplashAsync(string unsplashPhotoId, string? altText = null,
        MediaCategory category = MediaCategory.Uncategorized, string? tags = null, string? uploadedBy = null);

    // Update operations
    Task<bool> UpdateMediaAsync(Media media);
    Task<bool> UpdateMediaMetadataAsync(int mediaId, string? altText = null, string? title = null,
        string? caption = null, string? tags = null, MediaCategory? category = null);

    // Delete operations
    Task<bool> DeleteMediaAsync(int id, bool force = false);
    Task<bool> CanDeleteMediaAsync(int id);

    // Usage tracking
    Task TrackMediaUsageAsync(int mediaId, string entityType, int entityId, UsageType usageType);
    Task RemoveMediaUsageAsync(int mediaId, string entityType, int entityId);
    Task<IEnumerable<MediaUsage>> GetMediaUsagesAsync(int mediaId);
    Task<bool> IsMediaInUseAsync(int mediaId);

    // Statistics
    Task<int> GetTotalMediaCountAsync();
    Task<long> GetTotalStorageSizeAsync();
    Task<Dictionary<MediaCategory, int>> GetMediaCountByCategoryAsync();
}
