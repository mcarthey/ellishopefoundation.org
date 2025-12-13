using System.ComponentModel.DataAnnotations;
using EllisHope.Models.Domain;

namespace EllisHope.Models.ViewModels;

public class MediaEditViewModel
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public MediaSource Source { get; set; }

    [MaxLength(500)]
    public string? AltText { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Caption { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public MediaCategory Category { get; set; }

    // For Unsplash images
    public string? UnsplashPhotographer { get; set; }
    public string? UnsplashPhotographerUsername { get; set; }

    // Metadata
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; }

    // Usage information
    public IEnumerable<MediaUsage> Usages { get; set; } = new List<MediaUsage>();
    public bool IsInUse { get; set; }

    public static MediaEditViewModel FromMedia(Media media)
    {
        return new MediaEditViewModel
        {
            Id = media.Id,
            FileName = media.FileName,
            FilePath = media.FilePath,
            Source = media.Source,
            AltText = media.AltText,
            Title = media.Title,
            Caption = media.Caption,
            Tags = media.Tags,
            Category = media.Category,
            UnsplashPhotographer = media.UnsplashPhotographer,
            UnsplashPhotographerUsername = media.UnsplashPhotographerUsername,
            Width = media.Width,
            Height = media.Height,
            FileSize = media.FileSize,
            UploadedDate = media.UploadedDate,
            Usages = media.Usages,
            IsInUse = media.Usages.Any()
        };
    }

    public void ApplyTo(Media media)
    {
        media.AltText = AltText;
        media.Title = Title;
        media.Caption = Caption;
        media.Tags = Tags;
        media.Category = Category;
    }
}
