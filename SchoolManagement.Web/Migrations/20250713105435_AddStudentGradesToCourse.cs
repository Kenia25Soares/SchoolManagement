using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentGradesToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "StudentGrades",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGrades_CourseId1",
                table: "StudentGrades",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGrades_Courses_CourseId1",
                table: "StudentGrades",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGrades_Courses_CourseId1",
                table: "StudentGrades");

            migrationBuilder.DropIndex(
                name: "IX_StudentGrades_CourseId1",
                table: "StudentGrades");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "StudentGrades");
        }
    }
}
