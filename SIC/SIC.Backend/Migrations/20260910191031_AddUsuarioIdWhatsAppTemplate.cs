using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdWhatsAppTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "WhatsAppTemplates",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplates_UsuarioId",
                table: "WhatsAppTemplates",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WhatsAppTemplates_UsuarioId",
                table: "WhatsAppTemplates");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "WhatsAppTemplates");
        }
    }
}
