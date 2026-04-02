using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFarmDatesForRescues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatesForRescues",
                table: "Farms");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "SaveChickenRequests",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\r\n                        coalesce(\"Message\", '') || ' ' ||\r\n                        coalesce(\"Color\", '')\r\n                    )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\n                        coalesce(\"Message\", '') || ' ' ||\n                        coalesce(\"Color\", '')\n                    )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "SaveChickenDriveRequests",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                        coalesce(\"CarMake\", '') || ' ' ||\r\n                        coalesce(\"Message\", '')\r\n                    )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\n                        coalesce(\"CarMake\", '') || ' ' ||\n                        coalesce(\"Message\", '')\n                    )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Farms",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"Size\", '') || ' ' ||\r\n                                coalesce(\"Color\", '') || ' ' ||\r\n                                coalesce(\"GeneralInformation\", '') || ' ' ||\r\n                                coalesce(\"Name\", '')\r\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\n                                coalesce(\"Size\", '') || ' ' ||\n                                coalesce(\"Color\", '') || ' ' ||\n                                coalesce(\"GeneralInformation\", '') || ' ' ||\n                                coalesce(\"Name\", '')\n                            )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Contacts",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '') || ' ' ||\r\n                                coalesce(\"CarMake\", '')\r\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\n                                coalesce(\"FirstName\", '') || ' ' ||\n                                coalesce(\"LastName\", '') || ' ' ||\n                                coalesce(\"Email\", '') || ' ' ||\n                                coalesce(\"PhoneNumber\", '') || ' ' ||\n                                coalesce(\"CarMake\", '')\n                            )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Addresses",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"Street\", '') || ' ' ||\r\n                                coalesce(\"City\", '') || ' ' ||\r\n                                coalesce(\"PostalCode\", '')\r\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\n                                coalesce(\"Street\", '') || ' ' ||\n                                coalesce(\"City\", '') || ' ' ||\n                                coalesce(\"PostalCode\", '')\n                            )",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<DateOnly>>(
                name: "DatesForRescues",
                table: "Farms",
                type: "date[]",
                nullable: false);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "SaveChickenRequests",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\n                        coalesce(\"Message\", '') || ' ' ||\n                        coalesce(\"Color\", '')\n                    )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\r\n                        coalesce(\"Message\", '') || ' ' ||\r\n                        coalesce(\"Color\", '')\r\n                    )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "SaveChickenDriveRequests",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\n                        coalesce(\"CarMake\", '') || ' ' ||\n                        coalesce(\"Message\", '')\n                    )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                        coalesce(\"CarMake\", '') || ' ' ||\r\n                        coalesce(\"Message\", '')\r\n                    )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Farms",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\n                                coalesce(\"Size\", '') || ' ' ||\n                                coalesce(\"Color\", '') || ' ' ||\n                                coalesce(\"GeneralInformation\", '') || ' ' ||\n                                coalesce(\"Name\", '')\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"Size\", '') || ' ' ||\r\n                                coalesce(\"Color\", '') || ' ' ||\r\n                                coalesce(\"GeneralInformation\", '') || ' ' ||\r\n                                coalesce(\"Name\", '')\r\n                            )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Contacts",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\n                                coalesce(\"FirstName\", '') || ' ' ||\n                                coalesce(\"LastName\", '') || ' ' ||\n                                coalesce(\"Email\", '') || ' ' ||\n                                coalesce(\"PhoneNumber\", '') || ' ' ||\n                                coalesce(\"CarMake\", '')\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '') || ' ' ||\r\n                                coalesce(\"CarMake\", '')\r\n                            )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Addresses",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\n                                coalesce(\"Street\", '') || ' ' ||\n                                coalesce(\"City\", '') || ' ' ||\n                                coalesce(\"PostalCode\", '')\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"Street\", '') || ' ' ||\r\n                                coalesce(\"City\", '') || ' ' ||\r\n                                coalesce(\"PostalCode\", '')\r\n                            )",
                oldStored: true);
        }
    }
}
