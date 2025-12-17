# ?? Playwright End-to-End Testing Guide

## Overview
This project uses **Microsoft Playwright** for automated end-to-end (E2E) testing of the Ellis Hope Foundation web application.

## What is Playwright?
Playwright is a modern automation framework that enables reliable end-to-end testing for web applications across all major browsers (Chromium, Firefox, WebKit).

## ? Benefits
- **Real browser testing** - Tests actual user interactions
- **Cross-browser support** - Chromium, Firefox, WebKit
- **Auto-wait** - Playwright waits for elements automatically
- **Screenshots & videos** - Visual debugging of failures
- **Code coverage** - Improves overall test coverage metrics

## ?? Setup

### First-Time Setup
1. **Install Playwright packages** (already done):
   ```bash
   dotnet add package Microsoft.Playwright
   dotnet add package Microsoft.Playwright.MSTest
   ```

2. **Build the test project**:
   ```bash
   dotnet build EllisHope.Tests
   ```

3. **Install browsers**:
   ```bash
   playwright install
   ```

## ?? Running Tests

### Run All E2E Tests
```bash
dotnet test EllisHope.Tests --filter "TestCategory=E2E"
```

### Run Happy Path Tests Only
```bash
dotnet test EllisHope.Tests --filter "TestCategory=HappyPath"
```

### Run with Headed Browser (See the browser)
```powershell
$env:HEADED="1"
dotnet test EllisHope.Tests --filter "TestCategory=E2E"
```

### Run Specific Test
```bash
dotnet test EllisHope.Tests --filter "FullyQualifiedName~HappyPath_CompleteApplicationWorkflow_Success"
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

### Test Structure
```csharp
[TestClass]
public class MyE2ETests : PageTest
{
    [TestMethod]
    [TestCategory("E2E")]
    public async Task MyTest_Scenario_ExpectedResult()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:7042/MyPage");
        
        // Act
        await Page.ClickAsync("text=Button Text");
        
        // Assert
        await Expect(Page).ToHaveURLAsync("expected-url");
    }
}
```

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

// Assertions
await Expect(Page.Locator("h1")).ToHaveTextAsync("Welcome");
await Expect(Page).ToHaveURLAsync("expected-url");
await Expect(Page.Locator(".alert")).ToBeVisibleAsync();
```

## ?? Debugging

### View Test Results
After running tests, open the HTML report:
```bash
playwright show-report
```

### Screenshots & Videos
Failed tests automatically capture:
- Screenshots: `test-results/*/screenshots/`
- Videos: `test-results/*/videos/`
- Traces: `test-results/*/traces/`

### Enable Trace Viewer
```bash
playwright show-trace test-results/trace.zip
```

## ?? Configuration

Edit `playwright.config.json` to customize:
- Timeouts
- Retries
- Browsers
- Screenshots/video settings
- Parallel execution

## ?? CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run Playwright Tests
  run: |
    dotnet build
    playwright install
    dotnet test --filter "TestCategory=E2E"
```

### Test Results
Playwright generates JUnit XML results compatible with most CI systems:
- File: `playwright-results.xml`
- Can be uploaded to test reporting tools

## ?? Important Notes

### Before Running Tests
1. **Start the application**:
   ```bash
   dotnet run --project EllisHope
   ```
   
2. **Verify the port** in `playwright.config.json` matches your app (default: 7042)

3. **Database state**: Tests may create/modify data - use a test database

### Test Data Cleanup
Consider implementing cleanup in `TestCleanup`:
```csharp
[TestCleanup]
public async Task TestCleanup()
{
    // Delete test users
    // Reset database state
    await Page.CloseAsync();
}
```

## ?? Current Tests

### ApplicationHappyPathTests.cs
- **HappyPath_CompleteApplicationWorkflow_Success**: Complete registration ? application ? submission
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

2. **Integrate with code coverage** (Codecov):
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **Set up CI/CD pipeline** to run tests automatically

4. **Add visual regression testing** using Playwright screenshots

## ?? Resources

- [Playwright for .NET Docs](https://playwright.dev/dotnet/)
- [API Reference](https://playwright.dev/dotnet/docs/api/class-playwright)
- [Best Practices](https://playwright.dev/dotnet/docs/best-practices)

---

**Happy Testing! ??**  
*Mr. Happy Path approves this automation!* ?
