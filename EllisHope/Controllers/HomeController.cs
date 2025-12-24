using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    // GET: Home
    /// <summary>
    /// Displays the primary landing page of the Ellis Hope Foundation website (default layout)
    /// </summary>
    /// <returns>View displaying the main homepage with hero section, mission statement, and call-to-action elements</returns>
    /// <remarks>
    /// The main entry point for website visitors. Features:
    /// - Hero section with featured cause or event
    /// - Mission statement and organization overview
    /// - Quick links to key sections (About, Causes, Events, Apply)
    /// - Recent blog posts and upcoming events
    /// - Donation and volunteer call-to-action buttons
    /// - Testimonials or impact statistics
    ///
    /// Accessible to all users (no authentication required).
    /// </remarks>
    /// <response code="200">Successfully displayed homepage</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays the primary homepage with default layout",
        Description = "Main landing page featuring hero section, mission overview, recent content, and call-to-action elements. Primary entry point for all website visitors.",
        OperationId = "GetHomePage",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative homepage layout variation 2 for A/B testing and design exploration
    /// </summary>
    /// <returns>View displaying alternative homepage design with different content arrangement and visual hierarchy</returns>
    /// <remarks>
    /// Alternative homepage layout for testing different design approaches. Used for:
    /// - A/B testing conversion rates
    /// - Evaluating different content hierarchies
    /// - Testing alternative call-to-action placements
    /// - Design exploration and stakeholder feedback
    ///
    /// May feature different layouts for hero sections, content organization, or visual styles
    /// compared to the default Index page.
    /// </remarks>
    /// <response code="200">Successfully displayed alternative homepage layout</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative homepage layout variation 2",
        Description = "Alternative landing page design for A/B testing and design exploration. Features different content arrangement and visual hierarchy.",
        OperationId = "GetHomePageVariation2",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index2()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative homepage layout variation 3 for A/B testing and design exploration
    /// </summary>
    /// <returns>View displaying alternative homepage design with different content arrangement and visual hierarchy</returns>
    /// <remarks>
    /// Alternative homepage layout for testing different design approaches. Used for:
    /// - A/B testing conversion rates
    /// - Evaluating different content hierarchies
    /// - Testing alternative call-to-action placements
    /// - Design exploration and stakeholder feedback
    ///
    /// May feature different layouts for hero sections, content organization, or visual styles
    /// compared to the default Index page.
    /// </remarks>
    /// <response code="200">Successfully displayed alternative homepage layout</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative homepage layout variation 3",
        Description = "Alternative landing page design for A/B testing and design exploration. Features different content arrangement and visual hierarchy.",
        OperationId = "GetHomePageVariation3",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index3()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative homepage layout variation 4 for A/B testing and design exploration
    /// </summary>
    /// <returns>View displaying alternative homepage design with different content arrangement and visual hierarchy</returns>
    /// <remarks>
    /// Alternative homepage layout for testing different design approaches. Used for:
    /// - A/B testing conversion rates
    /// - Evaluating different content hierarchies
    /// - Testing alternative call-to-action placements
    /// - Design exploration and stakeholder feedback
    ///
    /// May feature different layouts for hero sections, content organization, or visual styles
    /// compared to the default Index page.
    /// </remarks>
    /// <response code="200">Successfully displayed alternative homepage layout</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative homepage layout variation 4",
        Description = "Alternative landing page design for A/B testing and design exploration. Features different content arrangement and visual hierarchy.",
        OperationId = "GetHomePageVariation4",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index4()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative homepage layout variation 5 for A/B testing and design exploration
    /// </summary>
    /// <returns>View displaying alternative homepage design with different content arrangement and visual hierarchy</returns>
    /// <remarks>
    /// Alternative homepage layout for testing different design approaches. Used for:
    /// - A/B testing conversion rates
    /// - Evaluating different content hierarchies
    /// - Testing alternative call-to-action placements
    /// - Design exploration and stakeholder feedback
    ///
    /// May feature different layouts for hero sections, content organization, or visual styles
    /// compared to the default Index page.
    /// </remarks>
    /// <response code="200">Successfully displayed alternative homepage layout</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative homepage layout variation 5",
        Description = "Alternative landing page design for A/B testing and design exploration. Features different content arrangement and visual hierarchy.",
        OperationId = "GetHomePageVariation5",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index5()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative homepage layout variation 6 for A/B testing and design exploration
    /// </summary>
    /// <returns>View displaying alternative homepage design with different content arrangement and visual hierarchy</returns>
    /// <remarks>
    /// Alternative homepage layout for testing different design approaches. Used for:
    /// - A/B testing conversion rates
    /// - Evaluating different content hierarchies
    /// - Testing alternative call-to-action placements
    /// - Design exploration and stakeholder feedback
    ///
    /// May feature different layouts for hero sections, content organization, or visual styles
    /// compared to the default Index page.
    /// </remarks>
    /// <response code="200">Successfully displayed alternative homepage layout</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative homepage layout variation 6",
        Description = "Alternative landing page design for A/B testing and design exploration. Features different content arrangement and visual hierarchy.",
        OperationId = "GetHomePageVariation6",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index6()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative homepage layout variation 7 for A/B testing and design exploration
    /// </summary>
    /// <returns>View displaying alternative homepage design with different content arrangement and visual hierarchy</returns>
    /// <remarks>
    /// Alternative homepage layout for testing different design approaches. Used for:
    /// - A/B testing conversion rates
    /// - Evaluating different content hierarchies
    /// - Testing alternative call-to-action placements
    /// - Design exploration and stakeholder feedback
    ///
    /// May feature different layouts for hero sections, content organization, or visual styles
    /// compared to the default Index page.
    /// </remarks>
    /// <response code="200">Successfully displayed alternative homepage layout</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative homepage layout variation 7",
        Description = "Alternative landing page design for A/B testing and design exploration. Features different content arrangement and visual hierarchy.",
        OperationId = "GetHomePageVariation7",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index7()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative homepage layout variation 8 for A/B testing and design exploration
    /// </summary>
    /// <returns>View displaying alternative homepage design with different content arrangement and visual hierarchy</returns>
    /// <remarks>
    /// Alternative homepage layout for testing different design approaches. Used for:
    /// - A/B testing conversion rates
    /// - Evaluating different content hierarchies
    /// - Testing alternative call-to-action placements
    /// - Design exploration and stakeholder feedback
    ///
    /// May feature different layouts for hero sections, content organization, or visual styles
    /// compared to the default Index page.
    /// </remarks>
    /// <response code="200">Successfully displayed alternative homepage layout</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative homepage layout variation 8",
        Description = "Alternative landing page design for A/B testing and design exploration. Features different content arrangement and visual hierarchy.",
        OperationId = "GetHomePageVariation8",
        Tags = new[] { "Home" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index8()
    {
        return View();
    }
}
