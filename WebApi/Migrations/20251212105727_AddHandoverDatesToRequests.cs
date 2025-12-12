using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHandoverDatesToRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateForHandOver",
                table: "SaveChickenRequests");

            migrationBuilder.AddColumn<DateOnly[]>(
                name: "DateForHandOver",
                table: "SaveChickenRequests",
                type: "date[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateForHandOver",
                table: "SaveChickenRequests");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateForHandOver",
                table: "SaveChickenRequests",
                type: "date",
                nullable: true);
        }
    }
}
