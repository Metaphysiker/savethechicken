using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceToSaveChickenAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SaveChickenActionId",
                table: "Farms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaveChickenActionId",
                table: "Drivers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farms_SaveChickenActionId",
                table: "Farms",
                column: "SaveChickenActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_SaveChickenActionId",
                table: "Drivers",
                column: "SaveChickenActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_SaveChickenActions_SaveChickenActionId",
                table: "Drivers",
                column: "SaveChickenActionId",
                principalTable: "SaveChickenActions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Farms_SaveChickenActions_SaveChickenActionId",
                table: "Farms",
                column: "SaveChickenActionId",
                principalTable: "SaveChickenActions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_SaveChickenActions_SaveChickenActionId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Farms_SaveChickenActions_SaveChickenActionId",
                table: "Farms");

            migrationBuilder.DropIndex(
                name: "IX_Farms_SaveChickenActionId",
                table: "Farms");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_SaveChickenActionId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "SaveChickenActionId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "SaveChickenActionId",
                table: "Drivers");
        }
    }
}
