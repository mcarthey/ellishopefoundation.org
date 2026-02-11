using EllisHope.Controllers;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<ITestimonialService> _testimonialServiceMock = new();

    public HomeControllerTests()
    {
        _testimonialServiceMock
            .Setup(s => s.GetFeaturedTestimonialsAsync(It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<Testimonial>());
    }

    private HomeController CreateController() => new(_testimonialServiceMock.Object);

    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    [Fact]
    public void Index2_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index2();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Index3_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index3();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Index4_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index4();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Index5_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index5();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Index6_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index6();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Index7_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index7();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Index8_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index8();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Controller_CanBeInstantiated()
    {
        // Act
        var controller = CreateController();

        // Assert
        Assert.NotNull(controller);
    }

    [Theory]
    [InlineData("Index")]
    [InlineData("Index2")]
    [InlineData("Index3")]
    [InlineData("Index4")]
    [InlineData("Index5")]
    [InlineData("Index6")]
    [InlineData("Index7")]
    [InlineData("Index8")]
    public async Task AllIndexActions_ReturnViewResults(string actionName)
    {
        // Arrange
        var controller = CreateController();
        var method = controller.GetType().GetMethod(actionName);

        // Act
        var result = method?.Invoke(controller, null);

        // Unwrap async results
        if (result is Task<IActionResult> taskResult)
        {
            result = await taskResult;
        }

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ViewResult>(result);
    }
}
