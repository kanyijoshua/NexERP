using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>Bin: a shelf, rack or zone of a location that stock is kept in. The code is unique per location.</summary>
public class Bin : CompanyEntity
{
    public string LocationCode { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }

    /// <summary>A blocked bin takes no stock in and lets none out.</summary>
    public bool Blocked { get; private set; }

    protected Bin() { }

    public Bin(Guid id, string locationCode, string code, string description)
        : base(id)
    {
        LocationCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        Code = NormalizeCode(Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength));
        Set(description, false);
    }

    public void Set(string description, bool blocked)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Blocked = blocked;
    }

    public static string NormalizeCode(string code) => CodeTableEntity.NormalizeCode(code);
}

/// <summary>
/// Warehouse Entry: one change to the quantity of an item in a bin. Item ledger movements at a
/// location with bins write one each; put-aways, picks and movements write a pair (out of one bin,
/// into another) that leaves the item ledger as it is. The quantity in a bin is the sum.
/// </summary>
public class WarehouseEntry : LedgerEntryBase
{
    public DateTime RegisteringDate { get; private set; }
    public WarehouseEntryType EntryType { get; private set; }
    public string LocationCode { get; private set; }
    public string BinCode { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string VariantCode { get; private set; }

    /// <summary>Signed, in the item's base unit of measure.</summary>
    public decimal Quantity { get; private set; }

    /// <summary>The document of the movement: the posted document, or the put-away, pick or movement.</summary>
    public string DocumentNo { get; private set; }

    public string Description { get; private set; }

    /// <summary>The item ledger entry this entry follows; null for a move between bins.</summary>
    public Guid? ItemLedgerEntryId { get; private set; }

    protected WarehouseEntry() { }

    public WarehouseEntry(
        Guid id,
        DateTime registeringDate,
        WarehouseEntryType entryType,
        string locationCode,
        string binCode,
        Guid itemId,
        string itemNo,
        string variantCode,
        decimal quantity,
        string documentNo,
        string description,
        Guid? itemLedgerEntryId
    )
        : base(id)
    {
        RegisteringDate = registeringDate.Date;
        EntryType = entryType;
        LocationCode = locationCode;
        BinCode = binCode;
        ItemId = itemId;
        ItemNo = itemNo;
        VariantCode = variantCode;
        Quantity = quantity;
        DocumentNo = Check.Length(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        ItemLedgerEntryId = itemLedgerEntryId;
    }
}

/// <summary>What one bin holds of one item and variant.</summary>
public sealed class BinContent
{
    public string LocationCode { get; init; }
    public string BinCode { get; init; }
    public Guid ItemId { get; init; }
    public string ItemNo { get; init; }
    public string VariantCode { get; init; }
    public decimal Quantity { get; init; }
}

/// <summary>
/// Warehouse Activity: a put-away (from the receipt bin to where the stock is stored), a pick (from
/// where it is stored to the shipment bin) or a movement between any two bins. Registering it moves
/// the quantities to handle; it is registered once every line is handled in full.
/// </summary>
public class WarehouseActivityHeader : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public WarehouseActivityType Type { get; private set; }
    public string LocationCode { get; private set; }
    public WarehouseActivityStatus Status { get; private set; }
    public WarehouseSourceType SourceType { get; private set; }
    public string SourceNo { get; private set; }
    public Guid? SourceId { get; private set; }
    public string Description { get; private set; }
    public DateTime? RegisteredDate { get; private set; }

    public Collection<WarehouseActivityLine> Lines { get; private set; } = new();

    protected WarehouseActivityHeader() { }

    public WarehouseActivityHeader(Guid id, string no, WarehouseActivityType type, string locationCode)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);
        Type = type;
        LocationCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        Status = WarehouseActivityStatus.Open;
    }

    public void SetSource(WarehouseSourceType sourceType, string sourceNo, Guid? sourceId)
    {
        SourceType = sourceType;
        SourceNo = Check.Length(sourceNo, nameof(sourceNo), ErpDomainConsts.MaxDocumentNoLength);
        SourceId = sourceId;
    }

    public void SetDescription(string description)
    {
        EnsureOpen();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public WarehouseActivityLine AddLine(Guid id, Item item, string variantCode, string description, string fromBinCode, string toBinCode, decimal quantity, Guid? sourceLineId)
    {
        EnsureOpen();
        var lineNo = (Lines.Count == 0 ? 0 : Lines.Max(l => l.LineNo)) + 10000;
        var line = new WarehouseActivityLine(id, Id, lineNo, item.Id, item.No, variantCode, description ?? item.Description, quantity, sourceLineId);
        line.SetBins(fromBinCode, toBinCode);
        Lines.Add(line);
        return line;
    }

    public void EnsureOpen()
    {
        if (Status == WarehouseActivityStatus.Registered)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.WarehouseDocumentRegistered).WithData("no", No);
        }
    }

    internal void CompleteIfHandled(DateTime date)
    {
        if (Lines.All(l => l.OutstandingQuantity == 0m))
        {
            Status = WarehouseActivityStatus.Registered;
            RegisteredDate = date.Date;
        }
    }
}

