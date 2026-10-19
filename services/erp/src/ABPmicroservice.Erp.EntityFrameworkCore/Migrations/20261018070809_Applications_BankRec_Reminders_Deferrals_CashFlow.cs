using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Applications_BankRec_Reminders_Deferrals_CashFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentDiscCreditAccNo",
                table: "ErpVendorPostingGroups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDiscDebitAccNo",
                table: "ErpVendorPostingGroups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPmtDiscPossible",
                table: "ErpVendorLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PmtDiscountDate",
                table: "ErpVendorLedgerEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingPmtDiscPossible",
                table: "ErpVendorLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FinChargeMemoNos",
                table: "ErpSalesReceivablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuedFinChargeMemoNos",
                table: "ErpSalesReceivablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuedReminderNos",
                table: "ErpSalesReceivablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderNos",
                table: "ErpSalesReceivablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeferralCode",
                table: "ErpSalesLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeferralCode",
                table: "ErpGenJournalLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentDiscountPct",
                table: "ErpGenJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PmtDiscountDate",
                table: "ErpGenJournalLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalFeeAccountNo",
                table: "ErpCustomerPostingGroups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterestAccountNo",
                table: "ErpCustomerPostingGroups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDiscCreditAccNo",
                table: "ErpCustomerPostingGroups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDiscDebitAccNo",
                table: "ErpCustomerPostingGroups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastIssuedReminderLevel",
                table: "ErpCustomerLedgerEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPmtDiscPossible",
                table: "ErpCustomerLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PmtDiscountDate",
                table: "ErpCustomerLedgerEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingPmtDiscPossible",
                table: "ErpCustomerLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "StatementLineNo",
                table: "ErpBankAccountLedgerEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatementNo",
                table: "ErpBankAccountLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatementStatus",
                table: "ErpBankAccountLedgerEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ErpCashFlowForecastEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CashFlowForecastNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CashFlowDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCashFlowForecastEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCashFlowForecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ManualPaymentsTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsiderReceivables = table.Column<bool>(type: "boolean", nullable: false),
                    ConsiderPayables = table.Column<bool>(type: "boolean", nullable: false),
                    ConsiderSalesOrders = table.Column<bool>(type: "boolean", nullable: false),
                    ConsiderPurchaseOrders = table.Column<bool>(type: "boolean", nullable: false),
                    ConsiderManual = table.Column<bool>(type: "boolean", nullable: false),
                    ConsiderLiquidFunds = table.Column<bool>(type: "boolean", nullable: false),
                    GLBudgetName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GLBudgetFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GLBudgetTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpCashFlowForecasts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCashFlowManualEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecurringFrequency = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EndingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpCashFlowManualEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCashFlowWorksheetLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CashFlowForecastNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    CashFlowDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpCashFlowWorksheetLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDeferralTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeferralAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeferralPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CalcMethod = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<int>(type: "integer", nullable: false),
                    NoOfPeriods = table.Column<int>(type: "integer", nullable: false),
                    PeriodDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpDeferralTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFinanceChargeMemoHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinChargeTermsCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssuedNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpFinanceChargeMemoHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFinanceChargeTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterestRate = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    InterestPeriodDays = table.Column<int>(type: "integer", nullable: false),
                    MinimumAmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AdditionalFeeLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GracePeriod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DueDateCalculation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PostInterest = table.Column<bool>(type: "boolean", nullable: false),
                    PostAdditionalFee = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpFinanceChargeTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedDeferralHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    DeferralCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DeferralAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NoOfPeriods = table.Column<int>(type: "integer", nullable: false),
                    AmountToDefer = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPostedDeferralHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpReminderHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderTermsCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ReminderLevel = table.Column<int>(type: "integer", nullable: false),
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
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssuedNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpReminderHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpReminderLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderTermsCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    No = table.Column<int>(type: "integer", nullable: false),
                    GracePeriod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DueDateCalculation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AdditionalFeeLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CalculateInterest = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpReminderLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpReminderTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostInterest = table.Column<bool>(type: "boolean", nullable: false),
                    PostAdditionalFee = table.Column<bool>(type: "boolean", nullable: false),
                    MaxNoOfReminders = table.Column<int>(type: "integer", nullable: false),
                    MinimumAmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpReminderTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFinanceChargeMemoLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinanceChargeMemoHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    OverdueDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpFinanceChargeMemoLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpFinanceChargeMemoLines_ErpFinanceChargeMemoHeaders_Finan~",
                        column: x => x.FinanceChargeMemoHeaderId,
                        principalTable: "ErpFinanceChargeMemoHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedDeferralLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedDeferralHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPostedDeferralLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPostedDeferralLines_ErpPostedDeferralHeaders_PostedDefer~",
                        column: x => x.PostedDeferralHeaderId,
                        principalTable: "ErpPostedDeferralHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpReminderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    OverdueDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpReminderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpReminderLines_ErpReminderHeaders_ReminderHeaderId",
                        column: x => x.ReminderHeaderId,
                        principalTable: "ErpReminderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpVendorLedgerEntries_CompanyId_VendorId_Open",
                table: "ErpVendorLedgerEntries",
                columns: new[] { "CompanyId", "VendorId", "Open" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpDetailedVendorLedgEntries_ApplicationNo",
                table: "ErpDetailedVendorLedgEntries",
                column: "ApplicationNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpDetailedVendorLedgEntries_VendorLedgerEntryNo",
                table: "ErpDetailedVendorLedgEntries",
                column: "VendorLedgerEntryNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpDetailedCustLedgEntries_ApplicationNo",
                table: "ErpDetailedCustLedgEntries",
                column: "ApplicationNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpDetailedCustLedgEntries_CustLedgerEntryNo",
                table: "ErpDetailedCustLedgEntries",
                column: "CustLedgerEntryNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpCustomerLedgerEntries_CompanyId_CustomerId_Open",
                table: "ErpCustomerLedgerEntries",
                columns: new[] { "CompanyId", "CustomerId", "Open" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccountLedgerEntries_CompanyId_BankAccountNo_Stateme~",
                table: "ErpBankAccountLedgerEntries",
                columns: new[] { "CompanyId", "BankAccountNo", "StatementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCashFlowForecastEntries_CompanyId_CashFlowForecastNo_Cas~",
                table: "ErpCashFlowForecastEntries",
                columns: new[] { "CompanyId", "CashFlowForecastNo", "CashFlowDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCashFlowForecasts_TenantId_CompanyId_No",
                table: "ErpCashFlowForecasts",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCashFlowManualEntries_TenantId_CompanyId_Code",
                table: "ErpCashFlowManualEntries",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCashFlowWorksheetLines_CompanyId_CashFlowForecastNo_Line~",
                table: "ErpCashFlowWorksheetLines",
                columns: new[] { "CompanyId", "CashFlowForecastNo", "LineNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpDeferralTemplates_TenantId_CompanyId_Code",
                table: "ErpDeferralTemplates",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFinanceChargeMemoHeaders_TenantId_CompanyId_No",
                table: "ErpFinanceChargeMemoHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFinanceChargeMemoLines_FinanceChargeMemoHeaderId",
                table: "ErpFinanceChargeMemoLines",
                column: "FinanceChargeMemoHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpFinanceChargeTerms_TenantId_CompanyId_Code",
                table: "ErpFinanceChargeTerms",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedDeferralHeaders_CompanyId_DocumentNo",
                table: "ErpPostedDeferralHeaders",
                columns: new[] { "CompanyId", "DocumentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedDeferralLines_PostedDeferralHeaderId",
                table: "ErpPostedDeferralLines",
                column: "PostedDeferralHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpReminderHeaders_TenantId_CompanyId_No",
                table: "ErpReminderHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpReminderLevels_CompanyId_ReminderTermsCode_No",
                table: "ErpReminderLevels",
                columns: new[] { "CompanyId", "ReminderTermsCode", "No" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpReminderLines_ReminderHeaderId",
                table: "ErpReminderLines",
                column: "ReminderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpReminderTerms_TenantId_CompanyId_Code",
                table: "ErpReminderTerms",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpCashFlowForecastEntries");

            migrationBuilder.DropTable(
                name: "ErpCashFlowForecasts");

            migrationBuilder.DropTable(
                name: "ErpCashFlowManualEntries");

            migrationBuilder.DropTable(
                name: "ErpCashFlowWorksheetLines");

            migrationBuilder.DropTable(
                name: "ErpDeferralTemplates");

            migrationBuilder.DropTable(
                name: "ErpFinanceChargeMemoLines");

            migrationBuilder.DropTable(
                name: "ErpFinanceChargeTerms");

            migrationBuilder.DropTable(
                name: "ErpPostedDeferralLines");

            migrationBuilder.DropTable(
                name: "ErpReminderLevels");

            migrationBuilder.DropTable(
                name: "ErpReminderLines");

            migrationBuilder.DropTable(
                name: "ErpReminderTerms");

            migrationBuilder.DropTable(
                name: "ErpFinanceChargeMemoHeaders");

            migrationBuilder.DropTable(
                name: "ErpPostedDeferralHeaders");

            migrationBuilder.DropTable(
                name: "ErpReminderHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ErpVendorLedgerEntries_CompanyId_VendorId_Open",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpDetailedVendorLedgEntries_ApplicationNo",
                table: "ErpDetailedVendorLedgEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpDetailedVendorLedgEntries_VendorLedgerEntryNo",
                table: "ErpDetailedVendorLedgEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpDetailedCustLedgEntries_ApplicationNo",
                table: "ErpDetailedCustLedgEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpDetailedCustLedgEntries_CustLedgerEntryNo",
                table: "ErpDetailedCustLedgEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpCustomerLedgerEntries_CompanyId_CustomerId_Open",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpBankAccountLedgerEntries_CompanyId_BankAccountNo_Stateme~",
                table: "ErpBankAccountLedgerEntries");

            migrationBuilder.DropColumn(
                name: "PaymentDiscCreditAccNo",
                table: "ErpVendorPostingGroups");

            migrationBuilder.DropColumn(
                name: "PaymentDiscDebitAccNo",
                table: "ErpVendorPostingGroups");

            migrationBuilder.DropColumn(
                name: "OriginalPmtDiscPossible",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "PmtDiscountDate",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "RemainingPmtDiscPossible",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "FinChargeMemoNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "IssuedFinChargeMemoNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "IssuedReminderNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "ReminderNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "DeferralCode",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "DeferralCode",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "PaymentDiscountPct",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "PmtDiscountDate",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "AdditionalFeeAccountNo",
                table: "ErpCustomerPostingGroups");

            migrationBuilder.DropColumn(
                name: "InterestAccountNo",
                table: "ErpCustomerPostingGroups");

            migrationBuilder.DropColumn(
                name: "PaymentDiscCreditAccNo",
                table: "ErpCustomerPostingGroups");

            migrationBuilder.DropColumn(
                name: "PaymentDiscDebitAccNo",
                table: "ErpCustomerPostingGroups");

            migrationBuilder.DropColumn(
                name: "LastIssuedReminderLevel",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "OriginalPmtDiscPossible",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "PmtDiscountDate",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "RemainingPmtDiscPossible",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "StatementLineNo",
                table: "ErpBankAccountLedgerEntries");

            migrationBuilder.DropColumn(
                name: "StatementNo",
                table: "ErpBankAccountLedgerEntries");

            migrationBuilder.DropColumn(
                name: "StatementStatus",
                table: "ErpBankAccountLedgerEntries");
        }
    }
}
