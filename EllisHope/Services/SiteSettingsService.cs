using EllisHope.Data;
using EllisHope.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EllisHope.Services;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CachePrefix = "SiteSetting_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public SiteSettingsService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var cacheKey = CachePrefix + key;
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        var setting = await _context.SiteSettings.FindAsync(key);
        var value = setting?.Value;

        _cache.Set(cacheKey, value, CacheDuration);
        return value;
    }

    public async Task SetSettingAsync(string key, string? value, string? description = null)
    {
        var setting = await _context.SiteSettings.FindAsync(key);
        if (setting == null)
        {
            setting = new SiteSetting
            {
                Key = key,
                Value = value,
                Description = description,
                UpdatedDate = DateTime.UtcNow
            };
            _context.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedDate = DateTime.UtcNow;
            if (description != null)
                setting.Description = description;
        }

        await _context.SaveChangesAsync();
        _cache.Remove(CachePrefix + key);
    }

    public async Task<Dictionary<string, string?>> GetSettingsByPrefixAsync(string prefix)
    {
        return await _context.SiteSettings
            .Where(s => s.Key.StartsWith(prefix))
            .ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public async Task<bool> IsGivebutterEnabledAsync()
    {
        var value = await GetSettingAsync("Givebutter.Enabled");
        return bool.TryParse(value, out var enabled) && enabled;
    }

    public async Task<string?> GetGivebutterAccountIdAsync()
    {
        return await GetSettingAsync("Givebutter.AccountId");
    }

    public async Task<string?> GetDefaultDonationUrlAsync()
    {
        return await GetSettingAsync("Givebutter.DefaultCampaignUrl");
    }
}
