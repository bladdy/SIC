using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class updateConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioWhatsAppConfigs_AspNetUsers_UsuarioId1",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioWhatsAppConfigs_UsuarioId1",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "UsuarioWhatsAppConfigs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioWhatsAppConfigs_UsuarioId",
                table: "UsuarioWhatsAppConfigs",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioWhatsAppConfigs_AspNetUsers_UsuarioId",
                table: "UsuarioWhatsAppConfigs",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioWhatsAppConfigs_AspNetUsers_UsuarioId",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioWhatsAppConfigs_UsuarioId",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "UsuarioWhatsAppConfigs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioId1",
                table: "UsuarioWhatsAppConfigs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioWhatsAppConfigs_UsuarioId1",
                table: "UsuarioWhatsAppConfigs",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioWhatsAppConfigs_AspNetUsers_UsuarioId1",
                table: "UsuarioWhatsAppConfigs",
                column: "UsuarioId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
