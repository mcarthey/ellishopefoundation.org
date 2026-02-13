using EllisHope.Areas.Admin.Models;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(IgnoreApi = true)]
public class SettingsController : Controller
{
    private readonly ISiteSettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISiteSettingsService settingsService,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    // GET: Admin/Settings
    public async Task<IActionResult> Index()
    {
        var model = new GivebutterSettingsViewModel
        {
            Enabled = await _settingsService.IsGivebutterEnabledAsync(),
            AccountId = await _settingsService.GetGivebutterAccountIdAsync(),
            DefaultCampaignUrl = await _settingsService.GetDefaultDonationUrlAsync(),
            DefaultWidgetId = await _settingsService.GetDefaultWidgetIdAsync()
        };

        return View(model);
    }

    // POST: Admin/Settings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(GivebutterSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the errors below.";
            return View(model);
        }

        await _settingsService.SetSettingAsync("Givebutter.Enabled",
            model.Enabled.ToString(), "Enable Givebutter donation widget");
        await _settingsService.SetSettingAsync("Givebutter.AccountId",
            model.AccountId, "Givebutter account ID");
        await _settingsService.SetSettingAsync("Givebutter.DefaultCampaignUrl",
            model.DefaultCampaignUrl, "Default donation campaign URL");
        await _settingsService.SetSettingAsync("Givebutter.DefaultWidgetId",
            model.DefaultWidgetId, "Givebutter widget ID for donation overlay");

        _logger.LogInformation("Givebutter settings updated by {User}", User.Identity?.Name);
        TempData["SuccessMessage"] = "Donation settings saved successfully!";
        return RedirectToAction(nameof(Index));
    }
}
