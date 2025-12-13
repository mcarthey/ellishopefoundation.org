using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.EntityFrameworkCore;

namespace EllisHope.Tests.Services;

public class EventServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsOnlyPublishedEvents_WhenIncludeUnpublishedIsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.AddRange(
            new Event { Id = 1, Title = "Published Event", Slug = "published-event", IsPublished = true, StartDate = DateTime.UtcNow },
            new Event { Id = 2, Title = "Draft Event", Slug = "draft-event", IsPublished = false, StartDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllEventsAsync(includeUnpublished: false);

        // Assert
        Assert.Single(result);
        Assert.Equal("Published Event", result.First().Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllEvents_WhenIncludeUnpublishedIsTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.AddRange(
            new Event { Id = 1, Title = "Published Event", Slug = "published-event", IsPublished = true, StartDate = DateTime.UtcNow },
            new Event { Id = 2, Title = "Draft Event", Slug = "draft-event", IsPublished = false, StartDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllEventsAsync(includeUnpublished: true);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsOnlyFuturePublishedEvents()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var now = DateTime.UtcNow;
        context.Events.AddRange(
            new Event { Id = 1, Title = "Future Event 1", Slug = "future-1", IsPublished = true, StartDate = now.AddDays(1) },
            new Event { Id = 2, Title = "Future Event 2", Slug = "future-2", IsPublished = true, StartDate = now.AddDays(2) },
            new Event { Id = 3, Title = "Past Event", Slug = "past", IsPublished = true, StartDate = now.AddDays(-1) },
            new Event { Id = 4, Title = "Draft Future Event", Slug = "draft-future", IsPublished = false, StartDate = now.AddDays(3) }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUpcomingEventsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, e => Assert.True(e.StartDate >= now));
        Assert.All(result, e => Assert.True(e.IsPublished));
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_RespectsCountParameter()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var now = DateTime.UtcNow;
        context.Events.AddRange(
            new Event { Id = 1, Title = "Event 1", Slug = "event-1", IsPublished = true, StartDate = now.AddDays(1) },
            new Event { Id = 2, Title = "Event 2", Slug = "event-2", IsPublished = true, StartDate = now.AddDays(2) },
            new Event { Id = 3, Title = "Event 3", Slug = "event-3", IsPublished = true, StartDate = now.AddDays(3) }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUpcomingEventsAsync(count: 2);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetPastEventsAsync_ReturnsOnlyPastPublishedEvents()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var now = DateTime.UtcNow;
        context.Events.AddRange(
            new Event { Id = 1, Title = "Past Event 1", Slug = "past-1", IsPublished = true, StartDate = now.AddDays(-2) },
            new Event { Id = 2, Title = "Past Event 2", Slug = "past-2", IsPublished = true, StartDate = now.AddDays(-1) },
            new Event { Id = 3, Title = "Future Event", Slug = "future", IsPublished = true, StartDate = now.AddDays(1) },
            new Event { Id = 4, Title = "Draft Past Event", Slug = "draft-past", IsPublished = false, StartDate = now.AddDays(-3) }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPastEventsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, e => Assert.True(e.StartDate < now));
        Assert.All(result, e => Assert.True(e.IsPublished));
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsCorrectEvent()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var eventItem = new Event { Id = 1, Title = "Test Event", Slug = "test-event", StartDate = DateTime.UtcNow };
        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Event", result.Title);
    }

    [Fact]
    public async Task GetEventByIdAsync_ReturnsNull_WhenEventNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        // Act
        var result = await service.GetEventByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEventBySlugAsync_ReturnsPublishedEvent()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.Add(new Event
        {
            Id = 1,
            Title = "Test Event",
            Slug = "test-event",
            IsPublished = true,
            StartDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventBySlugAsync("test-event");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Event", result.Title);
    }

    [Fact]
    public async Task GetEventBySlugAsync_ReturnsNull_WhenEventIsUnpublished()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.Add(new Event
        {
            Id = 1,
            Title = "Draft Event",
            Slug = "draft-event",
            IsPublished = false,
            StartDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventBySlugAsync("draft-event");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchEventsAsync_ReturnsMatchingEvents()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.AddRange(
            new Event { Id = 1, Title = "Tech Conference", Slug = "tech-conf", Description = "Technology event", Location = "Seattle", IsPublished = true, StartDate = DateTime.UtcNow },
            new Event { Id = 2, Title = "Music Festival", Slug = "music-fest", Description = "Music event", Location = "Portland", IsPublished = true, StartDate = DateTime.UtcNow },
            new Event { Id = 3, Title = "Art Show", Slug = "art-show", Description = "Art exhibition", Location = "Seattle", IsPublished = true, StartDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchEventsAsync("Seattle");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchEventsAsync_ReturnsAllPublishedEvents_WhenSearchTermIsEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.AddRange(
            new Event { Id = 1, Title = "Event 1", Slug = "event-1", IsPublished = true, StartDate = DateTime.UtcNow },
            new Event { Id = 2, Title = "Event 2", Slug = "event-2", IsPublished = false, StartDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchEventsAsync("");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetSimilarEventsAsync_ReturnsUpcomingEvents()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var now = DateTime.UtcNow;
        context.Events.AddRange(
            new Event { Id = 1, Title = "Current Event", Slug = "current", IsPublished = true, StartDate = now.AddDays(1) },
            new Event { Id = 2, Title = "Similar Event 1", Slug = "similar-1", IsPublished = true, StartDate = now.AddDays(2) },
            new Event { Id = 3, Title = "Similar Event 2", Slug = "similar-2", IsPublished = true, StartDate = now.AddDays(3) },
            new Event { Id = 4, Title = "Past Event", Slug = "past", IsPublished = true, StartDate = now.AddDays(-1) }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetSimilarEventsAsync(1, count: 2);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, e => e.Id == 1); // Should not include the current event
        Assert.All(result, e => Assert.True(e.StartDate >= now)); // Should only include future events
    }

    [Fact]
    public async Task GetSimilarEventsAsync_ReturnsEmpty_WhenEventNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        // Act
        var result = await service.GetSimilarEventsAsync(999);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateEventAsync_GeneratesSlugFromTitle()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var eventItem = new Event
        {
            Title = "My New Event",
            Slug = "",
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = await service.CreateEventAsync(eventItem);

        // Assert
        Assert.NotEmpty(result.Slug);
        Assert.Equal("my-new-event", result.Slug);
    }

    [Fact]
    public async Task CreateEventAsync_EnsuresUniqueSlug()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.Add(new Event
        {
            Title = "Test Event",
            Slug = "test-event",
            StartDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var newEvent = new Event
        {
            Title = "Test Event",
            Slug = "",
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = await service.CreateEventAsync(newEvent);

        // Assert
        Assert.Equal("test-event-1", result.Slug);
    }

    [Fact]
    public async Task CreateEventAsync_SetsCreatedAndModifiedDates()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var eventItem = new Event
        {
            Title = "Test Event",
            Slug = "test-event",
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = await service.CreateEventAsync(eventItem);

        // Assert
        Assert.NotEqual(default, result.CreatedDate);
        Assert.NotEqual(default, result.ModifiedDate);
    }

    [Fact]
    public async Task UpdateEventAsync_UpdatesModifiedDate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var eventItem = new Event
        {
            Title = "Test Event",
            Slug = "test-event",
            StartDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        var oldUpdateDate = eventItem.ModifiedDate;

        // Detach the entity to avoid tracking conflicts
        context.Entry(eventItem).State = EntityState.Detached;

        // Act
        eventItem.Title = "Updated Event";
        await Task.Delay(10); // Small delay to ensure time difference
        var result = await service.UpdateEventAsync(eventItem);

        // Assert
        Assert.True(result.ModifiedDate > oldUpdateDate);
    }

    [Fact]
    public async Task DeleteEventAsync_RemovesEvent()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        var eventItem = new Event { Title = "Test Event", Slug = "test-event", StartDate = DateTime.UtcNow };
        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        // Act
        await service.DeleteEventAsync(eventItem.Id);

        // Assert
        var deletedEvent = await context.Events.FindAsync(eventItem.Id);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task DeleteEventAsync_DoesNotThrow_WhenEventNotFound()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.DeleteEventAsync(999));
        Assert.Null(exception);
    }

    [Fact]
    public async Task SlugExistsAsync_ReturnsTrue_WhenSlugExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.Add(new Event { Title = "Test", Slug = "test-slug", StartDate = DateTime.UtcNow });
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
        var service = new EventService(context);

        // Act
        var result = await service.SlugExistsAsync("non-existent-slug");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SlugExistsAsync_ExcludesSpecifiedEvent()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        context.Events.Add(new Event { Id = 1, Title = "Test", Slug = "test-slug", StartDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act
        var result = await service.SlugExistsAsync("test-slug", excludeEventId: 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateSlug_CreatesValidSlug()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new EventService(context);

        // Act
        var result = service.GenerateSlug("My Test Event!");

        // Assert
        Assert.Equal("my-test-event", result);
    }
}
