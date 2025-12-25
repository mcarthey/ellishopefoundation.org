using EllisHope.Areas.Admin.Controllers;
using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EllisHope.Tests.Controllers.Admin;

public class ApplicationsControllerTests
{
    private readonly Mock<IClientApplicationService> _mockApplicationService;
    private readonly Mock<IPdfService> _mockPdfService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<ApplicationsController>> _mockLogger;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly ApplicationsController _controller;
    private readonly DefaultHttpContext _httpContext;
    private readonly ApplicationUser _testUser;
    private readonly ApplicationUser _testBoardMember;
    private readonly ApplicationUser _testAdmin;

    public ApplicationsControllerTests()
    {
        _mockApplicationService = new Mock<IClientApplicationService>();
        _mockPdfService = new Mock<IPdfService>();
        _mockUserManager = MockHelpers.MockUserManager<ApplicationUser>();
        _mockLogger = new Mock<ILogger<ApplicationsController>>();
        _mockUrlHelper = new Mock<IUrlHelper>();

        // Create controller using standard constructor
        _controller = new ApplicationsController(
            _mockApplicationService.Object,
            _mockPdfService.Object,
            _mockUserManager.Object,
            _mockLogger.Object
        );

        // Setup HttpContext with real ServiceCollection for IUrlHelperFactory
        _httpContext = new DefaultHttpContext();

        // Use real ServiceCollection instead of mocking IServiceProvider
        var services = new ServiceCollection();
        var urlHelperFactory = new Mock<IUrlHelperFactory>();
        urlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(_mockUrlHelper.Object);
        services.AddSingleton<IUrlHelperFactory>(urlHelperFactory.Object);
        services.AddSingleton<IUrlHelper>(_mockUrlHelper.Object);
        var serviceProvider = services.BuildServiceProvider();

        _httpContext.RequestServices = serviceProvider;

        // Setup TempData
        var tempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;

        // Setup ControllerContext
        _controller.ControllerContext = new ControllerContext { HttpContext = _httpContext };

        // Create test users with different roles
        _testUser = new ApplicationUser
        {
            Id = "user-1",
            Email = "user@test.com",
            UserName = "user@test.com",
            FirstName = "Test",
            LastName = "User",
            UserRole = UserRole.Client
        };

        _testBoardMember = new ApplicationUser
        {
            Id = "boardmember-1",
            Email = "board@test.com",
            UserName = "board@test.com",
            FirstName = "Board",
            LastName = "Member",
            UserRole = UserRole.BoardMember
        };

        _testAdmin = new ApplicationUser
        {
            Id = "admin-1",
            Email = "admin@test.com",
            UserName = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            UserRole = UserRole.Admin
        };

        // Setup User identity (BoardMember by default for most tests)
        SetupUserIdentity(_testBoardMember, "BoardMember");
    }

    private void SetupUserIdentity(ApplicationUser user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _httpContext.User = principal;
    }

    #region Index Action Tests

    [Fact]
    public async Task Index_ReturnsViewWithApplications_WhenNoFilters()
    {
        // Arrange
        var applications = new List<ClientApplication>
        {
            new ClientApplication { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Status = ApplicationStatus.UnderReview, Votes = new List<ApplicationVote>() },
            new ClientApplication { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", Status = ApplicationStatus.Submitted, Votes = new List<ApplicationVote>() }
        };

        var stats = new ApplicationStatistics { TotalApplications = 2, PendingReview = 1, UnderReview = 1 };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.GetAllApplicationsAsync(null, null, true, false)).ReturnsAsync(applications);
        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync()).ReturnsAsync(stats);
        _mockApplicationService.Setup(s => s.GetApplicationsNeedingReviewAsync(_testBoardMember.Id)).ReturnsAsync(new List<ClientApplication>());

        // Act
        var result = await _controller.Index(null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationListViewModel>(viewResult.Model);
        Assert.Equal(2, model.Applications.Count());
        Assert.Equal(2, model.TotalApplications);
    }

