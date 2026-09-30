using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Tax_CashManagement_HumanResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErpInventoryPostingSetups_TenantId_CompanyId_InventoryPosti~",
                table: "ErpInventoryPostingSetups");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodCode",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaserCode",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatBusPostingGroup",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditWarnings",
                table: "ErpSalesReceivablesSetups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ExtDocNoMandatory",
                table: "ErpSalesReceivablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatBaseAmount",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatBusPostingGroup",
                table: "ErpSalesLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VatCalculationType",
                table: "ErpSalesLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VatIdentifier",
                table: "ErpSalesLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatPercent",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatProdPostingGroup",
                table: "ErpSalesLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpSalesHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExtDocNoMandatory",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatBaseAmount",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatBusPostingGroup",
                table: "ErpPurchaseLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VatCalculationType",
                table: "ErpPurchaseLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VatIdentifier",
                table: "ErpPurchaseLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatPercent",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatProdPostingGroup",
                table: "ErpPurchaseLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatProdPostingGroup",
                table: "ErpItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpInventoryPostingSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FromBankEntryNo",
                table: "ErpGLRegisters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FromVatEntryNo",
                table: "ErpGLRegisters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ToBankEntryNo",
                table: "ErpGLRegisters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ToVatEntryNo",
                table: "ErpGLRegisters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "VatProdPostingGroup",
                table: "ErpGLAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNos",
                table: "ErpGeneralLedgerSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalespersonCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatBusPostingGroup",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpAccountingPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NewFiscalYear = table.Column<bool>(type: "boolean", nullable: false),
                    Closed = table.Column<bool>(type: "boolean", nullable: false),
                    DateLocked = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpAccountingPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBankAccountLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountNo = table.Column<string>(type: "text", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Open = table.Column<bool>(type: "boolean", nullable: false),
                    DimensionSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    RegisterNo = table.Column<long>(type: "bigint", nullable: false),
                    Reversed = table.Column<bool>(type: "boolean", nullable: false),
                    ReversedByEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    ReversedEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpBankAccountLedgerEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBankAccountPostingGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GLAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpBankAccountPostingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    BankAccountNo = table.Column<string>(type: "text", nullable: true),
                    BankBranchNo = table.Column<string>(type: "text", nullable: true),
                    Iban = table.Column<string>(type: "text", nullable: true),
                    SwiftCode = table.Column<string>(type: "text", nullable: true),
                    CurrencyCode = table.Column<string>(type: "text", nullable: true),
                    BankAccPostingGroup = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Contact = table.Column<string>(type: "text", nullable: true),
                    Balance = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpBankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCausesOfAbsence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitOfMeasureCode = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpCausesOfAbsence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCurrencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: true),
                    AmountRoundingPrecision = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RealizedGainsAccountNo = table.Column<string>(type: "text", nullable: true),
                    RealizedLossesAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpCurrencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCurrencyExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: true),
                    StartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExchangeRateAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RelationalExchangeRateAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpCurrencyExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmployeeAbsences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "text", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CauseOfAbsenceCode = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitOfMeasureCode = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpEmployeeAbsences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmployeePostingGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayablesAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpEmployeePostingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    JobTitle = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PostCode = table.Column<string>(type: "text", nullable: true),
                    CountryRegionCode = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    MobilePhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    CompanyEmail = table.Column<string>(type: "text", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SocialSecurityNo = table.Column<string>(type: "text", nullable: true),
                    EmploymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InactiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GroundsForTermCode = table.Column<string>(type: "text", nullable: true),
                    EmplymtContractCode = table.Column<string>(type: "text", nullable: true),
                    UnionCode = table.Column<string>(type: "text", nullable: true),
                    EmployeePostingGroup = table.Column<string>(type: "text", nullable: true),
                    BankAccountNo = table.Column<string>(type: "text", nullable: true),
                    Iban = table.Column<string>(type: "text", nullable: true),
                    SalespersPurchCode = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmploymentContracts",
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
                    table.PrimaryKey("PK_ErpEmploymentContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpGroundsForTermination",
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
                    table.PrimaryKey("PK_ErpGroundsForTermination", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpHumanResourcesSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNos = table.Column<string>(type: "text", nullable: true),
                    BaseUnitOfMeasure = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpHumanResourcesSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpHumanResourceUnitsOfMeasure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QtyPerUnitOfMeasure = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpHumanResourceUnitsOfMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpInventorySetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNos = table.Column<string>(type: "text", nullable: true),
                    LocationMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    PreventNegativeInventory = table.Column<bool>(type: "boolean", nullable: false),
                    AutomaticCostPosting = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpInventorySetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PostCode = table.Column<string>(type: "text", nullable: true),
                    CountryRegionCode = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Contact = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BalAccountType = table.Column<int>(type: "integer", nullable: true),
                    BalAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpPaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPaymentTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DueDateCalculation = table.Column<string>(type: "text", nullable: true),
                    DiscountDateCalculation = table.Column<string>(type: "text", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPaymentTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpQualifications",
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
                    table.PrimaryKey("PK_ErpQualifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSalespeoplePurchasers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    JobTitle = table.Column<string>(type: "text", nullable: true),
                    CommissionPercent = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSalespeoplePurchasers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpUnions",
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
                    table.PrimaryKey("PK_ErpUnions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVatBusinessPostingGroups",
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
                    table.PrimaryKey("PK_ErpVatBusinessPostingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVatEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Base = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    VatCalculationType = table.Column<int>(type: "integer", nullable: false),
                    BillToPayToNo = table.Column<string>(type: "text", nullable: true),
                    VatBusPostingGroup = table.Column<string>(type: "text", nullable: true),
                    VatProdPostingGroup = table.Column<string>(type: "text", nullable: true),
                    VatIdentifier = table.Column<string>(type: "text", nullable: true),
                    VatPercent = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    RegisterNo = table.Column<long>(type: "bigint", nullable: false),
                    Closed = table.Column<bool>(type: "boolean", nullable: false),
                    Reversed = table.Column<bool>(type: "boolean", nullable: false),
                    ReversedByEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    ReversedEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpVatEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVatPostingSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VatBusPostingGroup = table.Column<string>(type: "text", nullable: true),
                    VatProdPostingGroup = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    VatIdentifier = table.Column<string>(type: "text", nullable: true),
                    VatPercent = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    VatCalculationType = table.Column<int>(type: "integer", nullable: false),
                    SalesVatAccountNo = table.Column<string>(type: "text", nullable: true),
                    PurchaseVatAccountNo = table.Column<string>(type: "text", nullable: true),
                    ReverseChrgVatAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpVatPostingSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVatProductPostingGroups",
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
                    table.PrimaryKey("PK_ErpVatProductPostingGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpInventoryPostingSetups_TenantId_CompanyId_LocationCode_I~",
                table: "ErpInventoryPostingSetups",
                columns: new[] { "TenantId", "CompanyId", "LocationCode", "InventoryPostingGroup" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpAccountingPeriods_TenantId_CompanyId_StartingDate",
                table: "ErpAccountingPeriods",
                columns: new[] { "TenantId", "CompanyId", "StartingDate" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccountLedgerEntries_BankAccountId",
                table: "ErpBankAccountLedgerEntries",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccountLedgerEntries_CompanyId_EntryNo",
                table: "ErpBankAccountLedgerEntries",
                columns: new[] { "CompanyId", "EntryNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccountLedgerEntries_RegisterNo",
                table: "ErpBankAccountLedgerEntries",
                column: "RegisterNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccountPostingGroups_TenantId_CompanyId_Code",
                table: "ErpBankAccountPostingGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccounts_TenantId_CompanyId_No",
                table: "ErpBankAccounts",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCausesOfAbsence_TenantId_CompanyId_Code",
                table: "ErpCausesOfAbsence",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCurrencies_TenantId_CompanyId_Code",
                table: "ErpCurrencies",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCurrencyExchangeRates_TenantId_CompanyId_CurrencyCode_St~",
                table: "ErpCurrencyExchangeRates",
                columns: new[] { "TenantId", "CompanyId", "CurrencyCode", "StartingDate" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeAbsences_EmployeeId",
                table: "ErpEmployeeAbsences",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeePostingGroups_TenantId_CompanyId_Code",
                table: "ErpEmployeePostingGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployees_TenantId_CompanyId_No",
                table: "ErpEmployees",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmploymentContracts_TenantId_CompanyId_Code",
                table: "ErpEmploymentContracts",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpGroundsForTermination_TenantId_CompanyId_Code",
                table: "ErpGroundsForTermination",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpHumanResourcesSetups_TenantId_CompanyId",
                table: "ErpHumanResourcesSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpHumanResourceUnitsOfMeasure_TenantId_CompanyId_Code",
                table: "ErpHumanResourceUnitsOfMeasure",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpInventorySetups_TenantId_CompanyId",
                table: "ErpInventorySetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLocations_TenantId_CompanyId_Code",
                table: "ErpLocations",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPaymentMethods_TenantId_CompanyId_Code",
                table: "ErpPaymentMethods",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPaymentTerms_TenantId_CompanyId_Code",
                table: "ErpPaymentTerms",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpQualifications_TenantId_CompanyId_Code",
                table: "ErpQualifications",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSalespeoplePurchasers_TenantId_CompanyId_Code",
                table: "ErpSalespeoplePurchasers",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpUnions_TenantId_CompanyId_Code",
                table: "ErpUnions",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpVatBusinessPostingGroups_TenantId_CompanyId_Code",
                table: "ErpVatBusinessPostingGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpVatEntries_CompanyId_EntryNo",
                table: "ErpVatEntries",
                columns: new[] { "CompanyId", "EntryNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpVatEntries_RegisterNo",
                table: "ErpVatEntries",
                column: "RegisterNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpVatPostingSetups_TenantId_CompanyId_VatBusPostingGroup_V~",
                table: "ErpVatPostingSetups",
                columns: new[] { "TenantId", "CompanyId", "VatBusPostingGroup", "VatProdPostingGroup" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpVatProductPostingGroups_TenantId_CompanyId_Code",
                table: "ErpVatProductPostingGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpAccountingPeriods");

            migrationBuilder.DropTable(
                name: "ErpBankAccountLedgerEntries");

            migrationBuilder.DropTable(
                name: "ErpBankAccountPostingGroups");

            migrationBuilder.DropTable(
                name: "ErpBankAccounts");

            migrationBuilder.DropTable(
                name: "ErpCausesOfAbsence");

            migrationBuilder.DropTable(
                name: "ErpCurrencies");

            migrationBuilder.DropTable(
                name: "ErpCurrencyExchangeRates");

            migrationBuilder.DropTable(
                name: "ErpEmployeeAbsences");

            migrationBuilder.DropTable(
                name: "ErpEmployeePostingGroups");

            migrationBuilder.DropTable(
                name: "ErpEmployees");

            migrationBuilder.DropTable(
                name: "ErpEmploymentContracts");

            migrationBuilder.DropTable(
                name: "ErpGroundsForTermination");

            migrationBuilder.DropTable(
                name: "ErpHumanResourcesSetups");

            migrationBuilder.DropTable(
                name: "ErpHumanResourceUnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "ErpInventorySetups");

            migrationBuilder.DropTable(
                name: "ErpLocations");

            migrationBuilder.DropTable(
                name: "ErpPaymentMethods");

            migrationBuilder.DropTable(
                name: "ErpPaymentTerms");

            migrationBuilder.DropTable(
                name: "ErpQualifications");

            migrationBuilder.DropTable(
                name: "ErpSalespeoplePurchasers");

            migrationBuilder.DropTable(
                name: "ErpUnions");

            migrationBuilder.DropTable(
                name: "ErpVatBusinessPostingGroups");

            migrationBuilder.DropTable(
                name: "ErpVatEntries");

            migrationBuilder.DropTable(
                name: "ErpVatPostingSetups");

            migrationBuilder.DropTable(
                name: "ErpVatProductPostingGroups");

            migrationBuilder.DropIndex(
                name: "IX_ErpInventoryPostingSetups_TenantId_CompanyId_LocationCode_I~",
                table: "ErpInventoryPostingSetups");

            migrationBuilder.DropColumn(
                name: "PaymentMethodCode",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "PurchaserCode",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "VatBusPostingGroup",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "CreditWarnings",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "ExtDocNoMandatory",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "VatBaseAmount",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "VatBusPostingGroup",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "VatCalculationType",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "VatIdentifier",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "VatPercent",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "VatProdPostingGroup",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ExtDocNoMandatory",
                table: "ErpPurchasesPayablesSetups");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatBaseAmount",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatBusPostingGroup",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatCalculationType",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatIdentifier",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatPercent",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatProdPostingGroup",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VatProdPostingGroup",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "ErpInventoryPostingSetups");

            migrationBuilder.DropColumn(
                name: "FromBankEntryNo",
                table: "ErpGLRegisters");

            migrationBuilder.DropColumn(
                name: "FromVatEntryNo",
                table: "ErpGLRegisters");

            migrationBuilder.DropColumn(
                name: "ToBankEntryNo",
                table: "ErpGLRegisters");

            migrationBuilder.DropColumn(
                name: "ToVatEntryNo",
                table: "ErpGLRegisters");

            migrationBuilder.DropColumn(
                name: "VatProdPostingGroup",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "BankAccountNos",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "PaymentMethodCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "SalespersonCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "VatBusPostingGroup",
                table: "ErpCustomers");

            migrationBuilder.CreateIndex(
                name: "IX_ErpInventoryPostingSetups_TenantId_CompanyId_InventoryPosti~",
                table: "ErpInventoryPostingSetups",
                columns: new[] { "TenantId", "CompanyId", "InventoryPostingGroup" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }
    }
}
