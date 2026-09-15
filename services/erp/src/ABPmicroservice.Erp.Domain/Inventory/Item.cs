using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item card. Mirrors Business Central table 27 "Item".
/// </summary>
public class Item : FullAuditedAggregateRoot<Guid>
{
    /// <summary>Business key. Mirrors BC field "No.".</summary>
    public string No { get; private set; }

    public string Description { get; private set; }

    public ItemType Type { get; private set; }

    public string BaseUnitOfMeasureCode { get; private set; }

    public Guid? ItemCategoryId { get; private set; }

    public string ItemCategoryCode { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal UnitCost { get; private set; }

    /// <summary>Quantity on hand (denormalized, maintained by item ledger entries).</summary>
    public decimal Inventory { get; internal set; }

    public string GenProdPostingGroup { get; private set; }

    public string InventoryPostingGroup { get; private set; }

    public bool Blocked { get; private set; }

    public bool SalesBlocked { get; private set; }

    public bool PurchasingBlocked { get; private set; }

    protected Item() { }

    public Item(
        Guid id,
        string no,
        string description,
        ItemType type,
        string baseUnitOfMeasureCode,
        decimal unitPrice = 0m,
        decimal unitCost = 0m,
        Guid? itemCategoryId = null,
        string itemCategoryCode = null
    )
        : base(id)
    {
        SetNo(no);
        SetDescription(description);
        Type = type;
        SetBaseUnitOfMeasureCode(baseUnitOfMeasureCode);
        UnitPrice = unitPrice;
        UnitCost = unitCost;
        ItemCategoryId = itemCategoryId;
        ItemCategoryCode = itemCategoryCode;
        Inventory = 0m;
    }

    public void SetNo(string no) =>
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);

    public void SetDescription(string description) =>
        Description = Check.NotNullOrWhiteSpace(
            description,
            nameof(description),
            ErpDomainConsts.MaxDescriptionLength
        );

    public void SetType(ItemType type) => Type = type;

    public void SetBaseUnitOfMeasureCode(string code) =>
        BaseUnitOfMeasureCode = Check.NotNullOrWhiteSpace(
            code,
            nameof(code),
            ErpDomainConsts.MaxUnitOfMeasureCodeLength
        );

    public void SetPrice(decimal unitPrice) => UnitPrice = unitPrice;

    public void SetCost(decimal unitCost) => UnitCost = unitCost;

    public void SetCategory(Guid? itemCategoryId, string itemCategoryCode)
    {
        ItemCategoryId = itemCategoryId;
        ItemCategoryCode = Check.Length(
            itemCategoryCode,
            nameof(itemCategoryCode),
            ErpDomainConsts.MaxCodeLength
        );
    }

    public void SetPostingGroups(string genProdPostingGroup, string inventoryPostingGroup)
    {
        GenProdPostingGroup = Check.Length(
            genProdPostingGroup,
            nameof(genProdPostingGroup),
            ErpDomainConsts.MaxPostingGroupLength
        );
        InventoryPostingGroup = Check.Length(
            inventoryPostingGroup,
            nameof(inventoryPostingGroup),
            ErpDomainConsts.MaxPostingGroupLength
        );
    }

    public void Block() => Blocked = true;

    public void Unblock() => Blocked = false;

    internal void AdjustInventory(decimal quantity) => Inventory += quantity;
}
