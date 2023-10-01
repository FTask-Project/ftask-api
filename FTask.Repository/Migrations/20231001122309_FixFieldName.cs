using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class FixFieldName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Lecturer_ManagerId",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskActivity_TaskLecturer_TaskUserId",
                table: "TaskActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLecturer_Lecturer_UserId",
                table: "TaskLecturer");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskACtivityId",
                table: "TaskReport");

            migrationBuilder.DropIndex(
                name: "IX_TaskReport_TaskACtivityId",
                table: "TaskReport");

            migrationBuilder.RenameColumn(
                name: "TaskACtivityId",
                table: "TaskReport",
                newName: "TaskActivityId");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "TaskReport",
                newName: "ReportContent");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TaskLecturer",
                newName: "LecturerId");

            migrationBuilder.RenameColumn(
                name: "TaskUserId",
                table: "TaskLecturer",
                newName: "TaskLecturerId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskLecturer_UserId",
                table: "TaskLecturer",
                newName: "IX_TaskLecturer_LecturerId");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TaskActivity",
                newName: "ActivityTitle");

            migrationBuilder.RenameColumn(
                name: "TaskUserId",
                table: "TaskActivity",
                newName: "TaskLecturerId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "TaskActivity",
                newName: "ActivityDescription");

            migrationBuilder.RenameIndex(
                name: "IX_TaskActivity_TaskUserId",
                table: "TaskActivity",
                newName: "IX_TaskActivity_TaskLecturerId");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Task",
                newName: "TaskTitle");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Task",
                newName: "TaskLevel");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Task",
                newName: "TaskContent");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Subject",
                newName: "SubjectCode");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_Code",
                table: "Subject",
                newName: "IX_Subject_SubjectCode");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Semester",
                newName: "SemesterCode");

            migrationBuilder.RenameIndex(
                name: "IX_Semester_Code",
                table: "Semester",
                newName: "IX_Semester_SemesterCode");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Department",
                newName: "DepartmentHeadId");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Department",
                newName: "DepartmentCode");

            migrationBuilder.RenameIndex(
                name: "IX_Department_ManagerId",
                table: "Department",
                newName: "IX_Department_DepartmentHeadId");

            migrationBuilder.RenameIndex(
                name: "IX_Department_Code",
                table: "Department",
                newName: "IX_Department_DepartmentCode");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_TaskActivityId",
                table: "TaskReport",
                column: "TaskActivityId",
                unique: true,
                filter: "[TaskActivityId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Lecturer_DepartmentHeadId",
                table: "Department",
                column: "DepartmentHeadId",
                principalTable: "Lecturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskActivity_TaskLecturer_TaskLecturerId",
                table: "TaskActivity",
                column: "TaskLecturerId",
                principalTable: "TaskLecturer",
                principalColumn: "TaskLecturerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLecturer_Lecturer_LecturerId",
                table: "TaskLecturer",
                column: "LecturerId",
                principalTable: "Lecturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskActivityId",
                table: "TaskReport",
                column: "TaskActivityId",
                principalTable: "TaskActivity",
                principalColumn: "TaskActivityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Lecturer_DepartmentHeadId",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskActivity_TaskLecturer_TaskLecturerId",
                table: "TaskActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLecturer_Lecturer_LecturerId",
                table: "TaskLecturer");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskActivityId",
                table: "TaskReport");

            migrationBuilder.DropIndex(
                name: "IX_TaskReport_TaskActivityId",
                table: "TaskReport");

            migrationBuilder.RenameColumn(
                name: "TaskActivityId",
                table: "TaskReport",
                newName: "TaskACtivityId");

            migrationBuilder.RenameColumn(
                name: "ReportContent",
                table: "TaskReport",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "LecturerId",
                table: "TaskLecturer",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "TaskLecturerId",
                table: "TaskLecturer",
                newName: "TaskUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskLecturer_LecturerId",
                table: "TaskLecturer",
                newName: "IX_TaskLecturer_UserId");

            migrationBuilder.RenameColumn(
                name: "TaskLecturerId",
                table: "TaskActivity",
                newName: "TaskUserId");

            migrationBuilder.RenameColumn(
                name: "ActivityTitle",
                table: "TaskActivity",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "ActivityDescription",
                table: "TaskActivity",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_TaskActivity_TaskLecturerId",
                table: "TaskActivity",
                newName: "IX_TaskActivity_TaskUserId");

            migrationBuilder.RenameColumn(
                name: "TaskTitle",
                table: "Task",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "TaskLevel",
                table: "Task",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "TaskContent",
                table: "Task",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "SubjectCode",
                table: "Subject",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_SubjectCode",
                table: "Subject",
                newName: "IX_Subject_Code");

            migrationBuilder.RenameColumn(
                name: "SemesterCode",
                table: "Semester",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_Semester_SemesterCode",
                table: "Semester",
                newName: "IX_Semester_Code");

            migrationBuilder.RenameColumn(
                name: "DepartmentHeadId",
                table: "Department",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "DepartmentCode",
                table: "Department",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_Department_DepartmentHeadId",
                table: "Department",
                newName: "IX_Department_ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Department_DepartmentCode",
                table: "Department",
                newName: "IX_Department_Code");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_TaskACtivityId",
                table: "TaskReport",
                column: "TaskACtivityId",
                unique: true,
                filter: "[TaskACtivityId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Lecturer_ManagerId",
                table: "Department",
                column: "ManagerId",
                principalTable: "Lecturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskActivity_TaskLecturer_TaskUserId",
                table: "TaskActivity",
                column: "TaskUserId",
                principalTable: "TaskLecturer",
                principalColumn: "TaskUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLecturer_Lecturer_UserId",
                table: "TaskLecturer",
                column: "UserId",
                principalTable: "Lecturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskACtivityId",
                table: "TaskReport",
                column: "TaskACtivityId",
                principalTable: "TaskActivity",
                principalColumn: "TaskActivityId");
        }
    }
}
