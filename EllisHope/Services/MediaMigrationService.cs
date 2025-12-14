using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace EllisHope.Services;

public class MediaMigrationService : IMediaMigrationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMediaService _mediaService;
    private readonly IImageProcessingService _imageProcessing;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MediaMigrationService> _logger;

    public MediaMigrationService(
        ApplicationDbContext context,
        IMediaService mediaService,
        IImageProcessingService imageProcessing,
        IWebHostEnvironment environment,
        ILogger<MediaMigrationService> logger)
    {
        _context = context;
        _mediaService = mediaService;
        _imageProcessing = imageProcessing;
        _environment = environment;
        _logger = logger;
    }

    public async Task<MediaMigrationReport> AnalyzeLegacyImagesAsync()
    {
        var report = new MediaMigrationReport();
        var wwwroot = _environment.WebRootPath;

        // Legacy directories to scan
        var legacyDirs = new[]
        {
            ("uploads/blog", "Blog"),
            ("uploads/events", "Events"),
            ("uploads/causes", "Causes"),
            ("Unsplash", "Unsplash")
        };

        foreach (var (dir, label) in legacyDirs)
        {
            var fullPath = Path.Combine(wwwroot, dir);
            if (Directory.Exists(fullPath))
            {
                var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => IsImageFile(f))
                    .ToList();

                var size = files.Sum(f => new FileInfo(f).Length);

                report.Directories.Add($"{label}: {files.Count} files ({FormatBytes(size)})");
                
                switch (label)
                {
                    case "Blog": report.BlogImages = files.Count; break;
                    case "Events": report.EventImages = files.Count; break;
                    case "Causes": report.CauseImages = files.Count; break;
                    case "Unsplash": report.UnsplashImages = files.Count; break;
                }

                report.TotalLegacyImages += files.Count;
                report.TotalSizeBytes += size;
            }
        }

        // Count database references
        var blogPosts = await _context.BlogPosts.Where(b => !string.IsNullOrEmpty(b.FeaturedImageUrl)).ToListAsync();
        var events = await _context.Events.Where(e => !string.IsNullOrEmpty(e.FeaturedImageUrl)).ToListAsync();
        var causes = await _context.Causes.Where(c => !string.IsNullOrEmpty(c.FeaturedImageUrl)).ToListAsync();

        report.DatabaseReferencesFound = blogPosts.Count + events.Count + causes.Count;

        // Find broken references
        var brokenRefs = await FindBrokenReferencesAsync();
        report.BrokenReferences = brokenRefs.Count;

        return report;
    }

    public async Task<MediaMigrationResult> MigrateLegacyImagesAsync(bool updateDatabaseReferences = true, bool deleteOldFiles = false)
    {
        var result = new MediaMigrationResult();

        try
        {
            // Migrate each legacy directory
            var blogResult = await MigrateDirectoryAsync("uploads/blog", MediaCategory.Blog, false);
            var eventResult = await MigrateDirectoryAsync("uploads/events", MediaCategory.Event, false);
            var causeResult = await MigrateDirectoryAsync("uploads/causes", MediaCategory.Cause, false);
            var unsplashResult = await MigrateUnsplashDirectoryAsync();

            // Combine results
            result.TotalProcessed = blogResult.TotalProcessed + eventResult.TotalProcessed + 
                                   causeResult.TotalProcessed + unsplashResult.TotalProcessed;
            result.SuccessfullyMigrated = blogResult.SuccessfullyMigrated + eventResult.SuccessfullyMigrated + 
                                         causeResult.SuccessfullyMigrated + unsplashResult.SuccessfullyMigrated;
            result.AlreadyInMediaLibrary = blogResult.AlreadyInMediaLibrary + eventResult.AlreadyInMediaLibrary + 
                                          causeResult.AlreadyInMediaLibrary + unsplashResult.AlreadyInMediaLibrary;
            result.Failed = blogResult.Failed + eventResult.Failed + causeResult.Failed + unsplashResult.Failed;

            // Merge path mappings
            foreach (var mapping in blogResult.PathMappings.Concat(eventResult.PathMappings)
                .Concat(causeResult.PathMappings).Concat(unsplashResult.PathMappings))
            {
                result.PathMappings[mapping.Key] = mapping.Value;
            }

            result.Errors.AddRange(blogResult.Errors);
            result.Errors.AddRange(eventResult.Errors);
            result.Errors.AddRange(causeResult.Errors);
            result.Errors.AddRange(unsplashResult.Errors);

            // Update database references
            if (updateDatabaseReferences && result.PathMappings.Any())
            {
                result.DatabaseReferencesUpdated = await UpdateDatabaseReferencesAsync(result.PathMappings);
            }

            // Delete old files if requested
            if (deleteOldFiles && result.SuccessfullyMigrated > 0)
            {
                result.FilesDeleted = await DeleteMigratedFilesAsync(result.PathMappings.Keys);
            }

            _logger.LogInformation("Migration completed: {Migrated} migrated, {Failed} failed, {DbUpdated} DB references updated",
                result.SuccessfullyMigrated, result.Failed, result.DatabaseReferencesUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during legacy image migration");
            result.Errors.Add($"Migration failed: {ex.Message}");
        }

        return result;
    }

    public async Task<MediaMigrationResult> MigrateDirectoryAsync(string legacyDirectory, MediaCategory category, bool updateReferences = true)
    {
        var result = new MediaMigrationResult();
        var wwwroot = _environment.WebRootPath;
        var fullPath = Path.Combine(wwwroot, legacyDirectory);

        if (!Directory.Exists(fullPath))
        {
            _logger.LogWarning("Directory does not exist: {Path}", fullPath);
            return result;
        }

        var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
            .Where(f => IsImageFile(f))
            .ToList();

        result.TotalProcessed = files.Count;

        foreach (var oldFilePath in files)
        {
            try
            {
                var oldRelativePath = "/" + Path.GetRelativePath(wwwroot, oldFilePath).Replace("\\", "/");

                // Check if already in Media Library
                var existing = await _context.MediaLibrary
                    .FirstOrDefaultAsync(m => m.FilePath == oldRelativePath);

                if (existing != null)
                {
                    result.AlreadyInMediaLibrary++;
                    result.PathMappings[oldRelativePath] = existing.FilePath;
                    continue;
                }

                // Read file
                var imageData = await File.ReadAllBytesAsync(oldFilePath);

                // Check for duplicate by hash
                var fileHash = ComputeFileHash(imageData);
                var duplicate = await _context.MediaLibrary
                    .FirstOrDefaultAsync(m => m.FileHash == fileHash);

                if (duplicate != null)
                {
                    _logger.LogInformation("Duplicate found for {OldPath}, using existing {NewPath}", 
                        oldRelativePath, duplicate.FilePath);
                    result.AlreadyInMediaLibrary++;
                    result.PathMappings[oldRelativePath] = duplicate.FilePath;
                    continue;
                }

                // Get image info
                var (width, height) = await _imageProcessing.GetImageDimensionsAsync(imageData);
                var mimeType = _imageProcessing.GetImageMimeType(imageData);

                // Create new media entry
                var fileName = Path.GetFileName(oldFilePath);
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var newFileName = $"{Guid.NewGuid()}{extension}";
                var mediaDir = Path.Combine(wwwroot, "uploads", "media");
                Directory.CreateDirectory(mediaDir);

                var newFilePath = Path.Combine(mediaDir, newFileName);
                var newRelativePath = $"/uploads/media/{newFileName}";

                // Copy file to new location
                await File.WriteAllBytesAsync(newFilePath, imageData);

                // Create database entry
                var media = new Media
                {
                    FileName = fileName,
                    FilePath = newRelativePath,
                    Source = MediaSource.Local,
                    FileSize = imageData.Length,
                    FileHash = fileHash,
                    MimeType = mimeType,
                    AltText = Path.GetFileNameWithoutExtension(fileName),
                    Category = category,
                    Width = width,
                    Height = height,
                    UploadedDate = File.GetCreationTimeUtc(oldFilePath),
                    UploadedBy = "System Migration"
                };

                _context.MediaLibrary.Add(media);
                await _context.SaveChangesAsync();

                result.PathMappings[oldRelativePath] = newRelativePath;
                result.SuccessfullyMigrated++;

                _logger.LogInformation("Migrated: {OldPath} ? {NewPath}", oldRelativePath, newRelativePath);
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"Failed to migrate {oldFilePath}: {ex.Message}");
                _logger.LogError(ex, "Failed to migrate {FilePath}", oldFilePath);
            }
        }

        return result;
    }

    private async Task<MediaMigrationResult> MigrateUnsplashDirectoryAsync()
    {
        var result = new MediaMigrationResult();
        var wwwroot = _environment.WebRootPath;
        var unsplashPath = Path.Combine(wwwroot, "Unsplash");

        if (!Directory.Exists(unsplashPath))
        {
            return result;
        }

        var files = Directory.GetFiles(unsplashPath, "*.*", SearchOption.AllDirectories)
            .Where(f => IsImageFile(f))
            .ToList();

        result.TotalProcessed = files.Count;

        foreach (var oldFilePath in files)
        {
            try
            {
                var oldRelativePath = "/" + Path.GetRelativePath(wwwroot, oldFilePath).Replace("\\", "/");
                
                // Check if already migrated
                var existing = await _context.MediaLibrary
                    .FirstOrDefaultAsync(m => m.FilePath.Contains(Path.GetFileNameWithoutExtension(oldFilePath)));

                if (existing != null)
                {
                    result.AlreadyInMediaLibrary++;
                    result.PathMappings[oldRelativePath] = existing.FilePath;
                    continue;
                }

                // Read file
                var imageData = await File.ReadAllBytesAsync(oldFilePath);
                var fileHash = ComputeFileHash(imageData);

                // Check for duplicate
                var duplicate = await _context.MediaLibrary
                    .FirstOrDefaultAsync(m => m.FileHash == fileHash);

                if (duplicate != null)
                {
                    result.AlreadyInMediaLibrary++;
                    result.PathMappings[oldRelativePath] = duplicate.FilePath;
                    continue;
                }

                // Copy to new Unsplash directory under media
                var unsplashMediaDir = Path.Combine(wwwroot, "uploads", "media", "unsplash");
                Directory.CreateDirectory(unsplashMediaDir);

                var fileName = Path.GetFileName(oldFilePath);
                var newFilePath = Path.Combine(unsplashMediaDir, fileName);
                var newRelativePath = $"/uploads/media/unsplash/{fileName}";

                await File.WriteAllBytesAsync(newFilePath, imageData);

                // Get image info
                var (width, height) = await _imageProcessing.GetImageDimensionsAsync(imageData);

                // Create media entry
                var media = new Media
                {
                    FileName = fileName,
                    FilePath = newRelativePath,
                    Source = MediaSource.Unsplash,
                    FileSize = imageData.Length,
                    FileHash = fileHash,
                    MimeType = "image/jpeg",
                    AltText = Path.GetFileNameWithoutExtension(fileName),
                    Category = MediaCategory.Uncategorized,
                    Width = width,
                    Height = height,
                    UploadedDate = File.GetCreationTimeUtc(oldFilePath),
                    UploadedBy = "System Migration"
                };

                _context.MediaLibrary.Add(media);
                await _context.SaveChangesAsync();

                result.PathMappings[oldRelativePath] = newRelativePath;
                result.SuccessfullyMigrated++;

                _logger.LogInformation("Migrated Unsplash: {OldPath} ? {NewPath}", oldRelativePath, newRelativePath);
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"Failed to migrate Unsplash image {oldFilePath}: {ex.Message}");
                _logger.LogError(ex, "Failed to migrate Unsplash {FilePath}", oldFilePath);
            }
        }

        return result;
    }

    public async Task<int> UpdateDatabaseReferencesAsync(Dictionary<string, string> pathMappings)
    {
        var updated = 0;

        try
        {
            // Update BlogPosts
            var blogPosts = await _context.BlogPosts
                .Where(b => !string.IsNullOrEmpty(b.FeaturedImageUrl))
                .ToListAsync();

            foreach (var post in blogPosts)
            {
                if (pathMappings.TryGetValue(post.FeaturedImageUrl!, out var newPath))
                {
                    post.FeaturedImageUrl = newPath;
                    updated++;
                }
            }

            // Update Events
            var events = await _context.Events
                .Where(e => !string.IsNullOrEmpty(e.FeaturedImageUrl))
                .ToListAsync();

            foreach (var evt in events)
            {
                if (pathMappings.TryGetValue(evt.FeaturedImageUrl!, out var newPath))
                {
                    evt.FeaturedImageUrl = newPath;
                    updated++;
                }
            }

            // Update Causes
            var causes = await _context.Causes
                .Where(c => !string.IsNullOrEmpty(c.FeaturedImageUrl))
                .ToListAsync();

            foreach (var cause in causes)
            {
                if (pathMappings.TryGetValue(cause.FeaturedImageUrl!, out var newPath))
                {
                    cause.FeaturedImageUrl = newPath;
                    updated++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated {Count} database references", updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating database references");
            throw;
        }

        return updated;
    }

    public async Task<List<BrokenMediaReference>> FindBrokenReferencesAsync()
    {
        var brokenRefs = new List<BrokenMediaReference>();
        var wwwroot = _environment.WebRootPath;

        // Check BlogPosts
        var blogPosts = await _context.BlogPosts
            .Where(b => !string.IsNullOrEmpty(b.FeaturedImageUrl))
            .ToListAsync();

        foreach (var post in blogPosts)
        {
            var fullPath = Path.Combine(wwwroot, post.FeaturedImageUrl!.TrimStart('/'));
            var exists = File.Exists(fullPath);

            if (!exists)
            {
                brokenRefs.Add(new BrokenMediaReference
                {
                    EntityType = "BlogPost",
                    EntityId = post.Id,
                    EntityTitle = post.Title,
                    OldPath = post.FeaturedImageUrl!,
                    FileExists = false
                });
            }
        }

        // Check Events
        var events = await _context.Events
            .Where(e => !string.IsNullOrEmpty(e.FeaturedImageUrl))
            .ToListAsync();

        foreach (var evt in events)
        {
            var fullPath = Path.Combine(wwwroot, evt.FeaturedImageUrl!.TrimStart('/'));
            var exists = File.Exists(fullPath);

            if (!exists)
            {
                brokenRefs.Add(new BrokenMediaReference
                {
                    EntityType = "Event",
                    EntityId = evt.Id,
                    EntityTitle = evt.Title,
                    OldPath = evt.FeaturedImageUrl!,
                    FileExists = false
                });
            }
        }

        // Check Causes
        var causes = await _context.Causes
            .Where(c => !string.IsNullOrEmpty(c.FeaturedImageUrl))
            .ToListAsync();

        foreach (var cause in causes)
        {
            var fullPath = Path.Combine(wwwroot, cause.FeaturedImageUrl!.TrimStart('/'));
            var exists = File.Exists(fullPath);

            if (!exists)
            {
                brokenRefs.Add(new BrokenMediaReference
                {
                    EntityType = "Cause",
                    EntityId = cause.Id,
                    EntityTitle = cause.Title,
                    OldPath = cause.FeaturedImageUrl!,
                    FileExists = false
                });
            }
        }

        return brokenRefs;
    }

    public async Task<int> RemoveDuplicateImagesAsync()
    {
        var removed = 0;
        var wwwroot = _environment.WebRootPath;

        // Group by FileHash to find duplicates
        var allMedia = await _context.MediaLibrary.ToListAsync();
        var duplicateGroups = allMedia
            .Where(m => !string.IsNullOrEmpty(m.FileHash))
            .GroupBy(m => m.FileHash)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in duplicateGroups)
        {
            // Keep the oldest one (first uploaded)
            var toKeep = group.OrderBy(m => m.UploadedDate).First();
            var toRemove = group.Where(m => m.Id != toKeep.Id).ToList();

            foreach (var duplicate in toRemove)
            {
                try
                {
                    // Delete physical file
                    var fullPath = Path.Combine(wwwroot, duplicate.FilePath.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }

                    // Remove from database
                    _context.MediaLibrary.Remove(duplicate);
                    removed++;

                    _logger.LogInformation("Removed duplicate: {Path} (kept {OriginalPath})", 
                        duplicate.FilePath, toKeep.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove duplicate {Path}", duplicate.FilePath);
                }
            }
        }

        await _context.SaveChangesAsync();
        return removed;
    }

    private async Task<int> DeleteMigratedFilesAsync(IEnumerable<string> oldPaths)
    {
        var deleted = 0;
        var wwwroot = _environment.WebRootPath;

        foreach (var oldPath in oldPaths)
        {
            try
            {
                var fullPath = Path.Combine(wwwroot, oldPath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    deleted++;
                    _logger.LogInformation("Deleted migrated file: {Path}", oldPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete {Path}", oldPath);
            }
        }

        return deleted;
    }

    private bool IsImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" or ".bmp";
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private string ComputeFileHash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }
}
