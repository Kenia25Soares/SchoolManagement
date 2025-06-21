using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeTypeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_StudentClasses_StudentClassId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "GradeTypeId",
                table: "StudentGrades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GradeType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGrades_GradeTypeId",
                table: "StudentGrades",
                column: "GradeTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_StudentClasses_StudentClassId",
                table: "AspNetUsers",
                column: "StudentClassId",
                principalTable: "StudentClasses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGrades_GradeType_GradeTypeId",
                table: "StudentGrades",
                column: "GradeTypeId",
                principalTable: "GradeType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_StudentClasses_StudentClassId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGrades_GradeType_GradeTypeId",
                table: "StudentGrades");

            migrationBuilder.DropTable(
                name: "GradeType");

            migrationBuilder.DropIndex(
                name: "IX_StudentGrades_GradeTypeId",
                table: "StudentGrades");

            migrationBuilder.DropColumn(
                name: "GradeTypeId",
                table: "StudentGrades");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_StudentClasses_StudentClassId",
                table: "AspNetUsers",
                column: "StudentClassId",
                principalTable: "StudentClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
