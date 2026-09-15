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
    public bool Blocked { get; set; }
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
}

public class GetItemListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
    public ItemType? Type { get; set; }
    public Guid? ItemCategoryId { get; set; }
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
}

public class GetItemLedgerEntryListInput : PagedAndSortedResultRequestDto
{
    public Guid? ItemId { get; set; }
    public string DocumentNo { get; set; }
    public ItemLedgerEntryType? EntryType { get; set; }
}
