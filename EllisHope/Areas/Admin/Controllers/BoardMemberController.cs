using EllisHope.Areas.Admin.Models;
using EllisHope.Models.Domain;
using EllisHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EllisHope.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "BoardMember")]
    public class BoardMemberController : Controller
    {
        private readonly IClientApplicationService _applicationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BoardMemberController> _logger;

        public BoardMemberController(
            IClientApplicationService applicationService,
            UserManager<ApplicationUser> userManager,
            ILogger<BoardMemberController> logger)
        {
            _applicationService = applicationService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Admin/BoardMember/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var stats = await _applicationService.GetBoardMemberStatisticsAsync(currentUser.Id);
            var appStats = await _applicationService.GetApplicationStatisticsAsync();

            var viewModel = new BoardMemberDashboardViewModel
            {
                MemberName = currentUser.FullName,
                PendingVotes = stats.PendingVotes,
                TotalVotesCast = stats.TotalVotesCast,
                ParticipationRate = stats.ParticipationRate,
                ApplicationsUnderReview = appStats.UnderReview,
                AverageReviewDays = appStats.AverageReviewDays,
                ApprovalRate = appStats.ApprovalRate
            };

            return View(viewModel);
        }
    }
}
