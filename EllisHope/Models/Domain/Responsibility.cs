namespace EllisHope.Models.Domain;

/// <summary>
/// Defines the different responsibilities that can be assigned to users
/// for managing specific content areas of the site.
/// </summary>
public enum Responsibility
{
    /// <summary>
    /// Can create, edit, and publish blog posts
    /// </summary>
    Blogger = 1,

    /// <summary>
    /// Can create, edit, and publish events
    /// </summary>
    EventPlanner = 2,

    /// <summary>
    /// Can create, edit causes and update fundraising progress
    /// </summary>
    CauseManager = 3,

    /// <summary>
    /// Can create and send newsletters
    /// </summary>
    NewsletterEditor = 4,

    /// <summary>
    /// Can approve or reject sponsor quotes
    /// </summary>
    SponsorReviewer = 5,

    /// <summary>
    /// Has full access to the media library
    /// </summary>
    MediaManager = 6,

    /// <summary>
    /// Can create, edit, and publish testimonials
    /// </summary>
    TestimonialManager = 7
}
