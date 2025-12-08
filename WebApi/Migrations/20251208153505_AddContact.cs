using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Street",
                table: "SaveChickenRequests",
                newName: "Contact_Street");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "SaveChickenRequests",
                newName: "Contact_PostalCode");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "SaveChickenRequests",
                newName: "Contact_PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "SaveChickenRequests",
                newName: "Contact_LastName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "SaveChickenRequests",
                newName: "Contact_FirstName");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "SaveChickenRequests",
                newName: "Contact_Email");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "SaveChickenRequests",
                newName: "Contact_City");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Contact_Street",
                table: "SaveChickenRequests",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "Contact_PostalCode",
                table: "SaveChickenRequests",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "Contact_PhoneNumber",
                table: "SaveChickenRequests",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "Contact_LastName",
                table: "SaveChickenRequests",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Contact_FirstName",
                table: "SaveChickenRequests",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Contact_Email",
                table: "SaveChickenRequests",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Contact_City",
                table: "SaveChickenRequests",
                newName: "City");
        }
    }
}
