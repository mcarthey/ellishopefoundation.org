namespace EllisHope.Models.Domain;

/// <summary>
/// Defines the editable content for each page type
/// </summary>
public class PageTemplate
{
    public string PageName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<EditableImage> Images { get; set; } = new();
    public List<EditableContent> ContentAreas { get; set; } = new();
}

public class ImageRequirements
{
    public int RecommendedWidth { get; set; }
    public int RecommendedHeight { get; set; }
    public int? MinWidth { get; set; }
    public int? MinHeight { get; set; }
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
    public string AspectRatio { get; set; } = string.Empty; // e.g., "16:9", "3:1", "1:1"
    public string Orientation { get; set; } = "Any"; // Landscape, Portrait, Square, Any
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB default
    
    public string DisplayText => $"{RecommendedWidth}×{RecommendedHeight}px";
    public string AspectRatioDecimal => AspectRatio switch
    {
        "16:9" => "1.78",
        "3:1" => "3.00",
        "4:3" => "1.33",
        "1:1" => "1.00",
        "2:3" => "0.67",
        _ => "flexible"
    };
}

public class EditableImage
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Size requirements
    public ImageRequirements Requirements { get; set; } = new()
    {
        RecommendedWidth = 1200,
        RecommendedHeight = 630,
        AspectRatio = "16:9"
    };
    
    // Current database-managed image (from Media Library)
    public string? CurrentImagePath { get; set; }
    public int? CurrentMediaId { get; set; }
    
    // Current template image (hardcoded in /assets/)
    public string? CurrentTemplatePath { get; set; }
    
    // Fallback if no image is set
    public string? FallbackPath { get; set; }
    
    // Helper to get the effective image path
    public string EffectiveImagePath => CurrentImagePath ?? CurrentTemplatePath ?? FallbackPath ?? "/assets/img/default.jpg";
    
    // Helper to determine image source
    public string ImageSource => CurrentImagePath != null ? "Media Library" : 
                                 CurrentTemplatePath != null ? "Template (not managed)" : 
                                 "No image";
    
    public bool IsManagedImage => CurrentMediaId.HasValue;
    
    // Helper for user guidance
    public string SizeGuidance => Requirements.Orientation switch
    {
        "Landscape" => $"Use wide/landscape images ({Requirements.DisplayText}, aspect ratio {Requirements.AspectRatio})",
        "Portrait" => $"Use tall/portrait images ({Requirements.DisplayText}, aspect ratio {Requirements.AspectRatio})",
        "Square" => $"Use square images ({Requirements.DisplayText})",
        _ => $"Recommended size: {Requirements.DisplayText}"
    };
}

public class EditableContent
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContentType { get; set; } = "RichText"; // RichText, Text, HTML
    
    // Current database-managed content
    public string? CurrentContent { get; set; }
    
    // Current template content (hardcoded in views)
    public string? CurrentTemplateValue { get; set; }
    
    public int MaxLength { get; set; } = 0; // 0 = unlimited
    
    // Helper to get effective content
    public string EffectiveContent => CurrentContent ?? CurrentTemplateValue ?? string.Empty;
    
    // Helper to determine content source
    public string ContentSource => CurrentContent != null ? "Managed" : 
                                    CurrentTemplateValue != null ? "Template (not managed)" : 
                                    "No content";
    
    public bool IsManagedContent => CurrentContent != null;
}
