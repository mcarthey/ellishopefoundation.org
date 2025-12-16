using EllisHope.Models.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EllisHope.Services;

/// <summary>
/// Service for generating PDF documents
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
        
        // Configure QuestPDF license (Community license is free for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateApplicationPdfAsync(
        ClientApplication application,
        bool includeVotes = false,
        bool includeComments = false)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Text("Ellis Hope Foundation")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Title
                            column.Item().Text($"Client Application #{application.Id}")
                                .FontSize(16).SemiBold();

                            column.Item().Text($"Submitted: {application.SubmittedDate:MMMM dd, yyyy}")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Status Badge
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Status:");
                                row.RelativeItem().Text(application.Status.ToString())
                                    .SemiBold().FontColor(GetStatusColor(application.Status));
                            });

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Personal Information
                            AddSection(column, "Personal Information");
                            AddField(column, "Name", application.FullName);
                            AddField(column, "Email", application.Email);
                            AddField(column, "Phone", application.PhoneNumber);
                            AddField(column, "Address", application.FullAddress);
                            if (!string.IsNullOrEmpty(application.Occupation))
                                AddField(column, "Occupation", application.Occupation);
                            if (application.DateOfBirth.HasValue)
                                AddField(column, "Date of Birth", application.DateOfBirth.Value.ToShortDateString());

                            // Emergency Contact
                            if (!string.IsNullOrEmpty(application.EmergencyContactName))
                            {
                                AddSection(column, "Emergency Contact");
                                AddField(column, "Name", application.EmergencyContactName);
                                AddField(column, "Phone", application.EmergencyContactPhone ?? "N/A");
                            }

                            // Program Interest
                            AddSection(column, "Program Interest & Funding");
                            AddField(column, "Funding Types", string.Join(", ", application.FundingTypesList));
                            if (application.EstimatedMonthlyCost.HasValue)
                                AddField(column, "Estimated Monthly Cost", $"${application.EstimatedMonthlyCost:N2}");
                            AddField(column, "Program Duration", $"{application.ProgramDurationMonths} months");
                            if (!string.IsNullOrEmpty(application.FundingDetails))
                                AddField(column, "Funding Details", application.FundingDetails);

                            // Motivation & Commitment
                            AddSection(column, "Motivation & Commitment");
                            AddTextField(column, "Personal Statement", application.PersonalStatement);
                            AddTextField(column, "Expected Benefits", application.ExpectedBenefits);
                            AddTextField(column, "Commitment", application.CommitmentStatement);
                            if (!string.IsNullOrEmpty(application.ConcernsObstacles))
                                AddTextField(column, "Concerns/Obstacles", application.ConcernsObstacles);

                            // Health & Fitness
                            AddSection(column, "Health & Fitness");
                            AddField(column, "Current Fitness Level", application.CurrentFitnessLevel.ToString());
                            if (!string.IsNullOrEmpty(application.MedicalConditions))
                                AddTextField(column, "Medical Conditions", application.MedicalConditions);
                            if (!string.IsNullOrEmpty(application.CurrentMedications))
                                AddTextField(column, "Current Medications", application.CurrentMedications);
                            if (application.LastPhysicalExamDate.HasValue)
                                AddField(column, "Last Physical Exam", application.LastPhysicalExamDate.Value.ToShortDateString());
                            if (!string.IsNullOrEmpty(application.FitnessGoals))
                                AddTextField(column, "Fitness Goals", application.FitnessGoals);

                            // Program Requirements
                            AddSection(column, "Program Requirements Agreement");
                            AddCheckbox(column, "Agrees to work with nutritionist", application.AgreesToNutritionist);
                            AddCheckbox(column, "Agrees to work with personal trainer", application.AgreesToPersonalTrainer);
                            AddCheckbox(column, "Agrees to weekly check-ins", application.AgreesToWeeklyCheckIns);
                            AddCheckbox(column, "Agrees to progress reports", application.AgreesToProgressReports);
                            AddCheckbox(column, "Understands 12-month commitment", application.UnderstandsCommitment);

                            // Signature
                            AddSection(column, "Signature");
                            AddField(column, "Signed by", application.Signature);
                            if (application.SignedDate.HasValue)
                                AddField(column, "Date", application.SignedDate.Value.ToShortDateString());

                            // Decision (if applicable)
                            if (application.FinalDecision.HasValue)
                            {
                                column.Item().PageBreak();
                                AddSection(column, "Decision");
                                AddField(column, "Outcome", application.FinalDecision.ToString() ?? "Pending");
                                if (application.DecisionDate.HasValue)
                                    AddField(column, "Decision Date", application.DecisionDate.Value.ToShortDateString());
                                if (!string.IsNullOrEmpty(application.DecisionMessage))
                                    AddTextField(column, "Message", application.DecisionMessage);
                                if (application.ApprovedMonthlyAmount.HasValue)
                                    AddField(column, "Approved Monthly Amount", $"${application.ApprovedMonthlyAmount:N2}");
                            }

                            // Votes (if included and admin/board viewing)
                            if (includeVotes && application.Votes?.Any() == true)
                            {
                                column.Item().PageBreak();
                                AddSection(column, "Board Votes");
                                foreach (var vote in application.Votes.OrderBy(v => v.VotedDate))
                                {
                                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingBottom(10).Column(voteCol =>
                                    {
                                        voteCol.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"{vote.Voter?.FullName ?? "Unknown"}").SemiBold();
                                            row.RelativeItem().AlignRight().Text(vote.Decision.ToString())
                                                .FontColor(vote.Decision == VoteDecision.Approve ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                        });
                                        voteCol.Item().Text($"Date: {vote.VotedDate:MMM dd, yyyy}").FontSize(9);
                                        voteCol.Item().Text($"Confidence: {vote.ConfidenceLevel}/5").FontSize(9);
                                        voteCol.Item().PaddingTop(5).Text(vote.Reasoning).FontSize(10);
                                    });
                                }
                            }

                            // Comments (if included)
                            if (includeComments && application.Comments?.Any() == true)
                            {
                                column.Item().PageBreak();
                                AddSection(column, "Discussion Comments");
                                foreach (var comment in application.Comments.Where(c => !c.IsDeleted && !c.IsPrivate).OrderBy(c => c.CreatedDate))
                                {
                                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingBottom(10).Column(commentCol =>
                                    {
                                        commentCol.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"{comment.Author?.FullName ?? "Unknown"}").SemiBold();
                                            row.RelativeItem().AlignRight().Text($"{comment.CreatedDate:MMM dd, yyyy}").FontSize(9);
                                        });
                                        commentCol.Item().PaddingTop(5).Text(comment.Content).FontSize(10);
                                    });
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        });
    }

    public async Task<byte[]> GenerateApprovalLetterPdfAsync(ClientApplication application)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Ellis Hope Foundation")
                                .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            column.Item().Text(DateTime.Now.ToString("MMMM dd, yyyy"))
                                .FontSize(10);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            column.Item().Text($"Dear {application.FirstName} {application.LastName},")
                                .FontSize(12);

                            column.Item().PaddingTop(10).Text("Congratulations!")
                                .Bold().FontSize(16).FontColor(Colors.Green.Darken2);

                            column.Item().Text(application.DecisionMessage ?? 
                                "We are pleased to inform you that your application has been approved by our board of directors.")
                                .LineHeight(1.5f);

                            if (application.ApprovedMonthlyAmount.HasValue)
                            {
                                column.Item().Text($"Approved Monthly Support: ${application.ApprovedMonthlyAmount:N2}")
                                    .SemiBold().FontSize(14);
                            }

                            column.Item().Text("Program Details:")
                                .SemiBold();

                            column.Item().PaddingLeft(20).Column(details =>
                            {
                                details.Item().Text($"• Duration: {application.ProgramDurationMonths} months");
                                details.Item().Text($"• Start Date: {(application.ProgramStartDate?.ToString("MMMM dd, yyyy") ?? "To be determined")}");
                                if (application.AssignedSponsor != null)
                                {
                                    details.Item().Text($"• Your Sponsor: {application.AssignedSponsor.FullName}");
                                }
                            });

                            column.Item().PaddingTop(20).Text("Next Steps:")
                                .SemiBold();

                            column.Item().PaddingLeft(20).Column(steps =>
                            {
                                steps.Item().Text("1. We will contact you within 5 business days to schedule your initial consultation");
                                steps.Item().Text("2. Complete any required health screenings");
                                steps.Item().Text("3. Meet with your assigned trainer and nutritionist");
                                steps.Item().Text("4. Begin your fitness journey!");
                            });

                            column.Item().PaddingTop(20).Text("We look forward to supporting you on your path to better health and wellness.")
                                .LineHeight(1.5f);

                            column.Item().PaddingTop(40).Text("Sincerely,")
                                .FontSize(12);

                            column.Item().Text("The Ellis Hope Foundation Board")
                                .SemiBold().FontSize(12);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Ellis Hope Foundation | www.ellishope.org")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });

            return document.GeneratePdf();
        });
    }

    public async Task<byte[]> GenerateStatisticsReportPdfAsync(ApplicationStatistics statistics)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Ellis Hope Foundation")
                                .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            column.Item().Text("Application Statistics Report")
                                .FontSize(14);
                            column.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy HH:mm}")
                                .FontSize(9).FontColor(Colors.Grey.Darken2);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Overview
                            column.Item().Text("Overview").Bold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().BorderBottom(1).Padding(5).Text("Total Applications").SemiBold();
                                table.Cell().BorderBottom(1).Padding(5).AlignRight().Text(statistics.TotalApplications.ToString());

                                table.Cell().Padding(5).Text("Pending Review");
                                table.Cell().Padding(5).AlignRight().Text(statistics.PendingReview.ToString());

                                table.Cell().Padding(5).Text("Under Review");
                                table.Cell().Padding(5).AlignRight().Text(statistics.UnderReview.ToString());

                                table.Cell().BorderBottom(1).Padding(5).Text("Approved").FontColor(Colors.Green.Darken2);
                                table.Cell().BorderBottom(1).Padding(5).AlignRight().Text(statistics.Approved.ToString()).FontColor(Colors.Green.Darken2);

                                table.Cell().BorderBottom(1).Padding(5).Text("Rejected").FontColor(Colors.Red.Darken2);
                                table.Cell().BorderBottom(1).Padding(5).AlignRight().Text(statistics.Rejected.ToString()).FontColor(Colors.Red.Darken2);

                                table.Cell().Padding(5).Text("Active Programs");
                                table.Cell().Padding(5).AlignRight().Text(statistics.Active.ToString());

                                table.Cell().Padding(5).Text("Completed Programs");
                                table.Cell().Padding(5).AlignRight().Text(statistics.Completed.ToString());
                            });

                            // Metrics
                            column.Item().PaddingTop(20).Text("Performance Metrics").Bold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().BorderBottom(1).Padding(5).Text("Approval Rate").SemiBold();
                                table.Cell().BorderBottom(1).Padding(5).AlignRight().Text($"{statistics.ApprovalRate:N1}%");

                                table.Cell().Padding(5).Text("Average Review Time");
                                table.Cell().Padding(5).AlignRight().Text($"{statistics.AverageReviewDays:N1} days");
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        });
    }

    #region Helper Methods

    private void AddSection(ColumnDescriptor column, string title)
    {
        column.Item().PaddingTop(15).Text(title)
            .Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
        column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);
    }

    private void AddField(ColumnDescriptor column, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(150).Text($"{label}:").SemiBold();
            row.RelativeItem().Text(value);
        });
    }

    private void AddTextField(ColumnDescriptor column, string label, string value)
    {
        column.Item().Column(col =>
        {
            col.Item().Text($"{label}:").SemiBold();
            col.Item().PaddingLeft(10).Text(value).LineHeight(1.3f);
        });
    }

    private void AddCheckbox(ColumnDescriptor column, string label, bool isChecked)
    {
        column.Item().Row(row =>
        {
            row.AutoItem().Text(isChecked ? "?" : "?").FontSize(14);
            row.AutoItem().PaddingLeft(5).Text(label);
        });
    }

    private string GetStatusColor(ApplicationStatus status)
    {
        return status switch
        {
            ApplicationStatus.Approved => Colors.Green.Darken2,
            ApplicationStatus.Rejected => Colors.Red.Darken2,
            ApplicationStatus.Active => Colors.Blue.Darken2,
            ApplicationStatus.Completed => Colors.Grey.Darken2,
            _ => Colors.Orange.Darken2
        };
    }

    #endregion
}
