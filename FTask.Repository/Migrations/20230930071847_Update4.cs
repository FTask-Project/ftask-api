using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class Update4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_TaskCategory_TaskCategoryId",
                table: "Task");

            migrationBuilder.DropTable(
                name: "TaskCategory");

            migrationBuilder.DropIndex(
                name: "IX_Task_TaskCategoryId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "TaskCategoryId",
                table: "Task");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskCategoryId",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TaskCategory",
                columns: table => new
                {
                    TaskCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCategory", x => x.TaskCategoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Task_TaskCategoryId",
                table: "Task",
                column: "TaskCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_TaskCategory_TaskCategoryId",
                table: "Task",
                column: "TaskCategoryId",
                principalTable: "TaskCategory",
                principalColumn: "TaskCategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
