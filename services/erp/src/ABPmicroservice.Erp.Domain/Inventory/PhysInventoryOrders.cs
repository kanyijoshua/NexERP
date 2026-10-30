using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>Phys. Invt. Counting Period: how many times a year the items given it are counted (cycle counting).</summary>
public class PhysInvtCountingPeriod : CodeTableEntity
{
    public int CountFrequencyPerYear { get; private set; } = 1;

    protected PhysInvtCountingPeriod() { }

    public PhysInvtCountingPeriod(Guid id, string code, string description, int countFrequencyPerYear)
        : base(id, code, description)
    {
        SetFrequency(countFrequencyPerYear);
    }

    public void SetFrequency(int countFrequencyPerYear) => CountFrequencyPerYear = Math.Clamp(countFrequencyPerYear, 1, 365);
}

/// <summary>
/// Physical Inventory Order: a count of one location. Calculating it lists what the books expect
/// per item, variant, lot and (at a location with bins) bin; the counters record what they find;
/// finishing it posts every difference as an adjustment and marks the items counted.
/// </summary>
public class PhysInvtOrderHeader : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public string LocationCode { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public PhysInvtOrderStatus Status { get; private set; }

    public Collection<PhysInvtOrderLine> Lines { get; private set; } = new();

    protected PhysInvtOrderHeader() { }

    public PhysInvtOrderHeader(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Status = PhysInvtOrderStatus.Open;
    }

    public void Set(string description, string locationCode, DateTime orderDate, DateTime postingDate)
    {
        EnsureOpen();
        var location = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        if (Lines.Count > 0 && location != LocationCode)
        {
            // The lines were calculated for the location; another location needs a new order.
            throw new BusinessException(ErpErrorCodes.Inventory.PhysInvtLocationLocked).WithData("no", No);
        }

        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        LocationCode = location;
        OrderDate = orderDate.Date;
        PostingDate = postingDate.Date;
    }

    public PhysInvtOrderLine AddLine(Guid id, Item item, string variantCode, string binCode, string lotNo, string serialNo, decimal qtyExpected)
    {
        EnsureOpen();
        var lineNo = (Lines.Count == 0 ? 0 : Lines.Max(l => l.LineNo)) + 10000;
        var line = new PhysInvtOrderLine(id, Id, lineNo, item, variantCode, LocationCode, binCode, lotNo, serialNo, qtyExpected);
        Lines.Add(line);
        return line;
    }

    public void EnsureOpen()
    {
        if (Status == PhysInvtOrderStatus.Finished)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.PhysInvtOrderFinished).WithData("no", No);
        }
    }

    internal void Finish() => Status = PhysInvtOrderStatus.Finished;
}

public class PhysInvtOrderLine : Entity<Guid>
{
    public Guid PhysInvtOrderHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public string VariantCode { get; private set; }
    public string LocationCode { get; private set; }
    public string BinCode { get; private set; }
    public string LotNo { get; private set; }
    public string SerialNo { get; private set; }

    /// <summary>What the books said when the line was calculated, in the base unit of measure.</summary>
    public decimal QtyExpected { get; private set; }

    public decimal QtyRecorded { get; private set; }

    /// <summary>The line was counted; an order is finished only once every line is.</summary>
    public bool Recorded { get; private set; }

    public decimal QtyDifference => QtyRecorded - QtyExpected;

    protected PhysInvtOrderLine() { }

    public PhysInvtOrderLine(Guid id, Guid headerId, int lineNo, Item item, string variantCode, string locationCode, string binCode, string lotNo, string serialNo, decimal qtyExpected)
        : base(id)
    {
        PhysInvtOrderHeaderId = headerId;
        LineNo = lineNo;
        ItemId = item.Id;
        ItemNo = item.No;
        Description = item.Description;
        VariantCode = CodeTableEntity.NormalizeCode(variantCode);
        LocationCode = locationCode;
        BinCode = Bin.NormalizeCode(binCode);
        LotNo = lotNo.IsNullOrWhiteSpace() ? null : lotNo.Trim();
        SerialNo = serialNo.IsNullOrWhiteSpace() ? null : serialNo.Trim();
        QtyExpected = qtyExpected;
    }

