using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Pension_Setups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultPayModeCode",
                table: "ErpPensionSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TrivialPensionLimit",
                table: "ErpPensionSetups",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PensionableSalaryBasis",
                table: "ErpPensionSchemes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SalaryAveragingYears",
                table: "ErpPensionSchemes",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<decimal>(
                name: "Deductions",
                table: "ErpPensionPayrollLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPension",
                table: "ErpPensionPayrollLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherEarnings",
                table: "ErpPensionPayrollLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PayModeCode",
                table: "ErpPensionPayrollLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                table: "ErpPensionPayrollHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReasonCode",
                table: "ErpPensionIncrements",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranchCode",
                table: "ErpPensioners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "ErpPensioners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayModeCode",
                table: "ErpPensioners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReasonCode",
                table: "ErpPensioners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferSchemeCode",
                table: "ErpPensionContributionHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AgeFactor",
                table: "ErpPensionBenefitCalculations",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<bool>(
                name: "Trivial",
                table: "ErpPensionBenefitCalculations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ErpExitReasonDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExitReasonCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mandatory = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpExitReasonDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMemberExitDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExitNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Received = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpMemberExitDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpOtherPensionSchemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulatorReferenceNo = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    BankCode = table.Column<string>(type: "text", nullable: true),
                    BankBranchCode = table.Column<string>(type: "text", nullable: true),
                    BankAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpOtherPensionSchemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionAgeFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FactorType = table.Column<int>(type: "integer", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    MaleFactor = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FemaleFactor = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionAgeFactors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionBankBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BranchCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SwiftCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionBankBranches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionBanks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SwiftCode = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionBanks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionerPayItemAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PensionerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayItemCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionerPayItemAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionerPayItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Calculation = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Pct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Taxable = table.Column<bool>(type: "boolean", nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionerPayItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionerPayModes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionerPayModes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionerSuspensionReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LifeCertificate = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionerSuspensionReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionPayrollLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    PensionerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayItemCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Taxable = table.Column<bool>(type: "boolean", nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionPayrollLineItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionRevisionReasons",
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
                    table.PrimaryKey("PK_ErpPensionRevisionReasons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpExitReasonDocuments_TenantId_CompanyId_ExitReasonCode_Li~",
                table: "ErpExitReasonDocuments",
                columns: new[] { "TenantId", "CompanyId", "ExitReasonCode", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberExitDocuments_TenantId_CompanyId_ExitNo_LineNo",
                table: "ErpMemberExitDocuments",
                columns: new[] { "TenantId", "CompanyId", "ExitNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpOtherPensionSchemes_TenantId_CompanyId_Code",
                table: "ErpOtherPensionSchemes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionAgeFactors_TenantId_CompanyId_SchemeCode_FactorTy~",
                table: "ErpPensionAgeFactors",
                columns: new[] { "TenantId", "CompanyId", "SchemeCode", "FactorType", "Age" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionBankBranches_TenantId_CompanyId_BankCode_BranchCo~",
                table: "ErpPensionBankBranches",
                columns: new[] { "TenantId", "CompanyId", "BankCode", "BranchCode" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionBanks_TenantId_CompanyId_Code",
                table: "ErpPensionBanks",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionerPayItemAssignments_CompanyId_PensionerNo_PayIte~",
                table: "ErpPensionerPayItemAssignments",
                columns: new[] { "CompanyId", "PensionerNo", "PayItemCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionerPayItems_TenantId_CompanyId_Code",
                table: "ErpPensionerPayItems",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionerPayModes_TenantId_CompanyId_Code",
                table: "ErpPensionerPayModes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionerSuspensionReasons_TenantId_CompanyId_Code",
                table: "ErpPensionerSuspensionReasons",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionPayrollLineItems_CompanyId_DocumentNo_LineNo",
                table: "ErpPensionPayrollLineItems",
                columns: new[] { "CompanyId", "DocumentNo", "LineNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionRevisionReasons_TenantId_CompanyId_Code",
                table: "ErpPensionRevisionReasons",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            // Lines made before earnings existed: their gross was the month's pension plus arrears.
            migrationBuilder.Sql("UPDATE \"ErpPensionPayrollLines\" SET \"MonthlyPension\" = \"GrossPension\" - \"ArrearsAmount\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpExitReasonDocuments");

            migrationBuilder.DropTable(
                name: "ErpMemberExitDocuments");

            migrationBuilder.DropTable(
                name: "ErpOtherPensionSchemes");

            migrationBuilder.DropTable(
                name: "ErpPensionAgeFactors");

            migrationBuilder.DropTable(
                name: "ErpPensionBankBranches");

            migrationBuilder.DropTable(
                name: "ErpPensionBanks");

            migrationBuilder.DropTable(
                name: "ErpPensionerPayItemAssignments");

            migrationBuilder.DropTable(
                name: "ErpPensionerPayItems");

            migrationBuilder.DropTable(
                name: "ErpPensionerPayModes");

            migrationBuilder.DropTable(
                name: "ErpPensionerSuspensionReasons");

            migrationBuilder.DropTable(
                name: "ErpPensionPayrollLineItems");

            migrationBuilder.DropTable(
                name: "ErpPensionRevisionReasons");

            migrationBuilder.DropColumn(
                name: "DefaultPayModeCode",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "TrivialPensionLimit",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "PensionableSalaryBasis",
                table: "ErpPensionSchemes");

            migrationBuilder.DropColumn(
                name: "SalaryAveragingYears",
                table: "ErpPensionSchemes");

            migrationBuilder.DropColumn(
                name: "Deductions",
                table: "ErpPensionPayrollLines");

            migrationBuilder.DropColumn(
                name: "MonthlyPension",
                table: "ErpPensionPayrollLines");

            migrationBuilder.DropColumn(
                name: "OtherEarnings",
                table: "ErpPensionPayrollLines");

            migrationBuilder.DropColumn(
                name: "PayModeCode",
                table: "ErpPensionPayrollLines");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                table: "ErpPensionPayrollHeaders");

            migrationBuilder.DropColumn(
                name: "ReasonCode",
                table: "ErpPensionIncrements");

            migrationBuilder.DropColumn(
                name: "BankBranchCode",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "PayModeCode",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "SuspensionReasonCode",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "TransferSchemeCode",
                table: "ErpPensionContributionHeaders");

            migrationBuilder.DropColumn(
                name: "AgeFactor",
                table: "ErpPensionBenefitCalculations");

            migrationBuilder.DropColumn(
                name: "Trivial",
                table: "ErpPensionBenefitCalculations");
        }
    }
}
