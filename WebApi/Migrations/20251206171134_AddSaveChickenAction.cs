using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSaveChickenAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SaveChickenActionId",
                table: "SaveChickenRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SaveChickenActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Dates = table.Column<List<DateOnly>>(type: "date[]", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveChickenActions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenRequests_SaveChickenActionId",
                table: "SaveChickenRequests",
                column: "SaveChickenActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveChickenRequests_SaveChickenActions_SaveChickenActionId",
                table: "SaveChickenRequests",
                column: "SaveChickenActionId",
                principalTable: "SaveChickenActions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveChickenRequests_SaveChickenActions_SaveChickenActionId",
                table: "SaveChickenRequests");

            migrationBuilder.DropTable(
                name: "SaveChickenActions");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenRequests_SaveChickenActionId",
                table: "SaveChickenRequests");

            migrationBuilder.DropColumn(
                name: "SaveChickenActionId",
                table: "SaveChickenRequests");
        }
    }
}
