using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class UpdateTaskallowsubjectIdanddepartmentIdtobenull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Department_DepartmentId",
                table: "Task");

            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                table: "Task",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Task",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Department_DepartmentId",
                table: "Task",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Department_DepartmentId",
                table: "Task");

            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Department_DepartmentId",
                table: "Task",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
