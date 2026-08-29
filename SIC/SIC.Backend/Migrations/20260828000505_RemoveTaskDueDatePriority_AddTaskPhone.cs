using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaskDueDatePriority_AddTaskPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "MbMTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MbMTasks");

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePhone",
                table: "MbMTasks",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsiblePhone",
                table: "MbMTasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "MbMTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "MbMTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
