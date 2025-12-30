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

    /// <summary>
    /// Statistics to display in the fact section (causes progress + fallback stats)
    /// </summary>
    public List<AboutStatistic> Statistics { get; set; } = new();
}

/// <summary>
/// A single statistic to display in the fact/progress section
/// </summary>
public class AboutStatistic
{
    /// <summary>
    /// The display value (e.g., "90%", "25", "1.2k")
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// The label below the statistic (e.g., "Building a Hospital", "Active Members")
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The percentage for the circular progress (0-100)
    /// </summary>
    public int Percentage { get; set; }

    /// <summary>
    /// Whether this is a percentage stat (shows %) or a count stat
    /// </summary>
    public bool IsPercentage { get; set; }
}
