using EllisHope.Models.Domain;

namespace EllisHope.Models.ViewModels;

/// <summary>
/// ViewModel for the About page with board members and sponsors
/// </summary>
public class AboutPageViewModel
{
    /// <summary>
    /// Active board members to display in the team section
    /// </summary>
    public IEnumerable<ApplicationUser> BoardMembers { get; set; } = Enumerable.Empty<ApplicationUser>();

    /// <summary>
    /// Sponsors with approved quotes for the testimonial carousel
    /// </summary>
    public IEnumerable<ApplicationUser> SponsorsWithQuotes { get; set; } = Enumerable.Empty<ApplicationUser>();

    /// <summary>
    /// All active sponsors for the logo section (includes those without quotes)
    /// </summary>
    public IEnumerable<ApplicationUser> AllSponsors { get; set; } = Enumerable.Empty<ApplicationUser>();
}
