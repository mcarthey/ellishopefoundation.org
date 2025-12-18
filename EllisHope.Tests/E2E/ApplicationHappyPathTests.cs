using Microsoft.Playwright;
using Xunit;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace EllisHope.Tests.E2E;

/// <summary>
/// End-to-end tests for the Happy Path user journey
/// Tests the complete application workflow from registration to submission
/// </summary>
[Trait("Category", "E2E")]
public class ApplicationHappyPathTests : PlaywrightTestBase
{
    private const string BaseUrl = "https://localhost:7042";
    private const string TestEmail = "happy.path@test.com";
    private const string TestPassword = "HappyPath@123";

    [Fact(Skip = "Full workflow test - enable when needed")]
    public async Task HappyPath_CompleteApplicationWorkflow_Success()
    {
        // Step 1: Navigate to home page
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Step 2: Click "Get Involved" or "Apply" link
        var applyLink = Page.Locator("text=Apply for Support").Or(Page.Locator("text=Get Involved"));
        await applyLink.ClickAsync();

        // Should redirect to login if not authenticated
        await Expect(Page).ToHaveURLAsync(new Regex(".*Login.*"), new() { Timeout = 5000 });

        // Step 3: Click "Register" link
        await Page.ClickAsync("text=Register");
        await Expect(Page).ToHaveURLAsync(new Regex(".*Register.*"));

        // Step 4: Fill registration form
        await Page.FillAsync("input[name='Email']", TestEmail);
        await Page.FillAsync("input[name='FirstName']", "Happy");
        await Page.FillAsync("input[name='LastName']", "Path");
        await Page.FillAsync("input[name='Password']", TestPassword);
        await Page.FillAsync("input[name='ConfirmPassword']", TestPassword);

        // Submit registration
        await Page.ClickAsync("button[type='submit']:has-text('Register')");

        // Wait for successful registration (should redirect to dashboard or profile)
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Step 5: Navigate to "My Applications"
        var myApplicationsLink = Page.Locator("text=My Applications").Or(Page.Locator("a[href*='MyApplications']"));
        await myApplicationsLink.ClickAsync();
        
        // Step 6: Click "Create New Application"
        await Page.ClickAsync("text=Apply for Support");

        // Step 7: Fill Step 1 - Personal Information
        await Expect(Page.Locator("h3:has-text('Step 1: Personal Information')")).ToBeVisibleAsync();
        
        await Page.FillAsync("input[name='PhoneNumber']", "262-555-1111");
        await Page.FillAsync("input[name='Address']", "123 Pine St");
        await Page.FillAsync("input[name='City']", "Here");
        await Page.FillAsync("input[name='State']", "WI");
        await Page.FillAsync("input[name='ZipCode']", "51342");

        // Click Next
        await Page.ClickAsync("button[name='NextStep']");

        // Step 8: Fill Step 2 - Funding Request
        await Expect(Page.Locator("h3:has-text('Step 2: Program Interest')")).ToBeVisibleAsync();
        
        await Page.CheckAsync("input[value='GymMembership']");
        await Page.CheckAsync("input[value='PersonalTraining']");
        await Page.FillAsync("input[name='EstimatedMonthlyCost']", "150");
        await Page.FillAsync("input[name='ProgramDurationMonths']", "12");

        // Click Next
        await Page.ClickAsync("button[name='NextStep']");

        // Step 9: Fill Step 3 - Motivation
        await Expect(Page.Locator("h3:has-text('Step 3: Motivation')")).ToBeVisibleAsync();
        
        await Page.FillAsync("textarea[name='PersonalStatement']", 
            "I am excited to join this program and improve my health and fitness. This opportunity means a lot to me.");
        await Page.FillAsync("textarea[name='ExpectedBenefits']", 
            "I will gain better health, more energy, and improved quality of life through this program.");
        await Page.FillAsync("textarea[name='CommitmentStatement']", 
            "I am fully committed to completing this 12-month program and will devote the necessary time and energy.");

        // Click Next
        await Page.ClickAsync("button[name='NextStep']");

        // Step 10: Fill Step 4 - Health & Fitness
        await Expect(Page.Locator("h3:has-text('Step 4: Health')")).ToBeVisibleAsync();
        
        await Page.SelectOptionAsync("select[name='CurrentFitnessLevel']", "Beginner");
        await Page.FillAsync("textarea[name='FitnessGoals']", "Lose weight and build strength");

        // Click Next
        await Page.ClickAsync("button[name='NextStep']");

        // Step 11: Check Step 5 - Program Requirements
        await Expect(Page.Locator("h3:has-text('Step 5: Program Requirements')")).ToBeVisibleAsync();
        
        await Page.CheckAsync("input[name='AgreesToNutritionist']");
        await Page.CheckAsync("input[name='AgreesToPersonalTrainer']");
        await Page.CheckAsync("input[name='AgreesToWeeklyCheckIns']");
        await Page.CheckAsync("input[name='AgreesToProgressReports']");
        await Page.CheckAsync("input[name='UnderstandsCommitment']");

        // Click Next
        await Page.ClickAsync("button[name='NextStep']");

        // Step 12: Fill Step 6 - Signature & Submit
        await Expect(Page.Locator("h3:has-text('Step 6: Review')")).ToBeVisibleAsync();
        
        await Page.FillAsync("input[name='Signature']", "Happy Path");

        // Submit application
        await Page.ClickAsync("button[name='SubmitApplication']");

        // Wait for success
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify success message or redirect to details page
        await Expect(Page.Locator("text=Application submitted successfully").Or(
            Page.Locator("text=Submitted"))).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "Navigation")]
    public async Task ApplicationForm_PreviousButton_NavigatesBackward()
    {
        // Navigate to application form (assuming logged in)
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");

        // Go to step 2
        await Page.ClickAsync("button[name='NextStep']");
        await Expect(Page.Locator("h3:has-text('Step 2')")).ToBeVisibleAsync();

        // Click Previous
        await Page.ClickAsync("button[name='PreviousStep']");

        // Should be back at step 1
        await Expect(Page.Locator("h3:has-text('Step 1')")).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "Navigation")]
    public async Task ApplicationForm_SaveAsDraft_SavesAndRedirects()
    {
        // Navigate to application form
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");

        // Fill some data
        await Page.FillAsync("input[name='PhoneNumber']", "262-555-1111");

        // Click Save as Draft
        await Page.ClickAsync("button[name='SaveAsDraft']");

        // Should redirect to My Applications
        await Expect(Page).ToHaveURLAsync(new Regex(".*MyApplications$"));
        
        // Should see success message
        await Expect(Page.Locator("text=saved as draft")).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "Validation")]
    public async Task ApplicationForm_Step3_RequiresMinimum50Characters()
    {
        // Navigate to step 3
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");
        
        // Navigate through steps to get to step 3
        await Page.ClickAsync("button[name='NextStep']"); // Step 2
        await Page.CheckAsync("input[value='GymMembership']"); // Select at least one
        await Page.ClickAsync("button[name='NextStep']"); // Step 3

        // Fill with less than 50 characters
        await Page.FillAsync("textarea[name='PersonalStatement']", "Too short");
        
        // Try to proceed
        await Page.ClickAsync("button[name='NextStep']");

        // Should see validation error
        await Expect(Page.Locator("text=minimum 50 characters")).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "EditDraft")]
    public async Task EditDraftApplication_LoadsCorrectly()
    {
        // Assuming there's a draft application with ID 1
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Edit/1");

        // Should load edit form
        await Expect(Page.Locator("h2:has-text('Edit Application')")).ToBeVisibleAsync();
        
        // Should have Exit button
        await Expect(Page.Locator("text=Exit & Save Later")).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "Details")]
    public async Task ApplicationDetails_DisplaysCorrectly()
    {
        // Navigate to application details
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Details/1");

        // Should have proper padding (not covered by header)
        var header = Page.Locator("h2:has-text('Application')");
        await Expect(header).ToBeVisibleAsync();

        // Check that header is not covered by navigation
        var headerBox = await header.BoundingBoxAsync();
        Assert.NotNull(headerBox);
        Assert.True(headerBox.Y > 80, "Header should be below navigation bar");
    }
}
