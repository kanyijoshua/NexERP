using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Manufacturing;

/// <summary>A component a production order consumes, and how much.</summary>
public sealed record ConsumptionRequest(Guid ComponentId, decimal Quantity);

/// <summary>Time used on an operation of a production order.</summary>
public sealed record CapacityRequest(string OperationNo, decimal Minutes);

/// <summary>
/// Production orders: making them from an item's BOM and routing, moving them through their
/// statuses, and posting what they consume, the capacity they use and the output they make. Every
/// cost goes through the WIP account; finishing an order clears what is left there to the variance
/// account, which for an item with a standard cost is its production variance.
/// </summary>
public class ProductionManager : DomainService
{
    public const string SourceCode = "PRODORDER";

    private readonly IRepository<ProductionOrder, Guid> _orderRepository;

    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();
    private GenJnlPostLine GenJnlPostLine => Repo<GenJnlPostLine>();

    public ProductionManager(IRepository<ProductionOrder, Guid> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ManufacturingSetup> GetSetupAsync() =>
        await Repo<IRepository<ManufacturingSetup, Guid>>().FirstOrDefaultAsync()
        ?? throw new BusinessException(ErpErrorCodes.Manufacturing.SetupMissing);

    public async Task<Item> GetItemAsync(string itemNo)
    {
        var no = itemNo?.Trim();
        return await Repo<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
    }

    /// <summary>A new production order for an item, refreshed from its BOM and routing.</summary>
    public async Task<ProductionOrder> CreateAsync(string no, ProductionOrderStatus status, string itemNo, decimal quantity, DateTime dueDate, string locationCode)
    {
        if (status == ProductionOrderStatus.Finished)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.WrongOrderStatus).WithData("no", no ?? "").WithData("status", status.ToString()).WithData("expected", ProductionOrderStatus.Released.ToString());
        }

