using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSearch.Migrations
{
    /// <inheritdoc />
    public partial class AddExtractedTextToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtractedText",
                table: "T_Documents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedText",
                table: "T_Documents");
        }
    }
}