    public void Record(decimal qtyRecorded, bool recorded)
    {
        if (qtyRecorded < 0m)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.InsufficientInventory)
                .WithData("itemNo", ItemNo)
                .WithData("inventory", 0)
                .WithData("quantity", qtyRecorded);
        }

        QtyRecorded = qtyRecorded;
        Recorded = recorded;
    }

    public bool SameStock(string variantCode, string binCode, string lotNo, string serialNo) =>
        VariantCode == variantCode && BinCode == binCode && LotNo == lotNo && SerialNo == serialNo;
}

/// <summary>Calculates and posts physical inventory orders.</summary>
public class PhysInvtOrderManager : DomainService
{
    private const string SourceCode = "PHYSINVT";

    private readonly IRepository<PhysInvtOrderHeader, Guid> _orderRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _entryRepository;
    private readonly IRepository<PhysInvtCountingPeriod, Guid> _periodRepository;

    private ItemJnlPostLine PostLine => LazyServiceProvider.LazyGetRequiredService<ItemJnlPostLine>();
    private InventoryCostPoster CostPoster => LazyServiceProvider.LazyGetRequiredService<InventoryCostPoster>();
    private GLRegisterManager RegisterManager => LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
    private GeneralLedgerSetupManager GLSetupManager => LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();
    private WarehouseManager WarehouseManager => LazyServiceProvider.LazyGetRequiredService<WarehouseManager>();
    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public PhysInvtOrderManager(
        IRepository<PhysInvtOrderHeader, Guid> orderRepository,
        IRepository<Item, Guid> itemRepository,
        IRepository<ItemLedgerEntry, Guid> entryRepository,
        IRepository<PhysInvtCountingPeriod, Guid> periodRepository
    )
    {
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _entryRepository = entryRepository;
        _periodRepository = periodRepository;
    }

    public async Task<string> NextNoAsync() => $"PIO{await EntryNos.NextAsync(ErpSequenceNames.PhysInvtOrderNo):D5}";

    /// <summary>
    /// Adds a line for every item, variant, lot and bin with stock at the order's location that the
    /// order does not have yet. <paramref name="onlyItemsDueForCounting"/> keeps the items whose next
    /// counting date has come; <paramref name="includeItemsNotOnInventory"/> adds a zero line for
    /// the matching items without stock, so stock nobody booked is found too.
    /// </summary>
    public async Task<int> CalculateLinesAsync(Guid orderId, bool onlyItemsDueForCounting, string itemCategoryCode, bool includeItemsNotOnInventory)
    {
        var order = await _orderRepository.GetAsync(orderId, includeDetails: true);
        order.EnsureOpen();
        var category = CodeTableEntity.NormalizeCode(itemCategoryCode);
        var items = (await _itemRepository.GetListAsync(i => i.Type == ItemType.Inventory && !i.Blocked && (category == null || i.ItemCategoryCode == category)))
            .Where(i => !onlyItemsDueForCounting || (i.NextCountingDate.HasValue && i.NextCountingDate.Value <= order.PostingDate))
            .ToDictionary(i => i.Id);

        var location = order.LocationCode;
        var stock = (await _entryRepository.GetListAsync(e => e.LocationCode == location && e.PostingDate <= order.PostingDate))
            .Where(e => items.ContainsKey(e.ItemId))
            .GroupBy(e => new { e.ItemId, e.VariantCode, e.LotNo, e.SerialNo })
            .Select(g => new { g.Key.ItemId, g.Key.VariantCode, g.Key.LotNo, g.Key.SerialNo, Quantity = g.Sum(e => e.Quantity) })
            .Where(x => x.Quantity != 0m)
            .ToList();

        // At a location with bins an untracked item is counted bin by bin. Lots are not kept per
        // bin, so a tracked item is counted lot by lot and its difference goes through the bins.
        var binLocation = (await WarehouseManager.FindLocationAsync(location))?.BinMandatory == true;
        var contents = binLocation ? await WarehouseManager.GetBinContentsAsync(location) : [];

        var added = 0;
        void Add(Item item, string variant, string bin, string lot, string serial, decimal quantity)
        {
            if (order.Lines.Any(l => l.ItemId == item.Id && l.SameStock(variant, bin, lot, serial)))
            {
                return;
            }

            order.AddLine(GuidGenerator.Create(), item, variant, bin, lot, serial, quantity);
            added++;
        }

        foreach (var row in stock.OrderBy(x => items[x.ItemId].No))
        {
            var item = items[row.ItemId];
            if (binLocation && row.LotNo == null && row.SerialNo == null)
            {
                continue;
            }

            Add(item, row.VariantCode, null, row.LotNo, row.SerialNo, row.Quantity);
        }

        foreach (var content in contents.Where(c => items.ContainsKey(c.ItemId)))
        {
            var tracked = stock.Any(x => x.ItemId == content.ItemId && (x.LotNo != null || x.SerialNo != null));
            if (!tracked)
            {
                Add(items[content.ItemId], content.VariantCode, content.BinCode, null, null, content.Quantity);
            }
        }

        if (includeItemsNotOnInventory)
        {
            foreach (var item in items.Values.Where(i => stock.All(x => x.ItemId != i.Id)).OrderBy(i => i.No))
            {
                Add(item, null, binLocation ? (await WarehouseManager.FindLocationAsync(location))?.DefaultBinCode : null, null, null, 0m);
            }
        }

        await _orderRepository.UpdateAsync(order, autoSave: true);
        return added;
    }

