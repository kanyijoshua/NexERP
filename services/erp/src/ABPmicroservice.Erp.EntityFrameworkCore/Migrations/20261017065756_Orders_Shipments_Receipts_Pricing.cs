using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Orders_Shipments_Receipts_Pricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InvoicedQuantity",
                table: "ErpValueEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BlanketOrderNos",
                table: "ErpSalesReceivablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedReturnReceiptNos",
                table: "ErpSalesReceivablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedShipmentNos",
                table: "ErpSalesReceivablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnOrderNos",
                table: "ErpSalesReceivablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BlanketOrderLineNo",
                table: "ErpSalesLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlanketOrderNo",
                table: "ErpSalesLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpSalesLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyToInvoice",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyToShip",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityInvoiced",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityShipped",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipmentDate",
                table: "ErpSalesLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentLineId",
                table: "ErpSalesLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShipmentLineNo",
                table: "ErpSalesLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentNo",
                table: "ErpSalesLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPriceGroup",
                table: "ErpSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPostingNo",
                table: "ErpSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastShippingNo",
                table: "ErpSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuoteNo",
                table: "ErpSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToAddress",
                table: "ErpSalesHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToCity",
                table: "ErpSalesHeaders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToCode",
                table: "ErpSalesHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToContact",
                table: "ErpSalesHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToCountryRegionCode",
                table: "ErpSalesHeaders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToName",
                table: "ErpSalesHeaders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToPostCode",
                table: "ErpSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipmentDate",
                table: "ErpSalesHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementPlanNos",
                table: "ErpPurchasesPayablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequisitionNos",
                table: "ErpPurchasesPayablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RfqNos",
                table: "ErpPurchasesPayablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenderNos",
                table: "ErpPurchasesPayablesSetups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BlanketOrderLineNo",
                table: "ErpPurchaseLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlanketOrderNo",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptLineId",
                table: "ErpPurchaseLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptLineNo",
                table: "ErpPurchaseLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNo",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelComments",
                table: "ErpPurchaseHeaders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "ErpPurchaseHeaders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloseComments",
                table: "ErpPurchaseHeaders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedBy",
                table: "ErpPurchaseHeaders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractCommencementDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractCompletionDate",
                table: "ErpPurchaseHeaders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPostingNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReceivingNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LpoClosed",
                table: "ErpPurchaseHeaders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderStage",
                table: "ErpPurchaseHeaders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequisitionNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenderNo",
                table: "ErpPurchaseHeaders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountIncludingVat",
                table: "ErpPostedSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineDiscountPercent",
                table: "ErpPostedSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OrderLineNo",
                table: "ErpPostedSalesLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShipmentLineNo",
                table: "ErpPostedSalesLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentNo",
                table: "ErpPostedSalesLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasureCode",
                table: "ErpPostedSalesLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "ErpPostedSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatPercent",
                table: "ErpPostedSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "CreditMemo",
                table: "ErpPostedSalesHeaders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDocumentNo",
                table: "ErpPostedSalesHeaders",
                type: "character varying(35)",
                maxLength: 35,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNo",
                table: "ErpPostedSalesHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountIncludingVat",
                table: "ErpPostedPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineDiscountPercent",
                table: "ErpPostedPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OrderLineNo",
                table: "ErpPostedPurchaseLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptLineNo",
                table: "ErpPostedPurchaseLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNo",
                table: "ErpPostedPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasureCode",
                table: "ErpPostedPurchaseLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "ErpPostedPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatPercent",
                table: "ErpPostedPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "CreditMemo",
                table: "ErpPostedPurchaseHeaders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OrderNo",
                table: "ErpPostedPurchaseHeaders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorInvoiceNo",
                table: "ErpPostedPurchaseHeaders",
                type: "character varying(35)",
                maxLength: 35,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InvoicedQuantity",
                table: "ErpItemLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ErpCustomerPriceGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowLineDiscount = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpCustomerPriceGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedPurchaseReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Return = table.Column<bool>(type: "boolean", nullable: false),
                    OrderNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyFromVendorNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BuyFromVendorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VendorShipmentNo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPostedPurchaseReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedSalesShipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Return = table.Column<bool>(type: "boolean", nullable: false),
                    OrderNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellToCustomerNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SellToCustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ExternalDocumentNo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ShipToName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShipToAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShipToCity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShipToPostCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ShipToCountryRegionCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPostedSalesShipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPriceLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PriceType = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    StartingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpPriceLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpProcurementPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BudgetName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalEstimatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ErpProcurementPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPurchaseRequisitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DepartmentCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProcurementPlanCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TenderNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PurchaseOrderNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TotalEstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ErpPurchaseRequisitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpTenderHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TenderType = table.Column<int>(type: "integer", nullable: false),
                    RequisitionNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProcurementPlanCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProcurementPlanItemNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosingTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ValidityDays = table.Column<int>(type: "integer", nullable: false),
                    TenderSecurityAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AwardedVendorNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AwardedVendorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AwardedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AwardDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PurchaseOrderNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TotalEstimatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ErpTenderHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedPurchaseReceiptLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedPurchaseReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DirectUnitCost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderLineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemLedgerEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityInvoiced = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Correction = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPostedPurchaseReceiptLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPostedPurchaseReceiptLines_ErpPostedPurchaseReceipts_Pos~",
                        column: x => x.PostedPurchaseReceiptId,
                        principalTable: "ErpPostedPurchaseReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedSalesShipmentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedSalesShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderLineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemLedgerEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityInvoiced = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Correction = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPostedSalesShipmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPostedSalesShipmentLines_ErpPostedSalesShipments_PostedS~",
                        column: x => x.PostedSalesShipmentId,
                        principalTable: "ErpPostedSalesShipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpPriceListLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceListId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    AssetNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MinimumQuantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPriceListLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPriceListLines_ErpPriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "ErpPriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpProcurementPlanLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanItemNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcurementType = table.Column<int>(type: "integer", nullable: false),
                    ProcurementMethod = table.Column<int>(type: "integer", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SourceOfFunds = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DepartmentCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Quarter = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpProcurementPlanLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpProcurementPlanLines_ErpProcurementPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "ErpProcurementPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpPurchaseRequisitionLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Specifications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProcurementPlanItemNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedTotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OrderedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_ErpPurchaseRequisitionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPurchaseRequisitionLines_ErpPurchaseRequisitions_Requisi~",
                        column: x => x.RequisitionId,
                        principalTable: "ErpPurchaseRequisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpTenderBids",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidderNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BidderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryPeriodDays = table.Column<int>(type: "integer", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PassedMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    TechnicalScore = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    FinancialScore = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Awarded = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ErpTenderBids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpTenderBids_ErpTenderHeaders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "ErpTenderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpTenderCommittees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeType = table.Column<int>(type: "integer", nullable: false),
                    StaffNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StaffName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_ErpTenderCommittees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpTenderCommittees_ErpTenderHeaders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "ErpTenderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpTenderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    No = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Specifications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpTenderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpTenderLines_ErpTenderHeaders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "ErpTenderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpTenderMandatoryRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ErpTenderMandatoryRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpTenderMandatoryRequirements_ErpTenderHeaders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "ErpTenderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpSalesLines_ShipmentLineId",
                table: "ErpSalesLines",
                column: "ShipmentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPurchaseLines_ReceiptLineId",
                table: "ErpPurchaseLines",
                column: "ReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpCustomerPriceGroups_TenantId_CompanyId_Code",
                table: "ErpCustomerPriceGroups",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedPurchaseReceiptLines_OrderLineId",
                table: "ErpPostedPurchaseReceiptLines",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedPurchaseReceiptLines_PostedPurchaseReceiptId",
                table: "ErpPostedPurchaseReceiptLines",
                column: "PostedPurchaseReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedPurchaseReceipts_CompanyId_OrderNo",
                table: "ErpPostedPurchaseReceipts",
                columns: new[] { "CompanyId", "OrderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedPurchaseReceipts_CompanyId_VendorId_Return",
                table: "ErpPostedPurchaseReceipts",
                columns: new[] { "CompanyId", "VendorId", "Return" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedPurchaseReceipts_TenantId_CompanyId_No",
                table: "ErpPostedPurchaseReceipts",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedSalesShipmentLines_OrderLineId",
                table: "ErpPostedSalesShipmentLines",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedSalesShipmentLines_PostedSalesShipmentId",
                table: "ErpPostedSalesShipmentLines",
                column: "PostedSalesShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedSalesShipments_CompanyId_CustomerId_Return",
                table: "ErpPostedSalesShipments",
                columns: new[] { "CompanyId", "CustomerId", "Return" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedSalesShipments_CompanyId_OrderNo",
                table: "ErpPostedSalesShipments",
                columns: new[] { "CompanyId", "OrderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedSalesShipments_TenantId_CompanyId_No",
                table: "ErpPostedSalesShipments",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPriceListLines_PriceListId",
                table: "ErpPriceListLines",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPriceLists_TenantId_CompanyId_Code",
                table: "ErpPriceLists",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpProcurementPlanLines_CompanyId_PlanId_PlanItemNo",
                table: "ErpProcurementPlanLines",
                columns: new[] { "CompanyId", "PlanId", "PlanItemNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpProcurementPlanLines_PlanId",
                table: "ErpProcurementPlanLines",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpProcurementPlans_TenantId_CompanyId_Code",
                table: "ErpProcurementPlans",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPurchaseRequisitionLines_CompanyId_RequisitionId_LineNo",
                table: "ErpPurchaseRequisitionLines",
                columns: new[] { "CompanyId", "RequisitionId", "LineNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPurchaseRequisitionLines_RequisitionId",
                table: "ErpPurchaseRequisitionLines",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPurchaseRequisitions_CompanyId_Status_RequestDate",
                table: "ErpPurchaseRequisitions",
                columns: new[] { "CompanyId", "Status", "RequestDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPurchaseRequisitions_TenantId_CompanyId_No",
                table: "ErpPurchaseRequisitions",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderBids_CompanyId_TenderId_BidderNo",
                table: "ErpTenderBids",
                columns: new[] { "CompanyId", "TenderId", "BidderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderBids_TenderId",
                table: "ErpTenderBids",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderCommittees_CompanyId_TenderId_CommitteeType",
                table: "ErpTenderCommittees",
                columns: new[] { "CompanyId", "TenderId", "CommitteeType" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderCommittees_TenderId",
                table: "ErpTenderCommittees",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderHeaders_CompanyId_Status_ClosingDate",
                table: "ErpTenderHeaders",
                columns: new[] { "CompanyId", "Status", "ClosingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderHeaders_TenantId_CompanyId_No",
                table: "ErpTenderHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderLines_CompanyId_TenderId_LineNo",
                table: "ErpTenderLines",
                columns: new[] { "CompanyId", "TenderId", "LineNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderLines_TenderId",
                table: "ErpTenderLines",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpTenderMandatoryRequirements_TenderId",
                table: "ErpTenderMandatoryRequirements",
                column: "TenderId");

            // Everything posted before orders were shipped in parts was shipped and invoiced at once.
            migrationBuilder.Sql("UPDATE \"ErpItemLedgerEntries\" SET \"InvoicedQuantity\" = \"Quantity\";");
            migrationBuilder.Sql("UPDATE \"ErpValueEntries\" SET \"InvoicedQuantity\" = \"ValuationQuantity\";");
            migrationBuilder.Sql("UPDATE \"ErpSalesLines\" l SET \"QuantityShipped\" = l.\"Quantity\", \"QuantityInvoiced\" = l.\"Quantity\" FROM \"ErpSalesHeaders\" h WHERE h.\"Id\" = l.\"SalesHeaderId\" AND h.\"Posted\";");
            migrationBuilder.Sql("UPDATE \"ErpSalesLines\" l SET \"QtyToShip\" = l.\"Quantity\", \"QtyToInvoice\" = l.\"Quantity\" FROM \"ErpSalesHeaders\" h WHERE h.\"Id\" = l.\"SalesHeaderId\" AND NOT h.\"Posted\";");
            migrationBuilder.Sql("UPDATE \"ErpPurchaseLines\" l SET \"QuantityReceived\" = l.\"Quantity\", \"QuantityInvoiced\" = l.\"Quantity\", \"QtyToReceive\" = 0, \"QtyToInvoice\" = 0 FROM \"ErpPurchaseHeaders\" h WHERE h.\"Id\" = l.\"PurchaseHeaderId\" AND h.\"Posted\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpCustomerPriceGroups");

            migrationBuilder.DropTable(
                name: "ErpPostedPurchaseReceiptLines");

            migrationBuilder.DropTable(
                name: "ErpPostedSalesShipmentLines");

            migrationBuilder.DropTable(
                name: "ErpPriceListLines");

            migrationBuilder.DropTable(
                name: "ErpProcurementPlanLines");

            migrationBuilder.DropTable(
                name: "ErpPurchaseRequisitionLines");

            migrationBuilder.DropTable(
                name: "ErpTenderBids");

            migrationBuilder.DropTable(
                name: "ErpTenderCommittees");

            migrationBuilder.DropTable(
                name: "ErpTenderLines");

            migrationBuilder.DropTable(
                name: "ErpTenderMandatoryRequirements");

            migrationBuilder.DropTable(
                name: "ErpPostedPurchaseReceipts");

            migrationBuilder.DropTable(
                name: "ErpPostedSalesShipments");

            migrationBuilder.DropTable(
                name: "ErpPriceLists");

            migrationBuilder.DropTable(
                name: "ErpProcurementPlans");

            migrationBuilder.DropTable(
                name: "ErpPurchaseRequisitions");

            migrationBuilder.DropTable(
                name: "ErpTenderHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ErpSalesLines_ShipmentLineId",
                table: "ErpSalesLines");

            migrationBuilder.DropIndex(
                name: "IX_ErpPurchaseLines_ReceiptLineId",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "InvoicedQuantity",
                table: "ErpValueEntries");

            migrationBuilder.DropColumn(
                name: "BlanketOrderNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "PostedReturnReceiptNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "PostedShipmentNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "ReturnOrderNos",
                table: "ErpSalesReceivablesSetups");

            migrationBuilder.DropColumn(
                name: "BlanketOrderLineNo",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "BlanketOrderNo",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "QtyToInvoice",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "QtyToShip",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "QuantityInvoiced",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "QuantityShipped",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "ShipmentDate",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "ShipmentLineId",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "ShipmentLineNo",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "ShipmentNo",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "CustomerPriceGroup",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "LastPostingNo",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "LastShippingNo",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "QuoteNo",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToAddress",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToCity",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToCode",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToContact",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToCountryRegionCode",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToName",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipToPostCode",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ShipmentDate",
                table: "ErpSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ProcurementPlanNos",
                table: "ErpPurchasesPayablesSetups");

            migrationBuilder.DropColumn(
                name: "RequisitionNos",
                table: "ErpPurchasesPayablesSetups");

            migrationBuilder.DropColumn(
                name: "RfqNos",
                table: "ErpPurchasesPayablesSetups");

            migrationBuilder.DropColumn(
                name: "TenderNos",
                table: "ErpPurchasesPayablesSetups");

            migrationBuilder.DropColumn(
                name: "BlanketOrderLineNo",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "BlanketOrderNo",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "ReceiptLineId",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "ReceiptLineNo",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "ReceiptNo",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "CancelComments",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "CloseComments",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ClosedBy",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ContractCommencementDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "ContractCompletionDate",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "LastPostingNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "LastReceivingNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "LpoClosed",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "OrderStage",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "RequisitionNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "TenderNo",
                table: "ErpPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "AmountIncludingVat",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "LineDiscountPercent",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "OrderLineNo",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "ShipmentLineNo",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "ShipmentNo",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureCode",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "VatPercent",
                table: "ErpPostedSalesLines");

            migrationBuilder.DropColumn(
                name: "CreditMemo",
                table: "ErpPostedSalesHeaders");

            migrationBuilder.DropColumn(
                name: "ExternalDocumentNo",
                table: "ErpPostedSalesHeaders");

            migrationBuilder.DropColumn(
                name: "OrderNo",
                table: "ErpPostedSalesHeaders");

            migrationBuilder.DropColumn(
                name: "AmountIncludingVat",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "LineDiscountPercent",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "OrderLineNo",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "ReceiptLineNo",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "ReceiptNo",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureCode",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VatPercent",
                table: "ErpPostedPurchaseLines");

            migrationBuilder.DropColumn(
                name: "CreditMemo",
                table: "ErpPostedPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "OrderNo",
                table: "ErpPostedPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "VendorInvoiceNo",
                table: "ErpPostedPurchaseHeaders");

            migrationBuilder.DropColumn(
                name: "InvoicedQuantity",
                table: "ErpItemLedgerEntries");
        }
    }
}
