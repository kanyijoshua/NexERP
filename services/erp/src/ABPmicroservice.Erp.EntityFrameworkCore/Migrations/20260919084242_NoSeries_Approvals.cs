using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class NoSeries_Approvals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Not a rename: TableName held an entity name, WorkflowCode names the workflow that raised the request.
            migrationBuilder.DropColumn(
                name: "TableName",
                table: "ErpApprovalEntries");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowCode",
                table: "ErpApprovalEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApproverLimitType",
                table: "ErpWorkflows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocumentKind",
                table: "ErpWorkflows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DueDays",
                table: "ErpWorkflows",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumAmount",
                table: "ErpWorkflows",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            // text -> integer has no automatic cast in PostgreSQL; map the old status texts onto ApprovalStatus.
            migrationBuilder.Sql(
                """
                ALTER TABLE "ErpApprovalEntries"
                    ALTER COLUMN "Status" TYPE integer USING (
                        CASE "Status"
                            WHEN 'Open' THEN 1
                            WHEN 'Canceled' THEN 2
                            WHEN 'Rejected' THEN 3
                            WHEN 'Approved' THEN 4
                            ELSE 0
                        END),
                    ALTER COLUMN "Status" SET DEFAULT 0,
                    ALTER COLUMN "Status" SET NOT NULL;
                """);

            migrationBuilder.AddColumn<string>(
                name: "ApproverUserName",
                table: "ErpApprovalEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "ErpApprovalEntries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentKind",
                table: "ErpApprovalEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ErpApprovalEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChangeTime",
                table: "ErpApprovalEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderUserName",
                table: "ErpApprovalEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SequenceNo",
                table: "ErpApprovalEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ErpApprovalUserSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubstituteUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesAmountApprovalLimit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnlimitedSalesApproval = table.Column<bool>(type: "boolean", nullable: false),
                    PurchaseAmountApprovalLimit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnlimitedPurchaseApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsApprovalAdministrator = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpApprovalUserSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpNoSeries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DefaultNos = table.Column<bool>(type: "boolean", nullable: false),
                    ManualNos = table.Column<bool>(type: "boolean", nullable: false),
                    DateOrder = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpNoSeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPurchasesPayablesSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorNos = table.Column<string>(type: "text", nullable: true),
                    QuoteNos = table.Column<string>(type: "text", nullable: true),
                    OrderNos = table.Column<string>(type: "text", nullable: true),
                    InvoiceNos = table.Column<string>(type: "text", nullable: true),
                    CreditMemoNos = table.Column<string>(type: "text", nullable: true),
                    PostedInvoiceNos = table.Column<string>(type: "text", nullable: true),
                    PostedCreditMemoNos = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpPurchasesPayablesSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSalesReceivablesSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNos = table.Column<string>(type: "text", nullable: true),
                    QuoteNos = table.Column<string>(type: "text", nullable: true),
                    OrderNos = table.Column<string>(type: "text", nullable: true),
                    InvoiceNos = table.Column<string>(type: "text", nullable: true),
                    CreditMemoNos = table.Column<string>(type: "text", nullable: true),
                    PostedInvoiceNos = table.Column<string>(type: "text", nullable: true),
                    PostedCreditMemoNos = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpSalesReceivablesSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpNoSeriesLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    StartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartingNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EndingNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WarningNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IncrementByNo = table.Column<int>(type: "integer", nullable: false),
                    LastNoUsed = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastDateUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Open = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpNoSeriesLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpNoSeriesLines_ErpNoSeries_NoSeriesId",
                        column: x => x.NoSeriesId,
                        principalTable: "ErpNoSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpWorkflows_TenantId_CompanyId_Code",
                table: "ErpWorkflows",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpApprovalEntries_ApproverId_Status",
                table: "ErpApprovalEntries",
                columns: new[] { "ApproverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpApprovalEntries_DocumentKind_DocumentId",
                table: "ErpApprovalEntries",
                columns: new[] { "DocumentKind", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpApprovalUserSetups_TenantId_CompanyId_UserId",
                table: "ErpApprovalUserSetups",
                columns: new[] { "TenantId", "CompanyId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpNoSeries_TenantId_CompanyId_Code",
                table: "ErpNoSeries",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpNoSeriesLines_NoSeriesId",
                table: "ErpNoSeriesLines",
                column: "NoSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPurchasesPayablesSetups_TenantId_CompanyId",
                table: "ErpPurchasesPayablesSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSalesReceivablesSetups_TenantId_CompanyId",
                table: "ErpSalesReceivablesSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpApprovalUserSetups");

            migrationBuilder.DropTable(
                name: "ErpNoSeriesLines");

            migrationBuilder.DropTable(
                name: "ErpPurchasesPayablesSetups");

            migrationBuilder.DropTable(
                name: "ErpSalesReceivablesSetups");

            migrationBuilder.DropTable(
                name: "ErpNoSeries");

            migrationBuilder.DropIndex(
                name: "IX_ErpWorkflows_TenantId_CompanyId_Code",
                table: "ErpWorkflows");

            migrationBuilder.DropIndex(
                name: "IX_ErpApprovalEntries_ApproverId_Status",
                table: "ErpApprovalEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpApprovalEntries_DocumentKind_DocumentId",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "ApproverLimitType",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "DocumentKind",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "DueDays",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "MinimumAmount",
                table: "ErpWorkflows");

            migrationBuilder.DropColumn(
                name: "ApproverUserName",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "DocumentKind",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "LastStatusChangeTime",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "SenderUserName",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "SequenceNo",
                table: "ErpApprovalEntries");

            migrationBuilder.DropColumn(
                name: "WorkflowCode",
                table: "ErpApprovalEntries");

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "ErpApprovalEntries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ErpApprovalEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
