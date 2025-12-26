namespace EllisHope.Areas.Admin.Models;

/// <summary>
/// ViewModel for the Template Images Gallery
/// </summary>
public class TemplateImagesViewModel
{
    public List<TemplateImageGroup> PageGroups { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Groups template images by page
/// </summary>
public class TemplateImageGroup
{
    public string PageName { get; set; } = string.Empty;
    public string PageDisplayName { get; set; } = string.Empty;
    public List<TemplateImageSlot> ImageSlots { get; set; } = new();
}

/// <summary>
/// Represents a single template image slot
/// </summary>
public class TemplateImageSlot
{
    public string PageName { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CurrentPath { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public int? RecommendedWidth { get; set; }
    public int? RecommendedHeight { get; set; }
    public string? AspectRatio { get; set; }
    public bool FileExists { get; set; }
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Returns a display-friendly file size
    /// </summary>
    public string FileSizeDisplay => FileSizeBytes.HasValue
        ? FileSizeBytes.Value > 1024 * 1024
            ? $"{FileSizeBytes.Value / 1024 / 1024:F1} MB"
            : $"{FileSizeBytes.Value / 1024:F0} KB"
        : "N/A";

    /// <summary>
    /// Returns a display-friendly recommended size
    /// </summary>
    public string RecommendedSizeDisplay => RecommendedWidth.HasValue && RecommendedHeight.HasValue
        ? $"{RecommendedWidth} × {RecommendedHeight}"
        : "Not specified";
}
