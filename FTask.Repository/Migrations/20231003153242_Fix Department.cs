using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class FixDepartment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Lecturer_DepartmentHeadId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentHeadId",
                table: "Department",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department",
                column: "DepartmentHeadId",
                unique: true,
                filter: "[DepartmentHeadId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Lecturer_DepartmentHeadId",
                table: "Department",
                column: "DepartmentHeadId",
                principalTable: "Lecturer",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Lecturer_DepartmentHeadId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentHeadId",
                table: "Department",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department",
                column: "DepartmentHeadId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Lecturer_DepartmentHeadId",
                table: "Department",
                column: "DepartmentHeadId",
                principalTable: "Lecturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
