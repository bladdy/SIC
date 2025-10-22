using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addInvitationEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvitationEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvitationId = table.Column<int>(type: "int", nullable: false),
                    AdultsEntered = table.Column<int>(type: "int", nullable: false),
                    ChildrenEntered = table.Column<int>(type: "int", nullable: false),
                    EntryDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitationEntries_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvitationEntries_Code",
                table: "InvitationEntries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvitationEntries_InvitationId",
                table: "InvitationEntries",
                column: "InvitationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvitationEntries");
        }
    }
}