    /// <summary>Posts the counted differences as positive and negative adjustments and marks the items counted.</summary>
    public async Task<PhysInvtOrderHeader> FinishAsync(Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId, includeDetails: true);
        order.EnsureOpen();
        if (order.Lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.PhysInvtOrderHasNoLines).WithData("no", order.No);
        }

        var notCounted = order.Lines.OrderBy(l => l.LineNo).FirstOrDefault(l => !l.Recorded);
        if (notCounted != null)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.PhysInvtLineNotRecorded).WithData("no", order.No).WithData("lineNo", notCounted.LineNo);
        }

        await GLSetupManager.CheckPostingDateAsync(order.PostingDate);
        var register = await RegisterManager.OpenAsync(order.PostingDate, SourceCode, order.No);
        var context = new GLPostingContext(register, SourceCode);

        foreach (var line in order.Lines.Where(l => l.QtyDifference != 0m).OrderBy(l => l.LineNo))
        {
            var item = await _itemRepository.GetAsync(line.ItemId);
            var difference = line.QtyDifference;
            var entries = await PostLine.PostAllAsync(new ItemPostingRequest
            {
                ItemId = item.Id,
                ItemNo = item.No,
                PostingDate = order.PostingDate,
                EntryType = difference > 0m ? ItemLedgerEntryType.PositiveAdjmt : ItemLedgerEntryType.NegativeAdjmt,
                DocumentNo = order.No,
                Description = line.Description,
                Quantity = Math.Abs(difference),
                UseItemCost = true,
                LocationCode = line.LocationCode,
                VariantCode = line.VariantCode,
                BinCode = line.BinCode,
                Tracking = line.LotNo == null && line.SerialNo == null ? null : [new TrackingPart(line.LotNo, line.SerialNo, Math.Abs(difference), null)],
            });

            var value = entries.Sum(e => Math.Round(e.Quantity * e.UnitCost, 2, MidpointRounding.AwayFromZero));
            await CostPoster.PostAdjustmentAsync(item, line.LocationCode, value, order.PostingDate, order.No, line.Description ?? item.Description, context);
        }

        await RegisterManager.CloseAsync(register);

        foreach (var itemId in order.Lines.Select(l => l.ItemId).Distinct())
        {
            var item = await _itemRepository.GetAsync(itemId);
            var frequency = 0;
            if (item.PhysInvtCountingPeriodCode != null)
            {
                var code = item.PhysInvtCountingPeriodCode;
                frequency = (await _periodRepository.FirstOrDefaultAsync(p => p.Code == code))?.CountFrequencyPerYear ?? 0;
            }

            item.RecordCount(order.PostingDate, frequency);
            await _itemRepository.UpdateAsync(item, autoSave: true);
        }

        order.Finish();
        return await _orderRepository.UpdateAsync(order, autoSave: true);
    }
}
