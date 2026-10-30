using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Finance_Completion_Operations_Apps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Adjustment",
                table: "ErpValueEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPostedToGL",
                table: "ErpValueEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibilityCenter",
                table: "ErpSalesHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExportedToPaymentFile",
                table: "ErpGenJournalLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecipientBankAccount",
                table: "ErpGenJournalLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpAppraisalPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Closed = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpAppraisalPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpAppraisals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PeriodCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ReviewerEmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployeeComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewerComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpAppraisals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBusinessUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubsidiaryCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Consolidate = table.Column<bool>(type: "boolean", nullable: false),
                    ConsolidationPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ResidualAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastRunStartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunEndingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpBusinessUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCampaignMailingEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContactNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ErpCampaignMailingEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostAccountingSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterDimension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CostObjectDimension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastGLEntryNoTransferred = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ErpCostAccountingSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostAllocationSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CostTypeRange = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CostCenterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CostObjectCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreditToCostTypeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpCostAllocationSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostCenters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCostCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CostTypeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CostCenterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CostObjectCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GLEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    GLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AllocationCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RegisterNo = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCostEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCostObjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostRegisters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FromEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    ToEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    EntryCount = table.Column<int>(type: "integer", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCostRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    GLAccountRange = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Totaling = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CostCenterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CostObjectCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpCostTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCustomerSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlanCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LastInvoiceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastInvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpCustomerSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDirectDebitCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreditorIdentifier = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpDirectDebitCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDirectDebitMandates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerBankAccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ExpectedUses = table.Column<int>(type: "integer", nullable: false),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpDirectDebitMandates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpICInboxTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    FromCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    PartnerDocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedDocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpICInboxTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpICOutboxTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    PartnerCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpICOutboxTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpICPartners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DefaultGLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpICPartners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpJobApplicants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VacancyNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AppliedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    ExpectedSalary = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpJobApplicants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpJobVacancies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Positions = table.Column<int>(type: "integer", nullable: false),
                    HiredCount = table.Column<int>(type: "integer", nullable: false),
                    OpeningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpJobVacancies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMaintenanceEquipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FixedAssetNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TeamCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TechnicianEmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PreventiveIntervalDays = table.Column<int>(type: "integer", nullable: false),
                    NextPreventiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpMaintenanceEquipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMaintenanceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EquipmentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeamCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TechnicianEmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DurationHours = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_ErpMaintenanceRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMaintenanceTeams",
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
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpMaintenanceTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPosOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TerminalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    AmountTendered = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Change = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalPaid = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    PostedInvoiceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPosOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPosSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TerminalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpenedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OpeningFloat = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CashSales = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CardSales = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CountedCash = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPosSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPosTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    WalkInCustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CashBankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CardBankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPosTerminals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpQualityChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ControlPointCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ItemDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SourceDocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LotNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TestType = table.Column<int>(type: "integer", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaxValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MeasuredValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    CheckedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CheckedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_ErpQualityChecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpQualityControlPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ItemCategoryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TestType = table.Column<int>(type: "integer", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaxValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpQualityControlPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpSegments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardCustomerSalesCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InsertOnQuotes = table.Column<int>(type: "integer", nullable: false),
                    InsertOnOrders = table.Column<int>(type: "integer", nullable: false),
                    InsertOnInvoices = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpStandardCustomerSalesCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardPurchaseCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStandardPurchaseCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardSalesCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStandardSalesCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardVendorPurchaseCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InsertOnQuotes = table.Column<int>(type: "integer", nullable: false),
                    InsertOnOrders = table.Column<int>(type: "integer", nullable: false),
                    InsertOnInvoices = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpStandardVendorPurchaseCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingPeriodMonths = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LineType = table.Column<int>(type: "integer", nullable: false),
                    LineNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TrialDays = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVehicleContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecurringCost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CostFrequency = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpVehicleContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVehicleLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Odometer = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Liters = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpVehicleLogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LicensePlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Make = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Vin = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FuelType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DriverEmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FixedAssetNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Odometer = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpVehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpAppraisalLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Objective = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Target = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SelfScore = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ManagerScore = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Comment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpAppraisalLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpAppraisalLines_ErpAppraisals_AppraisalId",
                        column: x => x.AppraisalId,
                        principalTable: "ErpAppraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpCostAllocationTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CostAllocationSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    CostTypeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CostCenterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CostObjectCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Share = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCostAllocationTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpCostAllocationTargets_ErpCostAllocationSources_CostAlloc~",
                        column: x => x.CostAllocationSourceId,
                        principalTable: "ErpCostAllocationSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpDirectDebitCollectionEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DirectDebitCollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    CustomerLedgerEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MandateCode = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    SequenceType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpDirectDebitCollectionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpDirectDebitCollectionEntries_ErpDirectDebitCollections_D~",
                        column: x => x.DirectDebitCollectionId,
                        principalTable: "ErpDirectDebitCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpICInboxLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ICInboxTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LineDiscountPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpICInboxLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpICInboxLines_ErpICInboxTransactions_ICInboxTransactionId",
                        column: x => x.ICInboxTransactionId,
                        principalTable: "ErpICInboxTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpPosOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PosOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DiscountPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LineAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPosOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPosOrderLines_ErpPosOrders_PosOrderId",
                        column: x => x.PosOrderId,
                        principalTable: "ErpPosOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpSegmentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSegmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpSegmentLines_ErpSegments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "ErpSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardPurchaseLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardPurchaseCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStandardPurchaseLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpStandardPurchaseLines_ErpStandardPurchaseCodes_StandardP~",
                        column: x => x.StandardPurchaseCodeId,
                        principalTable: "ErpStandardPurchaseCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpStandardSalesLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardSalesCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpStandardSalesLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpStandardSalesLines_ErpStandardSalesCodes_StandardSalesCo~",
                        column: x => x.StandardSalesCodeId,
                        principalTable: "ErpStandardSalesCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpAppraisalLines_AppraisalId",
                table: "ErpAppraisalLines",
                column: "AppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpAppraisalPeriods_TenantId_CompanyId_Code",
                table: "ErpAppraisalPeriods",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpAppraisals_CompanyId_EmployeeNo_PeriodCode",
                table: "ErpAppraisals",
                columns: new[] { "CompanyId", "EmployeeNo", "PeriodCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpAppraisals_TenantId_CompanyId_No",
                table: "ErpAppraisals",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpBusinessUnits_TenantId_CompanyId_Code",
                table: "ErpBusinessUnits",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCampaignMailingEntries_CompanyId_CampaignNo",
                table: "ErpCampaignMailingEntries",
                columns: new[] { "CompanyId", "CampaignNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostAccountingSetups_TenantId_CompanyId",
                table: "ErpCostAccountingSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostAllocationSources_TenantId_CompanyId_Code",
                table: "ErpCostAllocationSources",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostAllocationTargets_CostAllocationSourceId",
                table: "ErpCostAllocationTargets",
                column: "CostAllocationSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostCenters_TenantId_CompanyId_Code",
                table: "ErpCostCenters",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostEntries_CompanyId_CostTypeNo_CostCenterCode",
                table: "ErpCostEntries",
                columns: new[] { "CompanyId", "CostTypeNo", "CostCenterCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostEntries_CompanyId_CostTypeNo_CostObjectCode",
                table: "ErpCostEntries",
                columns: new[] { "CompanyId", "CostTypeNo", "CostObjectCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostObjects_TenantId_CompanyId_Code",
                table: "ErpCostObjects",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostRegisters_CompanyId_No",
                table: "ErpCostRegisters",
                columns: new[] { "CompanyId", "No" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCostTypes_TenantId_CompanyId_No",
                table: "ErpCostTypes",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCustomerSubscriptions_CompanyId_Status_NextBillingDate",
                table: "ErpCustomerSubscriptions",
                columns: new[] { "CompanyId", "Status", "NextBillingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCustomerSubscriptions_TenantId_CompanyId_No",
                table: "ErpCustomerSubscriptions",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpDirectDebitCollectionEntries_CustomerLedgerEntryId",
                table: "ErpDirectDebitCollectionEntries",
                column: "CustomerLedgerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpDirectDebitCollectionEntries_DirectDebitCollectionId",
                table: "ErpDirectDebitCollectionEntries",
                column: "DirectDebitCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpDirectDebitCollections_TenantId_CompanyId_No",
                table: "ErpDirectDebitCollections",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpDirectDebitMandates_CompanyId_CustomerNo",
                table: "ErpDirectDebitMandates",
                columns: new[] { "CompanyId", "CustomerNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpDirectDebitMandates_TenantId_CompanyId_Code",
                table: "ErpDirectDebitMandates",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpICInboxLines_ICInboxTransactionId",
                table: "ErpICInboxLines",
                column: "ICInboxTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpICInboxTransactions_CompanyId_TransactionNo",
                table: "ErpICInboxTransactions",
                columns: new[] { "CompanyId", "TransactionNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpICOutboxTransactions_CompanyId_TransactionNo",
                table: "ErpICOutboxTransactions",
                columns: new[] { "CompanyId", "TransactionNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpICPartners_TenantId_CompanyId_Code",
                table: "ErpICPartners",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpJobApplicants_CompanyId_VacancyNo",
                table: "ErpJobApplicants",
                columns: new[] { "CompanyId", "VacancyNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpJobApplicants_TenantId_CompanyId_No",
                table: "ErpJobApplicants",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpJobVacancies_TenantId_CompanyId_No",
                table: "ErpJobVacancies",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMaintenanceEquipment_TenantId_CompanyId_No",
                table: "ErpMaintenanceEquipment",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMaintenanceRequests_CompanyId_EquipmentNo",
                table: "ErpMaintenanceRequests",
                columns: new[] { "CompanyId", "EquipmentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpMaintenanceRequests_TenantId_CompanyId_No",
                table: "ErpMaintenanceRequests",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMaintenanceTeams_TenantId_CompanyId_Code",
                table: "ErpMaintenanceTeams",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPosOrderLines_PosOrderId",
                table: "ErpPosOrderLines",
                column: "PosOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPosOrders_SessionId",
                table: "ErpPosOrders",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPosOrders_TenantId_CompanyId_No",
                table: "ErpPosOrders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPosSessions_CompanyId_TerminalCode_Status",
                table: "ErpPosSessions",
                columns: new[] { "CompanyId", "TerminalCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPosSessions_TenantId_CompanyId_No",
                table: "ErpPosSessions",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPosTerminals_TenantId_CompanyId_Code",
                table: "ErpPosTerminals",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpQualityChecks_CompanyId_SourceDocumentNo",
                table: "ErpQualityChecks",
                columns: new[] { "CompanyId", "SourceDocumentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpQualityChecks_TenantId_CompanyId_No",
                table: "ErpQualityChecks",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpQualityControlPoints_TenantId_CompanyId_Code",
                table: "ErpQualityControlPoints",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSegmentLines_SegmentId",
                table: "ErpSegmentLines",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSegments_TenantId_CompanyId_No",
                table: "ErpSegments",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardCustomerSalesCodes_TenantId_CompanyId_CustomerNo~",
                table: "ErpStandardCustomerSalesCodes",
                columns: new[] { "TenantId", "CompanyId", "CustomerNo", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardPurchaseCodes_TenantId_CompanyId_Code",
                table: "ErpStandardPurchaseCodes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardPurchaseLines_StandardPurchaseCodeId",
                table: "ErpStandardPurchaseLines",
                column: "StandardPurchaseCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardSalesCodes_TenantId_CompanyId_Code",
                table: "ErpStandardSalesCodes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardSalesLines_StandardSalesCodeId",
                table: "ErpStandardSalesLines",
                column: "StandardSalesCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpStandardVendorPurchaseCodes_TenantId_CompanyId_VendorNo_~",
                table: "ErpStandardVendorPurchaseCodes",
                columns: new[] { "TenantId", "CompanyId", "VendorNo", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSubscriptionPlans_TenantId_CompanyId_Code",
                table: "ErpSubscriptionPlans",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpVehicleContracts_CompanyId_VehicleNo",
                table: "ErpVehicleContracts",
                columns: new[] { "CompanyId", "VehicleNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpVehicleLogEntries_CompanyId_VehicleNo_Date",
                table: "ErpVehicleLogEntries",
                columns: new[] { "CompanyId", "VehicleNo", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpVehicles_TenantId_CompanyId_No",
                table: "ErpVehicles",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpAppraisalLines");

            migrationBuilder.DropTable(
                name: "ErpAppraisalPeriods");

            migrationBuilder.DropTable(
                name: "ErpBusinessUnits");

            migrationBuilder.DropTable(
                name: "ErpCampaignMailingEntries");

            migrationBuilder.DropTable(
                name: "ErpCostAccountingSetups");

            migrationBuilder.DropTable(
                name: "ErpCostAllocationTargets");

            migrationBuilder.DropTable(
                name: "ErpCostCenters");

            migrationBuilder.DropTable(
                name: "ErpCostEntries");

            migrationBuilder.DropTable(
                name: "ErpCostObjects");

            migrationBuilder.DropTable(
                name: "ErpCostRegisters");

            migrationBuilder.DropTable(
                name: "ErpCostTypes");

            migrationBuilder.DropTable(
                name: "ErpCustomerSubscriptions");

            migrationBuilder.DropTable(
                name: "ErpDirectDebitCollectionEntries");

            migrationBuilder.DropTable(
                name: "ErpDirectDebitMandates");

            migrationBuilder.DropTable(
                name: "ErpICInboxLines");

            migrationBuilder.DropTable(
                name: "ErpICOutboxTransactions");

            migrationBuilder.DropTable(
                name: "ErpICPartners");

            migrationBuilder.DropTable(
                name: "ErpJobApplicants");

            migrationBuilder.DropTable(
                name: "ErpJobVacancies");

            migrationBuilder.DropTable(
                name: "ErpMaintenanceEquipment");

            migrationBuilder.DropTable(
                name: "ErpMaintenanceRequests");

            migrationBuilder.DropTable(
                name: "ErpMaintenanceTeams");

            migrationBuilder.DropTable(
                name: "ErpPosOrderLines");

            migrationBuilder.DropTable(
                name: "ErpPosSessions");

            migrationBuilder.DropTable(
                name: "ErpPosTerminals");

            migrationBuilder.DropTable(
                name: "ErpQualityChecks");

            migrationBuilder.DropTable(
                name: "ErpQualityControlPoints");

            migrationBuilder.DropTable(
                name: "ErpSegmentLines");

            migrationBuilder.DropTable(
                name: "ErpStandardCustomerSalesCodes");

            migrationBuilder.DropTable(
                name: "ErpStandardPurchaseLines");

            migrationBuilder.DropTable(
                name: "ErpStandardSalesLines");

            migrationBuilder.DropTable(
                name: "ErpStandardVendorPurchaseCodes");

            migrationBuilder.DropTable(
                name: "ErpSubscriptionPlans");

            migrationBuilder.DropTable(
                name: "ErpVehicleContracts");

            migrationBuilder.DropTable(
                name: "ErpVehicleLogEntries");

            migrationBuilder.DropTable(
                name: "ErpVehicles");

            migrationBuilder.DropTable(
                name: "ErpAppraisals");

            migrationBuilder.DropTable(
                name: "ErpCostAllocationSources");

            migrationBuilder.DropTable(
                name: "ErpDirectDebitCollections");

            migrationBuilder.DropTable(
                name: "ErpICInboxTransactions");

            migrationBuilder.DropTable(
                name: "ErpPosOrders");

            migrationBuilder.DropTable(
                name: "ErpSegments");

            migrationBuilder.DropTable(
                name: "ErpStandardPurchaseCodes");

            migrationBuilder.DropTable(
                name: "ErpStandardSalesCodes");

            migrationBuilder.DropColumn(
                name: "Adjustment",
                table: "ErpValueEntries");

            migrationBuilder.DropColumn(
                name: "CostPostedToGL",
                table: "ErpValueEntries");

            migrationBuilder.DropColumn(
                name: "ResponsibilityCenter",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ExportedToPaymentFile",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "RecipientBankAccount",
                table: "ErpGenJournalLines");
        }
    }
}
