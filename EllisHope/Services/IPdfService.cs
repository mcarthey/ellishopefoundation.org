using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service interface for PDF generation
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generate PDF for client application
    /// </summary>
    Task<byte[]> GenerateApplicationPdfAsync(ClientApplication application, bool includeVotes = false, bool includeComments = false);

    /// <summary>
    /// Generate approval letter PDF
    /// </summary>
    Task<byte[]> GenerateApprovalLetterPdfAsync(ClientApplication application);

    /// <summary>
    /// Generate statistics report PDF
    /// </summary>
    Task<byte[]> GenerateStatisticsReportPdfAsync(ApplicationStatistics statistics);

    /// <summary>
    /// Generate a blank application form PDF for printing
    /// </summary>
    Task<byte[]> GenerateBlankApplicationFormPdfAsync();
}
