using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class mjaksas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Calendar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Calendar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mail",
                table: "Calendar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Calendar",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "Mail",
                table: "Calendar");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Calendar");
        }
    }
}
