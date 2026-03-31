using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class PersonCentricRevampAndDriverReplacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Farms_SaveChickenActions_SaveChickenActionId",
                table: "Farms");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Drivers_DriverId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_Addresses_AddressId",
                table: "SaveChickenRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_Contacts_ContactId",
                table: "SaveChickenRequests");

            migrationBuilder.DropTable(
                name: "BlackListedPersons");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_AddressId",
                table: "SaveChickenRequests");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_ContactId",
                table: "SaveChickenRequests");

            migrationBuilder.DropIndex(
                name: "IX_Farms_AddressId",
                table: "Farms");

            migrationBuilder.DropIndex(
                name: "IX_Farms_ContactId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "SaveChickenRequests");

            migrationBuilder.RenameColumn(
                name: "DriverId",
                table: "Files",
                newName: "SaveChickenDriveRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_DriverId",
                table: "Files",
                newName: "IX_Files_SaveChickenDriveRequestId");

            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "SaveChickenRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Dates",
                table: "SaveChickenActions",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<DateOnly>),
                oldType: "date[]");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "SaveChickenRequests",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\r\n                        coalesce(\"Message\", '') || ' ' ||\r\n                        coalesce(\"Color\", '')\r\n                    )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german', \n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\n                        coalesce(\"Message\", '') || ' ' ||\n                        coalesce(\"Color\", '')\n                    )",
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
                oldComputedColumnSql: "to_tsvector('german', \n                                coalesce(\"Size\", '') || ' ' ||\n                                coalesce(\"Color\", '') || ' ' ||\n                                coalesce(\"GeneralInformation\", '') || ' ' ||\n                                coalesce(\"Name\", '')\n                            )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Contacts",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '')\r\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german', \n                                coalesce(\"FirstName\", '') || ' ' ||\n                                coalesce(\"LastName\", '') || ' ' ||\n                                coalesce(\"Email\", '') || ' ' ||\n                                coalesce(\"PhoneNumber\", '')\n                            )",
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
                oldComputedColumnSql: "to_tsvector('german', \n                                coalesce(\"Street\", '') || ' ' ||\n                                coalesce(\"City\", '') || ' ' ||\n                                coalesce(\"PostalCode\", '')\n                            )",
                oldStored: true);

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    AddressId = table.Column<int>(type: "integer", nullable: false),
                    IsBlacklisted = table.Column<bool>(type: "boolean", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false, computedColumnSql: "to_tsvector('german', '')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Persons_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaveChickenDriveRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    CarMake = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CapacityForChickens = table.Column<int>(type: "integer", nullable: false),
                    AvailableDates = table.Column<string>(type: "text", nullable: false),
                    SaveChickenActionId = table.Column<int>(type: "integer", nullable: true),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false, computedColumnSql: "to_tsvector('german',\r\n                        coalesce(\"CarMake\", '') || ' ' ||\r\n                        coalesce(\"Message\", '')\r\n                    )", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveChickenDriveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaveChickenDriveRequests_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaveChickenDriveRequests_SaveChickenActions_SaveChickenActi~",
                        column: x => x.SaveChickenActionId,
                        principalTable: "SaveChickenActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_PersonId",
                table: "SaveChickenRequests",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Farms_AddressId",
                table: "Farms",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farms_ContactId",
                table: "Farms",
                column: "ContactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_AddressId",
                table: "Persons",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_ContactId",
                table: "Persons",
                column: "ContactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_SearchVector",
                table: "Persons",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenDriveRequests_PersonId",
                table: "SaveChickenDriveRequests",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenDriveRequests_SaveChickenActionId",
                table: "SaveChickenDriveRequests",
                column: "SaveChickenActionId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenDriveRequests_SearchVector",
                table: "SaveChickenDriveRequests",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.AddForeignKey(
                name: "FK_Farms_SaveChickenActions_SaveChickenActionId",
                table: "Farms",
                column: "SaveChickenActionId",
                principalTable: "SaveChickenActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_SaveChickenDriveRequests_SaveChickenDriveRequestId",
                table: "Files",
                column: "SaveChickenDriveRequestId",
                principalTable: "SaveChickenDriveRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveChickenRequests_Persons_PersonId",
                table: "SaveChickenRequests",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Farms_SaveChickenActions_SaveChickenActionId",
                table: "Farms");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_SaveChickenDriveRequests_SaveChickenDriveRequestId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_Persons_PersonId",
                table: "SaveChickenRequests");

            migrationBuilder.DropTable(
                name: "SaveChickenDriveRequests");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_PersonId",
                table: "SaveChickenRequests");

            migrationBuilder.DropIndex(
                name: "IX_Farms_AddressId",
                table: "Farms");

            migrationBuilder.DropIndex(
                name: "IX_Farms_ContactId",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "SaveChickenRequests");

            migrationBuilder.RenameColumn(
                name: "SaveChickenDriveRequestId",
                table: "Files",
                newName: "DriverId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_SaveChickenDriveRequestId",
                table: "Files",
                newName: "IX_Files_DriverId");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "SaveChickenRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "SaveChickenRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<List<DateOnly>>(
                name: "Dates",
                table: "SaveChickenActions",
                type: "date[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "SaveChickenRequests",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\n                        coalesce(\"Message\", '') || ' ' ||\n                        coalesce(\"Color\", '')\n                    )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                        coalesce(\"DescriptionOfPlaceForChickens\", '') || ' ' ||\r\n                        coalesce(\"Message\", '') || ' ' ||\r\n                        coalesce(\"Color\", '')\r\n                    )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Farms",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \n                                coalesce(\"Size\", '') || ' ' ||\n                                coalesce(\"Color\", '') || ' ' ||\n                                coalesce(\"GeneralInformation\", '') || ' ' ||\n                                coalesce(\"Name\", '')\n                            )",
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
                computedColumnSql: "to_tsvector('german', \n                                coalesce(\"FirstName\", '') || ' ' ||\n                                coalesce(\"LastName\", '') || ' ' ||\n                                coalesce(\"Email\", '') || ' ' ||\n                                coalesce(\"PhoneNumber\", '')\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"FirstName\", '') || ' ' ||\r\n                                coalesce(\"LastName\", '') || ' ' ||\r\n                                coalesce(\"Email\", '') || ' ' ||\r\n                                coalesce(\"PhoneNumber\", '')\r\n                            )",
                oldStored: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Addresses",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('german', \n                                coalesce(\"Street\", '') || ' ' ||\n                                coalesce(\"City\", '') || ' ' ||\n                                coalesce(\"PostalCode\", '')\n                            )",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('german',\r\n                                coalesce(\"Street\", '') || ' ' ||\r\n                                coalesce(\"City\", '') || ' ' ||\r\n                                coalesce(\"PostalCode\", '')\r\n                            )",
                oldStored: true);

            migrationBuilder.CreateTable(
                name: "BlackListedPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressId = table.Column<int>(type: "integer", nullable: false),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListedPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListedPersons_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlackListedPersons_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressId = table.Column<int>(type: "integer", nullable: false),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    SaveChickenActionId = table.Column<int>(type: "integer", nullable: true),
                    AvailableDates = table.Column<List<DateOnly>>(type: "date[]", nullable: false),
                    CapacityForChickens = table.Column<int>(type: "integer", nullable: false),
                    CarMake = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false, computedColumnSql: "to_tsvector('german', \n                                coalesce(\"CarMake\", '')\n                            )", stored: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drivers_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drivers_SaveChickenActions_SaveChickenActionId",
                        column: x => x.SaveChickenActionId,
                        principalTable: "SaveChickenActions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_AddressId",
                table: "SaveChickenRequests",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_ContactId",
                table: "SaveChickenRequests",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Farms_AddressId",
                table: "Farms",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Farms_ContactId",
                table: "Farms",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListedPersons_AddressId",
                table: "BlackListedPersons",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListedPersons_ContactId",
                table: "BlackListedPersons",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_AddressId",
                table: "Drivers",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_ContactId",
                table: "Drivers",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_SaveChickenActionId",
                table: "Drivers",
                column: "SaveChickenActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_SearchVector",
                table: "Drivers",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.AddForeignKey(
                name: "FK_Farms_SaveChickenActions_SaveChickenActionId",
                table: "Farms",
                column: "SaveChickenActionId",
                principalTable: "SaveChickenActions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Drivers_DriverId",
                table: "Files",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveChickenRequests_Addresses_AddressId",
                table: "SaveChickenRequests",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveChickenRequests_Contacts_ContactId",
                table: "SaveChickenRequests",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
