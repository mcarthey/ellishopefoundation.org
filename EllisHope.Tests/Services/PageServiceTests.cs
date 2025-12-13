using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Services;

public class PageServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private PageService GetPageService(ApplicationDbContext context)
    {
        var mockLogger = new Mock<ILogger<PageService>>();
        return new PageService(context, mockLogger.Object);
    }

    #region GetAllPagesAsync Tests

    [Fact]
    public async Task GetAllPagesAsync_ReturnsAllPages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        context.Pages.AddRange(
            new Page { PageName = "Home", Title = "Home Page", IsPublished = true },
            new Page { PageName = "About", Title = "About Us", IsPublished = false }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPagesAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllPagesAsync_IncludesContentSections()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.ContentSections.Add(new ContentSection
        {
            PageId = page.Id,
            SectionKey = "WelcomeText",
            Content = "Welcome!",
            ContentType = "Text",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPagesAsync();
        var homePage = result.First();

        // Assert
        Assert.Single(homePage.ContentSections);
        Assert.Equal("WelcomeText", homePage.ContentSections.First().SectionKey);
    }

    [Fact]
    public async Task GetAllPagesAsync_IncludesPageImages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media = new Media
        {
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.Add(media);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.PageImages.Add(new PageImage
        {
            PageId = page.Id,
            MediaId = media.Id,
            ImageKey = "HeroImage",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPagesAsync();
        var homePage = result.First();

        // Assert
        Assert.Single(homePage.PageImages);
        Assert.Equal("HeroImage", homePage.PageImages.First().ImageKey);
    }

    [Fact]
    public async Task GetAllPagesAsync_OrdersByPageName()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        context.Pages.AddRange(
            new Page { PageName = "Team", Title = "Team" },
            new Page { PageName = "About", Title = "About" },
            new Page { PageName = "Contact", Title = "Contact" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPagesAsync();

        // Assert
        Assert.Equal("About", result.First().PageName);
        Assert.Equal("Contact", result.Skip(1).First().PageName);
        Assert.Equal("Team", result.Last().PageName);
    }

    #endregion

    #region GetPageByIdAsync Tests

    [Fact]
    public async Task GetPageByIdAsync_ReturnsCorrectPage()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Home Page" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPageByIdAsync(page.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Home", result.PageName);
    }

    [Fact]
    public async Task GetPageByIdAsync_ReturnsNull_WhenPageNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        // Act
        var result = await service.GetPageByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPageByIdAsync_IncludesRelatedData()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media = new Media
        {
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.Add(media);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.ContentSections.Add(new ContentSection
        {
            PageId = page.Id,
            SectionKey = "Test",
            Content = "Content",
            ContentType = "Text",
            DisplayOrder = 0
        });

        context.PageImages.Add(new PageImage
        {
            PageId = page.Id,
            MediaId = media.Id,
            ImageKey = "HeroImage",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPageByIdAsync(page.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.ContentSections);
        Assert.Single(result.PageImages);
        Assert.NotNull(result.PageImages.First().Media);
    }

    #endregion

    #region GetPageByNameAsync Tests

    [Fact]
    public async Task GetPageByNameAsync_ReturnsCorrectPage()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        context.Pages.AddRange(
            new Page { PageName = "Home", Title = "Home Page" },
            new Page { PageName = "About", Title = "About Page" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPageByNameAsync("About");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("About", result.PageName);
        Assert.Equal("About Page", result.Title);
    }

    [Fact]
    public async Task GetPageByNameAsync_ReturnsNull_WhenPageNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        // Act
        var result = await service.GetPageByNameAsync("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPageByNameAsync_IsCaseSensitive()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        context.Pages.Add(new Page { PageName = "Home", Title = "Home Page" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPageByNameAsync("home");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CreatePageAsync Tests

    [Fact]
    public async Task CreatePageAsync_AddsPageToDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "NewPage", Title = "New Page" };

        // Act
        var result = await service.CreatePageAsync(page);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(1, await context.Pages.CountAsync());
    }

    [Fact]
    public async Task CreatePageAsync_SetsCreatedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "NewPage", Title = "New Page" };

        // Act
        var result = await service.CreatePageAsync(page);

        // Assert
        Assert.NotEqual(default, result.CreatedDate);
        Assert.True(result.CreatedDate <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreatePageAsync_SetsModifiedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "NewPage", Title = "New Page" };
        var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = await service.CreatePageAsync(page);

        // Assert
        Assert.NotEqual(default, result.ModifiedDate);
        Assert.True(result.ModifiedDate >= beforeCreate);
        Assert.True(result.ModifiedDate <= DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region UpdatePageAsync Tests

    [Fact]
    public async Task UpdatePageAsync_UpdatesPageInDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Original Title" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        page.Title = "Updated Title";

        // Act
        var result = await service.UpdatePageAsync(page);

        // Assert
        var updated = await context.Pages.FindAsync(page.Id);
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task UpdatePageAsync_UpdatesModifiedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page
        {
            PageName = "Home",
            Title = "Home",
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        var oldModifiedDate = page.ModifiedDate;
        await Task.Delay(10); // Ensure time difference

        // Act
        var result = await service.UpdatePageAsync(page);

        // Assert
        Assert.True(result.ModifiedDate > oldModifiedDate);
    }

    #endregion

    #region DeletePageAsync Tests

    [Fact]
    public async Task DeletePageAsync_RemovesPageFromDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "ToDelete", Title = "Delete Me" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        // Act
        await service.DeletePageAsync(page.Id);

        // Assert
        Assert.Equal(0, await context.Pages.CountAsync());
    }

    [Fact]
    public async Task DeletePageAsync_RemovesContentSections()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "ToDelete", Title = "Delete Me" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.ContentSections.Add(new ContentSection
        {
            PageId = page.Id,
            SectionKey = "Test",
            Content = "Content",
            ContentType = "Text",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        await service.DeletePageAsync(page.Id);

        // Assert
        Assert.Equal(0, await context.ContentSections.CountAsync());
    }

    [Fact]
    public async Task DeletePageAsync_RemovesPageImages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media = new Media
        {
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.Add(media);

        var page = new Page { PageName = "ToDelete", Title = "Delete Me" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.PageImages.Add(new PageImage
        {
            PageId = page.Id,
            MediaId = media.Id,
            ImageKey = "HeroImage",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        await service.DeletePageAsync(page.Id);

        // Assert
        Assert.Equal(0, await context.PageImages.CountAsync());
    }

    [Fact]
    public async Task DeletePageAsync_DoesNotThrow_WhenPageNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.DeletePageAsync(999));
        Assert.Null(exception);
    }

    #endregion

    #region ContentSection Tests

    [Fact]
    public async Task GetContentSectionAsync_ReturnsCorrectSection()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.ContentSections.Add(new ContentSection
        {
            PageId = page.Id,
            SectionKey = "WelcomeText",
            Content = "Welcome!",
            ContentType = "Text",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetContentSectionAsync(page.Id, "WelcomeText");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Welcome!", result.Content);
    }

    [Fact]
    public async Task GetContentSectionAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetContentSectionAsync(page.Id, "NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateContentSectionAsync_CreatesNewSection_WhenNotExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        // Act
        await service.UpdateContentSectionAsync(page.Id, "WelcomeText", "Hello!", "RichText");

        // Assert
        var section = await context.ContentSections.FirstOrDefaultAsync();
        Assert.NotNull(section);
        Assert.Equal("WelcomeText", section.SectionKey);
        Assert.Equal("Hello!", section.Content);
        Assert.Equal("RichText", section.ContentType);
    }

    [Fact]
    public async Task UpdateContentSectionAsync_UpdatesExistingSection()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.ContentSections.Add(new ContentSection
        {
            PageId = page.Id,
            SectionKey = "WelcomeText",
            Content = "Original",
            ContentType = "Text",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        await service.UpdateContentSectionAsync(page.Id, "WelcomeText", "Updated", "RichText");

        // Assert
        var section = await context.ContentSections.FirstOrDefaultAsync();
        Assert.NotNull(section);
        Assert.Equal("Updated", section.Content);
        Assert.Equal("RichText", section.ContentType);
    }

    [Fact]
    public async Task UpdateContentSectionAsync_UpdatesPageModifiedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page
        {
            PageName = "Home",
            Title = "Home",
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        var oldModifiedDate = page.ModifiedDate;
        await Task.Delay(10);

        // Act
        await service.UpdateContentSectionAsync(page.Id, "WelcomeText", "Content", "Text");

        // Assert
        var updatedPage = await context.Pages.FindAsync(page.Id);
        Assert.True(updatedPage!.ModifiedDate > oldModifiedDate);
    }

    [Fact]
    public async Task GetPageContentSectionsAsync_ReturnsAllSections()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.ContentSections.AddRange(
            new ContentSection { PageId = page.Id, SectionKey = "Section1", Content = "Content1", ContentType = "Text", DisplayOrder = 2 },
            new ContentSection { PageId = page.Id, SectionKey = "Section2", Content = "Content2", ContentType = "Text", DisplayOrder = 1 },
            new ContentSection { PageId = page.Id, SectionKey = "Section3", Content = "Content3", ContentType = "Text", DisplayOrder = 0 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPageContentSectionsAsync(page.Id);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal("Section3", result.First().SectionKey); // DisplayOrder 0 is first
    }

    #endregion

    #region PageImage Tests

    [Fact]
    public async Task GetPageImageAsync_ReturnsCorrectImage()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media = new Media
        {
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.Add(media);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.PageImages.Add(new PageImage
        {
            PageId = page.Id,
            MediaId = media.Id,
            ImageKey = "HeroImage",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPageImageAsync(page.Id, "HeroImage");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("HeroImage", result.ImageKey);
        Assert.NotNull(result.Media);
    }

    [Fact]
    public async Task SetPageImageAsync_CreatesNewImage_WhenNotExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media = new Media
        {
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.Add(media);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        // Act
        await service.SetPageImageAsync(page.Id, "HeroImage", media.Id, 0);

        // Assert
        var pageImage = await context.PageImages.FirstOrDefaultAsync();
        Assert.NotNull(pageImage);
        Assert.Equal("HeroImage", pageImage.ImageKey);
        Assert.Equal(media.Id, pageImage.MediaId);
    }

    [Fact]
    public async Task SetPageImageAsync_UpdatesExistingImage()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media1 = new Media
        {
            FileName = "test1.jpg",
            FilePath = "/uploads/test1.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        var media2 = new Media
        {
            FileName = "test2.jpg",
            FilePath = "/uploads/test2.jpg",
            MimeType = "image/jpeg",
            FileSize = 2048,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.AddRange(media1, media2);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.PageImages.Add(new PageImage
        {
            PageId = page.Id,
            MediaId = media1.Id,
            ImageKey = "HeroImage",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        await service.SetPageImageAsync(page.Id, "HeroImage", media2.Id, 1);

        // Assert
        var pageImage = await context.PageImages.FirstOrDefaultAsync();
        Assert.NotNull(pageImage);
        Assert.Equal(media2.Id, pageImage.MediaId);
        Assert.Equal(1, pageImage.DisplayOrder);
    }

    [Fact]
    public async Task RemovePageImageAsync_RemovesImage()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media = new Media
        {
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.Add(media);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.PageImages.Add(new PageImage
        {
            PageId = page.Id,
            MediaId = media.Id,
            ImageKey = "HeroImage",
            DisplayOrder = 0
        });
        await context.SaveChangesAsync();

        // Act
        await service.RemovePageImageAsync(page.Id, "HeroImage");

        // Assert
        Assert.Equal(0, await context.PageImages.CountAsync());
    }

    [Fact]
    public async Task GetPageImagesAsync_ReturnsAllImages_OrderedByDisplayOrder()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        var media = new Media
        {
            FileName = "test.jpg",
            FilePath = "/uploads/test.jpg",
            MimeType = "image/jpeg",
            FileSize = 1024,
            Source = MediaSource.Local,
            Category = MediaCategory.Page
        };
        context.MediaLibrary.Add(media);

        var page = new Page { PageName = "Home", Title = "Home" };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        context.PageImages.AddRange(
            new PageImage { PageId = page.Id, MediaId = media.Id, ImageKey = "Image3", DisplayOrder = 2 },
            new PageImage { PageId = page.Id, MediaId = media.Id, ImageKey = "Image1", DisplayOrder = 0 },
            new PageImage { PageId = page.Id, MediaId = media.Id, ImageKey = "Image2", DisplayOrder = 1 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPageImagesAsync(page.Id);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal("Image1", result.First().ImageKey);
        Assert.Equal("Image3", result.Last().ImageKey);
    }

    #endregion

    #region PageExistsAsync Tests

    [Fact]
    public async Task PageExistsAsync_ReturnsTrue_WhenPageExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        context.Pages.Add(new Page { PageName = "Home", Title = "Home" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.PageExistsAsync("Home");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PageExistsAsync_ReturnsFalse_WhenPageDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        // Act
        var result = await service.PageExistsAsync("NonExistent");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region InitializeDefaultPagesAsync Tests

    [Fact]
    public async Task InitializeDefaultPagesAsync_CreatesDefaultPages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        // Act
        await service.InitializeDefaultPagesAsync();

        // Assert
        var pages = await context.Pages.ToListAsync();
        Assert.Equal(5, pages.Count);
        Assert.Contains(pages, p => p.PageName == "Home");
        Assert.Contains(pages, p => p.PageName == "About");
        Assert.Contains(pages, p => p.PageName == "Team");
        Assert.Contains(pages, p => p.PageName == "Services");
        Assert.Contains(pages, p => p.PageName == "Contact");
    }

    [Fact]
    public async Task InitializeDefaultPagesAsync_DoesNotDuplicatePages()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        // Act
        await service.InitializeDefaultPagesAsync();
        await service.InitializeDefaultPagesAsync(); // Call twice

        // Assert
        var pages = await context.Pages.ToListAsync();
        Assert.Equal(5, pages.Count); // Still only 5 pages
    }

    [Fact]
    public async Task InitializeDefaultPagesAsync_SetsAllPagesToPublished()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = GetPageService(context);

        // Act
        await service.InitializeDefaultPagesAsync();

        // Assert
        var pages = await context.Pages.ToListAsync();
        Assert.All(pages, p => Assert.True(p.IsPublished));
    }

    #endregion
}
