using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsHandoverAtDifferentAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHandoverAtDifferentAddress",
                table: "SaveChickenRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHandoverAtDifferentAddress",
                table: "SaveChickenRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
