using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Pension_Setup_Per_Scheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErpPensionSetups_TenantId_CompanyId",
                table: "ErpPensionSetups");

            migrationBuilder.AddColumn<string>(
                name: "SchemeCode",
                table: "ErpPensionSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionSetups_TenantId_CompanyId_SchemeCode",
                table: "ErpPensionSetups",
                columns: new[] { "TenantId", "CompanyId", "SchemeCode" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErpPensionSetups_TenantId_CompanyId_SchemeCode",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "SchemeCode",
                table: "ErpPensionSetups");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionSetups_TenantId_CompanyId",
                table: "ErpPensionSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }
    }
}
