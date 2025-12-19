using EllisHope.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace EllisHope.Tests.Services;

public class ImageProcessingServiceTests
{
    private ImageProcessingService CreateService(string? webRootPath = null)
    {
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(e => e.WebRootPath).Returns(webRootPath ?? Path.GetTempPath());

        var mockLogger = new Mock<ILogger<ImageProcessingService>>();

        return new ImageProcessingService(mockEnvironment.Object, mockLogger.Object);
    }

    private byte[] CreateTestImage(int width, int height, bool asPng = false)
    {
        using var image = new Image<Rgba32>(width, height);

        // Fill with a simple pattern
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = new Rgba32((byte)(x % 256), (byte)(y % 256), 128);
                image[x, y] = color;
            }
        }

        using var outputStream = new MemoryStream();
        if (asPng)
        {
            image.Save(outputStream, new PngEncoder());
        }
        else
        {
            image.Save(outputStream, new JpegEncoder());
        }
        return outputStream.ToArray();
    }

    [Fact]
    public async Task ResizeImageAsync_ReducesImageSize()
    {
        // Arrange
        var service = CreateService();
        var originalImage = CreateTestImage(800, 600);

        // Act
        var resizedImage = await service.ResizeImageAsync(originalImage, 400, 300);

        // Assert
        Assert.NotNull(resizedImage);
        Assert.True(resizedImage.Length > 0);
        Assert.True(resizedImage.Length < originalImage.Length); // Resized should be smaller

        // Verify dimensions
        var dimensions = await service.GetImageDimensionsAsync(resizedImage);
        Assert.True(dimensions.width <= 400);
        Assert.True(dimensions.height <= 300);
    }

    [Fact]
    public async Task ResizeImageAsync_MaintainsAspectRatio_WhenRequested()
    {
        // Arrange
        var service = CreateService();
        var originalImage = CreateTestImage(1000, 500); // 2:1 aspect ratio

        // Act
        var resizedImage = await service.ResizeImageAsync(originalImage, 400, 400, maintainAspectRatio: true);

        // Assert
        var dimensions = await service.GetImageDimensionsAsync(resizedImage);

        // With aspect ratio maintained, should be 400x200 (not 400x400)
        Assert.Equal(400, dimensions.width);
        Assert.Equal(200, dimensions.height);
    }

    [Fact]
    public async Task ResizeImageAsync_StretchesImage_WhenAspectRatioNotMaintained()
    {
        // Arrange
        var service = CreateService();
        var originalImage = CreateTestImage(1000, 500);

        // Act
        var resizedImage = await service.ResizeImageAsync(originalImage, 400, 400, maintainAspectRatio: false);

        // Assert
        var dimensions = await service.GetImageDimensionsAsync(resizedImage);

        // Without aspect ratio, should be exactly 400x400
        Assert.Equal(400, dimensions.width);
        Assert.Equal(400, dimensions.height);
    }

    [Fact]
    public async Task ResizeImageAsync_ThrowsException_WhenInvalidImageData()
    {
        // Arrange
        var service = CreateService();
        var invalidData = new byte[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        await Assert.ThrowsAsync<UnknownImageFormatException>(async () =>
            await service.ResizeImageAsync(invalidData, 100, 100)
        );
    }

    [Fact]
    public async Task ResizeImageAsync_HandlesZeroWidth()
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(100, 100);

        // Act & Assert - Should throw or handle gracefully
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.ResizeImageAsync(image, 0, 100)
        );
    }

    [Fact]
    public async Task ResizeImageAsync_HandlesZeroHeight()
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(100, 100);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.ResizeImageAsync(image, 100, 0)
        );
    }

    [Fact]
    public async Task ResizeImageAsync_HandlesVeryLargeImage()
    {
        // Arrange
        var service = CreateService();
        var largeImage = CreateTestImage(4000, 3000);

        // Act
        var resizedImage = await service.ResizeImageAsync(largeImage, 800, 600);

        // Assert
        var dimensions = await service.GetImageDimensionsAsync(resizedImage);
        Assert.True(dimensions.width <= 800);
        Assert.True(dimensions.height <= 600);
    }

    [Fact]
    public async Task ResizeImageAsync_HandlesVerySmallImage()
    {
        // Arrange
        var service = CreateService();
        var smallImage = CreateTestImage(10, 10);

        // Act
        var resizedImage = await service.ResizeImageAsync(smallImage, 100, 100);

        // Assert
        Assert.NotNull(resizedImage);
        var dimensions = await service.GetImageDimensionsAsync(resizedImage);
        Assert.True(dimensions.width > 0);
        Assert.True(dimensions.height > 0);
    }

    [Fact]
    public async Task OptimizeForWebAsync_ProducesValidImage()
    {
        // Arrange
        var service = CreateService();
        var originalImage = CreateTestImage(500, 500);

        // Act
        var optimizedImage = await service.OptimizeForWebAsync(originalImage);

        // Assert
        Assert.NotNull(optimizedImage);
        Assert.True(optimizedImage.Length > 0);
        Assert.True(service.IsValidImage(optimizedImage));
    }

    [Fact]
    public async Task OptimizeForWebAsync_WithCustomQuality()
    {
        // Arrange
        var service = CreateService();
        var originalImage = CreateTestImage(500, 500);

        // Act
        var highQuality = await service.OptimizeForWebAsync(originalImage, quality: 95);
        var lowQuality = await service.OptimizeForWebAsync(originalImage, quality: 50);

        // Assert
        Assert.NotNull(highQuality);
        Assert.NotNull(lowQuality);

        // Higher quality should generally result in larger file size
        Assert.True(highQuality.Length > lowQuality.Length);
    }

    [Fact]
    public async Task OptimizeForWebAsync_ThrowsException_WhenInvalidData()
    {
        // Arrange
        var service = CreateService();
        var invalidData = new byte[] { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<UnknownImageFormatException>(async () =>
            await service.OptimizeForWebAsync(invalidData)
        );
    }

    [Fact]
    public async Task GetImageDimensionsAsync_ReturnsCorrectDimensions()
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(640, 480);

        // Act
        var dimensions = await service.GetImageDimensionsAsync(image);

        // Assert
        Assert.Equal(640, dimensions.width);
        Assert.Equal(480, dimensions.height);
    }

    [Fact]
    public async Task GetImageDimensionsAsync_WorksWithPngImages()
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(1024, 768, asPng: true);

        // Act
        var dimensions = await service.GetImageDimensionsAsync(image);

        // Assert
        Assert.Equal(1024, dimensions.width);
        Assert.Equal(768, dimensions.height);
    }

    [Fact]
    public async Task GetImageDimensionsAsync_ThrowsException_WhenInvalidData()
    {
        // Arrange
        var service = CreateService();
        var invalidData = new byte[] { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<UnknownImageFormatException>(async () =>
            await service.GetImageDimensionsAsync(invalidData)
        );
    }

    [Fact]
    public void IsValidImage_ReturnsTrue_ForValidJpeg()
    {
        // Arrange
        var service = CreateService();
        var validImage = CreateTestImage(100, 100);

        // Act
        var result = service.IsValidImage(validImage);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidImage_ReturnsTrue_ForValidPng()
    {
        // Arrange
        var service = CreateService();
        var validImage = CreateTestImage(100, 100, asPng: true);

        // Act
        var result = service.IsValidImage(validImage);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidImage_ReturnsFalse_ForInvalidData()
    {
        // Arrange
        var service = CreateService();
        var invalidData = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var result = service.IsValidImage(invalidData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidImage_ReturnsFalse_ForEmptyArray()
    {
        // Arrange
        var service = CreateService();
        var emptyData = Array.Empty<byte>();

        // Act
        var result = service.IsValidImage(emptyData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidImage_ReturnsFalse_ForTextData()
    {
        // Arrange
        var service = CreateService();
        var textData = System.Text.Encoding.UTF8.GetBytes("This is not an image");

        // Act
        var result = service.IsValidImage(textData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetImageMimeType_ReturnsCorrectType_ForJpeg()
    {
        // Arrange
        var service = CreateService();
        var jpegImage = CreateTestImage(100, 100, asPng: false);

        // Act
        var mimeType = service.GetImageMimeType(jpegImage);

        // Assert
        Assert.Equal("image/jpeg", mimeType);
    }

    [Fact]
    public void GetImageMimeType_ReturnsCorrectType_ForPng()
    {
        // Arrange
        var service = CreateService();
        var pngImage = CreateTestImage(100, 100, asPng: true);

        // Act
        var mimeType = service.GetImageMimeType(pngImage);

        // Assert
        Assert.Equal("image/png", mimeType);
    }

    [Fact]
    public void GetImageMimeType_ReturnsOctetStream_ForInvalidData()
    {
        // Arrange
        var service = CreateService();
        var invalidData = new byte[] { 1, 2, 3 };

        // Act
        var mimeType = service.GetImageMimeType(invalidData);

        // Assert
        Assert.Equal("application/octet-stream", mimeType);
    }

    [Fact]
    public async Task GenerateThumbnailsAsync_ReturnsEmpty_WhenSourceFileNotFound()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var service = CreateService(tempDir);

        try
        {
            var sizes = new[] { ("small", 100, 100), ("medium", 200, 200) };

            // Act
            var result = await service.GenerateThumbnailsAsync("/nonexistent/file.jpg", 1, sizes);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GenerateThumbnailsAsync_CreatesThumbnails_WhenSourceExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var service = CreateService(tempDir);

        try
        {
            // Create a test source image
            var sourceImage = CreateTestImage(800, 600);
            var sourceFilePath = Path.Combine(tempDir, "test.jpg");
            await File.WriteAllBytesAsync(sourceFilePath, sourceImage);

            var sizes = new[] { ("small", 100, 100), ("medium", 200, 200) };

            // Act
            var result = await service.GenerateThumbnailsAsync("/test.jpg", 123, sizes);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("small", result.Keys);
            Assert.Contains("medium", result.Keys);
            Assert.Contains("/uploads/media/thumbnails/123_small_100x100.jpg", result["small"]);
            Assert.Contains("/uploads/media/thumbnails/123_medium_200x200.jpg", result["medium"]);

            // Verify files were created
            var thumbnailDir = Path.Combine(tempDir, "uploads", "media", "thumbnails");
            Assert.True(Directory.Exists(thumbnailDir));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(200, 200)]
    [InlineData(1920, 1080)]
    [InlineData(50, 50)]
    public async Task ResizeImageAsync_HandlesDifferentTargetSizes(int width, int height)
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(1000, 1000);

        // Act
        var resized = await service.ResizeImageAsync(image, width, height);

        // Assert
        Assert.NotNull(resized);
        var dimensions = await service.GetImageDimensionsAsync(resized);
        Assert.True(dimensions.width <= width);
        Assert.True(dimensions.height <= height);
    }

    [Fact]
    public async Task ResizeImageAsync_PreservesImageQuality()
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(1000, 1000);

        // Act
        var resized = await service.ResizeImageAsync(image, 500, 500);

        // Assert
        Assert.True(service.IsValidImage(resized));

        // Image should still be recognizable and valid
        var dimensions = await service.GetImageDimensionsAsync(resized);
        Assert.Equal(500, dimensions.width);
        Assert.Equal(500, dimensions.height);
    }

    [Fact]
    public async Task OptimizeForWebAsync_ReducesFileSize()
    {
        // Arrange
        var service = CreateService();
        var originalImage = CreateTestImage(1000, 1000);

        // Act
        var optimized = await service.OptimizeForWebAsync(originalImage, quality: 75);

        // Assert
        Assert.NotNull(optimized);
        Assert.True(optimized.Length < originalImage.Length);
        Assert.True(service.IsValidImage(optimized));
    }

    [Fact]
    public void GetImageMimeType_HandlesDifferentImageFormats()
    {
        // Arrange
        var service = CreateService();
        var jpegImage = CreateTestImage(50, 50, asPng: false);
        var pngImage = CreateTestImage(50, 50, asPng: true);

        // Act
        var jpegMime = service.GetImageMimeType(jpegImage);
        var pngMime = service.GetImageMimeType(pngImage);

        // Assert
        Assert.Contains("jpeg", jpegMime);
        Assert.Contains("png", pngMime);
    }

    [Fact]
    public async Task ResizeImageAsync_HandlesNegativeWidth()
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(100, 100);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.ResizeImageAsync(image, -100, 100)
        );
    }

    [Fact]
    public async Task ResizeImageAsync_HandlesNegativeHeight()
    {
        // Arrange
        var service = CreateService();
        var image = CreateTestImage(100, 100);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.ResizeImageAsync(image, 100, -100)
        );
    }
}
