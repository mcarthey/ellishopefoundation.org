using EllisHope.Models.Domain;
using EllisHope.Services;
using Xunit;

namespace EllisHope.Tests.Unit;

/// <summary>
/// Unit tests for PageTemplateService
/// Tests template retrieval and configuration
/// </summary>
public class PageTemplateServiceTests
{
    private readonly PageTemplateService _service;

    public PageTemplateServiceTests()
    {
        _service = new PageTemplateService();
    }

    #region GetPageTemplate Tests

    [Theory]
    [InlineData("Home")]
    [InlineData("About")]
    [InlineData("Team")]
    [InlineData("Services")]
    [InlineData("Contact")]
    public void GetPageTemplate_WithValidPage_ReturnsTemplate(string pageName)
    {
        // Act
        var template = _service.GetPageTemplate(pageName);

        // Assert
        Assert.NotNull(template);
        Assert.Equal(pageName, template.PageName, ignoreCase: true);
        Assert.NotNull(template.DisplayName);
        Assert.NotNull(template.Description);
    }

    [Fact]
    public void GetPageTemplate_Home_HasCorrectImages()
    {
        // Act
        var template = _service.GetPageTemplate("Home");

        // Assert
        Assert.NotNull(template.Images);
        Assert.Contains(template.Images, i => i.Key == "HeroImage");
        Assert.Contains(template.Images, i => i.Key == "AboutImage");
        Assert.Contains(template.Images, i => i.Key == "CTABackground");
    }

    [Fact]
    public void GetPageTemplate_Home_HasCorrectContent()
    {
        // Act
        var template = _service.GetPageTemplate("Home");

        // Assert
        Assert.NotNull(template.ContentAreas);
        Assert.Contains(template.ContentAreas, c => c.Key == "HeroTitle");
        Assert.Contains(template.ContentAreas, c => c.Key == "HeroSubtitle");
        Assert.Contains(template.ContentAreas, c => c.Key == "Service1Title");
    }

    [Fact]
    public void GetPageTemplate_About_HasHeaderBanner()
    {
        // Act
        var template = _service.GetPageTemplate("About");

        // Assert
        Assert.Contains(template.Images, i => i.Key == "HeaderBanner");
    }

    [Fact]
    public void GetPageTemplate_Services_HasHeaderBanner()
    {
        // Act
        var template = _service.GetPageTemplate("Services");

        // Assert
        Assert.Contains(template.Images, i => i.Key == "HeaderBanner");
    }

    [Fact]
    public void GetPageTemplate_WithUnknownPage_ReturnsGenericTemplate()
    {
        // Act
        var template = _service.GetPageTemplate("UnknownPage");

        // Assert
        Assert.NotNull(template);
        Assert.Equal("UnknownPage", template.PageName);
        Assert.Contains("UnknownPage", template.DisplayName);
    }

    [Fact]
    public void GetPageTemplate_IsCaseInsensitive()
    {
        // Act
        var lowerCase = _service.GetPageTemplate("home");
        var upperCase = _service.GetPageTemplate("HOME");
        var mixedCase = _service.GetPageTemplate("HoMe");

        // Assert
        Assert.NotNull(lowerCase);
        Assert.NotNull(upperCase);
        Assert.NotNull(mixedCase);
        Assert.Equal(lowerCase.PageName, upperCase.PageName, ignoreCase: true);
        Assert.Equal(lowerCase.PageName, mixedCase.PageName, ignoreCase: true);
    }

    #endregion

    #region GetAvailablePages Tests

    [Fact]
    public void GetAvailablePages_ReturnsExpectedPages()
    {
        // Act
        var pages = _service.GetAvailablePages();

        // Assert
        Assert.NotNull(pages);
        Assert.NotEmpty(pages);
        Assert.Contains("Home", pages);
        Assert.Contains("About", pages);
        Assert.Contains("Team", pages);
        Assert.Contains("Services", pages);
        Assert.Contains("Contact", pages);
    }

    [Fact]
    public void GetAvailablePages_ReturnsCorrectCount()
    {
        // Act
        var pages = _service.GetAvailablePages();

        // Assert
        Assert.Equal(5, pages.Count);
    }

    #endregion

    #region Image Requirements Tests

