using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Domain service for Item invariants (uniqueness, delete rules).
/// </summary>
public class ItemManager : DomainService
{
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemLedgerEntryRepository;

    public ItemManager(
        IRepository<Item, Guid> itemRepository,
        IRepository<ItemLedgerEntry, Guid> itemLedgerEntryRepository
    )
    {
        _itemRepository = itemRepository;
        _itemLedgerEntryRepository = itemLedgerEntryRepository;
    }

    public async Task<Item> CreateAsync(
        string no,
        string description,
        ItemType type,
        string baseUnitOfMeasureCode,
        decimal unitPrice = 0m,
        decimal unitCost = 0m,
        Guid? itemCategoryId = null,
        string itemCategoryCode = null
    )
    {
        await EnsureNoIsUniqueAsync(no);

        return new Item(
            GuidGenerator.Create(),
            no,
            description,
            type,
            baseUnitOfMeasureCode,
            unitPrice,
            unitCost,
            itemCategoryId,
            itemCategoryCode
        );
    }

    public async Task EnsureNoIsUniqueAsync(string no, Guid? excludeId = null)
    {
        var existing = await _itemRepository.FirstOrDefaultAsync(x => x.No == no);
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(ErpErrorCodes.Items.ItemAlreadyExists).WithData("no", no);
        }
    }

    public async Task EnsureCanDeleteAsync(Item item)
    {
        var hasEntries = await _itemLedgerEntryRepository.AnyAsync(x => x.ItemId == item.Id);
        if (hasEntries)
        {
            throw new BusinessException(ErpErrorCodes.Items.CannotDeleteItemWithLedgerEntries)
                .WithData("no", item.No);
        }
    }
}
