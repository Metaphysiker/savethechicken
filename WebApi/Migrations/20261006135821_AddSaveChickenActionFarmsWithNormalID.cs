using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSaveChickenActionFarmsWithNormalID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveChickenActionFarms",
                table: "SaveChickenActionFarms");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SaveChickenActionFarms",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveChickenActionFarms",
                table: "SaveChickenActionFarms",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SaveChickenActionFarms_SaveChickenActionId_FarmId",
                table: "SaveChickenActionFarms",
                columns: new[] { "SaveChickenActionId", "FarmId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveChickenActionFarms",
                table: "SaveChickenActionFarms");

            migrationBuilder.DropIndex(
                name: "IX_SaveChickenActionFarms_SaveChickenActionId_FarmId",
                table: "SaveChickenActionFarms");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SaveChickenActionFarms",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveChickenActionFarms",
                table: "SaveChickenActionFarms",
                columns: new[] { "SaveChickenActionId", "FarmId" });
        }
    }
}
