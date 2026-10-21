using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class HumanResources_BaseCalendars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseCalendarCode",
                table: "ErpHumanResourcesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaseCalendarCode",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpBaseCalendarChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseCalendarCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecurringSystem = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Nonworking = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpBaseCalendarChanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBaseCalendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpBaseCalendars", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpBaseCalendarChanges_CompanyId_BaseCalendarCode",
                table: "ErpBaseCalendarChanges",
                columns: new[] { "CompanyId", "BaseCalendarCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpBaseCalendars_TenantId_CompanyId_Code",
                table: "ErpBaseCalendars",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpBaseCalendarChanges");

            migrationBuilder.DropTable(
                name: "ErpBaseCalendars");

            migrationBuilder.DropColumn(
                name: "BaseCalendarCode",
                table: "ErpHumanResourcesSetups");

            migrationBuilder.DropColumn(
                name: "BaseCalendarCode",
                table: "ErpEmployees");
        }
    }
}
