using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Inventory;

public class ItemChargeDto : CodeTableDto
{
    public string GenProdPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
}

public class CreateUpdateItemChargeDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string GenProdPostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string VatProdPostingGroup { get; set; }
}

public class ItemChargeAssignmentDto : FullAuditedEntityDto<Guid>
{
    public ItemChargeSourceType SourceType { get; set; }
    public string SourceDocumentNo { get; set; }
    public Guid SourceLineId { get; set; }
    public int SourceLineNo { get; set; }
    public string ItemChargeNo { get; set; }

    public Guid AppliesToEntryId { get; set; }
    public long AppliesToEntryNo { get; set; }
    public string AppliesToDocumentNo { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }

    public decimal QtyToAssign { get; set; }
    public decimal QtyAssigned { get; set; }
    public decimal UnitCost { get; set; }
    public decimal AmountToAssign { get; set; }
    public bool Posted { get; set; }
    public decimal AmountPostedLcy { get; set; }
}

/// <summary>Assigns part of a charge line to one entry; everything else is read from the line and the entry.</summary>
public class CreateItemChargeAssignmentDto
{
    public ItemChargeSourceType SourceType { get; set; }

    [Required]
    public Guid SourceLineId { get; set; }

    [Required]
    public Guid AppliesToEntryId { get; set; }

    public decimal QtyToAssign { get; set; }

    /// <summary>Blank means quantity times the line's unit cost (or price).</summary>
    public decimal? AmountToAssign { get; set; }
}

public class UpdateItemChargeAssignmentDto
{
    public decimal QtyToAssign { get; set; }
    public decimal? AmountToAssign { get; set; }
}

public class GetAssignableEntriesInput
{
    public ItemChargeSourceType SourceType { get; set; }

    [Required]
    public Guid SourceLineId { get; set; }

    public string Filter { get; set; }

    public int MaxResultCount { get; set; } = 200;
}

public class GetItemChargeAssignmentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public ItemChargeSourceType? SourceType { get; set; }
    public Guid? SourceLineId { get; set; }
    public string SourceDocumentNo { get; set; }
    public bool? Posted { get; set; }
}

public class SuggestItemChargeAllocationInput
{
    public ItemChargeSourceType SourceType { get; set; }
    public Guid SourceLineId { get; set; }
    public ItemChargeAllocationMethod Method { get; set; } = ItemChargeAllocationMethod.Equally;
    public List<Guid> TargetItemLedgerEntryIds { get; set; } = [];
}

public interface IItemChargeAppService
    : ICrudAppService<
        ItemChargeDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateItemChargeDto,
        CreateUpdateItemChargeDto
    >
{
}

public interface IItemChargeAssignmentAppService
    : ICrudAppService<
        ItemChargeAssignmentDto,
        Guid,
        GetItemChargeAssignmentListInput,
        CreateItemChargeAssignmentDto,
        UpdateItemChargeAssignmentDto
    >
{
    Task<List<ItemChargeAssignmentDto>> SuggestAllocationAsync(SuggestItemChargeAllocationInput input);

    Task<List<ItemLedgerEntryDto>> GetAssignableEntriesAsync(GetAssignableEntriesInput input);
}
