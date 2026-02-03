using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addPortadaUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PortadaUrl",
                table: "PhotoEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PortadaUrl",
                table: "PhotoEvents");
        }
    }
}
