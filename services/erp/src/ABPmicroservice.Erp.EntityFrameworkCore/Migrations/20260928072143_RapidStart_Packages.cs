using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class RapidStart_Packages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "TableName",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ErpConfigPackageFields");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "ErpConfigPackageTables",
                newName: "ProcessingOrder");

            migrationBuilder.RenameColumn(
                name: "FieldId",
                table: "ErpConfigPackageFields",
                newName: "ProcessingOrder");

            migrationBuilder.AddColumn<string>(
                name: "DataTemplateCode",
                table: "ErpConfigPackageTables",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeleteRecordsBeforeProcessing",
                table: "ErpConfigPackageTables",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "ErpConfigPackageTables",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Filters",
                table: "ErpConfigPackageTables",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfErrors",
                table: "ErpConfigPackageTables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ProductVersion",
                table: "ErpConfigPackages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PackageName",
                table: "ErpConfigPackages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ErpConfigPackages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "ErpConfigPackages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAppliedTime",
                table: "ErpConfigPackages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastImportedTime",
                table: "ErpConfigPackages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FieldName",
                table: "ErpConfigPackageFields",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mappings",
                table: "ErpConfigPackageFields",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ValidateField",
                table: "ErpConfigPackageFields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ErpConfigLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LineType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PackageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResponsibleUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_ErpConfigLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpConfigPackageErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigPackageTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigPackageRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordNo = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpConfigPackageErrors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpConfigPackageRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigPackageTableId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordNo = table.Column<int>(type: "integer", nullable: false),
                    Values = table.Column<string>(type: "text", nullable: false),
                    Invalid = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpConfigPackageRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpConfigTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_ErpConfigTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpConfigTemplateLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Mandatory = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpConfigTemplateLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpConfigTemplateLines_ErpConfigTemplates_ConfigTemplateId",
                        column: x => x.ConfigTemplateId,
                        principalTable: "ErpConfigTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfigPackages_TenantId_CompanyId_Code",
                table: "ErpConfigPackages",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfigPackageErrors_ConfigPackageId_ConfigPackageTableId",
                table: "ErpConfigPackageErrors",
                columns: new[] { "ConfigPackageId", "ConfigPackageTableId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfigPackageErrors_ConfigPackageRecordId",
                table: "ErpConfigPackageErrors",
                column: "ConfigPackageRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfigPackageRecords_ConfigPackageId_ConfigPackageTableI~",
                table: "ErpConfigPackageRecords",
                columns: new[] { "ConfigPackageId", "ConfigPackageTableId", "RecordNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfigTemplateLines_ConfigTemplateId",
                table: "ErpConfigTemplateLines",
                column: "ConfigTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfigTemplates_TenantId_CompanyId_Code",
                table: "ErpConfigTemplates",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpConfigLines");

            migrationBuilder.DropTable(
                name: "ErpConfigPackageErrors");

            migrationBuilder.DropTable(
                name: "ErpConfigPackageRecords");

            migrationBuilder.DropTable(
                name: "ErpConfigTemplateLines");

            migrationBuilder.DropTable(
                name: "ErpConfigTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ErpConfigPackages_TenantId_CompanyId_Code",
                table: "ErpConfigPackages");

            migrationBuilder.DropColumn(
                name: "DataTemplateCode",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "DeleteRecordsBeforeProcessing",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "EntityName",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "Filters",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "NoOfErrors",
                table: "ErpConfigPackageTables");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ErpConfigPackages");

            migrationBuilder.DropColumn(
                name: "LastAppliedTime",
                table: "ErpConfigPackages");

            migrationBuilder.DropColumn(
                name: "LastImportedTime",
                table: "ErpConfigPackages");

            migrationBuilder.DropColumn(
                name: "Mappings",
                table: "ErpConfigPackageFields");

            migrationBuilder.DropColumn(
                name: "ValidateField",
                table: "ErpConfigPackageFields");

            migrationBuilder.RenameColumn(
                name: "ProcessingOrder",
                table: "ErpConfigPackageTables",
                newName: "TableId");

            migrationBuilder.RenameColumn(
                name: "ProcessingOrder",
                table: "ErpConfigPackageFields",
                newName: "FieldId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "ErpConfigPackageTables",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "ErpConfigPackageTables",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "ErpConfigPackageTables",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "ErpConfigPackageTables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ErpConfigPackageTables",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "ErpConfigPackageTables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "ErpConfigPackageTables",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "ErpConfigPackageTables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ErpConfigPackageTables",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductVersion",
                table: "ErpConfigPackages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PackageName",
                table: "ErpConfigPackages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ErpConfigPackages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FieldName",
                table: "ErpConfigPackageFields",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "ErpConfigPackageFields",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "ErpConfigPackageFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "ErpConfigPackageFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "ErpConfigPackageFields",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ErpConfigPackageFields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "ErpConfigPackageFields",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "ErpConfigPackageFields",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ErpConfigPackageFields",
                type: "uuid",
                nullable: true);
        }
    }
}
