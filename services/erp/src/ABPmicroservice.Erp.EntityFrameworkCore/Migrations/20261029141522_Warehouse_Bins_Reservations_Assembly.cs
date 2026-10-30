using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPmicroservice.Erp.Migrations
{
    /// <inheritdoc />
    public partial class Warehouse_Bins_Reservations_Assembly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DropShipment",
                table: "ErpSalesLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseOrderLineId",
                table: "ErpSalesLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderNo",
                table: "ErpSalesLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DropShipment",
                table: "ErpPurchaseLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderLineId",
                table: "ErpPurchaseLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesOrderNo",
                table: "ErpPurchaseLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BinMandatory",
                table: "ErpLocations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DefaultBinCode",
                table: "ErpLocations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptBinCode",
                table: "ErpLocations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePick",
                table: "ErpLocations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePutAway",
                table: "ErpLocations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentBinCode",
                table: "ErpLocations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCountingDate",
                table: "ErpItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextCountingDate",
                table: "ErpItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysInvtCountingPeriodCode",
                table: "ErpItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BinCode",
                table: "ErpItemJournalLines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErpAssemblyBomComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ComponentItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    QuantityPer = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpAssemblyBomComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpAssemblyHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QuantityToAssemble = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    AssembledQuantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
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
                    table.PrimaryKey("PK_ErpAssemblyHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpBins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_ErpBins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPhysInvtCountingPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountFrequencyPerYear = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ErpPhysInvtCountingPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpPhysInvtOrderHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_ErpPhysInvtOrderHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpReservationEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SourceLineNo = table.Column<int>(type: "integer", nullable: false),
                    ShipmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpReservationEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpWarehouseActivityHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    No = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RegisteredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ErpWarehouseActivityHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpWarehouseEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteringDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    DocumentNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ItemLedgerEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNo = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpWarehouseEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpAssemblyLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssemblyHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    QuantityPer = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ConsumedQuantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpAssemblyLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpAssemblyLines_ErpAssemblyHeaders_AssemblyHeaderId",
                        column: x => x.AssemblyHeaderId,
                        principalTable: "ErpAssemblyHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpPhysInvtOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhysInvtOrderHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LocationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LotNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SerialNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    QtyExpected = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QtyRecorded = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Recorded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpPhysInvtOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpPhysInvtOrderLines_ErpPhysInvtOrderHeaders_PhysInvtOrder~",
                        column: x => x.PhysInvtOrderHeaderId,
                        principalTable: "ErpPhysInvtOrderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErpWarehouseActivityLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseActivityHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VariantCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FromBinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ToBinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QtyToHandle = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    QtyHandled = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    SourceLineId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpWarehouseActivityLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpWarehouseActivityLines_ErpWarehouseActivityHeaders_Wareh~",
                        column: x => x.WarehouseActivityHeaderId,
                        principalTable: "ErpWarehouseActivityHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpAssemblyBomComponents_CompanyId_ParentItemNo",
                table: "ErpAssemblyBomComponents",
                columns: new[] { "CompanyId", "ParentItemNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpAssemblyHeaders_TenantId_CompanyId_No",
                table: "ErpAssemblyHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpAssemblyLines_AssemblyHeaderId",
                table: "ErpAssemblyLines",
                column: "AssemblyHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpBins_TenantId_CompanyId_LocationCode_Code",
                table: "ErpBins",
                columns: new[] { "TenantId", "CompanyId", "LocationCode", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPhysInvtCountingPeriods_TenantId_CompanyId_Code",
                table: "ErpPhysInvtCountingPeriods",
                columns: new[] { "TenantId", "CompanyId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPhysInvtOrderHeaders_TenantId_CompanyId_No",
                table: "ErpPhysInvtOrderHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpPhysInvtOrderLines_PhysInvtOrderHeaderId",
                table: "ErpPhysInvtOrderLines",
                column: "PhysInvtOrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpReservationEntries_CompanyId_ItemId_LocationCode",
                table: "ErpReservationEntries",
                columns: new[] { "CompanyId", "ItemId", "LocationCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpReservationEntries_SourceId",
                table: "ErpReservationEntries",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpWarehouseActivityHeaders_TenantId_CompanyId_No",
                table: "ErpWarehouseActivityHeaders",
                columns: new[] { "TenantId", "CompanyId", "No" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErpWarehouseActivityLines_SourceLineId",
                table: "ErpWarehouseActivityLines",
                column: "SourceLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpWarehouseActivityLines_WarehouseActivityHeaderId",
                table: "ErpWarehouseActivityLines",
                column: "WarehouseActivityHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpWarehouseEntries_CompanyId_LocationCode_BinCode_ItemId",
                table: "ErpWarehouseEntries",
                columns: new[] { "CompanyId", "LocationCode", "BinCode", "ItemId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpAssemblyBomComponents");

            migrationBuilder.DropTable(
                name: "ErpAssemblyLines");

            migrationBuilder.DropTable(
                name: "ErpBins");

            migrationBuilder.DropTable(
                name: "ErpPhysInvtCountingPeriods");

            migrationBuilder.DropTable(
                name: "ErpPhysInvtOrderLines");

            migrationBuilder.DropTable(
                name: "ErpReservationEntries");

            migrationBuilder.DropTable(
                name: "ErpWarehouseActivityLines");

            migrationBuilder.DropTable(
                name: "ErpWarehouseEntries");

            migrationBuilder.DropTable(
                name: "ErpAssemblyHeaders");

            migrationBuilder.DropTable(
                name: "ErpPhysInvtOrderHeaders");

            migrationBuilder.DropTable(
                name: "ErpWarehouseActivityHeaders");

            migrationBuilder.DropColumn(
                name: "DropShipment",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderLineId",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderNo",
                table: "ErpSalesLines");

            migrationBuilder.DropColumn(
                name: "DropShipment",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "SalesOrderLineId",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "SalesOrderNo",
                table: "ErpPurchaseLines");

            migrationBuilder.DropColumn(
                name: "BinMandatory",
                table: "ErpLocations");

            migrationBuilder.DropColumn(
                name: "DefaultBinCode",
                table: "ErpLocations");

            migrationBuilder.DropColumn(
                name: "ReceiptBinCode",
                table: "ErpLocations");

            migrationBuilder.DropColumn(
                name: "RequirePick",
                table: "ErpLocations");

            migrationBuilder.DropColumn(
                name: "RequirePutAway",
                table: "ErpLocations");

            migrationBuilder.DropColumn(
                name: "ShipmentBinCode",
                table: "ErpLocations");

            migrationBuilder.DropColumn(
                name: "LastCountingDate",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "NextCountingDate",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "PhysInvtCountingPeriodCode",
                table: "ErpItems");

            migrationBuilder.DropColumn(
                name: "BinCode",
                table: "ErpItemJournalLines");
        }
    }
}
