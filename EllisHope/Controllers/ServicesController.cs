using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[ApiExplorerSettings(GroupName = "Services")]
[SwaggerTag("Programs and services information. Public-facing pages describing the charitable services, support programs, and assistance offerings provided by the Ellis Hope Foundation.")]
public class ServicesController : Controller
{
    /// <summary>
    /// Displays comprehensive overview of services and programs offered by Ellis Hope Foundation
    /// </summary>
    /// <returns>View displaying all services, programs, eligibility criteria, and how to access each service</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Services
    ///     GET /Services/Index
    ///
    /// Returns the services page featuring:
    /// - Complete catalog of programs and services offered
    /// - Eligibility requirements for each service
    /// - Application process and how to access services
    /// - Success stories and testimonials from program participants
    /// - Contact information for service-specific inquiries
    ///
    /// **Common Services Include:**
    /// - Financial assistance programs (housing, utilities, transportation)
    /// - Educational support (tuition assistance, tutoring, scholarships)
    /// - Healthcare services (medical expense assistance, wellness programs)
    /// - Employment support (job training, career counseling, resume assistance)
    /// - Nutritional support (meal programs, grocery assistance)
    /// - Mental health services (counseling, support groups)
    /// - Youth programs (mentorship, after-school activities)
    /// - Senior services (companionship, transportation, home care)
    ///
    /// Accessible to all users (no authentication required).
    /// </remarks>
    /// <response code="200">Successfully displayed services overview page</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays comprehensive overview of foundation services and programs",
        Description = "Returns services page featuring program catalog, eligibility criteria, application process, and success stories. Public access.",
        OperationId = "GetServicesPage",
        Tags = new[] { "Services" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index()
    {
        return View();
    }

    // GET: Services/v2
    /// <summary>
    /// Displays alternative services page layout (version 2) for A/B testing or design exploration
    /// </summary>
    /// <returns>View displaying services with alternative layout, organization, or presentation style</returns>
    /// <remarks>
    /// Alternative services page layout for testing different design approaches. May feature:
    /// - Different service categorization or grouping
    /// - Alternative visual presentation (cards, accordions, tabs)
    /// - Different information hierarchy or navigation
    /// - Alternative calls-to-action or application prompts
    ///
    /// Used for A/B testing user engagement and conversion optimization.
    ///
    /// Accessible to all users (no authentication required).
    /// </remarks>
    /// <response code="200">Successfully displayed alternative services page</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative services page layout (version 2)",
        Description = "Alternative services page design for A/B testing and UX optimization. Features different layout, categorization, or presentation style.",
        OperationId = "GetServicesPageV2",
        Tags = new[] { "Services" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult v2()
    {
        return View();
    }
}
