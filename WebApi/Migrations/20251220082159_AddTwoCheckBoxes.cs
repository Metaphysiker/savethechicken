using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoCheckBoxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlreadyReceivedChickenPreviously",
                table: "SaveChickenRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmThatIFulfillCriteria",
                table: "SaveChickenRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlreadyReceivedChickenPreviously",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "ConfirmThatIFulfillCriteria",
                table: "SaveChickenRequests");
        }
    }
}
