using EllisHope.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace EllisHope.Areas.Admin.Models;

public class PageListViewModel
{
    public IEnumerable<Page> Pages { get; set; } = new List<Page>();
    public string? SearchTerm { get; set; }
}

public class PageEditViewModel
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Display(Name = "Page Name")]
    public string PageName { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [MaxLength(500)]
    [Display(Name = "Meta Description")]
    public string? MetaDescription { get; set; }
    
    [Display(Name = "Published")]
    public bool IsPublished { get; set; }
    
    // Content Sections
    public List<ContentSectionViewModel> ContentSections { get; set; } = new();
    
    // Page Images
    public List<PageImageViewModel> PageImages { get; set; } = new();
}

public class ContentSectionViewModel
{
    public int Id { get; set; }
    public int PageId { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Display(Name = "Section Key")]
    public string SectionKey { get; set; } = string.Empty;
    
    public string? Content { get; set; }
    
    [Display(Name = "Content Type")]
    public string ContentType { get; set; } = "RichText";
    
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }
    
    public static ContentSectionViewModel FromContentSection(ContentSection section)
    {
        return new ContentSectionViewModel
        {
            Id = section.Id,
            PageId = section.PageId,
            SectionKey = section.SectionKey,
            Content = section.Content,
            ContentType = section.ContentType,
            DisplayOrder = section.DisplayOrder
        };
    }
}

public class PageImageViewModel
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public int MediaId { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Display(Name = "Image Key")]
    public string ImageKey { get; set; } = string.Empty;
    
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }
    
    // Additional properties for display
    public string? MediaFileName { get; set; }
    public string? MediaFilePath { get; set; }
    public string? MediaAltText { get; set; }
    
    public static PageImageViewModel FromPageImage(PageImage pageImage)
    {
        return new PageImageViewModel
        {
            Id = pageImage.Id,
            PageId = pageImage.PageId,
            MediaId = pageImage.MediaId,
            ImageKey = pageImage.ImageKey,
            DisplayOrder = pageImage.DisplayOrder,
            MediaFileName = pageImage.Media?.FileName,
            MediaFilePath = pageImage.Media?.FilePath,
            MediaAltText = pageImage.Media?.AltText
        };
    }
}

public class QuickEditSectionViewModel
{
    public int PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    
    [Required]
    public string SectionKey { get; set; } = string.Empty;
    
    public string? Content { get; set; }
    
    public string ContentType { get; set; } = "RichText";
}

public class QuickEditImageViewModel
{
    public int PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    
    [Required]
    public string ImageKey { get; set; } = string.Empty;
    
    public int? CurrentMediaId { get; set; }
    public string? CurrentMediaPath { get; set; }
    
    [Required]
    public int MediaId { get; set; }
    
    public int DisplayOrder { get; set; }
}
