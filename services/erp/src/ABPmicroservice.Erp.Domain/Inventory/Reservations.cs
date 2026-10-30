using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Reservation Entry: stock on hand at a location held for one document line. What is reserved
/// cannot leave the location for anything else; the line uses it up as it ships.
/// </summary>
public class ReservationEntry : CompanyEntity
{
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string LocationCode { get; private set; }
    public string VariantCode { get; private set; }

    /// <summary>Unsigned, in the item's base unit of measure.</summary>
    public decimal Quantity { get; private set; }

    public ReservationSourceType SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public string SourceDocumentNo { get; private set; }
    public int SourceLineNo { get; private set; }
    public DateTime? ShipmentDate { get; private set; }

    protected ReservationEntry() { }

    public ReservationEntry(
        Guid id,
        Item item,
        string locationCode,
        string variantCode,
        ReservationSourceType sourceType,
        Guid sourceId,
        string sourceDocumentNo,
        int sourceLineNo,
        DateTime? shipmentDate
    )
        : base(id)
    {
        ItemId = item.Id;
        ItemNo = item.No;
        LocationCode = CodeTableEntity.NormalizeCode(locationCode);
        VariantCode = CodeTableEntity.NormalizeCode(variantCode);
        SourceType = sourceType;
        SourceId = sourceId;
        SourceDocumentNo = Check.Length(sourceDocumentNo, nameof(sourceDocumentNo), ErpDomainConsts.MaxDocumentNoLength);
        SourceLineNo = sourceLineNo;
        ShipmentDate = shipmentDate?.Date;
    }

    internal void Add(decimal quantity) => Quantity += quantity;
}

/// <summary>
/// Reserves stock on hand for sales lines and assembly components and keeps other movements off it:
/// stock can leave a location only as far as what stays covers the reservations of other lines.
/// </summary>
public class ReservationManager : DomainService
{
    private readonly IRepository<ReservationEntry, Guid> _repository;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemEntryRepository;
    private readonly IRepository<Item, Guid> _itemRepository;

    public ReservationManager(IRepository<ReservationEntry, Guid> repository, IRepository<ItemLedgerEntry, Guid> itemEntryRepository, IRepository<Item, Guid> itemRepository)
    {
        _repository = repository;
        _itemEntryRepository = itemEntryRepository;
        _itemRepository = itemRepository;
    }

    public async Task<decimal> GetReservedAsync(Guid itemId, string locationCode, string variantCode, Guid? exceptSourceId = null)
    {
        var location = CodeTableEntity.NormalizeCode(locationCode);
        var variant = CodeTableEntity.NormalizeCode(variantCode);
        var entries = await _repository.GetListAsync(r => r.ItemId == itemId && r.LocationCode == location && r.VariantCode == variant && r.SourceId != exceptSourceId);
        return entries.Sum(r => r.Quantity);
    }

    public async Task<decimal> GetReservedForSourceAsync(Guid sourceId) => (await _repository.GetListAsync(r => r.SourceId == sourceId)).Sum(r => r.Quantity);

    /// <summary>What is on hand at the location and not reserved for another line than <paramref name="forSourceId"/>.</summary>
    public async Task<decimal> GetAvailableToReserveAsync(Guid itemId, string locationCode, string variantCode, Guid? forSourceId = null)
    {
        var location = CodeTableEntity.NormalizeCode(locationCode);
        var variant = CodeTableEntity.NormalizeCode(variantCode);
        var onHand = (await _itemEntryRepository.GetListAsync(e => e.ItemId == itemId && e.LocationCode == location && e.VariantCode == variant)).Sum(e => e.Quantity);
        return onHand - await GetReservedAsync(itemId, location, variant, forSourceId);
    }

