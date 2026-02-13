using System.ComponentModel.DataAnnotations;

namespace EllisHope.Areas.Admin.Models;

public class GivebutterSettingsViewModel
{
    [Display(Name = "Enable Givebutter Widget")]
    public bool Enabled { get; set; }

    [Display(Name = "Account ID")]
    [MaxLength(100)]
    public string? AccountId { get; set; }

    [Display(Name = "Default Campaign URL")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    [MaxLength(500)]
    public string? DefaultCampaignUrl { get; set; }

    [Display(Name = "Default Widget ID")]
    [MaxLength(100)]
    public string? DefaultWidgetId { get; set; }
}
