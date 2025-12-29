using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EllisHope.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorShowcaseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyLogoUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInSponsorSection",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SponsorQuote",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SponsorQuoteApproved",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SponsorQuoteApprovedById",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SponsorQuoteApprovedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorQuoteRejectionReason",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SponsorQuoteSubmittedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SponsorRating",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyLogoUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ShowInSponsorSection",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SponsorQuote",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SponsorQuoteApproved",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SponsorQuoteApprovedById",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SponsorQuoteApprovedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SponsorQuoteRejectionReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SponsorQuoteSubmittedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SponsorRating",
                table: "AspNetUsers");
        }
    }
}
