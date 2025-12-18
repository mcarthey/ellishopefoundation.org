using Microsoft.Playwright;
using Xunit;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace EllisHope.Tests.E2E;

/// <summary>
/// End-to-end tests for complete application workflows
/// Tests the scenarios from your manual test plan
/// </summary>
[Trait("Category", "E2E")]
[Trait("Category", "Scenarios")]
public class ApplicationWorkflowScenarios : PlaywrightTestBase
{
    private const string BaseUrl = "https://localhost:7049";

    /// <summary>
    /// Test Case 1: Happy Path
    /// User: mcarthey+happy@gmail.com
    /// Scenario: Submit ? Review ? Vote ? Approve
    /// Expected: All emails received
    /// </summary>
    [Fact(Skip = "Requires app running - run manually before release")]
    public async Task Scenario_HappyPath_SubmitReviewVoteApprove()
    {
        // Step 1: Login as happy path user
        await E2EAuthenticationHelper.LoginAsHappyPathUserAsync(Page, BaseUrl);
        
        // Step 2: Create and submit application
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");
        
        // Fill required fields (Step 1)
        await Page.FillAsync("input[name='PhoneNumber']", "262-555-1111");
        await Page.FillAsync("input[name='Address']", "123 Happy St");
        await Page.FillAsync("input[name='City']", "Milwaukee");
        await Page.FillAsync("input[name='State']", "WI");
        await Page.FillAsync("input[name='ZipCode']", "53202");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Step 2: Program Interest
        await Page.CheckAsync("input[value='GymMembership']");
        await Page.FillAsync("input[name='EstimatedMonthlyCost']", "150");
        await Page.FillAsync("input[name='ProgramDurationMonths']", "12");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Step 3: Motivation
        await Page.FillAsync("textarea[name='PersonalStatement']", 
            "I am very excited about this opportunity to improve my health and fitness. This program will help me achieve my goals.");
        await Page.FillAsync("textarea[name='ExpectedBenefits']", 
            "I expect to gain better health, more energy, and improved quality of life through this program.");
        await Page.FillAsync("textarea[name='CommitmentStatement']", 
            "I am fully committed to completing this program and will dedicate the necessary time and effort.");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Step 4: Health
        await Page.SelectOptionAsync("select[name='CurrentFitnessLevel']", "Beginner");
        await Page.FillAsync("textarea[name='FitnessGoals']", "Lose weight and build strength");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Step 5: Requirements
        await Page.CheckAsync("input[name='AgreesToNutritionist']");
        await Page.CheckAsync("input[name='AgreesToPersonalTrainer']");
        await Page.CheckAsync("input[name='AgreesToWeeklyCheckIns']");
        await Page.CheckAsync("input[name='AgreesToProgressReports']");
        await Page.CheckAsync("input[name='UnderstandsCommitment']");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Step 6: Review & Submit
        await Page.FillAsync("input[name='Signature']", "Happy Path User");
        await Page.ClickAsync("button[name='SubmitApplication']");
        
        // Verify submission success
        await Expect(Page.Locator("text=submitted").Or(
            Page.Locator("text=Submitted"))).ToBeVisibleAsync(new() { Timeout = 10000 });
        
        // TODO: Verify email was sent (check email service logs or mock)
        
        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
        
        // Manual verification step:
        // 1. Check mcarthey+happy@gmail.com for submission confirmation email
        // 2. Admin should start review
        // 3. Board members should vote
        // 4. Admin should approve
        // 5. Check for approval email
    }

    /// <summary>
    /// Test Case 2: Rejection Path
    /// User: mcarthey+reject@gmail.com
    /// Scenario: Submit ? Review ? Vote ? Reject
    /// Expected: Rejection email received
    /// </summary>
    [Fact(Skip = "Requires app running - run manually before release")]
    public async Task Scenario_RejectionPath_SubmitReviewVoteReject()
    {
        // Step 1: Login as reject test user
        await E2EAuthenticationHelper.LoginAsRejectUserAsync(Page, BaseUrl);
        
        // Step 2: Create minimal application (to be rejected)
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");
        
        // Fill minimal required fields
        await Page.FillAsync("input[name='PhoneNumber']", "262-555-2222");
        await Page.FillAsync("input[name='Address']", "456 Reject Ave");
        await Page.FillAsync("input[name='City']", "Milwaukee");
        await Page.FillAsync("input[name='State']", "WI");
        await Page.FillAsync("input[name='ZipCode']", "53203");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Program Interest
        await Page.CheckAsync("input[value='GymMembership']");
        await Page.FillAsync("input[name='EstimatedMonthlyCost']", "200");
        await Page.FillAsync("input[name='ProgramDurationMonths']", "6");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Motivation (minimal)
        await Page.FillAsync("textarea[name='PersonalStatement']", 
            "I want to join this program for personal reasons and goals.");
        await Page.FillAsync("textarea[name='ExpectedBenefits']", 
            "I expect to get healthier and feel better overall through this.");
        await Page.FillAsync("textarea[name='CommitmentStatement']", 
            "I will try my best to participate in the program activities.");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Health
        await Page.SelectOptionAsync("select[name='CurrentFitnessLevel']", "Sedentary");
        await Page.FillAsync("textarea[name='FitnessGoals']", "Get fit");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Requirements
        await Page.CheckAsync("input[name='AgreesToNutritionist']");
        await Page.CheckAsync("input[name='AgreesToPersonalTrainer']");
        await Page.CheckAsync("input[name='AgreesToWeeklyCheckIns']");
        await Page.CheckAsync("input[name='AgreesToProgressReports']");
        await Page.CheckAsync("input[name='UnderstandsCommitment']");
        await Page.ClickAsync("button[name='NextStep']");
        
        // Submit
        await Page.FillAsync("input[name='Signature']", "Reject Test");
        await Page.ClickAsync("button[name='SubmitApplication']");
        
        // Verify submission
        await Expect(Page.Locator("text=submitted").Or(
            Page.Locator("text=Submitted"))).ToBeVisibleAsync(new() { Timeout = 10000 });
        
        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
        
        // Manual verification:
        // 1. Board members vote to reject
        // 2. Admin rejects application
        // 3. Check mcarthey+reject@gmail.com for rejection email
    }

