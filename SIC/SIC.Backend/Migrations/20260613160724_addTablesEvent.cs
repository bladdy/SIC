using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addTablesEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TablesEventsId",
                table: "Invitations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TablesEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Seats = table.Column<int>(type: "int", nullable: false),
                    OccupiedSeats = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TablesEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TablesEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TablesEventsId",
                table: "Invitations",
                column: "TablesEventsId");

            migrationBuilder.CreateIndex(
                name: "IX_TablesEvents_EventId",
                table: "TablesEvents",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_TablesEvents_TablesEventsId",
                table: "Invitations",
                column: "TablesEventsId",
                principalTable: "TablesEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_TablesEvents_TablesEventsId",
                table: "Invitations");

            migrationBuilder.DropTable(
                name: "TablesEvents");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_TablesEventsId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "TablesEventsId",
                table: "Invitations");
        }
    }
}
