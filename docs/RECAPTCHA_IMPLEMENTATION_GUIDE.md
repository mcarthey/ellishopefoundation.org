# Google reCAPTCHA v3 Implementation Guide

This guide documents how reCAPTCHA v3 is implemented in the Ellis Hope Foundation website. Use this as a reference for implementing the same spam protection on other ASP.NET Core projects (e.g., LearnedGeek.com).

## Overview

This implementation uses **Google reCAPTCHA v3**, which:
- Works invisibly (no user interaction required)
- Returns a score (0.0 to 1.0) indicating likelihood of human vs bot
- Uses action names to identify where the reCAPTCHA was triggered

Additionally, we use a **honeypot field** as a secondary defense layer.

---

## Step 1: Get reCAPTCHA Keys from Google

1. Go to https://www.google.com/recaptcha/admin
2. Click **"+ Create"** (or the + icon)
3. Fill in the form:
   - **Label**: Your site name (e.g., "LearnedGeek.com")
   - **reCAPTCHA type**: Select **reCAPTCHA v3**
   - **Domains**: Add your domains (e.g., `learnedgeek.com`, `www.learnedgeek.com`, `localhost` for testing)
4. Accept the terms and click **Submit**
5. Copy your **Site Key** (public) and **Secret Key** (private)

---

## Step 2: Configuration Settings

### appsettings.json

Add the reCAPTCHA and ContactForm configuration sections:

```json
{
  "Recaptcha": {
    "SiteKey": "",
    "SecretKey": "",
    "MinimumScore": 0.5
  },
  "ContactForm": {
    "RecipientEmail": "admin@yourdomain.com",
    "SubjectPrefix": "[Website Contact]"
  }
}
```

### User Secrets (for development) or Environment Variables (for production)

**Never commit your secret key to source control!**

For local development:
```powershell
cd YourProject
dotnet user-secrets set "Recaptcha:SiteKey" "your-site-key-here"
dotnet user-secrets set "Recaptcha:SecretKey" "your-secret-key-here"
```

For SmarterASP.NET hosting, set these in the hosting control panel as environment variables or in a separate `appsettings.Production.json` that you don't commit to git.

---

## Step 3: Settings Classes

Create the configuration classes (can be in a separate file or in your service file):

```csharp
/// <summary>
/// reCAPTCHA configuration settings
/// </summary>
public class RecaptchaSettings
{
    /// <summary>
    /// Site key (public) - used in the client-side widget
    /// </summary>
    public string SiteKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret key (private) - used for server-side validation
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Minimum score (0.0 to 1.0) to consider valid. Default is 0.5
    /// Higher scores indicate more likely human users
    /// </summary>
    public float MinimumScore { get; set; } = 0.5f;
}

/// <summary>
/// Contact form configuration settings
/// </summary>
public class ContactFormSettings
{
    /// <summary>
    /// Email address that receives contact form submissions
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>
    /// Prefix added to email subjects from the contact form
    /// </summary>
    public string SubjectPrefix { get; set; } = "[Website Contact]";
}
```

---

## Step 4: reCAPTCHA Service

Create the service that validates tokens with Google:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace YourProject.Services;

/// <summary>
/// Service interface for Google reCAPTCHA validation
/// </summary>
public interface IRecaptchaService
{
    /// <summary>
    /// Validates a reCAPTCHA token
    /// </summary>
    /// <param name="token">The token from the client-side reCAPTCHA</param>
    /// <returns>Validation result with success status and score</returns>
    Task<RecaptchaValidationResult> ValidateAsync(string token);
}

/// <summary>
/// Google reCAPTCHA v3 validation service
/// </summary>
public class RecaptchaService : IRecaptchaService
{
    private readonly RecaptchaSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RecaptchaService> _logger;
    private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    public RecaptchaService(
        IOptions<RecaptchaSettings> settings,
        HttpClient httpClient,
        ILogger<RecaptchaService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RecaptchaValidationResult> ValidateAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new RecaptchaValidationResult
            {
                Success = false,
                ErrorMessage = "reCAPTCHA token is required"
            };
        }

