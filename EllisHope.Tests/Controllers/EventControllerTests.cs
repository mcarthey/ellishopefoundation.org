using EllisHope.Controllers;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class EventControllerTests
{
    private readonly Mock<IEventService> _mockEventService;
    private readonly EventController _controller;

    public EventControllerTests()
    {
        _mockEventService = new Mock<IEventService>();
        _controller = new EventController(_mockEventService.Object);
    }

    #region List Action Tests

    [Fact]
    public async Task List_ReturnsUpcomingEvents_WhenNoSearch()
    {
        // Arrange
        var events = new List<Event>
        {
            new Event { Id = 1, Title = "Event 1", Slug = "event-1", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) },
            new Event { Id = 2, Title = "Event 2", Slug = "event-2", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(14) }
        };

        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(100)).ReturnsAsync(events);

        // Act
        var result = await _controller.list(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Event>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task List_ReturnsSearchResults_WhenSearchTermProvided()
    {
        // Arrange
        var searchTerm = "charity";
        var events = new List<Event>
        {
            new Event { Id = 1, Title = "Charity Run", Slug = "charity-run", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) }
        };

        _mockEventService.Setup(s => s.SearchEventsAsync(searchTerm)).ReturnsAsync(events);

        // Act
        var result = await _controller.list(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Event>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal(searchTerm, _controller.ViewBag.SearchTerm);
    }

    [Fact]
    public async Task List_SetsViewBagSearchTerm()
    {
        // Arrange
        var searchTerm = "test";
        _mockEventService.Setup(s => s.SearchEventsAsync(searchTerm))
            .ReturnsAsync(new List<Event>());

        // Act
        await _controller.list(searchTerm);

        // Assert
        Assert.Equal(searchTerm, _controller.ViewBag.SearchTerm);
    }

    [Fact]
    public async Task List_CallsGetUpcomingEventsAsync_WithLimit100()
    {
        // Arrange
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(100))
            .ReturnsAsync(new List<Event>());

        // Act
        await _controller.list(null);

        // Assert
        _mockEventService.Verify(s => s.GetUpcomingEventsAsync(100), Times.Once);
    }

    #endregion

    #region Details Action Tests

    [Fact]
    public async Task Details_ReturnsNotFound_WhenSlugIsNull()
    {
        // Act
        var result = await _controller.details(null!);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenSlugIsEmpty()
    {
        // Act
        var result = await _controller.details("");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var slug = "non-existent-event";
        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync((Event?)null);

        // Act
        var result = await _controller.details(slug);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithEvent_WhenEventExists()
    {
        // Arrange
        var slug = "test-event";
        var eventItem = new Event
        {
            Id = 1,
            Title = "Test Event",
            Slug = slug,
            IsPublished = true,
            EventDate = DateTime.UtcNow.AddDays(7),
            Description = "Test description"
        };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(new List<Event>());
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(new List<Event>());

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Event>(viewResult.Model);
        Assert.Equal(eventItem.Id, model.Id);
        Assert.Equal(eventItem.Title, model.Title);
        Assert.Equal(eventItem.Slug, model.Slug);
    }

    [Fact]
    public async Task Details_PopulatesViewBagWithSimilarEvents()
    {
        // Arrange
        var slug = "test-event";
        var eventItem = new Event { Id = 1, Title = "Test", Slug = slug, IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) };
        var similarEvents = new List<Event>
        {
            new Event { Id = 2, Title = "Similar 1", Slug = "similar-1", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(10) },
            new Event { Id = 3, Title = "Similar 2", Slug = "similar-2", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(12) }
        };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(similarEvents);
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(new List<Event>());

        // Act
        await _controller.details(slug);

        // Assert
        var viewBagSimilarEvents = _controller.ViewBag.SimilarEvents as IEnumerable<Event>;
        Assert.NotNull(viewBagSimilarEvents);
        Assert.Equal(2, viewBagSimilarEvents.Count());
    }

    [Fact]
    public async Task Details_PopulatesViewBagWithRecentEvents()
    {
        // Arrange
        var slug = "test-event";
        var eventItem = new Event { Id = 1, Title = "Test", Slug = slug, IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) };
        var recentEvents = new List<Event>
        {
            new Event { Id = 2, Title = "Recent 1", Slug = "recent-1", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(5) },
            new Event { Id = 3, Title = "Recent 2", Slug = "recent-2", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(6) },
            new Event { Id = 4, Title = "Recent 3", Slug = "recent-3", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(8) }
        };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(new List<Event>());
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(recentEvents);

        // Act
        await _controller.details(slug);

        // Assert
        var viewBagRecentEvents = _controller.ViewBag.RecentEvents as IEnumerable<Event>;
        Assert.NotNull(viewBagRecentEvents);
        Assert.Equal(3, viewBagRecentEvents.Count());
    }

    [Fact]
    public async Task Details_CallsGetSimilarEventsAsync_WithCorrectParameters()
    {
        // Arrange
        var slug = "test-event";
        var eventItem = new Event { Id = 1, Title = "Test", Slug = slug, IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(new List<Event>());
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(new List<Event>());

        // Act
        await _controller.details(slug);

        // Assert
        _mockEventService.Verify(s => s.GetSimilarEventsAsync(eventItem.Id, 4), Times.Once);
    }

    [Fact]
    public async Task Details_CallsGetUpcomingEventsAsync_WithLimit3()
    {
        // Arrange
        var slug = "test-event";
        var eventItem = new Event { Id = 1, Title = "Test", Slug = slug, IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(new List<Event>());
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(new List<Event>());

        // Act
        await _controller.details(slug);

        // Assert
        _mockEventService.Verify(s => s.GetUpcomingEventsAsync(3), Times.Once);
    }

    [Fact]
    public async Task Details_CallsGetEventBySlugAsync_WithCorrectSlug()
    {
        // Arrange
        var slug = "my-test-event";
        var eventItem = new Event { Id = 1, Title = "Test", Slug = slug, IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(new List<Event>());
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(new List<Event>());

        // Act
        await _controller.details(slug);

        // Assert
        _mockEventService.Verify(s => s.GetEventBySlugAsync(slug), Times.Once);
    }

    [Fact]
    public async Task Details_HandlesEmptySimilarEvents()
    {
        // Arrange
        var slug = "only-event";
        var eventItem = new Event { Id = 1, Title = "Only Event", Slug = slug, IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(new List<Event>());
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(new List<Event>());

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var similarEvents = _controller.ViewBag.SimilarEvents as IEnumerable<Event>;
        Assert.NotNull(similarEvents);
        Assert.Empty(similarEvents);
    }

    [Fact]
    public async Task Details_HandlesEmptyRecentEvents()
    {
        // Arrange
        var slug = "first-event";
        var eventItem = new Event { Id = 1, Title = "First Event", Slug = slug, IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(eventItem);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(eventItem.Id, 4)).ReturnsAsync(new List<Event>());
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(new List<Event>());

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var recentEvents = _controller.ViewBag.RecentEvents as IEnumerable<Event>;
        Assert.NotNull(recentEvents);
        Assert.Empty(recentEvents);
    }

    #endregion

    #region Grid Action Tests

    [Fact]
    public void Grid_ReturnsView()
    {
        // Act
        var result = _controller.grid();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region Integration-like Tests

    [Fact]
    public async Task Details_IntegrationTest_CompleteWorkflow()
    {
        // Arrange
        var slug = "complete-test-event";
        var currentEvent = new Event
        {
            Id = 1,
            Title = "Complete Test Event",
            Slug = slug,
            Description = "<p>This is a complete test event</p>",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            OrganizerName = "Test Organizer",
            OrganizerEmail = "organizer@test.com",
            OrganizerPhone = "555-1234",
            RegistrationUrl = "https://test.com/register",
            IsPublished = true,
            Tags = "test,integration,complete",
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        };

        var similarEvents = new List<Event>
        {
            new Event { Id = 2, Title = "Similar 1", Slug = "similar-1", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(10) },
            new Event { Id = 3, Title = "Similar 2", Slug = "similar-2", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(12) }
        };

        var recentEvents = new List<Event>
        {
            new Event { Id = 4, Title = "Recent 1", Slug = "recent-1", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(5) },
            new Event { Id = 5, Title = "Recent 2", Slug = "recent-2", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(6) },
            new Event { Id = 6, Title = "Recent 3", Slug = "recent-3", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(8) }
        };

        _mockEventService.Setup(s => s.GetEventBySlugAsync(slug)).ReturnsAsync(currentEvent);
        _mockEventService.Setup(s => s.GetSimilarEventsAsync(currentEvent.Id, 4)).ReturnsAsync(similarEvents);
        _mockEventService.Setup(s => s.GetUpcomingEventsAsync(3)).ReturnsAsync(recentEvents);

        // Act
        var result = await _controller.details(slug);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Event>(viewResult.Model);
        
        // Verify model
        Assert.Equal(currentEvent.Id, model.Id);
        Assert.Equal(currentEvent.Title, model.Title);
        Assert.Equal(currentEvent.Description, model.Description);
        Assert.Equal(currentEvent.Location, model.Location);
        
        // Verify ViewBag
        var viewBagSimilarEvents = _controller.ViewBag.SimilarEvents as IEnumerable<Event>;
        Assert.NotNull(viewBagSimilarEvents);
        Assert.Equal(2, viewBagSimilarEvents.Count());
        
        var viewBagRecentEvents = _controller.ViewBag.RecentEvents as IEnumerable<Event>;
        Assert.NotNull(viewBagRecentEvents);
        Assert.Equal(3, viewBagRecentEvents.Count());
    }

    [Fact]
    public async Task List_IntegrationTest_SearchWorkflow()
    {
        // Arrange
        var searchTerm = "charity";
        var searchResults = new List<Event>
        {
            new Event { Id = 1, Title = "Charity Run", Slug = "charity-run", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(7) },
            new Event { Id = 2, Title = "Charity Gala", Slug = "charity-gala", IsPublished = true, EventDate = DateTime.UtcNow.AddDays(14) }
        };

        _mockEventService.Setup(s => s.SearchEventsAsync(searchTerm)).ReturnsAsync(searchResults);

        // Act
        var result = await _controller.list(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Event>>(viewResult.Model);
        
        Assert.Equal(2, model.Count());
        Assert.Equal(searchTerm, _controller.ViewBag.SearchTerm);
        Assert.All(model, e => Assert.Contains(searchTerm, e.Title, StringComparison.OrdinalIgnoreCase));
        
        _mockEventService.Verify(s => s.SearchEventsAsync(searchTerm), Times.Once);
        _mockEventService.Verify(s => s.GetUpcomingEventsAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}
