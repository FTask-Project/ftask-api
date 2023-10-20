using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FTask.Repository.Migrations
{
    public partial class AddFileNamepropforattachmentandevidenceentities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Evidence",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Attachment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Attachment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Attachment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Evidence");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Attachment");
        }
    }
}