        var item = await GetItemAsync(itemNo);
        var setup = await GetSetupAsync();
        var resolved = await Repo<NoSeriesManager>().ResolveNoAsync(setup.ProductionOrderNos, no, Clock.Now);
        var order = new ProductionOrder(GuidGenerator.Create(), resolved, status, item.No, item.Description, quantity, dueDate, locationCode);
        await RefreshAsync(order, item);
        await _orderRepository.InsertAsync(order, autoSave: true);
        return order;
    }

    /// <summary>Rebuilds the components and operations of an order that has not started yet.</summary>
    public async Task<ProductionOrder> RefreshAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id, includeDetails: true);
        if (order.Status == ProductionOrderStatus.Finished || order.Components.Any(c => c.ActConsumption != 0m) || order.FinishedQuantity != 0m)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.OrderStarted).WithData("no", order.No);
        }

        await RefreshAsync(order, await GetItemAsync(order.SourceItemNo));
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    /// <summary>
    /// The items one unit of a BOM is made of, with their quantity per unit and scrap; a line that is
    /// itself a BOM is exploded into its own components, its quantity multiplied through.
    /// </summary>
    public async Task<List<(string ItemNo, string Description, decimal QuantityPer, decimal ScrapPct)>> ExplodeAsync(string bomNo, decimal factor = 1m, int depth = 0)
    {
        if (depth > 10)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.BomLoop).WithData("no", bomNo);
        }

        var bom = await GetBomAsync(bomNo);
        var result = new List<(string, string, decimal, decimal)>();
        foreach (var line in bom.Lines.OrderBy(l => l.LineNo))
        {
            if (line.Type == ProductionBomLineType.ProductionBom)
            {
                result.AddRange(await ExplodeAsync(line.No, factor * line.QuantityPer * (1m + line.ScrapPct / 100m), depth + 1));
            }
            else
            {
                result.Add((line.No, line.Description, factor * line.QuantityPer, line.ScrapPct));
            }
        }

        return result;
    }

    public async Task<ProductionOrder> ChangeStatusAsync(Guid id, ProductionOrderStatus status)
    {
        var order = await _orderRepository.GetAsync(id, includeDetails: true);
        order.ChangeStatus(status);
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    /// <summary>Consumes components of a released order: they leave stock at their cost and go into WIP.</summary>
    public async Task<ProductionOrder> PostConsumptionAsync(Guid id, IReadOnlyList<ConsumptionRequest> requests, DateTime postingDate)
    {
        var order = await _orderRepository.GetAsync(id, includeDetails: true);
        order.EnsureStatus(ProductionOrderStatus.Released);
        var setup = await GetSetupAsync();
        var (register, context) = await OpenAsync(postingDate, order);

        // Nothing named: everything still expected.
        var lines = requests is { Count: > 0 }
            ? requests.Select(r => (Component: order.Components.FirstOrDefault(c => c.Id == r.ComponentId) ?? throw new BusinessException(ErpErrorCodes.Manufacturing.ComponentNotFound).WithData("no", order.No), r.Quantity)).ToList()
            : order.Components.Where(c => c.RemainingQuantity > 0m).Select(c => (Component: c, Quantity: c.RemainingQuantity)).ToList();

        foreach (var (component, quantity) in lines.Where(l => l.Quantity != 0m))
        {
            var item = await GetItemAsync(component.ItemNo);
            var entries = await Repo<ItemJnlPostLine>().PostAllAsync(new ItemPostingRequest
            {
                ItemId = item.Id,
                ItemNo = item.No,
                PostingDate = postingDate,
                EntryType = ItemLedgerEntryType.Consumption,
                DocumentNo = order.No,
                Description = component.Description ?? item.Description,
                Quantity = quantity,
                UnitCost = item.UnitCost,
                LocationCode = order.LocationCode,
            });
            var cost = Math.Round(entries.Sum(e => Math.Abs(e.Quantity) * e.UnitCost), 2);
            component.Consume(quantity, cost);
            order.AddMaterial(cost);

            if (cost != 0m)
            {
                var inventoryAccount = await Repo<PostingSetupManager>().GetInventoryAccountAsync(item.InventoryPostingGroup, order.LocationCode);
                await GLAsync(Required(setup.WipAccountNo, "WipAccountNo"), cost, postingDate, order, context);
                await GLAsync(inventoryAccount, -cost, postingDate, order, context);
            }
        }

        await Repo<GLRegisterManager>().CloseAsync(register);
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    /// <summary>
    /// Posts output of a released order with the capacity it took: the time goes to the capacity
    /// ledger and into WIP at the operation's cost, and the finished items come out of WIP into
    /// stock, at their standard cost when they have one, otherwise at what the order has cost per
    /// unit so far.
    /// </summary>
    public async Task<ProductionOrder> PostOutputAsync(Guid id, decimal quantity, IReadOnlyList<CapacityRequest> capacity, DateTime postingDate)
    {
        var order = await _orderRepository.GetAsync(id, includeDetails: true);
        order.EnsureStatus(ProductionOrderStatus.Released);
        var setup = await GetSetupAsync();
        var item = await GetItemAsync(order.SourceItemNo);
        var (register, context) = await OpenAsync(postingDate, order);

        // Capacity named per operation, or each operation's run time for the quantity (setup once, on the first output).
        var used = capacity is { Count: > 0 }
            ? capacity.Select(c => (Line: order.RoutingLines.FirstOrDefault(l => l.OperationNo == c.OperationNo) ?? throw new BusinessException(ErpErrorCodes.Manufacturing.OperationNotFound).WithData("no", order.No).WithData("operation", c.OperationNo), c.Minutes)).ToList()
            : order.RoutingLines.Select(l => (Line: l, Minutes: l.RunTime * quantity + (l.ActualMinutes == 0m ? l.SetupTime : 0m))).ToList();

        foreach (var (line, minutes) in used.Where(u => u.Minutes != 0m))
        {
            var cost = Math.Round(minutes / 60m * line.UnitCostPerHour, 2);
            line.Use(minutes, cost);
            order.AddCapacity(cost);
            await Repo<IRepository<CapacityLedgerEntry, Guid>>().InsertAsync(new CapacityLedgerEntry(GuidGenerator.Create())
            {
                EntryNo = await Repo<IEntryNoGenerator>().NextAsync(ErpSequenceNames.CapacityLedgerEntry),
                ProductionOrderNo = order.No,
                OperationNo = line.OperationNo,
                Type = line.Type,
                No = line.No,
                ItemNo = order.SourceItemNo,
                PostingDate = postingDate.Date,
                DocumentNo = order.No,
                OutputQuantity = quantity,
                Minutes = minutes,
                CostAmount = cost,
            });

            if (cost != 0m)
            {
                await GLAsync(Required(setup.WipAccountNo, "WipAccountNo"), cost, postingDate, order, context);
                await GLAsync(Required(setup.CapacityAppliedAccountNo, "CapacityAppliedAccountNo"), -cost, postingDate, order, context);
            }
        }

        if (quantity > 0m)
        {
            var remaining = order.Quantity - order.FinishedQuantity;
            var unitCost = item.StandardCost > 0m
                ? item.StandardCost
                : Math.Round(Math.Max(0m, order.MaterialCost + order.CapacityCost - order.OutputCost) / Math.Max(quantity, remaining), 5);
            var outputCost = Math.Round(unitCost * quantity, 2);

            await Repo<ItemJnlPostLine>().PostAllAsync(new ItemPostingRequest
            {
                ItemId = item.Id,
                ItemNo = item.No,
                PostingDate = postingDate,
                EntryType = ItemLedgerEntryType.Output,
                DocumentNo = order.No,
                Description = item.Description,
                Quantity = quantity,
                UnitCost = unitCost,
                LocationCode = order.LocationCode,
            });
            order.AddOutput(quantity, outputCost);

            if (outputCost != 0m)
            {
                var inventoryAccount = await Repo<PostingSetupManager>().GetInventoryAccountAsync(item.InventoryPostingGroup, order.LocationCode);
                await GLAsync(inventoryAccount, outputCost, postingDate, order, context);
                await GLAsync(Required(setup.WipAccountNo, "WipAccountNo"), -outputCost, postingDate, order, context);
            }
        }

        await Repo<GLRegisterManager>().CloseAsync(register);
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    /// <summary>Finishes a released order: what is left in its WIP goes to the variance account.</summary>
    public async Task<ProductionOrder> FinishAsync(Guid id, DateTime postingDate)
    {
        var order = await _orderRepository.GetAsync(id, includeDetails: true);
        order.EnsureStatus(ProductionOrderStatus.Released);
        var setup = await GetSetupAsync();
        var (register, context) = await OpenAsync(postingDate, order);

        var variance = order.Wip;
        if (variance != 0m)
        {
            await GLAsync(Required(setup.VarianceAccountNo, "VarianceAccountNo"), variance, postingDate, order, context);
            await GLAsync(Required(setup.WipAccountNo, "WipAccountNo"), -variance, postingDate, order, context);
        }

        order.Finish(postingDate, variance);
        await Repo<GLRegisterManager>().CloseAsync(register);
        await _orderRepository.UpdateAsync(order, autoSave: true);
        return order;
    }

    private async Task RefreshAsync(ProductionOrder order, Item item)
    {
        order.Components.Clear();
        order.RoutingLines.Clear();

        if (item.ProductionBomNo != null)
        {
            var lineNo = 0;
            foreach (var (itemNo, description, quantityPer, scrapPct) in await ExplodeAsync(item.ProductionBomNo))
            {
                lineNo += 10000;
                order.Components.Add(new ProdOrderComponent(GuidGenerator.Create(), order.Id, lineNo, itemNo, description, quantityPer, scrapPct, order.Quantity));
            }
        }

        if (item.RoutingNo != null)
        {
            var routing = await Repo<IRepository<Routing, Guid>>().FirstOrDefaultAsync(r => r.No == item.RoutingNo)
                ?? throw new BusinessException(ErpErrorCodes.Manufacturing.RoutingNotFound).WithData("no", item.RoutingNo);
            routing = await Repo<IRepository<Routing, Guid>>().GetAsync(routing.Id, includeDetails: true);
            foreach (var line in routing.Lines.OrderBy(l => l.OperationNo, StringComparer.Ordinal))
            {
                order.RoutingLines.Add(new ProdOrderRoutingLine(GuidGenerator.Create(), order.Id, line, await CostPerHourAsync(line), order.Quantity));
            }
        }
    }

    private async Task<decimal> CostPerHourAsync(RoutingLine line)
    {
        if (line.Type == RoutingLineType.MachineCenter)
        {
            var machine = await Repo<IRepository<MachineCenter, Guid>>().FirstOrDefaultAsync(m => m.Code == line.No)
                ?? throw new BusinessException(ErpErrorCodes.Manufacturing.CenterNotFound).WithData("no", line.No);
            return machine.UnitCostPerHour;
        }

        var center = await Repo<IRepository<WorkCenter, Guid>>().FirstOrDefaultAsync(w => w.Code == line.No)
            ?? throw new BusinessException(ErpErrorCodes.Manufacturing.CenterNotFound).WithData("no", line.No);
        return center.UnitCostPerHour;
    }

    private async Task<ProductionBom> GetBomAsync(string bomNo)
    {
        var no = bomNo?.Trim().ToUpperInvariant();
        var repository = Repo<IRepository<ProductionBom, Guid>>();
        var bom = await repository.FirstOrDefaultAsync(b => b.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Manufacturing.BomNotFound).WithData("no", no ?? "");
        bom = await repository.GetAsync(bom.Id, includeDetails: true);
        if (bom.Status != ProductionBomStatus.Certified)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.BomNotCertified).WithData("no", bom.No);
        }

        return bom;
    }

    private async Task<(GLRegister Register, GLPostingContext Context)> OpenAsync(DateTime postingDate, ProductionOrder order)
    {
        await Repo<GeneralLedgerSetupManager>().CheckPostingDateAsync(postingDate);
        var register = await Repo<GLRegisterManager>().OpenAsync(postingDate, SourceCode, order.No);
        return (register, new GLPostingContext(register, SourceCode));
    }

    private Task<GLEntry> GLAsync(string accountNo, decimal amount, DateTime date, ProductionOrder order, GLPostingContext context) =>
        GenJnlPostLine.PostGLDirectAsync(accountNo, date, GLEntryDocumentType.None, order.No, order.Description, amount, order.SourceItemNo, context: context);

    private static string Required(string accountNo, string what) =>
        accountNo.IsNullOrWhiteSpace()
            ? throw new BusinessException(ErpErrorCodes.Manufacturing.SetupAccountMissing).WithData("account", what)
            : accountNo;
}

