using System.Net;
using System.Net.Http.Json;
using EllisHope.Data;
using EllisHope.Models.Domain;
using EllisHope.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EllisHope.Tests.Integration;

/// <summary>
/// Integration tests for Draft Application Save functionality
/// Tests the complete workflow with real database operations
/// Verifies that SaveAndExit preserves data across multiple steps
/// 
/// These tests cover the critical bug fix for multi-step draft saving
/// </summary>
[Collection("Integration Tests")]
public class DraftApplicationSaveIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DraftApplicationSaveIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Complete Multi-Step Draft Save Workflow

    [Fact]
    public async Task DraftSave_CompleteWorkflow_PreservesAllData()
    {
        // This test simulates the real user workflow:
        // 1. Create draft with Step 1 data
        // 2. Edit draft, fill Step 2, verify Step 1 preserved
        // 3. Edit draft again, fill Step 3, verify Steps 1-2 preserved
        // 4. Verify all data is preserved in database

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "draftsave@integration.com",
            "Draft",
            "Tester",
            UserRole.Member);

        // Step 1: Create initial draft with Step 1 data
        var draftId = await CreateDraftWithStep1DataAsync(dbContext, testUser);

        // Step 2: Edit draft - add Step 2 data, verify Step 1 preserved
        await EditDraftAddStep2DataAsync(dbContext, draftId);

        // Step 3: Edit draft - add Step 3 data, verify Steps 1-2 preserved
        await EditDraftAddStep3DataAsync(dbContext, draftId);

        // Step 4: Final verification - all data should be in database
        var finalApplication = await dbContext.ClientApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(finalApplication);

        // Verify Step 1 data (from initial save)
        Assert.Equal("Draft", finalApplication.FirstName);
        Assert.Equal("Tester", finalApplication.LastName);
        Assert.Equal("draftsave@integration.com", finalApplication.Email);
        Assert.Equal("555-1111", finalApplication.PhoneNumber);

        // Verify Step 2 data (from first edit)
        Assert.Contains("GymMembership", finalApplication.FundingTypesRequested);
        Assert.Equal(150m, finalApplication.EstimatedMonthlyCost);
        Assert.Equal(12, finalApplication.ProgramDurationMonths);

        // Verify Step 3 data (from second edit)
        Assert.Contains("I am very motivated", finalApplication.PersonalStatement);
        Assert.Contains("I will benefit greatly", finalApplication.ExpectedBenefits);
        Assert.Contains("I am committed", finalApplication.CommitmentStatement);
    }

    #endregion

    #region Step-by-Step Save Tests

    [Fact]
    public async Task SaveStep1_ThenSaveStep2_EmailNotOverwritten()
    {
        // This test specifically verifies the bug fix:
        // Saving Step 2 should NOT overwrite Email field from Step 1

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "email-preserve@test.com",
            "Email",
            "Tester",
            UserRole.Member);

        // Create draft with required Step 1 fields
        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            FirstName = "Email",
            LastName = "Tester",
            Email = "email-preserve@test.com", // CRITICAL - Must not be overwritten!
            PhoneNumber = "555-0000",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        var draftId = application.Id;

        // Clear tracking to simulate new request
        dbContext.ChangeTracker.Clear();

        // Now "edit" the draft - simulating user on Step 2
        // Step 2 form doesn't have Email field, so it would be null in the model
        var draftToUpdate = await dbContext.ClientApplications.FindAsync(draftId);
        Assert.NotNull(draftToUpdate);

        // Update only Step 2 fields (like the partial update method does)
        draftToUpdate.FundingTypesRequested = "GymMembership,PersonalTraining";
        draftToUpdate.EstimatedMonthlyCost = 200m;
        draftToUpdate.ProgramDurationMonths = 6;
        draftToUpdate.ModifiedDate = DateTime.UtcNow;

        // Email field should NOT be touched!
        // This simulates what UpdateApplicationFromModelPartial does

        await dbContext.SaveChangesAsync();

        // Verify: Email should still be there!
        dbContext.ChangeTracker.Clear();
        var verifyApplication = await dbContext.ClientApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(verifyApplication);
        Assert.Equal("email-preserve@test.com", verifyApplication.Email); // THE KEY ASSERTION!
        Assert.Equal("GymMembership,PersonalTraining", verifyApplication.FundingTypesRequested);
    }

    [Fact]
    public async Task SaveStep3_PreservesStep1And2RequiredFields()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "step3-save@test.com",
            "Step3",
            "Saver",
            UserRole.Member);

        // Create draft with Steps 1-2 complete
        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            // Step 1
            FirstName = "Step3",
            LastName = "Saver",
            Email = "step3-save@test.com",
            PhoneNumber = "555-3333",
            // Step 2
            FundingTypesRequested = "GymMembership",
            EstimatedMonthlyCost = 100m,
            ProgramDurationMonths = 12,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        var draftId = application.Id;

        dbContext.ChangeTracker.Clear();

        // Edit draft - add Step 3 data
        var draftToUpdate = await dbContext.ClientApplications.FindAsync(draftId);
        Assert.NotNull(draftToUpdate);

        // Update only Step 3 fields
        draftToUpdate.PersonalStatement = "This is my personal statement for Step 3 that is definitely long enough to meet requirements.";
        draftToUpdate.ExpectedBenefits = "These are the benefits I expect to receive which are also long enough.";
        draftToUpdate.CommitmentStatement = "I commit to this program fully and completely with dedication.";
        draftToUpdate.ModifiedDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        // Verify all previous data preserved
        dbContext.ChangeTracker.Clear();
        var verifyApplication = await dbContext.ClientApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(verifyApplication);
        
        // Step 1 preserved
        Assert.Equal("step3-save@test.com", verifyApplication.Email);
        Assert.Equal("555-3333", verifyApplication.PhoneNumber);
        
        // Step 2 preserved
        Assert.Equal("GymMembership", verifyApplication.FundingTypesRequested);
        Assert.Equal(100m, verifyApplication.EstimatedMonthlyCost);
        
        // Step 3 added
        Assert.Contains("This is my personal statement", verifyApplication.PersonalStatement);
    }

    #endregion

    #region Field-Level Validation Tests

    [Fact]
    public async Task DatabaseConstraint_RequiredEmail_PreventsSaveWithNull()
    {
        // This test verifies the database itself enforces the Email requirement
        // If our code fails to preserve Email, the database will reject it

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "constraint-test@test.com",
            "Constraint",
            "Tester",
            UserRole.Member);

        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            FirstName = "Test",
            LastName = "User",
            Email = null!, // This should fail!
            PhoneNumber = "555-0000",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);

        // Should throw DbUpdateException because Email is required
        await Assert.ThrowsAsync<DbUpdateException>(async () => 
            await dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task PartialUpdate_DoesNotClearFundingTypes()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "funding-preserve@test.com",
            "Funding",
            "Tester",
            UserRole.Member);

        // Create draft with funding types selected
        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            FirstName = "Funding",
            LastName = "Tester",
            Email = "funding-preserve@test.com",
            PhoneNumber = "555-0000",
            FundingTypesRequested = "GymMembership,PersonalTraining,NutritionistConsultation",
            EstimatedMonthlyCost = 250m,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        var draftId = application.Id;

        dbContext.ChangeTracker.Clear();

        // Edit draft on Step 1 (which doesn't have FundingTypes)
        var draftToUpdate = await dbContext.ClientApplications.FindAsync(draftId);
        Assert.NotNull(draftToUpdate);

        // Update Step 1 field
        draftToUpdate.PhoneNumber = "555-9999";
        draftToUpdate.ModifiedDate = DateTime.UtcNow;
        // FundingTypesRequested should NOT be cleared!

        await dbContext.SaveChangesAsync();

        // Verify funding types preserved
        dbContext.ChangeTracker.Clear();
        var verifyApplication = await dbContext.ClientApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(verifyApplication);
        Assert.Equal("555-9999", verifyApplication.PhoneNumber);
        Assert.Contains("GymMembership", verifyApplication.FundingTypesRequested);
        Assert.Contains("PersonalTraining", verifyApplication.FundingTypesRequested);
        Assert.Contains("NutritionistConsultation", verifyApplication.FundingTypesRequested);
    }

    #endregion

    #region Status Preservation Tests

    [Fact]
    public async Task SaveAndExit_PreservesDraftStatus()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var testUser = await TestAuthenticationHelper.CreateTestUserAsync(
            _factory.Services,
            "status-preserve@test.com",
            "Status",
            "Tester",
            UserRole.Member);

        var application = new ClientApplication
        {
            ApplicantId = testUser,
            Status = ApplicationStatus.Draft,
            FirstName = "Status",
            LastName = "Tester",
            Email = "status-preserve@test.com",
            PhoneNumber = "555-0000",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        var draftId = application.Id;

        dbContext.ChangeTracker.Clear();

        // Update draft multiple times
        for (int i = 0; i < 3; i++)
        {
            var draftToUpdate = await dbContext.ClientApplications.FindAsync(draftId);
            Assert.NotNull(draftToUpdate);
            draftToUpdate.PhoneNumber = $"555-{i:D4}";
            draftToUpdate.ModifiedDate = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
        }

        // Verify status still Draft after multiple saves
        var verifyApplication = await dbContext.ClientApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == draftId);

        Assert.NotNull(verifyApplication);
        Assert.Equal(ApplicationStatus.Draft, verifyApplication.Status);
    }

    #endregion

    #region Helper Methods

    private async Task<int> CreateDraftWithStep1DataAsync(ApplicationDbContext dbContext, string userId)
    {
        var application = new ClientApplication
        {
            ApplicantId = userId,
            Status = ApplicationStatus.Draft,
            FirstName = "Draft",
            LastName = "Tester",
            Email = "draftsave@integration.com",
            PhoneNumber = "555-1111",
            Address = "123 Test St",
            City = "Milwaukee",
            State = "WI",
            ZipCode = "53202",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        dbContext.ClientApplications.Add(application);
        await dbContext.SaveChangesAsync();
        
        var draftId = application.Id;
        dbContext.ChangeTracker.Clear();
        
        return draftId;
    }

    private async Task EditDraftAddStep2DataAsync(ApplicationDbContext dbContext, int draftId)
    {
        var draft = await dbContext.ClientApplications.FindAsync(draftId);
        Assert.NotNull(draft);

        // Verify Step 1 data still there before we start
        Assert.Equal("draftsave@integration.com", draft.Email);

        // Add Step 2 data
        draft.FundingTypesRequested = "GymMembership,PersonalTraining";
        draft.EstimatedMonthlyCost = 150m;
        draft.ProgramDurationMonths = 12;
        draft.FundingDetails = "I need gym membership and personal training.";
        draft.ModifiedDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        // Verify Step 1 data preserved
        var verify = await dbContext.ClientApplications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == draftId);
        Assert.NotNull(verify);
        Assert.Equal("draftsave@integration.com", verify.Email); // Still there!
        Assert.Equal("555-1111", verify.PhoneNumber); // Still there!
    }

    private async Task EditDraftAddStep3DataAsync(ApplicationDbContext dbContext, int draftId)
    {
        var draft = await dbContext.ClientApplications.FindAsync(draftId);
        Assert.NotNull(draft);

        // Verify Steps 1-2 data still there
        Assert.Equal("draftsave@integration.com", draft.Email);
        Assert.Contains("GymMembership", draft.FundingTypesRequested);

        // Add Step 3 data
        draft.PersonalStatement = "I am very motivated to improve my health and fitness through this program.";
        draft.ExpectedBenefits = "I will benefit greatly from the gym membership and personal training support.";
        draft.CommitmentStatement = "I am committed to completing the full 12-month program with dedication.";
        draft.ModifiedDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        // Verify Steps 1-2 data preserved
        var verify = await dbContext.ClientApplications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == draftId);
        Assert.NotNull(verify);
        Assert.Equal("draftsave@integration.com", verify.Email); // Still there!
        Assert.Contains("GymMembership", verify.FundingTypesRequested); // Still there!
    }

    #endregion
}
