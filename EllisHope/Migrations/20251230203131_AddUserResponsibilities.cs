using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EllisHope.Migrations
{
    /// <inheritdoc />
    public partial class AddUserResponsibilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Events",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Events",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresApproval",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Causes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Causes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Causes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresApproval",
                table: "Causes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "BlogPosts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "BlogPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "BlogPosts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresApproval",
                table: "BlogPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserResponsibilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Responsibility = table.Column<int>(type: "int", nullable: false),
                    AutoApprove = table.Column<bool>(type: "bit", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResponsibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserResponsibilities_AspNetUsers_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserResponsibilities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_ApprovedById",
                table: "Events",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedById",
                table: "Events",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Events_RequiresApproval",
                table: "Events",
                column: "RequiresApproval");

            migrationBuilder.CreateIndex(
                name: "IX_Causes_ApprovedById",
                table: "Causes",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Causes_CreatedById",
                table: "Causes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Causes_RequiresApproval",
                table: "Causes",
                column: "RequiresApproval");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_ApprovedById",
                table: "BlogPosts",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_CreatedById",
                table: "BlogPosts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_RequiresApproval",
                table: "BlogPosts",
                column: "RequiresApproval");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponsibilities_AssignedById",
                table: "UserResponsibilities",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponsibilities_Responsibility",
                table: "UserResponsibilities",
                column: "Responsibility");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponsibilities_UserId_Responsibility",
                table: "UserResponsibilities",
                columns: new[] { "UserId", "Responsibility" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_AspNetUsers_ApprovedById",
                table: "BlogPosts",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_AspNetUsers_CreatedById",
                table: "BlogPosts",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Causes_AspNetUsers_ApprovedById",
                table: "Causes",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Causes_AspNetUsers_CreatedById",
                table: "Causes",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AspNetUsers_ApprovedById",
                table: "Events",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AspNetUsers_CreatedById",
                table: "Events",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_AspNetUsers_ApprovedById",
                table: "BlogPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_AspNetUsers_CreatedById",
                table: "BlogPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_Causes_AspNetUsers_ApprovedById",
                table: "Causes");

            migrationBuilder.DropForeignKey(
                name: "FK_Causes_AspNetUsers_CreatedById",
                table: "Causes");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_ApprovedById",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_CreatedById",
                table: "Events");

            migrationBuilder.DropTable(
                name: "UserResponsibilities");

            migrationBuilder.DropIndex(
                name: "IX_Events_ApprovedById",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_CreatedById",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_RequiresApproval",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Causes_ApprovedById",
                table: "Causes");

            migrationBuilder.DropIndex(
                name: "IX_Causes_CreatedById",
                table: "Causes");

            migrationBuilder.DropIndex(
                name: "IX_Causes_RequiresApproval",
                table: "Causes");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_ApprovedById",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_CreatedById",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_RequiresApproval",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequiresApproval",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Causes");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Causes");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Causes");

            migrationBuilder.DropColumn(
                name: "RequiresApproval",
                table: "Causes");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "RequiresApproval",
                table: "BlogPosts");
        }
    }
}
