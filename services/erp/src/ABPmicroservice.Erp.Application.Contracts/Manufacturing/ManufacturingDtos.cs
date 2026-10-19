using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Manufacturing;

public class ManufacturingSetupDto
{
    public string ProductionOrderNos { get; set; }
    public string WipAccountNo { get; set; }
    public string CapacityAppliedAccountNo { get; set; }
    public string VarianceAccountNo { get; set; }
}

public interface IManufacturingSetupAppService : IApplicationService
{
    Task<ManufacturingSetupDto> GetAsync();

    Task<ManufacturingSetupDto> UpdateAsync(ManufacturingSetupDto input);
}

public class WorkCenterDto : CodeTableDto
{
    public decimal UnitCostPerHour { get; set; }
    public decimal CapacityHoursPerDay { get; set; }
}

public class CreateUpdateWorkCenterDto : CreateUpdateCodeTableDto
{
    public decimal UnitCostPerHour { get; set; }
    public decimal CapacityHoursPerDay { get; set; } = 8m;
}

public class MachineCenterDto : CodeTableDto
{
    public string WorkCenterCode { get; set; }
    public decimal UnitCostPerHour { get; set; }
}

public class CreateUpdateMachineCenterDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string WorkCenterCode { get; set; }

    public decimal UnitCostPerHour { get; set; }
}

public interface IWorkCenterAppService : ICrudAppService<WorkCenterDto, Guid, GetCodeTableListInput, CreateUpdateWorkCenterDto> { }

public interface IMachineCenterAppService : ICrudAppService<MachineCenterDto, Guid, GetCodeTableListInput, CreateUpdateMachineCenterDto> { }

public class ProductionBomDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public ProductionBomStatus Status { get; set; }
}

public class CreateUpdateProductionBomDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public ProductionBomStatus Status { get; set; }
}

public class ProductionBomLineDto : EntityDto<Guid>
{
    public Guid ProductionBomId { get; set; }
    public int LineNo { get; set; }
    public ProductionBomLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal QuantityPer { get; set; }
    public decimal ScrapPct { get; set; }
}

public class CreateUpdateProductionBomLineDto
{
    public Guid ProductionBomId { get; set; }
    public ProductionBomLineType Type { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal QuantityPer { get; set; } = 1m;
    public decimal ScrapPct { get; set; }
}

public class GetProductionBomLineListInput : ErpPagedListInput
{
    public Guid? ProductionBomId { get; set; }
}

public interface IProductionBomAppService : ICrudAppService<ProductionBomDto, Guid, GetCodeTableListInput, CreateUpdateProductionBomDto> { }

public interface IProductionBomLineAppService : ICrudAppService<ProductionBomLineDto, Guid, GetProductionBomLineListInput, CreateUpdateProductionBomLineDto> { }

public class RoutingDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public ProductionBomStatus Status { get; set; }
}

public class CreateUpdateRoutingDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public ProductionBomStatus Status { get; set; }
}

public class RoutingLineDto : EntityDto<Guid>
{
    public Guid RoutingId { get; set; }
    public string OperationNo { get; set; }
    public RoutingLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal SetupTime { get; set; }
    public decimal RunTime { get; set; }
}

public class CreateUpdateRoutingLineDto
{
    public Guid RoutingId { get; set; }

    [Required]
    [StringLength(10)]
    public string OperationNo { get; set; }

    public RoutingLineType Type { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal SetupTime { get; set; }
    public decimal RunTime { get; set; }
}

public class GetRoutingLineListInput : ErpPagedListInput
{
    public Guid? RoutingId { get; set; }
}

public interface IRoutingAppService : ICrudAppService<RoutingDto, Guid, GetCodeTableListInput, CreateUpdateRoutingDto> { }

public interface IRoutingLineAppService : ICrudAppService<RoutingLineDto, Guid, GetRoutingLineListInput, CreateUpdateRoutingLineDto> { }

public class ProdOrderComponentDto : EntityDto<Guid>
{
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public decimal QuantityPer { get; set; }
    public decimal ScrapPct { get; set; }
    public decimal ExpectedQuantity { get; set; }
    public decimal ActConsumption { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal CostAmount { get; set; }
}

public class ProdOrderRoutingLineDto : EntityDto<Guid>
{
    public string OperationNo { get; set; }
    public RoutingLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal SetupTime { get; set; }
    public decimal RunTime { get; set; }
    public decimal UnitCostPerHour { get; set; }
    public decimal ExpectedMinutes { get; set; }
    public decimal ActualMinutes { get; set; }
    public decimal CostAmount { get; set; }
}

public class ProductionOrderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public ProductionOrderStatus Status { get; set; }
    public string SourceItemNo { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public DateTime DueDate { get; set; }
    public string LocationCode { get; set; }
    public decimal FinishedQuantity { get; set; }
    public DateTime? FinishedDate { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal CapacityCost { get; set; }
    public decimal OutputCost { get; set; }
    public decimal VarianceCost { get; set; }
    public decimal Wip { get; set; }
    public List<ProdOrderComponentDto> Components { get; set; } = new();
    public List<ProdOrderRoutingLineDto> RoutingLines { get; set; } = new();
}

public class CreateUpdateProductionOrderDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    public ProductionOrderStatus Status { get; set; } = ProductionOrderStatus.FirmPlanned;

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SourceItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Quantity { get; set; }
    public DateTime DueDate { get; set; }

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }
}

public class GetProductionOrderListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public ProductionOrderStatus? Status { get; set; }
}

