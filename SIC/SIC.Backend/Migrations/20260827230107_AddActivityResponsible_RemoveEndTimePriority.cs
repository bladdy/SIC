using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityResponsible_RemoveEndTimePriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "MbMActivities");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MbMActivities");

            migrationBuilder.AddColumn<string>(
                name: "Responsible",
                table: "MbMActivities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Responsible",
                table: "MbMActivities");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "MbMActivities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "MbMActivities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
