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
                    Description = "Main banner image at top of home page",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 600,
                        MinWidth = 1200,
                        MinHeight = 400,
                        MaxWidth = 2400,
                        MaxHeight = 800,
                        AspectRatio = "3:1",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 5 * 1024 * 1024 // 5MB
                    },
                    CurrentTemplatePath = "/assets/img/hero/hero-img-6-unsplash.png",
                    FallbackPath = "/assets/img/hero/hero-img-6-unsplash.png"
                },
                new() { 
                    Key = "AboutImage", 
                    Label = "About Section Image", 
                    Description = "Image for the 'About Ellis Hope' section",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 663,
                        RecommendedHeight = 839,
                        MinWidth = 500,
                        MinHeight = 600,
                        MaxWidth = 1000,
                        MaxHeight = 1200,
                        AspectRatio = "2:3",
                        Orientation = "Portrait",
                        MaxFileSizeBytes = 3 * 1024 * 1024 // 3MB
                    },
                    CurrentTemplatePath = "/assets/img/media/about-foundation-663x839.jpg",
                    FallbackPath = "/assets/img/media/about-foundation-663x839.jpg"
                },
                new() { 
                    Key = "CTABackground", 
                    Label = "Call-to-Action Background", 
                    Description = "Background image for the 'Get Involved' section",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 855,
                        MinWidth = 1200,
                        MinHeight = 500,
                        MaxWidth = 2400,
                        MaxHeight = 1200,
                        AspectRatio = "16:9",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 5 * 1024 * 1024 // 5MB
                    },
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
            Description = "Information about Ellis Hope Foundation - mission, vision, impact, and team",
            Images = new List<EditableImage>
            {
                new() {
                    Key = "HeaderBanner",
                    Label = "Page Header Banner",
                    Description = "Banner image at top of About page (breadcrumb area)",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 540,
                        MinWidth = 1200,
                        MinHeight = 360,
                        MaxWidth = 2400,
                        MaxHeight = 720,
                        AspectRatio = "10:3",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 4 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/page-title/about-us-1800x540.jpg",
                    FallbackPath = "/assets/img/page-title/about-us-1800x540.jpg"
                },
                new() {
                    Key = "AboutSectionImage",
                    Label = "About Section Image",
                    Description = "Main image in the 'About Us' section (portrait orientation)",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 587,
                        RecommendedHeight = 695,
                        MinWidth = 400,
                        MinHeight = 500,
                        MaxWidth = 800,
                        MaxHeight = 1000,
                        AspectRatio = "4:5",
                        Orientation = "Portrait",
                        MaxFileSizeBytes = 3 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/media/gift-empowerment-587x695.jpg",
                    FallbackPath = "/assets/img/media/gift-empowerment-587x695.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                // About Section (first major section)
                new() {
                    Key = "AboutTitle",
                    Label = "About Section Title",
                    Description = "Main title in the about section (e.g., 'Gifts Elevate and Enrich Lives')",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Gifts Elevate and Enrich Lives."
                },
                new() {
                    Key = "AboutSubtitle",
                    Label = "About Section Subtitle",
                    Description = "Subtitle text under the main title",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Our main purpose revolves around empowering individuals, fostering growth."
                },
                new() {
                    Key = "AboutFeature1",
                    Label = "Feature Point 1",
                    Description = "First bullet point in about section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Contributions foster growth."
                },
                new() {
                    Key = "AboutFeature2",
                    Label = "Feature Point 2",
                    Description = "Second bullet point in about section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Supportive acts nurture human potential."
                },
                new() {
                    Key = "AboutFeature3",
                    Label = "Feature Point 3",
                    Description = "Third bullet point in about section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "We welcome donations from every country."
                },
                new() {
                    Key = "VolunteerCount",
                    Label = "Volunteer Count Display",
                    Description = "Volunteer count shown on image (e.g., '102k+')",
                    ContentType = "Text",
                    MaxLength = 20,
                    CurrentTemplateValue = "102k+"
                },

                // Impact/Fact Section
                new() {
                    Key = "Stat1Value",
                    Label = "Statistic 1 Value",
                    Description = "First statistic percentage or number (e.g., '90%')",
                    ContentType = "Text",
                    MaxLength = 10,
                    CurrentTemplateValue = "90%"
                },
                new() {
                    Key = "Stat1Label",
                    Label = "Statistic 1 Label",
                    Description = "Label for first statistic",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Building a hospital"
                },
                new() {
                    Key = "Stat2Value",
                    Label = "Statistic 2 Value",
                    Description = "Second statistic value (e.g., '30k')",
                    ContentType = "Text",
                    MaxLength = 10,
                    CurrentTemplateValue = "30k"
                },
                new() {
                    Key = "Stat2Label",
                    Label = "Statistic 2 Label",
                    Description = "Label for second statistic",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Monthly donors"
                },
                new() {
                    Key = "Stat3Value",
                    Label = "Statistic 3 Value",
                    Description = "Third statistic percentage (e.g., '82%')",
                    ContentType = "Text",
                    MaxLength = 10,
                    CurrentTemplateValue = "82%"
                },
                new() {
                    Key = "Stat3Label",
                    Label = "Statistic 3 Label",
                    Description = "Label for third statistic",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Successful campaigns"
                },

                // What We Do Section
                new() {
                    Key = "WhatWeDoTitle",
                    Label = "What We Do - Title",
                    Description = "Main title for What We Do section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Enrich Lives offer Hope Inspire Change"
                },
                new() {
                    Key = "WhatWeDoSubtitle",
                    Label = "What We Do - Subtitle",
                    Description = "Subtitle/description for What We Do section",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Charity volunteers dedicate their time and effort to improve lives."
                },

                // Team Section
                new() {
                    Key = "TeamSectionTitle",
                    Label = "Team Section Title",
                    Description = "Title for the board members section",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Our Board Members"
                },
                new() {
                    Key = "TeamSectionSubtitle",
                    Label = "Team Section Description",
                    Description = "Description text for team section",
                    ContentType = "Text",
                    MaxLength = 300,
                    CurrentTemplateValue = "Our board members bring diverse talents and dedicated leadership to guide the Ellis Hope Foundation's mission."
                },

                // Testimonial Section
                new() {
                    Key = "TestimonialTitle",
                    Label = "Testimonial Section Title",
                    Description = "Title for testimonials (e.g., 'Over 1000+ people have faith in us')",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Over 1000+ people have faith in us."
                },
                new() {
                    Key = "TestimonialSubtitle",
                    Label = "Testimonial Section Description",
                    Description = "Supporting text for testimonial section",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "The trust and confidence of over thousand individuals solidify and validate our excellence."
                },

                // Newsletter Section
                new() {
                    Key = "NewsletterTitle",
                    Label = "Newsletter Section Title",
                    Description = "Call to action for newsletter signup",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Subscribe to Regular Newsletters."
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
