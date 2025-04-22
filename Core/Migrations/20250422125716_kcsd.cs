using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class kcsd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversation_User_UserId",
                table: "Conversation");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Conversation",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Conversation_UserId",
                table: "Conversation",
                newName: "IX_Conversation_OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversation_Organization_OrganizationId",
                table: "Conversation",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversation_Organization_OrganizationId",
                table: "Conversation");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Conversation",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Conversation_OrganizationId",
                table: "Conversation",
                newName: "IX_Conversation_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversation_User_UserId",
                table: "Conversation",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
