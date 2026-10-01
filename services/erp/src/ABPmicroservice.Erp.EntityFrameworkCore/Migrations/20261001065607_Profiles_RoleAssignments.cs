using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Profiles_RoleAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProfileId",
                table: "ErpUserProfiles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ErpProfileRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProfileId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpProfileRoleAssignments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpUserProfiles_TenantId_UserId",
                table: "ErpUserProfiles",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpProfileRoleAssignments_TenantId_RoleName",
                table: "ErpProfileRoleAssignments",
                columns: new[] { "TenantId", "RoleName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpProfileRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ErpUserProfiles_TenantId_UserId",
                table: "ErpUserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileId",
                table: "ErpUserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
