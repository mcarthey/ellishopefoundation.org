using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[ApiExplorerSettings(GroupName = "Contact")]
[SwaggerTag("Contact forms and communication channels. Public-facing pages for visitors to send messages, inquiries, and feedback to the Ellis Hope Foundation.")]
public class ContactController : Controller
{
    // GET: Contact
    /// <summary>
    /// Displays the primary contact form for visitors to send messages and inquiries
    /// </summary>
    /// <returns>View displaying contact form with fields for name, email, subject, and message</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Contact
    ///     GET /Contact/Index
    ///
    /// Returns the main contact page featuring:
    /// - Contact form with name, email, subject, message fields
    /// - Organization contact information (phone, email, address)
    /// - Office hours and availability
    /// - Map or directions to physical location (if applicable)
    /// - Links to social media channels
    ///
    /// **Form Submission:**
    /// - Form uses anti-forgery token validation for POST requests
    /// - Submitted messages are typically sent via email to organization admins
    /// - Client-side and server-side validation for required fields
    /// - May include reCAPTCHA or similar spam protection
    ///
    /// Accessible to all users (no authentication required).
    /// </remarks>
    /// <response code="200">Successfully displayed contact form</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays primary contact form for visitor inquiries",
        Description = "Returns contact page with form fields, organization contact information, and submission handling. Includes anti-forgery protection and validation.",
        OperationId = "GetContactPage",
        Tags = new[] { "Contact" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays alternative contact form layout (version 2) for A/B testing or design exploration
    /// </summary>
    /// <returns>View displaying alternative contact form with different layout or features</returns>
    /// <remarks>
    /// Alternative contact page layout for testing different design approaches. May feature:
    /// - Different form field arrangements
    /// - Additional or alternative contact methods
    /// - Different visual styling or user experience
    /// - Alternative validation or submission workflows
    ///
    /// Used for A/B testing conversion rates and user experience optimization.
    ///
    /// Accessible to all users (no authentication required).
    /// </remarks>
    /// <response code="200">Successfully displayed alternative contact form</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays alternative contact form layout (version 2)",
        Description = "Alternative contact page design for A/B testing and UX optimization. May feature different layout, fields, or submission workflow.",
        OperationId = "GetContactPageV2",
        Tags = new[] { "Contact" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult v2()
    {
        return View();
    }
}
