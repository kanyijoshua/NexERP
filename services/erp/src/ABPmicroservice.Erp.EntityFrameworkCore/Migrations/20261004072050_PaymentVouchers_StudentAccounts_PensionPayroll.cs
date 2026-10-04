using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class PaymentVouchers_StudentAccounts_PensionPayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayrollNos",
                table: "ErpPensionSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PensionerNos",
                table: "ErpPensionSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PensionsPaidAccountNo",
                table: "ErpPensionSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentVoucherNo",
                table: "ErpMemberExits",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusChangeNos",
                table: "ErpAcademicSetups",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpCashManagementSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentVoucherNos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpCashManagementSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPaymentDeductionCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionType = table.Column<int>(type: "integer", nullable: false),
                    RatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    PayableAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPaymentDeductionCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPaymentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VatRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    WithholdingTaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WithholdingVatCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RetentionCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPaymentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPaymentVoucherHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PayingBankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Payee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OnBehalfOf = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentNarration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChequeNo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalWithholdingTaxAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalWithholdingVatAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalRetentionAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalNetAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NoOfLines = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPaymentVoucherHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPaymentVoucherLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    PaymentTypeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AppliesToDocNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    VatRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    WithholdingTaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WithholdingTaxAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    WithholdingVatCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WithholdingVatAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RetentionCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RetentionAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPaymentVoucherLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensioners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MemberNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NationalId = table.Column<string>(type: "text", nullable: true),
                    TaxPinNo = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MonthlyPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    BankBranch = table.Column<string>(type: "text", nullable: true),
                    BankAccountNo = table.Column<string>(type: "text", nullable: true),
                    LastPaidPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpPensioners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionPayrollHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SchemeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PayPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TaxRatePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxFreeAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<string>(type: "text", nullable: true),
                    TotalGross = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NoOfPensioners = table.Column<int>(type: "integer", nullable: false),
                    PaymentVoucherNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpPensionPayrollHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPensionPayrollLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    PensionerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PensionerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GrossPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NetPension = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpPensionPayrollLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudentReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PayMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ExternalDocumentNo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AppliesToBillNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpStudentReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudentRefunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    PaymentVoucherNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpStudentRefunds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpStudentStatusChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: true),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResumeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PreviousStatus = table.Column<int>(type: "integer", nullable: false),
                    NewStatus = table.Column<int>(type: "integer", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ErpStudentStatusChanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCashManagementSetups_TenantId_CompanyId",
                table: "ErpCashManagementSetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPaymentDeductionCodes_TenantId_CompanyId_Code",
                table: "ErpPaymentDeductionCodes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPaymentTypes_TenantId_CompanyId_Code",
                table: "ErpPaymentTypes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPaymentVoucherHeaders_CompanyId_Status_PostingDate",
                table: "ErpPaymentVoucherHeaders",
                columns: new[] { "CompanyId", "Status", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPaymentVoucherHeaders_TenantId_CompanyId_No",
                table: "ErpPaymentVoucherHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPaymentVoucherLines_TenantId_CompanyId_DocumentNo_LineNo",
                table: "ErpPaymentVoucherLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensioners_CompanyId_SchemeCode",
                table: "ErpPensioners",
                columns: new[] { "CompanyId", "SchemeCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensioners_TenantId_CompanyId_No",
                table: "ErpPensioners",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionPayrollHeaders_CompanyId_SchemeCode_PayPeriod",
                table: "ErpPensionPayrollHeaders",
                columns: new[] { "CompanyId", "SchemeCode", "PayPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionPayrollHeaders_TenantId_CompanyId_No",
                table: "ErpPensionPayrollHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPensionPayrollLines_TenantId_CompanyId_DocumentNo_LineNo",
                table: "ErpPensionPayrollLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentReceipts_CompanyId_StudentNo",
                table: "ErpStudentReceipts",
                columns: new[] { "CompanyId", "StudentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentReceipts_TenantId_CompanyId_No",
                table: "ErpStudentReceipts",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentRefunds_CompanyId_StudentNo",
                table: "ErpStudentRefunds",
                columns: new[] { "CompanyId", "StudentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentRefunds_TenantId_CompanyId_No",
                table: "ErpStudentRefunds",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentStatusChanges_CompanyId_StudentNo",
                table: "ErpStudentStatusChanges",
                columns: new[] { "CompanyId", "StudentNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpStudentStatusChanges_TenantId_CompanyId_No",
                table: "ErpStudentStatusChanges",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpCashManagementSetups");

            migrationBuilder.DropTable(
                name: "ErpPaymentDeductionCodes");

            migrationBuilder.DropTable(
                name: "ErpPaymentTypes");

            migrationBuilder.DropTable(
                name: "ErpPaymentVoucherHeaders");

            migrationBuilder.DropTable(
                name: "ErpPaymentVoucherLines");

            migrationBuilder.DropTable(
                name: "ErpPensioners");

            migrationBuilder.DropTable(
                name: "ErpPensionPayrollHeaders");

            migrationBuilder.DropTable(
                name: "ErpPensionPayrollLines");

            migrationBuilder.DropTable(
                name: "ErpStudentReceipts");

            migrationBuilder.DropTable(
                name: "ErpStudentRefunds");

            migrationBuilder.DropTable(
                name: "ErpStudentStatusChanges");

            migrationBuilder.DropColumn(
                name: "PayrollNos",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "PensionerNos",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "PensionsPaidAccountNo",
                table: "ErpPensionSetups");

            migrationBuilder.DropColumn(
                name: "PaymentVoucherNo",
                table: "ErpMemberExits");

            migrationBuilder.DropColumn(
                name: "ReceiptNos",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "RefundNos",
                table: "ErpAcademicSetups");

            migrationBuilder.DropColumn(
                name: "StatusChangeNos",
                table: "ErpAcademicSetups");
        }
    }
}
