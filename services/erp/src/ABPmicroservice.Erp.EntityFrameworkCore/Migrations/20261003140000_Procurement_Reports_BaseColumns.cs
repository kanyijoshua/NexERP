using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Procurement_Reports_BaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- ErpCustomReportLayouts ---
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ErpCustomReportLayouts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportID",
                table: "ErpCustomReportLayouts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileExtension",
                table: "ErpCustomReportLayouts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BuiltIn",
                table: "ErpCustomReportLayouts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByUser",
                table: "ErpCustomReportLayouts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LayoutLastUpdated",
                table: "ErpCustomReportLayouts",
                type: "timestamp with time zone",
                nullable: true);

            // --- ErpReportLayoutSelections ---
            migrationBuilder.AddColumn<int>(
                name: "ReportID",
                table: "ErpReportLayoutSelections",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomReportLayoutCode",
                table: "ErpReportLayoutSelections",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportLayoutDescription",
                table: "ErpReportLayoutSelections",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportCaption",
                table: "ErpReportLayoutSelections",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            // --- ErpPurchasesPayablesSetups ---
            migrationBuilder.AddColumn<string>(
                name: "BlanketOrderNos",
                table: "ErpPurchasesPayablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedReceiptNos",
                table: "ErpPurchasesPayablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedReturnShptNos",
                table: "ErpPurchasesPayablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnOrderNos",
                table: "ErpPurchasesPayablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPosting",
                table: "ErpPurchasesPayablesSetups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ReceiptOnInvoice",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InvoiceRounding",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CalcInvDiscount",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowVATDifference",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CalcInvDiscPerVATID",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExactCostReversingMandatory",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PostWithJobQueue",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobQueueCategoryCode",
                table: "ErpPurchasesPayablesSetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnSuccess",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CopyCommentsBlanketToOrder",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CopyCommentsOrderToInvoice",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CopyCommentsOrderToReceipt",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CopyCommentsRetOrderToCrMemo",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CopyCommentsRetOrderToRetShpt",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnShipmentOnCreditMemo",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CopyVendorNameToEntries",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CopyLineDescrToGLEntry",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowMultiplePostingGroups",
                table: "ErpPurchasesPayablesSetups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // --- ErpVendors ---
            migrationBuilder.AddColumn<string>(
                name: "SearchName",
                table: "ErpVendors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name2",
                table: "ErpVendors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "ErpVendors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "ErpVendors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OurAccountNo",
                table: "ErpVendors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentMethodCode",
                table: "ErpVendors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAgentCode",
                table: "ErpVendors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceDiscCode",
                table: "ErpVendors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PricesIncludingVAT",
                table: "ErpVendors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VATRegistrationNo",
                table: "ErpVendors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomePage",
                table: "ErpVendors",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxAreaCode",
                table: "ErpVendors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxLiable",
                table: "ErpVendors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BlockPaymentTolerance",
                table: "ErpVendors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentPct",
                table: "ErpVendors",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AllowMultiplePostingGroups",
                table: "ErpVendors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhoneNo",
                table: "ErpVendors",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpVendors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadTimeCalculation",
                table: "ErpVendors",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            // --- ErpPurchaseHeaders ---
            migrationBuilder.AddColumn<string>(
                name: "YourReference",
                table: "ErpPurchaseHeaders",
                type: "character varying(35)",
                maxLength: 35,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToName",
                table: "ErpPurchaseHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToAddress",
                table: "ErpPurchaseHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToCity",
                table: "ErpPurchaseHeaders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToPostCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToCountryRegionCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayToContact",
                table: "ErpPurchaseHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToName",
                table: "ErpPurchaseHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToAddress",
                table: "ErpPurchaseHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToCity",
                table: "ErpPurchaseHeaders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToPostCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToCountryRegionCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToContact",
                table: "ErpPurchaseHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentMethodCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension1Code",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension2Code",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorPostingGroup",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenBusPostingGroup",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatBusPostingGroup",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PricesIncludingVat",
                table: "ErpPurchaseHeaders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OnHold",
                table: "ErpPurchaseHeaders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppliesToDocType",
                table: "ErpPurchaseHeaders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliesToDocNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliesToId",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxAreaCode",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxLiable",
                table: "ErpPurchaseHeaders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrepaymentPct",
                table: "ErpPurchaseHeaders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            // --- ErpPurchaseLines ---
            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpPurchaseLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedReceiptDate",
                table: "ErpPurchaseLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemCategoryCode",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension1Code",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortcutDimension2Code",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyToReceive",
                table: "ErpPurchaseLines",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityReceived",
                table: "ErpPurchaseLines",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyToInvoice",
                table: "ErpPurchaseLines",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityInvoiced",
                table: "ErpPurchaseLines",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeferralCode",
                table: "ErpPurchaseLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxAreaCode",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxLiable",
                table: "ErpPurchaseLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxGroupCode",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenBusPostingGroup",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenProdPostingGroup",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // --- ErpVendorLedgerEntries ---
            migrationBuilder.AddColumn<string>(
                name: "VendorName",
                table: "ErpVendorLedgerEntries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorPostingGroup",
                table: "ErpVendorLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension1Code",
                table: "ErpVendorLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlobalDimension2Code",
                table: "ErpVendorLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaserCode",
                table: "ErpVendorLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ErpVendorLedgerEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                table: "ErpVendorLedgerEntries",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnHold",
                table: "ErpVendorLedgerEntries",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliesToDocType",
                table: "ErpVendorLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliesToDocNo",
                table: "ErpVendorLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliesToId",
                table: "ErpVendorLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JournalBatchName",
                table: "ErpVendorLedgerEntries",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDocumentNo",
                table: "ErpVendorLedgerEntries",
                type: "character varying(35)",
                maxLength: 35,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodCode",
                table: "ErpVendorLedgerEntries",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- ErpVendorLedgerEntries ---
            migrationBuilder.DropColumn(name: "VendorName", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "VendorPostingGroup", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "GlobalDimension1Code", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "GlobalDimension2Code", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "PurchaserCode", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "UserId", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "SourceCode", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "OnHold", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "AppliesToDocType", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "AppliesToDocNo", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "AppliesToId", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "JournalBatchName", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "ExternalDocumentNo", table: "ErpVendorLedgerEntries");
            migrationBuilder.DropColumn(name: "PaymentMethodCode", table: "ErpVendorLedgerEntries");

            // --- ErpPurchaseLines ---
            migrationBuilder.DropColumn(name: "LocationCode", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "ExpectedReceiptDate", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "ItemCategoryCode", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "ShortcutDimension1Code", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "ShortcutDimension2Code", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "QtyToReceive", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "QuantityReceived", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "QtyToInvoice", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "QuantityInvoiced", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "DeferralCode", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "TaxAreaCode", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "TaxLiable", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "TaxGroupCode", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "GenBusPostingGroup", table: "ErpPurchaseLines");
            migrationBuilder.DropColumn(name: "GenProdPostingGroup", table: "ErpPurchaseLines");

            // --- ErpPurchaseHeaders ---
            migrationBuilder.DropColumn(name: "YourReference", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PayToName", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PayToAddress", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PayToCity", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PayToPostCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PayToCountryRegionCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PayToContact", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipToCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipToName", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipToAddress", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipToCity", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipToPostCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipToCountryRegionCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipToContact", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShipmentMethodCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PaymentMethodCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShortcutDimension1Code", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "ShortcutDimension2Code", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "VendorPostingGroup", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "GenBusPostingGroup", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "VatBusPostingGroup", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PricesIncludingVat", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "OnHold", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "AppliesToDocType", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "AppliesToDocNo", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "AppliesToId", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "TaxAreaCode", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "TaxLiable", table: "ErpPurchaseHeaders");
            migrationBuilder.DropColumn(name: "PrepaymentPct", table: "ErpPurchaseHeaders");

            // --- ErpVendors ---
            migrationBuilder.DropColumn(name: "SearchName", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "Name2", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "Address2", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "Contact", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "OurAccountNo", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "ShipmentMethodCode", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "ShippingAgentCode", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "InvoiceDiscCode", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "PricesIncludingVAT", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "VATRegistrationNo", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "HomePage", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "TaxAreaCode", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "TaxLiable", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "BlockPaymentTolerance", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "PrepaymentPct", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "AllowMultiplePostingGroups", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "MobilePhoneNo", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "LocationCode", table: "ErpVendors");
            migrationBuilder.DropColumn(name: "LeadTimeCalculation", table: "ErpVendors");

            // --- ErpPurchasesPayablesSetups ---
            migrationBuilder.DropColumn(name: "BlanketOrderNos", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "PostedReceiptNos", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "PostedReturnShptNos", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "ReturnOrderNos", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "DiscountPosting", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "ReceiptOnInvoice", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "InvoiceRounding", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CalcInvDiscount", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "AllowVATDifference", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CalcInvDiscPerVATID", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "ExactCostReversingMandatory", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "PostWithJobQueue", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "JobQueueCategoryCode", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "NotifyOnSuccess", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CopyCommentsBlanketToOrder", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CopyCommentsOrderToInvoice", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CopyCommentsOrderToReceipt", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CopyCommentsRetOrderToCrMemo", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CopyCommentsRetOrderToRetShpt", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "ReturnShipmentOnCreditMemo", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CopyVendorNameToEntries", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "CopyLineDescrToGLEntry", table: "ErpPurchasesPayablesSetups");
            migrationBuilder.DropColumn(name: "AllowMultiplePostingGroups", table: "ErpPurchasesPayablesSetups");

            // --- ErpReportLayoutSelections ---
            migrationBuilder.DropColumn(name: "ReportID", table: "ErpReportLayoutSelections");
            migrationBuilder.DropColumn(name: "CustomReportLayoutCode", table: "ErpReportLayoutSelections");
            migrationBuilder.DropColumn(name: "ReportLayoutDescription", table: "ErpReportLayoutSelections");
            migrationBuilder.DropColumn(name: "ReportCaption", table: "ErpReportLayoutSelections");

            // --- ErpCustomReportLayouts ---
            migrationBuilder.DropColumn(name: "Code", table: "ErpCustomReportLayouts");
            migrationBuilder.DropColumn(name: "ReportID", table: "ErpCustomReportLayouts");
            migrationBuilder.DropColumn(name: "FileExtension", table: "ErpCustomReportLayouts");
            migrationBuilder.DropColumn(name: "BuiltIn", table: "ErpCustomReportLayouts");
            migrationBuilder.DropColumn(name: "LastModifiedByUser", table: "ErpCustomReportLayouts");
            migrationBuilder.DropColumn(name: "LayoutLastUpdated", table: "ErpCustomReportLayouts");
        }
    }
}
