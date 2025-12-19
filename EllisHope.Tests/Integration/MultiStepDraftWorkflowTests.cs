using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Critical missing tests for multi-step draft save workflow
/// These tests verify the ACTUAL behavior with a real database
/// </summary>
[Collection("Integration Tests")]
public class MultiStepDraftWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MultiStepDraftWorkflowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SaveStep2_DoesNotOverwriteStep1Data()
    {
        // This is the CRITICAL test we were missing!
        // Verifies that saving Step 2 doesn't accidentally clear Step 1

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "step2save@test.com",
            "Step2",
            "Tester",
            UserRole.Member);

        // Create draft with Step 1 data
        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            FirstName = "Step2",
            LastName = "Tester",
            Email = "step2save@test.com",
            PhoneNumber = "555-1111",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        var draftId = application.Id;

        dbContext.ChangeTracker.Clear();

        // Now user edits and saves Step 2 only
        var draft = await dbContext.ClientApplications.FindAsync(draftId);
        Assert.NotNull(draft);

        // Add Step 2 data
        draft.FundingTypesRequested = "GymMembership";
        draft.EstimatedMonthlyCost = 150m;
        draft.ProgramDurationMonths = 12;
        draft.ModifiedDate = DateTime.UtcNow;

        // DO NOT touch Step 1 fields!
        // This simulates what conditional hidden fields do

        await dbContext.SaveChangesAsync();

        // Verify Step 1 data still exists
        dbContext.ChangeTracker.Clear();
        var verify = await dbContext.ClientApplications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(verify);
        Assert.Equal("Step2", verify.FirstName);
        Assert.Equal("Tester", verify.LastName);
        Assert.Equal("step2save@test.com", verify.Email);
        Assert.Equal("555-1111", verify.PhoneNumber);
        
        // And Step 2 data was added
        Assert.Equal("GymMembership", verify.FundingTypesRequested);
        Assert.Equal(150m, verify.EstimatedMonthlyCost);
    }

    [Fact]
    public async Task SaveStep3_WhenStep2Empty_DoesNotClearStep1()
    {
        // Edge case: What if user skips Step 2?
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "skipstep@test.com",
            "Skip",
            "Tester",
            UserRole.Member);

        // Create draft with Step 1 only
        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            FirstName = "Skip",
            LastName = "Tester",
            Email = "skipstep@test.com",
            PhoneNumber = "555-2222",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        var draftId = application.Id;

        dbContext.ChangeTracker.Clear();

        // User jumps to Step 3 without filling Step 2
        var draft = await dbContext.ClientApplications.FindAsync(draftId);
        Assert.NotNull(draft);

        draft.PersonalStatement = "I really want to improve my health through this program.";
        draft.ExpectedBenefits = "I expect to gain better fitness and overall wellness.";
        draft.CommitmentStatement = "I commit to completing the full 12-month program.";
        draft.ModifiedDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        // Verify Step 1 STILL intact
        dbContext.ChangeTracker.Clear();
        var verify = await dbContext.ClientApplications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(verify);
        Assert.Equal("skipstep@test.com", verify.Email);
        Assert.Equal("Skip", verify.FirstName);
    }

    [Fact]
    public async Task MultipleSaves_PreserveAllData()
    {
        // Test saving multiple times doesn't lose data
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "multiplesave@test.com",
            "Multi",
            "Save",
            UserRole.Member);

        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            FirstName = "Multi",
            LastName = "Save",
            Email = "multiplesave@test.com",
            PhoneNumber = "555-3333",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        var draftId = application.Id;

        // Save 1: Add Step 2 data
        dbContext.ChangeTracker.Clear();
        var draft1 = await dbContext.ClientApplications.FindAsync(draftId);
        draft1!.FundingTypesRequested = "GymMembership";
        draft1.EstimatedMonthlyCost = 100m;
        await dbContext.SaveChangesAsync();

        // Save 2: Add Step 3 data
        dbContext.ChangeTracker.Clear();
        var draft2 = await dbContext.ClientApplications.FindAsync(draftId);
        draft2!.PersonalStatement = "Personal statement that is long enough to meet requirements.";
        await dbContext.SaveChangesAsync();

        // Save 3: Update Step 2 data
        dbContext.ChangeTracker.Clear();
        var draft3 = await dbContext.ClientApplications.FindAsync(draftId);
        draft3!.EstimatedMonthlyCost = 150m; // Changed!
        await dbContext.SaveChangesAsync();

        // Verify ALL data is still there
        dbContext.ChangeTracker.Clear();
        var final = await dbContext.ClientApplications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(final);
        Assert.Equal("multiplesave@test.com", final.Email); // Step 1
        Assert.Equal("GymMembership", final.FundingTypesRequested); // Step 2
        Assert.Equal(150m, final.EstimatedMonthlyCost); // Step 2 (updated!)
        Assert.Contains("Personal statement", final.PersonalStatement); // Step 3
    }
}
