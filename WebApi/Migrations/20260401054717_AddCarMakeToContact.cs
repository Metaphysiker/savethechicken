using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCarMakeToContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarMake",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Contacts",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '') || ' ' ||\r\n                                coalesce(\"CarMake\", '')\r\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '')\r\n                            )",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarMake",
                table: "Contacts");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Contacts",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '')\r\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '') || ' ' ||\r\n                                coalesce(\"CarMake\", '')\r\n                            )",
                oldStored: true);
        }
    }
}
