using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "InvitationEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvitationEntries_EventId",
                table: "InvitationEntries",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationEntries_Events_EventId",
                table: "InvitationEntries",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvitationEntries_Events_EventId",
                table: "InvitationEntries");

            migrationBuilder.DropIndex(
                name: "IX_InvitationEntries_EventId",
                table: "InvitationEntries");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "InvitationEntries");
        }
    }
}
