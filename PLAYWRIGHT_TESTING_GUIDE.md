# ?? Playwright End-to-End Testing Guide

## Overview
This project uses **Microsoft Playwright** with **xUnit** for automated end-to-end (E2E) testing of the Ellis Hope Foundation web application.

## What is Playwright?
Playwright is a modern automation framework that enables reliable end-to-end testing for web applications across all major browsers (Chromium, Firefox, WebKit).

## ? Benefits
- **Real browser testing** - Tests actual user interactions
- **Cross-browser support** - Chromium, Firefox, WebKit
- **Auto-wait** - Playwright waits for elements automatically
- **Screenshots & videos** - Visual debugging of failures
- **Code coverage** - Improves overall test coverage metrics
- **Consistent with unit tests** - Uses xUnit like all other tests

## ?? Setup

### First-Time Setup
1. **Install Playwright packages** (already done):
   ```bash
   dotnet add package Microsoft.Playwright
   ```

2. **Build the test project**:
   ```bash
   dotnet build EllisHope.Tests
   ```

3. **Install browsers**:
   ```bash
   pwsh EllisHope.Tests/bin/Debug/net10.0/playwright.ps1 install
   ```

## ?? Running Tests

### Run All E2E Tests
```bash
dotnet test EllisHope.Tests --filter "Category=E2E"
```

### Run Happy Path Tests Only
```bash
dotnet test EllisHope.Tests --filter "Category=E2E&Category=HappyPath"
```

### Run with Headed Browser (See the browser)
```powershell
$env:HEADED="1"
dotnet test EllisHope.Tests --filter "Category=E2E"
```

### Run Specific Test
```bash
dotnet test EllisHope.Tests --filter "FullyQualifiedName~ApplicationForm_PreviousButton_NavigatesBackward"
```

## ?? Test Categories

| Category | Description |
|----------|-------------|
| `E2E` | All end-to-end tests |
| `HappyPath` | Complete user journey tests |
| `Navigation` | Navigation flow tests |
| `Validation` | Form validation tests |
| `EditDraft` | Draft editing tests |
| `Details` | Details page tests |

## ?? Writing New Tests

### Test Structure (xUnit)
```csharp
[Trait("Category", "E2E")]
public class MyE2ETests : PlaywrightTestBase
{
    [Fact]
    [Trait("Category", "MyCategory")]
    public async Task MyTest_Scenario_ExpectedResult()
    {
        // Arrange - Page is already initialized by base class
        await Page.GotoAsync("https://localhost:7042/MyPage");
        
        // Act
        await Page.ClickAsync("text=Button Text");
        
        // Assert - Use Playwright Assertions
        await Expect(Page).ToHaveURLAsync("expected-url");
    }
}
```

### PlaywrightTestBase Features

The base class provides:
- ? `Page` property - Ready to use
- ? `Browser` property - If you need it
- ? `Context` property - For advanced scenarios
- ? Auto-cleanup - Disposes everything automatically
- ? Configurable options - Override `LaunchOptions` or `ContextOptions`

### Common Playwright Actions

```csharp
// Navigate
await Page.GotoAsync("url");

// Click
await Page.ClickAsync("text=Login");
await Page.ClickAsync("button[type='submit']");

// Fill form
await Page.FillAsync("input[name='Email']", "test@test.com");

// Check checkbox
await Page.CheckAsync("input[type='checkbox']");

// Select dropdown
await Page.SelectOptionAsync("select[name='State']", "WI");

// Wait for element
await Page.WaitForSelectorAsync(".success-message");

// Assertions (using static Microsoft.Playwright.Assertions)
await Expect(Page.Locator("h1")).ToHaveTextAsync("Welcome");
await Expect(Page).ToHaveURLAsync("expected-url");
await Expect(Page.Locator(".alert")).ToBeVisibleAsync();
```

## ?? Debugging

### View Test Results
After running tests, failed tests will have:
- Screenshots in `test-results/*/screenshots/`
- Videos in `test-results/*/videos/` (if configured)

### Enable Trace Viewer
Add to your test:
```csharp
await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true });
// ... test code ...
await Context.Tracing.StopAsync(new() { Path = "trace.zip" });
```

Then view:
```bash
pwsh EllisHope.Tests/bin/Debug/net10.0/playwright.ps1 show-trace trace.zip
```

### Run Tests with Visible Browser
```powershell
$env:HEADED="1"
dotnet test --filter "Category=E2E"
```

### Slow Down Tests (for debugging)
```powershell
$env:SLOWMO="1000"  # 1 second delay between actions
dotnet test --filter "Category=E2E"
```

## ?? Configuration

### Customizing Browser Options

Override in your test class:

```csharp
public class MyTests : PlaywrightTestBase
{
    protected override BrowserTypeLaunchOptions LaunchOptions => new()
    {
        Headless = false,  // Always show browser
        SlowMo = 500       // 500ms delay
    };
}
```

## ?? CI/CD Integration

### GitHub Actions
Tests run automatically via `.github/workflows/dotnet-ci.yml`:

```yaml
e2e-tests:
  - Install Playwright browsers
  - Start application
  - Run E2E tests
  - Upload screenshots/videos on failure
```

### Test Results
- View in GitHub Actions ? Artifacts
- Screenshots and videos available for failed tests

## ?? Important Notes

### Before Running Tests Locally
1. **Start the application**:
   ```bash
   dotnet run --project EllisHope
   ```
   
2. **Verify the port** in tests matches your app (default: 7042)

3. **Database state**: Tests may create/modify data - use a test database

### Test Data Cleanup
The `PlaywrightTestBase` handles browser cleanup automatically via `IAsyncLifetime`.
Add custom cleanup in your test if needed:

```csharp
public override async Task DisposeAsync()
{
    // Custom cleanup
    await base.DisposeAsync();  // Always call base!
}
```

## ?? Current Tests

### ApplicationHappyPathTests.cs
- **HappyPath_CompleteApplicationWorkflow_Success**: Complete registration ? application ? submission (skipped by default)
- **ApplicationForm_PreviousButton_NavigatesBackward**: Previous button navigation
- **ApplicationForm_SaveAsDraft_SavesAndRedirects**: Draft saving functionality
- **ApplicationForm_Step3_RequiresMinimum50Characters**: Field validation
- **EditDraftApplication_LoadsCorrectly**: Edit draft functionality
- **ApplicationDetails_DisplaysCorrectly**: Details page layout

## ?? Next Steps

1. **Add more test scenarios**:
   - Error handling
   - Edge cases
   - Different user roles

2. **Integrate with code coverage** (Already configured!):
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **Visual regression testing** using Playwright screenshots

## ?? Resources

- [Playwright for .NET Docs](https://playwright.dev/dotnet/)
- [API Reference](https://playwright.dev/dotnet/docs/api/class-playwright)
- [Best Practices](https://playwright.dev/dotnet/docs/best-practices)
- [xUnit Documentation](https://xunit.net/)

---

**Happy Testing! ??**  
*All tests now use xUnit for consistency!* ?
