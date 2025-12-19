using EllisHope.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EllisHope.Tests.Controllers;

public class ContactControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Arrange
        var controller = new ContactController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    [Fact]
    public void v2_ReturnsViewResult()
    {
        // Arrange
        var controller = new ContactController();

        // Act
        var result = controller.v2();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    [Fact]
    public void Controller_CanBeInstantiated()
    {
        // Act
        var controller = new ContactController();

        // Assert
        Assert.NotNull(controller);
    }
}
