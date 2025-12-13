using EllisHope.Models.Domain;

namespace EllisHope.Models.ViewModels;

/// <summary>
/// Reusable ViewModel for the image picker component
/// Can be embedded in Blog, Event, and Page management forms
/// </summary>
public class MediaPickerViewModel
{
    public string InputName { get; set; } = "SelectedMediaId";
    public int? SelectedMediaId { get; set; }
    public string? SelectedMediaUrl { get; set; }
    public MediaCategory? FilterCategory { get; set; }
    public bool AllowUpload { get; set; } = true;
    public bool AllowUnsplash { get; set; } = true;
    public string? ModalId { get; set; }
}
