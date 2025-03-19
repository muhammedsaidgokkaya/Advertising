using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class mjaki : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "TaskLog",
                newName: "TaskTemplateTaskId");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "TaskLog",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "TaskTemplateId",
                table: "TaskLog",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskTemplateId",
                table: "TaskLog");

            migrationBuilder.RenameColumn(
                name: "TaskTemplateTaskId",
                table: "TaskLog",
                newName: "TemplateId");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "TaskLog",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
