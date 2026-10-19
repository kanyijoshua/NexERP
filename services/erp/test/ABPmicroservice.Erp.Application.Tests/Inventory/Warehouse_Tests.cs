using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Costing (FIFO, average, specific cost), lot and serial tracking, item journals (adjustment,
/// reclassification, physical inventory, revaluation), transfer orders and inventory valuation.
/// The seed has the item journal templates ITEM, RECLASS, PHYS INVT and REVAL with a DEFAULT
/// batch each, tracking codes LOTALL and SNALL, location MAIN and the IN-TRANSIT location.
/// </summary>
public class Warehouse_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    private readonly IItemAppService _items;
    private readonly IItemJournalBatchAppService _batches;
    private readonly IItemJournalLineAppService _journalLines;
    private readonly ITransferOrderAppService _transfers;
    private readonly ITransferLineAppService _transferLines;
    private readonly ILocationAppService _locations;
    private readonly IInventorySetupAppService _inventorySetup;
    private readonly IItemTrackingLineAppService _trackingLines;
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly ICustomerAppService _customers;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemEntries;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public Warehouse_Tests()
    {
        _items = GetRequiredService<IItemAppService>();
        _batches = GetRequiredService<IItemJournalBatchAppService>();
        _journalLines = GetRequiredService<IItemJournalLineAppService>();
        _transfers = GetRequiredService<ITransferOrderAppService>();
        _transferLines = GetRequiredService<ITransferLineAppService>();
        _locations = GetRequiredService<ILocationAppService>();
        _inventorySetup = GetRequiredService<IInventorySetupAppService>();
        _trackingLines = GetRequiredService<IItemTrackingLineAppService>();
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _itemEntries = GetRequiredService<IRepository<ItemLedgerEntry, Guid>>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task Fifo_Takes_The_Oldest_Receipts_First_At_Their_Own_Cost()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("FIFO-1", CostingMethod.Fifo);
            await AdjustAsync("FIFO-1", ItemLedgerEntryType.PositiveAdjmt, 5, 10m, "ADJ-1");
            await AdjustAsync("FIFO-1", ItemLedgerEntryType.PositiveAdjmt, 5, 14m, "ADJ-2");

            await AdjustAsync("FIFO-1", ItemLedgerEntryType.NegativeAdjmt, 7, 0m, "ADJ-3");

            var outbound = (await _itemEntries.GetListAsync(e => e.ItemNo == "FIFO-1" && e.DocumentNo == "ADJ-3")).ShouldHaveSingleItem();
            Math.Round(outbound.UnitCost * 7, 2).ShouldBe(78m); // 5 x 10 + 2 x 14
            outbound.Open.ShouldBeFalse();

            var receipts = (await _itemEntries.GetListAsync(e => e.ItemNo == "FIFO-1" && e.Positive)).OrderBy(e => e.EntryNo).ToList();
            receipts[0].RemainingQuantity.ShouldBe(0m);
            receipts[0].Open.ShouldBeFalse();
            receipts[1].RemainingQuantity.ShouldBe(3m);

            // The cost left the inventory account for the adjustment account.
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "ADJ-3");
            Sum(entries, "1400").ShouldBe(-78m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task Average_Cost_Items_Leave_At_The_Running_Average()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("AVG-1", CostingMethod.Average);
            await AdjustAsync("AVG-1", ItemLedgerEntryType.PositiveAdjmt, 10, 10m, "AVG-ADJ-1");
            await AdjustAsync("AVG-1", ItemLedgerEntryType.PositiveAdjmt, 10, 20m, "AVG-ADJ-2");
            (await _items.GetByNoAsync("AVG-1")).UnitCost.ShouldBe(15m);

            await AdjustAsync("AVG-1", ItemLedgerEntryType.NegativeAdjmt, 5, 0m, "AVG-ADJ-3");

            var outbound = (await _itemEntries.GetListAsync(e => e.DocumentNo == "AVG-ADJ-3")).ShouldHaveSingleItem();
            outbound.UnitCost.ShouldBe(15m);
            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == "AVG-ADJ-3"), "1400").ShouldBe(-75m);
        });
    }

    [Fact]
    public async Task Specific_Cost_Items_Leave_At_The_Cost_Of_Their_Serial_Number()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("SN-1", CostingMethod.Specific, "SNALL");
            await AdjustAsync("SN-1", ItemLedgerEntryType.PositiveAdjmt, 1, 100m, "SN-ADJ-1", serialNo: "S1");
            await AdjustAsync("SN-1", ItemLedgerEntryType.PositiveAdjmt, 1, 200m, "SN-ADJ-2", serialNo: "S2");

            // The same serial number cannot come in twice.
            var duplicate = await Should.ThrowAsync<BusinessException>(() => AdjustAsync("SN-1", ItemLedgerEntryType.PositiveAdjmt, 1, 50m, "SN-ADJ-X", serialNo: "S1"));
            duplicate.Code.ShouldBe(ErpErrorCodes.Inventory.DuplicateSerialNo);

            // A tracked item cannot move without its serial number.
            var untracked = await Should.ThrowAsync<BusinessException>(() => AdjustAsync("SN-1", ItemLedgerEntryType.NegativeAdjmt, 1, 0m, "SN-ADJ-Y"));
            untracked.Code.ShouldBe(ErpErrorCodes.Inventory.TrackingRequired);

            await AdjustAsync("SN-1", ItemLedgerEntryType.NegativeAdjmt, 1, 0m, "SN-ADJ-3", serialNo: "S2");
            var outbound = (await _itemEntries.GetListAsync(e => e.DocumentNo == "SN-ADJ-3")).ShouldHaveSingleItem();
            outbound.UnitCost.ShouldBe(200m);
            outbound.SerialNo.ShouldBe("S2");
        });
    }

    [Fact]
    public async Task A_Sale_Of_A_Lot_Tracked_Item_Posts_An_Entry_Per_Lot()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("LOT-1", CostingMethod.Fifo, "LOTALL");
            await AdjustAsync("LOT-1", ItemLedgerEntryType.PositiveAdjmt, 3, 5m, "LOT-ADJ-1", lotNo: "L1");
            await AdjustAsync("LOT-1", ItemLedgerEntryType.PositiveAdjmt, 4, 6m, "LOT-ADJ-2", lotNo: "L2");

            var customer = await _customers.GetByNoAsync("C00010");
            var invoice = await _salesDocuments.CreateAsync(new CreateUpdateSalesHeaderDto
            {
                DocumentType = SalesDocumentType.Invoice,
                No = "INV-LOT-1",
                CustomerId = customer.Id,
                PostingDate = PostingDate,
                Lines = [new SalesLineInputDto { Type = DocumentLineType.Item, No = "LOT-1", Description = "Lot item", Quantity = 5, UnitPrice = 20m, LocationCode = "MAIN" }],
            });
            var lineId = invoice.Lines.Single().Id;

            // Without its lots the line cannot be posted.
            var missing = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunPostingAsync(invoice.Id));
            missing.Code.ShouldBe(ErpErrorCodes.Inventory.TrackingRequired);

            await _trackingLines.CreateAsync(new CreateUpdateItemTrackingLineDto { SourceType = ItemTrackingSourceType.SalesLine, SourceId = lineId, ItemNo = "LOT-1", LotNo = "L2", Quantity = 4 });
            await _trackingLines.CreateAsync(new CreateUpdateItemTrackingLineDto { SourceType = ItemTrackingSourceType.SalesLine, SourceId = lineId, ItemNo = "LOT-1", LotNo = "L1", Quantity = 1 });

            var posted = await _salesDocuments.RunPostingAsync(invoice.Id);

            var sold = await _itemEntries.GetListAsync(e => e.ItemNo == "LOT-1" && !e.Positive);
            sold.Count.ShouldBe(2);
            sold.Single(e => e.LotNo == "L2").Quantity.ShouldBe(-4m);
            sold.Single(e => e.LotNo == "L1").Quantity.ShouldBe(-1m);
            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo), "5000").ShouldBe(29m); // 4 x 6 + 1 x 5
        });
    }

    [Fact]
    public async Task A_Transfer_Order_Moves_Stock_Through_Transit()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _locations.CreateAsync(new CreateUpdateLocationDto { Code = "EAST", Description = "East warehouse" });
            await NewItemAsync("TRF-1", CostingMethod.Fifo);
            await AdjustAsync("TRF-1", ItemLedgerEntryType.PositiveAdjmt, 10, 8m, "TRF-ADJ-1");

            var order = await _transfers.CreateAsync(new CreateUpdateTransferHeaderDto
            {
                FromLocationCode = "MAIN",
                ToLocationCode = "EAST",
                InTransitCode = "IN-TRANSIT",
                PostingDate = PostingDate,
            });
            order.No.ShouldStartWith("TO-");
            await _transferLines.CreateAsync(new CreateUpdateTransferLineDto { TransferHeaderId = order.Id, ItemNo = "TRF-1", Quantity = 6 });

            await _transfers.ShipAsync(order.Id);
            (await AvailableAsync("TRF-1", "MAIN")).ShouldBe(4m);
            (await AvailableAsync("TRF-1", "IN-TRANSIT")).ShouldBe(6m);

            var received = await _transfers.ReceiveAsync(order.Id);
            received.Completed.ShouldBeTrue();
            (await AvailableAsync("TRF-1", "IN-TRANSIT")).ShouldBe(0m);
            (await AvailableAsync("TRF-1", "EAST")).ShouldBe(6m);

            // The stock kept its cost on the way.
            var arrived = (await _itemEntries.GetListAsync(e => e.ItemNo == "TRF-1" && e.LocationCode == "EAST")).ShouldHaveSingleItem();
            arrived.UnitCost.ShouldBe(8m);
        });
    }

    [Fact]
    public async Task A_Physical_Inventory_Posts_The_Counted_Difference()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("PHYS-1", CostingMethod.Fifo);
            await AdjustAsync("PHYS-1", ItemLedgerEntryType.PositiveAdjmt, 10, 3m, "PHYS-ADJ-1");

            var batch = await BatchAsync(ItemJournalTemplateType.PhysicalInventory);
            await _batches.CalculateInventoryAsync(batch.Id, new CalculateInventoryInput { PostingDate = PostingDate, DocumentNo = "COUNT-1", LocationCode = "MAIN" });

            var line = (await _journalLines.GetListAsync(new GetItemJournalLineListInput { ItemJournalBatchId = batch.Id, MaxResultCount = 1000 }))
                .Items.Single(l => l.ItemNo == "PHYS-1");
            line.QtyCalculated.ShouldBe(10m);

            await _journalLines.UpdateAsync(line.Id, new CreateUpdateItemJournalLineDto
            {
                ItemJournalBatchId = batch.Id,
                PostingDate = PostingDate,
                EntryType = line.EntryType,
                DocumentNo = line.DocumentNo,
                ItemNo = line.ItemNo,
                LocationCode = line.LocationCode,
                UnitCost = line.UnitCost,
                QtyPhysInventory = 8,
            });

            await _batches.RunPostingAsync(batch.Id);

            (await AvailableAsync("PHYS-1", "MAIN")).ShouldBe(8m);
            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == "COUNT-1" && e.SourceNo == "PHYS-1"), "1400").ShouldBe(-6m);
        });
    }

    [Fact]
    public async Task A_Revaluation_Changes_The_Cost_Of_The_Stock_On_Hand()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("REVAL-1", CostingMethod.Fifo);
            await AdjustAsync("REVAL-1", ItemLedgerEntryType.PositiveAdjmt, 4, 10m, "REVAL-ADJ-1");

            var batch = await BatchAsync(ItemJournalTemplateType.Revaluation);
            await _batches.CalculateInventoryValueAsync(batch.Id, new CalculateInventoryInput { PostingDate = PostingDate, DocumentNo = "REVAL-1" });
            var line = (await _journalLines.GetListAsync(new GetItemJournalLineListInput { ItemJournalBatchId = batch.Id, MaxResultCount = 1000 }))
                .Items.Single(l => l.ItemNo == "REVAL-1");
            line.UnitCostCalculated.ShouldBe(10m);

            await _journalLines.UpdateAsync(line.Id, new CreateUpdateItemJournalLineDto
            {
                ItemJournalBatchId = batch.Id,
                PostingDate = PostingDate,
                EntryType = line.EntryType,
                DocumentNo = line.DocumentNo,
                ItemNo = line.ItemNo,
                LocationCode = line.LocationCode,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                UnitCostRevalued = 12.5m,
            });
            await _batches.RunPostingAsync(batch.Id);

            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == "REVAL-1" && e.SourceNo == "REVAL-1"), "1400").ShouldBe(10m);

            // Stock leaving after the revaluation takes the new cost.
            await AdjustAsync("REVAL-1", ItemLedgerEntryType.NegativeAdjmt, 1, 0m, "REVAL-ADJ-2");
            (await _itemEntries.GetListAsync(e => e.DocumentNo == "REVAL-ADJ-2")).ShouldHaveSingleItem().UnitCost.ShouldBe(12.5m);
        });
    }

    [Fact]
    public async Task Negative_Stock_Is_Refused_Per_Location_When_The_Setup_Prevents_It()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setup = await _inventorySetup.GetAsync();
            setup.PreventNegativeInventory = true;
            await _inventorySetup.UpdateAsync(setup);

            await _locations.CreateAsync(new CreateUpdateLocationDto { Code = "WEST", Description = "West warehouse" });
            await NewItemAsync("NEG-1", CostingMethod.Fifo);
            await AdjustAsync("NEG-1", ItemLedgerEntryType.PositiveAdjmt, 5, 1m, "NEG-ADJ-1");

            // There is stock at MAIN, but none at WEST.
            var refused = await Should.ThrowAsync<BusinessException>(() => AdjustAsync("NEG-1", ItemLedgerEntryType.NegativeAdjmt, 1, 0m, "NEG-ADJ-2", locationCode: "WEST"));
            refused.Code.ShouldBe(ErpErrorCodes.Inventory.InsufficientInventory);
        });
    }

    [Fact]
    public async Task The_Inventory_Valuation_Agrees_With_The_Inventory_Account()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("VAL-1", CostingMethod.Fifo);
            await AdjustAsync("VAL-1", ItemLedgerEntryType.PositiveAdjmt, 6, 7m, "VAL-ADJ-1");
            await AdjustAsync("VAL-1", ItemLedgerEntryType.NegativeAdjmt, 2, 0m, "VAL-ADJ-2");

            var report = await GetRequiredService<StandardReportCatalog>().Get("InventoryValuation")
                .RunAsync(new StandardReportRequest { ToDate = PostingDate, NoFilter = "VAL-1" });
            var row = report.Rows.First(r => Equals(r.Values["itemNo"], "VAL-1"));
            row.Values["quantity"].ShouldBe(4m);
            row.Values["value"].ShouldBe(28m);

            var account = (await _glEntries.GetListAsync(e => e.GLAccountNo == "1400" && e.SourceNo == "VAL-1")).Sum(e => e.Amount);
            account.ShouldBe(28m);
        });
    }

    private async Task NewItemAsync(string no, CostingMethod costingMethod, string trackingCode = null)
    {
        await _items.CreateAsync(new CreateUpdateItemDto
        {
            No = no,
            Description = no,
            Type = ItemType.Inventory,
            BaseUnitOfMeasureCode = "PCS",
            GenProdPostingGroup = "RETAIL",
            InventoryPostingGroup = "RETAIL",
            VatProdPostingGroup = "STANDARD",
            CostingMethod = costingMethod,
            ItemTrackingCode = trackingCode,
        });
    }

    private async Task<ItemJournalBatchDto> BatchAsync(ItemJournalTemplateType type)
    {
        var batches = await _batches.GetListAsync(new GetItemJournalBatchListInput { TemplateType = type, MaxResultCount = 10 });
        return batches.Items.First();
    }

    /// <summary>Posts one line through the item journal.</summary>
    private async Task AdjustAsync(string itemNo, ItemLedgerEntryType entryType, decimal quantity, decimal unitCost, string documentNo, string lotNo = null, string serialNo = null, string locationCode = "MAIN")
    {
        var batch = await BatchAsync(ItemJournalTemplateType.Item);
        var line = await _journalLines.CreateAsync(new CreateUpdateItemJournalLineDto
        {
            ItemJournalBatchId = batch.Id,
            PostingDate = PostingDate,
            EntryType = entryType,
            DocumentNo = documentNo,
            ItemNo = itemNo,
            LocationCode = locationCode,
            Quantity = quantity,
            UnitCost = unitCost,
            LotNo = lotNo,
            SerialNo = serialNo,
        });

        try
        {
            await _batches.RunPostingAsync(batch.Id);
        }
        catch
        {
            await _journalLines.DeleteAsync(line.Id);
            throw;
        }
    }

    private async Task<decimal> AvailableAsync(string itemNo, string locationCode)
    {
        return (await _itemEntries.GetListAsync(e => e.ItemNo == itemNo && e.LocationCode == locationCode)).Sum(e => e.Quantity);
    }

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);
}
