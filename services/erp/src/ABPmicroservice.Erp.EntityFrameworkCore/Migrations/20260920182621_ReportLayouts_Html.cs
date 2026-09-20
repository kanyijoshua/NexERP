using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class ReportLayouts_Html : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The layouts seeded before this migration were names only: they carried no template
            // and named formats nothing can render. They are cleared rather than converted, so
            // every report falls back to the built-in layout until someone uploads a real one.
            migrationBuilder.Sql("""
                DELETE FROM "ErpReportLayoutSelections";
                DELETE FROM "ErpCustomReportLayouts";
                """);

            // PostgreSQL will not cast text to integer on its own, so the enum columns are
            // converted explicitly. The tables are empty by now, but the cast still has to be named.
            migrationBuilder.Sql("""
                ALTER TABLE "ErpReportLayoutSelections"
                    ALTER COLUMN "LayoutType" TYPE integer USING 0,
                    ALTER COLUMN "LayoutType" SET DEFAULT 0,
                    ALTER COLUMN "LayoutType" SET NOT NULL;

                ALTER TABLE "ErpCustomReportLayouts"
                    ALTER COLUMN "LayoutType" TYPE integer USING 0,
                    ALTER COLUMN "LayoutType" SET DEFAULT 0,
                    ALTER COLUMN "LayoutType" SET NOT NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ReportName",
                table: "ErpReportLayoutSelections",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TemplateContent",
                table: "ErpCustomReportLayouts",
                type: "character varying(200000)",
                maxLength: 200000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReportName",
                table: "ErpCustomReportLayouts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LayoutName",
                table: "ErpCustomReportLayouts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ErpCustomReportLayouts",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpReportLayoutSelections_TenantId_CompanyId_ReportName",
                table: "ErpReportLayoutSelections",
                columns: new[] { "TenantId", "CompanyId", "ReportName" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCustomReportLayouts_CompanyId_ReportName",
                table: "ErpCustomReportLayouts",
                columns: new[] { "CompanyId", "ReportName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErpReportLayoutSelections_TenantId_CompanyId_ReportName",
                table: "ErpReportLayoutSelections");

            migrationBuilder.DropIndex(
                name: "IX_ErpCustomReportLayouts_CompanyId_ReportName",
                table: "ErpCustomReportLayouts");

            migrationBuilder.AlterColumn<string>(
                name: "ReportName",
                table: "ErpReportLayoutSelections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LayoutType",
                table: "ErpReportLayoutSelections",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "TemplateContent",
                table: "ErpCustomReportLayouts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200000)",
                oldMaxLength: 200000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReportName",
                table: "ErpCustomReportLayouts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LayoutType",
                table: "ErpCustomReportLayouts",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "LayoutName",
                table: "ErpCustomReportLayouts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ErpCustomReportLayouts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}
