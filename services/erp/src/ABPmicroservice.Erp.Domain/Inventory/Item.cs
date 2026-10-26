using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Item card.
/// </summary>
public class Item : CompanyAggregateRoot, IHasNo
{
    /// <summary>Business key.</summary>
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

    public string VatProdPostingGroup { get; private set; }

    public bool Blocked { get; private set; }

    public bool SalesBlocked { get; private set; }

    public bool PurchasingBlocked { get; private set; }

    public CostingMethod CostingMethod { get; private set; }

    /// <summary>How the item is tracked by lot or serial number; blank is not tracked.</summary>
    public string ItemTrackingCode { get; private set; }

    /// <summary>The cost per base unit of the last purchase.</summary>
    public decimal LastDirectCost { get; private set; }

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

    // Planning and manufacturing
    public Manufacturing.ReplenishmentSystem ReplenishmentSystem { get; private set; }
    public string ProductionBomNo { get; private set; }
    public string RoutingNo { get; private set; }
    public Manufacturing.ReorderPolicy ReorderPolicy { get; private set; }
    public decimal ReorderPoint { get; private set; }
    public decimal ReorderQuantity { get; private set; }
    public decimal MaximumInventory { get; private set; }
    public decimal SafetyStock { get; private set; }

    /// <summary>How long replenishing takes, as a date formula such as "5D".</summary>
    public string LeadTimeCalculation { get; private set; }

    /// <summary>What a unit should cost; output of a production order posts at it and leaves the rest as variance.</summary>
    public decimal StandardCost { get; private set; }

    /// <summary>Who planning buys the item from.</summary>
    public string VendorNo { get; private set; }

    public void SetPlanning(
        Manufacturing.ReplenishmentSystem replenishmentSystem,
        string productionBomNo,
        string routingNo,
        Manufacturing.ReorderPolicy reorderPolicy,
        decimal reorderPoint,
        decimal reorderQuantity,
        decimal maximumInventory,
        decimal safetyStock,
        string leadTimeCalculation,
        decimal standardCost,
        string vendorNo
    )
    {
        ReplenishmentSystem = replenishmentSystem;
        ProductionBomNo = Companies.CodeTableEntity.NormalizeCode(Check.Length(productionBomNo, nameof(productionBomNo), ErpDomainConsts.MaxNoLength));
        RoutingNo = Companies.CodeTableEntity.NormalizeCode(Check.Length(routingNo, nameof(routingNo), ErpDomainConsts.MaxNoLength));
        ReorderPolicy = reorderPolicy;
        ReorderPoint = Math.Max(0m, reorderPoint);
        ReorderQuantity = Math.Max(0m, reorderQuantity);
        MaximumInventory = Math.Max(0m, maximumInventory);
        SafetyStock = Math.Max(0m, safetyStock);
        LeadTimeCalculation = leadTimeCalculation.IsNullOrWhiteSpace() ? null : Check.Length(leadTimeCalculation.Trim().ToUpperInvariant(), nameof(leadTimeCalculation), ErpDomainConsts.MaxDateFormulaLength);
        StandardCost = Math.Max(0m, standardCost);
        VendorNo = Companies.CodeTableEntity.NormalizeCode(Check.Length(vendorNo, nameof(vendorNo), ErpDomainConsts.MaxNoLength));
    }

    public void SetCostingMethod(CostingMethod costingMethod) => CostingMethod = costingMethod;

    public void SetItemTrackingCode(string itemTrackingCode) =>
        ItemTrackingCode = CodeTableEntity.NormalizeCode(Check.Length(itemTrackingCode, nameof(itemTrackingCode), ErpDomainConsts.MaxCodeLength));

    /// <summary>
    /// Stock came in at <paramref name="unitCost"/>: an average cost item averages it in with what
    /// was on hand, any other item takes it as its unit cost. Called before the inventory moves.
    /// </summary>
    internal void TakeInboundCost(decimal quantity, decimal unitCost, bool purchase)
    {
        if (purchase)
        {
            LastDirectCost = unitCost;
        }

        if (CostingMethod == CostingMethod.Average && Inventory > 0m && Inventory + quantity > 0m)
        {
            UnitCost = Math.Round((Inventory * UnitCost + quantity * unitCost) / (Inventory + quantity), 5, MidpointRounding.AwayFromZero);
        }
        else
        {
            UnitCost = unitCost;
        }
    }

    /// <summary>A revaluation sets the cost of the stock on hand.</summary>
    internal void Revalue(decimal unitCost) => UnitCost = unitCost;

    /// <summary>Net weight of one base unit; item charges can be split by it.</summary>
    public decimal NetWeight { get; private set; }

    /// <summary>Volume of one base unit; item charges can be split by it.</summary>
    public decimal UnitVolume { get; private set; }

    public void SetMeasures(decimal netWeight, decimal unitVolume)
    {
        NetWeight = Math.Max(0m, netWeight);
        UnitVolume = Math.Max(0m, unitVolume);
    }

    public void SetVatProdPostingGroup(string vatProdPostingGroup) =>
        VatProdPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatProdPostingGroup, nameof(vatProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
}
