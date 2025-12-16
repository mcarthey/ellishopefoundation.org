using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service interface for analytics and reporting
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Get comprehensive dashboard analytics
    /// </summary>
    Task<DashboardAnalytics> GetDashboardAnalyticsAsync(string? userId = null, UserRole? role = null);

    /// <summary>
    /// Get application trends over time
    /// </summary>
    Task<ApplicationTrends> GetApplicationTrendsAsync(int months = 12);

    /// <summary>
    /// Get funding type breakdown
    /// </summary>
    Task<IEnumerable<FundingTypeStatistic>> GetFundingTypeStatisticsAsync();

    /// <summary>
    /// Get board member performance metrics
    /// </summary>
    Task<IEnumerable<BoardMemberPerformance>> GetBoardMemberPerformanceAsync();

    /// <summary>
    /// Get sponsor performance metrics
    /// </summary>
    Task<IEnumerable<SponsorPerformance>> GetSponsorPerformanceAsync();
}

/// <summary>
/// Comprehensive dashboard analytics
/// </summary>
public class DashboardAnalytics
{
    // Overall Statistics
    public int TotalApplications { get; set; }
    public int PendingReview { get; set; }
    public int ActivePrograms { get; set; }
    public int CompletedPrograms { get; set; }
    public decimal ApprovalRate { get; set; }
    public double AverageReviewDays { get; set; }
    
    // Monthly Trends
    public List<MonthlyData> Last12Months { get; set; } = new();
    
    // Status Breakdown
    public Dictionary<ApplicationStatus, int> StatusCounts { get; set; } = new();
    
    // Funding Types
    public Dictionary<FundingType, int> FundingTypeCounts { get; set; } = new();
    
    // Recent Activity
    public List<RecentActivity> RecentActivities { get; set; } = new();
    
    // User-Specific (if provided)
    public int? UserPendingVotes { get; set; }
    public int? UserTotalVotes { get; set; }
    public int? UserSponsoredClients { get; set; }
}

/// <summary>
/// Monthly trend data
/// </summary>
public class MonthlyData
{
    public string Month { get; set; } = string.Empty;
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Active { get; set; }
}

/// <summary>
/// Recent activity item
/// </summary>
public class RecentActivity
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ApplicationId { get; set; }
}

/// <summary>
/// Application trends
/// </summary>
public class ApplicationTrends
{
    public List<MonthlyData> MonthlyData { get; set; } = new();
    public double AverageSubmissionsPerMonth { get; set; }
    public double ApprovalTrend { get; set; } // Percentage change
    public int TotalLastYear { get; set; }
    public int TotalThisYear { get; set; }
}

/// <summary>
/// Funding type statistics
/// </summary>
public class FundingTypeStatistic
{
    public FundingType Type { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public decimal TotalApprovedAmount { get; set; }
}

/// <summary>
/// Board member performance
/// </summary>
public class BoardMemberPerformance
{
    public string MemberId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public int ApprovalsGiven { get; set; }
    public int RejectionsGiven { get; set; }
    public int PendingVotes { get; set; }
    public double ParticipationRate { get; set; }
    public double AverageConfidenceLevel { get; set; }
    public double AverageResponseTimeDays { get; set; }
}

/// <summary>
/// Sponsor performance
/// </summary>
public class SponsorPerformance
{
    public string SponsorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int CompletedPrograms { get; set; }
    public double SuccessRate { get; set; }
    public decimal TotalFundingProvided { get; set; }
}
