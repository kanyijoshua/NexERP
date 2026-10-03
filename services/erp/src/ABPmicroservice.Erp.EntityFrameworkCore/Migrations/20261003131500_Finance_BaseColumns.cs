using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Finance_BaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- ErpGLAccounts ---
            migrationBuilder.AddColumn<string>(
                name: "SearchName",
                table: "ErpGLAccounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DebitCredit",
                table: "ErpGLAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ReconciliationAccount",
                table: "ErpGLAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Totaling",
                table: "ErpGLAccounts",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenBusPostingGroup",
                table: "ErpGLAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenProdPostingGroup",
                table: "ErpGLAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutomaticExtTexts",
                table: "ErpGLAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxAreaCode",
                table: "ErpGLAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxLiable",
                table: "ErpGLAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxGroupCode",
                table: "ErpGLAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsolTranslationMethod",
                table: "ErpGLAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConsolDebitAcc",
                table: "ErpGLAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsolCreditAcc",
                table: "ErpGLAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostTypeNo",
                table: "ErpGLAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultDeferralTemplateCode",
                table: "ErpGLAccounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OmitDefaultDescrInJnl",
                table: "ErpGLAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // --- ErpGLEntries ---
            migrationBuilder.AddColumn<string>(
                name: "GLAccountName",
                table: "ErpGLEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenPostingType",
                table: "ErpGLEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GenProdPostingGroup",
                table: "ErpGLEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BalAccountType",
                table: "ErpGLEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BalAccountNo",
                table: "ErpGLEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VATAmount",
                table: "ErpGLEntries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VATBusPostingGroup",
                table: "ErpGLEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VATProdPostingGroup",
                table: "ErpGLEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDocumentNo",
                table: "ErpGLEntries",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "ErpGLEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ErpGLEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JournalBatchName",
                table: "ErpGLEntries",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ErpGLEntries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalCurrencyAmount",
                table: "ErpGLEntries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "JobNo",
                table: "ErpGLEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessUnitCode",
                table: "ErpGLEntries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // --- ErpGeneralLedgerSetups ---
            migrationBuilder.AddColumn<string>(
                name: "LocalCurrencySymbol",
                table: "ErpGeneralLedgerSetups",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalCurrencyDescription",
                table: "ErpGeneralLedgerSetups",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvRoundingType",
                table: "ErpGeneralLedgerSetups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VATRoundingType",
                table: "ErpGeneralLedgerSetups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PmtDiscExclVAT",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UnrealizedVAT",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AdjustForPaymentDisc",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MarkCrMemosAsCorrections",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalReportingCurrency",
                table: "ErpGeneralLedgerSetups",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxVATDifferenceAllowed",
                table: "ErpGeneralLedgerSetups",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentTolerancePct",
                table: "ErpGeneralLedgerSetups",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPaymentToleranceAmount",
                table: "ErpGeneralLedgerSetups",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "BlockDeletionOfGLAccounts",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PostWithJobQueue",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobQueueCategoryCode",
                table: "ErpGeneralLedgerSetups",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnSuccess",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RegisterTime",
                table: "ErpGeneralLedgerSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ErpGLAccounts
            migrationBuilder.DropColumn(name: "SearchName", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "DebitCredit", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "ReconciliationAccount", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "Totaling", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "GenBusPostingGroup", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "GenProdPostingGroup", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "AutomaticExtTexts", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "TaxAreaCode", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "TaxLiable", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "TaxGroupCode", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "ConsolTranslationMethod", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "ConsolDebitAcc", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "ConsolCreditAcc", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "CostTypeNo", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "DefaultDeferralTemplateCode", table: "ErpGLAccounts");
            migrationBuilder.DropColumn(name: "OmitDefaultDescrInJnl", table: "ErpGLAccounts");

            // ErpGLEntries
            migrationBuilder.DropColumn(name: "GLAccountName", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "GenPostingType", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "GenProdPostingGroup", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "BalAccountType", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "BalAccountNo", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "VATAmount", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "VATBusPostingGroup", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "VATProdPostingGroup", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "ExternalDocumentNo", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "SourceType", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "UserId", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "JournalBatchName", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "Quantity", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "AdditionalCurrencyAmount", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "JobNo", table: "ErpGLEntries");
            migrationBuilder.DropColumn(name: "BusinessUnitCode", table: "ErpGLEntries");

            // ErpGeneralLedgerSetups
            migrationBuilder.DropColumn(name: "LocalCurrencySymbol", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "LocalCurrencyDescription", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "InvRoundingType", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "VATRoundingType", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "PmtDiscExclVAT", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "UnrealizedVAT", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "AdjustForPaymentDisc", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "MarkCrMemosAsCorrections", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "AdditionalReportingCurrency", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "MaxVATDifferenceAllowed", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "PaymentTolerancePct", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "MaxPaymentToleranceAmount", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "BlockDeletionOfGLAccounts", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "PostWithJobQueue", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "JobQueueCategoryCode", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "NotifyOnSuccess", table: "ErpGeneralLedgerSetups");
            migrationBuilder.DropColumn(name: "RegisterTime", table: "ErpGeneralLedgerSetups");
        }
    }
}
