using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Inventory;

// Bins and bin contents

public class BinDto : FullAuditedEntityDto<Guid>
{
    public string LocationCode { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateBinDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public bool Blocked { get; set; }
}

public class GetBinListInput : ErpPagedListInput
{
    public string LocationCode { get; set; }
    public string Filter { get; set; }
}

public class BinContentDto
{
    public string LocationCode { get; set; }
    public string BinCode { get; set; }
    public string ItemNo { get; set; }
    public string VariantCode { get; set; }
    public decimal Quantity { get; set; }
}

public class GetBinContentListInput : PagedResultRequestDto
{
    public string LocationCode { get; set; }
    public string BinCode { get; set; }
    public string ItemNo { get; set; }
}

public interface IBinAppService : ICrudAppService<BinDto, Guid, GetBinListInput, CreateUpdateBinDto>
{
    Task<PagedResultDto<BinContentDto>> GetContentsAsync(GetBinContentListInput input);
}

public class WarehouseEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public DateTime RegisteringDate { get; set; }
    public WarehouseEntryType EntryType { get; set; }
    public string LocationCode { get; set; }
    public string BinCode { get; set; }
    public string ItemNo { get; set; }
    public string VariantCode { get; set; }
    public decimal Quantity { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
}

public class GetWarehouseEntryListInput : ErpPagedListInput
{
    public string LocationCode { get; set; }
    public string BinCode { get; set; }
    public string ItemNo { get; set; }
}

public interface IWarehouseEntryAppService : IReadOnlyAppService<WarehouseEntryDto, Guid, GetWarehouseEntryListInput> { }

// Put-aways, picks and movements

public class WarehouseActivityDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public WarehouseActivityType Type { get; set; }
    public string LocationCode { get; set; }
    public WarehouseActivityStatus Status { get; set; }
    public WarehouseSourceType SourceType { get; set; }
    public string SourceNo { get; set; }
    public string Description { get; set; }
    public DateTime? RegisteredDate { get; set; }
}

/// <summary>A new warehouse document by hand is a movement; put-aways and picks are made from their source documents.</summary>
public class CreateUpdateWarehouseActivityDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GetWarehouseActivityListInput : ErpPagedListInput
{
    public WarehouseActivityType? Type { get; set; }
    public WarehouseActivityStatus? Status { get; set; }
    public string Filter { get; set; }
}

public class MakeWarehouseDocumentInput
{
    /// <summary>The posted receipt for a put-away, the sales order for a pick.</summary>
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string SourceNo { get; set; }
}

public class RegisterWarehouseActivityInput
{
    public DateTime? RegisteringDate { get; set; }
}

public interface IWarehouseActivityAppService : ICrudAppService<WarehouseActivityDto, Guid, GetWarehouseActivityListInput, CreateUpdateWarehouseActivityDto>
{
    Task<WarehouseActivityDto> MakePutAwayAsync(MakeWarehouseDocumentInput input);

    Task<WarehouseActivityDto> MakePickAsync(MakeWarehouseDocumentInput input);

    Task<WarehouseActivityDto> RegisterAsync(Guid id, RegisterWarehouseActivityInput input);
}

public class WarehouseActivityLineDto : EntityDto<Guid>
{
    public Guid WarehouseActivityHeaderId { get; set; }
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string VariantCode { get; set; }
    public string Description { get; set; }
    public string FromBinCode { get; set; }
    public string ToBinCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal QtyToHandle { get; set; }
    public decimal QtyHandled { get; set; }
}

public class CreateUpdateWarehouseActivityLineDto
{
    public Guid WarehouseActivityHeaderId { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string VariantCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string FromBinCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ToBinCode { get; set; }

    public decimal Quantity { get; set; }
    public decimal? QtyToHandle { get; set; }
}

public class GetWarehouseActivityLineListInput : ErpPagedListInput
{
    public Guid? WarehouseActivityHeaderId { get; set; }
}

public interface IWarehouseActivityLineAppService
    : ICrudAppService<WarehouseActivityLineDto, Guid, GetWarehouseActivityLineListInput, CreateUpdateWarehouseActivityLineDto> { }

// Reservations

public class ReservationEntryDto : FullAuditedEntityDto<Guid>
{
    public string ItemNo { get; set; }
    public string LocationCode { get; set; }
    public string VariantCode { get; set; }
    public decimal Quantity { get; set; }
    public ReservationSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public string SourceDocumentNo { get; set; }
    public int SourceLineNo { get; set; }
    public DateTime? ShipmentDate { get; set; }
}

public class GetReservationEntryListInput : ErpPagedListInput
{
    public string ItemNo { get; set; }
    public string SourceDocumentNo { get; set; }
    public Guid? SourceId { get; set; }
}

public class ReserveDocumentInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }
}

public class ReservationResultDto
{
    public int LineCount { get; set; }
}

public class ItemAvailabilityDto
{
    public string ItemNo { get; set; }
    public string LocationCode { get; set; }
    public string VariantCode { get; set; }
    public decimal Inventory { get; set; }
    public decimal Reserved { get; set; }
    public decimal Available { get; set; }
}

public class GetItemAvailabilityInput
{
    [Required]
    public string ItemNo { get; set; }

    public string LocationCode { get; set; }
    public string VariantCode { get; set; }
}

public interface IReservationEntryAppService : IReadOnlyAppService<ReservationEntryDto, Guid, GetReservationEntryListInput>
{
    Task<ReservationResultDto> ReserveSalesOrderAsync(ReserveDocumentInput input);

    Task<ReservationResultDto> ReserveAssemblyOrderAsync(ReserveDocumentInput input);

    Task CancelDocumentAsync(ReserveDocumentInput input);

    Task CancelAsync(Guid id);