    /// <summary>Reserves up to <paramref name="quantity"/> for a line and returns what it could reserve.</summary>
    public async Task<decimal> ReserveAsync(
        Item item,
        string locationCode,
        string variantCode,
        decimal quantity,
        ReservationSourceType sourceType,
        Guid sourceId,
        string documentNo,
        int lineNo,
        DateTime? shipmentDate
    )
    {
        var free = await GetAvailableToReserveAsync(item.Id, locationCode, variantCode, sourceId) - await GetReservedForSourceAsync(sourceId);
        var reserve = Math.Min(quantity, free);
        if (reserve <= 0m)
        {
            return 0m;
        }

        var entry = await _repository.FirstOrDefaultAsync(r => r.SourceId == sourceId);
        if (entry == null)
        {
            entry = new ReservationEntry(GuidGenerator.Create(), item, locationCode, variantCode, sourceType, sourceId, documentNo, lineNo, shipmentDate);
            entry.Add(reserve);
            await _repository.InsertAsync(entry, autoSave: true);
        }
        else
        {
            entry.Add(reserve);
            await _repository.UpdateAsync(entry, autoSave: true);
        }

        return reserve;
    }

    /// <summary>Reserves what each item line of a sales order still has to ship, as far as there is stock; returns the lines it reserved for.</summary>
    public async Task<int> ReserveSalesOrderAsync(SalesHeader order)
    {
        var reservedLines = 0;
        var noStock = new List<string>();
        foreach (var line in order.Lines.Where(l => l.Type == Documents.DocumentLineType.Item && !l.No.IsNullOrWhiteSpace() && !l.DropShipment))
        {
            var item = await _itemRepository.FirstOrDefaultAsync(i => i.No == line.No);
            if (item == null || item.Type != ItemType.Inventory)
            {
                continue;
            }

            var outstanding = (line.Quantity - line.QuantityShipped) * line.QtyPerUnitOfMeasure - await GetReservedForSourceAsync(line.Id);
            if (outstanding <= 0m)
            {
                continue;
            }

            var reserved = await ReserveAsync(item, line.LocationCode ?? order.LocationCode, line.VariantCode, outstanding, ReservationSourceType.SalesLine, line.Id, order.No, line.LineNo, order.PostingDate);
            if (reserved > 0m)
            {
                reservedLines++;
            }
            else
            {
                noStock.Add(item.No);
            }
        }

        if (reservedLines == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.NothingToReserve)
                .WithData("itemNo", string.Join(", ", noStock))
                .WithData("locationCode", order.LocationCode ?? "");
        }

        return reservedLines;
    }

    public async Task CancelAsync(IEnumerable<Guid> sourceIds)
    {
        var ids = sourceIds.ToList();
        if (ids.Count > 0)
        {
            await _repository.DeleteAsync(r => ids.Contains(r.SourceId), autoSave: true);
        }
    }

    /// <summary>A line used part of its reservation by shipping or consuming it.</summary>
    public async Task ConsumeAsync(Guid sourceId, decimal quantity)
    {
        var entry = await _repository.FirstOrDefaultAsync(r => r.SourceId == sourceId);
        if (entry == null)
        {
            return;
        }

        if (entry.Quantity <= quantity)
        {
            await _repository.DeleteAsync(entry, autoSave: true);
            return;
        }

        entry.Add(-quantity);
        await _repository.UpdateAsync(entry, autoSave: true);
    }

    /// <summary>Refuses a movement out of a location that would leave less than other lines have reserved there.</summary>
    public async Task CheckOutboundAsync(Item item, string locationCode, string variantCode, decimal quantity, Guid? sourceId)
    {
        var reservedByOthers = await GetReservedAsync(item.Id, locationCode, variantCode, sourceId);
        if (reservedByOthers <= 0m)
        {
            return;
        }

        var location = CodeTableEntity.NormalizeCode(locationCode);
        var variant = CodeTableEntity.NormalizeCode(variantCode);
        var onHand = (await _itemEntryRepository.GetListAsync(e => e.ItemId == item.Id && e.LocationCode == location && e.VariantCode == variant)).Sum(e => e.Quantity);
        if (onHand - quantity < reservedByOthers)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.ReservedForOtherDocument)
                .WithData("itemNo", item.No)
                .WithData("locationCode", location ?? "")
                .WithData("reserved", reservedByOthers)
                .WithData("available", Math.Max(0m, onHand - reservedByOthers));
        }
    }
}
