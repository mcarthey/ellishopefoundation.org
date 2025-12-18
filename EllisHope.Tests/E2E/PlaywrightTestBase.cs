using Microsoft.Playwright;
using Xunit;

namespace EllisHope.Tests.E2E;

/// <summary>
/// Base class for Playwright E2E tests using xUnit
/// Provides automatic browser lifecycle management
/// </summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright? Playwright { get; private set; }
    protected IBrowser? Browser { get; private set; }
    protected IBrowserContext? Context { get; private set; }
    protected IPage Page { get; private set; } = null!;

    protected virtual BrowserTypeLaunchOptions LaunchOptions => new()
    {
        Headless = Environment.GetEnvironmentVariable("HEADED") != "1",
        SlowMo = Environment.GetEnvironmentVariable("SLOWMO") != null 
            ? int.Parse(Environment.GetEnvironmentVariable("SLOWMO")!) 
            : 0
    };

    protected virtual BrowserNewContextOptions ContextOptions => new()
    {
        IgnoreHTTPSErrors = true,
        ViewportSize = new() { Width = 1920, Height = 1080 }
    };

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(LaunchOptions);
        Context = await Browser.NewContextAsync(ContextOptions);
        Page = await Context.NewPageAsync();
        
        Page.SetDefaultTimeout(15000);
        Page.SetDefaultNavigationTimeout(30000);
    }

    public async Task DisposeAsync()
    {
        if (Page != null)
        {
            await Page.CloseAsync();
        }
        
        if (Context != null)
        {
            await Context.CloseAsync();
        }
        
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
        
        Playwright?.Dispose();
    }
}
