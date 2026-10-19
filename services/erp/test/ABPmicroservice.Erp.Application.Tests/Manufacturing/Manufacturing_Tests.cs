using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Manufacturing;

/// <summary>
/// BOMs and routings exploded onto production orders, consumption, capacity and output through
/// WIP, variance on finishing, and the planning worksheet. The seed has WIP account 1480, capacity
/// applied 5400, variance 5410, work center ASSEMBLY at 60 an hour and location MAIN.
/// </summary>
public class Manufacturing_Tests : ErpApplicationTestBase
{
    private readonly IItemAppService _items;
    private readonly IProductionBomAppService _boms;
    private readonly IProductionBomLineAppService _bomLines;
    private readonly IRoutingAppService _routings;
    private readonly IRoutingLineAppService _routingLines;
    private readonly IProductionOrderAppService _orders;
    private readonly IPlanningAppService _planning;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public Manufacturing_Tests()
    {
        _items = GetRequiredService<IItemAppService>();
        _boms = GetRequiredService<IProductionBomAppService>();
        _bomLines = GetRequiredService<IProductionBomLineAppService>();
        _routings = GetRequiredService<IRoutingAppService>();
        _routingLines = GetRequiredService<IRoutingLineAppService>();
        _orders = GetRequiredService<IProductionOrderAppService>();
        _planning = GetRequiredService<IPlanningAppService>();
        _itemRepository = GetRequiredService<IRepository<Item, Guid>>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task Refresh_Explodes_Nested_Boms_With_Scrap_Into_Components()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpTableAsync("T1", standardCost: 0m);
            var order = await _orders.CreateAsync(new CreateUpdateProductionOrderDto { SourceItemNo = "T1-TABLE", Quantity = 10m, LocationCode = "MAIN", DueDate = new DateTime(2026, 3, 20) });

            order.Components.Single(c => c.ItemNo == "T1-TOP").ExpectedQuantity.ShouldBe(10m);
            order.Components.Single(c => c.ItemNo == "T1-LEG").ExpectedQuantity.ShouldBe(42m); // 4 per table, 5% scrap
            order.Components.Single(c => c.ItemNo == "T1-SCREW").ExpectedQuantity.ShouldBe(160m); // 4 legs x 4 screws, from the leg kit
            order.RoutingLines.Single().ExpectedMinutes.ShouldBe(60m + 10m * 30m);
        });
    }

    [Fact]
    public async Task Consumption_And_Capacity_Roll_Into_The_Output_And_Wip_Ends_At_Zero()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpTableAsync("T2", standardCost: 0m);
            var order = await ReleasedOrderAsync("T2", 2m);

            order = await _orders.ConsumeAsync(order.Id, new PostConsumptionInput { PostingDate = new DateTime(2026, 3, 10) });
            // 2 tops at 40, 8.4 legs at 10, 32 screws at 0.5
            order.MaterialCost.ShouldBe(80m + 84m + 16m);

            order = await _orders.ReportOutputAsync(order.Id, new PostOutputInput { PostingDate = new DateTime(2026, 3, 10), Quantity = 2m });
            order.CapacityCost.ShouldBe(120m); // 60 + 2 x 30 minutes at 60 an hour
            order.OutputCost.ShouldBe(300m);
            order.Wip.ShouldBe(0m);

            (await _itemRepository.FirstAsync(i => i.No == "T2-TABLE")).Inventory.ShouldBe(2m);
            order = await _orders.FinishAsync(order.Id, new FinishProductionOrderInput { PostingDate = new DateTime(2026, 3, 11) });
            order.Status.ShouldBe(ProductionOrderStatus.Finished);
            order.VarianceCost.ShouldBe(0m);

            var all = await _glEntries.GetListAsync(e => e.DocumentNo == order.No);
            Sum(all, "1480").ShouldBe(0m);
            Sum(all, "5400").ShouldBe(-120m);
            all.Sum(e => e.Amount).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Standard_Cost_Item_Posts_Output_At_Standard_And_The_Rest_As_Variance()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpTableAsync("T3", standardCost: 140m);
            var order = await ReleasedOrderAsync("T3", 2m);
            await _orders.ConsumeAsync(order.Id, new PostConsumptionInput { PostingDate = new DateTime(2026, 3, 10) });
            order = await _orders.ReportOutputAsync(order.Id, new PostOutputInput { PostingDate = new DateTime(2026, 3, 10), Quantity = 2m });
            order.OutputCost.ShouldBe(280m);
            order.Wip.ShouldBe(20m); // 180 material + 120 capacity - 280 at standard

            var wrong = await Should.ThrowAsync<BusinessException>(() => _orders.ChangeStatusAsync(order.Id, new ChangeProductionOrderStatusInput { Status = ProductionOrderStatus.Planned }));
            wrong.Code.ShouldBe(ErpErrorCodes.Manufacturing.WrongOrderStatus);

            order = await _orders.FinishAsync(order.Id, new FinishProductionOrderInput { PostingDate = new DateTime(2026, 3, 11) });
            order.VarianceCost.ShouldBe(20m);
            var all = await _glEntries.GetListAsync(e => e.DocumentNo == order.No);
            Sum(all, "5410").ShouldBe(20m);
            Sum(all, "1480").ShouldBe(0m);
        });
    }

    [Fact]
    public async Task Planning_Orders_Up_To_The_Reorder_Point_And_Plans_Components_Of_What_It_Makes()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpTableAsync("T4", standardCost: 0m, stockComponents: false);
            await PlanAsync("T4-TABLE", ReplenishmentSystem.ProdOrder, ReorderPolicy.FixedReorderQty, reorderPoint: 5m, reorderQuantity: 10m, vendorNo: null, bomNo: "T4-TABLE", routingNo: null);
            await PlanAsync("T4-TOP", ReplenishmentSystem.Purchase, ReorderPolicy.LotForLot, 0m, 0m, "V00010", null, null);

            (await _planning.CalculateAsync(new CalculatePlanInput { ToDate = new DateTime(2026, 4, 30) })).ShouldBeGreaterThanOrEqualTo(2);
            var lines = (await _planning.GetListAsync(new ErpPagedListInput { MaxResultCount = 100 })).Items;
            var table = lines.Single(l => l.ItemNo == "T4-TABLE");
            table.Quantity.ShouldBe(10m); // nothing on hand, one fixed lot gets above 5
            table.ReplenishmentSystem.ShouldBe(ReplenishmentSystem.ProdOrder);
            var top = lines.Single(l => l.ItemNo == "T4-TOP");
            top.Quantity.ShouldBe(10m); // one top per table planned
            top.LowLevelCode.ShouldBeGreaterThan(table.LowLevelCode);

            var result = await _planning.CarryOutAsync();
            result.ProductionOrderNos.Count.ShouldBe(1);
            result.PurchaseOrderNos.Count.ShouldBe(1);
            (await _planning.GetListAsync(new ErpPagedListInput { MaxResultCount = 100 })).Items.Where(l => l.ItemNo.StartsWith("T4")).ShouldAllBe(l => l.CarriedOutNo != null);
        });
    }

    // --- helpers --------------------------------------------------------------------------------

    /// <summary>
    /// A table made of a top, 4 legs (5% scrap) and a leg kit BOM of 4 screws per leg, assembled in
    /// 60 minutes setup and 30 a table. Tops cost 40, legs 10, screws 0.50; enough of each in stock.
    /// </summary>
    private async Task SetUpTableAsync(string prefix, decimal standardCost, bool stockComponents = true)
    {
        foreach (var (no, cost) in new[] { ("TOP", 40m), ("LEG", 10m), ("SCREW", 0.5m), ("TABLE", 0m) })
        {
            await NewItemAsync($"{prefix}-{no}");
            if (stockComponents && cost > 0m)
            {
                var item = await _itemRepository.FirstAsync(i => i.No == $"{prefix}-{no}");
                await GetRequiredService<ItemJnlPostLine>().PostAsync(new ItemPostingRequest
                {
                    ItemId = item.Id, ItemNo = item.No, PostingDate = new DateTime(2026, 3, 1), EntryType = ItemLedgerEntryType.PositiveAdjmt, DocumentNo = $"{prefix}-STOCK", Quantity = 500m, UnitCost = cost, LocationCode = "MAIN",
                });
            }
        }

        var kit = await _boms.CreateAsync(new CreateUpdateProductionBomDto { No = $"{prefix}-KIT", Description = "Leg kit" });
        await _bomLines.CreateAsync(new CreateUpdateProductionBomLineDto { ProductionBomId = kit.Id, Type = ProductionBomLineType.Item, No = $"{prefix}-SCREW", QuantityPer = 4m });
        await _boms.UpdateAsync(kit.Id, new CreateUpdateProductionBomDto { No = kit.No, Description = kit.Description, Status = ProductionBomStatus.Certified });

        var bom = await _boms.CreateAsync(new CreateUpdateProductionBomDto { No = $"{prefix}-TABLE", Description = "Table" });
        await _bomLines.CreateAsync(new CreateUpdateProductionBomLineDto { ProductionBomId = bom.Id, Type = ProductionBomLineType.Item, No = $"{prefix}-TOP", QuantityPer = 1m });
        await _bomLines.CreateAsync(new CreateUpdateProductionBomLineDto { ProductionBomId = bom.Id, Type = ProductionBomLineType.Item, No = $"{prefix}-LEG", QuantityPer = 4m, ScrapPct = 5m });
        await _bomLines.CreateAsync(new CreateUpdateProductionBomLineDto { ProductionBomId = bom.Id, Type = ProductionBomLineType.ProductionBom, No = $"{prefix}-KIT", QuantityPer = 4m });
        await _boms.UpdateAsync(bom.Id, new CreateUpdateProductionBomDto { No = bom.No, Description = bom.Description, Status = ProductionBomStatus.Certified });

        var routing = await _routings.CreateAsync(new CreateUpdateRoutingDto { No = $"{prefix}-ROUTE", Description = "Assembly", Status = ProductionBomStatus.Certified });
        await _routingLines.CreateAsync(new CreateUpdateRoutingLineDto { RoutingId = routing.Id, OperationNo = "10", Type = RoutingLineType.WorkCenter, No = "ASSEMBLY", SetupTime = 60m, RunTime = 30m });

        await PlanAsync($"{prefix}-TABLE", ReplenishmentSystem.ProdOrder, ReorderPolicy.None, 0m, 0m, null, $"{prefix}-TABLE", $"{prefix}-ROUTE", standardCost);
    }

    private async Task<ProductionOrderDto> ReleasedOrderAsync(string prefix, decimal quantity)
    {
        var order = await _orders.CreateAsync(new CreateUpdateProductionOrderDto { SourceItemNo = $"{prefix}-TABLE", Quantity = quantity, LocationCode = "MAIN", DueDate = new DateTime(2026, 3, 20) });
        return await _orders.ChangeStatusAsync(order.Id, new ChangeProductionOrderStatusInput { Status = ProductionOrderStatus.Released });
    }

    private async Task PlanAsync(string no, ReplenishmentSystem replenishment, ReorderPolicy policy, decimal reorderPoint, decimal reorderQuantity, string vendorNo, string bomNo, string routingNo, decimal standardCost = 0m)
    {
        var item = await _itemRepository.FirstAsync(i => i.No == no);
        item.SetPlanning(replenishment, bomNo ?? item.ProductionBomNo, routingNo ?? item.RoutingNo, policy, reorderPoint, reorderQuantity, 0m, 0m, null, standardCost, vendorNo);
        await _itemRepository.UpdateAsync(item, autoSave: true);
    }

    private async Task NewItemAsync(string no) =>
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
        });

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);
}
