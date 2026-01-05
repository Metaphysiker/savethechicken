using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTsVectorToMainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarMake",
                table: "Contacts");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "SaveChickenRequests",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \r\n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\r\n                        coalesce(\"Message\", '') || ' ' ||\r\n                        coalesce(\"Color\", '')\r\n                    )",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Farms",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \r\n                                coalesce(\"Size\", '') || ' ' ||\r\n                                coalesce(\"Color\", '') || ' ' ||\r\n                                coalesce(\"GeneralInformation\", '') || ' ' ||\r\n                                coalesce(\"Name\", '')\r\n                            )",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Drivers",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \r\n                                coalesce(\"CarMake\", '')\r\n                            )",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Contacts",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '')\r\n                            )",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Addresses",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \r\n                                coalesce(\"Street\", '') || ' ' ||\r\n                                coalesce(\"City\", '') || ' ' ||\r\n                                coalesce(\"PostalCode\", '')\r\n                            )",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_SearchVector",
                table: "SaveChickenRequests",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Farms_SearchVector",
                table: "Farms",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_SearchVector",
                table: "Drivers",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_SearchVector",
                table: "Contacts",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_SearchVector",
                table: "Addresses",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_SearchVector",
                table: "SaveChickenRequests");

            migrationBuilder.DropIndex(
                name: "IX_Farms_SearchVector",
                table: "Farms");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_SearchVector",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_SearchVector",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_SearchVector",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Addresses");

            migrationBuilder.AddColumn<string>(
                name: "CarMake",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