        // If secret key is not configured, allow in development
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            _logger.LogWarning("reCAPTCHA secret key not configured - skipping validation");
            return new RecaptchaValidationResult
            {
                Success = true,
                Score = 1.0f,
                Action = "contact_form"
            };
        }

        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _settings.SecretKey),
                new KeyValuePair<string, string>("response", token)
            });

            var response = await _httpClient.PostAsync(VerifyUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<RecaptchaResponse>(json);

            if (result == null)
            {
                _logger.LogWarning("Failed to deserialize reCAPTCHA response");
                return new RecaptchaValidationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to validate reCAPTCHA"
                };
            }

            // For reCAPTCHA v3, check the score
            if (result.Success && result.Score >= _settings.MinimumScore)
            {
                _logger.LogInformation("reCAPTCHA validation successful. Score: {Score}", result.Score);
                return new RecaptchaValidationResult
                {
                    Success = true,
                    Score = result.Score,
                    Action = result.Action
                };
            }

            _logger.LogWarning("reCAPTCHA validation failed. Success: {Success}, Score: {Score}, Errors: {Errors}",
                result.Success, result.Score, string.Join(", ", result.ErrorCodes ?? Array.Empty<string>()));

            return new RecaptchaValidationResult
            {
                Success = false,
                Score = result.Score,
                ErrorMessage = result.Score < _settings.MinimumScore
                    ? "Suspicious activity detected. Please try again."
                    : "reCAPTCHA validation failed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reCAPTCHA token");
            return new RecaptchaValidationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during verification"
            };
        }
    }
}

/// <summary>
/// Result of reCAPTCHA validation
/// </summary>
public class RecaptchaValidationResult
{
    public bool Success { get; set; }
    public float Score { get; set; }
    public string? Action { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response from Google reCAPTCHA API
/// </summary>
public class RecaptchaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("challenge_ts")]
    public string? ChallengeTs { get; set; }

    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    [JsonPropertyName("error-codes")]
    public string[]? ErrorCodes { get; set; }
}
```

---

## Step 5: Register Services in Program.cs

Add these lines before `var app = builder.Build();`:

```csharp
// Configure reCAPTCHA settings
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("Recaptcha"));
builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();

// Configure Contact Form settings
builder.Services.Configure<ContactFormSettings>(builder.Configuration.GetSection("ContactForm"));
```

---

## Step 6: ViewModel with reCAPTCHA Token

Create or update your contact form ViewModel:

```csharp
using System.ComponentModel.DataAnnotations;

public class ContactFormViewModel
{
    [Required(ErrorMessage = "Please enter your name")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your message")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 5000 characters")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Google reCAPTCHA token - populated by the client-side reCAPTCHA widget
    /// </summary>
    [Required(ErrorMessage = "Please complete the reCAPTCHA verification")]
    public string RecaptchaToken { get; set; } = string.Empty;

    /// <summary>
    /// Honeypot field - should be empty for legitimate submissions
    /// Bots often fill all fields, triggering this trap
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Success message to display after submission
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Error message to display if submission fails
    /// </summary>
    public string? ErrorMessage { get; set; }
}
```

---

## Step 7: Controller Implementation

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

public class ContactController : Controller
{
    private readonly IRecaptchaService _recaptchaService;
    private readonly IEmailService _emailService;  // Your email service
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
    [HttpGet]
    public IActionResult Index()
    {
        // Pass site key to view for client-side reCAPTCHA
        ViewBag.RecaptchaSiteKey = _recaptchaSettings.SiteKey;
        return View(new ContactFormViewModel());
    }

    // POST: Contact
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
            return View(new ContactFormViewModel
            {
                SuccessMessage = "Thank you for your message. We'll get back to you soon!"
            });
        }

        // Validate reCAPTCHA
        var recaptchaResult = await _recaptchaService.ValidateAsync(model.RecaptchaToken);
        if (!recaptchaResult.Success)
        {
            _logger.LogWarning("reCAPTCHA validation failed: {Error}", recaptchaResult.ErrorMessage);
            ModelState.AddModelError(string.Empty,
                recaptchaResult.ErrorMessage ?? "Security verification failed. Please try again.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Build and send email (implement your email logic here)
            var subject = $"{_contactSettings.SubjectPrefix} Message from {model.Name}";
            // ... send email using your email service ...

            _logger.LogInformation("Contact form submitted successfully from {Email}", model.Email);

            return View(new ContactFormViewModel
            {
                SuccessMessage = "Thank you for your message! We'll get back to you soon."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact form email from {Email}", model.Email);
            model.ErrorMessage = "We're sorry, there was a problem sending your message. Please try again later.";
            return View(model);
        }
    }
}
```

