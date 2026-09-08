using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class CascadeRequirementImageOptionsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequirementImageOptions_EventRequirements_RequirementId",
                table: "RequirementImageOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementImageOptions_EventRequirements_RequirementId",
                table: "RequirementImageOptions",
                column: "RequirementId",
                principalTable: "EventRequirements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequirementImageOptions_EventRequirements_RequirementId",
                table: "RequirementImageOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementImageOptions_EventRequirements_RequirementId",
                table: "RequirementImageOptions",
                column: "RequirementId",
                principalTable: "EventRequirements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
