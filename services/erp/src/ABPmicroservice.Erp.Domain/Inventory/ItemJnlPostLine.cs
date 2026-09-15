using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Core Item Journal Posting Engine.
/// Mirrors Business Central Codeunit 22 "Item Jnl.-Post Line".
/// Creates Item Ledger Entries & Value Entries and updates Item inventory stock and unit costs.
/// </summary>
public class ItemJnlPostLine : DomainService
{
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemLedgerEntryRepository;
    private readonly IRepository<ValueEntry, Guid> _valueEntryRepository;

    public ItemJnlPostLine(
        IRepository<Item, Guid> itemRepository,
        IRepository<ItemLedgerEntry, Guid> itemLedgerEntryRepository,
        IRepository<ValueEntry, Guid> valueEntryRepository
    )
    {
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
        Guid dimensionSetId = default
    )
    {
        var item = await _itemRepository.GetAsync(itemId);
        if (item.Blocked)
        {
            throw new UserFriendlyException($"Item '{itemNo}' is blocked.");
        }

        // Signed quantity based on entry type
        decimal signedQty = (entryType == ItemLedgerEntryType.Sale || entryType == ItemLedgerEntryType.NegativeAdjmt)
            ? -Math.Abs(quantity)
            : Math.Abs(quantity);

        var itemLedgerEntry = new ItemLedgerEntry(
            GuidGenerator.Create(),
            item.Id,
            item.No,
            postingDate,
            entryType,
            documentNo,
            signedQty,
            unitCost,
            0m,
            null
        );

        await _itemLedgerEntryRepository.InsertAsync(itemLedgerEntry);

        // Value Entry for cost accounting
        decimal costAmount = Math.Abs(signedQty) * unitCost;
        var valueEntry = new ValueEntry(
            GuidGenerator.Create(),
            itemLedgerEntry.Id,
            item.Id,
            item.No,
            postingDate,
            documentNo,
            signedQty,
            costAmount,
            dimensionSetId
        );
        await _valueEntryRepository.InsertAsync(valueEntry);

        // Update item stock & cost
        item.AdjustInventory(signedQty);
        await _itemRepository.UpdateAsync(item);
    }
}
