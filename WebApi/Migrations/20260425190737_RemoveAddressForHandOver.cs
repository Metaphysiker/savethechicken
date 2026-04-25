using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAddressForHandOver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_Addresses_AddressForHandOverId",
                table: "SaveChickenRequests");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_AddressForHandOverId",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "AddressForHandOverId",
                table: "SaveChickenRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressForHandOverId",
                table: "SaveChickenRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_AddressForHandOverId",
                table: "SaveChickenRequests",
                column: "AddressForHandOverId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveChickenRequests_Addresses_AddressForHandOverId",
                table: "SaveChickenRequests",
                column: "AddressForHandOverId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }
    }
}
