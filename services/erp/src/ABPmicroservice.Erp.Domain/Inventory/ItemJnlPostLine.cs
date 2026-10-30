using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>One stock movement for <see cref="ItemJnlPostLine.PostAllAsync"/>.</summary>
public sealed class ItemPostingRequest
{
    public Guid ItemId { get; init; }
    public string ItemNo { get; init; }
    public DateTime PostingDate { get; init; }
    public ItemLedgerEntryType EntryType { get; init; }
    public string DocumentNo { get; init; }
    public string Description { get; init; }

    /// <summary>Unsigned, in the item's base unit of measure; the entry type and <see cref="Correction"/> decide the direction.</summary>
    public decimal Quantity { get; init; }

    /// <summary>The cost of one base unit coming in. Stock going out is costed from the entries it takes from.</summary>
    public decimal UnitCost { get; init; }

    /// <summary>Stock coming in without a cost of its own (a positive adjustment) is valued at the item's unit cost.</summary>
    public bool UseItemCost { get; init; }

    public decimal SalesAmount { get; init; }
    public Guid DimensionSetId { get; init; }
    public string LocationCode { get; init; }
    public string VariantCode { get; init; }

    /// <summary>The lots and serial numbers of the movement; null for an item that is not tracked.</summary>
    public IReadOnlyList<TrackingPart> Tracking { get; init; }

    /// <summary>The posted shipment or receipt line (or document line) the entries belong to.</summary>
    public Guid? DocumentLineId { get; init; }

    /// <summary>Turns the direction round: a sales return comes in, a purchase return goes out.</summary>
    public bool Correction { get; init; }

    /// <summary>Sets the direction outright, for entry types that move both ways (a transfer).</summary>
    public bool? Outbound { get; init; }

    /// <summary>Stock going out takes from this inbound entry only, e.g. when a receipt is undone.</summary>
    public Guid? AppliesToEntryId { get; init; }

    /// <summary>
    /// A shipment or receipt not invoiced yet: the value entry carries the cost as expected cost,
    /// and the invoice later turns it into actual cost through <see cref="ItemJnlPostLine.PostInvoicedCostAsync"/>.
    /// </summary>
    public bool ExpectedCost { get; init; }

    /// <summary>The bin at a location with bins; null takes the location's receipt, shipment or default bin.</summary>
    public string BinCode { get; init; }

    /// <summary>The document line the stock may have been reserved for: it can use its own reservation, and uses it up.</summary>
    public Guid? ReservationSourceId { get; init; }

    /// <summary>
    /// Goods that go straight from the vendor to the customer: they never enter a bin and are not
    /// held back by reservations.
    /// </summary>
    public bool DropShipment { get; init; }

    /// <summary>
    /// The caller posts the cost to the G/L itself, whatever the setup says (a project or production
    /// order). Left out, the cost counts as posted when automatic cost posting is on.
    /// </summary>
    public bool? CostPostedToGL { get; init; }
}

/// <summary>
/// Core Item Journal Posting Engine.
/// <para>
/// Creates Item Ledger Entries and Value Entries, applies outbound entries to the inbound entries
/// still in stock and keeps the item's quantity and unit cost up to date:
/// </para>
/// <list type="bullet">
/// <item>Stock coming in opens an inbound entry at its cost, and first covers any stock that went
/// out without being there (negative inventory) at the same location.</item>
/// <item>Stock going out takes from the open inbound entries of the item at the location, variant,
/// lot and serial number: the oldest first (FIFO), or the one with its serial number (specific
/// cost). FIFO and specific cost items leave at the cost of what they took; average cost items at
/// the item's running average.</item>
/// <item>A tracked item posts one entry per lot or serial number; a lot or serial number going out
/// has to be in stock, whatever the negative inventory rule says.</item>
/// </list>
/// </summary>
public class ItemJnlPostLine : DomainService
{
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemLedgerEntryRepository;
    private readonly IRepository<ValueEntry, Guid> _valueEntryRepository;
    private readonly InventorySetupManager _setupManager;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();
    private IRepository<ItemApplicationEntry, Guid> ApplicationRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<ItemApplicationEntry, Guid>>();
    private ItemTrackingManager TrackingManager => LazyServiceProvider.LazyGetRequiredService<ItemTrackingManager>();
    private ItemUnitOfMeasureManager UnitManager => LazyServiceProvider.LazyGetRequiredService<ItemUnitOfMeasureManager>();
    private WarehouseManager WarehouseManager => LazyServiceProvider.LazyGetRequiredService<WarehouseManager>();
    private ReservationManager ReservationManager => LazyServiceProvider.LazyGetRequiredService<ReservationManager>();

