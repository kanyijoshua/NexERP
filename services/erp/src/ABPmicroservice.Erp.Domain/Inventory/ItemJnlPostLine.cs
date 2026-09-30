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
    private readonly InventorySetupManager _setupManager;

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
        var item = await _itemRepository.GetAsync(itemId);
        if (item.Blocked)
        {
            throw new UserFriendlyException($"Item '{itemNo}' is blocked.");
        }

        // Signed quantity based on entry type. A correction (a credit memo) moves the stock back:
        // a sales return comes in, a purchase return goes out.
        var outbound = entryType == ItemLedgerEntryType.Sale || entryType == ItemLedgerEntryType.NegativeAdjmt;
        if (correction)
        {
            outbound = !outbound;
        }

        decimal signedQty = outbound ? -Math.Abs(quantity) : Math.Abs(quantity);

        await CheckRulesAsync(item, signedQty, locationCode);

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
            locationCode
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

    /// <summary>The Inventory Setup's rules for stock items: a location if mandatory, and no negative stock if prevented.</summary>
    private async Task CheckRulesAsync(Item item, decimal signedQty, string locationCode)
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

        if (setup.PreventNegativeInventory && signedQty < 0 && item.Inventory + signedQty < 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.InsufficientInventory)
                .WithData("itemNo", item.No)
                .WithData("inventory", item.Inventory)
                .WithData("quantity", -signedQty);
        }
    }
}
