using EllisHope.Controllers;
using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EllisHope.Tests.Controllers;

/// <summary>
/// Tests for simple public controllers that only return views
/// </summary>
public class PublicControllersTests
{
    // AboutController tests moved to AboutControllerTests.cs for comprehensive coverage

    #region ErrorController Tests

    private static ErrorController CreateErrorController()
    {
        var environmentMock = new Mock<IWebHostEnvironment>();
        environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Foundation:Email"]).Returns("test@example.com");

        var controller = new ErrorController(environmentMock.Object, configurationMock.Object);

        // Set up HttpContext for the controller
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public void ErrorController_Index_ReturnsView()
    {
        // Arrange
        var controller = CreateErrorController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public void ErrorController_CanInstantiate()
    {
        // Act & Assert
        var controller = CreateErrorController();
        Assert.NotNull(controller);
    }

    #endregion

    #region FaqController Tests

    [Fact]
    public void FaqController_Index_ReturnsView()
    {
        // Arrange
        var controller = new FaqController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    [Fact]
    public void FaqController_CanInstantiate()
    {
        // Act & Assert
        var controller = new FaqController();
        Assert.NotNull(controller);
    }

    #endregion

    #region ServicesController Tests

    [Fact]
    public void ServicesController_Index_ReturnsView()
    {
        // Arrange
        var controller = new ServicesController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    [Fact]
    public void ServicesController_V2_ReturnsView()
    {
        // Arrange
        var controller = new ServicesController();

        // Act
        var result = controller.v2();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Default view
    }

    [Fact]
    public void ServicesController_CanInstantiate()
    {
        // Act & Assert
        var controller = new ServicesController();
        Assert.NotNull(controller);
    }

    #endregion

    // TeamController tests moved to TeamControllerTests.cs for comprehensive coverage
}