    [Fact]
    public async Task Index_FiltersApplicationsByStatus()
    {
        // Arrange
        var applications = new List<ClientApplication>
        {
            new ClientApplication { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Status = ApplicationStatus.UnderReview, Votes = new List<ApplicationVote>() }
        };

        var stats = new ApplicationStatistics { TotalApplications = 1, UnderReview = 1 };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.GetAllApplicationsAsync(ApplicationStatus.UnderReview, null, true, false)).ReturnsAsync(applications);
        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync()).ReturnsAsync(stats);
        _mockApplicationService.Setup(s => s.GetApplicationsNeedingReviewAsync(_testBoardMember.Id)).ReturnsAsync(new List<ClientApplication>());

        // Act
        var result = await _controller.Index(ApplicationStatus.UnderReview, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationListViewModel>(viewResult.Model);
        Assert.Single(model.Applications);
        Assert.Equal(ApplicationStatus.UnderReview, model.StatusFilter);
    }

    [Fact]
    public async Task Index_ReturnsUnauthorized_WhenUserNotFound()
    {
        // Arrange
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Index(null, null);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    #endregion

    #region NeedingReview Action Tests

    [Fact]
    public async Task NeedingReview_ReturnsApplicationsNeedingUserVote()
    {
        // Arrange
        var needingReview = new List<ClientApplication>
        {
            new ClientApplication { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Status = ApplicationStatus.UnderReview, Votes = new List<ApplicationVote>() }
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.GetApplicationsNeedingReviewAsync(_testBoardMember.Id)).ReturnsAsync(needingReview);

        // Act
        var result = await _controller.NeedingReview();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        var model = Assert.IsType<ApplicationListViewModel>(viewResult.Model);
        Assert.Single(model.Applications);
        Assert.Equal(1, model.NeedingMyVote);
    }

    #endregion

    #region Details/Review Action Tests

    [Fact]
    public async Task Details_ReturnsViewWithApplication()
    {
        // Arrange
        var application = new ClientApplication
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Status = ApplicationStatus.UnderReview,
            Votes = new List<ApplicationVote>()
        };

        var votingSummary = new VotingSummary();
        var votes = new List<ApplicationVote>();
        var comments = new List<ApplicationComment>();

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, true, true)).ReturnsAsync(application);
        _mockApplicationService.Setup(s => s.GetApplicationVotesAsync(1)).ReturnsAsync(votes);
        _mockApplicationService.Setup(s => s.GetApplicationCommentsAsync(1, true)).ReturnsAsync(comments);
        _mockApplicationService.Setup(s => s.GetVotingSummaryAsync(1)).ReturnsAsync(votingSummary);
        _mockApplicationService.Setup(s => s.GetVoteAsync(1, _testBoardMember.Id)).ReturnsAsync((ApplicationVote?)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationDetailsViewModel>(viewResult.Model);
        Assert.Equal(1, model.Application.Id);
        Assert.True(model.CanUserVote);
        Assert.False(model.HasUserVoted);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenApplicationDoesNotExist()
    {
        // Arrange
        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(999, true, true)).ReturnsAsync((ClientApplication?)null);

        // Act
        var result = await _controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Review_CallsDetails()
    {
        // Arrange
        var application = new ClientApplication { Id = 1, FirstName = "John", LastName = "Doe", Status = ApplicationStatus.UnderReview };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, true, true)).ReturnsAsync(application);
        _mockApplicationService.Setup(s => s.GetApplicationVotesAsync(1)).ReturnsAsync(new List<ApplicationVote>());
        _mockApplicationService.Setup(s => s.GetApplicationCommentsAsync(1, true)).ReturnsAsync(new List<ApplicationComment>());
        _mockApplicationService.Setup(s => s.GetVotingSummaryAsync(1)).ReturnsAsync(new VotingSummary());
        _mockApplicationService.Setup(s => s.GetVoteAsync(1, _testBoardMember.Id)).ReturnsAsync((ApplicationVote?)null);

        // Act
        var result = await _controller.Review(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationDetailsViewModel>(viewResult.Model);
        Assert.Equal(1, model.Application.Id);
    }

    #endregion

    #region Vote Action Tests

    [Fact]
    public async Task Vote_POST_CastsVote_WithValidModel()
    {
        // Arrange
        var voteModel = new VoteFormViewModel
        {
            ApplicationId = 1,
            Decision = VoteDecision.Approve,
            Reasoning = "Strong candidate with clear commitment to fitness goals and community participation.",
            ConfidenceLevel = 4
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.CastVoteAsync(1, _testBoardMember.Id, VoteDecision.Approve, voteModel.Reasoning, 4))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Vote(voteModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues!["id"]);
        Assert.Equal("Your vote has been recorded.", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Vote_POST_ReturnsError_WhenVoteFails()
    {
        // Arrange
        var voteModel = new VoteFormViewModel
        {
            ApplicationId = 1,
            Decision = VoteDecision.Approve,
            Reasoning = "Strong candidate with clear commitment.",
            ConfidenceLevel = 4
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.CastVoteAsync(1, _testBoardMember.Id, VoteDecision.Approve, voteModel.Reasoning, 4))
            .ReturnsAsync((false, new[] { "You have already voted on this application." }));

        // Act
        var result = await _controller.Vote(voteModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("You have already voted", _controller.TempData["ErrorMessage"]?.ToString());
    }

    [Fact]
    public async Task Vote_POST_ReturnsError_WhenModelStateInvalid()
    {
        // Arrange
        var voteModel = new VoteFormViewModel
        {
            ApplicationId = 1,
            Decision = VoteDecision.Approve,
            Reasoning = "", // Invalid - too short
            ConfidenceLevel = 4
        };

        _controller.ModelState.AddModelError("Reasoning", "Reasoning is required");

        // Act
        var result = await _controller.Vote(voteModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Please provide valid voting information.", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Vote_POST_SupportsAllVoteDecisions()
    {
        // Test Approve
        await TestVoteDecision(VoteDecision.Approve);

        // Test Reject
        await TestVoteDecision(VoteDecision.Reject);

        // Test NeedsMoreInfo
        await TestVoteDecision(VoteDecision.NeedsMoreInfo);

        // Test Abstain
        await TestVoteDecision(VoteDecision.Abstain);
    }

    private async Task TestVoteDecision(VoteDecision decision)
    {
        var voteModel = new VoteFormViewModel
        {
            ApplicationId = 1,
            Decision = decision,
            Reasoning = $"Voting {decision} for valid reasons with detailed explanation that meets minimum length.",
            ConfidenceLevel = 3
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.CastVoteAsync(1, _testBoardMember.Id, decision, It.IsAny<string>(), 3))
            .ReturnsAsync((true, Array.Empty<string>()));

        var result = await _controller.Vote(voteModel);

        Assert.IsType<RedirectToActionResult>(result);
    }

    #endregion

    #region Comment Action Tests

    [Fact]
    public async Task Comment_POST_AddsComment_WithValidModel()
    {
        // Arrange
        var commentModel = new CommentFormViewModel
        {
            ApplicationId = 1,
            Content = "Please clarify the medical clearance documentation.",
            IsPrivate = true,
            IsInformationRequest = false
        };

        var newComment = new ApplicationComment { Id = 1, ApplicationId = 1, Content = commentModel.Content };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.AddCommentAsync(1, _testBoardMember.Id, commentModel.Content, true, false, null))
            .ReturnsAsync((true, Array.Empty<string>(), newComment));

        // Act
        var result = await _controller.Comment(commentModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal("Comment added successfully.", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Comment_POST_ReturnsError_WhenModelStateInvalid()
    {
        // Arrange
        var commentModel = new CommentFormViewModel
        {
            ApplicationId = 1,
            Content = "", // Invalid
            IsPrivate = true
        };

        _controller.ModelState.AddModelError("Content", "Content is required");

        // Act
        var result = await _controller.Comment(commentModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Please provide a valid comment.", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Comment_POST_AddsPublicComment()
    {
        // Arrange
        var commentModel = new CommentFormViewModel
        {
            ApplicationId = 1,
            Content = "Great application! Looking forward to supporting this candidate.",
            IsPrivate = false
        };

        var newComment = new ApplicationComment { Id = 1, ApplicationId = 1, Content = commentModel.Content, IsPrivate = false };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.AddCommentAsync(1, _testBoardMember.Id, commentModel.Content, false, false, null))
            .ReturnsAsync((true, Array.Empty<string>(), newComment));

        // Act
        var result = await _controller.Comment(commentModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Comment added successfully.", _controller.TempData["SuccessMessage"]);
    }

    #endregion

    #region Approve Action Tests

    [Fact]
    public async Task Approve_GET_ReturnsViewWithModel()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        var application = new ClientApplication { Id = 1, EstimatedMonthlyCost = 150.00m };
        var sponsors = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "sponsor-1", FirstName = "Sponsor", LastName = "One", UserRole = UserRole.Sponsor, IsActive = true }
        };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, true, true)).ReturnsAsync(application);
        _mockUserManager.Setup(um => um.Users).Returns(sponsors.AsQueryable());

        // Act
        var result = await _controller.Approve(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApproveApplicationViewModel>(viewResult.Model);
        Assert.Equal(1, model.ApplicationId);
        Assert.Equal(150.00m, model.ApprovedMonthlyAmount);
        Assert.Single(model.AvailableSponsors);
    }

    [Fact]
    public async Task Approve_GET_ReturnsNotFound_WhenApplicationDoesNotExist()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");
        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(999, true, true)).ReturnsAsync((ClientApplication?)null);

        // Act
        var result = await _controller.Approve(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Approve_POST_ApprovesApplication_WithValidModel()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        var approveModel = new ApproveApplicationViewModel
        {
            ApplicationId = 1,
            ApprovedMonthlyAmount = 150.00m,
            SponsorId = "sponsor-1",
            DecisionMessage = "Approved! We're excited to support your fitness journey."
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testAdmin);
        _mockApplicationService.Setup(s => s.ApproveApplicationAsync(1, _testAdmin.Id, 150.00m, "sponsor-1", approveModel.DecisionMessage))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Approve(approveModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("Application approved successfully!", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Approve_POST_ReturnsView_WhenApprovalFails()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        var approveModel = new ApproveApplicationViewModel
        {
            ApplicationId = 1,
            ApprovedMonthlyAmount = 150.00m
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testAdmin);
        _mockApplicationService.Setup(s => s.ApproveApplicationAsync(1, _testAdmin.Id, 150.00m, null, null))
            .ReturnsAsync((false, new[] { "Application is not in a valid state for approval." }));

        // Act
        var result = await _controller.Approve(approveModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region Reject Action Tests

    [Fact]
    public async Task Reject_GET_ReturnsViewWithModel()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        var application = new ClientApplication { Id = 1 };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, true, true)).ReturnsAsync(application);

        // Act
        var result = await _controller.Reject(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RejectApplicationViewModel>(viewResult.Model);
        Assert.Equal(1, model.ApplicationId);
    }

    [Fact]
    public async Task Reject_POST_RejectsApplication_WithValidModel()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        var rejectModel = new RejectApplicationViewModel
        {
            ApplicationId = 1,
            RejectionReason = "Does not meet the current income eligibility criteria for the program."
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testAdmin);
        _mockApplicationService.Setup(s => s.RejectApplicationAsync(1, _testAdmin.Id, rejectModel.RejectionReason))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.Reject(rejectModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
        Assert.Equal("Application decision recorded.", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Reject_POST_ReturnsView_WhenRejectionFails()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        var rejectModel = new RejectApplicationViewModel
        {
            ApplicationId = 1,
            RejectionReason = "Application already processed."
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testAdmin);
        _mockApplicationService.Setup(s => s.RejectApplicationAsync(1, _testAdmin.Id, rejectModel.RejectionReason))
            .ReturnsAsync((false, new[] { "Application cannot be rejected in current state." }));

        // Act
        var result = await _controller.Reject(rejectModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region RequestInfo Action Tests

    [Fact]
    public async Task RequestInfo_GET_ReturnsViewWithModel()
    {
        // Arrange
        var application = new ClientApplication { Id = 1 };

        _mockApplicationService.Setup(s => s.GetApplicationByIdAsync(1, true, true)).ReturnsAsync(application);

        // Act
        var result = await _controller.RequestInfo(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RequestInformationViewModel>(viewResult.Model);
        Assert.Equal(1, model.ApplicationId);
    }

    [Fact]
    public async Task RequestInfo_POST_RequestsInformation_WithValidModel()
    {
        // Arrange
        var requestModel = new RequestInformationViewModel
        {
            ApplicationId = 1,
            RequestDetails = "Please upload a copy of your medical clearance form from your physician."
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.RequestAdditionalInformationAsync(1, _testBoardMember.Id, requestModel.RequestDetails))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.RequestInfo(requestModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal("Information request sent to applicant.", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task RequestInfo_POST_ReturnsView_WhenRequestFails()
    {
        // Arrange
        var requestModel = new RequestInformationViewModel
        {
            ApplicationId = 1,
            RequestDetails = "Please provide additional information."
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testBoardMember);
        _mockApplicationService.Setup(s => s.RequestAdditionalInformationAsync(1, _testBoardMember.Id, requestModel.RequestDetails))
            .ReturnsAsync((false, new[] { "Application cannot accept information requests in current state." }));

        // Act
        var result = await _controller.RequestInfo(requestModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    #endregion

    #region StartReview Action Tests

    [Fact]
    public async Task StartReview_POST_StartsReview_Successfully()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        _mockApplicationService.Setup(s => s.StartReviewProcessAsync(1))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.StartReview(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues!["id"]);
        Assert.Equal("Review process started. Board members have been notified.", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task StartReview_POST_ReturnsError_WhenStartFails()
    {
        // Arrange
        SetupUserIdentity(_testAdmin, "Admin");

        _mockApplicationService.Setup(s => s.StartReviewProcessAsync(1))
            .ReturnsAsync((false, new[] { "Application must be in Submitted status to start review." }));

        // Act
        var result = await _controller.StartReview(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("Application must be in Submitted status", _controller.TempData["ErrorMessage"]?.ToString());
    }

    #endregion

    #region Statistics Action Tests

    [Fact]
    public async Task Statistics_ReturnsViewWithStatistics()
    {
        // Arrange
        var stats = new ApplicationStatistics
        {
            TotalApplications = 50,
            PendingReview = 10,
            UnderReview = 15,
            Approved = 20,
            Rejected = 5
        };

        _mockApplicationService.Setup(s => s.GetApplicationStatisticsAsync()).ReturnsAsync(stats);

        // Act
        var result = await _controller.Statistics();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ApplicationStatistics>(viewResult.Model);
        Assert.Equal(50, model.TotalApplications);
        Assert.Equal(10, model.PendingReview);
        Assert.Equal(15, model.UnderReview);
        Assert.Equal(20, model.Approved);
        Assert.Equal(5, model.Rejected);
    }

    #endregion
}

/// <summary>
/// Helper class for mocking UserManager
/// </summary>
public static class MockHelpers
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }
}

