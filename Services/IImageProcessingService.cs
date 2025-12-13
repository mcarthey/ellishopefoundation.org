namespace EllisHope.Services;

public interface IImageProcessingService
{
    /// <summary>
    /// Resizes an image to the specified dimensions
    /// </summary>
    Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height, bool maintainAspectRatio = true);

    /// <summary>
    /// Generates multiple thumbnail sizes from an image
    /// </summary>
    Task<Dictionary<string, string>> GenerateThumbnailsAsync(string sourceFilePath, int mediaId,
        IEnumerable<(string name, int width, int height)> sizes);

    /// <summary>
    /// Optimizes an image for web use (compression, format conversion)
    /// </summary>
    Task<byte[]> OptimizeForWebAsync(byte[] imageData, int quality = 85);

    /// <summary>
    /// Gets image dimensions without loading the entire image
    /// </summary>
    Task<(int width, int height)> GetImageDimensionsAsync(byte[] imageData);

    /// <summary>
    /// Validates that a file is a valid image
    /// </summary>
    bool IsValidImage(byte[] imageData);

    /// <summary>
    /// Gets the MIME type of an image
    /// </summary>
    string GetImageMimeType(byte[] imageData);
}
