using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Ships and receives transfer orders. Shipping moves each line's quantity to ship out of the
/// from-location into the in-transit location; receiving moves the quantity to receive out of
/// transit into the to-location, taking exactly the lots and costs that were shipped. The stock
/// keeps its cost all the way; the G/L only moves when the locations' inventory accounts differ.
/// </summary>
public class TransferPostingEngine : DomainService
{
    private const string SourceCode = "TRANSFER";

    private readonly IRepository<TransferHeader, Guid> _transferRepository;
    private readonly IRepository<PostedTransfer, Guid> _postedRepository;
    private readonly IRepository<PostedTransferLine, Guid> _postedLineRepository;
    private readonly IRepository<Location, Guid> _locationRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _entryRepository;
    private readonly ItemJnlPostLine _postLine;
    private readonly InventoryCostPoster _costPoster;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;

    private ItemTrackingManager TrackingManager => LazyServiceProvider.LazyGetRequiredService<ItemTrackingManager>();
    private InventorySetupManager SetupManager => LazyServiceProvider.LazyGetRequiredService<InventorySetupManager>();
    private NoSeriesManager NoSeriesManager => LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>();

    public TransferPostingEngine(
        IRepository<TransferHeader, Guid> transferRepository,
        IRepository<PostedTransfer, Guid> postedRepository,
        IRepository<PostedTransferLine, Guid> postedLineRepository,
        IRepository<Location, Guid> locationRepository,
        IRepository<Item, Guid> itemRepository,
        IRepository<ItemLedgerEntry, Guid> entryRepository,
        ItemJnlPostLine postLine,
        InventoryCostPoster costPoster,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager
    )
    {
        _transferRepository = transferRepository;
        _postedRepository = postedRepository;
        _postedLineRepository = postedLineRepository;
        _locationRepository = locationRepository;
        _itemRepository = itemRepository;
        _entryRepository = entryRepository;
        _postLine = postLine;
        _costPoster = costPoster;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
    }

