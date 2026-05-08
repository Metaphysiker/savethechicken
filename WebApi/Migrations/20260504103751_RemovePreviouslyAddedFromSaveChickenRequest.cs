using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreviouslyAddedFromSaveChickenRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlreadyReceivedChickenPreviously",
                table: "SaveChickenRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlreadyReceivedChickenPreviously",
                table: "SaveChickenRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
