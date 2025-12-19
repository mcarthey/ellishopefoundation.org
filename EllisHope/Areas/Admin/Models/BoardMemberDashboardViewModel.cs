namespace EllisHope.Areas.Admin.Models
{
    public class BoardMemberDashboardViewModel
    {
        public string MemberName { get; set; } = string.Empty;
        public int PendingVotes { get; set; }
        public int TotalVotesCast { get; set; }
        public double ParticipationRate { get; set; }
        public int ApplicationsUnderReview { get; set; }
        public double AverageReviewDays { get; set; }
        public decimal ApprovalRate { get; set; }
    }
}
