using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EllisHope.Tests.Services;

public class CauseServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllCausesAsync_ReturnsOnlyPublishedCauses_WhenIncludeUnpublishedIsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Published Cause", Slug = "published-cause", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Draft Cause", Slug = "draft-cause", IsPublished = false, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllCausesAsync(includeUnpublished: false);

        // Assert
        Assert.Single(result);
        Assert.Equal("Published Cause", result.First().Title);
    }

    [Fact]
    public async Task GetAllCausesAsync_ReturnsAllCauses_WhenIncludeUnpublishedIsTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Published Cause", Slug = "published-cause", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Draft Cause", Slug = "draft-cause", IsPublished = false, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllCausesAsync(includeUnpublished: true);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFeaturedCausesAsync_ReturnsOnlyFeaturedPublishedCauses()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Featured 1", Slug = "featured-1", IsPublished = true, IsFeatured = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Featured 2", Slug = "featured-2", IsPublished = true, IsFeatured = true, GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Not Featured", Slug = "not-featured", IsPublished = true, IsFeatured = false, GoalAmount = 1000 },
            new Cause { Id = 4, Title = "Featured Draft", Slug = "featured-draft", IsPublished = false, IsFeatured = true, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetFeaturedCausesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.True(c.IsFeatured && c.IsPublished));
    }

    [Fact]
    public async Task GetFeaturedCausesAsync_RespectsCountParameter()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Featured 1", Slug = "featured-1", IsPublished = true, IsFeatured = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Featured 2", Slug = "featured-2", IsPublished = true, IsFeatured = true, GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Featured 3", Slug = "featured-3", IsPublished = true, IsFeatured = true, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetFeaturedCausesAsync(count: 2);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetActiveCausesAsync_ReturnsOnlyNonExpiredPublishedCauses()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        var now = DateTime.UtcNow;
        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Active Cause", Slug = "active-cause", IsPublished = true, EndDate = now.AddDays(10), GoalAmount = 1000 },
            new Cause { Id = 2, Title = "No End Date", Slug = "no-end-date", IsPublished = true, EndDate = null, GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Expired Cause", Slug = "expired-cause", IsPublished = true, EndDate = now.AddDays(-1), GoalAmount = 1000 },
            new Cause { Id = 4, Title = "Draft Active", Slug = "draft-active", IsPublished = false, EndDate = now.AddDays(10), GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetActiveCausesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Title == "Active Cause");
        Assert.Contains(result, c => c.Title == "No End Date");
    }

    [Fact]
    public async Task GetActiveCausesAsync_OrdersByFeaturedThenCreatedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        var now = DateTime.UtcNow;
        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Regular Old", Slug = "regular-old", IsPublished = true, IsFeatured = false, CreatedDate = now.AddDays(-10), GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Featured New", Slug = "featured-new", IsPublished = true, IsFeatured = true, CreatedDate = now.AddDays(-1), GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Featured Old", Slug = "featured-old", IsPublished = true, IsFeatured = true, CreatedDate = now.AddDays(-5), GoalAmount = 1000 },
            new Cause { Id = 4, Title = "Regular New", Slug = "regular-new", IsPublished = true, IsFeatured = false, CreatedDate = now, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = (await service.GetActiveCausesAsync()).ToList();

        // Assert
        Assert.Equal(4, result.Count);
        // Featured causes should come first
        Assert.True(result[0].IsFeatured);
        Assert.True(result[1].IsFeatured);
        // Within featured, newer should come first
        Assert.Equal("Featured New", result[0].Title);
        Assert.Equal("Featured Old", result[1].Title);
    }

    [Fact]
    public async Task GetCauseByIdAsync_ReturnsCorrectCause()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        var cause = new Cause { Id = 1, Title = "Test Cause", Slug = "test-cause", GoalAmount = 1000 };
        context.Causes.Add(cause);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCauseByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Cause", result.Title);
    }

    [Fact]
    public async Task GetCauseByIdAsync_ReturnsNull_WhenCauseNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        // Act
        var result = await service.GetCauseByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCauseBySlugAsync_ReturnsPublishedCause()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.Add(new Cause
        {
            Id = 1,
            Title = "Test Cause",
            Slug = "test-cause",
            IsPublished = true,
            GoalAmount = 1000
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCauseBySlugAsync("test-cause");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Cause", result.Title);
    }

    [Fact]
    public async Task GetCauseBySlugAsync_ReturnsNull_WhenCauseIsUnpublished()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.Add(new Cause
        {
            Id = 1,
            Title = "Draft Cause",
            Slug = "draft-cause",
            IsPublished = false,
            GoalAmount = 1000
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCauseBySlugAsync("draft-cause");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchCausesAsync_ReturnsMatchingCauses()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Water for Africa", Slug = "water-africa", Description = "Provide clean water", Category = "Water", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Education Initiative", Slug = "education", Description = "Support schools", Category = "Education", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Clean Water Project", Slug = "clean-water", ShortDescription = "Water purification", Category = "Water", IsPublished = true, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchCausesAsync("water");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchCausesAsync_ReturnsAllPublishedCauses_WhenSearchTermIsEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Cause 1", Slug = "cause-1", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Cause 2", Slug = "cause-2", IsPublished = false, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchCausesAsync("");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetCausesByCategoryAsync_ReturnsOnlyCausesInCategory()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Water Cause", Slug = "water-cause", Category = "Water", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Education Cause", Slug = "education-cause", Category = "Education", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Another Water", Slug = "another-water", Category = "Water", IsPublished = true, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCausesByCategoryAsync("Water");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal("Water", c.Category));
    }

    [Fact]
    public async Task GetSimilarCausesAsync_ReturnsCausesWithSameCategory()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.AddRange(
            new Cause { Id = 1, Title = "Current Cause", Slug = "current", Category = "Water", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Similar 1", Slug = "similar-1", Category = "Water", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Similar 2", Slug = "similar-2", Category = "Water", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 4, Title = "Different", Slug = "different", Category = "Education", IsPublished = true, GoalAmount = 1000 }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetSimilarCausesAsync(1, count: 2);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, c => c.Id == 1); // Should not include current cause
        Assert.All(result, c => Assert.Equal("Water", c.Category));
    }

    [Fact]
    public async Task GetSimilarCausesAsync_ReturnsEmpty_WhenCauseNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        // Act
        var result = await service.GetSimilarCausesAsync(999);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateCauseAsync_GeneratesSlugFromTitle()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        var cause = new Cause
        {
            Title = "My New Cause",
            Slug = "",
            GoalAmount = 1000
        };

        // Act
        var result = await service.CreateCauseAsync(cause);

        // Assert
        Assert.NotEmpty(result.Slug);
        Assert.Equal("my-new-cause", result.Slug);
    }

    [Fact]
    public async Task CreateCauseAsync_EnsuresUniqueSlug()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.Add(new Cause
        {
            Title = "Test Cause",
            Slug = "test-cause",
            GoalAmount = 1000
        });
        await context.SaveChangesAsync();

        var newCause = new Cause
        {
            Title = "Test Cause",
            Slug = "",
            GoalAmount = 1000
        };

        // Act
        var result = await service.CreateCauseAsync(newCause);

        // Assert
        Assert.Equal("test-cause-1", result.Slug);
    }

    [Fact]
    public async Task CreateCauseAsync_SetsCreatedAndUpdatedDates()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        var cause = new Cause
        {
            Title = "Test Cause",
            Slug = "test-cause",
            GoalAmount = 1000
        };

        // Act
        var result = await service.CreateCauseAsync(cause);

        // Assert
        Assert.NotEqual(default, result.CreatedDate);
        Assert.NotEqual(default, result.UpdatedDate);
    }

    [Fact]
    public async Task UpdateCauseAsync_UpdatesUpdatedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        var cause = new Cause
        {
            Title = "Test Cause",
            Slug = "test-cause",
            GoalAmount = 1000,
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            UpdatedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.Causes.Add(cause);
        await context.SaveChangesAsync();

        var oldUpdateDate = cause.UpdatedDate;

        // Detach the entity to avoid tracking conflicts
        context.Entry(cause).State = EntityState.Detached;

        // Act
        cause.Title = "Updated Cause";
        await Task.Delay(10); // Small delay to ensure time difference
        var result = await service.UpdateCauseAsync(cause);

        // Assert
        Assert.True(result.UpdatedDate > oldUpdateDate);
    }

    [Fact]
    public async Task DeleteCauseAsync_RemovesCause()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        var cause = new Cause { Title = "Test Cause", Slug = "test-cause", GoalAmount = 1000 };
        context.Causes.Add(cause);
        await context.SaveChangesAsync();

        // Act
        await service.DeleteCauseAsync(cause.Id);

        // Assert
        var deletedCause = await context.Causes.FindAsync(cause.Id);
        Assert.Null(deletedCause);
    }

    [Fact]
    public async Task DeleteCauseAsync_DoesNotThrow_WhenCauseNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.DeleteCauseAsync(999));
        Assert.Null(exception);
    }

    [Fact]
    public async Task SlugExistsAsync_ReturnsTrue_WhenSlugExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.Add(new Cause { Title = "Test", Slug = "test-slug", GoalAmount = 1000 });
        await context.SaveChangesAsync();

        // Act
        var result = await service.SlugExistsAsync("test-slug");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SlugExistsAsync_ReturnsFalse_WhenSlugDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        // Act
        var result = await service.SlugExistsAsync("non-existent-slug");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SlugExistsAsync_ExcludesSpecifiedCause()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        context.Causes.Add(new Cause { Id = 1, Title = "Test", Slug = "test-slug", GoalAmount = 1000 });
        await context.SaveChangesAsync();

        // Act
        var result = await service.SlugExistsAsync("test-slug", excludeCauseId: 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateSlug_CreatesValidSlug()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new CauseService(context);

        // Act
        var result = service.GenerateSlug("My Test Cause!");

        // Assert
        Assert.Equal("my-test-cause", result);
    }

    [Fact]
    public void Cause_ProgressPercentage_CalculatesCorrectly()
    {
        // Arrange & Act
        var cause = new Cause
        {
            GoalAmount = 1000,
            RaisedAmount = 750
        };

        // Assert
        Assert.Equal(75, cause.ProgressPercentage);
    }

    [Fact]
    public void Cause_ProgressPercentage_ReturnsZero_WhenGoalIsZero()
    {
        // Arrange & Act
        var cause = new Cause
        {
            GoalAmount = 0,
            RaisedAmount = 500
        };

        // Assert
        Assert.Equal(0, cause.ProgressPercentage);
    }

    [Fact]
    public void Cause_ProgressPercentage_CanExceed100()
    {
        // Arrange & Act
        var cause = new Cause
        {
            GoalAmount = 1000,
            RaisedAmount = 1500
        };

        // Assert
        Assert.Equal(150, cause.ProgressPercentage);
    }
}
