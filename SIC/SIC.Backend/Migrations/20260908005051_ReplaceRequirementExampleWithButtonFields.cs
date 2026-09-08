using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceRequirementExampleWithButtonFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExampleUrl",
                table: "EventRequirements",
                newName: "ButtonUrl");

            migrationBuilder.AddColumn<string>(
                name: "ButtonName",
                table: "EventRequirements",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ButtonName",
                table: "EventRequirements");

            migrationBuilder.RenameColumn(
                name: "ButtonUrl",
                table: "EventRequirements",
                newName: "ExampleUrl");
        }
    }
}
