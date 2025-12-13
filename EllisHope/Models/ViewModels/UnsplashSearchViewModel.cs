using System.ComponentModel.DataAnnotations;
using EllisHope.Models.Domain;

namespace EllisHope.Models.ViewModels;

public class UnsplashSearchViewModel
{
    public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 30;
    public UnsplashSearchResult? Results { get; set; }
    public MediaCategory DefaultCategory { get; set; } = MediaCategory.Uncategorized;
}

public class ImportUnsplashPhotoViewModel
{
    [Required]
    public string UnsplashPhotoId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AltText { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public MediaCategory Category { get; set; } = MediaCategory.Uncategorized;
}
