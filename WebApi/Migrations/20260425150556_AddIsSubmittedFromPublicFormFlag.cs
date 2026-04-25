using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSubmittedFromPublicFormFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubmittedFromPublicForm",
                table: "SaveChickenRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubmittedFromPublicForm",
                table: "SaveChickenDriveRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubmittedFromPublicForm",
                table: "Farms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubmittedFromPublicForm",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "IsSubmittedFromPublicForm",
                table: "SaveChickenDriveRequests");

            migrationBuilder.DropColumn(
                name: "IsSubmittedFromPublicForm",
                table: "Farms");
        }
    }
}
