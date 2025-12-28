using EllisHope.Services;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

/// <summary>
/// Public controller for newsletter subscription and unsubscription
/// </summary>
public class NewsletterController : Controller
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(
        INewsletterService newsletterService,
        ILogger<NewsletterController> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }

    /// <summary>
    /// Handle newsletter subscription from contact page or other forms
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email, string? name = null, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["NewsletterError"] = "Please enter a valid email address.";
            return RedirectToReturnUrl(returnUrl);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var source = returnUrl?.Contains("contact", StringComparison.OrdinalIgnoreCase) == true
            ? "Contact Page"
            : "Website";

        var (success, message) = await _newsletterService.SubscribeAsync(email, name, source, ipAddress);

        if (success)
        {
            TempData["NewsletterSuccess"] = message;
        }
        else
        {
            TempData["NewsletterError"] = message;
        }

        return RedirectToReturnUrl(returnUrl);
    }

    /// <summary>
    /// Display unsubscribe confirmation page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Unsubscribe(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("UnsubscribeError", "Invalid unsubscribe link.");
        }

        var subscriber = await _newsletterService.GetSubscriberByTokenAsync(token);
        if (subscriber == null)
        {
            return View("UnsubscribeError", "Invalid or expired unsubscribe link.");
        }

        if (!subscriber.IsActive)
        {
            return View("UnsubscribeSuccess", "This email has already been unsubscribed.");
        }

        // Show confirmation page
        ViewBag.Token = token;
        ViewBag.Email = subscriber.Email;
        return View();
    }

    /// <summary>
    /// Process unsubscribe confirmation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmUnsubscribe(string token)
    {
        var (success, message) = await _newsletterService.UnsubscribeAsync(token);

        if (success)
        {
            return View("UnsubscribeSuccess", message);
        }

        return View("UnsubscribeError", message);
    }

    private IActionResult RedirectToReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Contact");
    }
}
