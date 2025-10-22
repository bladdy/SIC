using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addInvitationEntryRelaiontoInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvitationEntries_InvitationId",
                table: "InvitationEntries");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationEntries_InvitationId",
                table: "InvitationEntries",
                column: "InvitationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvitationEntries_InvitationId",
                table: "InvitationEntries");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationEntries_InvitationId",
                table: "InvitationEntries",
                column: "InvitationId");
        }
    }
}
