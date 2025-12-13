using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatesHandOver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DatesForHandOver",
                table: "SaveChickenRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<DateOnly>),
                oldType: "date[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<DateOnly>>(
                name: "DatesForHandOver",
                table: "SaveChickenRequests",
                type: "date[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
