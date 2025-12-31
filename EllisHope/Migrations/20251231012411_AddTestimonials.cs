using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EllisHope.Migrations
{
    /// <inheritdoc />
    public partial class AddTestimonials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AuthorRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuthorPhotoId = table.Column<int>(type: "int", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ApprovedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Testimonials_AspNetUsers_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Testimonials_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Testimonials_MediaLibrary_AuthorPhotoId",
                        column: x => x.AuthorPhotoId,
                        principalTable: "MediaLibrary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_ApprovedById",
                table: "Testimonials",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_AuthorPhotoId",
                table: "Testimonials",
                column: "AuthorPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_CreatedById",
                table: "Testimonials",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_DisplayOrder",
                table: "Testimonials",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_IsFeatured",
                table: "Testimonials",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_IsPublished",
                table: "Testimonials",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_RequiresApproval",
                table: "Testimonials",
                column: "RequiresApproval");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Testimonials");
        }
    }
}
