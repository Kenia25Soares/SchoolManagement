using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "Alerts",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "IsResolved",
                table: "Alerts",
                newName: "IsRead");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Alerts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Alerts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Alerts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Alerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentClassId",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentGradeId",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "Alerts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_StudentClassId",
                table: "Alerts",
                column: "StudentClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_StudentGradeId",
                table: "Alerts",
                column: "StudentGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_StudentId",
                table: "Alerts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SubjectId",
                table: "Alerts",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_AspNetUsers_StudentId",
                table: "Alerts",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_StudentClasses_StudentClassId",
                table: "Alerts",
                column: "StudentClassId",
                principalTable: "StudentClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_StudentGrades_StudentGradeId",
                table: "Alerts",
                column: "StudentGradeId",
                principalTable: "StudentGrades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Subjects_SubjectId",
                table: "Alerts",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_AspNetUsers_StudentId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_StudentClasses_StudentClassId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_StudentGrades_StudentGradeId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Subjects_SubjectId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_StudentClassId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_StudentGradeId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_StudentId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_SubjectId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "StudentClassId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "StudentGradeId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Alerts",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "Alerts",
                newName: "IsResolved");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