/// <summary>
/// The planning worksheet: what to buy or make so that no item falls below its reorder point or
/// safety stock, given what is on hand, on order, in production and needed by open sales orders and
/// production components. Items are planned from the top of the BOMs down, so what is planned to be
/// made adds to the demand for its components.
/// </summary>
public class PlanningManager : DomainService
{
    private readonly IRepository<PlanningLine, Guid> _lineRepository;

    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();

    public PlanningManager(IRepository<PlanningLine, Guid> lineRepository)
    {
        _lineRepository = lineRepository;
    }

    /// <summary>Replaces the worksheet with fresh suggestions up to the date. Returns how many.</summary>
    public async Task<int> CalculateAsync(DateTime toDate)
    {
        await _lineRepository.DeleteAsync(l => l.CarriedOutNo == null);

        var items = (await Repo<IRepository<Item, Guid>>().GetListAsync(i => i.Type == ItemType.Inventory && !i.Blocked)).ToDictionary(i => i.No);
        var boms = await Repo<IRepository<ProductionBom, Guid>>().WithDetailsAsync();
        var bomList = await AsyncExecuter.ToListAsync(boms.Where(b => b.Status == ProductionBomStatus.Certified));
        var levels = LowLevelCodes(items.Values.ToList(), bomList);

        var onHand = (await Repo<IRepository<ItemLedgerEntry, Guid>>().GetListAsync()).GroupBy(e => e.ItemNo).ToDictionary(g => g.Key, g => g.Sum(e => e.Quantity));
        var supply = new Dictionary<string, decimal>();
        var demand = new Dictionary<string, decimal>();
        void Add(Dictionary<string, decimal> map, string no, decimal qty) => map[no] = map.GetValueOrDefault(no) + qty;

        foreach (var order in await Repo<IRepository<Purchasing.PurchaseHeader, Guid>>().GetListAsync(h => h.DocumentType == Purchasing.PurchaseDocumentType.Order, includeDetails: true))
        {
            foreach (var line in order.Lines.Where(l => l.Type == Documents.DocumentLineType.Item))
            {
                Add(supply, line.No, (line.Quantity - line.QuantityReceived) * line.QtyPerUnitOfMeasure);
            }
        }

        foreach (var order in await Repo<IRepository<Sales.SalesHeader, Guid>>().GetListAsync(h => h.DocumentType == Sales.SalesDocumentType.Order, includeDetails: true))
        {
            foreach (var line in order.Lines.Where(l => l.Type == Documents.DocumentLineType.Item))
            {
                Add(demand, line.No, (line.Quantity - line.QuantityShipped) * line.QtyPerUnitOfMeasure);
            }
        }

        foreach (var order in await Repo<IRepository<ProductionOrder, Guid>>().GetListAsync(o => o.Status != ProductionOrderStatus.Finished && o.Status != ProductionOrderStatus.Planned, includeDetails: true))
        {
            Add(supply, order.SourceItemNo, order.Quantity - order.FinishedQuantity);
            foreach (var component in order.Components)
            {
                Add(demand, component.ItemNo, component.RemainingQuantity);
            }
        }

        var count = 0;
        foreach (var item in items.Values.OrderBy(i => levels.GetValueOrDefault(i.No)).ThenBy(i => i.No, StringComparer.Ordinal))
        {
            var projected = onHand.GetValueOrDefault(item.No) + supply.GetValueOrDefault(item.No) - demand.GetValueOrDefault(item.No);
            var quantity = Suggest(item, projected);
            if (quantity <= 0m)
            {
                continue;
            }

            var dueDate = toDate.Date;
            await _lineRepository.InsertAsync(new PlanningLine(GuidGenerator.Create(), item.No, item.Description, item.ReplenishmentSystem, item.VendorNo, null, quantity, dueDate, levels.GetValueOrDefault(item.No)));
            count++;

            // What is to be made needs its components, which are planned after it.
            if (item.ReplenishmentSystem == ReplenishmentSystem.ProdOrder && item.ProductionBomNo != null)
            {
                foreach (var (componentNo, _, quantityPer, scrapPct) in await Repo<ProductionManager>().ExplodeAsync(item.ProductionBomNo))
                {
                    Add(demand, componentNo, quantityPer * (1m + scrapPct / 100m) * quantity);
                }
            }
        }

        return count;
    }