    public async Task<PostedTransfer> ShipAsync(Guid transferId)
    {
        var order = await PrepareAsync(transferId);
        var lines = order.Lines.Where(l => l.QtyToShip > 0m).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.TransferNothingToPost).WithData("documentNo", order.No);
        }

        var setup = await SetupManager.GetAsync();
        var no = await NumberAsync(setup.PostedTransferShipmentNos, "TS", order);
        var posted = new PostedTransfer(GuidGenerator.Create(), no, receipt: false, order);
        var register = await _registerManager.OpenAsync(order.PostingDate, SourceCode, no);
        var context = new GLPostingContext(register, SourceCode);

        foreach (var line in lines)
        {
            var item = await _itemRepository.GetAsync(line.ItemId);
            var postedLine = new PostedTransferLine(GuidGenerator.Create(), posted.Id, line, line.QtyToShip);
            posted.Lines.Add(postedLine);

            var baseQty = line.QtyToShip * line.QtyPerUnitOfMeasure;
            var outbound = await _postLine.PostAllAsync(new ItemPostingRequest
            {
                ItemId = item.Id,
                ItemNo = item.No,
                PostingDate = order.PostingDate,
                EntryType = ItemLedgerEntryType.Transfer,
                DocumentNo = no,
                Description = line.Description,
                Quantity = baseQty,
                Outbound = true,
                LocationCode = order.FromLocationCode,
                VariantCode = line.VariantCode,
                Tracking = await TrackingManager.TakeAsync(item, ItemTrackingSourceType.TransferLine, line.Id, baseQty),
                DocumentLineId = postedLine.Id,
            });

            var value = await MoveInAsync(item, outbound, order.InTransitCode, no, order.PostingDate, line, postedLine.Id);
            await _costPoster.PostTransferAsync(item, order.FromLocationCode, order.InTransitCode, value, order.PostingDate, no, line.Description, context);
            line.RecordShipped(line.QtyToShip);
        }

        await _postedRepository.InsertAsync(posted);
        await _registerManager.CloseAsync(register);
        order.RecordShipment(no);
        await _transferRepository.UpdateAsync(order, autoSave: true);
        return posted;
    }

    public async Task<PostedTransfer> ReceiveAsync(Guid transferId)
    {
        var order = await PrepareAsync(transferId);
        var lines = order.Lines.Where(l => l.QtyToReceive > 0m).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.TransferNothingToPost).WithData("documentNo", order.No);
        }

        var setup = await SetupManager.GetAsync();
        var no = await NumberAsync(setup.PostedTransferReceiptNos, "TR", order);
        var posted = new PostedTransfer(GuidGenerator.Create(), no, receipt: true, order);
        var register = await _registerManager.OpenAsync(order.PostingDate, SourceCode, no);
        var context = new GLPostingContext(register, SourceCode);

        foreach (var line in lines)
        {
            var item = await _itemRepository.GetAsync(line.ItemId);
            var postedLine = new PostedTransferLine(GuidGenerator.Create(), posted.Id, line, line.QtyToReceive);
            posted.Lines.Add(postedLine);

            // The stock in transit for this line: what its shipments put there and is still there.
            var shippedLineIds = (await _postedLineRepository.GetListAsync(l => l.TransferLineId == line.Id)).Select(l => l.Id).ToList();
            var inTransit = (await _entryRepository.GetListAsync(e =>
                    e.Positive && e.Open && e.LocationCode == order.InTransitCode && e.DocumentLineId != null && shippedLineIds.Contains(e.DocumentLineId.Value)))
                .Where(e => e.RemainingQuantity > 0m)
                .OrderBy(e => e.EntryNo)
                .ToList();

            var remaining = line.QtyToReceive * line.QtyPerUnitOfMeasure;
            var value = 0m;
            foreach (var entry in inTransit)
            {
                if (remaining == 0m)
                {
                    break;
                }

                var take = Math.Min(remaining, entry.RemainingQuantity);
                var outbound = await _postLine.PostAllAsync(new ItemPostingRequest
                {
                    ItemId = item.Id,
                    ItemNo = item.No,
                    PostingDate = order.PostingDate,
                    EntryType = ItemLedgerEntryType.Transfer,
                    DocumentNo = no,
                    Description = line.Description,
                    Quantity = take,
                    Outbound = true,
                    LocationCode = order.InTransitCode,
                    VariantCode = line.VariantCode,
                    Tracking = TrackingOf(entry, take),
                    AppliesToEntryId = entry.Id,
                    DocumentLineId = postedLine.Id,
                });
                value += await MoveInAsync(item, outbound, order.ToLocationCode, no, order.PostingDate, line, postedLine.Id);
                remaining -= take;
            }

            await _costPoster.PostTransferAsync(item, order.InTransitCode, order.ToLocationCode, value, order.PostingDate, no, line.Description, context);
            line.RecordReceived(line.QtyToReceive);
        }

        await _postedRepository.InsertAsync(posted);
        await _registerManager.CloseAsync(register);
        order.RecordReceipt(no);
        await _transferRepository.UpdateAsync(order, autoSave: true);
        return posted;
    }

    /// <summary>Takes the outbound entries' stock into <paramref name="locationCode"/>, lot by lot at their cost; returns its value.</summary>
    private async Task<decimal> MoveInAsync(Item item, List<ItemLedgerEntry> outbound, string locationCode, string documentNo, DateTime postingDate, TransferLine line, Guid documentLineId)
    {
        var value = 0m;
        foreach (var from in outbound)
        {
            var quantity = Math.Abs(from.Quantity);
            await _postLine.PostAllAsync(new ItemPostingRequest
            {
                ItemId = item.Id,
                ItemNo = item.No,
                PostingDate = postingDate,
                EntryType = ItemLedgerEntryType.Transfer,
                DocumentNo = documentNo,
                Description = line.Description,
                Quantity = quantity,
                UnitCost = from.UnitCost,
                Outbound = false,
                LocationCode = locationCode,
                VariantCode = line.VariantCode,
                Tracking = TrackingOf(from, quantity),
                DocumentLineId = documentLineId,
            });
            value += Math.Round(quantity * from.UnitCost, 2, MidpointRounding.AwayFromZero);
        }

        return value;
    }

    private static IReadOnlyList<TrackingPart> TrackingOf(ItemLedgerEntry entry, decimal quantity) =>
        entry.LotNo == null && entry.SerialNo == null ? null : [new TrackingPart(entry.LotNo, entry.SerialNo, quantity, entry.ExpirationDate)];

    /// <summary>Checks the order can be posted and releases it if it is still open.</summary>
    private async Task<TransferHeader> PrepareAsync(Guid transferId)
    {
        var order = await _transferRepository.GetAsync(transferId);
        if (order.Completed)
        {
            throw new BusinessException(ErpErrorCodes.Documents.OrderClosed).WithData("documentNo", order.No);
        }

        await _glSetupManager.CheckPostingDateAsync(order.PostingDate);

        var inTransit = order.InTransitCode == null ? null : await _locationRepository.FirstOrDefaultAsync(l => l.Code == order.InTransitCode);
        if (inTransit == null || !inTransit.UseAsInTransit)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.InTransitLocationRequired).WithData("documentNo", order.No);
        }
        if (order.FromLocationCode == order.ToLocationCode)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.SameLocation).WithData("lineNo", 0);
        }

        foreach (var code in new[] { order.FromLocationCode, order.ToLocationCode }.Where(c => c != null))
        {
            var location = await _locationRepository.FirstOrDefaultAsync(l => l.Code == code);
            if (location?.UseAsInTransit == true)
            {
                throw new BusinessException(ErpErrorCodes.Inventory.LocationIsInTransit).WithData("locationCode", code);
            }
        }

        if (order.Status == TransferStatus.Open)
        {
            order.Release();
        }

        return order;
    }

    private async Task<string> NumberAsync(string series, string prefix, TransferHeader order)
    {
        if (!series.IsNullOrWhiteSpace())
        {
            return await NoSeriesManager.GetNextNoAsync(series, order.PostingDate);
        }

        var count = await _postedRepository.CountAsync(p => p.TransferOrderNo == order.No);
        return count == 0 ? $"{prefix}-{order.No}" : $"{prefix}-{order.No}-{count + 1}";
    }
}