public class WarehouseActivityLine : Entity<Guid>
{
    public Guid WarehouseActivityHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string VariantCode { get; private set; }
    public string Description { get; private set; }
    public string FromBinCode { get; private set; }
    public string ToBinCode { get; private set; }

    /// <summary>In the item's base unit of measure.</summary>
    public decimal Quantity { get; private set; }

    public decimal QtyToHandle { get; private set; }
    public decimal QtyHandled { get; private set; }

    /// <summary>The receipt line or sales line the line was made for.</summary>
    public Guid? SourceLineId { get; private set; }

    public decimal OutstandingQuantity => Quantity - QtyHandled;

    protected WarehouseActivityLine() { }

    public WarehouseActivityLine(Guid id, Guid headerId, int lineNo, Guid itemId, string itemNo, string variantCode, string description, decimal quantity, Guid? sourceLineId)
        : base(id)
    {
        if (quantity <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.NothingToHandle).WithData("no", itemNo);
        }

        WarehouseActivityHeaderId = headerId;
        LineNo = lineNo;
        ItemId = itemId;
        ItemNo = itemNo;
        VariantCode = CodeTableEntity.NormalizeCode(variantCode);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        QtyToHandle = quantity;
        SourceLineId = sourceLineId;
    }

    public void SetBins(string fromBinCode, string toBinCode)
    {
        FromBinCode = Bin.NormalizeCode(fromBinCode);
        ToBinCode = Bin.NormalizeCode(toBinCode);
    }

    public void SetQtyToHandle(string documentNo, decimal qtyToHandle)
    {
        if (qtyToHandle < 0m || qtyToHandle > OutstandingQuantity)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.QtyToHandleTooLarge)
                .WithData("no", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("qtyToHandle", qtyToHandle)
                .WithData("outstanding", OutstandingQuantity);
        }

        QtyToHandle = qtyToHandle;
    }

    /// <summary>Only a line nothing was handled of can have its quantity changed.</summary>
    public void SetQuantity(string documentNo, decimal quantity)
    {
        if (quantity <= 0m || quantity < QtyHandled)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.QtyToHandleTooLarge)
                .WithData("no", documentNo)
                .WithData("lineNo", LineNo)
                .WithData("qtyToHandle", quantity)
                .WithData("outstanding", Quantity);
        }

        Quantity = quantity;
        QtyToHandle = Math.Min(QtyToHandle, OutstandingQuantity);
    }

    internal void Handle()
    {
        QtyHandled += QtyToHandle;
        QtyToHandle = OutstandingQuantity;
    }
}

/// <summary>
/// Keeps the bins of a location in step with its stock and moves stock between bins.
/// <para>
/// Every item ledger entry at a location with bins also goes into a bin: the bin the movement
/// names, else the receipt bin (when the location requires put-aways) or the default bin for stock
/// coming in, and the shipment bin (when the location requires picks) or the bins holding the item,
/// the default bin first, for stock going out. A bin never goes below zero through a put-away, pick
/// or movement; a shipment from a location that requires picks needs the stock in the shipment bin.
/// </para>
/// </summary>
public class WarehouseManager : DomainService
{
    private readonly IRepository<Bin, Guid> _binRepository;
    private readonly IRepository<WarehouseEntry, Guid> _entryRepository;
    private readonly IRepository<WarehouseActivityHeader, Guid> _activityRepository;
    private readonly IRepository<Location, Guid> _locationRepository;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();
    private IRepository<Item, Guid> ItemRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>();

    public WarehouseManager(
        IRepository<Bin, Guid> binRepository,
        IRepository<WarehouseEntry, Guid> entryRepository,
        IRepository<WarehouseActivityHeader, Guid> activityRepository,
        IRepository<Location, Guid> locationRepository
    )
    {
        _binRepository = binRepository;
        _entryRepository = entryRepository;
        _activityRepository = activityRepository;
        _locationRepository = locationRepository;
    }

