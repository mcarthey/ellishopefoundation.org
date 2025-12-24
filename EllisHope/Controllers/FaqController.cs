using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class FaqController : Controller
{
    // GET: Faq
    /// <summary>
    /// Displays the Frequently Asked Questions page with categorized Q and A sections
    /// </summary>
    /// <returns>View displaying organized FAQ content with questions and answers grouped by category</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Faq
    ///     GET /Faq/Index
    ///
    /// Returns the FAQ page featuring:
    /// - Questions organized by category (e.g., About Us, Programs, Applications, Donations, Volunteering)
    /// - Expandable/collapsible accordion-style Q and A sections
    /// - Search functionality to quickly find relevant questions
    /// - Links to related pages for more detailed information
    /// - Contact information for questions not covered in FAQ
    ///
    /// **Common FAQ Categories:**
    /// - About the Ellis Hope Foundation (mission, history, leadership)
    /// - Program Information (eligibility, services offered, duration)
    /// - Application Process (how to apply, timeline, requirements)
    /// - Board Review (voting process, decision criteria)
    /// - Sponsorship (becoming a sponsor, responsibilities)
    /// - Donations and Fundraising (how to donate, tax deductions)
    /// - Volunteer Opportunities (how to get involved, time commitment)
    ///
    /// Accessible to all users (no authentication required).
    /// </remarks>
    /// <response code="200">Successfully displayed FAQ page</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays Frequently Asked Questions page with categorized Q&A",
        Description = "Returns FAQ page with organized questions and answers about the foundation, programs, application process, and how to get involved. Includes search and navigation.",
        OperationId = "GetFaqPage",
        Tags = new[] { "FAQ" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index()
    {
        return View();
    }
}
