using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class Update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_AspNetUsers_ManagerId",
                table: "Department");

            migrationBuilder.DropTable(
                name: "Lecturer_Subjects");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LecturerSubject",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    LecturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerSubject", x => new { x.SubjectId, x.LecturerId });
                    table.ForeignKey(
                        name: "FK_LecturerSubject_Lecturer_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LecturerSubject_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateTable(
                name: "UserDepartment",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartment", x => new { x.DepartmentId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserDepartment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserDepartment_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LecturerSubject_LecturerId",
                table: "LecturerSubject",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartment_UserId",
                table: "UserDepartment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Lecturer_ManagerId",
                table: "Department",
                column: "ManagerId",
                principalTable: "Lecturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Lecturer_ManagerId",
                table: "Department");

            migrationBuilder.DropTable(
                name: "LecturerSubject");

            migrationBuilder.DropTable(
                name: "UserDepartment");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Task");

            migrationBuilder.CreateTable(
                name: "Lecturer_Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    LecturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturer_Subjects", x => new { x.SubjectId, x.LecturerId });
                    table.ForeignKey(
                        name: "FK_Lecturer_Subjects_Lecturer_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Lecturer_Subjects_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "SubjectId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lecturer_Subjects_LecturerId",
                table: "Lecturer_Subjects",
                column: "LecturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Department_AspNetUsers_ManagerId",
                table: "Department",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
