namespace EllisHope.Services;

public interface ISiteSettingsService
{
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string? value, string? description = null);
    Task<Dictionary<string, string?>> GetSettingsByPrefixAsync(string prefix);

    // Typed helpers for Givebutter
    Task<bool> IsGivebutterEnabledAsync();
    Task<string?> GetGivebutterAccountIdAsync();
    Task<string?> GetDefaultDonationUrlAsync();
}
