using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteToFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Farms_FarmId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_SaveChickenRequests_SaveChickenRequestId",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Farms_FarmId",
                table: "Files",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_SaveChickenRequests_SaveChickenRequestId",
                table: "Files",
                column: "SaveChickenRequestId",
                principalTable: "SaveChickenRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Farms_FarmId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_SaveChickenRequests_SaveChickenRequestId",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Farms_FarmId",
                table: "Files",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_SaveChickenRequests_SaveChickenRequestId",
                table: "Files",
                column: "SaveChickenRequestId",
                principalTable: "SaveChickenRequests",
                principalColumn: "Id");
        }
    }
}
