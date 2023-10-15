using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class Update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Department_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Lecturer_Subjects_AspNetUsers_UserId",
                table: "Lecturer_Subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskActivity_TaskUser_TaskUserId",
                table: "TaskActivity");

            migrationBuilder.DropTable(
                name: "TaskUser");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Lecturer_Subjects",
                newName: "LecturerId");

            migrationBuilder.RenameIndex(
                name: "IX_Lecturer_Subjects_UserId",
                table: "Lecturer_Subjects",
                newName: "IX_Lecturer_Subjects_LecturerId");

            migrationBuilder.CreateTable(
                name: "Lecturer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lecturer_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId");
                });

            migrationBuilder.CreateTable(
                name: "TaskLecturer",
                columns: table => new
                {
                    TaskUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLecturer", x => x.TaskUserId);
                    table.ForeignKey(
                        name: "FK_TaskLecturer_Lecturer_UserId",
                        column: x => x.UserId,
                        principalTable: "Lecturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskLecturer_Task_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Task",
                        principalColumn: "TaskId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lecturer_DepartmentId",
                table: "Lecturer",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLecturer_TaskId",
                table: "TaskLecturer",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLecturer_UserId",
                table: "TaskLecturer",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lecturer_Subjects_Lecturer_LecturerId",
                table: "Lecturer_Subjects",
                column: "LecturerId",
                principalTable: "Lecturer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskActivity_TaskLecturer_TaskUserId",
                table: "TaskActivity",
                column: "TaskUserId",
                principalTable: "TaskLecturer",
                principalColumn: "TaskUserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lecturer_Subjects_Lecturer_LecturerId",
                table: "Lecturer_Subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskActivity_TaskLecturer_TaskUserId",
                table: "TaskActivity");

            migrationBuilder.DropTable(
                name: "TaskLecturer");

            migrationBuilder.DropTable(
                name: "Lecturer");

            migrationBuilder.RenameColumn(
                name: "LecturerId",
                table: "Lecturer_Subjects",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Lecturer_Subjects_LecturerId",
                table: "Lecturer_Subjects",
                newName: "IX_Lecturer_Subjects_UserId");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskUser",
                columns: table => new
                {
                    TaskUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskUser", x => x.TaskUserId);
                    table.ForeignKey(
                        name: "FK_TaskUser_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskUser_Task_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Task",
                        principalColumn: "TaskId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_TaskId",
                table: "TaskUser",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_UserId",
                table: "TaskUser",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Department_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lecturer_Subjects_AspNetUsers_UserId",
                table: "Lecturer_Subjects",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskActivity_TaskUser_TaskUserId",
                table: "TaskActivity",
                column: "TaskUserId",
                principalTable: "TaskUser",
                principalColumn: "TaskUserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