    public ItemJnlPostLine(
        IRepository<Item, Guid> itemRepository,
        IRepository<ItemLedgerEntry, Guid> itemLedgerEntryRepository,
        IRepository<ValueEntry, Guid> valueEntryRepository,
        InventorySetupManager setupManager
    )
    {
        _setupManager = setupManager;
        _itemRepository = itemRepository;
        _itemLedgerEntryRepository = itemLedgerEntryRepository;
        _valueEntryRepository = valueEntryRepository;
    }

    public async Task PostItemEntryAsync(
        Guid itemId,
        string itemNo,
        DateTime postingDate,
        ItemLedgerEntryType entryType,
        string documentNo,
        string description,
        decimal quantity,
        decimal unitCost,
        Guid dimensionSetId = default,
        string locationCode = null,
        bool correction = false
    )
    {
        await PostAllAsync(new ItemPostingRequest
        {
            ItemId = itemId,
            ItemNo = itemNo,
            PostingDate = postingDate,
            EntryType = entryType,
            DocumentNo = documentNo,
            Description = description,
            Quantity = quantity,
            UnitCost = unitCost,
            DimensionSetId = dimensionSetId,
            LocationCode = locationCode,
            Correction = correction,
        });
    }

    /// <summary>Posts one stock movement of an item that is not split by tracking and returns its entry.</summary>
    public async Task<ItemLedgerEntry> PostAsync(ItemPostingRequest request)
    {
        return (await PostAllAsync(request)).First();
    }

