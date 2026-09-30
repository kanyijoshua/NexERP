using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class ForeignCurrency_VatSettlement_EmployeeLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountLcy",
                table: "ErpVendorLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ClosedByEntryNo",
                table: "ErpVendorLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ErpVendorLedgerEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmountLcy",
                table: "ErpVendorLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ClosedByEntryNo",
                table: "ErpVatEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FromEmployeeEntryNo",
                table: "ErpGLRegisters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ToEmployeeEntryNo",
                table: "ErpGLRegisters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "GenPostingType",
                table: "ErpGLAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VatBusPostingGroup",
                table: "ErpGLAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountLcy",
                table: "ErpGenJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BalGenPostingType",
                table: "ErpGenJournalLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BalVatAmount",
                table: "ErpGenJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalVatBaseAmount",
                table: "ErpGenJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BalVatBusPostingGroup",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BalVatProdPostingGroup",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrencyFactor",
                table: "ErpGenJournalLines",
                type: "numeric(28,15)",
                precision: 28,
                scale: 15,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GenPostingType",
                table: "ErpGenJournalLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "ErpGenJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatBaseAmount",
                table: "ErpGenJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatBusPostingGroup",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatProdPostingGroup",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "ErpEmployees",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountLcy",
                table: "ErpCustomerLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ClosedByEntryNo",
                table: "ErpCustomerLedgerEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ErpCustomerLedgerEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmountLcy",
                table: "ErpCustomerLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnrealizedGainsAccountNo",
                table: "ErpCurrencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnrealizedLossesAccountNo",
                table: "ErpCurrencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceLcy",
                table: "ErpBankAccounts",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountLcy",
                table: "ErpBankAccountLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ErpBankAccountLedgerEntries",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpEmployeeLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "text", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Open = table.Column<bool>(type: "boolean", nullable: false),
                    DimensionSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosedByEntryNo = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ErpEmployeeLedgerEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpExchRateAdjmtRegisters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentNo = table.Column<string>(type: "text", nullable: true),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    AccountNo = table.Column<string>(type: "text", nullable: true),
                    CurrencyCode = table.Column<string>(type: "text", nullable: true),
                    CurrencyFactor = table.Column<decimal>(type: "numeric(28,15)", precision: 28, scale: 15, nullable: false),
                    AdjustedBase = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AdjustedBaseLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AdjustedAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GLRegisterNo = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ErpExchRateAdjmtRegisters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeLedgerEntries_CompanyId_EntryNo",
                table: "ErpEmployeeLedgerEntries",
                columns: new[] { "CompanyId", "EntryNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeLedgerEntries_EmployeeId",
                table: "ErpEmployeeLedgerEntries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeLedgerEntries_RegisterNo",
                table: "ErpEmployeeLedgerEntries",
                column: "RegisterNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpExchRateAdjmtRegisters_GLRegisterNo",
                table: "ErpExchRateAdjmtRegisters",
                column: "GLRegisterNo");

            // Everything posted before foreign currencies existed was in LCY: its LCY amounts are its amounts.
            migrationBuilder.Sql("UPDATE \"ErpCustomerLedgerEntries\" SET \"AmountLcy\" = \"Amount\", \"RemainingAmountLcy\" = \"RemainingAmount\";");
            migrationBuilder.Sql("UPDATE \"ErpVendorLedgerEntries\" SET \"AmountLcy\" = \"Amount\", \"RemainingAmountLcy\" = \"RemainingAmount\";");
            migrationBuilder.Sql("UPDATE \"ErpBankAccountLedgerEntries\" SET \"AmountLcy\" = \"Amount\";");
            migrationBuilder.Sql("UPDATE \"ErpBankAccounts\" SET \"BalanceLcy\" = \"Balance\";");
            migrationBuilder.Sql("UPDATE \"ErpGenJournalLines\" SET \"CurrencyFactor\" = 1, \"AmountLcy\" = \"Amount\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpEmployeeLedgerEntries");

            migrationBuilder.DropTable(
                name: "ErpExchRateAdjmtRegisters");

            migrationBuilder.DropColumn(
                name: "AmountLcy",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ClosedByEntryNo",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "RemainingAmountLcy",
                table: "ErpVendorLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ClosedByEntryNo",
                table: "ErpVatEntries");

            migrationBuilder.DropColumn(
                name: "FromEmployeeEntryNo",
                table: "ErpGLRegisters");

            migrationBuilder.DropColumn(
                name: "ToEmployeeEntryNo",
                table: "ErpGLRegisters");

            migrationBuilder.DropColumn(
                name: "GenPostingType",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "VatBusPostingGroup",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "AmountLcy",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "BalGenPostingType",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "BalVatAmount",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "BalVatBaseAmount",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "BalVatBusPostingGroup",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "BalVatProdPostingGroup",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "CurrencyFactor",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "GenPostingType",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "VatBaseAmount",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "VatBusPostingGroup",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "VatProdPostingGroup",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "AmountLcy",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ClosedByEntryNo",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "RemainingAmountLcy",
                table: "ErpCustomerLedgerEntries");

            migrationBuilder.DropColumn(
                name: "UnrealizedGainsAccountNo",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "UnrealizedLossesAccountNo",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "BalanceLcy",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "AmountLcy",
                table: "ErpBankAccountLedgerEntries");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ErpBankAccountLedgerEntries");
        }
    }
}
