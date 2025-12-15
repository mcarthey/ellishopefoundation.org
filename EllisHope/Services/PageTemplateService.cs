using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Provides page-specific templates that define what content can be edited
/// This makes the page manager user-friendly by showing real labels like
/// "Hero Banner Image" instead of technical "ImageKey: HeroBanner"
/// </summary>
public interface IPageTemplateService
{
    PageTemplate GetPageTemplate(string pageName);
    List<string> GetAvailablePages();
}

public class PageTemplateService : IPageTemplateService
{
    public PageTemplate GetPageTemplate(string pageName)
    {
        return pageName.ToLower() switch
        {
            "home" => GetHomePageTemplate(),
            "about" => GetAboutPageTemplate(),
            "team" => GetTeamPageTemplate(),
            "services" => GetServicesPageTemplate(),
            "contact" => GetContactPageTemplate(),
            _ => GetGenericPageTemplate(pageName)
        };
    }

    public List<string> GetAvailablePages()
    {
        return new List<string> { "Home", "About", "Team", "Services", "Contact" };
    }

    private PageTemplate GetHomePageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Home",
            DisplayName = "Home Page",
            Description = "Main landing page - hero section, services preview, and featured causes",
            Images = new List<EditableImage>
            {
                new() { 
                    Key = "HeroBanner", 
                    Label = "Hero Banner Image", 
                    Description = "Main banner image at top of home page (recommended: 1800x600px)",
                    CurrentTemplatePath = "/assets/img/hero/hero-img-6-unsplash.png",
                    FallbackPath = "/assets/img/hero/hero-img-6-unsplash.png"
                },
                new() { 
                    Key = "AboutImage", 
                    Label = "About Section Image", 
                    Description = "Image for the 'About Ellis Hope' section (recommended: 600x800px)",
                    CurrentTemplatePath = "/assets/img/media/about-foundation-663x839.jpg",
                    FallbackPath = "/assets/img/media/about-foundation-663x839.jpg"
                },
                new() { 
                    Key = "CTABackground", 
                    Label = "Call-to-Action Background", 
                    Description = "Background image for the 'Get Involved' section (recommended: 1800x600px)",
                    CurrentTemplatePath = "/assets/img/bg/make-difference-1800x855.jpg",
                    FallbackPath = "/assets/img/bg/make-difference-1800x855.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() { 
                    Key = "HeroTitle", 
                    Label = "Hero Title", 
                    Description = "Main headline at top of page", 
                    ContentType = "Text", 
                    MaxLength = 100,
                    CurrentTemplateValue = "Empowering Health, Fitness, and Hope"
                },
                new() { 
                    Key = "HeroSubtitle", 
                    Label = "Hero Subtitle", 
                    Description = "Supporting text under main headline", 
                    ContentType = "Text", 
                    MaxLength = 300,
                    CurrentTemplateValue = "At Ellis Hope Foundation, we believe that every individual deserves the chance to pursue a healthier and more fulfilling life. Together, we turn dreams into reality."
                },
                new() { 
                    Key = "ServicesIntro", 
                    Label = "Services Introduction", 
                    Description = "Brief text introducing the services section", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>Our Services</h2><p>What We Do</p>"
                },
                new() { 
                    Key = "AboutSummary", 
                    Label = "About Summary", 
                    Description = "Short description of Ellis Hope Foundation", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>About Ellis Hope Foundation</h2><p>We are a community-driven organization dedicated to empowering individuals through fitness, education, and support. We believe in the resilience of the human spirit and the power of hope.</p>"
                },
                new() { 
                    Key = "CTAText", 
                    Label = "Call-to-Action Message", 
                    Description = "Encouraging message to get involved", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>Ready to Make a Difference?</h2><p>Your contribution helps change lives through health, fitness, and hope.</p>"
                }
            }
        };
    }

    private PageTemplate GetAboutPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "About",
            DisplayName = "About Us Page",
            Description = "Information about Ellis Hope Foundation - mission, vision, impact",
            Images = new List<EditableImage>
            {
                new() { 
                    Key = "HeaderBanner", 
                    Label = "Page Header Banner", 
                    Description = "Banner image at top of About page",
                    CurrentTemplatePath = "/assets/img/page-title/about-us-1800x540.jpg",
                    FallbackPath = "/assets/img/page-title/about-us-1800x540.jpg"
                },
                new() { 
                    Key = "MissionImage", 
                    Label = "Mission Section Image", 
                    Description = "Image accompanying mission statement",
                    CurrentTemplatePath = "/assets/img/media/gift-empowerment-587x695.jpg",
                    FallbackPath = "/assets/img/media/gift-empowerment-587x695.jpg"
                },
                new() { 
                    Key = "TeamPhoto", 
                    Label = "Team Photo", 
                    Description = "Group photo of team/volunteers",
                    CurrentTemplatePath = "/assets/img/media/media-10.jpg",
                    FallbackPath = "/assets/img/media/media-10.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() { 
                    Key = "MissionStatement", 
                    Label = "Mission Statement", 
                    Description = "Our mission and why we exist", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>Our Mission</h2><p>Empowering individuals through fitness, education, and community support.</p>"
                },
                new() { 
                    Key = "VisionStatement", 
                    Label = "Vision Statement", 
                    Description = "Our vision for the future", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>Our Vision</h2><p>A healthier, more empowered community where everyone has access to wellness resources.</p>"
                },
                new() { 
                    Key = "ImpactStats", 
                    Label = "Impact Statistics", 
                    Description = "Key achievements and impact numbers", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>102k+ Volunteers | 90% Success Rate | 30k Monthly Donors</p>"
                },
                new() { 
                    Key = "History", 
                    Label = "Our History", 
                    Description = "Story of how Ellis Hope was founded", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>Founded with a vision to make health and fitness accessible to all...</p>"
                }
            }
        };
    }

    private PageTemplate GetTeamPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Team",
            DisplayName = "Team & Volunteers Page",
            Description = "Meet our team members and volunteers",
            Images = new List<EditableImage>
            {
                new() { 
                    Key = "HeaderBanner", 
                    Label = "Page Header Banner", 
                    Description = "Banner image at top of Team page",
                    CurrentTemplatePath = "/assets/img/page-title/page-title-bg.jpg",
                    FallbackPath = "/assets/img/page-title/page-title-bg.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() { 
                    Key = "PageIntro", 
                    Label = "Page Introduction", 
                    Description = "Welcome message for team page", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>Meet Our Team</h2><p>Dedicated volunteers working to make a difference.</p>"
                },
                new() { 
                    Key = "VolunteerInfo", 
                    Label = "Volunteer Information", 
                    Description = "How to become a volunteer", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>Interested in volunteering? Contact us to learn more about opportunities.</p>"
                },
                new() { 
                    Key = "TestimonialQuote", 
                    Label = "Volunteer Testimonial", 
                    Description = "Quote from a current volunteer", 
                    ContentType = "Text", 
                    MaxLength = 500,
                    CurrentTemplateValue = "I'm always eager to participate in charity events they've offered me invaluable experiences."
                },
                new() { 
                    Key = "TestimonialAuthor", 
                    Label = "Testimonial Author", 
                    Description = "Name and role of testimonial author", 
                    ContentType = "Text", 
                    MaxLength = 100,
                    CurrentTemplateValue = "A Grateful Volunteer"
                }
            }
        };
    }

    private PageTemplate GetServicesPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Services",
            DisplayName = "Services Page",
            Description = "Programs and services offered by Ellis Hope",
            Images = new List<EditableImage>
            {
                new() { 
                    Key = "HeaderBanner", 
                    Label = "Page Header Banner", 
                    Description = "Banner image at top of Services page",
                    CurrentTemplatePath = "/assets/img/page-title/our-service-1800x540.jpg",
                    FallbackPath = "/assets/img/page-title/our-service-1800x540.jpg"
                },
                new() { 
                    Key = "Service1Icon", 
                    Label = "Service 1 Icon/Image", 
                    Description = "Icon or image for first service",
                    CurrentTemplatePath = "/assets/img/icon/icon-28.svg",
                    FallbackPath = "/assets/img/icon/icon-28.svg"
                },
                new() { 
                    Key = "Service2Icon", 
                    Label = "Service 2 Icon/Image", 
                    Description = "Icon or image for second service",
                    CurrentTemplatePath = "/assets/img/icon/icon-29.svg",
                    FallbackPath = "/assets/img/icon/icon-29.svg"
                },
                new() { 
                    Key = "Service3Icon", 
                    Label = "Service 3 Icon/Image", 
                    Description = "Icon or image for third service",
                    CurrentTemplatePath = "/assets/img/icon/icon-30.svg",
                    FallbackPath = "/assets/img/icon/icon-30.svg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() { 
                    Key = "PageIntro", 
                    Label = "Page Introduction", 
                    Description = "Introduction to our services", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>What We Do</h2><p>Enrich Lives offer Hope Inspire Change</p>"
                },
                new() { 
                    Key = "Service1Title", 
                    Label = "Service 1 Title", 
                    Description = "Name of first service", 
                    ContentType = "Text", 
                    MaxLength = 100,
                    CurrentTemplateValue = "Liquid life unveiled"
                },
                new() { 
                    Key = "Service1Description", 
                    Label = "Service 1 Description", 
                    Description = "Description of first service", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>Become a member.</p>"
                },
                new() { 
                    Key = "Service2Title", 
                    Label = "Service 2 Title", 
                    Description = "Name of second service", 
                    ContentType = "Text", 
                    MaxLength = 100,
                    CurrentTemplateValue = "Health tech revolution"
                },
                new() { 
                    Key = "Service2Description", 
                    Label = "Service 2 Description", 
                    Description = "Description of second service", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>Become a member..</p>"
                },
                new() { 
                    Key = "Service3Title", 
                    Label = "Service 3 Title", 
                    Description = "Name of third service", 
                    ContentType = "Text", 
                    MaxLength = 100,
                    CurrentTemplateValue = "Educate to elevate"
                },
                new() { 
                    Key = "Service3Description", 
                    Label = "Service 3 Description", 
                    Description = "Description of third service", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>Become a member.</p>"
                }
            }
        };
    }

    private PageTemplate GetContactPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Contact",
            DisplayName = "Contact Us Page",
            Description = "Contact information and inquiry form",
            Images = new List<EditableImage>
            {
                new() { 
                    Key = "HeaderBanner", 
                    Label = "Page Header Banner", 
                    Description = "Banner image at top of Contact page",
                    CurrentTemplatePath = "/assets/img/page-title/page-title-bg.jpg",
                    FallbackPath = "/assets/img/page-title/page-title-bg.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() { 
                    Key = "PageIntro", 
                    Label = "Page Introduction", 
                    Description = "Welcome message on contact page", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<h2>Contact</h2><p>Get in touch with us.</p>"
                },
                new() { 
                    Key = "OfficeHours", 
                    Label = "Office Hours", 
                    Description = "When the office is open", 
                    ContentType = "Text", 
                    MaxLength = 200,
                    CurrentTemplateValue = "Monday - Friday: 9:00 AM - 5:00 PM"
                },
                new() { 
                    Key = "AdditionalInfo", 
                    Label = "Additional Information", 
                    Description = "Any other contact-related information", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>We look forward to hearing from you!</p>"
                }
            }
        };
    }

    private PageTemplate GetGenericPageTemplate(string pageName)
    {
        return new PageTemplate
        {
            PageName = pageName,
            DisplayName = $"{pageName} Page",
            Description = $"Content for {pageName} page",
            Images = new List<EditableImage>
            {
                new() { 
                    Key = "HeaderBanner", 
                    Label = "Page Header Banner", 
                    Description = "Banner image at top of page",
                    CurrentTemplatePath = "/assets/img/page-title/page-title-bg.jpg",
                    FallbackPath = "/assets/img/page-title/page-title-bg.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() { 
                    Key = "PageContent", 
                    Label = "Page Content", 
                    Description = "Main content for this page", 
                    ContentType = "RichText",
                    CurrentTemplateValue = "<p>Page content goes here.</p>"
                }
            }
        };
    }
}
