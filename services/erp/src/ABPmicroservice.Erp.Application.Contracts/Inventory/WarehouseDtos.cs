using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Inventory;

public class ItemVariantDto : FullAuditedEntityDto<Guid>
{
    public string ItemNo { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateItemVariantDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public bool Blocked { get; set; }
}

public class ItemUnitOfMeasureDto : FullAuditedEntityDto<Guid>
{
    public string ItemNo { get; set; }
    public string Code { get; set; }
    public decimal QtyPerUnitOfMeasure { get; set; }
}

public class CreateUpdateItemUnitOfMeasureDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string Code { get; set; }

    [Range(0.00001, double.MaxValue)]
    public decimal QtyPerUnitOfMeasure { get; set; } = 1m;
}

/// <summary>Lists the rows of one item.</summary>
public class GetItemChildListInput : ErpPagedListInput
{
    public string ItemNo { get; set; }
    public string Filter { get; set; }
}

public class ItemTrackingCodeDto : CodeTableDto
{
    public bool LotSpecific { get; set; }
    public bool SerialSpecific { get; set; }
    public bool ExpirationDateRequired { get; set; }
}

public class CreateUpdateItemTrackingCodeDto : CreateUpdateCodeTableDto
{
    public bool LotSpecific { get; set; }
    public bool SerialSpecific { get; set; }
    public bool ExpirationDateRequired { get; set; }
}

public class LotNoInformationDto : FullAuditedEntityDto<Guid>
{
    public string ItemNo { get; set; }
    public string VariantCode { get; set; }
    public string LotNo { get; set; }
    public string Description { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateLotNoInformationDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string VariantCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxLotNoLength)]
    public string LotNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public DateTime? ExpirationDate { get; set; }
    public bool Blocked { get; set; }
}

public class SerialNoInformationDto : FullAuditedEntityDto<Guid>
{
    public string ItemNo { get; set; }
    public string VariantCode { get; set; }
    public string SerialNo { get; set; }
    public string Description { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateSerialNoInformationDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string VariantCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxLotNoLength)]
    public string SerialNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public bool Blocked { get; set; }
}

public class ItemTrackingLineDto : FullAuditedEntityDto<Guid>
{
    public ItemTrackingSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public string ItemNo { get; set; }
    public string LotNo { get; set; }
    public string SerialNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityHandled { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class CreateUpdateItemTrackingLineDto
{
    public ItemTrackingSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxLotNoLength)]
    public string LotNo { get; set; }

    [StringLength(ErpDomainConsts.MaxLotNoLength)]
    public string SerialNo { get; set; }

    public decimal Quantity { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class GetItemTrackingLineListInput : ErpPagedListInput
{
    public ItemTrackingSourceType? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string ItemNo { get; set; }
}

public class ItemJournalTemplateDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemJournalTemplateType Type { get; set; }
}

public class CreateUpdateItemJournalTemplateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public ItemJournalTemplateType Type { get; set; }
}

public class ItemJournalBatchDto : FullAuditedEntityDto<Guid>
{
    public string JournalTemplateName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>The kind of journal, from the template.</summary>
    public ItemJournalTemplateType TemplateType { get; set; }
}

public class CreateUpdateItemJournalBatchDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string JournalTemplateName { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GetItemJournalBatchListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public ItemJournalTemplateType? TemplateType { get; set; }
}

