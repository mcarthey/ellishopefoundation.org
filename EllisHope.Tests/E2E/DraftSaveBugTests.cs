using Microsoft.Playwright;
using Xunit;
using EllisHope.Tests.E2E;
using static Microsoft.Playwright.Assertions;

namespace EllisHope.Tests.E2E;

/// <summary>
/// E2E test for the draft save bug
/// This test MUST pass before we can consider the bug fixed!
/// </summary>
[Trait("Category", "E2E")]
[Trait("Category", "DraftSave")]
public class DraftSaveBugTests : PlaywrightTestBase
{
    private const string BaseUrl = "https://localhost:7049";

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

        // Step 3: Fill Step 1 with data
        await Page.FillAsync("input[name='FirstName']", "Test");
        await Page.FillAsync("input[name='LastName']", "User");
        await Page.FillAsync("input[name='PhoneNumber']", "555-1234");
        await Page.FillAsync("input[name='Email']", "test@savetest.com");
        await Page.FillAsync("input[name='Address']", "123 Test St");
        await Page.FillAsync("input[name='City']", "Milwaukee");
        await Page.FillAsync("input[name='State']", "WI");
        await Page.FillAsync("input[name='ZipCode']", "53202");

        // Step 4: Click "Save as Draft" to create the draft
        await Page.ClickAsync("button[name='SaveAsDraft']");

        // Wait for redirect to My Applications list
        await Page.WaitForURLAsync($"{BaseUrl}/MyApplications");
        
        // Verify draft was created
        await Expect(Page.Locator("text=Draft")).ToBeVisibleAsync();

        // Step 5: Click "Edit" to resume the draft
        await Page.ClickAsync("a:has-text('Edit'), a:has-text('Continue')");

        // Should be on Step 1 with our data
        var emailValue = await Page.InputValueAsync("input[name='Email']");
        Assert.Equal("test@savetest.com", emailValue);

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
        Assert.Equal("test@savetest.com", emailValue); // THIS IS THE KEY TEST!

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

        // Fill Step 1
        await Page.FillAsync("input[name='FirstName']", "Multi");
        await Page.FillAsync("input[name='LastName']", "Step");
        await Page.FillAsync("input[name='PhoneNumber']", "555-9999");
        await Page.FillAsync("input[name='Email']", "multistep@test.com");

        await Page.ClickAsync("button[name='SaveAsDraft']");
        await Page.WaitForURLAsync($"{BaseUrl}/MyApplications");

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
        Assert.Equal("multistep@test.com", email);

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
