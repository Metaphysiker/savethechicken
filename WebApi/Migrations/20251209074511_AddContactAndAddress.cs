using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddContactAndAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact_City",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "Contact_Email",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "Contact_FirstName",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "Contact_LastName",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "Contact_PhoneNumber",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "Contact_PostalCode",
                table: "SaveChickenRequests");

            migrationBuilder.RenameColumn(
                name: "Contact_Street",
                table: "SaveChickenRequests",
                newName: "Color");

            migrationBuilder.AddColumn<int>(
                name: "AddressForHandOverId",
                table: "SaveChickenRequests",
                type: "integer",
                nullable: true);

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

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateForHandOver",
                table: "SaveChickenRequests",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "IsHandoverAtDifferentAddress",
                table: "SaveChickenRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBoxes",
                table: "SaveChickenRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Categories = table.Column<string>(type: "text", nullable: false),
                    CarMake = table.Column<string>(type: "text", nullable: false),
                    AvailableDates = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumberOfChickens = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    DatesForRescues = table.Column<List<DateOnly>>(type: "date[]", nullable: false),
                    GeneralInformation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_AddressForHandOverId",
                table: "SaveChickenRequests",
                column: "AddressForHandOverId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_AddressId",
                table: "SaveChickenRequests",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_ContactId",
                table: "SaveChickenRequests",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveChickenRequests_Addresses_AddressForHandOverId",
                table: "SaveChickenRequests",
                column: "AddressForHandOverId",
                principalTable: "Addresses",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_Addresses_AddressForHandOverId",
                table: "SaveChickenRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_Addresses_AddressId",
                table: "SaveChickenRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_Contacts_ContactId",
                table: "SaveChickenRequests");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Farms");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_AddressForHandOverId",
                table: "SaveChickenRequests");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_AddressId",
                table: "SaveChickenRequests");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_ContactId",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "AddressForHandOverId",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "DateForHandOver",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "IsHandoverAtDifferentAddress",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "NumberOfBoxes",
                table: "SaveChickenRequests");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "SaveChickenRequests",
                newName: "Contact_Street");

            migrationBuilder.AddColumn<string>(
                name: "Contact_City",
                table: "SaveChickenRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contact_Email",
                table: "SaveChickenRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contact_FirstName",
                table: "SaveChickenRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contact_LastName",
                table: "SaveChickenRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contact_PhoneNumber",
                table: "SaveChickenRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contact_PostalCode",
                table: "SaveChickenRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
