using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSaveChickenActionFarms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaveChickenActionFarms",
                columns: table => new
                {
                    FarmId = table.Column<int>(type: "integer", nullable: false),
                    SaveChickenActionId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumberOfChickensToBeSaved = table.Column<int>(type: "integer", nullable: false),
                    NumberOfRoostersToBeSaved = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveChickenActionFarms", x => new { x.SaveChickenActionId, x.FarmId });
                    table.ForeignKey(
                        name: "FK_SaveChickenActionFarms_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaveChickenActionFarms_SaveChickenActions_SaveChickenAction~",
                        column: x => x.SaveChickenActionId,
                        principalTable: "SaveChickenActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenActionFarms_FarmId",
                table: "SaveChickenActionFarms",
                column: "FarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaveChickenActionFarms");
        }
    }
}
