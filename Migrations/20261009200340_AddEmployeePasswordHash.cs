using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSearch.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "T_Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_T_Employees_EmployeeCode",
                table: "T_Employees",
                column: "EmployeeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Employees_EmployeeCode",
                table: "T_Employees");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "T_Employees");
        }
    }
}
