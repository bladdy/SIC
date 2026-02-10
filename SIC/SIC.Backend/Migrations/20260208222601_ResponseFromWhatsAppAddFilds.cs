using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ResponseFromWhatsAppAddFilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventCode",
                table: "ResponseFromWhatsApps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "ResponseFromWhatsApps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameConversation",
                table: "ResponseFromWhatsApps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ResponseFromWhatsApps",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseFromWhatsApps_UserId",
                table: "ResponseFromWhatsApps",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseFromWhatsApps_AspNetUsers_UserId",
                table: "ResponseFromWhatsApps",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseFromWhatsApps_AspNetUsers_UserId",
                table: "ResponseFromWhatsApps");

            migrationBuilder.DropIndex(
                name: "IX_ResponseFromWhatsApps_UserId",
                table: "ResponseFromWhatsApps");

            migrationBuilder.DropColumn(
                name: "EventCode",
                table: "ResponseFromWhatsApps");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "ResponseFromWhatsApps");

            migrationBuilder.DropColumn(
                name: "NameConversation",
                table: "ResponseFromWhatsApps");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ResponseFromWhatsApps");
        }
    }
}
