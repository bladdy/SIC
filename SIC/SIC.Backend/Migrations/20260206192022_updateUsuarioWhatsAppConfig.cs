using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class updateUsuarioWhatsAppConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WabaId",
                table: "UsuarioWhatsAppConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessId",
                table: "UsuarioWhatsAppConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UsuarioWhatsAppConfigs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UsuarioWhatsAppConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "UsuarioWhatsAppConfigs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemUserId",
                table: "UsuarioWhatsAppConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.DropColumn(
                name: "SystemUserId",
                table: "UsuarioWhatsAppConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "WabaId",
                table: "UsuarioWhatsAppConfigs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
