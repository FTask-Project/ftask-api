using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class UpdateaconstraintforafieldofTaskReporttobenotnull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskActivityId",
                table: "TaskReport");

            migrationBuilder.DropIndex(
                name: "IX_TaskReport_TaskActivityId",
                table: "TaskReport");

            migrationBuilder.AlterColumn<int>(
                name: "TaskActivityId",
                table: "TaskReport",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_TaskActivityId",
                table: "TaskReport",
                column: "TaskActivityId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskActivityId",
                table: "TaskReport",
                column: "TaskActivityId",
                principalTable: "TaskActivity",
                principalColumn: "TaskActivityId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskActivityId",
                table: "TaskReport");

            migrationBuilder.DropIndex(
                name: "IX_TaskReport_TaskActivityId",
                table: "TaskReport");

            migrationBuilder.AlterColumn<int>(
                name: "TaskActivityId",
                table: "TaskReport",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReport_TaskActivityId",
                table: "TaskReport",
                column: "TaskActivityId",
                unique: true,
                filter: "[TaskActivityId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskReport_TaskActivity_TaskActivityId",
                table: "TaskReport",
                column: "TaskActivityId",
                principalTable: "TaskActivity",
                principalColumn: "TaskActivityId");
        }
    }
}
