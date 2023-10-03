using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class TrytomakedepartmentheadIdnottobeuniqueandcanbenullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department",
                column: "DepartmentHeadId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department",
                column: "DepartmentHeadId",
                unique: true,
                filter: "[DepartmentHeadId] IS NOT NULL");
        }
    }
}
