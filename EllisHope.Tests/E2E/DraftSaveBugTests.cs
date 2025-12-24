using Microsoft.Playwright;
using Xunit;
using EllisHope.Tests.E2E;
using static Microsoft.Playwright.Assertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace EllisHope.Tests.E2E;

/// <summary>
/// E2E test for the draft save bug
/// This test MUST pass before we can consider the bug fixed!
/// Implements proper cleanup to remove test data after execution.
/// </summary>
[Trait("Category", "E2E")]
[Trait("Category", "DraftSave")]
public class DraftSaveBugTests : PlaywrightTestBase
{
    private const string BaseUrl = "https://localhost:7049";

    // Track application IDs created during tests for cleanup
    private readonly List<int> _createdApplicationIds = new();

    // Unique identifier for this test run to identify our test data
    private readonly string _testRunId = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Clean up any applications created during this test run
    /// Called automatically by xUnit after each test
    /// </summary>
    public override async Task DisposeAsync()
    {
        // First, try to clean up test applications from database
        await CleanupCreatedApplicationsAsync();

        // Then call base disposal (closes browser)
        await base.DisposeAsync();
    }

    private async Task CleanupCreatedApplicationsAsync()
    {
        if (_createdApplicationIds.Count == 0)
            return;

        try
        {
            // Try to delete applications through the database directly
            // This ensures complete cleanup even if UI operations fail
            var connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                await DeleteApplicationsFromDatabaseAsync(connectionString);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to clean up test applications: {ex.Message}");
            // Don't throw - we don't want cleanup failures to fail the test
        }
    }