    /// <summary>Posts one stock movement and returns its entries, one per lot or serial number.</summary>
    public async Task<List<ItemLedgerEntry>> PostAllAsync(ItemPostingRequest request)
    {
        var item = await _itemRepository.GetAsync(request.ItemId);
        if (item.Blocked)
        {
            throw new UserFriendlyException($"Item '{request.ItemNo}' is blocked.");
        }

        var variantCode = CodeTableEntity.NormalizeCode(request.VariantCode);
        var locationCode = CodeTableEntity.NormalizeCode(request.LocationCode);
        await UnitManager.EnsureVariantAsync(item.No, variantCode);

        // Signed quantity based on entry type. A correction (a credit memo) moves the stock back:
        // a sales return comes in, a purchase return goes out.
        var outbound = request.EntryType is ItemLedgerEntryType.Sale or ItemLedgerEntryType.NegativeAdjmt or ItemLedgerEntryType.Consumption or ItemLedgerEntryType.AssemblyConsumption;
        if (request.Correction)
        {
            outbound = !outbound;
        }
        outbound = request.Outbound ?? outbound;
        var sign = outbound ? -1m : 1m;
        var quantity = Math.Abs(request.Quantity);

        var parts = await TrackingPartsAsync(item, request, quantity);
        var costPosted = request.CostPostedToGL ?? (request.EntryType is ItemLedgerEntryType.Purchase or ItemLedgerEntryType.Consumption or ItemLedgerEntryType.Output
            || (await _setupManager.GetAsync()).AutomaticCostPosting);
        await CheckRulesAsync(item, sign * quantity, locationCode, variantCode);
        if (outbound && !request.DropShipment && item.Type == ItemType.Inventory)
        {
            await ReservationManager.CheckOutboundAsync(item, locationCode, variantCode, quantity, request.ReservationSourceId);
        }

        var entries = new List<ItemLedgerEntry>();
        foreach (var part in parts)
        {
            var signedQty = sign * part.Quantity;
            var entry = new ItemLedgerEntry(
                GuidGenerator.Create(),
                item.Id,
                item.No,
                request.PostingDate,
                request.EntryType,
                request.DocumentNo,
                signedQty,
                0m,
                request.SalesAmount,
                locationCode,
                invoiced: !request.ExpectedCost
            )
            {
                EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ItemLedgerEntry),
            };
            entry.SetTracking(variantCode, part.LotNo, part.SerialNo, part.ExpirationDate);
            entry.SetDocumentLine(request.DocumentLineId);

            if (outbound)
            {
                var cost = await ApplyOutboundAsync(item, entry, part, request.AppliesToEntryId);
                entry.SetCost(part.Quantity == 0m ? 0m : Math.Round(cost / part.Quantity, 5, MidpointRounding.AwayFromZero), !request.ExpectedCost);
                await _itemLedgerEntryRepository.InsertAsync(entry, autoSave: true);
            }
            else
            {
                await EnsureSerialIsNewAsync(item, part);
                var unitCost = request.UseItemCost ? item.UnitCost : request.UnitCost;
                item.TakeInboundCost(signedQty, unitCost, request.EntryType == ItemLedgerEntryType.Purchase && !request.Correction);
                entry.SetCost(unitCost, !request.ExpectedCost);
                await _itemLedgerEntryRepository.InsertAsync(entry, autoSave: true);
                await ApplyInboundToNegativesAsync(item, entry);
            }

            // Value Entry for cost accounting; the cost follows the sign of the movement.
            var costAmount = RoundCost(signedQty * entry.UnitCost);
            await _valueEntryRepository.InsertAsync(new ValueEntry(
                GuidGenerator.Create(),
                entry.Id,
                item.Id,
                item.No,
                request.PostingDate,
                request.DocumentNo,
                signedQty,
                request.ExpectedCost ? 0m : costAmount,
                request.DimensionSetId,
                costAmountExpected: request.ExpectedCost ? costAmount : 0m,
                invoicedQuantity: request.ExpectedCost ? 0m : signedQty
            ) { EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ValueEntry) }
                .WithDetails(ValueEntryType.DirectCost, locationCode, variantCode)
                .PostedToGL(costPosted));

            item.AdjustInventory(signedQty);
            entries.Add(entry);