    public async Task<Location> FindLocationAsync(string locationCode)
    {
        var code = CodeTableEntity.NormalizeCode(locationCode);
        return code == null ? null : await _locationRepository.FirstOrDefaultAsync(l => l.Code == code);
    }

    /// <summary>A bin of the location that is not blocked.</summary>
    public async Task<Bin> GetBinAsync(string locationCode, string binCode)
    {
        var location = CodeTableEntity.NormalizeCode(locationCode);
        var code = Bin.NormalizeCode(binCode);
        var bin = await _binRepository.FirstOrDefaultAsync(b => b.LocationCode == location && b.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Inventory.BinNotFound).WithData("binCode", code ?? "").WithData("locationCode", location ?? "");
        if (bin.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.BinBlocked).WithData("binCode", bin.Code).WithData("locationCode", bin.LocationCode);
        }

        return bin;
    }

    public async Task<List<BinContent>> GetBinContentsAsync(string locationCode = null, string binCode = null, Guid? itemId = null)
    {
        var location = CodeTableEntity.NormalizeCode(locationCode);
        var bin = Bin.NormalizeCode(binCode);
        var entries = await _entryRepository.GetListAsync(e =>
            (location == null || e.LocationCode == location) && (bin == null || e.BinCode == bin) && (itemId == null || e.ItemId == itemId));
        return entries
            .GroupBy(e => new { e.LocationCode, e.BinCode, e.ItemId, e.ItemNo, e.VariantCode })
            .Select(g => new BinContent
            {
                LocationCode = g.Key.LocationCode,
                BinCode = g.Key.BinCode,
                ItemId = g.Key.ItemId,
                ItemNo = g.Key.ItemNo,
                VariantCode = g.Key.VariantCode,
                Quantity = g.Sum(e => e.Quantity),
            })
            .Where(c => c.Quantity != 0m)
            .OrderBy(c => c.LocationCode)
            .ThenBy(c => c.BinCode)
            .ThenBy(c => c.ItemNo)
            .ToList();
    }

    public async Task<decimal> GetQuantityInBinAsync(string locationCode, string binCode, Guid itemId, string variantCode)
    {
        var variant = CodeTableEntity.NormalizeCode(variantCode);
        var entries = await _entryRepository.GetListAsync(e => e.LocationCode == locationCode && e.BinCode == binCode && e.ItemId == itemId && e.VariantCode == variant);
        return entries.Sum(e => e.Quantity);
    }

    /// <summary>Puts an item ledger entry into the bins of its location; nothing happens at a location without bins.</summary>
    public async Task RecordItemEntryAsync(ItemLedgerEntry entry, string binCode, string description)
    {
        var location = await FindLocationAsync(entry.LocationCode);
        if (location == null || !location.BinMandatory || entry.Quantity == 0m)
        {
            return;
        }

        if (entry.Quantity > 0m)
        {
            var into = Bin.NormalizeCode(binCode) ?? (location.RequirePutAway ? location.ReceiptBinCode : location.DefaultBinCode)
                ?? throw new BusinessException(ErpErrorCodes.Inventory.BinRequired).WithData("locationCode", location.Code).WithData("itemNo", entry.ItemNo);
            await GetBinAsync(location.Code, into);
            await InsertAsync(entry, WarehouseEntryType.PositiveAdjmt, location.Code, into, entry.Quantity, description);
            return;
        }

        var quantity = -entry.Quantity;
        var named = Bin.NormalizeCode(binCode) ?? (location.RequirePick ? location.ShipmentBinCode : null);
        if (named != null)
        {
            await GetBinAsync(location.Code, named);
            await EnsureContentAsync(location.Code, named, entry.ItemId, entry.ItemNo, entry.VariantCode, quantity, location.RequirePick && binCode == null);
            await InsertAsync(entry, WarehouseEntryType.NegativeAdjmt, location.Code, named, -quantity, description);
            return;
        }

        // No bin named: take from the bins that hold the item, the default bin first; what they
        // do not have leaves the default bin, as stock can leave a location it is not at.
        var contents = (await GetBinContentsAsync(location.Code, null, entry.ItemId))
            .Where(c => c.VariantCode == entry.VariantCode && c.Quantity > 0m && c.BinCode != location.ReceiptBinCode)
            .OrderBy(c => c.BinCode == location.DefaultBinCode ? 0 : 1)
            .ThenBy(c => c.BinCode)
            .ToList();
        foreach (var content in contents)
        {
            if (quantity == 0m)
            {
                break;
            }

            var take = Math.Min(quantity, content.Quantity);
            await InsertAsync(entry, WarehouseEntryType.NegativeAdjmt, location.Code, content.BinCode, -take, description);
            quantity -= take;
        }

        if (quantity > 0m)
        {
            var fallback = location.DefaultBinCode
                ?? throw new BusinessException(ErpErrorCodes.Inventory.InsufficientBinContent)
                    .WithData("binCode", "")
                    .WithData("locationCode", location.Code)
                    .WithData("itemNo", entry.ItemNo)
                    .WithData("available", -entry.Quantity - quantity)
                    .WithData("quantity", -entry.Quantity);
            await InsertAsync(entry, WarehouseEntryType.NegativeAdjmt, location.Code, fallback, -quantity, description);
        }
    }

    /// <summary>
    /// Makes a put-away of what a posted receipt brought into the receipt bin of a location that
    /// requires put-aways, less what earlier put-aways of the receipt already cover. Each line goes
    /// to the bin that already holds most of the item, else the default bin.
    /// </summary>
    public async Task<WarehouseActivityHeader> CreatePutAwayAsync(PostedPurchaseReceipt receipt)
    {
        var lines = receipt.Lines.Where(l => l.Type == Documents.DocumentLineType.Item && !l.Correction && l.Quantity > 0m).ToList();
        WarehouseActivityHeader header = null;
        foreach (var group in lines.GroupBy(l => CodeTableEntity.NormalizeCode(l.LocationCode ?? receipt.LocationCode)))
        {
            var location = await FindLocationAsync(group.Key);
            if (location == null || !location.RequirePutAway)
            {
                continue;
            }

            foreach (var line in group)
            {
                var item = await ItemRepository.FirstOrDefaultAsync(i => i.No == line.No);
                if (item == null || item.Type != ItemType.Inventory)
                {
                    continue;
                }

                var outstanding = line.Quantity * line.QtyPerUnitOfMeasure - await QuantityOnActivitiesAsync(WarehouseActivityType.PutAway, line.Id);
                if (outstanding <= 0m)
                {
                    continue;
                }

                header ??= await NewActivityAsync(WarehouseActivityType.PutAway, location.Code, WarehouseSourceType.PurchaseReceipt, receipt.No, receipt.Id);
                var toBin = await BestBinForAsync(location, item.Id, line.VariantCode);
                header.AddLine(GuidGenerator.Create(), item, line.VariantCode, line.Description, location.ReceiptBinCode, toBin, outstanding, line.Id);
            }
        }

        if (header == null)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.NothingToPutAway).WithData("documentNo", receipt.No);
        }

        return await _activityRepository.InsertAsync(header, autoSave: true);
    }

    /// <summary>
    /// Makes a pick of the item lines of a sales order at a location that requires picks: what the
    /// lines still need, from the bins that hold it (the default bin first, never the receipt or
    /// shipment bin, nor stock earlier open picks already take), to the shipment bin.
    /// </summary>
    public async Task<WarehouseActivityHeader> CreatePickAsync(SalesHeader order)
    {
        WarehouseActivityHeader header = null;
        var taken = new Dictionary<(string Bin, Guid Item, string Variant), decimal>();
        foreach (var line in order.Lines.Where(l => l.Type == Documents.DocumentLineType.Item && !l.No.IsNullOrWhiteSpace()).OrderBy(l => l.LineNo))
        {
            var location = await FindLocationAsync(line.LocationCode ?? order.LocationCode);
            if (location == null || !location.RequirePick)
            {
                continue;
            }

            var item = await ItemRepository.FirstOrDefaultAsync(i => i.No == line.No);
            if (item == null || item.Type != ItemType.Inventory)
            {
                continue;
            }

            var needed = line.Quantity * line.QtyPerUnitOfMeasure - await QuantityOnActivitiesAsync(WarehouseActivityType.Pick, line.Id);
            if (needed <= 0m)
            {
                continue;
            }

            var variant = CodeTableEntity.NormalizeCode(line.VariantCode);
            var openPicks = await OpenTakesAsync(location.Code, item.Id, variant);
            var contents = (await GetBinContentsAsync(location.Code, null, item.Id))
                .Where(c => c.VariantCode == variant && c.BinCode != location.ShipmentBinCode && c.BinCode != location.ReceiptBinCode)
                .OrderBy(c => c.BinCode == location.DefaultBinCode ? 0 : 1)
                .ThenBy(c => c.BinCode)
                .ToList();
            foreach (var content in contents)
            {
                if (needed == 0m)
                {
                    break;
                }

                var key = (content.BinCode, item.Id, variant);
                var free = content.Quantity - openPicks.GetValueOrDefault(content.BinCode) - taken.GetValueOrDefault(key);
                if (free <= 0m)
                {
                    continue;
                }

                var take = Math.Min(needed, free);
                header ??= await NewActivityAsync(WarehouseActivityType.Pick, location.Code, WarehouseSourceType.SalesOrder, order.No, order.Id);
                header.AddLine(GuidGenerator.Create(), item, variant, line.Description, content.BinCode, location.ShipmentBinCode, take, line.Id);
                taken[key] = taken.GetValueOrDefault(key) + take;
                needed -= take;
            }
        }

        if (header == null)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.NothingToPick).WithData("documentNo", order.No);
        }

        return await _activityRepository.InsertAsync(header, autoSave: true);
    }

    public async Task<WarehouseActivityHeader> CreateMovementAsync(string locationCode, string description)
    {
        var location = await FindLocationAsync(locationCode);
        if (location == null || !location.BinMandatory)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.BinsNotUsed).WithData("locationCode", CodeTableEntity.NormalizeCode(locationCode) ?? "");
        }

        var header = await NewActivityAsync(WarehouseActivityType.Movement, location.Code, WarehouseSourceType.None, null, null);
        header.SetDescription(description);
        return await _activityRepository.InsertAsync(header, autoSave: true);
    }

    /// <summary>Moves the quantity to handle of every line from its bin to the other; the document is registered once nothing is left.</summary>
    public async Task<WarehouseActivityHeader> RegisterAsync(Guid activityId, DateTime registeringDate)
    {
        var header = await _activityRepository.GetAsync(activityId, includeDetails: true);
        header.EnsureOpen();
        var toHandle = header.Lines.Where(l => l.QtyToHandle > 0m).OrderBy(l => l.LineNo).ToList();
        if (toHandle.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.NothingToHandle).WithData("no", header.No);
        }

        // Every line is checked before anything moves, so a document is never half registered.
        var needed = new Dictionary<(string Bin, Guid Item, string Variant), decimal>();
        foreach (var line in toHandle)
        {
            if (line.FromBinCode == null || line.ToBinCode == null)
            {
                throw new BusinessException(ErpErrorCodes.Inventory.BinRequired).WithData("locationCode", header.LocationCode).WithData("itemNo", line.ItemNo);
            }

            if (line.FromBinCode == line.ToBinCode)
            {
                throw new BusinessException(ErpErrorCodes.Inventory.SameBin).WithData("binCode", line.FromBinCode).WithData("lineNo", line.LineNo);
            }

            await GetBinAsync(header.LocationCode, line.FromBinCode);
            await GetBinAsync(header.LocationCode, line.ToBinCode);
            var key = (line.FromBinCode, line.ItemId, line.VariantCode);
            needed[key] = needed.GetValueOrDefault(key) + line.QtyToHandle;
            await EnsureContentAsync(header.LocationCode, line.FromBinCode, line.ItemId, line.ItemNo, line.VariantCode, needed[key], false);
        }

        foreach (var line in toHandle)
        {
            await InsertMovementAsync(header, line, line.FromBinCode, -line.QtyToHandle, registeringDate);
            await InsertMovementAsync(header, line, line.ToBinCode, line.QtyToHandle, registeringDate);
            line.Handle();
        }

        header.CompleteIfHandled(registeringDate);
        return await _activityRepository.UpdateAsync(header, autoSave: true);
    }

    private async Task EnsureContentAsync(string locationCode, string binCode, Guid itemId, string itemNo, string variantCode, decimal quantity, bool pickRequired)
    {
        var available = await GetQuantityInBinAsync(locationCode, binCode, itemId, variantCode);
        if (available >= quantity)
        {
            return;
        }

        if (pickRequired)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.PickRequired).WithData("itemNo", itemNo).WithData("binCode", binCode).WithData("locationCode", locationCode);
        }

        throw new BusinessException(ErpErrorCodes.Inventory.InsufficientBinContent)
            .WithData("binCode", binCode)
            .WithData("locationCode", locationCode)
            .WithData("itemNo", itemNo)
            .WithData("available", available)
            .WithData("quantity", quantity);
    }

    /// <summary>The storage bin (not the receipt or shipment bin) holding most of the item, else the default bin.</summary>
    private async Task<string> BestBinForAsync(Location location, Guid itemId, string variantCode)
    {
        var variant = CodeTableEntity.NormalizeCode(variantCode);
        var best = (await GetBinContentsAsync(location.Code, null, itemId))
            .Where(c => c.VariantCode == variant && c.Quantity > 0m && c.BinCode != location.ReceiptBinCode && c.BinCode != location.ShipmentBinCode)
            .OrderByDescending(c => c.Quantity)
            .FirstOrDefault();
        return best?.BinCode ?? (location.DefaultBinCode == location.ReceiptBinCode ? null : location.DefaultBinCode);
    }

    /// <summary>The quantity of a receipt or sales line that activities of a type already cover: handled, or still to handle on an open one.</summary>
    private async Task<decimal> QuantityOnActivitiesAsync(WarehouseActivityType type, Guid sourceLineId)
    {
        var query = await _activityRepository.WithDetailsAsync(h => h.Lines);
        var headers = await AsyncExecuter.ToListAsync(query.Where(h => h.Type == type && h.Lines.Any(l => l.SourceLineId == sourceLineId)));
        return headers
            .SelectMany(h => h.Lines.Where(l => l.SourceLineId == sourceLineId).Select(l => h.Status == WarehouseActivityStatus.Open ? l.Quantity : l.QtyHandled))
            .Sum();
    }

    /// <summary>What open picks and movements are yet to take out of each bin, for an item.</summary>
    private async Task<Dictionary<string, decimal>> OpenTakesAsync(string locationCode, Guid itemId, string variantCode)
    {
        var query = await _activityRepository.WithDetailsAsync(h => h.Lines);
        var headers = await AsyncExecuter.ToListAsync(query.Where(h => h.LocationCode == locationCode && h.Status == WarehouseActivityStatus.Open && h.Type != WarehouseActivityType.PutAway));
        return headers
            .SelectMany(h => h.Lines)
            .Where(l => l.ItemId == itemId && l.VariantCode == variantCode && l.FromBinCode != null)
            .GroupBy(l => l.FromBinCode)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.OutstandingQuantity));
    }

    private async Task<WarehouseActivityHeader> NewActivityAsync(WarehouseActivityType type, string locationCode, WarehouseSourceType sourceType, string sourceNo, Guid? sourceId)
    {
        var (sequence, prefix) = type switch
        {
            WarehouseActivityType.PutAway => (ErpSequenceNames.PutAwayNo, "PA"),
            WarehouseActivityType.Pick => (ErpSequenceNames.PickNo, "PI"),
            _ => (ErpSequenceNames.MovementNo, "MV"),
        };
        var header = new WarehouseActivityHeader(GuidGenerator.Create(), $"{prefix}{await EntryNos.NextAsync(sequence):D5}", type, locationCode);
        header.SetSource(sourceType, sourceNo, sourceId);
        return header;
    }

    private async Task InsertAsync(ItemLedgerEntry entry, WarehouseEntryType type, string locationCode, string binCode, decimal quantity, string description)
    {
        await _entryRepository.InsertAsync(
            new WarehouseEntry(GuidGenerator.Create(), entry.PostingDate, type, locationCode, binCode, entry.ItemId, entry.ItemNo, entry.VariantCode, quantity, entry.DocumentNo, description, entry.Id)
            {
                EntryNo = await EntryNos.NextAsync(ErpSequenceNames.WarehouseEntry),
            },
            autoSave: true
        );
    }

    private async Task InsertMovementAsync(WarehouseActivityHeader header, WarehouseActivityLine line, string binCode, decimal quantity, DateTime date)
    {
        await _entryRepository.InsertAsync(
            new WarehouseEntry(GuidGenerator.Create(), date, WarehouseEntryType.Movement, header.LocationCode, binCode, line.ItemId, line.ItemNo, line.VariantCode, quantity, header.No, line.Description, null)
            {
                EntryNo = await EntryNos.NextAsync(ErpSequenceNames.WarehouseEntry),
            },
            autoSave: true
        );
    }
}
