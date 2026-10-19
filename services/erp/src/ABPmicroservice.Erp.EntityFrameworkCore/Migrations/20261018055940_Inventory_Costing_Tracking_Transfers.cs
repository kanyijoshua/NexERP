using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Inventory_Costing_Tracking_Transfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntryType",
                table: "ErpValueEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpValueEntries",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                table: "ErpValueEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyPerUnitOfMeasure",
                table: "ErpSalesLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                table: "ErpSalesLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyPerUnitOfMeasure",
                table: "ErpPurchaseLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyPerUnitOfMeasure",
                table: "ErpPostedSalesShipmentLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                table: "ErpPostedSalesShipmentLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyPerUnitOfMeasure",
                table: "ErpPostedPurchaseReceiptLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                table: "ErpPostedPurchaseReceiptLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseAsInTransit",
                table: "ErpLocations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CostingMethod",
                table: "ErpItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ItemTrackingCode",
                table: "ErpItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastDirectCost",
                table: "ErpItems",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostAmountActual",
                table: "ErpItemLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentLineId",
                table: "ErpItemLedgerEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedUnitCost",
                table: "ErpItemLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "ErpItemLedgerEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                table: "ErpItemLedgerEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Open",
                table: "ErpItemLedgerEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Positive",
                table: "ErpItemLedgerEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingQuantity",
                table: "ErpItemLedgerEntries",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SerialNo",
                table: "ErpItemLedgerEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                table: "ErpItemLedgerEntries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemNo",
                table: "ErpItemJournalLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNo",
                table: "ErpItemJournalLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ErpItemJournalLines",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "ErpItemJournalLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "ErpItemJournalLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                table: "ErpItemJournalLines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewLocationCode",
                table: "ErpItemJournalLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyCalculated",
                table: "ErpItemJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyPerUnitOfMeasure",
                table: "ErpItemJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyPhysInventory",
                table: "ErpItemJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SerialNo",
                table: "ErpItemJournalLines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCostCalculated",
                table: "ErpItemJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCostRevalued",
                table: "ErpItemJournalLines",
                type: "numeric(18,5)",
                precision: 18,
                scale: 5,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasureCode",
                table: "ErpItemJournalLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantCode",
                table: "ErpItemJournalLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedTransferReceiptNos",
                table: "ErpInventorySetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedTransferShipmentNos",
                table: "ErpInventorySetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferOrderNos",
                table: "ErpInventorySetups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpItemApplicationEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InboundEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboundEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpItemApplicationEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemJournalTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpItemJournalTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemTrackingCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LotSpecific = table.Column<bool>(type: "boolean", nullable: false),
                    SerialSpecific = table.Column<bool>(type: "boolean", nullable: false),
                    ExpirationDateRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpItemTrackingCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemTrackingLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LotNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SerialNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QuantityHandled = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpItemTrackingLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemUnitsOfMeasure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    QtyPerUnitOfMeasure = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpItemUnitsOfMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpItemVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpItemVariants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpLotNoInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LotNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpLotNoInformation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Receipt = table.Column<bool>(type: "boolean", nullable: false),
                    TransferOrderNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FromLocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ToLocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    InTransitCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPostedTransfers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpSerialNoInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SerialNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ErpSerialNoInformation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpTransferHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FromLocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ToLocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    InTransitCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShipmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    LastShipmentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastReceiptNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_ErpTransferHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPostedTransferLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    TransferLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPostedTransferLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPostedTransferLines_ErpPostedTransfers_PostedTransferId",
                        column: x => x.PostedTransferId,
                        principalTable: "ErpPostedTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpTransferLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    UnitOfMeasureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    QtyPerUnitOfMeasure = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QtyToShip = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QuantityShipped = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QtyToReceive = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpTransferLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpTransferLines_ErpTransferHeaders_TransferHeaderId",
                        column: x => x.TransferHeaderId,
                        principalTable: "ErpTransferHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpValueEntries_ItemLedgerEntryId",
                table: "ErpValueEntries",
                column: "ItemLedgerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemLedgerEntries_CompanyId_ItemId_LocationCode_VariantC~",
                table: "ErpItemLedgerEntries",
                columns: new[] { "CompanyId", "ItemId", "LocationCode", "VariantCode", "Open", "Positive" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemLedgerEntries_DocumentLineId",
                table: "ErpItemLedgerEntries",
                column: "DocumentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemJournalLines_ItemJournalBatchId_LineNo",
                table: "ErpItemJournalLines",
                columns: new[] { "ItemJournalBatchId", "LineNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemApplicationEntries_InboundEntryId",
                table: "ErpItemApplicationEntries",
                column: "InboundEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemApplicationEntries_OutboundEntryId",
                table: "ErpItemApplicationEntries",
                column: "OutboundEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemJournalTemplates_TenantId_CompanyId_Name",
                table: "ErpItemJournalTemplates",
                columns: new[] { "TenantId", "CompanyId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemTrackingCodes_TenantId_CompanyId_Code",
                table: "ErpItemTrackingCodes",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemTrackingLines_SourceType_SourceId",
                table: "ErpItemTrackingLines",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemUnitsOfMeasure_CompanyId_ItemNo_Code",
                table: "ErpItemUnitsOfMeasure",
                columns: new[] { "CompanyId", "ItemNo", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpItemVariants_CompanyId_ItemNo_Code",
                table: "ErpItemVariants",
                columns: new[] { "CompanyId", "ItemNo", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpLotNoInformation_CompanyId_ItemNo_VariantCode_LotNo",
                table: "ErpLotNoInformation",
                columns: new[] { "CompanyId", "ItemNo", "VariantCode", "LotNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedTransferLines_PostedTransferId",
                table: "ErpPostedTransferLines",
                column: "PostedTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedTransferLines_TransferLineId",
                table: "ErpPostedTransferLines",
                column: "TransferLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedTransfers_CompanyId_TransferOrderNo",
                table: "ErpPostedTransfers",
                columns: new[] { "CompanyId", "TransferOrderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpPostedTransfers_TenantId_CompanyId_No",
                table: "ErpPostedTransfers",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpSerialNoInformation_CompanyId_ItemNo_VariantCode_SerialNo",
                table: "ErpSerialNoInformation",
                columns: new[] { "CompanyId", "ItemNo", "VariantCode", "SerialNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpTransferHeaders_TenantId_CompanyId_No",
                table: "ErpTransferHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpTransferLines_TransferHeaderId",
                table: "ErpTransferLines",
                column: "TransferHeaderId");

            // Entries posted before costing applied stock: the newest receipts hold what is still in stock (FIFO).
            migrationBuilder.Sql("UPDATE \"ErpItemLedgerEntries\" SET \"Positive\" = \"Quantity\" > 0, \"ExpectedUnitCost\" = \"UnitCost\", \"CostAmountActual\" = ROUND(\"InvoicedQuantity\" * \"UnitCost\", 2);");
            migrationBuilder.Sql("UPDATE \"ErpItemLedgerEntries\" e SET \"RemainingQuantity\" = x.rem, \"Open\" = x.rem > 0 FROM (SELECT p.\"Id\", GREATEST(0, LEAST(p.\"Quantity\", n.net - (SUM(p.\"Quantity\") OVER (PARTITION BY p.\"ItemId\", p.\"LocationCode\", p.\"VariantCode\" ORDER BY p.\"EntryNo\" DESC, p.\"Id\" ROWS UNBOUNDED PRECEDING) - p.\"Quantity\"))) AS rem FROM \"ErpItemLedgerEntries\" p JOIN (SELECT \"ItemId\", \"LocationCode\", \"VariantCode\", SUM(\"Quantity\") AS net FROM \"ErpItemLedgerEntries\" GROUP BY \"ItemId\", \"LocationCode\", \"VariantCode\") n ON n.\"ItemId\" = p.\"ItemId\" AND n.\"LocationCode\" IS NOT DISTINCT FROM p.\"LocationCode\" AND n.\"VariantCode\" IS NOT DISTINCT FROM p.\"VariantCode\" WHERE p.\"Quantity\" > 0) x WHERE e.\"Id\" = x.\"Id\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpItemApplicationEntries");

            migrationBuilder.DropTable(
                name: "ErpItemJournalTemplates");

            migrationBuilder.DropTable(
                name: "ErpItemTrackingCodes");

            migrationBuilder.DropTable(
                name: "ErpItemTrackingLines");

            migrationBuilder.DropTable(
                name: "ErpItemUnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "ErpItemVariants");

            migrationBuilder.DropTable(
                name: "ErpLotNoInformation");

            migrationBuilder.DropTable(
                name: "ErpPostedTransferLines");

            migrationBuilder.DropTable(
                name: "ErpSerialNoInformation");

            migrationBuilder.DropTable(
                name: "ErpTransferLines");

            migrationBuilder.DropTable(
                name: "ErpPostedTransfers");

            migrationBuilder.DropTable(
                name: "ErpTransferHeaders");

            migrationBuilder.DropIndex(
                name: "IX_ErpValueEntries_ItemLedgerEntryId",
                table: "ErpValueEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpItemLedgerEntries_CompanyId_ItemId_LocationCode_VariantC~",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpItemLedgerEntries_DocumentLineId",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_ErpItemJournalLines_ItemJournalBatchId_LineNo",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "EntryType",
                table: "ErpValueEntries");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "ErpValueEntries");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                table: "ErpValueEntries");

            migrationBuilder.DropColumn(
                name: "QtyPerUnitOfMeasure",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "QtyPerUnitOfMeasure",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "QtyPerUnitOfMeasure",
                table: "ErpPostedSalesShipmentLines");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                table: "ErpPostedSalesShipmentLines");

            migrationBuilder.DropColumn(
                name: "QtyPerUnitOfMeasure",
                table: "ErpPostedPurchaseReceiptLines");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                table: "ErpPostedPurchaseReceiptLines");

            migrationBuilder.DropColumn(
                name: "UseAsInTransit",
                table: "ErpLocations");

            migrationBuilder.DropColumn(
                name: "CostingMethod",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "ItemTrackingCode",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "LastDirectCost",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "CostAmountActual",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "DocumentLineId",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ExpectedUnitCost",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "LotNo",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "Open",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "Positive",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "RemainingQuantity",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "SerialNo",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                table: "ErpItemLedgerEntries");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "LotNo",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "NewLocationCode",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "QtyCalculated",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "QtyPerUnitOfMeasure",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "QtyPhysInventory",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "SerialNo",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "UnitCostCalculated",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "UnitCostRevalued",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureCode",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "VariantCode",
                table: "ErpItemJournalLines");

            migrationBuilder.DropColumn(
                name: "PostedTransferReceiptNos",
                table: "ErpInventorySetups");

            migrationBuilder.DropColumn(
                name: "PostedTransferShipmentNos",
                table: "ErpInventorySetups");

            migrationBuilder.DropColumn(
                name: "TransferOrderNos",
                table: "ErpInventorySetups");

            migrationBuilder.AlterColumn<string>(
                name: "ItemNo",
                table: "ErpItemJournalLines",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNo",
                table: "ErpItemJournalLines",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ErpItemJournalLines",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}