---

## Step 8: View Implementation (Razor)

```html
@model ContactFormViewModel
@{
    var recaptchaSiteKey = ViewBag.RecaptchaSiteKey as string;
}

@* Success Message *@
@if (!string.IsNullOrEmpty(Model?.SuccessMessage))
{
    <div class="alert alert-success" role="alert">
        @Model.SuccessMessage
    </div>
}
else
{
    @* Error Message *@
    @if (!string.IsNullOrEmpty(Model?.ErrorMessage))
    {
        <div class="alert alert-danger" role="alert">
            @Model.ErrorMessage
        </div>
    }

    @* Validation Summary *@
    @if (!ViewData.ModelState.IsValid)
    {
        <div class="alert alert-warning" role="alert">
            <div asp-validation-summary="All" class="text-danger"></div>
        </div>
    }

    <form asp-action="Index" asp-controller="Contact" method="post" id="contactForm">
        @Html.AntiForgeryToken()

        @* Honeypot field - hidden from real users, bots will fill it *@
        <div style="position: absolute; left: -5000px;" aria-hidden="true">
            <input type="text" name="Website" tabindex="-1" autocomplete="off" asp-for="Website">
        </div>

        <div class="mb-3">
            <input type="text" asp-for="Name" placeholder="Name" class="form-control">
            <span asp-validation-for="Name" class="text-danger"></span>
        </div>

        <div class="mb-3">
            <input type="email" asp-for="Email" placeholder="Email" class="form-control">
            <span asp-validation-for="Email" class="text-danger"></span>
        </div>

        <div class="mb-3">
            <textarea asp-for="Message" placeholder="Your Message" class="form-control" rows="5"></textarea>
            <span asp-validation-for="Message" class="text-danger"></span>
        </div>

        @* Hidden field for reCAPTCHA token *@
        <input type="hidden" asp-for="RecaptchaToken" id="recaptchaToken">

        <button type="submit" class="btn btn-primary" id="submitBtn">
            <span class="btn-text">Send Message</span>
            <span class="btn-loading d-none">
                <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                Sending...
            </span>
        </button>

        @if (!string.IsNullOrEmpty(recaptchaSiteKey))
        {
            <div class="mt-3">
                <small class="text-muted">
                    This site is protected by reCAPTCHA and the Google
                    <a href="https://policies.google.com/privacy" target="_blank" rel="noopener">Privacy Policy</a> and
                    <a href="https://policies.google.com/terms" target="_blank" rel="noopener">Terms of Service</a> apply.
                </small>
            </div>
        }
    </form>
}

@section Scripts {
    <partial name="_ValidationScriptsPartial" />

    @if (!string.IsNullOrEmpty(recaptchaSiteKey))
    {
        <script src="https://www.google.com/recaptcha/api.js?render=@recaptchaSiteKey"></script>
        <script>
            document.getElementById('contactForm')?.addEventListener('submit', function(e) {
                e.preventDefault();

                const form = this;
                const submitBtn = document.getElementById('submitBtn');
                const btnText = submitBtn.querySelector('.btn-text');
                const btnLoading = submitBtn.querySelector('.btn-loading');

                // Show loading state
                btnText.classList.add('d-none');
                btnLoading.classList.remove('d-none');
                submitBtn.disabled = true;

                // Execute reCAPTCHA
                grecaptcha.ready(function() {
                    grecaptcha.execute('@recaptchaSiteKey', { action: 'contact_form' }).then(function(token) {
                        // Set the token in the hidden field
                        document.getElementById('recaptchaToken').value = token;
                        // Submit the form
                        form.submit();
                    }).catch(function(error) {
                        console.error('reCAPTCHA error:', error);
                        // Reset button state
                        btnText.classList.remove('d-none');
                        btnLoading.classList.add('d-none');
                        submitBtn.disabled = false;
                        alert('Security verification failed. Please try again.');
                    });
                });
            });
        </script>
    }
    else
    {
        <script>
            // Development mode without reCAPTCHA - just show loading state
            document.getElementById('contactForm')?.addEventListener('submit', function(e) {
                const submitBtn = document.getElementById('submitBtn');
                const btnText = submitBtn.querySelector('.btn-text');
                const btnLoading = submitBtn.querySelector('.btn-loading');

                btnText.classList.add('d-none');
                btnLoading.classList.remove('d-none');
                submitBtn.disabled = true;

                // Set a dummy token for development
                document.getElementById('recaptchaToken').value = 'development-mode-token';
            });
        </script>
    }
}
```

