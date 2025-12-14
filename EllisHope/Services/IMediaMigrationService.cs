using EllisHope.Models.Domain;

namespace EllisHope.Services;

public interface IMediaMigrationService
{
    /// <summary>
    /// Scans all legacy upload directories and returns statistics
    /// </summary>
    Task<MediaMigrationReport> AnalyzeLegacyImagesAsync();

    /// <summary>
    /// Migrates all legacy images to centralized Media Library
    /// </summary>
    Task<MediaMigrationResult> MigrateLegacyImagesAsync(bool updateDatabaseReferences = true, bool deleteOldFiles = false);

    /// <summary>
    /// Migrates images from a specific legacy directory
    /// </summary>
    Task<MediaMigrationResult> MigrateDirectoryAsync(string legacyDirectory, MediaCategory category, bool updateReferences = true);

    /// <summary>
    /// Updates database references from old paths to new paths
    /// </summary>
    Task<int> UpdateDatabaseReferencesAsync(Dictionary<string, string> pathMappings);

    /// <summary>
    /// Validates all media references in the database
    /// </summary>
    Task<List<BrokenMediaReference>> FindBrokenReferencesAsync();

    /// <summary>
    /// Removes duplicate images based on file hash
    /// </summary>
    Task<int> RemoveDuplicateImagesAsync();
}

public class MediaMigrationReport
{
    public int TotalLegacyImages { get; set; }
    public int BlogImages { get; set; }
    public int EventImages { get; set; }
    public int CauseImages { get; set; }
    public int UnsplashImages { get; set; }
    public long TotalSizeBytes { get; set; }
    public int DatabaseReferencesFound { get; set; }
    public int BrokenReferences { get; set; }
    public List<string> Directories { get; set; } = new();
}

public class MediaMigrationResult
{
    public int TotalProcessed { get; set; }
    public int SuccessfullyMigrated { get; set; }
    public int AlreadyInMediaLibrary { get; set; }
    public int Failed { get; set; }
    public int DatabaseReferencesUpdated { get; set; }
    public int FilesDeleted { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> PathMappings { get; set; } = new();
}

public class BrokenMediaReference
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string EntityTitle { get; set; } = string.Empty;
    public string OldPath { get; set; } = string.Empty;
    public bool FileExists { get; set; }
}