    /// <summary>
    /// Board Member Workflow: Review and vote on applications
    /// </summary>
    [Fact(Skip = "Requires app running and submitted applications - run manually")]
    public async Task Scenario_BoardMember_ReviewAndVote()
    {
        // Login as board member 1
        await E2EAuthenticationHelper.LoginAsBoardMember1Async(Page, BaseUrl);
        
        // Navigate to applications needing review
        await Page.GotoAsync($"{BaseUrl}/Admin/Applications/NeedingReview");
        
        // Should see applications in review
        await Expect(Page.Locator("text=Applications")).ToBeVisibleAsync();
        
        // Click on first application to review
        var firstApp = Page.Locator("a:has-text('Details')").First;
        await firstApp.ClickAsync();
        
        // On details page, cast a vote
        await Page.ClickAsync("button:has-text('Vote')");
        
        // Fill vote form
        await Page.CheckAsync("input[value='Approve']");
        await Page.FillAsync("textarea[name='Reasoning']", 
            "This applicant shows strong commitment and clear goals. I support approval.");
        await Page.SelectOptionAsync("select[name='ConfidenceLevel']", "4");
        
        // Submit vote
        await Page.ClickAsync("button[type='submit']:has-text('Submit Vote')");
        
        // Verify vote was recorded
        await Expect(Page.Locator("text=vote").Or(
            Page.Locator("text=Vote"))).ToBeVisibleAsync();
        
        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
    }

    /// <summary>
    /// Client Portal: View progress and resources
    /// </summary>
    [Fact(Skip = "Requires app running - run manually")]
    public async Task Scenario_Client_ViewProgressAndResources()
    {
        // Login as client
        await E2EAuthenticationHelper.LoginAsClient1Async(Page, BaseUrl);
        
        // Navigate to client dashboard
        await Page.GotoAsync($"{BaseUrl}/Admin/Client/Dashboard");
        
        // Verify client dashboard elements
        await Expect(Page.Locator("h2:has-text('Dashboard')")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Alice Johnson").Or(
            Page.Locator("text=Client"))).ToBeVisibleAsync();
        
        // Check sponsor information if assigned
        var sponsorSection = Page.Locator("text=Sponsor");
        if (await sponsorSection.IsVisibleAsync())
        {
            // Verify sponsor details are shown
            await Expect(sponsorSection).ToBeVisibleAsync();
        }
        
        // Navigate to progress page
        await Page.ClickAsync("a:has-text('Progress')");
        await Expect(Page.Locator("text=Progress").Or(
            Page.Locator("text=Milestones"))).ToBeVisibleAsync();
        
        // Navigate to resources
        await Page.ClickAsync("a:has-text('Resources')");
        await Expect(Page.Locator("h2:has-text('Resources')")).ToBeVisibleAsync();
        
        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
    }

    /// <summary>
    /// Draft workflow: Save draft, logout, login, resume
    /// </summary>
    [Fact(Skip = "Requires app running - run manually")]
    public async Task Scenario_DraftWorkflow_SaveResumeComplete()
    {
        // Login
        await E2EAuthenticationHelper.LoginAsync(Page, BaseUrl, 
            "mcarthey+moreinfo@gmail.com", "Password123!");
        
        // Start application
        await Page.GotoAsync($"{BaseUrl}/MyApplications/Create");
        
        // Fill partial data
        await Page.FillAsync("input[name='PhoneNumber']", "262-555-3333");
        await Page.FillAsync("input[name='Address']", "789 Draft Lane");
        
        // Save as draft
        await Page.ClickAsync("button[name='SaveAsDraft']");
        
        // Verify draft saved
        await Expect(Page.Locator("text=Draft").Or(
            Page.Locator("text=saved"))).ToBeVisibleAsync();
        
        // Logout
        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
        
        // Login again
        await E2EAuthenticationHelper.LoginAsync(Page, BaseUrl, 
            "mcarthey+moreinfo@gmail.com", "Password123!");
        
        // Go to MyApplications
        await Page.GotoAsync($"{BaseUrl}/MyApplications");
        
        // Should see draft
        await Expect(Page.Locator("text=Draft")).ToBeVisibleAsync();
        
        // Click to edit draft
        await Page.ClickAsync("a:has-text('Edit').Or(a:has-text('Continue'))");
        
        // Verify we're back in the form with saved data
        var phoneField = await Page.InputValueAsync("input[name='PhoneNumber']");
        Assert.Equal("262-555-3333", phoneField);
        
        await E2EAuthenticationHelper.LogoutAsync(Page, BaseUrl);
    }
}
