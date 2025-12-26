using EllisHope.Controllers;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class CausesControllerTests
{
    private readonly Mock<ICauseService> _mockCauseService;
    private readonly Mock<ILogger<CausesController>> _mockLogger;
    private readonly CausesController _controller;

    public CausesControllerTests()
    {
        _mockCauseService = new Mock<ICauseService>();
        _mockLogger = new Mock<ILogger<CausesController>>();
        _controller = new CausesController(_mockCauseService.Object, _mockLogger.Object);
    }

    #region List Action Tests

    [Fact]
    public async Task List_ReturnsActiveCauses_WhenNoSearch()
    {
        // Arrange
        var causes = new List<Cause>
        {
            new Cause { Id = 1, Title = "Cause 1", Slug = "cause-1", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Cause 2", Slug = "cause-2", IsPublished = true, GoalAmount = 2000 }
        };

        _mockCauseService.Setup(s => s.GetActiveCausesAsync())
            .ReturnsAsync(causes);

        // Act
        var result = await _controller.Index(null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Cause>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task List_ReturnsSearchResults_WhenSearchTermProvided()
    {
        // Arrange
        var searchResults = new List<Cause>
        {
            new Cause { Id = 1, Title = "Water Project", Slug = "water-project", IsPublished = true, GoalAmount = 1000 }
        };

        _mockCauseService.Setup(s => s.SearchCausesAsync("water"))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.Index("water");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Cause>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal("water", _controller.ViewBag.SearchTerm);
    }

    [Fact]
    public async Task List_SetsViewBagSearchTerm()
    {
        // Arrange
        _mockCauseService.Setup(s => s.SearchCausesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Index("education");

        // Assert
        Assert.Equal("education", _controller.ViewBag.SearchTerm);
    }

    [Fact]
    public async Task List_CallsGetActiveCausesAsync_WhenSearchIsNull()
    {
        // Arrange
        _mockCauseService.Setup(s => s.GetActiveCausesAsync())
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Index(null);

        // Assert
        _mockCauseService.Verify(s => s.GetActiveCausesAsync(), Times.Once);
        _mockCauseService.Verify(s => s.SearchCausesAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task List_CallsSearchCausesAsync_WhenSearchProvided()
    {
        // Arrange
        _mockCauseService.Setup(s => s.SearchCausesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Index("test");

        // Assert
        _mockCauseService.Verify(s => s.SearchCausesAsync("test"), Times.Once);
        _mockCauseService.Verify(s => s.GetActiveCausesAsync(), Times.Never);
    }

    #endregion

    #region Details Action Tests

    [Fact]
    public async Task Details_ReturnsNotFound_WhenSlugIsNull()
    {
        // Act
        var result = await _controller.Details(null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenSlugIsEmpty()
    {
        // Act
        var result = await _controller.Details("");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenCauseDoesNotExist()
    {
        // Arrange
        _mockCauseService.Setup(s => s.GetCauseBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync((Cause?)null);

        // Act
        var result = await _controller.Details("non-existent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithCause_WhenCauseExists()
    {
        // Arrange
        var cause = new Cause
        {
            Id = 1,
            Title = "Test Cause",
            Slug = "test-cause",
            IsPublished = true,
            GoalAmount = 1000
        };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("test-cause"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        var result = await _controller.Details("test-cause");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Cause>(viewResult.Model);
        Assert.Equal("Test Cause", model.Title);
    }

    [Fact]
    public async Task Details_PopulatesViewBagWithSimilarCauses()
    {
        // Arrange
        var cause = new Cause { Id = 1, Title = "Main", Slug = "main", IsPublished = true, GoalAmount = 1000 };
        var similarCauses = new List<Cause>
        {
            new Cause { Id = 2, Title = "Similar 1", Slug = "similar-1", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 3, Title = "Similar 2", Slug = "similar-2", IsPublished = true, GoalAmount = 1000 }
        };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("main"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(1, 4))
            .ReturnsAsync(similarCauses);
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Details("main");

        // Assert
        Assert.NotNull(_controller.ViewBag.SimilarCauses);
        var viewBagCauses = _controller.ViewBag.SimilarCauses as IEnumerable<Cause>;
        Assert.Equal(2, viewBagCauses.Count());
    }

    [Fact]
    public async Task Details_PopulatesViewBagWithFeaturedCauses()
    {
        // Arrange
        var cause = new Cause { Id = 1, Title = "Main", Slug = "main", IsPublished = true, GoalAmount = 1000 };
        var featuredCauses = new List<Cause>
        {
            new Cause { Id = 2, Title = "Featured 1", Slug = "featured-1", IsPublished = true, IsFeatured = true, GoalAmount = 1000 }
        };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("main"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(3))
            .ReturnsAsync(featuredCauses);

        // Act
        await _controller.Details("main");

        // Assert
        Assert.NotNull(_controller.ViewBag.FeaturedCauses);
        var viewBagCauses = _controller.ViewBag.FeaturedCauses as IEnumerable<Cause>;
        Assert.Single(viewBagCauses);
    }

    [Fact]
    public async Task Details_CallsGetSimilarCausesAsync_WithCorrectParameters()
    {
        // Arrange
        var cause = new Cause { Id = 5, Title = "Main", Slug = "main", IsPublished = true, GoalAmount = 1000 };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("main"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Details("main");

        // Assert
        _mockCauseService.Verify(s => s.GetSimilarCausesAsync(5, 4), Times.Once);
    }

    [Fact]
    public async Task Details_CallsGetFeaturedCausesAsync_WithCorrectParameters()
    {
        // Arrange
        var cause = new Cause { Id = 1, Title = "Main", Slug = "main", IsPublished = true, GoalAmount = 1000 };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("main"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Details("main");

        // Assert
        _mockCauseService.Verify(s => s.GetFeaturedCausesAsync(3), Times.Once);
    }

    [Fact]
    public async Task Details_CallsGetCauseBySlugAsync_WithCorrectSlug()
    {
        // Arrange
        var cause = new Cause { Id = 1, Title = "Test", Slug = "test-slug", IsPublished = true, GoalAmount = 1000 };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("test-slug"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Details("test-slug");

        // Assert
        _mockCauseService.Verify(s => s.GetCauseBySlugAsync("test-slug"), Times.Once);
    }

    [Fact]
    public async Task Details_HandlesEmptySimilarCauses()
    {
        // Arrange
        var cause = new Cause { Id = 1, Title = "Main", Slug = "main", IsPublished = true, GoalAmount = 1000 };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("main"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        var result = await _controller.Details("main");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    [Fact]
    public async Task Details_HandlesEmptyFeaturedCauses()
    {
        // Arrange
        var cause = new Cause { Id = 1, Title = "Main", Slug = "main", IsPublished = true, GoalAmount = 1000 };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("main"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Cause>());

        // Act
        var result = await _controller.Details("main");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    #endregion

    #region Grid Action Tests

    [Fact]
    public async Task Grid_ReturnsViewWithActiveCauses()
    {
        // Arrange
        var causes = new List<Cause>
        {
            new Cause { Id = 1, Title = "Cause 1", Slug = "cause-1", IsPublished = true, GoalAmount = 1000 },
            new Cause { Id = 2, Title = "Cause 2", Slug = "cause-2", IsPublished = true, GoalAmount = 2000 }
        };

        _mockCauseService.Setup(s => s.GetActiveCausesAsync())
            .ReturnsAsync(causes);

        // Act
        var result = await _controller.Grid();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Cause>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Grid_CallsGetActiveCausesAsync()
    {
        // Arrange
        _mockCauseService.Setup(s => s.GetActiveCausesAsync())
            .ReturnsAsync(new List<Cause>());

        // Act
        await _controller.Grid();

        // Assert
        _mockCauseService.Verify(s => s.GetActiveCausesAsync(), Times.Once);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Details_IntegrationTest_CompleteWorkflow()
    {
        // Arrange
        var cause = new Cause
        {
            Id = 1,
            Title = "Complete Test",
            Slug = "complete-test",
            Description = "Full test description",
            Category = "Water",
            IsPublished = true,
            GoalAmount = 5000,
            RaisedAmount = 3000
        };

        var similarCauses = new List<Cause>
        {
            new Cause { Id = 2, Title = "Similar", Slug = "similar", Category = "Water", IsPublished = true, GoalAmount = 1000 }
        };

        var featuredCauses = new List<Cause>
        {
            new Cause { Id = 3, Title = "Featured", Slug = "featured", IsFeatured = true, IsPublished = true, GoalAmount = 2000 }
        };

        _mockCauseService.Setup(s => s.GetCauseBySlugAsync("complete-test"))
            .ReturnsAsync(cause);
        _mockCauseService.Setup(s => s.GetSimilarCausesAsync(1, 4))
            .ReturnsAsync(similarCauses);
        _mockCauseService.Setup(s => s.GetFeaturedCausesAsync(3))
            .ReturnsAsync(featuredCauses);

        // Act
        var result = await _controller.Details("complete-test");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Cause>(viewResult.Model);
        
        Assert.Equal("Complete Test", model.Title);
        Assert.Equal(60, model.ProgressPercentage); // 3000/5000 = 60%
        Assert.NotNull(_controller.ViewBag.SimilarCauses);
        Assert.NotNull(_controller.ViewBag.FeaturedCauses);
        
        _mockCauseService.Verify(s => s.GetCauseBySlugAsync("complete-test"), Times.Once);
        _mockCauseService.Verify(s => s.GetSimilarCausesAsync(1, 4), Times.Once);
        _mockCauseService.Verify(s => s.GetFeaturedCausesAsync(3), Times.Once);
    }

    [Fact]
    public async Task List_IntegrationTest_SearchWorkflow()
    {
        // Arrange
        var searchResults = new List<Cause>
        {
            new Cause { Id = 1, Title = "Clean Water", Slug = "clean-water", Description = "Provide water", IsPublished = true, GoalAmount = 1000 }
        };

        _mockCauseService.Setup(s => s.SearchCausesAsync("water"))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.Index("water");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Cause>>(viewResult.Model);
        
        Assert.Single(model);
        Assert.Equal("water", _controller.ViewBag.SearchTerm);
        Assert.Equal("Clean Water", model.First().Title);
        
        _mockCauseService.Verify(s => s.SearchCausesAsync("water"), Times.Once);
    }

    #endregion
}
