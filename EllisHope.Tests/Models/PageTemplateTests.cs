using EllisHope.Models.Domain;
using Xunit;

namespace EllisHope.Tests.Domain;

public class ImageRequirementsTests
{
    [Fact]
    public void DisplayText_FormatsCorrectly()
    {
        // Arrange
        var requirements = new ImageRequirements
        {
            RecommendedWidth = 1800,
            RecommendedHeight = 600
        };

        // Act
        var displayText = requirements.DisplayText;

        // Assert
        Assert.Equal("1800×600px", displayText);
    }

    [Fact]
    public void AspectRatioDecimal_Returns16_9Correctly()
    {
        // Arrange
        var requirements = new ImageRequirements
        {
            AspectRatio = "16:9"
        };

        // Act
        var decimalValue = requirements.AspectRatioDecimal;

        // Assert
        Assert.Equal("1.78", decimalValue);
    }

    [Fact]
    public void AspectRatioDecimal_Returns3_1Correctly()
    {
        // Arrange
        var requirements = new ImageRequirements
        {
            AspectRatio = "3:1"
        };

        // Act
        var decimalValue = requirements.AspectRatioDecimal;

        // Assert
        Assert.Equal("3.00", decimalValue);
    }

    [Fact]
    public void AspectRatioDecimal_ReturnsFlexibleForUnknown()
    {
        // Arrange
        var requirements = new ImageRequirements
        {
            AspectRatio = "custom"
        };

        // Act
        var decimalValue = requirements.AspectRatioDecimal;

        // Assert
        Assert.Equal("flexible", decimalValue);
    }
}

public class EditableImageTests
{
    [Fact]
    public void EffectiveImagePath_ReturnsCurrentImagePath_WhenSet()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentImagePath = "/uploads/custom.jpg",
            CurrentTemplatePath = "/assets/template.jpg",
            FallbackPath = "/assets/fallback.jpg"
        };

        // Act
        var effectivePath = image.EffectiveImagePath;

        // Assert
        Assert.Equal("/uploads/custom.jpg", effectivePath);
    }

    [Fact]
    public void EffectiveImagePath_ReturnsTemplatePath_WhenCurrentNotSet()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentImagePath = null,
            CurrentTemplatePath = "/assets/template.jpg",
            FallbackPath = "/assets/fallback.jpg"
        };

        // Act
        var effectivePath = image.EffectiveImagePath;

        // Assert
        Assert.Equal("/assets/template.jpg", effectivePath);
    }

    [Fact]
    public void EffectiveImagePath_ReturnsFallback_WhenNeitherSet()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentImagePath = null,
            CurrentTemplatePath = null,
            FallbackPath = "/assets/fallback.jpg"
        };

        // Act
        var effectivePath = image.EffectiveImagePath;

        // Assert
        Assert.Equal("/assets/fallback.jpg", effectivePath);
    }

    [Fact]
    public void EffectiveImagePath_ReturnsDefault_WhenNoneSet()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentImagePath = null,
            CurrentTemplatePath = null,
            FallbackPath = null
        };

        // Act
        var effectivePath = image.EffectiveImagePath;

        // Assert
        Assert.Equal("/assets/img/default.jpg", effectivePath);
    }

    [Fact]
    public void ImageSource_ReturnsMediaLibrary_WhenManaged()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentImagePath = "/uploads/custom.jpg"
        };

        // Act
        var source = image.ImageSource;

        // Assert
        Assert.Equal("Media Library", source);
    }

    [Fact]
    public void ImageSource_ReturnsTemplate_WhenNotManaged()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentImagePath = null,
            CurrentTemplatePath = "/assets/template.jpg"
        };

        // Act
        var source = image.ImageSource;

        // Assert
        Assert.Equal("Template (not managed)", source);
    }

    [Fact]
    public void ImageSource_ReturnsNoImage_WhenNoneSet()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentImagePath = null,
            CurrentTemplatePath = null
        };

        // Act
        var source = image.ImageSource;

        // Assert
        Assert.Equal("No image", source);
    }

    [Fact]
    public void IsManagedImage_ReturnsTrue_WhenMediaIdSet()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentMediaId = 1
        };

        // Act
        var isManaged = image.IsManagedImage;

        // Assert
        Assert.True(isManaged);
    }

    [Fact]
    public void IsManagedImage_ReturnsFalse_WhenMediaIdNotSet()
    {
        // Arrange
        var image = new EditableImage
        {
            CurrentMediaId = null
        };

        // Act
        var isManaged = image.IsManagedImage;

        // Assert
        Assert.False(isManaged);
    }

    [Fact]
    public void SizeGuidance_ReturnsLandscapeGuidance()
    {
        // Arrange
        var image = new EditableImage
        {
            Requirements = new ImageRequirements
            {
                RecommendedWidth = 1800,
                RecommendedHeight = 600,
                AspectRatio = "3:1",
                Orientation = "Landscape"
            }
        };

        // Act
        var guidance = image.SizeGuidance;

        // Assert
        Assert.Contains("landscape", guidance, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1800×600px", guidance);
        Assert.Contains("3:1", guidance);
    }

    [Fact]
    public void SizeGuidance_ReturnsPortraitGuidance()
    {
        // Arrange
        var image = new EditableImage
        {
            Requirements = new ImageRequirements
            {
                RecommendedWidth = 600,
                RecommendedHeight = 800,
                AspectRatio = "3:4",
                Orientation = "Portrait"
            }
        };

        // Act
        var guidance = image.SizeGuidance;

        // Assert
        Assert.Contains("portrait", guidance, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("600×800px", guidance);
    }

    [Fact]
    public void SizeGuidance_ReturnsSquareGuidance()
    {
        // Arrange
        var image = new EditableImage
        {
            Requirements = new ImageRequirements
            {
                RecommendedWidth = 800,
                RecommendedHeight = 800,
                AspectRatio = "1:1",
                Orientation = "Square"
            }
        };

        // Act
        var guidance = image.SizeGuidance;

        // Assert
        Assert.Contains("square", guidance, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("800×800px", guidance);
    }
}
