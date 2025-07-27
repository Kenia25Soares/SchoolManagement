using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Web.Migrations
{
    public partial class FixAlertCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
        name: "FK_Alerts_AspNetUsers_CreatedById",
        table: "Alerts");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_AspNetUsers_CreatedById",
                table: "Alerts",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
        name: "FK_Alerts_AspNetUsers_CreatedById",
        table: "Alerts");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_AspNetUsers_CreatedById",
                table: "Alerts",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
