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

public class EditableImage
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
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
