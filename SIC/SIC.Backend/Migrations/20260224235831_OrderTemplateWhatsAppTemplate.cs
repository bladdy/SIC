using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class OrderTemplateWhatsAppTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderTemplate",
                table: "WhatsAppTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderTemplate",
                table: "WhatsAppTemplates");
        }
    }
}
