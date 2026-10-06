using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Pension_Administration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExcessContributionAllocation",
                table: "ErpPensionSetups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IncrementNos",
                table: "ErpPensionSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LifeCertificateFrequencyMonths",
                table: "ErpPensionSetups",
                type: "integer",
                nullable: false,
                // Existing setups ask for a life certificate every year, as new ones do.
                defaultValue: 12);

            migrationBuilder.AddColumn<string>(
                name: "TransfersInAccountNo",
                table: "ErpPensionSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ArrearsAmount",
                table: "ErpPensionPayrollLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ArrearsMonths",
                table: "ErpPensionPayrollLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PersonalRelief",
                table: "ErpPensionPayrollHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxTableCode",
                table: "ErpPensionPayrollHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ArrearsAmount",
                table: "ErpPensioners",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ArrearsMonths",
                table: "ErpPensioners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLifeCertificateDate",
                table: "ErpPensioners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LifeCertificateDueDate",
                table: "ErpPensioners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "ErpPensioners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxExempt",
                table: "ErpPensioners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyVestingScale",
                table: "ErpExitReasons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ErpMemberSalaryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SponsorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Period = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Salary = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpMemberSalaryEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMemberStatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SponsorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpMemberStatusEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionBeneficiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Relationship = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NationalId = table.Column<string>(type: "text", nullable: true),
                    BenefitPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GuardianName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    BankAccountNo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionBeneficiaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionContributionRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SponsorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmployeeRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionContributionRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionerChangeEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PensionerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldMonthlyPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NewMonthlyPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionerChangeEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionIncrements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IncrementPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MinimumMonthlyPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedBy = table.Column<string>(type: "text", nullable: true),
                    NoOfPensioners = table.Column<int>(type: "integer", nullable: false),
                    TotalMonthlyIncrease = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalArrears = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionIncrements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionTaxReliefLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MonthlyLimit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionTaxReliefLimits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionVestingScales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SponsorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FromServiceYears = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EmployerVestedPct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionVestingScales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberSalaryEntries_TenantId_CompanyId_MemberNo_Period",
                table: "ErpMemberSalaryEntries",
                columns: new[] { "TenantId", "CompanyId", "MemberNo", "Period" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberStatusEntries_CompanyId_MemberNo_EffectiveDate",
                table: "ErpMemberStatusEntries",
                columns: new[] { "CompanyId", "MemberNo", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpMemberStatusEntries_CompanyId_SchemeCode_EffectiveDate",
                table: "ErpMemberStatusEntries",
                columns: new[] { "CompanyId", "SchemeCode", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionBeneficiaries_TenantId_CompanyId_MemberNo_LineNo",
                table: "ErpPensionBeneficiaries",
                columns: new[] { "TenantId", "CompanyId", "MemberNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionContributionRates_CompanyId_SponsorNo_StartDate",
                table: "ErpPensionContributionRates",
                columns: new[] { "CompanyId", "SponsorNo", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionerChangeEntries_CompanyId_PensionerNo_EffectiveDa~",
                table: "ErpPensionerChangeEntries",
                columns: new[] { "CompanyId", "PensionerNo", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionIncrements_TenantId_CompanyId_No",
                table: "ErpPensionIncrements",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionTaxReliefLimits_TenantId_CompanyId_EffectiveDate",
                table: "ErpPensionTaxReliefLimits",
                columns: new[] { "TenantId", "CompanyId", "EffectiveDate" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionVestingScales_TenantId_CompanyId_SponsorNo_FromSe~",
                table: "ErpPensionVestingScales",
                columns: new[] { "TenantId", "CompanyId", "SponsorNo", "FromServiceYears" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpMemberSalaryEntries");

            migrationBuilder.DropTable(
                name: "ErpMemberStatusEntries");

            migrationBuilder.DropTable(
                name: "ErpPensionBeneficiaries");

            migrationBuilder.DropTable(
                name: "ErpPensionContributionRates");

            migrationBuilder.DropTable(
                name: "ErpPensionerChangeEntries");

            migrationBuilder.DropTable(
                name: "ErpPensionIncrements");

            migrationBuilder.DropTable(
                name: "ErpPensionTaxReliefLimits");

            migrationBuilder.DropTable(
                name: "ErpPensionVestingScales");

            migrationBuilder.DropColumn(
                name: "ExcessContributionAllocation",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "IncrementNos",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "LifeCertificateFrequencyMonths",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "TransfersInAccountNo",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "ArrearsAmount",
                table: "ErpPensionPayrollLines");

            migrationBuilder.DropColumn(
                name: "ArrearsMonths",
                table: "ErpPensionPayrollLines");

            migrationBuilder.DropColumn(
                name: "PersonalRelief",
                table: "ErpPensionPayrollHeaders");

            migrationBuilder.DropColumn(
                name: "TaxTableCode",
                table: "ErpPensionPayrollHeaders");

            migrationBuilder.DropColumn(
                name: "ArrearsAmount",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "ArrearsMonths",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "LastLifeCertificateDate",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "LifeCertificateDueDate",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "TaxExempt",
                table: "ErpPensioners");

            migrationBuilder.DropColumn(
                name: "ApplyVestingScale",
                table: "ErpExitReasons");
        }
    }
}
