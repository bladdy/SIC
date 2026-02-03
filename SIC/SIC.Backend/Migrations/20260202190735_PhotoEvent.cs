using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class PhotoEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhotoEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhotoEventImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhotoEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoEventImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhotoEventImages_PhotoEvents_PhotoEventId",
                        column: x => x.PhotoEventId,
                        principalTable: "PhotoEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhotoEventImages_PhotoEventId",
                table: "PhotoEventImages",
                column: "PhotoEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotoEventImages");

            migrationBuilder.DropTable(
                name: "PhotoEvents");
        }
    }
}