            if (!request.DropShipment && item.Type == ItemType.Inventory)
            {
                await WarehouseManager.RecordItemEntryAsync(entry, request.BinCode, request.Description);
            }
        }

        if (outbound && request.ReservationSourceId.HasValue)
        {
            await ReservationManager.ConsumeAsync(request.ReservationSourceId.Value, quantity);
        }

        await _itemRepository.UpdateAsync(item, autoSave: true);
        return entries;
    }

    /// <summary>
    /// Invoices part of a shipment or receipt: reverses its share of the expected cost and books the
    /// actual cost, which on a purchase is what the invoice says the goods cost. Both amounts are
    /// unsigned and take the sign of the entry. An average cost item takes a purchase price that
    /// differs from the receipt into its average.
    /// </summary>
    public async Task PostInvoicedCostAsync(
        ItemLedgerEntry entry,
        decimal quantity,
        decimal actualCostAmount,
        string documentNo,
        DateTime postingDate,
        Guid dimensionSetId = default
    )
    {
        var sign = Math.Sign(entry.Quantity);
        var signedQty = sign * Math.Abs(quantity);
        var expected = RoundCost(signedQty * entry.ExpectedUnitCost);
        var actual = sign * Math.Abs(actualCostAmount);

        await _valueEntryRepository.InsertAsync(new ValueEntry(
            GuidGenerator.Create(),
            entry.Id,
            entry.ItemId,
            entry.ItemNo,
            postingDate,
            documentNo,
            0m,
            actual,
            dimensionSetId,
            costAmountExpected: -expected,
            invoicedQuantity: signedQty
        ) { EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ValueEntry) }
            .WithDetails(ValueEntryType.DirectCost, entry.LocationCode, entry.VariantCode)
            .PostedToGL(entry.EntryType is ItemLedgerEntryType.Purchase || (await _setupManager.GetAsync()).AutomaticCostPosting));

        entry.AddInvoiced(signedQty, actual);
        await _itemLedgerEntryRepository.UpdateAsync(entry, autoSave: true);

        var difference = actual - expected;
        if (entry.Positive && difference != 0m)
        {
            var item = await _itemRepository.GetAsync(entry.ItemId);
            if (item.CostingMethod == CostingMethod.Average && item.Inventory > 0m)
            {
                item.Revalue(Math.Round(item.UnitCost + difference / item.Inventory, 5, MidpointRounding.AwayFromZero));
                await _itemRepository.UpdateAsync(item, autoSave: true);
            }
        }
    }

    /// <summary>
    /// Posts an item charge (landed cost such as freight, customs duty, or insurance) to an existing
    /// item ledger entry, creating a <see cref="ValueEntry"/> of type <see cref="ValueEntryType.ItemCharge"/>
    /// and increasing the entry's actual cost and the item's unit cost.
    /// </summary>
    public async Task<ValueEntry> PostLandedCostAsync(
        ItemLedgerEntry entry,
        decimal landedCostAmount,
        string documentNo,
        DateTime postingDate,
        Guid dimensionSetId = default
    )
    {
        var roundedCost = RoundCost(landedCostAmount);
        var valueEntry = new ValueEntry(
            GuidGenerator.Create(),
            entry.Id,
            entry.ItemId,
            entry.ItemNo,
            postingDate,
            documentNo,
            entry.Quantity,
            roundedCost,
            dimensionSetId,
            costAmountExpected: 0m,
            invoicedQuantity: 0m
        ) { EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ValueEntry) }
            .WithDetails(ValueEntryType.ItemCharge, entry.LocationCode, entry.VariantCode)
            .PostedToGL(true);

        await _valueEntryRepository.InsertAsync(valueEntry, autoSave: true);

        entry.AddLandedCost(roundedCost);
        await _itemLedgerEntryRepository.UpdateAsync(entry, autoSave: true);

        // Only the part of the receipt still in stock raises the item's cost; what has already left
        // was costed when it left (entries already applied are not re-costed).
        if (entry.Positive && roundedCost != 0m && entry.RemainingQuantity > 0m && entry.Quantity != 0m)
        {
            var item = await _itemRepository.GetAsync(entry.ItemId);
            var onHandShare = roundedCost * entry.RemainingQuantity / entry.Quantity;
            if (item.Inventory > 0m)
            {
                item.Revalue(Math.Round(item.UnitCost + onHandShare / item.Inventory, 5, MidpointRounding.AwayFromZero));
                await _itemRepository.UpdateAsync(item, autoSave: true);
            }
        }

        return valueEntry;
    }

    /// <summary>
    /// Revalues the stock of an item on hand at a location (all locations when blank) to
    /// <paramref name="newUnitCost"/>: a revaluation value entry per open inbound entry for the
    /// difference on what is left of it. Returns the total change in value.
    /// </summary>
    public async Task<decimal> RevalueAsync(Item item, string locationCode, string variantCode, decimal newUnitCost, DateTime postingDate, string documentNo)
    {
        var location = CodeTableEntity.NormalizeCode(locationCode);
        var variant = CodeTableEntity.NormalizeCode(variantCode);
        var open = await _itemLedgerEntryRepository.GetListAsync(e =>
            e.ItemId == item.Id && e.Positive && e.Open && e.RemainingQuantity > 0m
            && (location == null || e.LocationCode == location)
            && (variant == null || e.VariantCode == variant));

        var total = 0m;
        foreach (var entry in open)
        {
            var change = RoundCost(entry.RemainingQuantity * (newUnitCost - entry.UnitCost));
            if (change != 0m)
            {
                await _valueEntryRepository.InsertAsync(new ValueEntry(
                    GuidGenerator.Create(),
                    entry.Id,
                    item.Id,
                    item.No,
                    postingDate,
                    documentNo,
                    entry.RemainingQuantity,
                    change,
                    invoicedQuantity: 0m
                ) { EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ValueEntry) }
                    .WithDetails(ValueEntryType.Revaluation, entry.LocationCode, entry.VariantCode)
                    .PostedToGL((await _setupManager.GetAsync()).AutomaticCostPosting));
            }

            entry.Revalue(newUnitCost);
            await _itemLedgerEntryRepository.UpdateAsync(entry, autoSave: true);
            total += change;
        }

        if (item.CostingMethod == CostingMethod.Average || location == null)
        {
            item.Revalue(newUnitCost);
            await _itemRepository.UpdateAsync(item, autoSave: true);
        }

        return total;
    }

    /// <summary>The quantity of an item on hand at a location and variant, and of a lot or serial number when given.</summary>
    public async Task<decimal> GetAvailableAsync(Guid itemId, string locationCode, string variantCode, string lotNo = null, string serialNo = null)
    {
        var location = CodeTableEntity.NormalizeCode(locationCode);
        var variant = CodeTableEntity.NormalizeCode(variantCode);
        var entries = await _itemLedgerEntryRepository.GetListAsync(e =>
            e.ItemId == itemId && e.LocationCode == location && e.VariantCode == variant
            && (lotNo == null || e.LotNo == lotNo)
            && (serialNo == null || e.SerialNo == serialNo));
        return entries.Sum(e => e.Quantity);
    }

    /// <summary>Takes stock for an outbound entry from the open inbound entries and returns what it cost (unsigned).</summary>
    private async Task<decimal> ApplyOutboundAsync(Item item, ItemLedgerEntry outbound, TrackingPart part, Guid? appliesToEntryId)
    {
        if (item.CostingMethod == CostingMethod.Specific && part.SerialNo == null && item.Type == ItemType.Inventory)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.SerialRequiredForSpecificCost).WithData("itemNo", item.No);
        }

        var location = outbound.LocationCode;
        var variant = outbound.VariantCode;
        var lot = part.LotNo;
        var serial = part.SerialNo;
        var inbound = (await _itemLedgerEntryRepository.GetListAsync(e =>
                e.ItemId == item.Id && e.Positive && e.Open
                && (appliesToEntryId == null || e.Id == appliesToEntryId)
                && e.LocationCode == location && e.VariantCode == variant
                && (lot == null || e.LotNo == lot)
                && (serial == null || e.SerialNo == serial)))
            .Where(e => e.RemainingQuantity > 0m)
            .OrderBy(e => e.PostingDate)
            .ThenBy(e => e.EntryNo)
            .ToList();

        var remaining = part.Quantity;
        var cost = 0m;
        foreach (var from in inbound)
        {
            if (remaining == 0m)
            {
                break;
            }

            var take = Math.Min(remaining, from.RemainingQuantity);
            await LinkAsync(from, outbound, take);
            from.Apply(take);
            outbound.Apply(take);
            await _itemLedgerEntryRepository.UpdateAsync(from, autoSave: true);
            cost += take * from.UnitCost;
            remaining -= take;
        }

        if (remaining > 0m)
        {
            // A lot or serial number can only leave if it is there.
            if (part.TrackingNo != null)
            {
                throw new BusinessException(ErpErrorCodes.Inventory.InsufficientTrackedQuantity)
                    .WithData("itemNo", item.No)
                    .WithData("trackingNo", part.TrackingNo)
                    .WithData("locationCode", location ?? "")
                    .WithData("available", part.Quantity - remaining)
                    .WithData("quantity", part.Quantity);
            }

            // Negative inventory: the rest leaves at the item's cost and waits for a receipt.
            cost += remaining * item.UnitCost;
        }

        return item.CostingMethod == CostingMethod.Average ? part.Quantity * item.UnitCost : cost;
    }

    /// <summary>A receipt first covers stock that went out without being there.</summary>
    private async Task ApplyInboundToNegativesAsync(Item item, ItemLedgerEntry inbound)
    {
        var location = inbound.LocationCode;
        var variant = inbound.VariantCode;
        var negatives = (await _itemLedgerEntryRepository.GetListAsync(e =>
                e.ItemId == item.Id && !e.Positive && e.Open && e.LocationCode == location && e.VariantCode == variant))
            .Where(e => e.RemainingQuantity < 0m)
            .OrderBy(e => e.PostingDate)
            .ThenBy(e => e.EntryNo)
            .ToList();

        foreach (var outbound in negatives)
        {
            if (inbound.RemainingQuantity <= 0m)
            {
                break;
            }

            var take = Math.Min(inbound.RemainingQuantity, -outbound.RemainingQuantity);
            await LinkAsync(inbound, outbound, take);
            inbound.Apply(take);
            outbound.Apply(take);
            await _itemLedgerEntryRepository.UpdateAsync(outbound, autoSave: true);
        }

        await _itemLedgerEntryRepository.UpdateAsync(inbound, autoSave: true);
    }

    private async Task LinkAsync(ItemLedgerEntry inbound, ItemLedgerEntry outbound, decimal quantity)
    {
        await ApplicationRepository.InsertAsync(new ItemApplicationEntry(
            GuidGenerator.Create(),
            inbound.Id,
            outbound.Id,
            inbound.ItemNo,
            outbound.PostingDate,
            quantity
        ) { EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ItemApplicationEntry) });
    }

    /// <summary>The lots and serial numbers the movement is split into; one untracked part for an item without tracking.</summary>
    private async Task<IReadOnlyList<TrackingPart>> TrackingPartsAsync(Item item, ItemPostingRequest request, decimal quantity)
    {
        var code = await TrackingManager.GetCodeAsync(item);
        var tracked = code != null && (code.LotSpecific || code.SerialSpecific);
        var parts = request.Tracking;

        if (parts == null || parts.Count == 0)
        {
            if (tracked && item.Type == ItemType.Inventory)
            {
                throw new BusinessException(ErpErrorCodes.Inventory.TrackingRequired)
                    .WithData("itemNo", item.No)
                    .WithData("quantity", quantity);
            }

            return [new TrackingPart(null, null, quantity, null)];
        }

        var total = parts.Sum(p => p.Quantity);
        if (total != quantity)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.TrackingQuantityMismatch)
                .WithData("itemNo", item.No)
                .WithData("quantity", quantity)
                .WithData("tracked", total);
        }
        if (code?.SerialSpecific == true && parts.Any(p => p.Quantity != 1m || p.SerialNo == null))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.SerialQuantityNotOne).WithData("itemNo", item.No);
        }

        return parts;
    }

    /// <summary>A serial number cannot come in while the same one is still in stock.</summary>
    private async Task EnsureSerialIsNewAsync(Item item, TrackingPart part)
    {
        if (part.SerialNo == null)
        {
            return;
        }

        var serial = part.SerialNo;
        if (await _itemLedgerEntryRepository.AnyAsync(e => e.ItemId == item.Id && e.SerialNo == serial && e.Positive && e.Open && e.RemainingQuantity > 0m))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.DuplicateSerialNo)
                .WithData("itemNo", item.No)
                .WithData("serialNo", serial);
        }
    }

    private static decimal RoundCost(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The Inventory Setup's rules for stock items: a location if mandatory, and no negative stock at
    /// the location and variant if prevented.
    /// </summary>
    private async Task CheckRulesAsync(Item item, decimal signedQty, string locationCode, string variantCode)
    {
        if (item.Type != ItemType.Inventory)
        {
            return;
        }

        var setup = await _setupManager.GetAsync();
        if (setup.LocationMandatory && locationCode.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Inventory.LocationMandatory).WithData("itemNo", item.No);
        }

        if (setup.PreventNegativeInventory && signedQty < 0)
        {
            var available = await GetAvailableAsync(item.Id, locationCode, variantCode);
            if (available + signedQty < 0)
            {
                throw new BusinessException(ErpErrorCodes.Inventory.InsufficientInventory)
                    .WithData("itemNo", item.No)
                    .WithData("inventory", available)
                    .WithData("quantity", -signedQty);
            }
        }
    }
}
