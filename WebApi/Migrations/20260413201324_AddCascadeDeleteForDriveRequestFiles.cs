using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteForDriveRequestFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_SaveChickenDriveRequests_SaveChickenDriveRequestId",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_SaveChickenDriveRequests_SaveChickenDriveRequestId",
                table: "Files",
                column: "SaveChickenDriveRequestId",
                principalTable: "SaveChickenDriveRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_SaveChickenDriveRequests_SaveChickenDriveRequestId",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_SaveChickenDriveRequests_SaveChickenDriveRequestId",
                table: "Files",
                column: "SaveChickenDriveRequestId",
                principalTable: "SaveChickenDriveRequests",
                principalColumn: "Id");
        }
    }
}
