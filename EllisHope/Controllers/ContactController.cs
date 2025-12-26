using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using EllisHope.Services;
using EllisHope.Models.ViewModels;

namespace EllisHope.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ContactController : Controller
{
    private readonly IRecaptchaService _recaptchaService;
    private readonly IEmailService _emailService;
    private readonly ContactFormSettings _contactSettings;
    private readonly RecaptchaSettings _recaptchaSettings;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        IRecaptchaService recaptchaService,
        IEmailService emailService,
        IOptions<ContactFormSettings> contactSettings,
        IOptions<RecaptchaSettings> recaptchaSettings,
        ILogger<ContactController> logger)
    {
        _recaptchaService = recaptchaService;
        _emailService = emailService;
        _contactSettings = contactSettings.Value;
        _recaptchaSettings = recaptchaSettings.Value;
        _logger = logger;
    }

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
        ViewBag.RecaptchaSiteKey = _recaptchaSettings.SiteKey;
        return View(new ContactFormViewModel());
    }

    // POST: Contact
    /// <summary>
    /// Processes the contact form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactFormViewModel model)
    {
        ViewBag.RecaptchaSiteKey = _recaptchaSettings.SiteKey;

        // Check honeypot field - bots often fill all fields
        if (!string.IsNullOrWhiteSpace(model.Website))
        {
            _logger.LogWarning("Contact form honeypot triggered - likely bot submission");
            // Return success to not tip off the bot, but don't process
            model.SuccessMessage = "Thank you for your message. We'll get back to you soon!";
            return View(new ContactFormViewModel { SuccessMessage = model.SuccessMessage });
        }

        // Validate reCAPTCHA
        var recaptchaResult = await _recaptchaService.ValidateAsync(model.RecaptchaToken);
        if (!recaptchaResult.Success)
        {
            _logger.LogWarning("reCAPTCHA validation failed: {Error}", recaptchaResult.ErrorMessage);
            ModelState.AddModelError(string.Empty, recaptchaResult.ErrorMessage ?? "Security verification failed. Please try again.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Build email content
            var subject = $"{_contactSettings.SubjectPrefix} Message from {model.Name}";
            var body = BuildEmailBody(model);

            // Send email to the configured recipient
            await _emailService.SendEmailAsync(
                _contactSettings.RecipientEmail,
                subject,
                body,
                isHtml: true,
                replyTo: model.Email
            );

            _logger.LogInformation("Contact form submitted successfully from {Email}", model.Email);

            // Return success
            return View(new ContactFormViewModel
            {
                SuccessMessage = "Thank you for your message! We'll get back to you as soon as possible."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact form email from {Email}", model.Email);
            model.ErrorMessage = "We're sorry, there was a problem sending your message. Please try again later or contact us directly.";
            return View(model);
        }
    }

    private static string BuildEmailBody(ContactFormViewModel model)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2d8f78; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .field {{ margin-bottom: 15px; }}
        .label {{ font-weight: bold; color: #555; }}
        .message {{ background-color: white; padding: 15px; border-left: 4px solid #2d8f78; margin-top: 10px; }}
        .footer {{ padding: 15px; text-align: center; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>New Contact Form Submission</h2>
        </div>
        <div class='content'>
            <div class='field'>
                <span class='label'>Name:</span> {System.Web.HttpUtility.HtmlEncode(model.Name)}
            </div>
            <div class='field'>
                <span class='label'>Email:</span> <a href='mailto:{System.Web.HttpUtility.HtmlEncode(model.Email)}'>{System.Web.HttpUtility.HtmlEncode(model.Email)}</a>
            </div>
            <div class='field'>
                <span class='label'>Message:</span>
                <div class='message'>
                    {System.Web.HttpUtility.HtmlEncode(model.Message).Replace("\n", "<br>")}
                </div>
            </div>
        </div>
        <div class='footer'>
            <p>This message was sent from the Ellis Hope Foundation website contact form.</p>
            <p>Submitted on {DateTime.UtcNow:MMMM dd, yyyy} at {DateTime.UtcNow:HH:mm} UTC</p>
        </div>
    </div>
</body>
</html>";
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
