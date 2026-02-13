using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace EllisHope.Tests.Services;

public class SiteSettingsServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly SiteSettingsService _service;

    public SiteSettingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new SiteSettingsService(_context, _cache);
    }

    [Fact]
    public async Task GetSettingAsync_ReturnsNull_WhenKeyNotFound()
    {
        var result = await _service.GetSettingAsync("NonExistent.Key");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingAsync_ReturnsValue_WhenKeyExists()
    {
        _context.SiteSettings.Add(new SiteSetting
        {
            Key = "Test.Key",
            Value = "TestValue"
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetSettingAsync("Test.Key");
        Assert.Equal("TestValue", result);
    }

    [Fact]
    public async Task SetSettingAsync_CreatesNewSetting_WhenKeyNotExists()
    {
        await _service.SetSettingAsync("New.Key", "NewValue", "A new setting");

        var setting = await _context.SiteSettings.FindAsync("New.Key");
        Assert.NotNull(setting);
        Assert.Equal("NewValue", setting.Value);
        Assert.Equal("A new setting", setting.Description);
    }

    [Fact]
    public async Task SetSettingAsync_UpdatesExistingSetting()
    {
        _context.SiteSettings.Add(new SiteSetting
        {
            Key = "Existing.Key",
            Value = "OldValue",
            Description = "Old description"
        });
        await _context.SaveChangesAsync();

        await _service.SetSettingAsync("Existing.Key", "NewValue");

        var setting = await _context.SiteSettings.FindAsync("Existing.Key");
        Assert.Equal("NewValue", setting!.Value);
    }

    [Fact]
    public async Task SetSettingAsync_InvalidatesCache()
    {
        _context.SiteSettings.Add(new SiteSetting
        {
            Key = "Cache.Key",
            Value = "Original"
        });
        await _context.SaveChangesAsync();

        // Populate cache
        var first = await _service.GetSettingAsync("Cache.Key");
        Assert.Equal("Original", first);

        // Update directly in DB and via service
        await _service.SetSettingAsync("Cache.Key", "Updated");

        var second = await _service.GetSettingAsync("Cache.Key");
        Assert.Equal("Updated", second);
    }

    [Fact]
    public async Task GetSettingsByPrefixAsync_ReturnsMatchingSettings()
    {
        _context.SiteSettings.AddRange(
            new SiteSetting { Key = "Givebutter.Enabled", Value = "true" },
            new SiteSetting { Key = "Givebutter.AccountId", Value = "abc123" },
            new SiteSetting { Key = "Other.Setting", Value = "ignored" }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetSettingsByPrefixAsync("Givebutter.");
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Givebutter.Enabled"));
        Assert.True(result.ContainsKey("Givebutter.AccountId"));
    }

    [Fact]
    public async Task IsGivebutterEnabledAsync_ReturnsTrue_WhenSetToTrue()
    {
        _context.SiteSettings.Add(new SiteSetting
        {
            Key = "Givebutter.Enabled",
            Value = "True"
        });
        await _context.SaveChangesAsync();

        var result = await _service.IsGivebutterEnabledAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task IsGivebutterEnabledAsync_ReturnsFalse_WhenNotSet()
    {
        var result = await _service.IsGivebutterEnabledAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task IsGivebutterEnabledAsync_ReturnsFalse_WhenSetToFalse()
    {
        _context.SiteSettings.Add(new SiteSetting
        {
            Key = "Givebutter.Enabled",
            Value = "False"
        });
        await _context.SaveChangesAsync();

        var result = await _service.IsGivebutterEnabledAsync();
        Assert.False(result);
    }

    [Fact]
    public async Task GetGivebutterAccountIdAsync_ReturnsValue()
    {
        _context.SiteSettings.Add(new SiteSetting
        {
            Key = "Givebutter.AccountId",
            Value = "hT6RjF97wDnuVW83"
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetGivebutterAccountIdAsync();
        Assert.Equal("hT6RjF97wDnuVW83", result);
    }

    [Fact]
    public async Task GetDefaultDonationUrlAsync_ReturnsUrl()
    {
        _context.SiteSettings.Add(new SiteSetting
        {
            Key = "Givebutter.DefaultCampaignUrl",
            Value = "https://givebutter.com/QMBsZm"
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetDefaultDonationUrlAsync();
        Assert.Equal("https://givebutter.com/QMBsZm", result);
    }

    [Fact]
    public async Task GetDefaultDonationUrlAsync_ReturnsNull_WhenNotSet()
    {
        var result = await _service.GetDefaultDonationUrlAsync();
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
    }
}
