using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Core.Migrations
{
    public partial class mksalaas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardNumber = table.Column<string>(type: "text", nullable: false),
                    CardHolder = table.Column<string>(type: "text", nullable: false),
                    Cvv = table.Column<string>(type: "text", nullable: false),
                    ExpirationDate = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    _CardNumber = table.Column<string>(type: "text", nullable: false),
                    _CardHolder = table.Column<string>(type: "text", nullable: false),
                    _Cvv = table.Column<string>(type: "text", nullable: false),
                    _ExpirationDate = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    IsYearly = table.Column<bool>(type: "boolean", nullable: false),
                    IsPayment = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plan_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrganizationId",
                table: "Payment",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_OrganizationId",
                table: "Plan",
                column: "OrganizationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Plan");
        }
    }
}