    /// <summary>How much to order of an item projected to have <paramref name="projected"/> on hand.</summary>
    public static decimal Suggest(Item item, decimal projected)
    {
        var floor = Math.Max(item.ReorderPoint, item.SafetyStock);
        switch (item.ReorderPolicy)
        {
            case ReorderPolicy.FixedReorderQty:
                if (projected > item.ReorderPoint || item.ReorderQuantity <= 0m)
                {
                    return projected < item.SafetyStock ? item.SafetyStock - projected : 0m;
                }

                // As many fixed lots as it takes to get above the reorder point.
                var lots = Math.Floor((item.ReorderPoint - projected) / item.ReorderQuantity) + 1m;
                return lots * item.ReorderQuantity;

            case ReorderPolicy.MaximumQty:
                return projected <= item.ReorderPoint ? Math.Max(0m, Math.Max(item.MaximumInventory, floor) - projected) : 0m;

            case ReorderPolicy.LotForLot:
                return projected < item.SafetyStock ? item.SafetyStock - projected : projected < 0m ? -projected : 0m;

            default:
                return 0m;
        }
    }

    /// <summary>The depth of each item in the BOMs: 0 for what is not a component, one more than its deepest parent otherwise.</summary>
    public static Dictionary<string, int> LowLevelCodes(List<Item> items, List<ProductionBom> boms)
    {
        var bomByNo = boms.ToDictionary(b => b.No);
        var levels = items.ToDictionary(i => i.No, _ => 0);

        void Walk(string bomNo, int level, int depth)
        {
            if (depth > 10 || !bomByNo.TryGetValue(bomNo, out var bom))
            {
                return;
            }

            foreach (var line in bom.Lines)
            {
                if (line.Type == ProductionBomLineType.ProductionBom)
                {
                    Walk(line.No, level, depth + 1);
                }
                else
                {
                    levels[line.No] = Math.Max(levels.GetValueOrDefault(line.No), level);
                    var component = items.FirstOrDefault(i => i.No == line.No);
                    if (component?.ProductionBomNo != null)
                    {
                        Walk(component.ProductionBomNo, level + 1, depth + 1);
                    }
                }
            }
        }

        foreach (var item in items.Where(i => i.ProductionBomNo != null))
        {
            Walk(item.ProductionBomNo, levels.GetValueOrDefault(item.No) + 1, 0);
        }

        return levels;
    }

    /// <summary>Marks a line as carried out by the order made of it.</summary>
    public async Task MarkCarriedOutAsync(PlanningLine line, string orderNo)
    {
        line.CarriedOut(orderNo);
        await _lineRepository.UpdateAsync(line, autoSave: true);
    }
}
