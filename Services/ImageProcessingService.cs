using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace EllisHope.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(IWebHostEnvironment environment, ILogger<ImageProcessingService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height, bool maintainAspectRatio = true)
    {
        try
        {
            using var image = Image.Load(imageData);

            var resizeOptions = new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = maintainAspectRatio ? ResizeMode.Max : ResizeMode.Stretch
            };

            image.Mutate(x => x.Resize(resizeOptions));

            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, GetEncoder(image));
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image to {Width}x{Height}", width, height);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GenerateThumbnailsAsync(string sourceFilePath, int mediaId,
        IEnumerable<(string name, int width, int height)> sizes)
    {
        var thumbnails = new Dictionary<string, string>();
        var sourcePath = Path.Combine(_environment.WebRootPath, sourceFilePath.TrimStart('/'));

        if (!File.Exists(sourcePath))
        {
            _logger.LogWarning("Source file not found: {Path}", sourcePath);
            return thumbnails;
        }

        try
        {
            using var image = await Image.LoadAsync(sourcePath);
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "media", "thumbnails");
            Directory.CreateDirectory(uploadsDir);

            foreach (var (name, width, height) in sizes)
            {
                var thumbnail = image.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Crop
                }));

                var fileName = $"{mediaId}_{name}_{width}x{height}.jpg";
                var filePath = Path.Combine(uploadsDir, fileName);

                await thumbnail.SaveAsync(filePath, new JpegEncoder { Quality = 85 });

                // Store as relative web path
                thumbnails[name] = $"/uploads/media/thumbnails/{fileName}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnails for {Path}", sourcePath);
        }

        return thumbnails;
    }

    public async Task<byte[]> OptimizeForWebAsync(byte[] imageData, int quality = 85)
    {
        try
        {
            using var image = Image.Load(imageData);
            using var outputStream = new MemoryStream();

            // Convert to WebP for better compression, fallback to JPEG
            var encoder = new WebpEncoder { Quality = quality };
            await image.SaveAsync(outputStream, encoder);

            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing image for web");
            throw;
        }
    }

    public async Task<(int width, int height)> GetImageDimensionsAsync(byte[] imageData)
    {
        try
        {
            using var image = await Image.LoadAsync(new MemoryStream(imageData));
            return (image.Width, image.Height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image dimensions");
            throw;
        }
    }

    public bool IsValidImage(byte[] imageData)
    {
        try
        {
            using var image = Image.Load(imageData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetImageMimeType(byte[] imageData)
    {
        try
        {
            var format = Image.DetectFormat(imageData);
            return format.DefaultMimeType;
        }
        catch
        {
            return "application/octet-stream";
        }
    }

    private IImageEncoder GetEncoder(Image image)
    {
        var format = image.Metadata.DecodedImageFormat;

        if (format?.Name == "PNG")
            return new PngEncoder();

        if (format?.Name == "WEBP")
            return new WebpEncoder { Quality = 85 };

        // Default to JPEG
        return new JpegEncoder { Quality = 85 };
    }
}
