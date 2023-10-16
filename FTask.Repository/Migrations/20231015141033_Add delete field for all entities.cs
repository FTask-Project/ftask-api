using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class Adddeletefieldforallentities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "TaskReport",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "TaskLecturer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "TaskActivity",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Task",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Subject",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Semester",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Lecturer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Evidence",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Department",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Attachment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TaskReport");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TaskLecturer");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "TaskActivity");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Subject");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Semester");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Lecturer");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Evidence");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "AspNetRoles");
        }
    }
}
