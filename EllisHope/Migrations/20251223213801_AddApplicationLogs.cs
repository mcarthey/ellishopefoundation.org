using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EllisHope.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ExceptionDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InnerException = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_CorrelationId",
                table: "ApplicationLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_CreatedAt",
                table: "ApplicationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_IsReviewed",
                table: "ApplicationLogs",
                column: "IsReviewed");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Level",
                table: "ApplicationLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Level_CreatedAt",
                table: "ApplicationLogs",
                columns: new[] { "Level", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogs");
        }
    }
}
