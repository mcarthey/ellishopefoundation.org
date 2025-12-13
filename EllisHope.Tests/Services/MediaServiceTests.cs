using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Services;

public class MediaServiceTests
{
    private readonly Mock<IImageProcessingService> _imageProcessingMock;
    private readonly Mock<IUnsplashService> _unsplashMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<MediaService>> _loggerMock;

    public MediaServiceTests()
    {
        _imageProcessingMock = new Mock<IImageProcessingService>();
        _unsplashMock = new Mock<IUnsplashService>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<MediaService>>();
        
        // Setup default web root path
        _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");
    }

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private MediaService CreateService(ApplicationDbContext context)
    {
        return new MediaService(
            context,
            _imageProcessingMock.Object,
            _unsplashMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);
    }

    #region GetAllMediaAsync Tests

    [Fact]
    public async Task GetAllMediaAsync_ReturnsAllMedia_WhenNoFiltersApplied()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local, Category = MediaCategory.Blog },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Unsplash, Category = MediaCategory.Event }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllMediaAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllMediaAsync_FiltersByCategory_WhenCategoryProvided()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local, Category = MediaCategory.Blog },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Local, Category = MediaCategory.Event }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllMediaAsync(category: MediaCategory.Blog);

        // Assert
        Assert.Single(result);
        Assert.Equal("image1.jpg", result.First().FileName);
    }

    [Fact]
    public async Task GetAllMediaAsync_FiltersBySource_WhenSourceProvided()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local, Category = MediaCategory.Blog },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Unsplash, Category = MediaCategory.Blog }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllMediaAsync(source: MediaSource.Local);

        // Assert
        Assert.Single(result);
        Assert.Equal(MediaSource.Local, result.First().Source);
    }

    #endregion

    #region GetMediaByIdAsync Tests

    [Fact]
    public async Task GetMediaByIdAsync_ReturnsMedia_WhenFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        var media = new Media { Id = 1, FileName = "test.jpg", FilePath = "/uploads/test.jpg", Source = MediaSource.Local };
        context.MediaLibrary.Add(media);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetMediaByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.jpg", result.FileName);
    }

    [Fact]
    public async Task GetMediaByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetMediaByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SearchMediaAsync Tests

    [Fact]
    public async Task SearchMediaAsync_ReturnsMatchingMedia_ByFileName()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "sunset.jpg", FilePath = "/uploads/sunset.jpg", Source = MediaSource.Local },
            new Media { Id = 2, FileName = "mountains.jpg", FilePath = "/uploads/mountains.jpg", Source = MediaSource.Local }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchMediaAsync("sunset");

        // Assert
        Assert.Single(result);
        Assert.Equal("sunset.jpg", result.First().FileName);
    }

    [Fact]
    public async Task SearchMediaAsync_ReturnsMatchingMedia_ByTags()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local, Tags = "nature, landscape" },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Local, Tags = "portrait, people" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchMediaAsync("nature");

        // Assert
        Assert.Single(result);
        Assert.Contains("nature", result.First().Tags);
    }

    [Fact]
    public async Task SearchMediaAsync_ReturnsAllMedia_WhenSearchTermEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Local }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchMediaAsync("");

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetMediaByTagsAsync Tests

    [Fact]
    public async Task GetMediaByTagsAsync_ReturnsMatchingMedia()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local, Tags = "nature sunset" },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Local, Tags = "city urban" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetMediaByTagsAsync("nature");

        // Assert
        Assert.Single(result);
        Assert.Contains("nature", result.First().Tags);
    }

    [Fact]
    public async Task GetMediaByTagsAsync_ReturnsEmpty_WhenTagsEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetMediaByTagsAsync("");

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region UpdateMediaAsync Tests

    [Fact]
    public async Task UpdateMediaAsync_UpdatesMedia_Successfully()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        var media = new Media { Id = 1, FileName = "test.jpg", FilePath = "/uploads/test.jpg", Source = MediaSource.Local, Title = "Old Title" };
        context.MediaLibrary.Add(media);
        await context.SaveChangesAsync();

        // Detach to avoid tracking issues
        context.Entry(media).State = EntityState.Detached;

        // Act
        media.Title = "New Title";
        var result = await service.UpdateMediaAsync(media);

        // Assert
        Assert.True(result);
        var updated = await context.MediaLibrary.FindAsync(1);
        Assert.Equal("New Title", updated?.Title);
    }

    #endregion

    #region UpdateMediaMetadataAsync Tests

    [Fact]
    public async Task UpdateMediaMetadataAsync_UpdatesAllFields_WhenProvided()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        var media = new Media { Id = 1, FileName = "test.jpg", FilePath = "/uploads/test.jpg", Source = MediaSource.Local };
        context.MediaLibrary.Add(media);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateMediaMetadataAsync(
            1,
            altText: "New Alt",
            title: "New Title",
            caption: "New Caption",
            tags: "tag1, tag2",
            category: MediaCategory.Blog);

        // Assert
        Assert.True(result);
        var updated = await context.MediaLibrary.FindAsync(1);
        Assert.Equal("New Alt", updated?.AltText);
        Assert.Equal("New Title", updated?.Title);
        Assert.Equal("New Caption", updated?.Caption);
        Assert.Equal("tag1, tag2", updated?.Tags);
        Assert.Equal(MediaCategory.Blog, updated?.Category);
    }

    [Fact]
    public async Task UpdateMediaMetadataAsync_ReturnsFalse_WhenMediaNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateMediaMetadataAsync(999, altText: "Test");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region TrackMediaUsageAsync Tests

    [Fact]
    public async Task TrackMediaUsageAsync_CreatesNewUsage()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        var media = new Media { Id = 1, FileName = "test.jpg", FilePath = "/uploads/test.jpg", Source = MediaSource.Local };
        context.MediaLibrary.Add(media);
        await context.SaveChangesAsync();

        // Act
        await service.TrackMediaUsageAsync(1, "BlogPost", 1, UsageType.Featured);

        // Assert
        var usage = await context.MediaUsages.FirstOrDefaultAsync();
        Assert.NotNull(usage);
        Assert.Equal(1, usage.MediaId);
        Assert.Equal("BlogPost", usage.EntityType);
        Assert.Equal(1, usage.EntityId);
    }

    [Fact]
    public async Task TrackMediaUsageAsync_DoesNotDuplicate_WhenUsageExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        var media = new Media { Id = 1, FileName = "test.jpg", FilePath = "/uploads/test.jpg", Source = MediaSource.Local };
        context.MediaLibrary.Add(media);
        
        var usage = new MediaUsage
        {
            MediaId = 1,
            EntityType = "BlogPost",
            EntityId = 1,
            UsageType = UsageType.Featured
        };
        context.MediaUsages.Add(usage);
        await context.SaveChangesAsync();

        // Act
        await service.TrackMediaUsageAsync(1, "BlogPost", 1, UsageType.Featured);

        // Assert
        var count = await context.MediaUsages.CountAsync();
        Assert.Equal(1, count); // Should still be 1, not 2
    }

    #endregion

    #region RemoveMediaUsageAsync Tests

    [Fact]
    public async Task RemoveMediaUsageAsync_RemovesUsage_WhenExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        var usage = new MediaUsage
        {
            MediaId = 1,
            EntityType = "BlogPost",
            EntityId = 1,
            UsageType = UsageType.Featured
        };
        context.MediaUsages.Add(usage);
        await context.SaveChangesAsync();

        // Act
        await service.RemoveMediaUsageAsync(1, "BlogPost", 1);

        // Assert
        var count = await context.MediaUsages.CountAsync();
        Assert.Equal(0, count);
    }

    #endregion

    #region GetMediaUsagesAsync Tests

    [Fact]
    public async Task GetMediaUsagesAsync_ReturnsAllUsages_ForMedia()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaUsages.AddRange(
            new MediaUsage { MediaId = 1, EntityType = "BlogPost", EntityId = 1, UsageType = UsageType.Featured },
            new MediaUsage { MediaId = 1, EntityType = "BlogPost", EntityId = 2, UsageType = UsageType.Inline },
            new MediaUsage { MediaId = 2, EntityType = "Event", EntityId = 1, UsageType = UsageType.Featured }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetMediaUsagesAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region IsMediaInUseAsync Tests

    [Fact]
    public async Task IsMediaInUseAsync_ReturnsTrue_WhenMediaHasUsages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaUsages.Add(new MediaUsage
        {
            MediaId = 1,
            EntityType = "BlogPost",
            EntityId = 1,
            UsageType = UsageType.Featured
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.IsMediaInUseAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsMediaInUseAsync_ReturnsFalse_WhenMediaHasNoUsages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.IsMediaInUseAsync(1);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CanDeleteMediaAsync Tests

    [Fact]
    public async Task CanDeleteMediaAsync_ReturnsFalse_WhenMediaInUse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaUsages.Add(new MediaUsage
        {
            MediaId = 1,
            EntityType = "BlogPost",
            EntityId = 1,
            UsageType = UsageType.Featured
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.CanDeleteMediaAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanDeleteMediaAsync_ReturnsTrue_WhenMediaNotInUse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.CanDeleteMediaAsync(1);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalMediaCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Local }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTotalMediaCountAsync();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetTotalStorageSizeAsync_ReturnsSumOfFileSizes()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local, FileSize = 1000 },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Local, FileSize = 2000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTotalStorageSizeAsync();

        // Assert
        Assert.Equal(3000, result);
    }

    [Fact]
    public async Task GetMediaCountByCategoryAsync_ReturnsCorrectCounts()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = CreateService(context);

        context.MediaLibrary.AddRange(
            new Media { Id = 1, FileName = "image1.jpg", FilePath = "/uploads/image1.jpg", Source = MediaSource.Local, Category = MediaCategory.Blog },
            new Media { Id = 2, FileName = "image2.jpg", FilePath = "/uploads/image2.jpg", Source = MediaSource.Local, Category = MediaCategory.Blog },
            new Media { Id = 3, FileName = "image3.jpg", FilePath = "/uploads/image3.jpg", Source = MediaSource.Local, Category = MediaCategory.Event }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetMediaCountByCategoryAsync();

        // Assert
        Assert.Equal(2, result[MediaCategory.Blog]);
        Assert.Equal(1, result[MediaCategory.Event]);
    }

    #endregion
}
