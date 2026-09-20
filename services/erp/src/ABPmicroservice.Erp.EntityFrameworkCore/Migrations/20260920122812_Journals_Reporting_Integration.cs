using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Journals_Reporting_Integration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RegisterNo",
                table: "ErpVendorLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Reversed",
                table: "ErpVendorLedgerEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ReversedByEntryNo",
                table: "ErpVendorLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ReversedEntryNo",
                table: "ErpVendorLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TransactionNo",
                table: "ErpVendorLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Web services become company data and gain a required company id. Rows written before
            // that belonged to no company, so there is no company to assign them to; the table is
            // cleared and services are published again per company.
            migrationBuilder.Sql("""DELETE FROM "ErpPublishedWebServices";""");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "ErpPublishedWebServices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            // PostgreSQL will not cast text to integer on its own, so the option values are
            // mapped across explicitly. Scaffolding produced a plain AlterColumn, which fails.
            migrationBuilder.Sql(
                """
                ALTER TABLE "ErpPublishedWebServices"
                    ALTER COLUMN "ObjectType" TYPE integer USING (
                        CASE "ObjectType" WHEN 'Query' THEN 1 ELSE 0 END),
                    ALTER COLUMN "ObjectType" SET DEFAULT 0,
                    ALTER COLUMN "ObjectType" SET NOT NULL;
                """
            );

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "ErpPublishedWebServices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "ErpPublishedWebServices",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "ErpPublishedWebServices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExcludedFields",
                table: "ErpPublishedWebServices",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "ErpPublishedWebServices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DimensionSetId",
                table: "ErpGLEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentDate",
                table: "ErpGLEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReasonCode",
                table: "ErpGLEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RegisterNo",
                table: "ErpGLEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Reversed",
                table: "ErpGLEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ReversedByEntryNo",
                table: "ErpGLEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ReversedEntryNo",
                table: "ErpGLEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                table: "ErpGLEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TransactionNo",
                table: "ErpGLEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // The account type on a journal line becomes an enum. The BC captions it used to hold
            // are mapped across; anything unrecognised falls back to a G/L account, which is what
            // a blank account type meant before.
            migrationBuilder.Sql(
                """
                ALTER TABLE "ErpGenJournalLines"
                    ALTER COLUMN "BalAccountType" TYPE integer USING (
                        CASE "BalAccountType"
                            WHEN 'Customer' THEN 1
                            WHEN 'Vendor' THEN 2
                            WHEN 'G/L Account' THEN 0
                            ELSE NULL
                        END),
                    ALTER COLUMN "AccountType" TYPE integer USING (
                        CASE "AccountType" WHEN 'Customer' THEN 1 WHEN 'Vendor' THEN 2 ELSE 0 END),
                    ALTER COLUMN "AccountType" SET DEFAULT 0,
                    ALTER COLUMN "AccountType" SET NOT NULL;
                """
            );

            migrationBuilder.AddColumn<string>(
                name: "AppliesToDocNo",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentDate",
                table: "ErpGenJournalLines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "ErpGenJournalLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDocumentNo",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurringFrequency",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurringMethod",
                table: "ErpGenJournalLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BalAccountNo",
                table: "ErpGenJournalBatches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BalAccountType",
                table: "ErpGenJournalBatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoSeriesCode",
                table: "ErpGenJournalBatches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonCode",
                table: "ErpGenJournalBatches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RegisterNo",
                table: "ErpCustomerLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Reversed",
                table: "ErpCustomerLedgerEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ReversedByEntryNo",
                table: "ErpCustomerLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ReversedEntryNo",
                table: "ErpCustomerLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TransactionNo",
                table: "ErpCustomerLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "DefaultColumnLayoutName",
                table: "ErpAccountSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(
                """
                ALTER TABLE "ErpAccountScheduleLines"
                    ALTER COLUMN "TotalingType" TYPE integer USING (
                        CASE "TotalingType"
                            WHEN 'Total Accounts' THEN 1
                            WHEN 'Formula' THEN 2
                            WHEN 'Description' THEN 3
                            ELSE 0
                        END),
                    ALTER COLUMN "TotalingType" SET DEFAULT 0,
                    ALTER COLUMN "TotalingType" SET NOT NULL;
                """
            );

            migrationBuilder.AddColumn<bool>(
                name: "Bold",
                table: "ErpAccountScheduleLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideIfZero",
                table: "ErpAccountScheduleLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Indentation",
                table: "ErpAccountScheduleLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Italic",
                table: "ErpAccountScheduleLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOppositeSign",
                table: "ErpAccountScheduleLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ErpColumnLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpColumnLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpExportTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Fields = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    IsShared = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ErpExportTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpGenJournalTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Recurring = table.Column<bool>(type: "boolean", nullable: false),
                    SourceCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    NoSeriesCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpGenJournalTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpGLRegisters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<long>(type: "bigint", nullable: false),
                    FromEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    ToEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    FromCustomerEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    ToCustomerEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    FromVendorEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    ToVendorEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    JournalBatchName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    Reversed = table.Column<bool>(type: "boolean", nullable: false),
                    ReversedByRegisterNo = table.Column<long>(type: "bigint", nullable: false),
                    ReversedRegisterNo = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpGLRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpNumberSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastValue = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ErpNumberSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardGeneralJournals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalTemplateName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpStandardGeneralJournals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpWebhookDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangeKind = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: true),
                    Error = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpWebhookDeliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpWebhookSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EndpointUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ChangeKinds = table.Column<int>(type: "integer", nullable: false),
                    Secret = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LastDeliveryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpWebhookSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpColumnLayoutLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ColumnLayoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ColumnNo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ColumnHeader = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ColumnType = table.Column<int>(type: "integer", nullable: false),
                    ComparisonDateFormula = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ShowOppositeSign = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpColumnLayoutLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpColumnLayoutLines_ErpColumnLayouts_ColumnLayoutId",
                        column: x => x.ColumnLayoutId,
                        principalTable: "ErpColumnLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardGeneralJournalLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardGeneralJournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    BalAccountType = table.Column<int>(type: "integer", nullable: true),
                    BalAccountNo = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStandardGeneralJournalLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpStandardGeneralJournalLines_ErpStandardGeneralJournals_S~",
                        column: x => x.StandardGeneralJournalId,
                        principalTable: "ErpStandardGeneralJournals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPublishedWebServices_TenantId_CompanyId_ServiceName",
                table: "ErpPublishedWebServices",
                columns: new[] { "TenantId", "CompanyId", "ServiceName" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpColumnLayoutLines_ColumnLayoutId",
                table: "ErpColumnLayoutLines",
                column: "ColumnLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpColumnLayouts_TenantId_CompanyId_Name",
                table: "ErpColumnLayouts",
                columns: new[] { "TenantId", "CompanyId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpExportTemplates_TenantId_CompanyId_EntityName_Name",
                table: "ErpExportTemplates",
                columns: new[] { "TenantId", "CompanyId", "EntityName", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpGenJournalTemplates_TenantId_CompanyId_Name",
                table: "ErpGenJournalTemplates",
                columns: new[] { "TenantId", "CompanyId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpGLRegisters_TenantId_CompanyId_No",
                table: "ErpGLRegisters",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpNumberSequences_TenantId_CompanyId_Name",
                table: "ErpNumberSequences",
                columns: new[] { "TenantId", "CompanyId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardGeneralJournalLines_StandardGeneralJournalId",
                table: "ErpStandardGeneralJournalLines",
                column: "StandardGeneralJournalId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardGeneralJournals_TenantId_CompanyId_JournalTempla~",
                table: "ErpStandardGeneralJournals",
                columns: new[] { "TenantId", "CompanyId", "JournalTemplateName", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpWebhookDeliveries_Status_NextAttemptTime",
                table: "ErpWebhookDeliveries",
                columns: new[] { "Status", "NextAttemptTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpWebhookDeliveries_SubscriptionId",
                table: "ErpWebhookDeliveries",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpWebhookSubscriptions_EntityName_Active",
                table: "ErpWebhookSubscriptions",
                columns: new[] { "EntityName", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpColumnLayoutLines");

            migrationBuilder.DropTable(
                name: "ErpExportTemplates");

            migrationBuilder.DropTable(
                name: "ErpGenJournalTemplates");

            migrationBuilder.DropTable(
                name: "ErpGLRegisters");

            migrationBuilder.DropTable(
                name: "ErpNumberSequences");

            migrationBuilder.DropTable(
                name: "ErpStandardGeneralJournalLines");

            migrationBuilder.DropTable(
                name: "ErpWebhookDeliveries");

            migrationBuilder.DropTable(
                name: "ErpWebhookSubscriptions");

            migrationBuilder.DropTable(
                name: "ErpColumnLayouts");

            migrationBuilder.DropTable(
                name: "ErpStandardGeneralJournals");

            migrationBuilder.DropIndex(
                name: "IX_ErpPublishedWebServices_TenantId_CompanyId_ServiceName",
                table: "ErpPublishedWebServices");

            migrationBuilder.DropColumn(
                name: "RegisterNo",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "Reversed",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByEntryNo",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ReversedEntryNo",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "TransactionNo",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ErpPublishedWebServices");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "ErpPublishedWebServices");

            migrationBuilder.DropColumn(
                name: "EntityName",
                table: "ErpPublishedWebServices");

            migrationBuilder.DropColumn(
                name: "ExcludedFields",
                table: "ErpPublishedWebServices");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "ErpPublishedWebServices");

            migrationBuilder.DropColumn(
                name: "DimensionSetId",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "DocumentDate",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "ReasonCode",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "RegisterNo",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "Reversed",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByEntryNo",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "ReversedEntryNo",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "SourceCode",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "TransactionNo",
                table: "ErpGLEntries");

            migrationBuilder.DropColumn(
                name: "AppliesToDocNo",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "DocumentDate",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "ExternalDocumentNo",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "RecurringFrequency",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "RecurringMethod",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "BalAccountNo",
                table: "ErpGenJournalBatches");

            migrationBuilder.DropColumn(
                name: "BalAccountType",
                table: "ErpGenJournalBatches");

            migrationBuilder.DropColumn(
                name: "NoSeriesCode",
                table: "ErpGenJournalBatches");

            migrationBuilder.DropColumn(
                name: "ReasonCode",
                table: "ErpGenJournalBatches");

            migrationBuilder.DropColumn(
                name: "RegisterNo",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "Reversed",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByEntryNo",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ReversedEntryNo",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "TransactionNo",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "DefaultColumnLayoutName",
                table: "ErpAccountSchedules");

            migrationBuilder.DropColumn(
                name: "Bold",
                table: "ErpAccountScheduleLines");

            migrationBuilder.DropColumn(
                name: "HideIfZero",
                table: "ErpAccountScheduleLines");

            migrationBuilder.DropColumn(
                name: "Indentation",
                table: "ErpAccountScheduleLines");

            migrationBuilder.DropColumn(
                name: "Italic",
                table: "ErpAccountScheduleLines");

            migrationBuilder.DropColumn(
                name: "ShowOppositeSign",
                table: "ErpAccountScheduleLines");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "ErpPublishedWebServices",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ObjectType",
                table: "ErpPublishedWebServices",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "BalAccountType",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountType",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "TotalingType",
                table: "ErpAccountScheduleLines",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
