using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class EventFormGuestResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventFormResponses_EventId_InvitationId",
                table: "EventFormResponses");

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "EventFormResponses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationGuestId",
                table: "EventFormResponses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventFormResponses_EventId_InvitationGuestId",
                table: "EventFormResponses",
                columns: new[] { "EventId", "InvitationGuestId" },
                unique: true,
                filter: "[InvitationGuestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventFormResponses_InvitationGuestId",
                table: "EventFormResponses",
                column: "InvitationGuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventFormResponses_InvitationGuest_InvitationGuestId",
                table: "EventFormResponses",
                column: "InvitationGuestId",
                principalTable: "InvitationGuest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFormResponses_InvitationGuest_InvitationGuestId",
                table: "EventFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_EventFormResponses_EventId_InvitationGuestId",
                table: "EventFormResponses");

            migrationBuilder.DropIndex(
                name: "IX_EventFormResponses_InvitationGuestId",
                table: "EventFormResponses");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "EventFormResponses");

            migrationBuilder.DropColumn(
                name: "InvitationGuestId",
                table: "EventFormResponses");

            migrationBuilder.CreateIndex(
                name: "IX_EventFormResponses_EventId_InvitationId",
                table: "EventFormResponses",
                columns: new[] { "EventId", "InvitationId" },
                unique: true);
        }
    }
}
