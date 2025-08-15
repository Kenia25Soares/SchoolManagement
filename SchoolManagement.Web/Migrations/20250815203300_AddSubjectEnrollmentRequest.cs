using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectEnrollmentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubjectEnrollmentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectEnrollmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectEnrollmentRequests_AspNetUsers_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectEnrollmentRequests_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectEnrollmentRequests_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectEnrollmentRequests_ProcessedById",
                table: "SubjectEnrollmentRequests",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectEnrollmentRequests_StudentId",
                table: "SubjectEnrollmentRequests",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectEnrollmentRequests_SubjectId",
                table: "SubjectEnrollmentRequests",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectEnrollmentRequests");
        }
    }
}
