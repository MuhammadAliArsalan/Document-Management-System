using DocumentSearch.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentSearch.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261010000000_AddEmployeeIsAdmin")]
public partial class AddEmployeeIsAdmin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsAdmin",
            table: "T_Employees",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsAdmin",
            table: "T_Employees");
    }
}
