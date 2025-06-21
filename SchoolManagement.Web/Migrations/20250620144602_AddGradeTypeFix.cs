using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeTypeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGrades_GradeTypes_GradeTypeId",
                table: "StudentGrades");

            migrationBuilder.AlterColumn<int>(
                name: "GradeTypeId",
                table: "StudentGrades",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGrades_GradeTypes_GradeTypeId",
                table: "StudentGrades",
                column: "GradeTypeId",
                principalTable: "GradeTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGrades_GradeTypes_GradeTypeId",
                table: "StudentGrades");

            migrationBuilder.AlterColumn<int>(
                name: "GradeTypeId",
                table: "StudentGrades",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGrades_GradeTypes_GradeTypeId",
                table: "StudentGrades",
                column: "GradeTypeId",
                principalTable: "GradeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
