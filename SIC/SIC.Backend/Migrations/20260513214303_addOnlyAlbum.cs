using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addOnlyAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnlyAlbum",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlyAlbum",
                table: "Events");
        }
    }
}