public class ItemJournalLineDto : FullAuditedEntityDto<Guid>
{
    public Guid ItemJournalBatchId { get; set; }
    public int LineNo { get; set; }
    public DateTime PostingDate { get; set; }
    public ItemLedgerEntryType EntryType { get; set; }
    public string DocumentNo { get; set; }
    public Guid ItemId { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public string LocationCode { get; set; }
    public string NewLocationCode { get; set; }
    public string BinCode { get; set; }
    public string VariantCode { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Amount { get; set; }
    public string LotNo { get; set; }
    public string SerialNo { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal QtyCalculated { get; set; }
    public decimal QtyPhysInventory { get; set; }
    public decimal UnitCostCalculated { get; set; }
    public decimal UnitCostRevalued { get; set; }
}

public class CreateUpdateItemJournalLineDto
{
    public Guid ItemJournalBatchId { get; set; }
    public DateTime PostingDate { get; set; }
    public ItemLedgerEntryType EntryType { get; set; } = ItemLedgerEntryType.PositiveAdjmt;

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string NewLocationCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BinCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string VariantCode { get; set; }

    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string UnitOfMeasureCode { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }

    [StringLength(ErpDomainConsts.MaxLotNoLength)]
    public string LotNo { get; set; }

    [StringLength(ErpDomainConsts.MaxLotNoLength)]
    public string SerialNo { get; set; }

    public DateTime? ExpirationDate { get; set; }

    /// <summary>Physical inventory: the counted quantity.</summary>
    public decimal QtyPhysInventory { get; set; }

    /// <summary>Revaluation: the new unit cost.</summary>
    public decimal UnitCostRevalued { get; set; }
}

public class GetItemJournalLineListInput : ErpPagedListInput
{
    public Guid? ItemJournalBatchId { get; set; }
    public string Filter { get; set; }
}

public class CalculateInventoryInput
{
    public DateTime PostingDate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }
}

public class ItemJournalPostingResultDto
{
    public int LineCount { get; set; }
    public long? RegisterNo { get; set; }
}

public class TransferHeaderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string FromLocationCode { get; set; }
    public string ToLocationCode { get; set; }
    public string InTransitCode { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime? ShipmentDate { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public TransferStatus Status { get; set; }
    public bool Completed { get; set; }
    public string LastShipmentNo { get; set; }
    public string LastReceiptNo { get; set; }
}

public class CreateUpdateTransferHeaderDto
{
    /// <summary>Blank takes the next number of the transfer order series.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string FromLocationCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string ToLocationCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string InTransitCode { get; set; }

    public DateTime PostingDate { get; set; }
    public DateTime? ShipmentDate { get; set; }
    public DateTime? ReceiptDate { get; set; }
}

public class GetTransferListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public bool? Completed { get; set; }
}

public class TransferLineDto : EntityDto<Guid>
{
    public Guid TransferHeaderId { get; set; }
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string VariantCode { get; set; }
    public string Description { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal QtyToShip { get; set; }
    public decimal QuantityShipped { get; set; }
    public decimal QtyToReceive { get; set; }
    public decimal QuantityReceived { get; set; }
}

public class CreateUpdateTransferLineDto
{
    public Guid TransferHeaderId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string VariantCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string UnitOfMeasureCode { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Blank ships everything outstanding.</summary>
    public decimal? QtyToShip { get; set; }

    /// <summary>Blank receives everything in transit.</summary>
    public decimal? QtyToReceive { get; set; }
}

public class GetTransferLineListInput : ErpPagedListInput
{
    public Guid? TransferHeaderId { get; set; }
}

public class PostedTransferDto : EntityDto<Guid>
{
    public string No { get; set; }
    public bool Receipt { get; set; }
    public string TransferOrderNo { get; set; }
    public string FromLocationCode { get; set; }
    public string ToLocationCode { get; set; }
    public string InTransitCode { get; set; }
    public DateTime PostingDate { get; set; }
    public List<PostedTransferLineDto> Lines { get; set; } = new();
}

public class PostedTransferLineDto : EntityDto<Guid>
{
    public Guid PostedTransferId { get; set; }
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string VariantCode { get; set; }
    public string Description { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal Quantity { get; set; }
}

public class GetPostedTransferListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public bool? Receipt { get; set; }
}

public interface IItemVariantAppService
    : ICrudAppService<ItemVariantDto, Guid, GetItemChildListInput, CreateUpdateItemVariantDto, CreateUpdateItemVariantDto> { }

public interface IItemUnitOfMeasureAppService
    : ICrudAppService<ItemUnitOfMeasureDto, Guid, GetItemChildListInput, CreateUpdateItemUnitOfMeasureDto, CreateUpdateItemUnitOfMeasureDto> { }

public interface IItemTrackingCodeAppService
    : ICrudAppService<ItemTrackingCodeDto, Guid, GetCodeTableListInput, CreateUpdateItemTrackingCodeDto, CreateUpdateItemTrackingCodeDto> { }

public interface ILotNoInformationAppService
    : ICrudAppService<LotNoInformationDto, Guid, GetItemChildListInput, CreateUpdateLotNoInformationDto, CreateUpdateLotNoInformationDto> { }

public interface ISerialNoInformationAppService
    : ICrudAppService<SerialNoInformationDto, Guid, GetItemChildListInput, CreateUpdateSerialNoInformationDto, CreateUpdateSerialNoInformationDto> { }

public interface IItemTrackingLineAppService
    : ICrudAppService<ItemTrackingLineDto, Guid, GetItemTrackingLineListInput, CreateUpdateItemTrackingLineDto, CreateUpdateItemTrackingLineDto> { }

public interface IItemJournalTemplateAppService
    : ICrudAppService<ItemJournalTemplateDto, Guid, GetCodeTableListInput, CreateUpdateItemJournalTemplateDto, CreateUpdateItemJournalTemplateDto> { }

public interface IItemJournalBatchAppService
    : ICrudAppService<ItemJournalBatchDto, Guid, GetItemJournalBatchListInput, CreateUpdateItemJournalBatchDto, CreateUpdateItemJournalBatchDto>
{
    /// <summary>Posts the batch's lines. Routed as POST .../{id}/run-posting.</summary>
    Task<ItemJournalPostingResultDto> RunPostingAsync(Guid id);

    /// <summary>Physical inventory: a line per item, location and lot with stock. Routed as POST .../{id}/calculate-inventory.</summary>
    Task<int> CalculateInventoryAsync(Guid id, CalculateInventoryInput input);

    /// <summary>Revaluation: a line per item and location with its cost now. Routed as POST .../{id}/calculate-inventory-value.</summary>
    Task<int> CalculateInventoryValueAsync(Guid id, CalculateInventoryInput input);
}

public interface IItemJournalLineAppService
    : ICrudAppService<ItemJournalLineDto, Guid, GetItemJournalLineListInput, CreateUpdateItemJournalLineDto, CreateUpdateItemJournalLineDto> { }

public interface ITransferOrderAppService
    : ICrudAppService<TransferHeaderDto, Guid, GetTransferListInput, CreateUpdateTransferHeaderDto, CreateUpdateTransferHeaderDto>
{
    Task<TransferHeaderDto> ReleaseAsync(Guid id);

    Task<TransferHeaderDto> ReopenAsync(Guid id);

    /// <summary>Ships the quantities to ship into transit. Routed as POST .../{id}/ship.</summary>
    Task<TransferHeaderDto> ShipAsync(Guid id);

    /// <summary>Receives the quantities to receive out of transit. Routed as POST .../{id}/receive.</summary>
    Task<TransferHeaderDto> ReceiveAsync(Guid id);
}

public interface ITransferLineAppService
    : ICrudAppService<TransferLineDto, Guid, GetTransferLineListInput, CreateUpdateTransferLineDto, CreateUpdateTransferLineDto> { }

public interface IPostedTransferAppService
    : IReadOnlyAppService<PostedTransferDto, PostedTransferDto, Guid, GetPostedTransferListInput> { }