    private string? GetConnectionString()
    {
        try
        {
            // Try to get connection string from appsettings
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "EllisHope"))
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            return config.GetConnectionString("DefaultConnection");
        }
        catch
        {
            // Fallback to a default local connection string for development
            return "Server=(localdb)\\mssqllocaldb;Database=EllisHopeFoundation;Trusted_Connection=True;MultipleActiveResultSets=true";
        }
    }

    private async Task DeleteApplicationsFromDatabaseAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var appId in _createdApplicationIds)
        {
            try
            {
                // Delete related records first (comments, votes, notifications)
                await using var deleteComments = new SqlCommand(
                    "DELETE FROM ApplicationComments WHERE ApplicationId = @Id", connection);
                deleteComments.Parameters.AddWithValue("@Id", appId);
                await deleteComments.ExecuteNonQueryAsync();

                await using var deleteVotes = new SqlCommand(
                    "DELETE FROM ApplicationVotes WHERE ApplicationId = @Id", connection);
                deleteVotes.Parameters.AddWithValue("@Id", appId);
                await deleteVotes.ExecuteNonQueryAsync();

                await using var deleteNotifications = new SqlCommand(
                    "DELETE FROM ApplicationNotifications WHERE ApplicationId = @Id", connection);
                deleteNotifications.Parameters.AddWithValue("@Id", appId);
                await deleteNotifications.ExecuteNonQueryAsync();

                // Then delete the application
                await using var deleteApp = new SqlCommand(
                    "DELETE FROM ClientApplications WHERE Id = @Id", connection);
                deleteApp.Parameters.AddWithValue("@Id", appId);
                await deleteApp.ExecuteNonQueryAsync();

                Console.WriteLine($"Cleaned up test application ID: {appId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to delete application {appId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Extract application ID from the current URL after navigating to edit page
    /// </summary>
    private int? ExtractApplicationIdFromUrl()
    {
        var url = Page.Url;
        // URL pattern: /MyApplications/Edit/123 or /MyApplications/Details/123
        var match = Regex.Match(url, @"/MyApplications/(?:Edit|Details)/(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
        {
            return id;
        }
        return null;
    }

    /// <summary>
    /// Extract application ID from the page after creating a draft
    /// Looks for the application ID in the page content or URL
    /// </summary>
    private async Task TrackCreatedApplicationAsync()
    {
        // After saving a draft and returning to MyApplications, find the new draft
        // and extract its ID by clicking Edit and checking the URL
        try
        {
            // Look for the Edit link on the most recent draft
            var editLink = Page.Locator("a:has-text('Edit Draft'), a:has-text('Edit')").First;
            if (await editLink.IsVisibleAsync())
            {
                // Get the href to extract the ID without navigating
                var href = await editLink.GetAttributeAsync("href");
                if (href != null)
                {
                    var match = Regex.Match(href, @"/(\d+)$");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                    {
                        if (!_createdApplicationIds.Contains(id))
                        {
                            _createdApplicationIds.Add(id);
                            Console.WriteLine($"Tracking test application ID: {id} for cleanup");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not track application ID: {ex.Message}");
        }
    }

    [Fact]
    public async Task DraftSave_FillStep1_ThenSaveOnStep2_PreservesEmail()
    {
        // This is THE critical test for the bug you found!
        // User fills Step 1, goes to Step 2, clicks "Save & Exit"
        // Email from Step 1 MUST be preserved!

        // Step 1: Login
        await E2EAuthenticationHelper.LoginAsHappyPathUserAsync(Page, BaseUrl);

        // Step 2: Create new draft application
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");

        // Use unique email to identify this test's data
        var testEmail = $"test_{_testRunId}@savetest.com";

        // Step 3: Fill Step 1 with data
        await Page.FillAsync("input[name='FirstName']", "Test");
        await Page.FillAsync("input[name='LastName']", "User");
        await Page.FillAsync("input[name='PhoneNumber']", "555-1234");
        await Page.FillAsync("input[name='Email']", testEmail);
        await Page.FillAsync("input[name='Address']", "123 Test St");
        await Page.FillAsync("input[name='City']", "Milwaukee");
        await Page.FillAsync("input[name='State']", "WI");
        await Page.FillAsync("input[name='ZipCode']", "53202");

        // Step 4: Click "Save as Draft" to create the draft
        await Page.ClickAsync("button[name='SaveAsDraft']");

        // Wait for redirect to My Applications list
        await Page.WaitForURLAsync($"{BaseUrl}/MyApplications");

        // Track the created application for cleanup
        await TrackCreatedApplicationAsync();

        // Verify draft was created
        await Expect(Page.Locator("text=Draft")).ToBeVisibleAsync();

        // Step 5: Click "Edit" to resume the draft
        await Page.ClickAsync("a:has-text('Edit'), a:has-text('Continue')");

        // Should be on Step 1 with our data
        var emailValue = await Page.InputValueAsync("input[name='Email']");
        Assert.Equal(testEmail, emailValue);

        // Step 6: Click "Next" to go to Step 2
        await Page.ClickAsync("button[name='NextStep']");

        // Should be on Step 2 now
        await Expect(Page.Locator("text=Step 2")).ToBeVisibleAsync();

        // Step 7: Fill some Step 2 data
        await Page.CheckAsync("input[value='GymMembership']");
        await Page.FillAsync("input[name='EstimatedMonthlyCost']", "150");

        // Step 8: Click "Save & Exit" (THE CRITICAL MOMENT!)
        await Page.ClickAsync("button[name='SaveAndExit']");

        // Wait for redirect to My Applications list
        await Page.WaitForURLAsync($"{BaseUrl}/MyApplications");

        // Verify success message
        await Expect(Page.Locator("text=Draft saved")).ToBeVisibleAsync();

        // Step 9: Edit the draft again to verify data was saved
        await Page.ClickAsync("a:has-text('Edit'), a:has-text('Continue')");

        // Step 10: Verify Step 1 data is STILL THERE!
        emailValue = await Page.InputValueAsync("input[name='Email']");
        Assert.Equal(testEmail, emailValue); // THIS IS THE KEY TEST!

        var firstNameValue = await Page.InputValueAsync("input[name='FirstName']");
        Assert.Equal("Test", firstNameValue);

        var phoneValue = await Page.InputValueAsync("input[name='PhoneNumber']");
        Assert.Equal("555-1234", phoneValue);

        // Step 11: Go to Step 2 and verify that data was saved too
        await Page.ClickAsync("button[name='NextStep']");

        var gymChecked = await Page.IsCheckedAsync("input[value='GymMembership']");
        Assert.True(gymChecked);

        var costValue = await Page.InputValueAsync("input[name='EstimatedMonthlyCost']");
        Assert.Equal("150", costValue);

        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
    }

    [Fact]
    public async Task DraftSave_FromStep3_PreservesAllPreviousData()
    {
        // Test saving from Step 3 doesn't overwrite Steps 1-2

        await E2EAuthenticationHelper.LoginAsHappyPathUserAsync(Page, BaseUrl);

        // Create draft
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");

        // Use unique email to identify this test's data
        var testEmail = $"multistep_{_testRunId}@test.com";

        // Fill Step 1
        await Page.FillAsync("input[name='FirstName']", "Multi");
        await Page.FillAsync("input[name='LastName']", "Step");
        await Page.FillAsync("input[name='PhoneNumber']", "555-9999");
        await Page.FillAsync("input[name='Email']", testEmail);

        await Page.ClickAsync("button[name='SaveAsDraft']");
        await Page.WaitForURLAsync($"{BaseUrl}/MyApplications");

        // Track the created application for cleanup
        await TrackCreatedApplicationAsync();

        // Edit and go to Step 2
        await Page.ClickAsync("a:has-text('Edit'), a:has-text('Continue')");
        await Page.ClickAsync("button[name='NextStep']");

        // Fill Step 2
        await Page.CheckAsync("input[value='PersonalTraining']");
        await Page.FillAsync("input[name='EstimatedMonthlyCost']", "200");

        await Page.ClickAsync("button[name='NextStep']");

        // Fill Step 3
        await Page.FillAsync("textarea[name='PersonalStatement']",
            "This is my personal statement that is long enough to meet the minimum character requirements for this field.");
        await Page.FillAsync("textarea[name='ExpectedBenefits']",
            "These are my expected benefits and they are also long enough to meet requirements.");
        await Page.FillAsync("textarea[name='CommitmentStatement']",
            "I am fully committed to this program and will dedicate the necessary time.");

        // Save from Step 3
        await Page.ClickAsync("button[name='SaveAndExit']");
        await Page.WaitForURLAsync($"{BaseUrl}/MyApplications");

        // Edit again and verify ALL data
        await Page.ClickAsync("a:has-text('Edit'), a:has-text('Continue')");

        // Check Step 1
        var email = await Page.InputValueAsync("input[name='Email']");
        Assert.Equal(testEmail, email);

        // Go to Step 2
        await Page.ClickAsync("button[name='NextStep']");
        var training = await Page.IsCheckedAsync("input[value='PersonalTraining']");
        Assert.True(training);

        // Go to Step 3
        await Page.ClickAsync("button[name='NextStep']");
        var statement = await Page.InputValueAsync("textarea[name='PersonalStatement']");
        Assert.Contains("personal statement", statement);

        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
    }
}
