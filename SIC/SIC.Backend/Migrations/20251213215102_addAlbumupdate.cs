using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addAlbumupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "EventImages");

            migrationBuilder.AddColumn<DateTime>(
                name: "PostingDate",
                table: "EventImages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostingDate",
                table: "EventImages");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "EventImages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
