using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Bins and bin contents, put-aways, picks and movements, reservations, physical inventory orders
/// with counting periods, assembly orders and drop shipments.
/// </summary>
public class WarehouseManagement_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    private readonly IItemAppService _items;
    private readonly IItemJournalBatchAppService _batches;
    private readonly IItemJournalLineAppService _journalLines;
    private readonly ILocationAppService _locations;
    private readonly IBinAppService _bins;
    private readonly IWarehouseActivityAppService _activities;
    private readonly IWarehouseActivityLineAppService _activityLines;
    private readonly IReservationEntryAppService _reservations;
    private readonly IPhysInvtCountingPeriodAppService _countingPeriods;
    private readonly IPhysInvtOrderAppService _physOrders;
    private readonly IPhysInvtOrderLineAppService _physLines;
    private readonly IAssemblyBomComponentAppService _bom;
    private readonly IAssemblyOrderAppService _assemblyOrders;
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly IPurchaseDocumentAppService _purchaseDocuments;
    private readonly ICustomerAppService _customers;
    private readonly IVendorAppService _vendors;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemEntries;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public WarehouseManagement_Tests()
    {
        _items = GetRequiredService<IItemAppService>();
        _batches = GetRequiredService<IItemJournalBatchAppService>();
        _journalLines = GetRequiredService<IItemJournalLineAppService>();
        _locations = GetRequiredService<ILocationAppService>();
        _bins = GetRequiredService<IBinAppService>();
        _activities = GetRequiredService<IWarehouseActivityAppService>();
        _activityLines = GetRequiredService<IWarehouseActivityLineAppService>();
        _reservations = GetRequiredService<IReservationEntryAppService>();
        _countingPeriods = GetRequiredService<IPhysInvtCountingPeriodAppService>();
        _physOrders = GetRequiredService<IPhysInvtOrderAppService>();
        _physLines = GetRequiredService<IPhysInvtOrderLineAppService>();
        _bom = GetRequiredService<IAssemblyBomComponentAppService>();
        _assemblyOrders = GetRequiredService<IAssemblyOrderAppService>();
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _purchaseDocuments = GetRequiredService<IPurchaseDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _itemEntries = GetRequiredService<IRepository<ItemLedgerEntry, Guid>>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task Bins_Follow_The_Stock_And_A_Movement_Moves_It_Between_Bins()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await BinLocationAsync("BINS", requirePutAway: false, requirePick: false, "A1", "A2");
            await NewItemAsync("BIN-1", 2m);
            await AdjustAsync("BIN-1", ItemLedgerEntryType.PositiveAdjmt, 10, 2m, "BIN-ADJ-1", "BINS", "A1");
            (await ContentAsync("BINS", "A1", "BIN-1")).ShouldBe(10m);

            var movement = await _activities.CreateAsync(new CreateUpdateWarehouseActivityDto { LocationCode = "BINS", Description = "Restock" });
            movement.Type.ShouldBe(WarehouseActivityType.Movement);
            await _activityLines.CreateAsync(new CreateUpdateWarehouseActivityLineDto { WarehouseActivityHeaderId = movement.Id, ItemNo = "BIN-1", FromBinCode = "A1", ToBinCode = "A2", Quantity = 4 });
            var registered = await _activities.RegisterAsync(movement.Id, new RegisterWarehouseActivityInput { RegisteringDate = PostingDate });
            registered.Status.ShouldBe(WarehouseActivityStatus.Registered);

            (await ContentAsync("BINS", "A1", "BIN-1")).ShouldBe(6m);
            (await ContentAsync("BINS", "A2", "BIN-1")).ShouldBe(4m);
            (await OnHandAsync("BIN-1", "BINS")).ShouldBe(10m);

            // Without a bin, stock leaves the default bin first and then the others.
            await AdjustAsync("BIN-1", ItemLedgerEntryType.NegativeAdjmt, 7, 0m, "BIN-ADJ-2", "BINS");
            (await ContentAsync("BINS", "A1", "BIN-1")).ShouldBe(0m);
            (await ContentAsync("BINS", "A2", "BIN-1")).ShouldBe(3m);

            // A bin cannot give more than it holds, nor be deleted while it holds stock.
            var tooMuch = await _activities.CreateAsync(new CreateUpdateWarehouseActivityDto { LocationCode = "BINS" });
            await _activityLines.CreateAsync(new CreateUpdateWarehouseActivityLineDto { WarehouseActivityHeaderId = tooMuch.Id, ItemNo = "BIN-1", FromBinCode = "A2", ToBinCode = "A1", Quantity = 5 });
            (await Should.ThrowAsync<BusinessException>(() => _activities.RegisterAsync(tooMuch.Id, new RegisterWarehouseActivityInput()))).Code.ShouldBe(ErpErrorCodes.Inventory.InsufficientBinContent);
            var a2 = (await _bins.GetListAsync(new GetBinListInput { LocationCode = "BINS", MaxResultCount = 10 })).Items.Single(b => b.Code == "A2");
            (await Should.ThrowAsync<BusinessException>(() => _bins.DeleteAsync(a2.Id))).Code.ShouldBe(ErpErrorCodes.Inventory.BinHasContent);
        });
    }

    [Fact]
    public async Task Received_Stock_Is_Put_Away_And_Shipped_Stock_Must_Be_Picked_First()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await BinLocationAsync("DC", requirePutAway: true, requirePick: true, "STORE");
            await NewItemAsync("PICK-1", 10m);

            var vendor = await _vendors.GetByNoAsync("V00010");
            var purchase = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                No = "PO-DC-1",
                VendorId = vendor.Id,
                PostingDate = PostingDate,
                VendorInvoiceNo = "VI-DC-1",
                Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "PICK-1", Description = "Pick item", Quantity = 5, DirectUnitCost = 10m, LocationCode = "DC" }],
            });
            var received = await _purchaseDocuments.RunOrderPostingAsync(purchase.Id, new PurchaseOrderPostingInput { Receive = true, Invoice = false });
            (await ContentAsync("DC", "RECEIVE", "PICK-1")).ShouldBe(5m);

            var putAway = await _activities.MakePutAwayAsync(new MakeWarehouseDocumentInput { SourceNo = received.ReceiptNo });
            putAway.Type.ShouldBe(WarehouseActivityType.PutAway);
            var putAwayLine = (await _activityLines.GetListAsync(new GetWarehouseActivityLineListInput { WarehouseActivityHeaderId = putAway.Id, MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
            putAwayLine.FromBinCode.ShouldBe("RECEIVE");
            putAwayLine.ToBinCode.ShouldBe("STORE");
            await _activities.RegisterAsync(putAway.Id, new RegisterWarehouseActivityInput { RegisteringDate = PostingDate });
            (await ContentAsync("DC", "STORE", "PICK-1")).ShouldBe(5m);

            // A second put-away of the same receipt has nothing left to do.
            (await Should.ThrowAsync<BusinessException>(() => _activities.MakePutAwayAsync(new MakeWarehouseDocumentInput { SourceNo = received.ReceiptNo }))).Code.ShouldBe(ErpErrorCodes.Inventory.NothingToPutAway);

            var customer = await _customers.GetByNoAsync("C00010");
            var order = await _salesDocuments.CreateAsync(new CreateUpdateSalesHeaderDto
            {
                DocumentType = SalesDocumentType.Order,
                No = "SO-DC-1",
                CustomerId = customer.Id,
                PostingDate = PostingDate,
                Lines = [new SalesLineInputDto { Type = DocumentLineType.Item, No = "PICK-1", Description = "Pick item", Quantity = 3, UnitPrice = 25m, LocationCode = "DC" }],
            });

            var unpicked = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = false }));
            unpicked.Code.ShouldBe(ErpErrorCodes.Inventory.PickRequired);

            var pick = await _activities.MakePickAsync(new MakeWarehouseDocumentInput { SourceNo = "SO-DC-1" });
            var pickLine = (await _activityLines.GetListAsync(new GetWarehouseActivityLineListInput { WarehouseActivityHeaderId = pick.Id, MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
            pickLine.FromBinCode.ShouldBe("STORE");
            pickLine.ToBinCode.ShouldBe("SHIP");
            pickLine.Quantity.ShouldBe(3m);
            await _activities.RegisterAsync(pick.Id, new RegisterWarehouseActivityInput { RegisteringDate = PostingDate });

            await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = false });
            (await ContentAsync("DC", "SHIP", "PICK-1")).ShouldBe(0m);
            (await ContentAsync("DC", "STORE", "PICK-1")).ShouldBe(2m);
        });
    }

    [Fact]
    public async Task Reserved_Stock_Cannot_Be_Used_By_Anything_Else()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("RES-1", 4m);
            await AdjustAsync("RES-1", ItemLedgerEntryType.PositiveAdjmt, 10, 4m, "RES-ADJ-1", "MAIN");

            var customer = await _customers.GetByNoAsync("C00010");
            var order = await _salesDocuments.CreateAsync(new CreateUpdateSalesHeaderDto
            {
                DocumentType = SalesDocumentType.Order,
                No = "SO-RES-1",
                CustomerId = customer.Id,
                PostingDate = PostingDate,
                Lines = [new SalesLineInputDto { Type = DocumentLineType.Item, No = "RES-1", Description = "Reserved item", Quantity = 8, UnitPrice = 9m, LocationCode = "MAIN" }],
            });

            (await _reservations.ReserveSalesOrderAsync(new ReserveDocumentInput { DocumentNo = "SO-RES-1" })).LineCount.ShouldBe(1);
            var availability = await _reservations.GetAvailabilityAsync(new GetItemAvailabilityInput { ItemNo = "RES-1", LocationCode = "MAIN" });
            availability.Reserved.ShouldBe(8m);
            availability.Available.ShouldBe(2m);

            var blocked = await Should.ThrowAsync<BusinessException>(() => AdjustAsync("RES-1", ItemLedgerEntryType.NegativeAdjmt, 5, 0m, "RES-ADJ-2", "MAIN"));
            blocked.Code.ShouldBe(ErpErrorCodes.Inventory.ReservedForOtherDocument);
            await AdjustAsync("RES-1", ItemLedgerEntryType.NegativeAdjmt, 2, 0m, "RES-ADJ-3", "MAIN");

            // The order uses its own reservation as it ships.
            await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = false });
            (await _reservations.GetListAsync(new GetReservationEntryListInput { SourceDocumentNo = "SO-RES-1", MaxResultCount = 10 })).TotalCount.ShouldBe(0);
            (await OnHandAsync("RES-1", "MAIN")).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Physical_Inventory_Order_Posts_The_Count_And_Schedules_The_Next_One()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _countingPeriods.CreateAsync(new CreateUpdatePhysInvtCountingPeriodDto { Code = "MONTHLY", Description = "Every month", CountFrequencyPerYear = 12 });
            await NewItemAsync("CNT-1", 3m, countingPeriod: "MONTHLY", nextCountingDate: PostingDate);
            await NewItemAsync("CNT-2", 3m, countingPeriod: "MONTHLY", nextCountingDate: PostingDate.AddDays(10));
            await AdjustAsync("CNT-1", ItemLedgerEntryType.PositiveAdjmt, 10, 3m, "CNT-ADJ-1", "MAIN");
            await AdjustAsync("CNT-2", ItemLedgerEntryType.PositiveAdjmt, 4, 3m, "CNT-ADJ-2", "MAIN");

            var order = await _physOrders.CreateAsync(new CreateUpdatePhysInvtOrderDto { LocationCode = "MAIN", OrderDate = PostingDate, PostingDate = PostingDate });
            order.No.ShouldStartWith("PIO");

            // Cycle counting: only the item that is due.
            await _physOrders.CalculateLinesAsync(order.Id, new CalculatePhysInvtLinesInput { OnlyItemsDueForCounting = true });
            var line = (await _physLines.GetListAsync(new GetPhysInvtOrderLineListInput { PhysInvtOrderHeaderId = order.Id, MaxResultCount = 100 })).Items.ShouldHaveSingleItem();
            line.ItemNo.ShouldBe("CNT-1");
            line.QtyExpected.ShouldBe(10m);

            (await Should.ThrowAsync<BusinessException>(() => _physOrders.FinishAsync(order.Id))).Code.ShouldBe(ErpErrorCodes.Inventory.PhysInvtLineNotRecorded);

            await _physLines.UpdateAsync(line.Id, new CreateUpdatePhysInvtOrderLineDto { PhysInvtOrderHeaderId = order.Id, QtyRecorded = 7, Recorded = true });
            var finished = await _physOrders.FinishAsync(order.Id);
            finished.Status.ShouldBe(PhysInvtOrderStatus.Finished);

            (await OnHandAsync("CNT-1", "MAIN")).ShouldBe(7m);
            (await _glEntries.GetListAsync(e => e.DocumentNo == order.No && e.GLAccountNo == "1400")).Sum(e => e.Amount).ShouldBe(-9m);
            var counted = await _items.GetByNoAsync("CNT-1");
            counted.LastCountingDate.ShouldBe(PostingDate);
            counted.NextCountingDate.ShouldBe(PostingDate.AddDays(30));
        });
    }

    [Fact]
    public async Task An_Assembly_Order_Turns_Components_Into_The_Item_At_Their_Cost()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("KIT-C1", 2m);
            await NewItemAsync("KIT-C2", 3m);
            await NewItemAsync("KIT-1", 0m);
            await AdjustAsync("KIT-C1", ItemLedgerEntryType.PositiveAdjmt, 20, 2m, "KIT-ADJ-1", "MAIN");
            await AdjustAsync("KIT-C2", ItemLedgerEntryType.PositiveAdjmt, 20, 3m, "KIT-ADJ-2", "MAIN");
            await _bom.CreateAsync(new CreateUpdateAssemblyBomComponentDto { ParentItemNo = "KIT-1", ComponentItemNo = "KIT-C1", QuantityPer = 2 });
            await _bom.CreateAsync(new CreateUpdateAssemblyBomComponentDto { ParentItemNo = "KIT-1", ComponentItemNo = "KIT-C2", QuantityPer = 1 });

            var recursive = await Should.ThrowAsync<BusinessException>(() => _bom.CreateAsync(new CreateUpdateAssemblyBomComponentDto { ParentItemNo = "KIT-1", ComponentItemNo = "KIT-1", QuantityPer = 1 }));
            recursive.Code.ShouldBe(ErpErrorCodes.Inventory.AssemblyBomRecursive);

            var order = await _assemblyOrders.CreateAsync(new CreateUpdateAssemblyOrderDto { ItemNo = "KIT-1", Quantity = 4, QuantityToAssemble = 1, LocationCode = "MAIN", DueDate = PostingDate, PostingDate = PostingDate });
            var lines = GetRequiredService<IAssemblyLineAppService>();
            var componentLines = (await lines.GetListAsync(new GetAssemblyLineListInput { AssemblyHeaderId = order.Id, MaxResultCount = 10 })).Items;
            componentLines.Single(l => l.ItemNo == "KIT-C1").Quantity.ShouldBe(8m);
            componentLines.Single(l => l.ItemNo == "KIT-C2").Quantity.ShouldBe(4m);

            // Part of the order first, then the rest.
            var partly = await _assemblyOrders.RunPostingAsync(order.Id);
            partly.AssembledQuantity.ShouldBe(1m);
            partly.Status.ShouldBe(AssemblyOrderStatus.Released);
            var done = await _assemblyOrders.RunPostingAsync(order.Id);
            done.Status.ShouldBe(AssemblyOrderStatus.Finished);
            done.UnitCost.ShouldBe(7m); // 2 x 2 + 1 x 3

            (await OnHandAsync("KIT-1", "MAIN")).ShouldBe(4m);
            (await OnHandAsync("KIT-C1", "MAIN")).ShouldBe(12m);
            (await OnHandAsync("KIT-C2", "MAIN")).ShouldBe(16m);
            var output = await _itemEntries.GetListAsync(e => e.ItemNo == "KIT-1" && e.EntryType == ItemLedgerEntryType.AssemblyOutput);
            output.Sum(e => e.Quantity * e.UnitCost).ShouldBe(28m);
            (await _glEntries.GetListAsync(e => e.DocumentNo == order.No)).Sum(e => e.Amount).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Drop_Shipment_Ships_When_Its_Purchase_Order_Is_Received()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("DROP-1", 50m);
            var customer = await _customers.GetByNoAsync("C00010");
            var order = await _salesDocuments.CreateAsync(new CreateUpdateSalesHeaderDto
            {
                DocumentType = SalesDocumentType.Order,
                No = "SO-DROP-1",
                CustomerId = customer.Id,
                PostingDate = PostingDate,
                Lines = [new SalesLineInputDto { Type = DocumentLineType.Item, No = "DROP-1", Description = "Drop item", Quantity = 2, UnitPrice = 80m, LocationCode = "MAIN", DropShipment = true }],
            });

            // The line does not ship from stock.
            (await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = false })))
                .Code.ShouldBe(ErpErrorCodes.Documents.NothingToPost);

            var made = await _salesDocuments.MakeDropShipmentOrderAsync(order.Id, new MakeDropShipmentOrderInput { VendorNo = "V00010", OrderDate = PostingDate });
            made.LineCount.ShouldBe(1);
            var purchase = await _purchaseDocuments.GetAsync(made.PurchaseOrderId);
            purchase.Lines.Single().DropShipment.ShouldBeTrue();
            purchase.Lines.Single().SalesOrderNo.ShouldBe("SO-DROP-1");

            await _purchaseDocuments.RunOrderPostingAsync(purchase.Id, new PurchaseOrderPostingInput { Receive = true, Invoice = false });

            var shipped = await _salesDocuments.GetAsync(order.Id);
            shipped.Lines.Single().QuantityShipped.ShouldBe(2m);
            shipped.Lines.Single().PurchaseOrderNo.ShouldBe(purchase.No);
            var entries = await _itemEntries.GetListAsync(e => e.ItemNo == "DROP-1");
            entries.Count.ShouldBe(2);
            entries.Sum(e => e.Quantity).ShouldBe(0m);
            entries.Single(e => !e.Positive).UnitCost.ShouldBe(entries.Single(e => e.Positive).UnitCost);

            // The shipped line can now be invoiced.
            var invoiced = await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = false, Invoice = true });
            invoiced.InvoiceNo.ShouldNotBeNull();
        });
    }

    private async Task BinLocationAsync(string code, bool requirePutAway, bool requirePick, params string[] storageBins)
    {
        var location = await _locations.CreateAsync(new CreateUpdateLocationDto { Code = code, Description = code, BinMandatory = true });
        var bins = storageBins.ToList();
        if (requirePutAway)
        {
            bins.Add("RECEIVE");
        }

        if (requirePick)
        {
            bins.Add("SHIP");
        }

        foreach (var bin in bins)
        {
            await _bins.CreateAsync(new CreateUpdateBinDto { LocationCode = code, Code = bin, Description = bin });
        }

        await _locations.UpdateAsync(location.Id, new CreateUpdateLocationDto
        {
            Code = code,
            Description = code,
            BinMandatory = true,
            RequirePutAway = requirePutAway,
            RequirePick = requirePick,
            ReceiptBinCode = requirePutAway ? "RECEIVE" : null,
            ShipmentBinCode = requirePick ? "SHIP" : null,
            DefaultBinCode = storageBins[0],
        });
    }

    private async Task NewItemAsync(string no, decimal unitCost, string countingPeriod = null, DateTime? nextCountingDate = null)
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
            CostingMethod = CostingMethod.Fifo,
            UnitCost = unitCost,
            PhysInvtCountingPeriodCode = countingPeriod,
            NextCountingDate = nextCountingDate,
        });
    }

    private async Task AdjustAsync(string itemNo, ItemLedgerEntryType entryType, decimal quantity, decimal unitCost, string documentNo, string locationCode, string binCode = null)
    {
        var batch = (await _batches.GetListAsync(new GetItemJournalBatchListInput { TemplateType = ItemJournalTemplateType.Item, MaxResultCount = 10 })).Items.First();
        var line = await _journalLines.CreateAsync(new CreateUpdateItemJournalLineDto
        {
            ItemJournalBatchId = batch.Id,
            PostingDate = PostingDate,
            EntryType = entryType,
            DocumentNo = documentNo,
            ItemNo = itemNo,
            LocationCode = locationCode,
            BinCode = binCode,
            Quantity = quantity,
            UnitCost = unitCost,
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

    private async Task<decimal> ContentAsync(string locationCode, string binCode, string itemNo) =>
        (await _bins.GetContentsAsync(new GetBinContentListInput { LocationCode = locationCode, BinCode = binCode, ItemNo = itemNo, MaxResultCount = 100 })).Items.Sum(c => c.Quantity);

    private async Task<decimal> OnHandAsync(string itemNo, string locationCode) =>
        (await _itemEntries.GetListAsync(e => e.ItemNo == itemNo && e.LocationCode == locationCode)).Sum(e => e.Quantity);
}
