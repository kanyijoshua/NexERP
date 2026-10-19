using System;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Inventory;

public class ItemDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public string BaseUnitOfMeasureCode { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public string ItemCategoryCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Inventory { get; set; }
    public string GenProdPostingGroup { get; set; }
    public string InventoryPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public bool Blocked { get; set; }
    public CostingMethod CostingMethod { get; set; }
    public string ItemTrackingCode { get; set; }
    public ABPmicroservice.Erp.Manufacturing.ReplenishmentSystem ReplenishmentSystem { get; set; }
    public string ProductionBomNo { get; set; }
    public string RoutingNo { get; set; }
    public ABPmicroservice.Erp.Manufacturing.ReorderPolicy ReorderPolicy { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public decimal MaximumInventory { get; set; }
    public decimal SafetyStock { get; set; }
    public string LeadTimeCalculation { get; set; }
    public decimal StandardCost { get; set; }
    public string VendorNo { get; set; }
    public decimal LastDirectCost { get; set; }
}

public class CreateUpdateItemDto
{
    public string No { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public string BaseUnitOfMeasureCode { get; set; }
    public Guid? ItemCategoryId { get; set; }
    public string ItemCategoryCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public string GenProdPostingGroup { get; set; }
    public string InventoryPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public CostingMethod CostingMethod { get; set; }
    public string ItemTrackingCode { get; set; }
    public ABPmicroservice.Erp.Manufacturing.ReplenishmentSystem ReplenishmentSystem { get; set; }
    public string ProductionBomNo { get; set; }
    public string RoutingNo { get; set; }
    public ABPmicroservice.Erp.Manufacturing.ReorderPolicy ReorderPolicy { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public decimal MaximumInventory { get; set; }
    public decimal SafetyStock { get; set; }
    public string LeadTimeCalculation { get; set; }
    public decimal StandardCost { get; set; }
    public string VendorNo { get; set; }
}

public class GetItemListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public ItemType? Type { get; set; }
    public Guid? ItemCategoryId { get; set; }
}

public class GetItemCategoryListInput : ErpPagedListInput
{
    /// <summary>Matches code or description, ignoring case.</summary>
    public string Filter { get; set; }
}

public class GetUnitOfMeasureListInput : ErpPagedListInput
{
    /// <summary>Matches code or description, ignoring case.</summary>
    public string Filter { get; set; }
}

public class ItemCategoryDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string ParentCategoryCode { get; set; }
}

public class CreateUpdateItemCategoryDto
{
    public string Code { get; set; }
    public string Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string ParentCategoryCode { get; set; }
}

public class UnitOfMeasureDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class CreateUpdateUnitOfMeasureDto
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class ItemLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public Guid ItemId { get; set; }
    public string ItemNo { get; set; }
    public DateTime PostingDate { get; set; }
    public ItemLedgerEntryType EntryType { get; set; }
    public string DocumentNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SalesAmount { get; set; }
    public string LocationCode { get; set; }
    public string VariantCode { get; set; }
    public string LotNo { get; set; }
    public string SerialNo { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal InvoicedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public bool Open { get; set; }
    public decimal CostAmountActual { get; set; }
}

public class GetItemLedgerEntryListInput : ErpPagedListInput
{
    public Guid? ItemId { get; set; }
    public string DocumentNo { get; set; }
    public ItemLedgerEntryType? EntryType { get; set; }
    public string LocationCode { get; set; }
    public string LotNo { get; set; }
    public bool? Open { get; set; }
}