    [Fact]
    public void GetPageTemplate_HeroImage_HasValidRequirements()
    {
        // Act
        var template = _service.GetPageTemplate("Home");
        var heroImage = template.Images.FirstOrDefault(i => i.Key == "HeroImage");

        // Assert
        Assert.NotNull(heroImage);
        // Note: Requirements may be null if not specified on template
        if (heroImage.Requirements != null)
        {
            Assert.True(heroImage.Requirements.RecommendedWidth > 0);
            Assert.True(heroImage.Requirements.RecommendedHeight > 0);
        }
    }

    [Fact]
    public void GetPageTemplate_AllImages_HaveFallbackPaths()
    {
        // Arrange
        var pageNames = new[] { "Home", "About", "Team", "Services", "Contact" };

        foreach (var pageName in pageNames)
        {
            // Act
            var template = _service.GetPageTemplate(pageName);

            // Assert
            foreach (var image in template.Images)
            {
                Assert.NotNull(image.FallbackPath);
                Assert.NotEmpty(image.FallbackPath);
            }
        }
    }

    #endregion

    #region Content Area Tests

    [Fact]
    public void GetPageTemplate_TextContent_HasMaxLength()
    {
        // Act
        var template = _service.GetPageTemplate("Home");
        var heroTitle = template.ContentAreas.FirstOrDefault(c => c.Key == "HeroTitle");

        // Assert
        Assert.NotNull(heroTitle);
        Assert.Equal("Text", heroTitle.ContentType);
        Assert.True(heroTitle.MaxLength > 0);
    }

    [Fact]
    public void GetPageTemplate_RichTextContent_HasNoMaxLengthOrZero()
    {
        // Act - Team page has RichText content (bio fields)
        var template = _service.GetPageTemplate("Team");
        var richText = template.ContentAreas.FirstOrDefault(c => c.ContentType == "RichText");

        // Assert
        Assert.NotNull(richText);
        // RichText may have MaxLength of 0 or null (both indicate no limit)
        Assert.True(richText.MaxLength == null || richText.MaxLength == 0);
    }

    [Fact]
    public void GetPageTemplate_AllContent_HasTemplateValues()
    {
        // Arrange
        var pageNames = new[] { "Home", "About", "Team", "Services", "Contact" };

        foreach (var pageName in pageNames)
        {
            // Act
            var template = _service.GetPageTemplate(pageName);

            // Assert
            foreach (var content in template.ContentAreas)
            {
                Assert.NotNull(content.CurrentTemplateValue);
                Assert.NotEmpty(content.CurrentTemplateValue);
            }
        }
    }

    #endregion

    #region Template Structure Tests

    [Theory]
    [InlineData("Home", 3)] // HeroImage, AboutImage, CTABackground
    [InlineData("About", 2)] // HeaderBanner, AboutImage
    [InlineData("Team", 1)] // HeaderBanner
    [InlineData("Services", 1)] // HeaderBanner
    [InlineData("Contact", 1)] // HeaderBanner
    public void GetPageTemplate_HasExpectedImageCount(string pageName, int expectedCount)
    {
        // Act
        var template = _service.GetPageTemplate(pageName);

        // Assert
        Assert.Equal(expectedCount, template.Images.Count);
    }

    [Fact]
    public void GetPageTemplate_Home_HasExpectedContentCount()
    {
        // Act
        var template = _service.GetPageTemplate("Home");

        // Assert
        Assert.Equal(17, template.ContentAreas.Count); // Hero, Testimonial, Services, Initiatives, About, CTA sections
    }

    [Fact]
    public void GetPageTemplate_AllTemplates_HaveUniqueKeys()
    {
        // Arrange
        var pageNames = new[] { "Home", "About", "Team", "Services", "Contact" };

        foreach (var pageName in pageNames)
        {
            // Act
            var template = _service.GetPageTemplate(pageName);

            // Assert - Image keys should be unique
            var imageKeys = template.Images.Select(i => i.Key).ToList();
            Assert.Equal(imageKeys.Count, imageKeys.Distinct().Count());

            // Assert - Content keys should be unique
            var contentKeys = template.ContentAreas.Select(c => c.Key).ToList();
            Assert.Equal(contentKeys.Count, contentKeys.Distinct().Count());
        }
    }

    #endregion
}
