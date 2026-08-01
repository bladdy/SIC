using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIC.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addEventTypeRequirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Section = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    Placeholder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    MinImages = table.Column<int>(type: "int", nullable: false),
                    MaxImages = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRequirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventRequirementAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRequirementAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRequirementAnswers_EventRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "EventRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventRequirementAnswers_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventTypeRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypeRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTypeRequirements_EventRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "EventRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventTypeRequirements_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventRequirementImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementAnswerId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OriginalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRequirementImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRequirementImages_EventRequirementAnswers_RequirementAnswerId",
                        column: x => x.RequirementAnswerId,
                        principalTable: "EventRequirementAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRequirementAnswers_EventId_RequirementId",
                table: "EventRequirementAnswers",
                columns: new[] { "EventId", "RequirementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRequirementAnswers_RequirementId",
                table: "EventRequirementAnswers",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRequirementImages_RequirementAnswerId",
                table: "EventRequirementImages",
                column: "RequirementAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypeRequirements_EventTypeId_RequirementId",
                table: "EventTypeRequirements",
                columns: new[] { "EventTypeId", "RequirementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTypeRequirements_RequirementId",
                table: "EventTypeRequirements",
                column: "RequirementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRequirementImages");

            migrationBuilder.DropTable(
                name: "EventTypeRequirements");

            migrationBuilder.DropTable(
                name: "EventRequirementAnswers");

            migrationBuilder.DropTable(
                name: "EventRequirements");
        }
    }
}
