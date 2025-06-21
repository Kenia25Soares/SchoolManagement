using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeTypeEntityFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGrades_GradeType_GradeTypeId",
                table: "StudentGrades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GradeType",
                table: "GradeType");

            migrationBuilder.RenameTable(
                name: "GradeType",
                newName: "GradeTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GradeTypes",
                table: "GradeTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGrades_GradeTypes_GradeTypeId",
                table: "StudentGrades",
                column: "GradeTypeId",
                principalTable: "GradeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGrades_GradeTypes_GradeTypeId",
                table: "StudentGrades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GradeTypes",
                table: "GradeTypes");

            migrationBuilder.RenameTable(
                name: "GradeTypes",
                newName: "GradeType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GradeType",
                table: "GradeType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGrades_GradeType_GradeTypeId",
                table: "StudentGrades",
                column: "GradeTypeId",
                principalTable: "GradeType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