---

## Step 9: Unit Testing

When testing controllers that use reCAPTCHA, mock the `IRecaptchaService`:

```csharp
using Moq;
using Xunit;

public class ContactControllerTests
{
    private readonly Mock<IRecaptchaService> _recaptchaServiceMock;

    public ContactControllerTests()
    {
        _recaptchaServiceMock = new Mock<IRecaptchaService>();
    }

    [Fact]
    public async Task Index_Post_RecaptchaFails_ReturnsViewWithError()
    {
        // Arrange
        var model = new ContactFormViewModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Message = "Test message",
            RecaptchaToken = "invalid-token"
        };

        _recaptchaServiceMock.Setup(x => x.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(new RecaptchaValidationResult
            {
                Success = false,
                ErrorMessage = "reCAPTCHA validation failed"
            });

        // Act & Assert...
    }

    [Fact]
    public async Task Index_Post_ValidSubmission_SendsEmail()
    {
        // Arrange
        _recaptchaServiceMock.Setup(x => x.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(new RecaptchaValidationResult
            {
                Success = true,
                Score = 0.9f
            });

        // Act & Assert...
    }
}
```

---

## How It Works

### Flow Diagram

```
User loads page
       |
       v
Page includes reCAPTCHA JS with site key
       |
       v
User fills form and clicks Submit
       |
       v
JavaScript intercepts submit, calls grecaptcha.execute()
       |
       v
Google returns token (contains encrypted score)
       |
       v
Token placed in hidden field, form submits
       |
       v
Server receives token, calls Google API with secret key
       |
       v
Google returns: { success: true, score: 0.9, action: "contact_form" }
       |
       v
If score >= MinimumScore (0.5), process form
       |
       v
If score < MinimumScore, reject as suspicious
```

### Score Interpretation

| Score | Interpretation |
|-------|----------------|
| 0.0 - 0.3 | Very likely a bot |
| 0.3 - 0.5 | Suspicious, may be a bot |
| 0.5 - 0.7 | Possibly human |
| 0.7 - 1.0 | Very likely human |

The default `MinimumScore` of 0.5 is a good balance. You can adjust based on your needs:
- Lower (0.3) = more permissive, some spam may get through
- Higher (0.7) = stricter, may block some legitimate users

---

## Deployment Checklist

1. **Register your production domain** in Google reCAPTCHA admin console
2. **Set the secret key** in your production environment (not in appsettings.json!)
3. **Test the form** on production to verify tokens are being validated
4. **Monitor logs** for reCAPTCHA failures and adjust MinimumScore if needed
5. **Check Google reCAPTCHA dashboard** for analytics on bot traffic

---

## Troubleshooting

### "reCAPTCHA token is required"
- Ensure the JavaScript is loading (check browser console)
- Verify the site key is being passed to the view

### "reCAPTCHA validation failed" in production
- Check that the secret key is correctly set
- Verify the domain is registered in Google reCAPTCHA admin
- Check server logs for specific error codes

### Low scores for legitimate users
- This can happen with VPNs, privacy browsers, or first-time visitors
- Consider lowering MinimumScore to 0.3-0.4
- The honeypot provides secondary protection

### reCAPTCHA badge is showing (optional to hide)
reCAPTCHA v3 shows a badge by default. To hide it, add this CSS and keep the required text disclosure:

```css
.grecaptcha-badge {
    visibility: hidden;
}
```

You must still include the privacy policy text as shown in the view template.