    Task<ItemAvailabilityDto> GetAvailabilityAsync(GetItemAvailabilityInput input);
}

// Physical inventory orders

public class PhysInvtCountingPeriodDto : CodeTableDto
{
    public int CountFrequencyPerYear { get; set; }
}

public class CreateUpdatePhysInvtCountingPeriodDto : CreateUpdateCodeTableDto
{
    [Range(1, 365)]
    public int CountFrequencyPerYear { get; set; } = 1;
}

public interface IPhysInvtCountingPeriodAppService
    : ICrudAppService<PhysInvtCountingPeriodDto, Guid, GetCodeTableListInput, CreateUpdatePhysInvtCountingPeriodDto> { }

public class PhysInvtOrderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public string LocationCode { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime PostingDate { get; set; }
    public PhysInvtOrderStatus Status { get; set; }
    public int LineCount { get; set; }
    public int RecordedLineCount { get; set; }
}

public class CreateUpdatePhysInvtOrderDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    public DateTime? OrderDate { get; set; }
    public DateTime? PostingDate { get; set; }
}

public class GetPhysInvtOrderListInput : ErpPagedListInput
{
    public PhysInvtOrderStatus? Status { get; set; }
    public string Filter { get; set; }
}

public class CalculatePhysInvtLinesInput
{
    /// <summary>Only items whose next counting date has come (cycle counting).</summary>
    public bool OnlyItemsDueForCounting { get; set; }

    public string ItemCategoryCode { get; set; }

    /// <summary>Also a line, expecting nothing, for each matching item without stock.</summary>
    public bool IncludeItemsNotOnInventory { get; set; }
}

public class CalculatePhysInvtLinesResultDto
{
    public int LinesAdded { get; set; }
}

public interface IPhysInvtOrderAppService : ICrudAppService<PhysInvtOrderDto, Guid, GetPhysInvtOrderListInput, CreateUpdatePhysInvtOrderDto>
{
    Task<CalculatePhysInvtLinesResultDto> CalculateLinesAsync(Guid id, CalculatePhysInvtLinesInput input);

    Task<PhysInvtOrderDto> FinishAsync(Guid id);
}

public class PhysInvtOrderLineDto : EntityDto<Guid>
{
    public Guid PhysInvtOrderHeaderId { get; set; }
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public string VariantCode { get; set; }
    public string LocationCode { get; set; }
    public string BinCode { get; set; }
    public string LotNo { get; set; }
    public string SerialNo { get; set; }
    public decimal QtyExpected { get; set; }
    public decimal QtyRecorded { get; set; }
    public bool Recorded { get; set; }
    public decimal QtyDifference { get; set; }
}

public class CreateUpdatePhysInvtOrderLineDto
{
    public Guid PhysInvtOrderHeaderId { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string VariantCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BinCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string LotNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SerialNo { get; set; }

    public decimal QtyRecorded { get; set; }
    public bool Recorded { get; set; }
}

public class GetPhysInvtOrderLineListInput : ErpPagedListInput
{
    public Guid? PhysInvtOrderHeaderId { get; set; }
}

public interface IPhysInvtOrderLineAppService
    : ICrudAppService<PhysInvtOrderLineDto, Guid, GetPhysInvtOrderLineListInput, CreateUpdatePhysInvtOrderLineDto> { }

// Assembly

public class AssemblyBomComponentDto : FullAuditedEntityDto<Guid>
{
    public string ParentItemNo { get; set; }
    public int LineNo { get; set; }
    public string ComponentItemNo { get; set; }
    public string Description { get; set; }
    public decimal QuantityPer { get; set; }
}

public class CreateUpdateAssemblyBomComponentDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ParentItemNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ComponentItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal QuantityPer { get; set; } = 1m;
}

public class GetAssemblyBomComponentListInput : ErpPagedListInput
{
    public string ParentItemNo { get; set; }
    public string Filter { get; set; }
}

public interface IAssemblyBomComponentAppService
    : ICrudAppService<AssemblyBomComponentDto, Guid, GetAssemblyBomComponentListInput, CreateUpdateAssemblyBomComponentDto> { }

public class AssemblyOrderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public string VariantCode { get; set; }
    public string LocationCode { get; set; }
    public string BinCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityToAssemble { get; set; }
    public decimal AssembledQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime PostingDate { get; set; }
    public AssemblyOrderStatus Status { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreateUpdateAssemblyOrderDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string VariantCode { get; set; }

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BinCode { get; set; }

    public decimal Quantity { get; set; }
    public decimal? QuantityToAssemble { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PostingDate { get; set; }
}

public class GetAssemblyOrderListInput : ErpPagedListInput
{
    public AssemblyOrderStatus? Status { get; set; }
    public string Filter { get; set; }
}

public interface IAssemblyOrderAppService : ICrudAppService<AssemblyOrderDto, Guid, GetAssemblyOrderListInput, CreateUpdateAssemblyOrderDto>
{
    Task<AssemblyOrderDto> ReleaseAsync(Guid id);

    Task<AssemblyOrderDto> ReopenAsync(Guid id);

    Task<AssemblyOrderDto> RefreshLinesAsync(Guid id);

    Task<AssemblyOrderDto> RunPostingAsync(Guid id);
}

public class AssemblyLineDto : EntityDto<Guid>
{
    public Guid AssemblyHeaderId { get; set; }
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public string VariantCode { get; set; }
    public string LocationCode { get; set; }
    public decimal QuantityPer { get; set; }
    public decimal Quantity { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}

public class GetAssemblyLineListInput : ErpPagedListInput
{
    public Guid? AssemblyHeaderId { get; set; }
}

public interface IAssemblyLineAppService : IReadOnlyAppService<AssemblyLineDto, Guid, GetAssemblyLineListInput> { }
