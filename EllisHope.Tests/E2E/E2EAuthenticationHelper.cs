using Microsoft.Playwright;

namespace EllisHope.Tests.E2E;

/// <summary>
/// Helper class for authenticating users in E2E tests
/// Provides methods to login as different user types
/// </summary>
public static class E2EAuthenticationHelper
{
    /// <summary>
    /// Login as a specific user
    /// </summary>
    public static async Task LoginAsync(IPage page, string baseUrl, string email, string password)
    {
        // Navigate to login page
        await page.GotoAsync($"{baseUrl}/Admin/Account/Login");
        
        // Fill login form
        await page.FillAsync("input[name='Email']", email);
        await page.FillAsync("input[name='Password']", password);
        
        // Submit
        await page.ClickAsync("button[type='submit']:has-text('Sign In')");
        
        // Wait for redirect
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Login as the happy path test user
    /// </summary>
    public static async Task LoginAsHappyPathUserAsync(IPage page, string baseUrl)
    {
        await LoginAsync(page, baseUrl, "mcarthey+happy@gmail.com", "Password123!");
    }

    /// <summary>
    /// Login as the rejection test user
    /// </summary>
    public static async Task LoginAsRejectUserAsync(IPage page, string baseUrl)
    {
        await LoginAsync(page, baseUrl, "mcarthey+reject@gmail.com", "Password123!");
    }

    /// <summary>
    /// Login as a board member
    /// </summary>
    public static async Task LoginAsBoardMember1Async(IPage page, string baseUrl)
    {
        await LoginAsync(page, baseUrl, "mcarthey+board1@gmail.com", "Password123!");
    }

    /// <summary>
    /// Login as second board member
    /// </summary>
    public static async Task LoginAsBoardMember2Async(IPage page, string baseUrl)
    {
        await LoginAsync(page, baseUrl, "mcarthey+board2@gmail.com", "Password123!");
    }

    /// <summary>
    /// Login as client 1
    /// </summary>
    public static async Task LoginAsClient1Async(IPage page, string baseUrl)
    {
        await LoginAsync(page, baseUrl, "mcarthey+client1@gmail.com", "Password123!");
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    public static async Task LogoutAsync(IPage page, string baseUrl)
    {
        // Try to find and click logout button
        try
        {
            // Wait for account dropdown to be visible
            var accountToggle = page.Locator("#accountMenuToggle, #publicAccountMenuToggle").First;
            await accountToggle.ClickAsync(new() { Timeout = 2000 });
            
            // Click logout button in dropdown
            await page.ClickAsync("button:has-text('Sign Out')", new() { Timeout = 2000 });
            
            // Wait for redirect
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        catch
        {
            // If dropdown method fails, navigate directly to login
            await page.GotoAsync($"{baseUrl}/Admin/Account/Login");
        }
    }

    /// <summary>
    /// Check if user is currently logged in
    /// </summary>
    public static async Task<bool> IsLoggedInAsync(IPage page)
    {
        // Check if account menu is visible (indicates logged in)
        var accountMenu = page.Locator("#accountMenuToggle, #publicAccountMenuToggle").First;
        try
        {
            await accountMenu.WaitForAsync(new() { Timeout = 1000, State = WaitForSelectorState.Visible });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