public class ChangeProductionOrderStatusInput
{
    public ProductionOrderStatus Status { get; set; }
}

public class ConsumptionLineDto
{
    public Guid ComponentId { get; set; }
    public decimal Quantity { get; set; }
}

public class PostConsumptionInput
{
    public DateTime PostingDate { get; set; }

    /// <summary>Blank consumes everything still expected.</summary>
    public List<ConsumptionLineDto> Lines { get; set; } = new();
}

public class CapacityLineDto
{
    public string OperationNo { get; set; }
    public decimal Minutes { get; set; }
}

public class PostOutputInput
{
    public DateTime PostingDate { get; set; }
    public decimal Quantity { get; set; }

    /// <summary>Blank takes each operation's run time for the quantity.</summary>
    public List<CapacityLineDto> Capacity { get; set; } = new();
}

public class FinishProductionOrderInput
{
    public DateTime PostingDate { get; set; }
}

public interface IProductionOrderAppService : ICrudAppService<ProductionOrderDto, Guid, GetProductionOrderListInput, CreateUpdateProductionOrderDto>
{
    Task<ProductionOrderDto> RefreshAsync(Guid id);

    Task<ProductionOrderDto> ChangeStatusAsync(Guid id, ChangeProductionOrderStatusInput input);

    Task<ProductionOrderDto> ConsumeAsync(Guid id, PostConsumptionInput input);

    Task<ProductionOrderDto> ReportOutputAsync(Guid id, PostOutputInput input);

    Task<ProductionOrderDto> FinishAsync(Guid id, FinishProductionOrderInput input);
}

public class CapacityLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string ProductionOrderNo { get; set; }
    public string OperationNo { get; set; }
    public RoutingLineType Type { get; set; }
    public string No { get; set; }
    public string ItemNo { get; set; }
    public DateTime PostingDate { get; set; }
    public decimal OutputQuantity { get; set; }
    public decimal Minutes { get; set; }
    public decimal CostAmount { get; set; }
}

public class GetCapacityLedgerListInput : ErpPagedListInput
{
    public string ProductionOrderNo { get; set; }
}

public interface ICapacityLedgerEntryAppService : IReadOnlyAppService<CapacityLedgerEntryDto, Guid, GetCapacityLedgerListInput> { }

public class PlanningLineDto : EntityDto<Guid>
{
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public PlanningActionMessage ActionMessage { get; set; }
    public ReplenishmentSystem ReplenishmentSystem { get; set; }
    public string VendorNo { get; set; }
    public string LocationCode { get; set; }
    public decimal Quantity { get; set; }
    public DateTime DueDate { get; set; }
    public int LowLevelCode { get; set; }
    public bool AcceptActionMessage { get; set; }
    public string CarriedOutNo { get; set; }
}

public class UpdatePlanningLineDto
{
    public decimal Quantity { get; set; }
    public DateTime DueDate { get; set; }
    public bool AcceptActionMessage { get; set; }
    public string VendorNo { get; set; }
}

public class CalculatePlanInput
{
    public DateTime ToDate { get; set; }
}

public class CarryOutResultDto
{
    public List<string> PurchaseOrderNos { get; set; } = new();
    public List<string> ProductionOrderNos { get; set; } = new();
}

public interface IPlanningAppService : IApplicationService
{
    Task<PagedResultDto<PlanningLineDto>> GetListAsync(ErpPagedListInput input);

    Task<PlanningLineDto> GetAsync(Guid id);

    Task<PlanningLineDto> UpdateAsync(Guid id, UpdatePlanningLineDto input);

    Task DeleteAsync(Guid id);

    Task<int> CalculateAsync(CalculatePlanInput input);

    /// <summary>Makes purchase orders (one per vendor) and firm planned production orders of the accepted lines.</summary>
    Task<CarryOutResultDto> CarryOutAsync();
}
