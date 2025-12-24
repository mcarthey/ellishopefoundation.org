using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EllisHope.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,BoardMember")]
public class ApplicationsController : Controller
{
    private readonly IClientApplicationService _applicationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(
        IClientApplicationService applicationService,
        UserManager<ApplicationUser> userManager,
        ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _userManager = userManager;
        _logger = logger;
    }

    #region List/Index Actions

    // GET: Admin/Applications
    /// <summary>
    /// list applications; optional `status`, `searchTerm` query parameters. Roles: Admin, BoardMember.
    /// </summary>
    [SwaggerOperation(Summary = "list applications; optional `status`, `searchTerm` query parameters. Roles: Admin, BoardMember.")]
    public async Task<IActionResult> Index(ApplicationStatus? status, string? searchTerm)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var applications = await _applicationService.GetAllApplicationsAsync(
            status: status,
            includeVotes: true,
            includeComments: false);

        var statistics = await _applicationService.GetApplicationStatisticsAsync();
        var needingMyVote = await _applicationService.GetApplicationsNeedingReviewAsync(currentUser.Id);

        var viewModel = new ApplicationListViewModel
        {
            Applications = applications.Select(a => new ApplicationSummaryViewModel
            {
                Id = a.Id,
                ApplicantName = a.FullName,
                ApplicantEmail = a.Email,
                Status = a.Status,
                SubmittedDate = a.SubmittedDate,
                DaysSinceSubmission = a.DaysSinceSubmission,
                FundingTypes = string.Join(", ", a.FundingTypesList),
                EstimatedMonthlyCost = a.EstimatedMonthlyCost,
                TotalVotes = a.Votes.Count,
                VotesRequired = a.VotesRequiredForApproval,
                ApprovalVotes = a.ApprovalVoteCount,
                RejectionVotes = a.RejectionVoteCount,
                HasUserVoted = a.Votes.Any(v => v.VoterId == currentUser.Id)
            }),
            StatusFilter = status,
            SearchTerm = searchTerm,
            TotalApplications = statistics.TotalApplications,
            PendingReview = statistics.PendingReview,
            UnderReview = statistics.UnderReview,
            NeedingMyVote = needingMyVote.Count(),
            Approved = statistics.Approved,
            Rejected = statistics.Rejected
        };

