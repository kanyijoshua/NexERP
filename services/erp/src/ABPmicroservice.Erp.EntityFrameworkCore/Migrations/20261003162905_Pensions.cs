using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Pensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErpExitReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOption = table.Column<int>(type: "integer", nullable: false),
                    EmployerPortionPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxTableCode = table.Column<string>(type: "text", nullable: true),
                    LumpsumTaxFree = table.Column<bool>(type: "boolean", nullable: false),
                    StatusAfterExit = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpExitReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLumpsumTaxBands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxTableCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LowerLimit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UpperLimit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpLumpsumTaxBands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLumpsumTaxTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnnualTaxFreeAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaxTaxFreeAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaxAgeTaxable = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpLumpsumTaxTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMemberExits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MemberName = table.Column<string>(type: "text", nullable: true),
                    SchemeCode = table.Column<string>(type: "text", nullable: true),
                    SponsorNo = table.Column<string>(type: "text", nullable: true),
                    ReasonCode = table.Column<string>(type: "text", nullable: true),
                    WithdrawalType = table.Column<int>(type: "integer", nullable: false),
                    ExitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfCalculation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    AgeAtExit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ServiceYears = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployeeBalance = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerBalance = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployeePayable = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerPayable = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DeferredAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RegisteredPayable = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnregisteredPayable = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GrossLumpsum = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxFreeAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxOnLumpsum = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NetPayable = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpMemberExits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMemberLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SponsorNo = table.Column<string>(type: "text", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContributionPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    ContributionType = table.Column<int>(type: "integer", nullable: false),
                    ContributionMode = table.Column<int>(type: "integer", nullable: false),
                    ExemptionType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Salary = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DimensionSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    RegisterNo = table.Column<long>(type: "bigint", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpMemberLedgerEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionContributionHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SchemeCode = table.Column<string>(type: "text", nullable: true),
                    SponsorNo = table.Column<string>(type: "text", nullable: true),
                    SponsorName = table.Column<string>(type: "text", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContributionPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ContributionMode = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NoOfMembers = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionContributionHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionContributionLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MemberName = table.Column<string>(type: "text", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployeeTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployeeNonTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployeeAvcTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployeeAvcNonTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerNonTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerAvcTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerAvcNonTaxExempt = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionContributionLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionInterestRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDeclared = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegisteredRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnregisteredRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Posted = table.Column<bool>(type: "boolean", nullable: false),
                    PostedDocumentNo = table.Column<string>(type: "text", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalInterest = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionInterestRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SponsorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    OtherName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NationalId = table.Column<string>(type: "text", nullable: true),
                    TaxPinNo = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaritalStatus = table.Column<int>(type: "integer", nullable: false),
                    PayrollNo = table.Column<string>(type: "text", nullable: true),
                    Designation = table.Column<string>(type: "text", nullable: true),
                    DateOfEmployment = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JoinSchemeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentSalary = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ExpectedRetirementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ContributionStatus = table.Column<int>(type: "integer", nullable: false),
                    ExitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    BankBranch = table.Column<string>(type: "text", nullable: true),
                    BankAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionSchemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemeType = table.Column<int>(type: "integer", nullable: false),
                    PlanType = table.Column<int>(type: "integer", nullable: false),
                    SchemeMode = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InterestCalculationMode = table.Column<int>(type: "integer", nullable: false),
                    RegulatorReferenceNo = table.Column<string>(type: "text", nullable: true),
                    TaxPinNo = table.Column<string>(type: "text", nullable: true),
                    NormalRetirementAge = table.Column<int>(type: "integer", nullable: false),
                    MinimumRetirementAge = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionSchemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemeDimensionCode = table.Column<string>(type: "text", nullable: true),
                    MemberNos = table.Column<string>(type: "text", nullable: true),
                    SponsorNos = table.Column<string>(type: "text", nullable: true),
                    ContributionNos = table.Column<string>(type: "text", nullable: true),
                    InterestBatchNos = table.Column<string>(type: "text", nullable: true),
                    ExitNos = table.Column<string>(type: "text", nullable: true),
                    MemberFundsAccountNo = table.Column<string>(type: "text", nullable: true),
                    ContributionAccrualAccountNo = table.Column<string>(type: "text", nullable: true),
                    InterestAccountNo = table.Column<string>(type: "text", nullable: true),
                    BenefitsPayableAccountNo = table.Column<string>(type: "text", nullable: true),
                    TaxAccountNo = table.Column<string>(type: "text", nullable: true),
                    AllowContributionDuplication = table.Column<bool>(type: "boolean", nullable: false),
                    NoOfDaysInAYear = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionSponsors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerNo = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Contact = table.Column<string>(type: "text", nullable: true),
                    TaxPinNo = table.Column<string>(type: "text", nullable: true),
                    EmployeeRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LastScheduleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionSponsors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpExitReasons_TenantId_CompanyId_Code",
                table: "ErpExitReasons",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLumpsumTaxBands_TenantId_CompanyId_TaxTableCode_LowerLim~",
                table: "ErpLumpsumTaxBands",
                columns: new[] { "TenantId", "CompanyId", "TaxTableCode", "LowerLimit" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLumpsumTaxTables_TenantId_CompanyId_Code",
                table: "ErpLumpsumTaxTables",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberExits_CompanyId_MemberNo",
                table: "ErpMemberExits",
                columns: new[] { "CompanyId", "MemberNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberExits_TenantId_CompanyId_No",
                table: "ErpMemberExits",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberLedgerEntries_CompanyId_EntryNo",
                table: "ErpMemberLedgerEntries",
                columns: new[] { "CompanyId", "EntryNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberLedgerEntries_CompanyId_MemberNo_PostingDate",
                table: "ErpMemberLedgerEntries",
                columns: new[] { "CompanyId", "MemberNo", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberLedgerEntries_CompanyId_SchemeCode_PostingDate",
                table: "ErpMemberLedgerEntries",
                columns: new[] { "CompanyId", "SchemeCode", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionContributionHeaders_TenantId_CompanyId_No",
                table: "ErpPensionContributionHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionContributionLines_TenantId_CompanyId_DocumentNo_L~",
                table: "ErpPensionContributionLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionInterestRates_CompanyId_SchemeCode_StartDate",
                table: "ErpPensionInterestRates",
                columns: new[] { "CompanyId", "SchemeCode", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionMembers_CompanyId_SchemeCode_SponsorNo",
                table: "ErpPensionMembers",
                columns: new[] { "CompanyId", "SchemeCode", "SponsorNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionMembers_TenantId_CompanyId_No",
                table: "ErpPensionMembers",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionSchemes_TenantId_CompanyId_Code",
                table: "ErpPensionSchemes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionSetups_TenantId_CompanyId",
                table: "ErpPensionSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionSponsors_TenantId_CompanyId_No",
                table: "ErpPensionSponsors",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpExitReasons");

            migrationBuilder.DropTable(
                name: "ErpLumpsumTaxBands");

            migrationBuilder.DropTable(
                name: "ErpLumpsumTaxTables");

            migrationBuilder.DropTable(
                name: "ErpMemberExits");

            migrationBuilder.DropTable(
                name: "ErpMemberLedgerEntries");

            migrationBuilder.DropTable(
                name: "ErpPensionContributionHeaders");

            migrationBuilder.DropTable(
                name: "ErpPensionContributionLines");

            migrationBuilder.DropTable(
                name: "ErpPensionInterestRates");

            migrationBuilder.DropTable(
                name: "ErpPensionMembers");

            migrationBuilder.DropTable(
                name: "ErpPensionSchemes");

            migrationBuilder.DropTable(
                name: "ErpPensionSetups");

            migrationBuilder.DropTable(
                name: "ErpPensionSponsors");
        }
    }
}
