using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Leave_Prepayments_ItemCharges_Emailing_ChangeLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentPct",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtAmtDeducted",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtAmtInv",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtLineAmount",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LastPrepmtCrMemoNo",
                table: "ErpSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPrepmtInvoiceNo",
                table: "ErpSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentPct",
                table: "ErpSalesHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtDeductedAmount",
                table: "ErpSalesHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtInvoicedAmount",
                table: "ErpSalesHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentPct",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtAmtDeducted",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtAmtInv",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtLineAmount",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LastPrepmtCrMemoNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPrepmtInvoiceNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtDeductedAmount",
                table: "ErpPurchaseHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepmtInvoicedAmount",
                table: "ErpPurchaseHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeight",
                table: "ErpItems",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitVolume",
                table: "ErpItems",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LeaveApplicationNos",
                table: "ErpHumanResourcesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchPrepaymentsAccountNo",
                table: "ErpGeneralPostingSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesPrepaymentsAccountNo",
                table: "ErpGeneralPostingSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpChangeLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeOfChange = table.Column<int>(type: "integer", nullable: false),
                    OldValue = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpChangeLogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpChangeLogSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeLogActivated = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpChangeLogSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpChangeLogTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogInsertion = table.Column<int>(type: "integer", nullable: false),
                    LogModification = table.Column<int>(type: "integer", nullable: false),
                    LogDeletion = table.Column<int>(type: "integer", nullable: false),
                    TrackedFields = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_ErpChangeLogTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmailTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    BodyHtmlTemplate = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpEmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemChargeAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceDocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemChargeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AppliesToEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliesToEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    AppliesToDocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    QtyToAssign = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QtyAssigned = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AmountToAssign = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Posted = table.Column<bool>(type: "boolean", nullable: false),
                    AmountPostedLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpItemChargeAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemCharges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GenProdPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VatProdPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpItemCharges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLeaveApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LeaveTypeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LeavePeriodYear = table.Column<int>(type: "integer", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysApplied = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DaysApproved = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RelieverEmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApproverComment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LeaveStatus = table.Column<int>(type: "integer", nullable: false),
                    PostedAbsenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecalledDays = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RecallDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecallReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpLeaveApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLeaveLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LeaveTypeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LeavePeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Days = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpLeaveLedgerEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CauseOfAbsenceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DefaultEntitlementDays = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaxCarryForwardDays = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AccrualFrequency = table.Column<int>(type: "integer", nullable: false),
                    Paid = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    GenderRestriction = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_ErpLeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSentEmailLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PartyNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PartyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RecipientEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CcEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    BodyHtml = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AttachmentCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ErpSentEmailLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpChangeLogEntries_CompanyId_ChangedAt",
                table: "ErpChangeLogEntries",
                columns: new[] { "CompanyId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpChangeLogEntries_CompanyId_EntityName_RecordId",
                table: "ErpChangeLogEntries",
                columns: new[] { "CompanyId", "EntityName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpChangeLogSetups_TenantId_CompanyId",
                table: "ErpChangeLogSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpChangeLogTables_TenantId_CompanyId_EntityName",
                table: "ErpChangeLogTables",
                columns: new[] { "TenantId", "CompanyId", "EntityName" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmailTemplates_TenantId_CompanyId_Code",
                table: "ErpEmailTemplates",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemChargeAssignments_AppliesToEntryId",
                table: "ErpItemChargeAssignments",
                column: "AppliesToEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemChargeAssignments_CompanyId_SourceType_SourceLineId",
                table: "ErpItemChargeAssignments",
                columns: new[] { "CompanyId", "SourceType", "SourceLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemCharges_TenantId_CompanyId_Code",
                table: "ErpItemCharges",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLeaveApplications_CompanyId_EmployeeId_StartDate",
                table: "ErpLeaveApplications",
                columns: new[] { "CompanyId", "EmployeeId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpLeaveApplications_TenantId_CompanyId_No",
                table: "ErpLeaveApplications",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLeaveLedgerEntries_CompanyId_EmployeeId_LeaveTypeCode_Le~",
                table: "ErpLeaveLedgerEntries",
                columns: new[] { "CompanyId", "EmployeeId", "LeaveTypeCode", "LeavePeriodYear" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpLeaveLedgerEntries_CompanyId_EntryNo",
                table: "ErpLeaveLedgerEntries",
                columns: new[] { "CompanyId", "EntryNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpLeaveTypes_TenantId_CompanyId_Code",
                table: "ErpLeaveTypes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSentEmailLogs_CompanyId_DocumentType_DocumentId",
                table: "ErpSentEmailLogs",
                columns: new[] { "CompanyId", "DocumentType", "DocumentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpChangeLogEntries");

            migrationBuilder.DropTable(
                name: "ErpChangeLogSetups");

            migrationBuilder.DropTable(
                name: "ErpChangeLogTables");

            migrationBuilder.DropTable(
                name: "ErpEmailTemplates");

            migrationBuilder.DropTable(
                name: "ErpItemChargeAssignments");

            migrationBuilder.DropTable(
                name: "ErpItemCharges");

            migrationBuilder.DropTable(
                name: "ErpLeaveApplications");

            migrationBuilder.DropTable(
                name: "ErpLeaveLedgerEntries");

            migrationBuilder.DropTable(
                name: "ErpLeaveTypes");

            migrationBuilder.DropTable(
                name: "ErpSentEmailLogs");

            migrationBuilder.DropColumn(
                name: "PrepaymentPct",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "PrepmtAmtDeducted",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "PrepmtAmtInv",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "PrepmtLineAmount",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "LastPrepmtCrMemoNo",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "LastPrepmtInvoiceNo",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "PrepaymentPct",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "PrepmtDeductedAmount",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "PrepmtInvoicedAmount",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "PrepaymentPct",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "PrepmtAmtDeducted",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "PrepmtAmtInv",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "PrepmtLineAmount",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "LastPrepmtCrMemoNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "LastPrepmtInvoiceNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PrepmtDeductedAmount",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PrepmtInvoicedAmount",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "UnitVolume",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "LeaveApplicationNos",
                table: "ErpHumanResourcesSetups");

            migrationBuilder.DropColumn(
                name: "PurchPrepaymentsAccountNo",
                table: "ErpGeneralPostingSetups");

            migrationBuilder.DropColumn(
                name: "SalesPrepaymentsAccountNo",
                table: "ErpGeneralPostingSetups");
        }
    }
}
