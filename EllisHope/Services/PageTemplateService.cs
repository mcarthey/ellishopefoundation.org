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
            "blog" => GetBlogPageTemplate(),
            "events" => GetEventsPageTemplate(),
            "causes" => GetCausesPageTemplate(),
            "faq" => GetFaqPageTemplate(),
            _ => GetGenericPageTemplate(pageName)
        };
    }

    public List<string> GetAvailablePages()
    {
        return new List<string> { "Home", "About", "Team", "Services", "Contact", "Blog", "Events", "Causes", "Faq" };
    }

    private PageTemplate GetHomePageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Home",
            DisplayName = "Home Page",
            Description = "Main landing page - hero section, testimonial, services, causes, about, and CTA",
            Images = new List<EditableImage>
            {
                new() {
                    Key = "HeroImage",
                    Label = "Hero Section Image",
                    Description = "Main image in the hero section (right side)",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 800,
                        RecommendedHeight = 600,
                        MinWidth = 600,
                        MinHeight = 400,
                        MaxWidth = 1200,
                        MaxHeight = 900,
                        AspectRatio = "4:3",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 5 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/hero/hero-img-6-unsplash.png",
                    FallbackPath = "/assets/img/hero/hero-img-6-unsplash.png"
                },
                new() {
                    Key = "AboutImage",
                    Label = "About Section Image",
                    Description = "Image for the 'About Ellis Hope' section (portrait orientation)",
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
                        MaxFileSizeBytes = 3 * 1024 * 1024
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
                        MaxFileSizeBytes = 5 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/bg/make-difference-1800x855.jpg",
                    FallbackPath = "/assets/img/bg/make-difference-1800x855.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                // Hero Section
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
                    Label = "Hero Description",
                    Description = "Supporting text under main headline",
                    ContentType = "Text",
                    MaxLength = 300,
                    CurrentTemplateValue = "At Ellis Hope Foundation, we believe that every individual deserves the chance to pursue a healthier and more fulfilling life. Together, we turn dreams into reality."
                },

                // Testimonial Section
                new() {
                    Key = "TestimonialLabel",
                    Label = "Testimonial Label",
                    Description = "Label above testimonial (e.g., 'A Grateful Participant')",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "A Grateful Participant"
                },
                new() {
                    Key = "TestimonialQuote",
                    Label = "Testimonial Quote",
                    Description = "The testimonial quote text",
                    ContentType = "Text",
                    MaxLength = 500,
                    CurrentTemplateValue = "Thanks to Ellis Hope Foundation, I found the support and guidance I needed to regain my health and confidence. They changed my life."
                },
                new() {
                    Key = "TestimonialAuthor",
                    Label = "Testimonial Author",
                    Description = "Name/title of testimonial author",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Community Member"
                },

                // Services Section
                new() {
                    Key = "Service1Title",
                    Label = "Service 1 Title",
                    Description = "First service title (e.g., 'Fitness Programs')",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Fitness Programs"
                },
                new() {
                    Key = "Service1Description",
                    Label = "Service 1 Description",
                    Description = "Description for first service",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Access personalized fitness support to build strength, endurance, and confidence."
                },
                new() {
                    Key = "Service2Title",
                    Label = "Service 2 Title",
                    Description = "Second service title (e.g., 'Health Education')",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Health Education"
                },
                new() {
                    Key = "Service2Description",
                    Label = "Service 2 Description",
                    Description = "Description for second service",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Learn practical strategies for wellness, nutrition, and lifelong health habits."
                },
                new() {
                    Key = "Service3Title",
                    Label = "Service 3 Title",
                    Description = "Third service title (e.g., 'Community Support')",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Community Support"
                },
                new() {
                    Key = "Service3Description",
                    Label = "Service 3 Description",
                    Description = "Description for third service",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Build friendships, encouragement, and accountability through peer-led programs."
                },

                // Initiatives/Causes Section
                new() {
                    Key = "InitiativesTitle",
                    Label = "Initiatives Section Title",
                    Description = "Title for the initiatives/causes section",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Our Current Initiatives"
                },
                new() {
                    Key = "InitiativesSubtitle",
                    Label = "Initiatives Section Subtitle",
                    Description = "Subtitle for the initiatives section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Small steps. Big impact. Every effort counts toward building stronger lives."
                },

                // About Section
                new() {
                    Key = "AboutTitle",
                    Label = "About Section Title",
                    Description = "Title for the about section",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "About Ellis Hope Foundation"
                },
                new() {
                    Key = "AboutDescription",
                    Label = "About Section Description",
                    Description = "Description in the about section",
                    ContentType = "Text",
                    MaxLength = 500,
                    CurrentTemplateValue = "We are a community-driven organization dedicated to empowering individuals through fitness, education, and support. We believe in the resilience of the human spirit and the power of hope."
                },

                // CTA Section
                new() {
                    Key = "CTATitle",
                    Label = "CTA Title",
                    Description = "Call-to-action headline",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Ready to Make a Difference?"
                },
                new() {
                    Key = "CTADescription",
                    Label = "CTA Description",
                    Description = "Call-to-action supporting text",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Your contribution helps change lives through health, fitness, and hope."
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
            Description = "Programs and services offered by Ellis Hope Foundation",
            Images = new List<EditableImage>
            {
                new() {
                    Key = "HeaderBanner",
                    Label = "Page Header Banner",
                    Description = "Banner image at top of Services page",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 540,
                        MinWidth = 1200,
                        MinHeight = 360,
                        AspectRatio = "10:3",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 4 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/page-title/our-service-1800x540.jpg",
                    FallbackPath = "/assets/img/page-title/our-service-1800x540.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                // Main section intro
                new() {
                    Key = "SectionTitle",
                    Label = "Section Title",
                    Description = "Main title for services section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Enrich Lives offer Hope Inspire Change"
                },
                new() {
                    Key = "SectionSubtitle",
                    Label = "Section Subtitle",
                    Description = "Subtitle/description for services section",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Charity volunteers dedicate their time and effort to improve lives."
                },

                // 6 Services
                new() { Key = "Service1Title", Label = "Service 1 Title", ContentType = "Text", MaxLength = 50, CurrentTemplateValue = "Liquid life unveiled" },
                new() { Key = "Service1Description", Label = "Service 1 Description", ContentType = "Text", MaxLength = 200, CurrentTemplateValue = "Become a member." },
                new() { Key = "Service2Title", Label = "Service 2 Title", ContentType = "Text", MaxLength = 50, CurrentTemplateValue = "Health tech revolution" },
                new() { Key = "Service2Description", Label = "Service 2 Description", ContentType = "Text", MaxLength = 200, CurrentTemplateValue = "Become a member." },
                new() { Key = "Service3Title", Label = "Service 3 Title", ContentType = "Text", MaxLength = 50, CurrentTemplateValue = "Educate to elevate" },
                new() { Key = "Service3Description", Label = "Service 3 Description", ContentType = "Text", MaxLength = 200, CurrentTemplateValue = "Become a member." },
                new() { Key = "Service4Title", Label = "Service 4 Title", ContentType = "Text", MaxLength = 50, CurrentTemplateValue = "Fresh and fit dining" },
                new() { Key = "Service4Description", Label = "Service 4 Description", ContentType = "Text", MaxLength = 200, CurrentTemplateValue = "Become a member." },
                new() { Key = "Service5Title", Label = "Service 5 Title", ContentType = "Text", MaxLength = 50, CurrentTemplateValue = "Home offering service" },
                new() { Key = "Service5Description", Label = "Service 5 Description", ContentType = "Text", MaxLength = 200, CurrentTemplateValue = "Become a member." },
                new() { Key = "Service6Title", Label = "Service 6 Title", ContentType = "Text", MaxLength = 50, CurrentTemplateValue = "Save green energy" },
                new() { Key = "Service6Description", Label = "Service 6 Description", ContentType = "Text", MaxLength = 200, CurrentTemplateValue = "Become a member." },

                // Testimonial Section
                new() { Key = "TestimonialTitle", Label = "Testimonial Title", ContentType = "Text", MaxLength = 100, CurrentTemplateValue = "Over 1000+ People have Faith in us." },
                new() { Key = "TestimonialSubtitle", Label = "Testimonial Subtitle", ContentType = "Text", MaxLength = 200, CurrentTemplateValue = "The trust and confidence of over thousand and validate our excellence." },

                // Newsletter
                new() { Key = "NewsletterTitle", Label = "Newsletter Title", ContentType = "Text", MaxLength = 100, CurrentTemplateValue = "Subscribe to Regular Newsletters." }
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
                // Contact Info Section Labels
                new() {
                    Key = "AddressLabel",
                    Label = "Address Label",
                    Description = "Label for the address section",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Office Address"
                },
                new() {
                    Key = "EmailLabel",
                    Label = "Email Label",
                    Description = "Label for the email section",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Email Address"
                },
                new() {
                    Key = "PhoneLabel",
                    Label = "Phone Label",
                    Description = "Label for the phone section",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Phone Number"
                },
                // Contact Form Section
                new() {
                    Key = "FormTitle",
                    Label = "Contact Form Title",
                    Description = "Title above the contact form",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Send Message."
                },
                new() {
                    Key = "FormSubtitle",
                    Label = "Contact Form Subtitle",
                    Description = "Subtitle/description under form title",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Your email address will not be published. Required fields are marked *"
                },
                // Newsletter Section
                new() {
                    Key = "NewsletterTitle",
                    Label = "Newsletter Section Title",
                    Description = "Title for the newsletter signup section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Subscribe to Regular Newsletters."
                }
            }
        };
    }

    private PageTemplate GetBlogPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Blog",
            DisplayName = "Blog Page",
            Description = "Blog listing page with posts and sidebar",
            Images = new List<EditableImage>
            {
                new() {
                    Key = "HeaderBanner",
                    Label = "Page Header Banner",
                    Description = "Banner image at top of Blog page",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 540,
                        MinWidth = 1200,
                        MinHeight = 360,
                        AspectRatio = "10:3",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 4 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/page-title/page-title-bg.jpg",
                    FallbackPath = "/assets/img/page-title/page-title-bg.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() {
                    Key = "PageTitle",
                    Label = "Page Title",
                    Description = "Main title displayed in the header",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Blog"
                },
                new() {
                    Key = "NewsletterTitle",
                    Label = "Newsletter Section Title",
                    Description = "Title for the newsletter signup section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Subscribe to Regular Newsletters."
                }
            }
        };
    }

    private PageTemplate GetEventsPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Events",
            DisplayName = "Events Page",
            Description = "Upcoming events listing page",
            Images = new List<EditableImage>
            {
                new() {
                    Key = "HeaderBanner",
                    Label = "Page Header Banner",
                    Description = "Banner image at top of Events page",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 540,
                        MinWidth = 1200,
                        MinHeight = 360,
                        AspectRatio = "10:3",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 4 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/page-title/numbers-1800x540.jpg",
                    FallbackPath = "/assets/img/page-title/numbers-1800x540.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() {
                    Key = "PageTitle",
                    Label = "Page Title",
                    Description = "Main title displayed in the header",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Upcoming Events"
                },
                new() {
                    Key = "SectionTitle",
                    Label = "Section Title",
                    Description = "Title for the events section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Building Brighter Futures"
                },
                new() {
                    Key = "SectionSubtitle",
                    Label = "Section Subtitle",
                    Description = "Description under section title",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Join us in making a difference. Explore our upcoming events and be part of something meaningful."
                },
                new() {
                    Key = "NewsletterTitle",
                    Label = "Newsletter Section Title",
                    Description = "Title for the newsletter signup section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Subscribe to Regular Newsletters."
                }
            }
        };
    }

    private PageTemplate GetCausesPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Causes",
            DisplayName = "Causes Page",
            Description = "Causes and initiatives listing page",
            Images = new List<EditableImage>
            {
                new() {
                    Key = "HeaderBanner",
                    Label = "Page Header Banner",
                    Description = "Banner image at top of Causes page",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 540,
                        MinWidth = 1200,
                        MinHeight = 360,
                        AspectRatio = "10:3",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 4 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/page-title/page-title-bg.jpg",
                    FallbackPath = "/assets/img/page-title/page-title-bg.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() {
                    Key = "PageTitle",
                    Label = "Page Title",
                    Description = "Main title displayed in the header",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Causes"
                },
                new() {
                    Key = "SectionTitle",
                    Label = "Section Title",
                    Description = "Main section title",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Small donations make big impacts."
                },
                new() {
                    Key = "NewsletterTitle",
                    Label = "Newsletter Section Title",
                    Description = "Title for the newsletter signup section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Subscribe to Regular Newsletters."
                }
            }
        };
    }

    private PageTemplate GetFaqPageTemplate()
    {
        return new PageTemplate
        {
            PageName = "Faq",
            DisplayName = "FAQ Page",
            Description = "Frequently asked questions page",
            Images = new List<EditableImage>
            {
                new() {
                    Key = "HeaderBanner",
                    Label = "Page Header Banner",
                    Description = "Banner image at top of FAQ page",
                    Requirements = new ImageRequirements
                    {
                        RecommendedWidth = 1800,
                        RecommendedHeight = 540,
                        MinWidth = 1200,
                        MinHeight = 360,
                        AspectRatio = "10:3",
                        Orientation = "Landscape",
                        MaxFileSizeBytes = 4 * 1024 * 1024
                    },
                    CurrentTemplatePath = "/assets/img/page-title/page-title-bg.jpg",
                    FallbackPath = "/assets/img/page-title/page-title-bg.jpg"
                }
            },
            ContentAreas = new List<EditableContent>
            {
                new() {
                    Key = "PageTitle",
                    Label = "Page Title",
                    Description = "Main title displayed in the header",
                    ContentType = "Text",
                    MaxLength = 50,
                    CurrentTemplateValue = "Frequently Asked Questions"
                },
                new() {
                    Key = "SectionTitle",
                    Label = "Section Title",
                    Description = "Title for the FAQ section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Questions Asked About Our Foundation"
                },
                new() {
                    Key = "SectionSubtitle",
                    Label = "Section Subtitle",
                    Description = "Description under section title",
                    ContentType = "Text",
                    MaxLength = 200,
                    CurrentTemplateValue = "Find answers to commonly asked questions about our programs and services."
                },
                new() {
                    Key = "NewsletterTitle",
                    Label = "Newsletter Section Title",
                    Description = "Title for the newsletter signup section",
                    ContentType = "Text",
                    MaxLength = 100,
                    CurrentTemplateValue = "Subscribe to Regular Newsletters."
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
