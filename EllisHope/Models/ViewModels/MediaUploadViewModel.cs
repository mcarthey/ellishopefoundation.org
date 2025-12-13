using System.ComponentModel.DataAnnotations;
using EllisHope.Models.Domain;

namespace EllisHope.Models.ViewModels;

public class MediaUploadViewModel
{
    [Required(ErrorMessage = "Please select a file to upload")]
    public IFormFile? File { get; set; }

    [MaxLength(500)]
    public string? AltText { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Caption { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public MediaCategory Category { get; set; } = MediaCategory.Uncategorized;
}
