using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class BaseTables_BaseColumns_Attachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationMethod",
                table: "ErpVendors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaxNo",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension1Code",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension2Code",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToVendorNo",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredBankAccountCode",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactNo",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ErpVendors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyBlocked",
                table: "ErpVendors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibilityCenter",
                table: "ErpVendors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromAddress",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromAddress2",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromCity",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromContact",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromCountryRegionCode",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromCounty",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromPostCode",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyFromVendorName2",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditorNo",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceReceivedDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderAddressCode",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToAddress2",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToCounty",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToName2",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentDiscountPct",
                table: "ErpPurchaseHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PmtDiscountDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostingDescription",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PromisedReceiptDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaserCode",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuoteNo",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonCode",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedReceiptDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibilityCenter",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToAddress2",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToCounty",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToName2",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatBaseDiscountPct",
                table: "ErpPurchaseHeaders",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatRegistrationNo",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorCrMemoNo",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorOrderNo",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorShipmentNo",
                table: "ErpPurchaseHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CalcPmtDiscOnCrMemos",
                table: "ErpPaymentTerms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DirectDebit",
                table: "ErpPaymentMethods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DirectDebitPmtTermsCode",
                table: "ErpPaymentMethods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PmtExportLineDefinition",
                table: "ErpPaymentMethods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension1Code",
                table: "ErpGLAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension2Code",
                table: "ErpGLAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Indentation",
                table: "ErpGLAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NewPage",
                table: "ErpGLAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NoOfBlankLines",
                table: "ErpGLAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AppliesToId",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankPaymentType",
                table: "ErpGenJournalLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Correction",
                table: "ErpGenJournalLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CountryRegionCode",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ErpGenJournalLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageToRecipient",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnHold",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodCode",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostingGroup",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ErpGenJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReasonCode",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalespersPurchCode",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension1Code",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension2Code",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatRegistrationNo",
                table: "ErpGenJournalLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AllowDeferralPostingFrom",
                table: "ErpGeneralLedgerSetups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AllowDeferralPostingTo",
                table: "ErpGeneralLedgerSetups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApplnRoundingPrecision",
                table: "ErpGeneralLedgerSetups",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BillToSellToVatCalc",
                table: "ErpGeneralLedgerSetups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EmuCurrency",
                table: "ErpGeneralLedgerSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrintVatSpecificationInLcy",
                table: "ErpGeneralLedgerSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension3Code",
                table: "ErpGeneralLedgerSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension4Code",
                table: "ErpGeneralLedgerSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension5Code",
                table: "ErpGeneralLedgerSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension6Code",
                table: "ErpGeneralLedgerSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension7Code",
                table: "ErpGeneralLedgerSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension8Code",
                table: "ErpGeneralLedgerSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShowAmounts",
                table: "ErpGeneralLedgerSetups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VatTolerancePct",
                table: "ErpGeneralLedgerSetups",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AltAddressCode",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AltAddressEndDate",
                table: "ErpEmployees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AltAddressStartDate",
                table: "ErpEmployees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApplicationMethod",
                table: "ErpEmployees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BankBranchNo",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CauseOfInactivityCode",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaxNo",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "ErpEmployees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension1Code",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension2Code",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Initials",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerNo",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pager",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyBlocked",
                table: "ErpEmployees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SearchName",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatisticsGroupCode",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwiftCode",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnionMembershipNo",
                table: "ErpEmployees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApplicationMethod",
                table: "ErpCustomers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "BlockPaymentTolerance",
                table: "ErpCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CombineShipments",
                table: "ErpCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerDiscGroup",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPriceGroup",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinChargeTermsCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension1Code",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension2Code",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomePage",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceDiscCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastStatementNo",
                table: "ErpCustomers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhoneNo",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name2",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredBankAccountCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentPct",
                table: "ErpCustomers",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PricesIncludingVat",
                table: "ErpCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactNo",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrintStatements",
                table: "ErpCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyBlocked",
                table: "ErpCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderTermsCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibilityCenter",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchName",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentMethodCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxAreaCode",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxLiable",
                table: "ErpCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VatRegistrationNo",
                table: "ErpCustomers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AmountDecimalPlaces",
                table: "ErpCurrencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApplnRoundingPrecision",
                table: "ErpCurrencies",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EmuCurrency",
                table: "ErpCurrencies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "InvoiceRoundingPrecision",
                table: "ErpCurrencies",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceRoundingType",
                table: "ErpCurrencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IsoCode",
                table: "ErpCurrencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsoNumericCode",
                table: "ErpCurrencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPaymentToleranceAmount",
                table: "ErpCurrencies",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentTolerancePct",
                table: "ErpCurrencies",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnitAmountDecimalPlaces",
                table: "ErpCurrencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitAmountRoundingPrecision",
                table: "ErpCurrencies",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceLastStatement",
                table: "ErpBankAccounts",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankClearingCode",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryRegionCode",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaxNo",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension1Code",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension2Code",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomePage",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastCheckNo",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPaymentStatementNo",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastStatementNo",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinBalance",
                table: "ErpBankAccounts",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name2",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OurContactCode",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostCode",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransitNo",
                table: "ErpBankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpAlternativeAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    County = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FaxNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    table.PrimaryKey("PK_ErpAlternativeAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpApprovalCommentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    TableId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateAndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_ErpApprovalCommentLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBankAccountStatementLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StatementNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StatementLineNo = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatementAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AppliedEntries = table.Column<int>(type: "integer", nullable: false),
                    ValueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpBankAccountStatementLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBankAccountStatements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StatementNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StatementEndingBalance = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StatementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BalanceLastStatement = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    GLBalanceAtPostingDate = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    OutstdPaymentsAtPosting = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    OutstdTransactAtPosting = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalPosDiffAtPosting = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalNegDiffAtPosting = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpBankAccountStatements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBankAccReconciliationLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StatementType = table.Column<int>(type: "integer", nullable: false),
                    BankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatementNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatementLineNo = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatementAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ValueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyForApplication = table.Column<bool>(type: "boolean", nullable: false),
                    CheckNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RelatedPartyName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AdditionalTransactionInfo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    AccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransactionText = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true),
                    RelatedPartyBankAccNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedPartyAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedPartyCity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentReferenceNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShortcutDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ShortcutDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ErpBankAccReconciliationLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBankAccReconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StatementType = table.Column<int>(type: "integer", nullable: false),
                    BankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatementNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatementEndingBalance = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    StatementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BalanceLastStatement = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ShortcutDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ShortcutDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostPaymentsOnly = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpBankAccReconciliations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCheckLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BankAccountLedgerEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CheckType = table.Column<int>(type: "integer", nullable: false),
                    BankPaymentType = table.Column<int>(type: "integer", nullable: false),
                    EntryStatus = table.Column<int>(type: "integer", nullable: false),
                    OriginalEntryStatus = table.Column<int>(type: "integer", nullable: false),
                    BalAccountType = table.Column<int>(type: "integer", nullable: false),
                    BalAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Open = table.Column<bool>(type: "boolean", nullable: false),
                    StatementStatus = table.Column<int>(type: "integer", nullable: false),
                    StatementNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StatementLineNo = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalDocumentNo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCheckLedgerEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCommentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TableName = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Comment = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_ErpCommentLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpConfidentialInformations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfidentialCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_ErpConfidentialInformations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpConfidentials",
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
                    table.PrimaryKey("PK_ErpConfidentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCountriesRegions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    IsoNumericCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    EuCountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IntrastatCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AddressFormat = table.Column<int>(type: "integer", nullable: false),
                    ContactAddressFormat = table.Column<int>(type: "integer", nullable: false),
                    VatScheme = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CountyName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("PK_ErpCountriesRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCustomerBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Contact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BankBranchNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BankAccountNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TransitNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    County = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FaxNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Iban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SwiftCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BankClearingCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankClearingStandard = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ErpCustomerBankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpCustomReportSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Usage = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    CustomReportLayoutCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SendToEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UseForEmailAttachment = table.Column<bool>(type: "boolean", nullable: false),
                    UseForEmailBody = table.Column<bool>(type: "boolean", nullable: false),
                    EmailBodyLayoutCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UseEmailFromContact = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpCustomReportSelections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDepreciationBooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GLIntegrationAcqCost = table.Column<bool>(type: "boolean", nullable: false),
                    GLIntegrationDepreciation = table.Column<bool>(type: "boolean", nullable: false),
                    GLIntegrationWriteDown = table.Column<bool>(type: "boolean", nullable: false),
                    GLIntegrationAppreciation = table.Column<bool>(type: "boolean", nullable: false),
                    GLIntegrationDisposal = table.Column<bool>(type: "boolean", nullable: false),
                    GLIntegrationMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    DisposalCalculationMethod = table.Column<int>(type: "integer", nullable: false),
                    AllowDeprBelowZero = table.Column<bool>(type: "boolean", nullable: false),
                    AllowIndexation = table.Column<bool>(type: "boolean", nullable: false),
                    UseSameFAAndGLPostingDates = table.Column<bool>(type: "boolean", nullable: false),
                    UseRoundingInPeriodicDepr = table.Column<bool>(type: "boolean", nullable: false),
                    AllowChangesInDeprFields = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultFinalRoundingAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DefaultEndingBookValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MarkErrorsAsCorrections = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAcqCostBelowZero = table.Column<bool>(type: "boolean", nullable: false),
                    AllowIdenticalDocumentNo = table.Column<bool>(type: "boolean", nullable: false),
                    FiscalYear365Days = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpDepreciationBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDetailedCustLedgEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustLedgerEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    JournalBatchName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DebitAmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreditAmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    InitialEntryDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InitialEntryGlobalDim1 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InitialEntryGlobalDim2 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GenBusPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GenProdPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VatBusPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VatProdPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InitialDocumentType = table.Column<int>(type: "integer", nullable: false),
                    AppliedCustLedgerEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    Unapplied = table.Column<bool>(type: "boolean", nullable: false),
                    UnappliedByEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    RemainingPmtDiscPossible = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaxPaymentTolerance = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ApplicationNo = table.Column<int>(type: "integer", nullable: false),
                    LedgerEntryAmount = table.Column<bool>(type: "boolean", nullable: false),
                    PostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ExchRateAdjmtRegNo = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpDetailedCustLedgEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDetailedVendorLedgEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorLedgerEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    JournalBatchName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DebitAmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreditAmountLcy = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    InitialEntryDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InitialEntryGlobalDim1 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InitialEntryGlobalDim2 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GenBusPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GenProdPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VatBusPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VatProdPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InitialDocumentType = table.Column<int>(type: "integer", nullable: false),
                    AppliedVendLedgerEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    Unapplied = table.Column<bool>(type: "boolean", nullable: false),
                    UnappliedByEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    RemainingPmtDiscPossible = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    MaxPaymentTolerance = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ApplicationNo = table.Column<int>(type: "integer", nullable: false),
                    LedgerEntryAmount = table.Column<bool>(type: "boolean", nullable: false),
                    PostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ExchRateAdjmtRegNo = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpDetailedVendorLedgEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDocumentAttachmentContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpDocumentAttachmentContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDocumentAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    AttachmentNo = table.Column<int>(type: "integer", nullable: false),
                    AttachedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AttachedByUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    DocumentFlowPurchase = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentFlowSales = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpDocumentAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmployeeQualifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    QualificationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InstitutionCompany = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CourseGrade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmployeeStatus = table.Column<int>(type: "integer", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpEmployeeQualifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmployeeRelatives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    RelativeCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RelativesEmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpEmployeeRelatives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpEmployeeStatisticsGroups",
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
                    table.PrimaryKey("PK_ErpEmployeeStatisticsGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFAClasses",
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
                    table.PrimaryKey("PK_ErpFAClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFADepreciationBooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FANo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DepreciationBookCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DepreciationMethod = table.Column<int>(type: "integer", nullable: false),
                    DepreciationStartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StraightLinePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NoOfDepreciationYears = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NoOfDepreciationMonths = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FixedDeprAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DecliningBalancePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FinalRoundingAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    EndingBookValue = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FAPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DepreciationEndingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GLAcquisitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisposalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAcquisitionCostDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastDepreciationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastWriteDownDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAppreciationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSalvageValueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectedDisposalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectedProceedsOnDisposal = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UseHalfYearConvention = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultFADepreciationBook = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpFADepreciationBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFALedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GLEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    FANo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FAPostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ExternalDocumentNo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DepreciationBookCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FAPostingCategory = table.Column<int>(type: "integer", nullable: false),
                    FAPostingType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ReclassificationEntry = table.Column<bool>(type: "boolean", nullable: false),
                    PartOfBookValue = table.Column<bool>(type: "boolean", nullable: false),
                    PartOfDepreciableBasis = table.Column<bool>(type: "boolean", nullable: false),
                    DisposalCalculationMethod = table.Column<int>(type: "integer", nullable: false),
                    DisposalEntryNo = table.Column<long>(type: "bigint", nullable: false),
                    NoOfDepreciationDays = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FASubclassCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FALocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FAPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GlobalDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GlobalDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DepreciationMethod = table.Column<int>(type: "integer", nullable: false),
                    DepreciationStartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StraightLinePct = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    NoOfDepreciationYears = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    JournalBatchName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SourceCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TransactionNo = table.Column<long>(type: "bigint", nullable: false),
                    BalAccountType = table.Column<int>(type: "integer", nullable: false),
                    BalAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FAClassCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DepreciationEndingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpFALedgerEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFALocations",
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
                    table.PrimaryKey("PK_ErpFALocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFAPostingGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcquisitionCostAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccumDepreciationAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WriteDownAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AppreciationAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AcqCostAccOnDisposal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccumDeprAccOnDisposal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WriteDownAccOnDisposal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AppreciationAccOnDisposal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GainsAccOnDisposal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LossesAccOnDisposal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BookValAccOnDispGain = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SalesAccOnDispGain = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WriteDownBalAccOnDisp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApprecBalAccOnDisp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaintenanceExpenseAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaintenanceBalAcc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AcquisitionCostBalAcc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DepreciationExpenseAcc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WriteDownExpenseAcc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AppreciationBalAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SalesBalAcc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SalesAccOnDispLoss = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BookValAccOnDispLoss = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpFAPostingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFASetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowPostingToMainAssets = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultDeprBook = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AllowFAPostingFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowFAPostingTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FixedAssetNos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpFASetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFASubclasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FAClassCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DefaultFAPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpFASubclasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpFixedAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SearchDescription = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FAClassCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FASubclassCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GlobalDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GlobalDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FALocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MainAssetComponent = table.Column<int>(type: "integer", nullable: false),
                    ComponentOfMainAsset = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetedAsset = table.Column<bool>(type: "boolean", nullable: false),
                    WarrantyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsibleEmployee = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SerialNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    MaintenanceVendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UnderMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    NextServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Inactive = table.Column<bool>(type: "boolean", nullable: false),
                    FAPostingGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpFixedAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpGLBudgetEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    BudgetName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    GLAccountNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GlobalDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GlobalDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BusinessUnitCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetDimension3Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetDimension4Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpGLBudgetEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpGLBudgetNames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    BudgetDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetDimension3Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BudgetDimension4Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpGLBudgetNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpHumanResourceCommentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TableName = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TableLineNo = table.Column<int>(type: "integer", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    AlternativeAddressCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Comment = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_ErpHumanResourceCommentLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemVendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LeadTimeCalculation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    VendorItemNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ErpItemVendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMainAssetComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MainAssetNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FANo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_ErpMainAssetComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMaintenanceRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FANo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaintenanceVendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Comment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ServiceAgentName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ServiceAgentPhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ServiceAgentMobilePhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("PK_ErpMaintenanceRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMaintenances",
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
                    table.PrimaryKey("PK_ErpMaintenances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMiscArticleInformations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MiscArticleCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InUse = table.Column<bool>(type: "boolean", nullable: false),
                    SerialNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ErpMiscArticleInformations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpMiscArticles",
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
                    table.PrimaryKey("PK_ErpMiscArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpOrderAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Contact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FaxNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    County = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_ErpOrderAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SearchCity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    County = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("PK_ErpPostCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPurchCommentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DocumentLineNo = table.Column<int>(type: "integer", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Comment = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_ErpPurchCommentLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpReasonCodes",
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
                    table.PrimaryKey("PK_ErpReasonCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpRelatives",
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
                    table.PrimaryKey("PK_ErpRelatives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpReportSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Usage = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    CustomReportLayoutCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UseForEmailAttachment = table.Column<bool>(type: "boolean", nullable: false),
                    UseForEmailBody = table.Column<bool>(type: "boolean", nullable: false),
                    EmailBodyLayoutCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ReportLayoutName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpReportSelections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpResponsibilityCenters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FaxNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Name2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Contact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GlobalDimension1Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GlobalDimension2Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    County = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_ErpResponsibilityCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpShipmentMethods",
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
                    table.PrimaryKey("PK_ErpShipmentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSourceCodes",
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
                    table.PrimaryKey("PK_ErpSourceCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpUserSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AllowPostingFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowPostingTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegisterTime = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDeferralPostingFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowDeferralPostingTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalespersPurchCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApproverId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesAmountApprovalLimit = table.Column<int>(type: "integer", nullable: false),
                    PurchaseAmountApprovalLimit = table.Column<int>(type: "integer", nullable: false),
                    UnlimitedSalesApproval = table.Column<bool>(type: "boolean", nullable: false),
                    UnlimitedPurchaseApproval = table.Column<bool>(type: "boolean", nullable: false),
                    Substitute = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RequestAmountApprovalLimit = table.Column<int>(type: "integer", nullable: false),
                    UnlimitedRequestApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalAdministrator = table.Column<bool>(type: "boolean", nullable: false),
                    AllowVatDateFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowVatDateTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesInvoicePostingPolicy = table.Column<int>(type: "integer", nullable: false),
                    PurchInvoicePostingPolicy = table.Column<int>(type: "integer", nullable: false),
                    AllowFAPostingFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowFAPostingTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesRespCtrFilter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PurchaseRespCtrFilter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    table.PrimaryKey("PK_ErpUserSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpVendorBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Contact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BankBranchNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BankAccountNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TransitNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    County = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FaxNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Iban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SwiftCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BankClearingCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankClearingStandard = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ErpVendorBankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpWorkflowUserGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowUserGroupCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SequenceNo = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpWorkflowUserGroupMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpWorkflowUserGroups",
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
                    table.PrimaryKey("PK_ErpWorkflowUserGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpAlternativeAddresses_TenantId_CompanyId_EmployeeNo_Code",
                table: "ErpAlternativeAddresses",
                columns: new[] { "TenantId", "CompanyId", "EmployeeNo", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpApprovalCommentLines_TenantId_CompanyId_EntryNo",
                table: "ErpApprovalCommentLines",
                columns: new[] { "TenantId", "CompanyId", "EntryNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccountStatementLines_BankAccountNo",
                table: "ErpBankAccountStatementLines",
                column: "BankAccountNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccountStatements_BankAccountNo",
                table: "ErpBankAccountStatements",
                column: "BankAccountNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccReconciliationLines_TenantId_CompanyId_StatementT~",
                table: "ErpBankAccReconciliationLines",
                columns: new[] { "TenantId", "CompanyId", "StatementType", "BankAccountNo", "StatementNo", "StatementLineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpBankAccReconciliations_TenantId_CompanyId_StatementType_~",
                table: "ErpBankAccReconciliations",
                columns: new[] { "TenantId", "CompanyId", "StatementType", "BankAccountNo", "StatementNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCheckLedgerEntries_BankAccountNo",
                table: "ErpCheckLedgerEntries",
                column: "BankAccountNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpCommentLines_TenantId_CompanyId_TableName_No_LineNo",
                table: "ErpCommentLines",
                columns: new[] { "TenantId", "CompanyId", "TableName", "No", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfidentialInformations_TenantId_CompanyId_EmployeeNo_C~",
                table: "ErpConfidentialInformations",
                columns: new[] { "TenantId", "CompanyId", "EmployeeNo", "ConfidentialCode", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpConfidentials_TenantId_CompanyId_Code",
                table: "ErpConfidentials",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCountriesRegions_TenantId_CompanyId_Code",
                table: "ErpCountriesRegions",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCustomerBankAccounts_TenantId_CompanyId_CustomerNo_Code",
                table: "ErpCustomerBankAccounts",
                columns: new[] { "TenantId", "CompanyId", "CustomerNo", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCustomReportSelections_TenantId_CompanyId_SourceType_Sou~",
                table: "ErpCustomReportSelections",
                columns: new[] { "TenantId", "CompanyId", "SourceType", "SourceNo", "Usage", "Sequence" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpDepreciationBooks_TenantId_CompanyId_Code",
                table: "ErpDepreciationBooks",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpDetailedCustLedgEntries_CustomerNo",
                table: "ErpDetailedCustLedgEntries",
                column: "CustomerNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpDetailedVendorLedgEntries_VendorNo",
                table: "ErpDetailedVendorLedgEntries",
                column: "VendorNo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpDocumentAttachments_CompanyId_EntityName_RecordId",
                table: "ErpDocumentAttachments",
                columns: new[] { "CompanyId", "EntityName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeQualifications_TenantId_CompanyId_EmployeeNo_Lin~",
                table: "ErpEmployeeQualifications",
                columns: new[] { "TenantId", "CompanyId", "EmployeeNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeRelatives_TenantId_CompanyId_EmployeeNo_LineNo",
                table: "ErpEmployeeRelatives",
                columns: new[] { "TenantId", "CompanyId", "EmployeeNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeStatisticsGroups_TenantId_CompanyId_Code",
                table: "ErpEmployeeStatisticsGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFAClasses_TenantId_CompanyId_Code",
                table: "ErpFAClasses",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFADepreciationBooks_TenantId_CompanyId_FANo_Depreciation~",
                table: "ErpFADepreciationBooks",
                columns: new[] { "TenantId", "CompanyId", "FANo", "DepreciationBookCode" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFALedgerEntries_FANo",
                table: "ErpFALedgerEntries",
                column: "FANo");

            migrationBuilder.CreateIndex(
                name: "IX_ErpFALocations_TenantId_CompanyId_Code",
                table: "ErpFALocations",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFAPostingGroups_TenantId_CompanyId_Code",
                table: "ErpFAPostingGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFASetups_TenantId_CompanyId",
                table: "ErpFASetups",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFASubclasses_TenantId_CompanyId_Code",
                table: "ErpFASubclasses",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpFixedAssets_TenantId_CompanyId_No",
                table: "ErpFixedAssets",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpGLBudgetEntries_TenantId_CompanyId_EntryNo",
                table: "ErpGLBudgetEntries",
                columns: new[] { "TenantId", "CompanyId", "EntryNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpGLBudgetNames_TenantId_CompanyId_Name",
                table: "ErpGLBudgetNames",
                columns: new[] { "TenantId", "CompanyId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpHumanResourceCommentLines_TenantId_CompanyId_TableName_N~",
                table: "ErpHumanResourceCommentLines",
                columns: new[] { "TenantId", "CompanyId", "TableName", "No", "TableLineNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemVendors_TenantId_CompanyId_VendorNo_ItemNo",
                table: "ErpItemVendors",
                columns: new[] { "TenantId", "CompanyId", "VendorNo", "ItemNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMainAssetComponents_TenantId_CompanyId_MainAssetNo_FANo",
                table: "ErpMainAssetComponents",
                columns: new[] { "TenantId", "CompanyId", "MainAssetNo", "FANo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMaintenanceRegistrations_TenantId_CompanyId_FANo_LineNo",
                table: "ErpMaintenanceRegistrations",
                columns: new[] { "TenantId", "CompanyId", "FANo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMaintenances_TenantId_CompanyId_Code",
                table: "ErpMaintenances",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMiscArticleInformations_TenantId_CompanyId_EmployeeNo_Mi~",
                table: "ErpMiscArticleInformations",
                columns: new[] { "TenantId", "CompanyId", "EmployeeNo", "MiscArticleCode", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpMiscArticles_TenantId_CompanyId_Code",
                table: "ErpMiscArticles",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpOrderAddresses_TenantId_CompanyId_VendorNo_Code",
                table: "ErpOrderAddresses",
                columns: new[] { "TenantId", "CompanyId", "VendorNo", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostCodes_TenantId_CompanyId_Code_City",
                table: "ErpPostCodes",
                columns: new[] { "TenantId", "CompanyId", "Code", "City" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPurchCommentLines_TenantId_CompanyId_DocumentType_No_Doc~",
                table: "ErpPurchCommentLines",
                columns: new[] { "TenantId", "CompanyId", "DocumentType", "No", "DocumentLineNo", "LineNo" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpReasonCodes_TenantId_CompanyId_Code",
                table: "ErpReasonCodes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpRelatives_TenantId_CompanyId_Code",
                table: "ErpRelatives",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpReportSelections_TenantId_CompanyId_Usage_Sequence",
                table: "ErpReportSelections",
                columns: new[] { "TenantId", "CompanyId", "Usage", "Sequence" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpResponsibilityCenters_TenantId_CompanyId_Code",
                table: "ErpResponsibilityCenters",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpShipmentMethods_TenantId_CompanyId_Code",
                table: "ErpShipmentMethods",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSourceCodes_TenantId_CompanyId_Code",
                table: "ErpSourceCodes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpUserSetups_TenantId_CompanyId_UserId",
                table: "ErpUserSetups",
                columns: new[] { "TenantId", "CompanyId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpVendorBankAccounts_TenantId_CompanyId_VendorNo_Code",
                table: "ErpVendorBankAccounts",
                columns: new[] { "TenantId", "CompanyId", "VendorNo", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpWorkflowUserGroupMembers_TenantId_CompanyId_WorkflowUser~",
                table: "ErpWorkflowUserGroupMembers",
                columns: new[] { "TenantId", "CompanyId", "WorkflowUserGroupCode", "UserName" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpWorkflowUserGroups_TenantId_CompanyId_Code",
                table: "ErpWorkflowUserGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpAlternativeAddresses");

            migrationBuilder.DropTable(
                name: "ErpApprovalCommentLines");

            migrationBuilder.DropTable(
                name: "ErpBankAccountStatementLines");

            migrationBuilder.DropTable(
                name: "ErpBankAccountStatements");

            migrationBuilder.DropTable(
                name: "ErpBankAccReconciliationLines");

            migrationBuilder.DropTable(
                name: "ErpBankAccReconciliations");

            migrationBuilder.DropTable(
                name: "ErpCheckLedgerEntries");

            migrationBuilder.DropTable(
                name: "ErpCommentLines");

            migrationBuilder.DropTable(
                name: "ErpConfidentialInformations");

            migrationBuilder.DropTable(
                name: "ErpConfidentials");

            migrationBuilder.DropTable(
                name: "ErpCountriesRegions");

            migrationBuilder.DropTable(
                name: "ErpCustomerBankAccounts");

            migrationBuilder.DropTable(
                name: "ErpCustomReportSelections");

            migrationBuilder.DropTable(
                name: "ErpDepreciationBooks");

            migrationBuilder.DropTable(
                name: "ErpDetailedCustLedgEntries");

            migrationBuilder.DropTable(
                name: "ErpDetailedVendorLedgEntries");

            migrationBuilder.DropTable(
                name: "ErpDocumentAttachmentContents");

            migrationBuilder.DropTable(
                name: "ErpDocumentAttachments");

            migrationBuilder.DropTable(
                name: "ErpEmployeeQualifications");

            migrationBuilder.DropTable(
                name: "ErpEmployeeRelatives");

            migrationBuilder.DropTable(
                name: "ErpEmployeeStatisticsGroups");

            migrationBuilder.DropTable(
                name: "ErpFAClasses");

            migrationBuilder.DropTable(
                name: "ErpFADepreciationBooks");

            migrationBuilder.DropTable(
                name: "ErpFALedgerEntries");

            migrationBuilder.DropTable(
                name: "ErpFALocations");

            migrationBuilder.DropTable(
                name: "ErpFAPostingGroups");

            migrationBuilder.DropTable(
                name: "ErpFASetups");

            migrationBuilder.DropTable(
                name: "ErpFASubclasses");

            migrationBuilder.DropTable(
                name: "ErpFixedAssets");

            migrationBuilder.DropTable(
                name: "ErpGLBudgetEntries");

            migrationBuilder.DropTable(
                name: "ErpGLBudgetNames");

            migrationBuilder.DropTable(
                name: "ErpHumanResourceCommentLines");

            migrationBuilder.DropTable(
                name: "ErpItemVendors");

            migrationBuilder.DropTable(
                name: "ErpMainAssetComponents");

            migrationBuilder.DropTable(
                name: "ErpMaintenanceRegistrations");

            migrationBuilder.DropTable(
                name: "ErpMaintenances");

            migrationBuilder.DropTable(
                name: "ErpMiscArticleInformations");

            migrationBuilder.DropTable(
                name: "ErpMiscArticles");

            migrationBuilder.DropTable(
                name: "ErpOrderAddresses");

            migrationBuilder.DropTable(
                name: "ErpPostCodes");

            migrationBuilder.DropTable(
                name: "ErpPurchCommentLines");

            migrationBuilder.DropTable(
                name: "ErpReasonCodes");

            migrationBuilder.DropTable(
                name: "ErpRelatives");

            migrationBuilder.DropTable(
                name: "ErpReportSelections");

            migrationBuilder.DropTable(
                name: "ErpResponsibilityCenters");

            migrationBuilder.DropTable(
                name: "ErpShipmentMethods");

            migrationBuilder.DropTable(
                name: "ErpSourceCodes");

            migrationBuilder.DropTable(
                name: "ErpUserSetups");

            migrationBuilder.DropTable(
                name: "ErpVendorBankAccounts");

            migrationBuilder.DropTable(
                name: "ErpWorkflowUserGroupMembers");

            migrationBuilder.DropTable(
                name: "ErpWorkflowUserGroups");

            migrationBuilder.DropColumn(
                name: "ApplicationMethod",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "County",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "FaxNo",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "GlobalDimension1Code",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "GlobalDimension2Code",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "PayToVendorNo",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "PreferredBankAccountCode",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "PrimaryContactNo",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "PrivacyBlocked",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "ResponsibilityCenter",
                table: "ErpVendors");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromAddress",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromAddress2",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromCity",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromContact",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromCountryRegionCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromCounty",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromPostCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "BuyFromVendorName2",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "CreditorNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "DocumentDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "InvoiceReceivedDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "OrderAddressCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PayToAddress2",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PayToCounty",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PayToName2",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PaymentDiscountPct",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PmtDiscountDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PostingDescription",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PromisedReceiptDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "PurchaserCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "QuoteNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ReasonCode",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "RequestedReceiptDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ResponsibilityCenter",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToAddress2",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToCounty",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToName2",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VatBaseDiscountPct",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VatRegistrationNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VendorCrMemoNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VendorOrderNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VendorShipmentNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "CalcPmtDiscOnCrMemos",
                table: "ErpPaymentTerms");

            migrationBuilder.DropColumn(
                name: "DirectDebit",
                table: "ErpPaymentMethods");

            migrationBuilder.DropColumn(
                name: "DirectDebitPmtTermsCode",
                table: "ErpPaymentMethods");

            migrationBuilder.DropColumn(
                name: "PmtExportLineDefinition",
                table: "ErpPaymentMethods");

            migrationBuilder.DropColumn(
                name: "GlobalDimension1Code",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "GlobalDimension2Code",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "Indentation",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "NewPage",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "NoOfBlankLines",
                table: "ErpGLAccounts");

            migrationBuilder.DropColumn(
                name: "AppliesToId",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "BankPaymentType",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "Correction",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "CountryRegionCode",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "MessageToRecipient",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "OnHold",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "PaymentMethodCode",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "PostingGroup",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "ReasonCode",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "SalespersPurchCode",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension1Code",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension2Code",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "SourceCode",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "VatRegistrationNo",
                table: "ErpGenJournalLines");

            migrationBuilder.DropColumn(
                name: "AllowDeferralPostingFrom",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "AllowDeferralPostingTo",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ApplnRoundingPrecision",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "BillToSellToVatCalc",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "EmuCurrency",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "PrintVatSpecificationInLcy",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension3Code",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension4Code",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension5Code",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension6Code",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension7Code",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ShortcutDimension8Code",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "ShowAmounts",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "VatTolerancePct",
                table: "ErpGeneralLedgerSetups");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "AltAddressCode",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "AltAddressEndDate",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "AltAddressStartDate",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "ApplicationMethod",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "BankBranchNo",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "CauseOfInactivityCode",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "County",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "FaxNo",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "GlobalDimension1Code",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "GlobalDimension2Code",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "Initials",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "ManagerNo",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "Pager",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "PrivacyBlocked",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "SearchName",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "StatisticsGroupCode",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "SwiftCode",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "UnionMembershipNo",
                table: "ErpEmployees");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "ApplicationMethod",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "BlockPaymentTolerance",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "CombineShipments",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "County",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "CustomerDiscGroup",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "CustomerPriceGroup",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "FinChargeTermsCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "GlobalDimension1Code",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "GlobalDimension2Code",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "HomePage",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "InvoiceDiscCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "LastStatementNo",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "MobilePhoneNo",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "Name2",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "PreferredBankAccountCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "PrepaymentPct",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "PricesIncludingVat",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "PrimaryContactNo",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "PrintStatements",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "PrivacyBlocked",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "ReminderTermsCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "ResponsibilityCenter",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "SearchName",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "ShipmentMethodCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "TaxAreaCode",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "TaxLiable",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "VatRegistrationNo",
                table: "ErpCustomers");

            migrationBuilder.DropColumn(
                name: "AmountDecimalPlaces",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "ApplnRoundingPrecision",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "EmuCurrency",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "InvoiceRoundingPrecision",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "InvoiceRoundingType",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "IsoCode",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "IsoNumericCode",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "MaxPaymentToleranceAmount",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "PaymentTolerancePct",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "UnitAmountDecimalPlaces",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "UnitAmountRoundingPrecision",
                table: "ErpCurrencies");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "BalanceLastStatement",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "BankClearingCode",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "CountryRegionCode",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "County",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "FaxNo",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "GlobalDimension1Code",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "GlobalDimension2Code",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "HomePage",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "LastCheckNo",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "LastPaymentStatementNo",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "LastStatementNo",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "MinBalance",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "Name2",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "OurContactCode",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "PostCode",
                table: "ErpBankAccounts");

            migrationBuilder.DropColumn(
                name: "TransitNo",
                table: "ErpBankAccounts");
        }
    }
}
