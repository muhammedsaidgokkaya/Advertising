using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class maks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeveloperToken",
                table: "GoogleApp",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Type",
                table: "GoogleApp",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Type",
                table: "GoogleAccessToken",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeveloperToken",
                table: "GoogleApp");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "GoogleApp");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "GoogleAccessToken");
        }
    }
}
