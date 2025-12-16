using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EllisHope.Migrations
{
    /// <inheritdoc />
    public partial class AddClientApplicationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Occupation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FundingTypesRequested = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedMonthlyCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ProgramDurationMonths = table.Column<int>(type: "int", nullable: false),
                    FundingDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PersonalStatement = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    ExpectedBenefits = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    CommitmentStatement = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ConcernsObstacles = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MedicalConditions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CurrentMedications = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastPhysicalExamDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FitnessGoals = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CurrentFitnessLevel = table.Column<int>(type: "int", nullable: false),
                    AgreesToNutritionist = table.Column<bool>(type: "bit", nullable: false),
                    AgreesToPersonalTrainer = table.Column<bool>(type: "bit", nullable: false),
                    AgreesToWeeklyCheckIns = table.Column<bool>(type: "bit", nullable: false),
                    AgreesToProgressReports = table.Column<bool>(type: "bit", nullable: false),
                    UnderstandsCommitment = table.Column<bool>(type: "bit", nullable: false),
                    MedicalClearanceDocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceLettersDocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncomeVerificationDocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherDocumentsUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Signature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmissionIpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VotesRequiredForApproval = table.Column<int>(type: "int", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewStartedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalDecision = table.Column<int>(type: "int", nullable: true),
                    DecisionMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DecisionMadeById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedSponsorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProgramStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgramEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedMonthlyAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientApplications_AspNetUsers_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientApplications_AspNetUsers_AssignedSponsorId",
                        column: x => x.AssignedSponsorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClientApplications_AspNetUsers_DecisionMadeById",
                        column: x => x.DecisionMadeById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientApplications_AspNetUsers_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    IsInformationRequest = table.Column<bool>(type: "bit", nullable: false),
                    HasResponse = table.Column<bool>(type: "bit", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationComments_ApplicationComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "ApplicationComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationComments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationComments_ClientApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ClientApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationNotifications_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationNotifications_ClientApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ClientApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    VoterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Reasoning = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ConfidenceLevel = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    VotedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoterIpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationVotes_AspNetUsers_VoterId",
                        column: x => x.VoterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationVotes_ClientApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ClientApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationComments_ApplicationId",
                table: "ApplicationComments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationComments_AuthorId",
                table: "ApplicationComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationComments_CreatedDate",
                table: "ApplicationComments",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationComments_IsPrivate",
                table: "ApplicationComments",
                column: "IsPrivate");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationComments_ParentCommentId",
                table: "ApplicationComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotifications_ApplicationId",
                table: "ApplicationNotifications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotifications_CreatedDate",
                table: "ApplicationNotifications",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotifications_RecipientId_IsRead",
                table: "ApplicationNotifications",
                columns: new[] { "RecipientId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotifications_Type",
                table: "ApplicationNotifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVotes_ApplicationId_VoterId",
                table: "ApplicationVotes",
                columns: new[] { "ApplicationId", "VoterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVotes_VotedDate",
                table: "ApplicationVotes",
                column: "VotedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVotes_VoterId",
                table: "ApplicationVotes",
                column: "VoterId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_ApplicantId",
                table: "ClientApplications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_AssignedSponsorId",
                table: "ClientApplications",
                column: "AssignedSponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_DecisionMadeById",
                table: "ClientApplications",
                column: "DecisionMadeById");

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_LastModifiedById",
                table: "ClientApplications",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_Status",
                table: "ClientApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_Status_SubmittedDate",
                table: "ClientApplications",
                columns: new[] { "Status", "SubmittedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_SubmittedDate",
                table: "ClientApplications",
                column: "SubmittedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationComments");

            migrationBuilder.DropTable(
                name: "ApplicationNotifications");

            migrationBuilder.DropTable(
                name: "ApplicationVotes");

            migrationBuilder.DropTable(
                name: "ClientApplications");
        }
    }
}
