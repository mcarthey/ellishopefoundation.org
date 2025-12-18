using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EllisHope.Controllers;

[Authorize]
public class MyApplicationsController : Controller
{
    private readonly IClientApplicationService _applicationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<MyApplicationsController> _logger;

    public MyApplicationsController(
        IClientApplicationService applicationService,
        UserManager<ApplicationUser> userManager,
        ILogger<MyApplicationsController> logger)
    {
        _applicationService = applicationService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: MyApplications
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var applications = await _applicationService.GetApplicationsByApplicantAsync(currentUser.Id);

        var viewModel = new MyApplicationsViewModel
        {
            Applications = applications.Select(a => new MyApplicationSummary
            {
                Id = a.Id,
                Status = a.Status,
                SubmittedDate = a.SubmittedDate,
                CreatedDate = a.CreatedDate,
                FundingTypes = string.Join(", ", a.FundingTypesList),
                EstimatedMonthlyCost = a.EstimatedMonthlyCost,
                FinalDecision = a.FinalDecision,
                DecisionMessage = a.DecisionMessage,
                CanEdit = a.Status == ApplicationStatus.Draft,
                CanWithdraw = a.Status == ApplicationStatus.Submitted || 
                             a.Status == ApplicationStatus.UnderReview ||
                             a.Status == ApplicationStatus.InDiscussion
            }),
            CanCreateNew = true
        };

        return View(viewModel);
    }

    // GET: MyApplications/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id, includeVotes: false);
        if (application == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || application.ApplicantId != currentUser.Id)
        {
            return Forbid();
        }

        // Get non-private comments only
        var comments = await _applicationService.GetApplicationCommentsAsync(id, includePrivate: false);

        var viewModel = new ApplicationDetailsViewModel
        {
            Application = application,
            Comments = comments,
            CanEdit = application.Status == ApplicationStatus.Draft,
            CanUserVote = false, // Applicants can't vote
            HasUserVoted = false,
            CanApproveReject = false
        };

