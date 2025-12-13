using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EllisHope.Services;

public class MediaService : IMediaService
{
    private readonly ApplicationDbContext _context;
    private readonly IImageProcessingService _imageProcessing;
    private readonly IUnsplashService _unsplash;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MediaService> _logger;

    public MediaService(
        ApplicationDbContext context,
        IImageProcessingService imageProcessing,
        IUnsplashService unsplash,
        IWebHostEnvironment environment,
        ILogger<MediaService> logger)
    {
        _context = context;
        _imageProcessing = imageProcessing;
        _unsplash = unsplash;
        _environment = environment;
        _logger = logger;
    }

    #region Get Operations

    public async Task<IEnumerable<Media>> GetAllMediaAsync(MediaCategory? category = null, MediaSource? source = null)
    {
        var query = _context.MediaLibrary.AsQueryable();

        if (category.HasValue)
            query = query.Where(m => m.Category == category.Value);

        if (source.HasValue)
            query = query.Where(m => m.Source == source.Value);

        return await query
            .OrderByDescending(m => m.UploadedDate)
            .ToListAsync();
    }

    public async Task<Media?> GetMediaByIdAsync(int id)
    {
        return await _context.MediaLibrary
            .Include(m => m.Usages)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Media>> SearchMediaAsync(string searchTerm, MediaCategory? category = null)
    {
        var query = _context.MediaLibrary.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(m =>
                m.FileName.ToLower().Contains(searchTerm) ||
                (m.Title != null && m.Title.ToLower().Contains(searchTerm)) ||
                (m.AltText != null && m.AltText.ToLower().Contains(searchTerm)) ||
                (m.Tags != null && m.Tags.ToLower().Contains(searchTerm)));
        }

        if (category.HasValue)
            query = query.Where(m => m.Category == category.Value);

        return await query
            .OrderByDescending(m => m.UploadedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Media>> GetMediaByTagsAsync(string tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return Enumerable.Empty<Media>();

        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLower())
            .ToList();

        // Load all media with tags, then filter in memory (more testable with InMemory provider)
        var allMedia = await _context.MediaLibrary
            .Where(m => m.Tags != null)
            .ToListAsync();
            
        // Filter in memory to avoid InMemory provider limitations
        return allMedia
            .Where(m => tagList.Any(tag => m.Tags!.ToLower().Contains(tag)))
            .OrderByDescending(m => m.UploadedDate)
            .ToList();
    }

    #endregion

    #region Upload Operations

    public async Task<Media> UploadLocalImageAsync(IFormFile file, string? altText = null, string? title = null,
        MediaCategory category = MediaCategory.Uncategorized, string? tags = null, string? uploadedBy = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty", nameof(file));

        // Validate file is an image
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var imageData = memoryStream.ToArray();

        if (!_imageProcessing.IsValidImage(imageData))
            throw new ArgumentException("File is not a valid image", nameof(file));

        // Get image dimensions
        var (width, height) = await _imageProcessing.GetImageDimensionsAsync(imageData);

        // Create uploads directory if it doesn't exist
        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "media");
        Directory.CreateDirectory(uploadsDir);

        // Generate unique filename
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        // Save file
        await File.WriteAllBytesAsync(filePath, imageData);

        // Create media entity
        var media = new Media
        {
            FileName = file.FileName,
            FilePath = $"/uploads/media/{fileName}",
            Source = MediaSource.Local,
            FileSize = file.Length,
            MimeType = _imageProcessing.GetImageMimeType(imageData),
            AltText = altText ?? Path.GetFileNameWithoutExtension(file.FileName),
            Title = title,
            Tags = tags,
            Category = category,
            Width = width,
            Height = height,
            UploadedDate = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };

        _context.MediaLibrary.Add(media);
        await _context.SaveChangesAsync();

        // Generate thumbnails asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                var sizes = await GetDefaultThumbnailSizes(category);
                var thumbnails = await _imageProcessing.GenerateThumbnailsAsync(media.FilePath, media.Id, sizes);

                if (thumbnails.Any())
                {
                    media.Thumbnails = JsonSerializer.Serialize(thumbnails);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnails for media {MediaId}", media.Id);
            }
        });

        return media;
    }

    public async Task<Media> ImportFromUnsplashAsync(string unsplashPhotoId, string? altText = null,
        MediaCategory category = MediaCategory.Uncategorized, string? tags = null, string? uploadedBy = null)
    {
        // Get photo details from Unsplash
        var photo = await _unsplash.GetPhotoAsync(unsplashPhotoId);
        if (photo == null)
            throw new ArgumentException("Photo not found on Unsplash", nameof(unsplashPhotoId));

        // Download the image
        var imageData = await _unsplash.DownloadPhotoAsync(photo.Urls.Regular);

        // Create uploads directory
        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "media", "unsplash");
        Directory.CreateDirectory(uploadsDir);

        // Save file
        var fileName = $"{unsplashPhotoId}.jpg";
        var filePath = Path.Combine(uploadsDir, fileName);
        await File.WriteAllBytesAsync(filePath, imageData);

        // Trigger download tracking on Unsplash
        await _unsplash.TriggerDownloadAsync(unsplashPhotoId);

        // Create media entity
        var media = new Media
        {
            FileName = fileName,
            FilePath = $"/uploads/media/unsplash/{fileName}",
            Source = MediaSource.Unsplash,
            UnsplashId = unsplashPhotoId,
            UnsplashPhotographer = photo.User.Name,
            UnsplashPhotographerUsername = photo.User.Username,
            FileSize = imageData.Length,
            MimeType = "image/jpeg",
            AltText = altText ?? photo.Description ?? $"Photo by {photo.User.Name}",
            Title = photo.Description,
            Tags = tags,
            Category = category,
            Width = photo.Width,
            Height = photo.Height,
            UploadedDate = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };

        _context.MediaLibrary.Add(media);
        await _context.SaveChangesAsync();

        // Generate thumbnails asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                var sizes = await GetDefaultThumbnailSizes(category);
                var thumbnails = await _imageProcessing.GenerateThumbnailsAsync(media.FilePath, media.Id, sizes);

                if (thumbnails.Any())
                {
                    media.Thumbnails = JsonSerializer.Serialize(thumbnails);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnails for Unsplash media {MediaId}", media.Id);
            }
        });

        return media;
    }

    #endregion

    #region Update Operations

    public async Task<bool> UpdateMediaAsync(Media media)
    {
        try
        {
            _context.MediaLibrary.Update(media);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media {MediaId}", media.Id);
            return false;
        }
    }

    public async Task<bool> UpdateMediaMetadataAsync(int mediaId, string? altText = null, string? title = null,
        string? caption = null, string? tags = null, MediaCategory? category = null)
    {
        var media = await GetMediaByIdAsync(mediaId);
        if (media == null)
            return false;

        if (altText != null) media.AltText = altText;
        if (title != null) media.Title = title;
        if (caption != null) media.Caption = caption;
        if (tags != null) media.Tags = tags;
        if (category.HasValue) media.Category = category.Value;

        return await UpdateMediaAsync(media);
    }

    #endregion

    #region Delete Operations

    public async Task<bool> DeleteMediaAsync(int id, bool force = false)
    {
        var media = await GetMediaByIdAsync(id);
        if (media == null)
            return false;

        // Check if media is in use
        if (!force && await IsMediaInUseAsync(id))
        {
            _logger.LogWarning("Cannot delete media {MediaId} - it is currently in use", id);
            return false;
        }

        try
        {
            // Delete physical file
            var fullPath = Path.Combine(_environment.WebRootPath, media.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // Delete thumbnails
            if (!string.IsNullOrEmpty(media.Thumbnails))
            {
                var thumbnails = JsonSerializer.Deserialize<Dictionary<string, string>>(media.Thumbnails);
                if (thumbnails != null)
                {
                    foreach (var thumbPath in thumbnails.Values)
                    {
                        var thumbFullPath = Path.Combine(_environment.WebRootPath, thumbPath.TrimStart('/'));
                        if (File.Exists(thumbFullPath))
                        {
                            File.Delete(thumbFullPath);
                        }
                    }
                }
            }

            // Delete from database
            _context.MediaLibrary.Remove(media);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media {MediaId}", id);
            return false;
        }
    }

    public async Task<bool> CanDeleteMediaAsync(int id)
    {
        return !await IsMediaInUseAsync(id);
    }

    #endregion

    #region Usage Tracking

    public async Task TrackMediaUsageAsync(int mediaId, string entityType, int entityId, UsageType usageType)
    {
        // Check if usage already exists
        var existingUsage = await _context.MediaUsages
            .FirstOrDefaultAsync(mu => mu.MediaId == mediaId &&
                                      mu.EntityType == entityType &&
                                      mu.EntityId == entityId &&
                                      mu.UsageType == usageType);

        if (existingUsage == null)
        {
            var usage = new MediaUsage
            {
                MediaId = mediaId,
                EntityType = entityType,
                EntityId = entityId,
                UsageType = usageType,
                DateAdded = DateTime.UtcNow
            };

            _context.MediaUsages.Add(usage);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveMediaUsageAsync(int mediaId, string entityType, int entityId)
    {
        var usages = await _context.MediaUsages
            .Where(mu => mu.MediaId == mediaId &&
                        mu.EntityType == entityType &&
                        mu.EntityId == entityId)
            .ToListAsync();

        if (usages.Any())
        {
            _context.MediaUsages.RemoveRange(usages);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<MediaUsage>> GetMediaUsagesAsync(int mediaId)
    {
        return await _context.MediaUsages
            .Where(mu => mu.MediaId == mediaId)
            .ToListAsync();
    }

    public async Task<bool> IsMediaInUseAsync(int mediaId)
    {
        return await _context.MediaUsages
            .AnyAsync(mu => mu.MediaId == mediaId);
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalMediaCountAsync()
    {
        return await _context.MediaLibrary.CountAsync();
    }

    public async Task<long> GetTotalStorageSizeAsync()
    {
        return await _context.MediaLibrary.SumAsync(m => m.FileSize);
    }

    public async Task<Dictionary<MediaCategory, int>> GetMediaCountByCategoryAsync()
    {
        return await _context.MediaLibrary
            .GroupBy(m => m.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);
    }

    #endregion

    #region Helper Methods

    private async Task<List<(string name, int width, int height)>> GetDefaultThumbnailSizes(MediaCategory category)
    {
        // Get sizes from ImageSizes table based on category
        var sizes = await _context.ImageSizes
            .Where(s => s.IsActive && s.Category == category)
            .Select(s => new { s.Name, s.Width, s.Height })
            .ToListAsync();

        // Convert to tuple list
        var tupleList = sizes.Select(s => (s.Name, s.Width, s.Height)).ToList();

        // If no specific sizes found, use general sizes
        if (!tupleList.Any())
        {
            tupleList = new List<(string, int, int)>
            {
                ("thumbnail", 150, 150),
                ("small", 300, 300),
                ("medium", 600, 600)
            };
        }

        return tupleList;
    }

    #endregion
}
