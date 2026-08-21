using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestTableAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TablesEventsId",
                table: "InvitationGuest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvitationGuest_TablesEventsId",
                table: "InvitationGuest",
                column: "TablesEventsId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationGuest_TablesEvents_TablesEventsId",
                table: "InvitationGuest",
                column: "TablesEventsId",
                principalTable: "TablesEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvitationGuest_TablesEvents_TablesEventsId",
                table: "InvitationGuest");

            migrationBuilder.DropIndex(
                name: "IX_InvitationGuest_TablesEventsId",
                table: "InvitationGuest");

            migrationBuilder.DropColumn(
                name: "TablesEventsId",
                table: "InvitationGuest");
        }
    }
}