        return View(viewModel);
    }

    // GET: MyApplications/Create
    public IActionResult Create()
    {
        var viewModel = new ApplicationCreateViewModel
        {
            CurrentStep = 1
        };

        return View(viewModel);
    }

    // POST: MyApplications/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationCreateViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Check which button was clicked
        bool isNextButton = Request.Form.ContainsKey("NextStep");
        bool isPreviousButton = Request.Form.ContainsKey("PreviousStep");
        bool isSaveDraft = Request.Form.ContainsKey("SaveAsDraft");

        // Handle save as draft
        if (isSaveDraft)
        {
            var draftApplication = MapToApplication(model, currentUser);
            draftApplication.Status = ApplicationStatus.Draft;

            var (succeeded, errors, application) = await _applicationService.CreateApplicationAsync(draftApplication);

            if (succeeded)
            {
                TempData["SuccessMessage"] = "Application saved as draft.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        // Handle previous button (go back without validation)
        if (isPreviousButton && model.CurrentStep > 1)
        {
            model.CurrentStep--;
            return View(model);
        }

        // Handle next button (validate current step before moving forward)
        if (isNextButton && model.CurrentStep < 6)
        {
            if (!ValidateStep(model, model.CurrentStep))
            {
                return View(model);
            }
            
            model.CurrentStep++;
            return View(model);
        }

        // Final step - validate all and submit
        if (!ValidateStep(model, model.CurrentStep))
        {
            return View(model);
        }

        // Create and submit application
        var newApplication = MapToApplication(model, currentUser);
        newApplication.Status = ApplicationStatus.Draft; // Will be changed to Submitted

        var (createSucceeded, createErrors, createdApp) = await _applicationService.CreateApplicationAsync(newApplication);

        if (!createSucceeded || createdApp == null)
        {
            foreach (var error in createErrors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        // Submit the application
        var (submitSucceeded, submitErrors) = await _applicationService.SubmitApplicationAsync(createdApp.Id, currentUser.Id);

        if (submitSucceeded)
        {
            TempData["SuccessMessage"] = "Application submitted successfully! You will be notified when it is reviewed.";
            return RedirectToAction(nameof(Details), new { id = createdApp.Id });
        }

        foreach (var error in submitErrors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    // GET: MyApplications/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id, includeVotes: false, includeComments: false);
        if (application == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || application.ApplicantId != currentUser.Id)
        {
            return Forbid();
        }

        if (application.Status != ApplicationStatus.Draft)
        {
            TempData["ErrorMessage"] = "Only draft applications can be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var viewModel = MapFromApplication(application);
        return View(viewModel);
    }

    // POST: MyApplications/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ApplicationEditViewModel model, string? action)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var application = await _applicationService.GetApplicationByIdAsync(id, includeVotes: false, includeComments: false);
        if (application == null || application.ApplicantId != currentUser.Id)
        {
            return Forbid();
        }

        if (application.Status != ApplicationStatus.Draft)
        {
            TempData["ErrorMessage"] = "Only draft applications can be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Check which button was clicked
        bool isNextButton = Request.Form.ContainsKey("NextStep");
        bool isPreviousButton = Request.Form.ContainsKey("PreviousStep");
        bool isSaveAndExit = Request.Form.ContainsKey("SaveAndExit");

        // Handle Save & Exit - save all data
        if (isSaveAndExit)
        {
            // Update application with ALL data from the model
            // (Hidden fields ensure we have data from all steps)
            UpdateApplicationFromModel(application, model);

            var (succeeded, errors) = await _applicationService.UpdateApplicationAsync(application);

            if (succeeded)
            {
                TempData["SuccessMessage"] = "Draft saved successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        // Navigate between steps without validation (except on final submit)
        if (isPreviousButton && model.CurrentStep > 1)
        {
            model.CurrentStep--;
            return View(model);
        }

        if (isNextButton && model.CurrentStep < 6)
        {
            // Validate current step before moving forward
            if (!ValidateStep(model, model.CurrentStep))
            {
                return View(model);
            }
            
            model.CurrentStep++;
            return View(model);
        }

        // Final step - validate all and update
        if (!ValidateStep(model, model.CurrentStep))
        {
            return View(model);
        }

        // Update application
        UpdateApplicationFromModel(application, model);

        var (updateSucceeded, updateErrors) = await _applicationService.UpdateApplicationAsync(application);

        if (updateSucceeded)
        {
            TempData["SuccessMessage"] = "Application updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        foreach (var error in updateErrors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    // POST: MyApplications/Withdraw/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id, string reason)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var application = await _applicationService.GetApplicationByIdAsync(id, includeVotes: false, includeComments: false);
        if (application == null || application.ApplicantId != currentUser.Id)
        {
            return Forbid();
        }

        var (succeeded, errors) = await _applicationService.WithdrawApplicationAsync(id, currentUser.Id, reason ?? "Withdrawn by applicant");

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Application withdrawn.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", errors);
        }

        return RedirectToAction(nameof(Index));
    }

    #region Helper Methods

    private bool ValidateStep(ApplicationCreateViewModel model, int step)
    {
        // Clear all errors first
        ModelState.Clear();

        switch (step)
        {
            case 1: // Personal Information
                if (string.IsNullOrWhiteSpace(model.FirstName))
                    ModelState.AddModelError(nameof(model.FirstName), "First name is required");
                if (string.IsNullOrWhiteSpace(model.LastName))
                    ModelState.AddModelError(nameof(model.LastName), "Last name is required");
                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                    ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is required");
                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Email is required");
                break;

            case 2: // Program Interest
                if (!model.FundingTypesRequested.Any())
                    ModelState.AddModelError(nameof(model.FundingTypesRequested), "Please select at least one funding type");
                break;

            case 3: // Motivation
                if (string.IsNullOrWhiteSpace(model.PersonalStatement) || model.PersonalStatement.Length < 50)
                    ModelState.AddModelError(nameof(model.PersonalStatement), "Please provide a detailed personal statement (minimum 50 characters)");
                if (string.IsNullOrWhiteSpace(model.ExpectedBenefits) || model.ExpectedBenefits.Length < 50)
                    ModelState.AddModelError(nameof(model.ExpectedBenefits), "Please explain how you will benefit (minimum 50 characters)");
                if (string.IsNullOrWhiteSpace(model.CommitmentStatement) || model.CommitmentStatement.Length < 50)
                    ModelState.AddModelError(nameof(model.CommitmentStatement), "Please explain your commitment (minimum 50 characters)");
                break;

            case 4: // Health & Fitness
                // Optional fields, no validation
                break;

            case 5: // Program Requirements
                if (!model.UnderstandsCommitment)
                    ModelState.AddModelError(nameof(model.UnderstandsCommitment), "You must acknowledge the 12-month commitment");
                break;

            case 6: // Signature
                if (string.IsNullOrWhiteSpace(model.Signature))
                    ModelState.AddModelError(nameof(model.Signature), "Signature is required");
                break;
        }

        return ModelState.IsValid;
    }

    private ClientApplication MapToApplication(ApplicationCreateViewModel model, ApplicationUser user)
    {
        return new ClientApplication
        {
            ApplicantId = user.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Address = model.Address,
            City = model.City,
            State = model.State,
            ZipCode = model.ZipCode,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            Occupation = model.Occupation,
            DateOfBirth = model.DateOfBirth,
            EmergencyContactName = model.EmergencyContactName,
            EmergencyContactPhone = model.EmergencyContactPhone,
            FundingTypesRequested = string.Join(",", model.FundingTypesRequested),
            EstimatedMonthlyCost = model.EstimatedMonthlyCost,
            ProgramDurationMonths = model.ProgramDurationMonths,
            FundingDetails = model.FundingDetails,
            PersonalStatement = model.PersonalStatement,
            ExpectedBenefits = model.ExpectedBenefits,
            CommitmentStatement = model.CommitmentStatement,
            ConcernsObstacles = model.ConcernsObstacles,
            MedicalConditions = model.MedicalConditions,
            CurrentMedications = model.CurrentMedications,
            LastPhysicalExamDate = model.LastPhysicalExamDate,
            FitnessGoals = model.FitnessGoals,
            CurrentFitnessLevel = model.CurrentFitnessLevel,
            AgreesToNutritionist = model.AgreesToNutritionist,
            AgreesToPersonalTrainer = model.AgreesToPersonalTrainer,
            AgreesToWeeklyCheckIns = model.AgreesToWeeklyCheckIns,
            AgreesToProgressReports = model.AgreesToProgressReports,
            UnderstandsCommitment = model.UnderstandsCommitment,
            Signature = model.Signature,
            SubmissionIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };
    }

    private ApplicationEditViewModel MapFromApplication(ClientApplication application)
    {
        return new ApplicationEditViewModel
        {
            Id = application.Id,
            Status = application.Status,
            FirstName = application.FirstName,
            LastName = application.LastName,
            Address = application.Address,
            City = application.City,
            State = application.State,
            ZipCode = application.ZipCode,
            PhoneNumber = application.PhoneNumber,
            Email = application.Email,
            Occupation = application.Occupation,
            DateOfBirth = application.DateOfBirth,
            EmergencyContactName = application.EmergencyContactName,
            EmergencyContactPhone = application.EmergencyContactPhone,
            FundingTypesRequested = application.FundingTypesList.ToList(),
            EstimatedMonthlyCost = application.EstimatedMonthlyCost,
            ProgramDurationMonths = application.ProgramDurationMonths,
            FundingDetails = application.FundingDetails,
            PersonalStatement = application.PersonalStatement,
            ExpectedBenefits = application.ExpectedBenefits,
            CommitmentStatement = application.CommitmentStatement,
            ConcernsObstacles = application.ConcernsObstacles,
            MedicalConditions = application.MedicalConditions,
            CurrentMedications = application.CurrentMedications,
            LastPhysicalExamDate = application.LastPhysicalExamDate,
            FitnessGoals = application.FitnessGoals,
            CurrentFitnessLevel = application.CurrentFitnessLevel,
            AgreesToNutritionist = application.AgreesToNutritionist,
            AgreesToPersonalTrainer = application.AgreesToPersonalTrainer,
            AgreesToWeeklyCheckIns = application.AgreesToWeeklyCheckIns,
            AgreesToProgressReports = application.AgreesToProgressReports,
            UnderstandsCommitment = application.UnderstandsCommitment,
            Signature = application.Signature,
            CurrentStep = 1
        };
    }

    private void UpdateApplicationFromModel(ClientApplication application, ApplicationEditViewModel model)
    {
        application.FirstName = model.FirstName;
        application.LastName = model.LastName;
        application.Address = model.Address;
        application.City = model.City;
        application.State = model.State;
        application.ZipCode = model.ZipCode;
        application.PhoneNumber = model.PhoneNumber;
        application.Email = model.Email;
        application.Occupation = model.Occupation;
        application.DateOfBirth = model.DateOfBirth;
        application.EmergencyContactName = model.EmergencyContactName;
        application.EmergencyContactPhone = model.EmergencyContactPhone;
        application.FundingTypesRequested = string.Join(",", model.FundingTypesRequested);
        application.EstimatedMonthlyCost = model.EstimatedMonthlyCost;
        application.ProgramDurationMonths = model.ProgramDurationMonths;
        application.FundingDetails = model.FundingDetails;
        application.PersonalStatement = model.PersonalStatement;
        application.ExpectedBenefits = model.ExpectedBenefits;
        application.CommitmentStatement = model.CommitmentStatement;
        application.ConcernsObstacles = model.ConcernsObstacles;
        application.MedicalConditions = model.MedicalConditions;
        application.CurrentMedications = model.CurrentMedications;
        application.LastPhysicalExamDate = model.LastPhysicalExamDate;
        application.FitnessGoals = model.FitnessGoals;
        application.CurrentFitnessLevel = model.CurrentFitnessLevel;
        application.AgreesToNutritionist = model.AgreesToNutritionist;
        application.AgreesToPersonalTrainer = model.AgreesToPersonalTrainer;
        application.AgreesToWeeklyCheckIns = model.AgreesToWeeklyCheckIns;
        application.AgreesToProgressReports = model.AgreesToProgressReports;
        application.UnderstandsCommitment = model.UnderstandsCommitment;
        application.Signature = model.Signature;
    }

    private void UpdateApplicationFromModelPartial(ClientApplication application, ApplicationEditViewModel model)
    {
        // Only update fields from the current step to avoid overwriting data
        switch (model.CurrentStep)
        {
            case 1: // Personal Information
                if (!string.IsNullOrEmpty(model.FirstName)) application.FirstName = model.FirstName;
                if (!string.IsNullOrEmpty(model.LastName)) application.LastName = model.LastName;
                if (!string.IsNullOrEmpty(model.PhoneNumber)) application.PhoneNumber = model.PhoneNumber;
                if (!string.IsNullOrEmpty(model.Email)) application.Email = model.Email;
                application.Address = model.Address; // Optional fields - always update
                application.City = model.City;
                application.State = model.State;
                application.ZipCode = model.ZipCode;
                application.Occupation = model.Occupation;
                application.DateOfBirth = model.DateOfBirth;
                application.EmergencyContactName = model.EmergencyContactName;
                application.EmergencyContactPhone = model.EmergencyContactPhone;
                break;

            case 2: // Program Interest & Funding
                if (model.FundingTypesRequested.Any())
                    application.FundingTypesRequested = string.Join(",", model.FundingTypesRequested);
                application.EstimatedMonthlyCost = model.EstimatedMonthlyCost;
                application.ProgramDurationMonths = model.ProgramDurationMonths;
                application.FundingDetails = model.FundingDetails;
                break;

            case 3: // Motivation & Commitment
                application.PersonalStatement = model.PersonalStatement;
                application.ExpectedBenefits = model.ExpectedBenefits;
                application.CommitmentStatement = model.CommitmentStatement;
                application.ConcernsObstacles = model.ConcernsObstacles;
                break;

            case 4: // Health & Fitness
                application.MedicalConditions = model.MedicalConditions;
                application.CurrentMedications = model.CurrentMedications;
                application.LastPhysicalExamDate = model.LastPhysicalExamDate;
                application.FitnessGoals = model.FitnessGoals;
                application.CurrentFitnessLevel = model.CurrentFitnessLevel;
                break;

            case 5: // Program Requirements
                application.AgreesToNutritionist = model.AgreesToNutritionist;
                application.AgreesToPersonalTrainer = model.AgreesToPersonalTrainer;
                application.AgreesToWeeklyCheckIns = model.AgreesToWeeklyCheckIns;
                application.AgreesToProgressReports = model.AgreesToProgressReports;
                application.UnderstandsCommitment = model.UnderstandsCommitment;
                break;

            case 6: // Review & Sign
                application.Signature = model.Signature;
                break;
        }
    }

    #endregion
}