        return View(viewModel);
    }

    // GET: Admin/Applications/NeedingReview
    /// <summary>
    /// list apps needing current user's review.
    /// </summary>
    [SwaggerOperation(Summary = "list apps needing current user's review.")]
    public async Task<IActionResult> NeedingReview()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var applications = await _applicationService.GetApplicationsNeedingReviewAsync(currentUser.Id);

        var viewModel = new ApplicationListViewModel
        {
            Applications = applications.Select(a => new ApplicationSummaryViewModel
            {
                Id = a.Id,
                ApplicantName = a.FullName,
                ApplicantEmail = a.Email,
                Status = a.Status,
                SubmittedDate = a.SubmittedDate,
                DaysSinceSubmission = a.DaysSinceSubmission,
                FundingTypes = string.Join(", ", a.FundingTypesList),
                EstimatedMonthlyCost = a.EstimatedMonthlyCost,
                TotalVotes = a.Votes.Count,
                VotesRequired = a.VotesRequiredForApproval,
                ApprovalVotes = a.ApprovalVoteCount,
                RejectionVotes = a.RejectionVoteCount,
                HasUserVoted = false // Already filtered out
            }),
            NeedingMyVote = applications.Count()
        };

        return View("Index", viewModel);
    }

    #endregion

    #region Details/Review Actions

    // GET: Admin/Applications/Details/5
    /// <summary>
    /// app details with votes/comments. Roles: Admin, BoardMember.
    /// </summary>
    [SwaggerOperation(Summary = "app details with votes/comments. Roles: Admin, BoardMember.")]
    public async Task<IActionResult> Details(int id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)          
        {
            return Unauthorized();
        }

        var votes = await _applicationService.GetApplicationVotesAsync(id);
        var comments = await _applicationService.GetApplicationCommentsAsync(id);
        var votingSummary = await _applicationService.GetVotingSummaryAsync(id);
        var userVote = await _applicationService.GetVoteAsync(id, currentUser.Id);

        var viewModel = new ApplicationDetailsViewModel
        {
            Application = application,
            Votes = votes,
            Comments = comments,
            VotingSummary = votingSummary,
            CanUserVote = User.IsInRole("BoardMember") && 
                         (application.Status == ApplicationStatus.UnderReview || 
                          application.Status == ApplicationStatus.InDiscussion),
            HasUserVoted = userVote != null,
            UserVote = userVote,
            CanApproveReject = User.IsInRole("Admin"),
            CanEdit = application.Status == ApplicationStatus.Draft,
            VoteForm = new VoteFormViewModel { ApplicationId = id },
            CommentForm = new CommentFormViewModel { ApplicationId = id }
        };

        return View(viewModel);
    }

    // GET: Admin/Applications/Review/5 (same as Details but emphasized for voting)
    /// <summary>
    /// alias to Details for review.
    /// </summary>
    [SwaggerOperation(Summary = "alias to Details for review.")]
    public async Task<IActionResult> Review(int id)
    {
        return await Details(id);
    }

    #endregion

    #region Voting Actions

    // POST: Admin/Applications/Vote
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "BoardMember")]
    /// <summary>
    /// cast vote (VoteFormViewModel: ApplicationId, Decision, Reasoning, ConfidenceLevel). Roles: BoardMember. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "cast vote (VoteFormViewModel: ApplicationId, Decision, Reasoning, ConfidenceLevel). Roles: BoardMember. Anti-forgery required.")]
    public async Task<IActionResult> Vote(VoteFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide valid voting information.";
            return RedirectToAction(nameof(Details), new { id = model.ApplicationId });
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var (succeeded, errors) = await _applicationService.CastVoteAsync(
            model.ApplicationId,
            currentUser.Id,
            model.Decision,
            model.Reasoning,
            model.ConfidenceLevel);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Your vote has been recorded.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", errors);
        }

        return RedirectToAction(nameof(Details), new { id = model.ApplicationId });
    }

    #endregion

    #region Comment Actions

    // POST: Admin/Applications/Comment
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// add comment (CommentFormViewModel: ApplicationId, Content, IsPrivate, ...). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "add comment (CommentFormViewModel: ApplicationId, Content, IsPrivate, ...). Anti-forgery required.")]
    public async Task<IActionResult> Comment(CommentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide a valid comment.";
            return RedirectToAction(nameof(Details), new { id = model.ApplicationId });
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var (succeeded, errors, comment) = await _applicationService.AddCommentAsync(
            model.ApplicationId,
            currentUser.Id,
            model.Content,
            model.IsPrivate,
            model.IsInformationRequest,
            model.ParentCommentId);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Comment added successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", errors);
        }

        return RedirectToAction(nameof(Details), new { id = model.ApplicationId });
    }

    #endregion

    #region Decision Actions

    // GET: Admin/Applications/Approve/5
    [Authorize(Roles = "Admin")]
    /// <summary>
    /// show approve form. Roles: Admin.
    /// </summary>
    [SwaggerOperation(Summary = "show approve form. Roles: Admin.")]
    public async Task<IActionResult> Approve(int id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var sponsors = _userManager.Users
            .Where(u => u.UserRole == UserRole.Sponsor && u.IsActive)
            .ToList();

        var viewModel = new ApproveApplicationViewModel
        {
            ApplicationId = id,
            ApprovedMonthlyAmount = application.EstimatedMonthlyCost,
            DecisionMessage = "Congratulations! Your application has been approved. We look forward to supporting you on your fitness journey.",
            AvailableSponsors = sponsors.Select(s => new SponsorSelectItem
            {
                Id = s.Id,
                Name = s.FullName,
                CurrentClientCount = s.SponsoredClients?.Count ?? 0
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: Admin/Applications/Approve
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    /// <summary>
    /// approve application (ApproveApplicationViewModel). Roles: Admin. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "approve application (ApproveApplicationViewModel). Roles: Admin. Anti-forgery required.")]
    public async Task<IActionResult> Approve(ApproveApplicationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var (succeeded, errors) = await _applicationService.ApproveApplicationAsync(
            model.ApplicationId,
            currentUser.Id,
            model.ApprovedMonthlyAmount,
            model.SponsorId,
            model.DecisionMessage);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Application approved successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    // GET: Admin/Applications/Reject/5
    [Authorize(Roles = "Admin")]
    /// <summary>
    /// show reject form. Roles: Admin.
    /// </summary>
    [SwaggerOperation(Summary = "show reject form. Roles: Admin.")]
    public async Task<IActionResult> Reject(int id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var viewModel = new RejectApplicationViewModel
        {
            ApplicationId = id
        };

        return View(viewModel);
    }

    // POST: Admin/Applications/Reject
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    /// <summary>
    /// reject application (RejectApplicationViewModel). Roles: Admin. Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "reject application (RejectApplicationViewModel). Roles: Admin. Anti-forgery required.")]
    public async Task<IActionResult> Reject(RejectApplicationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var (succeeded, errors) = await _applicationService.RejectApplicationAsync(
            model.ApplicationId,
            currentUser.Id,
            model.RejectionReason);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Application decision recorded.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    // GET: Admin/Applications/RequestInfo/5
    /// <summary>
    /// show request info form.
    /// </summary>
    [SwaggerOperation(Summary = "show request info form.")]
    public async Task<IActionResult> RequestInfo(int id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var viewModel = new RequestInformationViewModel
        {
            ApplicationId = id
        };

        return View(viewModel);
    }

    // POST: Admin/Applications/RequestInfo
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// request info (RequestInformationViewModel). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "request info (RequestInformationViewModel). Anti-forgery required.")]
    public async Task<IActionResult> RequestInfo(RequestInformationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var (succeeded, errors) = await _applicationService.RequestAdditionalInformationAsync(
            model.ApplicationId,
            currentUser.Id,
            model.RequestDetails);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Information request sent to applicant.";
            return RedirectToAction(nameof(Details), new { id = model.ApplicationId });
        }

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    // POST: Admin/Applications/StartReview/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    /// <summary>
    /// start review workflow (Admin). Anti-forgery required.
    /// </summary>
    [SwaggerOperation(Summary = "start review workflow (Admin). Anti-forgery required.")]
    public async Task<IActionResult> StartReview(int id)
    {
        var (succeeded, errors) = await _applicationService.StartReviewProcessAsync(id);

        if (succeeded)
        {
            TempData["SuccessMessage"] = "Review process started. Board members have been notified.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", errors);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    #endregion

    #region Helper Actions

    // GET: Admin/Applications/Statistics
    /// <summary>
    /// view statistics summary.
    /// </summary>
    [SwaggerOperation(Summary = "view statistics summary.")]
    public async Task<IActionResult> Statistics()
    {
        var stats = await _applicationService.GetApplicationStatisticsAsync();
        return View(stats);
    }

    #endregion
}
