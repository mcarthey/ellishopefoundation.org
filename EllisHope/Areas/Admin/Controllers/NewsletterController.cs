using EllisHope.Areas.Admin.Models;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editor")]
[ApiExplorerSettings(IgnoreApi = true)]
public class NewsletterController : Controller
{
    private readonly INewsletterService _newsletterService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(
        INewsletterService newsletterService,
        IConfiguration configuration,
        ILogger<NewsletterController> logger)
    {
        _newsletterService = newsletterService;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: Admin/Newsletter
    public async Task<IActionResult> Index()
    {
        var allSubscribers = await _newsletterService.GetAllSubscribersAsync();
        var activeCount = allSubscribers.Count(s => s.IsActive);
        var recentNewsletters = await _newsletterService.GetSentNewslettersAsync();
        var draftNewsletters = await _newsletterService.GetDraftNewslettersAsync();

        var viewModel = new NewsletterIndexViewModel
        {
            Subscribers = allSubscribers,
            ActiveCount = activeCount,
            TotalCount = allSubscribers.Count,
            UnsubscribedCount = allSubscribers.Count - activeCount,
            RecentNewsletters = recentNewsletters.Take(5).ToList(),
            DraftNewsletters = draftNewsletters
        };

        return View(viewModel);
    }

    // GET: Admin/Newsletter/Compose
    public async Task<IActionResult> Compose(int? id = null)
    {
        SetTinyMceApiKey();
        var subscriberCount = await _newsletterService.GetActiveSubscriberCountAsync();

        if (id.HasValue)
        {
            var newsletter = await _newsletterService.GetNewsletterByIdAsync(id.Value);
            if (newsletter == null || newsletter.IsSent)
            {
                TempData["ErrorMessage"] = "Newsletter not found or already sent.";
                return RedirectToAction(nameof(Index));
            }

            return View(new ComposeNewsletterViewModel
            {
                Id = newsletter.Id,
                Subject = newsletter.Subject,
                HtmlContent = newsletter.HtmlContent,
                PlainTextContent = newsletter.PlainTextContent,
                SubscriberCount = subscriberCount
            });
        }

        return View(new ComposeNewsletterViewModel
        {
            SubscriberCount = subscriberCount
        });
    }

    // POST: Admin/Newsletter/SaveDraft
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(ComposeNewsletterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetTinyMceApiKey();
            model.SubscriberCount = await _newsletterService.GetActiveSubscriberCountAsync();
            return View("Compose", model);
        }

        if (model.Id.HasValue)
        {
            var success = await _newsletterService.UpdateDraftAsync(
                model.Id.Value,
                model.Subject,
                model.HtmlContent,
                model.PlainTextContent);

            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to update draft.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Draft updated successfully.";
        }
        else
        {
            var newsletter = await _newsletterService.CreateDraftAsync(
                model.Subject,
                model.HtmlContent,
                model.PlainTextContent);

            model.Id = newsletter.Id;
            TempData["SuccessMessage"] = "Draft saved successfully.";
        }

        return RedirectToAction(nameof(Compose), new { id = model.Id });
    }

    // POST: Admin/Newsletter/Preview
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(ComposeNewsletterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetTinyMceApiKey();
            model.SubscriberCount = await _newsletterService.GetActiveSubscriberCountAsync();
            return View("Compose", model);
        }

        // Save draft first
        if (model.Id.HasValue)
        {
            await _newsletterService.UpdateDraftAsync(
                model.Id.Value,
                model.Subject,
                model.HtmlContent,
                model.PlainTextContent);
        }
        else
        {
            var newsletter = await _newsletterService.CreateDraftAsync(
                model.Subject,
                model.HtmlContent,
                model.PlainTextContent);
            model.Id = newsletter.Id;
        }

        var subscriberCount = await _newsletterService.GetActiveSubscriberCountAsync();

        var previewModel = new PreviewNewsletterViewModel
        {
            Id = model.Id!.Value,
            Subject = model.Subject,
            HtmlContent = model.HtmlContent,
            SubscriberCount = subscriberCount
        };

        return View(previewModel);
    }

    // POST: Admin/Newsletter/Send
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User not authenticated.";
            return RedirectToAction(nameof(Index));
        }

        var (success, recipientCount, message) = await _newsletterService.SendNewsletterAsync(id, userId);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Newsletter/History
    public async Task<IActionResult> History()
    {
        var sentNewsletters = await _newsletterService.GetSentNewslettersAsync();

        var viewModel = new NewsletterHistoryViewModel
        {
            SentNewsletters = sentNewsletters
        };

        return View(viewModel);
    }

    // GET: Admin/Newsletter/View/5
    public async Task<IActionResult> View(int id)
    {
        var newsletter = await _newsletterService.GetNewsletterByIdAsync(id);
        if (newsletter == null)
        {
            TempData["ErrorMessage"] = "Newsletter not found.";
            return RedirectToAction(nameof(History));
        }

        return View(newsletter);
    }

    // GET: Admin/Newsletter/Export
    public async Task<IActionResult> Export()
    {
        var csv = await _newsletterService.ExportSubscribersAsCsvAsync();
        var bytes = Encoding.UTF8.GetBytes(csv);
        var fileName = $"newsletter-subscribers-{DateTime.UtcNow:yyyy-MM-dd}.csv";

        return File(bytes, "text/csv", fileName);
    }

    // POST: Admin/Newsletter/DeleteSubscriber
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubscriber(int id)
    {
        var success = await _newsletterService.DeleteSubscriberAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = "Subscriber deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete subscriber.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Newsletter/DeleteDraft
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        var success = await _newsletterService.DeleteDraftAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = "Draft deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete draft. It may have already been sent.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void SetTinyMceApiKey()
    {
        ViewData["TinyMceApiKey"] = _configuration["TinyMCE:ApiKey"] ?? "no-api-key";
    }
}
