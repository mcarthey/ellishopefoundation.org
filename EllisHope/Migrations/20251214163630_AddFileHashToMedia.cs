using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EllisHope.Migrations
{
    /// <inheritdoc />
    public partial class AddFileHashToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "MediaLibrary",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "MediaLibrary");
        }
    }
}
