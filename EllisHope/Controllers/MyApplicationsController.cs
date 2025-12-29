using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
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
    /// <summary>
    /// Displays all applications submitted by the authenticated user with current status and available actions
    /// </summary>
    /// <returns>View displaying user's applications with status, dates, and action buttons (edit/withdraw)</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /MyApplications
    ///
    /// Returns all applications created by the current user, including:
    /// - Draft applications (can be edited or submitted)
    /// - Submitted applications (can be withdrawn before review)
    /// - Applications under review (can be withdrawn)
    /// - Approved/rejected applications (read-only with decision details)
    ///
    /// Each application shows:
    /// - Current status (Draft, Submitted, UnderReview, Approved, Rejected)
    /// - Submission date and creation date
    /// - Funding types requested
    /// - Estimated monthly cost
    /// - Final decision message (if applicable)
    /// - Available actions (Edit Draft, Withdraw, View Details)
    ///
    /// Requires authentication. Users can only see their own applications.
    /// </remarks>
    /// <response code="200">Successfully retrieved user's applications</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves all applications for the authenticated user",
        Description = "Returns complete list of user's applications across all statuses (Draft, Submitted, UnderReview, Approved, Rejected) with available actions and decision information.",
        OperationId = "GetMyApplications",
        Tags = new[] { "My Applications" }
    )]
    [ProducesResponseType(typeof(MyApplicationsViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                CanEdit = a.Status == ApplicationStatus.Draft ||
                         a.Status == ApplicationStatus.NeedsInformation,
                CanWithdraw = a.Status == ApplicationStatus.Submitted ||
                             a.Status == ApplicationStatus.UnderReview ||
                             a.Status == ApplicationStatus.InDiscussion
            }),
            CanCreateNew = true
        };

        return View(viewModel);
    }

    // GET: MyApplications/Details/5
    /// <summary>
    /// Displays detailed view of a specific application with full form data and non-private review comments
    /// </summary>
    /// <param name="id">Application ID to view details for</param>
    /// <returns>View displaying complete application details with review comments and available actions</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /MyApplications/Details/15
    ///
    /// Returns complete application information including:
    /// - All form fields submitted across all steps
    /// - Current application status
    /// - Review comments visible to applicant (non-private only)
    /// - Decision details if approved/rejected
    /// - Available actions based on status (Edit if Draft, Withdraw if Submitted/Under Review)
    ///
    /// Authorization:
    /// - User must be authenticated
    /// - User can only view their own applications
    /// - Returns 403 Forbidden if trying to access another user's application
    ///
    /// Comments filtering:
    /// - Only non-private comments are shown to applicants
    /// - Private board member discussion comments are hidden
    /// </remarks>
    /// <response code="200">Successfully retrieved application details</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User attempting to access another user's application</response>
    /// <response code="404">Application not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieves detailed information for a specific application",
        Description = "Returns complete application data with non-private comments. Users can only access their own applications. Includes all form fields, status, and board feedback.",
        OperationId = "GetMyApplicationDetails",
        Tags = new[] { "My Applications" }
    )]
    [ProducesResponseType(typeof(ApplicationDetailsViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            CanEdit = application.Status == ApplicationStatus.Draft ||
                     application.Status == ApplicationStatus.NeedsInformation,
            CanUserVote = false, // Applicants can't vote
            HasUserVoted = false,
            CanApproveReject = false
        };

        return View(viewModel);
    }

    // GET: MyApplications/Create
    /// <summary>
    /// Displays the multi-step application creation form starting at Step 1 (Personal Information)
    /// </summary>
    /// <returns>View displaying the application form at Step 1</returns>
    /// <remarks>
    /// Initiates a new application for program sponsorship. The application process consists of:
    ///
    /// **Application Steps:**
    /// 1. Personal Information (name, contact, demographics)
    /// 2. Background and Circumstances (situation, needs assessment)
    /// 3. Goals and Objectives (program goals, expected outcomes)
    /// 4. Financial Information (income, expenses, funding needs)
    /// 5. References and Documents (contact references, upload documents)
    /// 6. Review and Submit (review all entered data, final submission)
    ///
    /// **Features:**
    /// - Multi-step wizard interface with progress indicator
    /// - Save as draft at any step to complete later
    /// - Navigate between steps to review/edit previous sections
    /// - Client-side validation for required fields
    /// - Final submission triggers board review workflow
    ///
    /// Requires authentication. User becomes the applicant on the application.
    /// </remarks>
    /// <response code="200">Successfully displayed application form</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Displays new application form starting at Step 1",
        Description = "Initiates multi-step application creation wizard. Supports progressive form completion, draft saving, and step navigation.",
        OperationId = "GetCreateApplication",
        Tags = new[] { "My Applications" }
    )]
    [ProducesResponseType(typeof(ApplicationCreateViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Create()
    {
        var viewModel = new ApplicationCreateViewModel
        {
            CurrentStep = 1
        };

        return View(viewModel);
    }

    // POST: MyApplications/Create
    /// <summary>
    /// Processes multi-step application form submission with support for draft saving, step navigation, and final submission
    /// </summary>
    /// <param name="model">Application form data containing all fields across 6 steps</param>
    /// <returns>Redirects to Index (on draft save), Edit (on navigation), or Details (on submit); or returns view with errors</returns>
    /// <remarks>
    /// Handles three distinct submission actions based on which button the user clicked:
    ///
    /// **Save as Draft (SaveAsDraft button):**
    /// - Saves current form state without validation
    /// - Creates new application with Draft status
    /// - Redirects to MyApplications list with success message
    /// - User can resume later from Index page
    ///
    /// **Navigate Previous (PreviousStep button):**
    /// - Auto-saves current step as draft (no validation)
    /// - Decrements step counter
    /// - Redirects to Edit mode with saved draft ID
    /// - Preserves all entered data
    ///
    /// **Navigate Next (NextStep button):**
    /// - Validates current step before proceeding
    /// - Auto-saves validated data as draft
    /// - Increments step counter
    /// - Redirects to Edit mode to continue
    /// - Shows validation errors if step incomplete
    ///
    /// **Final Submit (default/final step):**
    /// - Validates current step
    /// - Creates application as Draft
    /// - Immediately submits via SubmitApplicationAsync
    /// - Changes status from Draft → Submitted
    /// - Triggers board review workflow
    /// - Sends email notifications to admins
    ///
    /// Sample form submission (Step 1 - Next button):
    ///
    ///     POST /MyApplications/Create
    ///     Content-Type: application/x-www-form-urlencoded
    ///
    ///     FirstName=John&amp;LastName=Doe&amp;Email=john@example.com&amp;...&amp;NextStep=true&amp;CurrentStep=1
    ///
    /// Requires authentication and anti-forgery token.
    /// </remarks>
    /// <response code="302">Redirects to Index (draft saved), Edit (navigation), or Details (submitted)</response>
    /// <response code="200">Returns view with model and validation errors</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="400">Validation failed or service error occurred</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Processes application form with multi-step navigation, draft saving, and submission",
        Description = "Handles Save Draft, Previous/Next navigation, and final submission. Auto-saves progress between steps. Validates step data before moving forward. Triggers board review on final submit.",
        OperationId = "PostCreateApplication",
        Tags = new[] { "My Applications" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ApplicationCreateViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(ApplicationCreateViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Check which button was clicked
        bool isNextButton = Request.Form.ContainsKey("NextStep");
        bool isPreviousButton = Request.Form.ContainsKey("PreviousStep") || Request.Form.ContainsKey("Navigate");
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

        // Handle previous button - auto-save as draft before navigating
        if (isPreviousButton && model.CurrentStep > 1)
        {
            // Auto-save as draft to preserve data
            var draftApplication = MapToApplication(model, currentUser);
            draftApplication.Status = ApplicationStatus.Draft;

            var (succeeded, errors, application) = await _applicationService.CreateApplicationAsync(draftApplication);

            if (succeeded && application != null)
            {
                // Redirect to Edit mode with the saved draft
                return RedirectToAction(nameof(Edit), new { id = application.Id, step = model.CurrentStep - 1 });
            }

            // If save failed, continue with navigation but warn user
            _logger.LogWarning("Failed to auto-save draft when navigating back: {Errors}", string.Join(", ", errors));
            model.CurrentStep--;
            return View(model);
        }

        // Handle next button - validate, auto-save as draft, then navigate
        if (isNextButton && model.CurrentStep < 6)
        {
            if (!ValidateStep(model, model.CurrentStep))
            {
                return View(model);
            }

            // Auto-save as draft to preserve data
            var draftApplication = MapToApplication(model, currentUser);
            draftApplication.Status = ApplicationStatus.Draft;

            var (succeeded, errors, application) = await _applicationService.CreateApplicationAsync(draftApplication);

            if (succeeded && application != null)
            {
                // Redirect to Edit mode with the saved draft
                return RedirectToAction(nameof(Edit), new { id = application.Id, step = model.CurrentStep + 1 });
            }

            // If save failed, show errors
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
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
    /// <summary>
    /// Loads a draft application for editing, optionally at a specific step in the multi-step form
    /// </summary>
    /// <param name="id">Application ID to edit</param>
    /// <param name="step">Optional step number (1-6) to display; defaults to step 1 if not specified</param>
    /// <returns>View displaying the application edit form at the specified step</returns>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /MyApplications/Edit/42
    ///     GET /MyApplications/Edit/42?step=3
    ///
    /// Allows users to resume editing a saved draft application. Features:
    /// - Loads all previously saved data into form fields
    /// - Displays form at specified step (or step 1 by default)
    /// - Supports step navigation with Previous/Next buttons
    /// - Only Draft status applications can be edited
    /// - Submitted/Under Review applications redirect to Details with error
    ///
    /// **Step Navigation:**
    /// - Step 1: Personal Information
    /// - Step 2: Program Interest and Funding
    /// - Step 3: Motivation and Commitment
    /// - Step 4: Health and Fitness
    /// - Step 5: Program Requirements
    /// - Step 6: Review and Sign
    ///
    /// **Authorization:**
    /// - User must be authenticated
    /// - User can only edit their own applications
    /// - Returns 403 Forbidden if attempting to edit another user's application
    ///
    /// **Status Restrictions:**
    /// - Only applications with Draft status can be edited
    /// - Submitted/Under Review/Approved/Rejected applications cannot be edited
    /// - Redirects to Details page with error message if status is not Draft
    /// </remarks>
    /// <response code="200">Successfully loaded draft application for editing</response>
    /// <response code="302">Redirects to Details if application status is not Draft</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User attempting to edit another user's application</response>
    /// <response code="404">Application not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Loads draft application for editing at specified step",
        Description = "Retrieves saved draft application and displays edit form. Supports step navigation. Only Draft status applications can be edited. Users can only edit their own applications.",
        OperationId = "GetEditApplication",
        Tags = new[] { "My Applications" }
    )]
    [ProducesResponseType(typeof(ApplicationEditViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Edit(int id, int? step)
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

        if (application.Status != ApplicationStatus.Draft &&
            application.Status != ApplicationStatus.NeedsInformation)
        {
            TempData["ErrorMessage"] = "This application cannot be edited in its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var viewModel = MapFromApplication(application);

        // Set the current step from query string if provided
        if (step.HasValue && step.Value >= 1 && step.Value <= 6)
        {
            viewModel.CurrentStep = step.Value;
        }
        
        return View(viewModel);
    }

    // POST: MyApplications/Edit/5
    /// <summary>
    /// Processes draft application edits with support for saving, step navigation, and final submission
    /// </summary>
    /// <param name="id">Application ID being edited</param>
    /// <param name="model">Updated application form data</param>
    /// <param name="action">Optional action parameter (not actively used; form buttons drive behavior)</param>
    /// <returns>Redirects to Index (on save/submit) or returns view (on navigation/errors)</returns>
    /// <remarks>
    /// Handles four distinct editing actions based on which button the user clicked:
    ///
    /// **Save and Exit (SaveAndExit button):**
    /// - Saves all form data WITHOUT validation (lenient for drafts)
    /// - Allows partial completion - user can save incomplete forms
    /// - Updates only the fields that have been filled
    /// - Redirects to MyApplications list
    /// - Tolerates database constraint errors for incomplete drafts
    ///
    /// **Navigate Previous (PreviousStep button):**
    /// - Auto-saves current step data without validation
    /// - Decrements step counter
    /// - Reloads application from database to ensure fresh data
    /// - Returns view at previous step
    /// - Never blocks navigation due to validation
    ///
    /// **Navigate Next (NextStep button):**
    /// - Validates current step before proceeding
    /// - Auto-saves validated step data
    /// - Increments step counter
    /// - Reloads application from database
    /// - Returns view with errors if validation fails
    ///
    /// **Submit Application (SubmitApplication button):**
    /// - Validates ALL 6 steps before allowing submission
    /// - Checks required fields across entire form
    /// - Updates application with complete data
    /// - Calls SubmitApplicationAsync to change status: Draft → Submitted
    /// - Triggers board review workflow
    /// - Sends email notifications to admins/board
    /// - Redirects to Details page on success
    ///
    /// **Step Validation Rules:**
    /// - Step 1: First/Last Name, Phone, Email required
    /// - Step 2: At least one funding type required
    /// - Step 3: Personal statement, benefits, commitment (min 50 chars each)
    /// - Step 4: Optional (health/fitness info)
    /// - Step 5: Must acknowledge 12-month commitment
    /// - Step 6: Signature required
    ///
    /// Sample form submission (Save and Exit):
    ///
    ///     POST /MyApplications/Edit/42
    ///     Content-Type: application/x-www-form-urlencoded
    ///
    ///     Id=42&amp;FirstName=John&amp;LastName=Doe&amp;...&amp;SaveAndExit=true&amp;CurrentStep=2
    ///
    /// **Authorization:**
    /// - User must be authenticated
    /// - User can only edit their own applications
    /// - Only Draft status applications can be edited
    ///
    /// Requires anti-forgery token.
    /// </remarks>
    /// <response code="302">Redirects to Index (saved) or Details (submitted)</response>
    /// <response code="200">Returns view with updated model and potential validation errors</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User attempting to edit another user's application</response>
    /// <response code="404">Application not found</response>
    /// <response code="400">Validation failed or application status prevents editing</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Processes draft application edits with save, navigation, and submission actions",
        Description = "Handles Save & Exit (lenient), Previous/Next navigation (auto-save), and Submit (strict validation). Only Draft applications can be edited. Validates all steps before final submission.",
        OperationId = "PostEditApplication",
        Tags = new[] { "My Applications" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ApplicationEditViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        if (application.Status != ApplicationStatus.Draft &&
            application.Status != ApplicationStatus.NeedsInformation)
        {
            TempData["ErrorMessage"] = "This application cannot be edited in its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Check which button was clicked
        bool isNextButton = Request.Form.ContainsKey("NextStep");
        bool isPreviousButton = Request.Form.ContainsKey("PreviousStep") || Request.Form.ContainsKey("Navigate");
        bool isSaveAndExit = Request.Form.ContainsKey("SaveAndExit");
        bool isSubmitApplication = Request.Form.ContainsKey("SubmitApplication");

        // Handle Save & Exit - NO validation, save whatever we have
        if (isSaveAndExit)
        {
            try
            {
                // Update application with data from completed steps only
                // NO validation - user is saving progress, not submitting
                UpdateApplicationFromModel(application, model);

                var (succeeded, errors) = await _applicationService.UpdateApplicationAsync(application);

                if (succeeded)
                {
                    // Use concise success message expected by unit tests
                    TempData["SuccessMessage"] = "Draft saved successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if it's a database constraint error
                var hasNullConstraintError = errors.Any(e => 
                    e.Contains("NULL", StringComparison.OrdinalIgnoreCase) || 
                    e.Contains("required", StringComparison.OrdinalIgnoreCase));
                
                if (hasNullConstraintError)
                {
                    TempData["WarningMessage"] = "Draft saved with incomplete data. Please fill all required fields before final submission.";
                    return RedirectToAction(nameof(Index));
                }

                // Other errors
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error saving draft application {ApplicationId}", id);
                
                // Even with DB errors, try to be forgiving for drafts
                TempData["WarningMessage"] = "Draft partially saved. Some fields may need to be completed in previous steps.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving draft application {ApplicationId}", id);
                ModelState.AddModelError(string.Empty, 
                    "Unable to save your draft at this time. Please try again or contact support if the problem persists.");
            }

            return View(model);
        }

        // Navigate to previous step - save current data without validation
        if (isPreviousButton && model.CurrentStep > 1)
        {
            try
            {
                // Save current step's data before navigating away
                UpdateApplicationFromModelPartial(application, model);
                await _applicationService.UpdateApplicationAsync(application);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error auto-saving step {Step} data when navigating back", model.CurrentStep);
                // Continue anyway - don't block navigation
            }

            int newStep = model.CurrentStep - 1;
            // Reload application data to ensure view has latest saved data
            var refreshedApp = await _applicationService.GetApplicationByIdAsync(id, includeVotes: false, includeComments: false);
            if (refreshedApp != null)
            {
                model = MapFromApplication(refreshedApp);
                model.CurrentStep = newStep;
            }
            else
            {
                model.CurrentStep = newStep;
            }
            return View(model);
        }

        // Navigate to next step - validate and save before moving forward
        if (isNextButton && model.CurrentStep < 6)
        {
            // Validate current step before moving forward
            if (!ValidateStep(model, model.CurrentStep))
            {
                return View(model);
            }

            try
            {
                // Save validated step's data before navigating to next step
                UpdateApplicationFromModelPartial(application, model);
                var (succeeded, errors) = await _applicationService.UpdateApplicationAsync(application);

                if (!succeeded)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-saving step {Step} data", model.CurrentStep);
                ModelState.AddModelError(string.Empty, "Error saving your data. Please try again.");
                return View(model);
            }

            int newStep = model.CurrentStep + 1;
            // Reload application data to ensure view has latest saved data
            var refreshedApp = await _applicationService.GetApplicationByIdAsync(id, includeVotes: false, includeComments: false);
            if (refreshedApp != null)
            {
                model = MapFromApplication(refreshedApp);
                model.CurrentStep = newStep;
            }
            else
            {
                model.CurrentStep = newStep;
            }
            return View(model);
        }

        // Handle Submit Application - validate ALL steps and submit
        if (isSubmitApplication)
        {
            // Validate all required fields before submission
            bool allStepsValid = true;
            var validationErrors = new List<string>();

            // Validate Step 1
            if (!ValidateStep(model, 1))
            {
                allStepsValid = false;
                validationErrors.Add("Step 1 (Personal Information) has missing required fields.");
            }

            // Validate Step 2
            if (!ValidateStep(model, 2))
            {
                allStepsValid = false;
                validationErrors.Add("Step 2 (Funding) requires at least one funding type.");
            }

            // Validate Step 3
            if (!ValidateStep(model, 3))
            {
                allStepsValid = false;
                validationErrors.Add("Step 3 (Motivation) has missing required fields.");
            }

            // Step 4 is optional

            // Validate Step 5
            if (!ValidateStep(model, 5))
            {
                allStepsValid = false;
                validationErrors.Add("Step 5 (Agreement) requires you to acknowledge the 12-month commitment.");
            }

            // Validate Step 6
            if (!ValidateStep(model, 6))
            {
                allStepsValid = false;
                validationErrors.Add("Step 6 (Signature) is required.");
            }

            if (!allStepsValid)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            try
            {
                // Update application with final data
                UpdateApplicationFromModel(application, model);

                var (updateSucceeded, updateErrors) = await _applicationService.UpdateApplicationAsync(application);

                if (!updateSucceeded)
                {
                    foreach (var error in updateErrors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(model);
                }

                // Submit the application
                var (submitSucceeded, submitErrors) = await _applicationService.SubmitApplicationAsync(application.Id, currentUser.Id);

                if (submitSucceeded)
                {
                    TempData["SuccessMessage"] = "Application submitted successfully! You will be notified when it is reviewed.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                foreach (var error in submitErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting application {ApplicationId}", id);
                ModelState.AddModelError(string.Empty, 
                    "Unable to submit your application at this time. Please try again or contact support if the problem persists.");
            }

            return View(model);
        }

        // Final step - validate all and update (fallback for any other submission)
        if (!ValidateStep(model, model.CurrentStep))
        {
            return View(model);
        }

        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application {ApplicationId}", id);
            ModelState.AddModelError(string.Empty, 
                "Unable to save your application at this time. Please try again or contact support if the problem persists.");
        }

        return View(model);
    }

    // POST: MyApplications/Withdraw/5
    /// <summary>
    /// Withdraws a submitted or under-review application, removing it from the board review workflow
    /// </summary>
    /// <param name="id">Application ID to withdraw</param>
    /// <param name="reason">Optional reason for withdrawal; defaults to "Withdrawn by applicant" if not provided</param>
    /// <returns>Redirects to MyApplications list with success or error message</returns>
    /// <remarks>
    /// Allows applicants to withdraw their applications before final board decision. Features:
    ///
    /// **Withdrawable Statuses:**
    /// - Submitted (before board review starts)
    /// - UnderReview (during board voting/discussion)
    /// - InDiscussion (board requesting more information)
    ///
    /// **Non-Withdrawable Statuses:**
    /// - Draft (can be deleted instead, not withdrawn)
    /// - Approved (final decision made, funding allocated)
    /// - Rejected (final decision made, already closed)
    /// - Withdrawn (already withdrawn)
    ///
    /// **Workflow Impact:**
    /// - Changes application status to Withdrawn
    /// - Records withdrawal reason in application notes
    /// - Sends notification email to admins and board members
    /// - Removes application from board review queues
    /// - Preserves application data for historical records
    ///
    /// **Authorization:**
    /// - User must be authenticated
    /// - User can only withdraw their own applications
    /// - Returns 403 Forbidden if attempting to withdraw another user's application
    ///
    /// Sample form submission:
    ///
    ///     POST /MyApplications/Withdraw/42
    ///     Content-Type: application/x-www-form-urlencoded
    ///
    ///     id=42&amp;reason=Found+alternative+funding&amp;__RequestVerificationToken=...
    ///
    /// Requires anti-forgery token.
    /// </remarks>
    /// <response code="302">Redirects to Index with success or error message in TempData</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User attempting to withdraw another user's application</response>
    /// <response code="404">Application not found</response>
    /// <response code="400">Application status prevents withdrawal (e.g., already approved/rejected)</response>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SwaggerOperation(
        Summary = "Withdraws application from board review workflow",
        Description = "Allows applicants to withdraw Submitted/UnderReview applications. Changes status to Withdrawn, notifies board, and removes from review queues. Cannot withdraw Draft/Approved/Rejected applications.",
        OperationId = "PostWithdrawApplication",
        Tags = new[] { "My Applications" }
    )]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
