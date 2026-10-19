using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.ServiceManagement;

public class ServiceMgtSetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ServiceOrderNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ServiceItemNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ServiceContractNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ServiceRevenueAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ContractRevenueAccountNo { get; set; }

    [Range(0, 120)]
    public int DefaultWarrantyMonths { get; set; }
}

public interface IServiceMgtSetupAppService : IApplicationService
{
    Task<ServiceMgtSetupDto> GetAsync();

    Task<ServiceMgtSetupDto> UpdateAsync(ServiceMgtSetupDto input);
}

public class ServiceOrderTypeDto : CodeTableDto { }

public class CreateUpdateServiceOrderTypeDto : CreateUpdateCodeTableDto { }

public class FaultCodeDto : CodeTableDto { }

public class CreateUpdateFaultCodeDto : CreateUpdateCodeTableDto { }

public class ResolutionCodeDto : CodeTableDto { }

public class CreateUpdateResolutionCodeDto : CreateUpdateCodeTableDto { }

public interface IServiceOrderTypeAppService : ICrudAppService<ServiceOrderTypeDto, Guid, GetCodeTableListInput, CreateUpdateServiceOrderTypeDto> { }

public interface IFaultCodeAppService : ICrudAppService<FaultCodeDto, Guid, GetCodeTableListInput, CreateUpdateFaultCodeDto> { }

public interface IResolutionCodeAppService : ICrudAppService<ResolutionCodeDto, Guid, GetCodeTableListInput, CreateUpdateResolutionCodeDto> { }

// Service items

public class ServiceItemDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public string ItemNo { get; set; }
    public string SerialNo { get; set; }
    public DateTime? InstallationDate { get; set; }
    public DateTime? WarrantyStartingDate { get; set; }
    public DateTime? WarrantyEndingDate { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateServiceItemDto
{
    /// <summary>Blank takes the next number of the service item series.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SerialNo { get; set; }

    public DateTime? InstallationDate { get; set; }

    /// <summary>The installation date when blank; the end defaults to the setup's warranty months after it.</summary>
    public DateTime? WarrantyStartingDate { get; set; }

    public DateTime? WarrantyEndingDate { get; set; }
    public bool Blocked { get; set; }
}

public interface IServiceItemAppService : ICrudAppService<ServiceItemDto, Guid, GetCodeTableListInput, CreateUpdateServiceItemDto> { }

// Contracts

public class ServiceContractDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public ServiceContractStatus Status { get; set; }
    public DateTime StartingDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public ServiceInvoicePeriod InvoicePeriod { get; set; }
    public DateTime? NextInvoiceDate { get; set; }
    public DateTime? LastInvoiceDate { get; set; }
    public string LastInvoiceNo { get; set; }
    public decimal AnnualAmount { get; set; }
    public decimal PeriodAmount { get; set; }
}

public class CreateUpdateServiceContractDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    public DateTime StartingDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public ServiceInvoicePeriod InvoicePeriod { get; set; } = ServiceInvoicePeriod.Month;
}

public class ServiceContractLineDto : EntityDto<Guid>
{
    public Guid ServiceContractId { get; set; }
    public int LineNo { get; set; }
    public string ServiceItemNo { get; set; }
    public string Description { get; set; }
    public decimal LineValue { get; set; }
}

public class CreateUpdateServiceContractLineDto
{
    public Guid ServiceContractId { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ServiceItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal LineValue { get; set; }
}

public class GetServiceContractLineListInput : ErpPagedListInput
{
    public Guid? ServiceContractId { get; set; }
}

public class InvoiceServiceContractsInput
{
    /// <summary>Every period that has started by this date is invoiced; today when blank.</summary>
    public DateTime AsOfDate { get; set; }
}

public class ServiceContractInvoicingResultDto
{
    public int InvoicedPeriods { get; set; }
}

public interface IServiceContractAppService : ICrudAppService<ServiceContractDto, Guid, GetCodeTableListInput, CreateUpdateServiceContractDto>
{
    Task<ServiceContractDto> SignAsync(Guid id);

    Task<ServiceContractDto> CancelAsync(Guid id);

    /// <summary>Invoices the periods of one contract that have started.</summary>
    Task<ServiceContractInvoicingResultDto> InvoiceDueAsync(Guid id, InvoiceServiceContractsInput input);

    /// <summary>Invoices the started periods of every signed contract.</summary>
    Task<ServiceContractInvoicingResultDto> InvoiceAllDueAsync(InvoiceServiceContractsInput input);
}

public interface IServiceContractLineAppService : ICrudAppService<ServiceContractLineDto, Guid, GetServiceContractLineListInput, CreateUpdateServiceContractLineDto> { }

// Orders

public class ServiceItemLineDto : EntityDto<Guid>
{
    public Guid ServiceOrderId { get; set; }
    public int LineNo { get; set; }
    public string ServiceItemNo { get; set; }
    public string Description { get; set; }
    public string ItemNo { get; set; }
    public string SerialNo { get; set; }
    public string FaultCode { get; set; }
    public string ResolutionCode { get; set; }
    public string FaultComment { get; set; }
    public bool Warranty { get; set; }
}

public class CreateUpdateServiceItemLineDto
{
    public Guid ServiceOrderId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ServiceItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string FaultCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ResolutionCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string FaultComment { get; set; }

    /// <summary>Taken from the service item's warranty on the order date when blank.</summary>
    public bool? Warranty { get; set; }
}

public class ServiceLineDto : EntityDto<Guid>
{
    public Guid ServiceOrderId { get; set; }
    public int LineNo { get; set; }
    public string ServiceItemNo { get; set; }
    public ServiceLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public string LocationCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineDiscountPct { get; set; }
    public decimal LineAmount { get; set; }
    public bool Warranty { get; set; }
    public decimal QuantityPosted { get; set; }
}

public class CreateUpdateServiceLineDto
{
    public Guid ServiceOrderId { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ServiceItemNo { get; set; }

    public ServiceLineType Type { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    public decimal Quantity { get; set; } = 1m;

    /// <summary>The item's or resource's price when blank.</summary>
    public decimal? UnitPrice { get; set; }

    public decimal? UnitCost { get; set; }
    public decimal LineDiscountPct { get; set; }

    /// <summary>Taken from the service item line of the same service item when blank.</summary>
    public bool? Warranty { get; set; }
}

public class GetServiceOrderLineListInput : ErpPagedListInput
{
    public Guid? ServiceOrderId { get; set; }
}

public class ServiceOrderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public string ServiceOrderType { get; set; }
    public ServiceOrderStatus Status { get; set; }
    public ServicePriority Priority { get; set; }
    public DateTime OrderDate { get; set; }
    public string ResourceNo { get; set; }
    public string ContractNo { get; set; }
    public DateTime? FinishingDate { get; set; }
    public string LastInvoiceNo { get; set; }
    public decimal Amount { get; set; }
    public decimal WarrantyAmount { get; set; }
    public List<ServiceItemLineDto> ItemLines { get; set; } = new();
    public List<ServiceLineDto> Lines { get; set; } = new();
}

public class CreateUpdateServiceOrderDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ServiceOrderType { get; set; }

    public ServicePriority Priority { get; set; } = ServicePriority.Medium;
    public DateTime OrderDate { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ResourceNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ContractNo { get; set; }
}

public class GetServiceOrderListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public ServiceOrderStatus? Status { get; set; }
    public string ResourceNo { get; set; }
}

public class ChangeServiceOrderStatusInput
{
    public ServiceOrderStatus Status { get; set; }
}

public class ServicePostingInput
{
    public DateTime PostingDate { get; set; }
}

public class ServicePostingResultDto
{
    public string InvoiceNo { get; set; }
    public int LineCount { get; set; }
    public decimal Amount { get; set; }
    public decimal WarrantyAmount { get; set; }
}

public interface IServiceOrderAppService : ICrudAppService<ServiceOrderDto, Guid, GetServiceOrderListInput, CreateUpdateServiceOrderDto>
{
    Task<ServiceOrderDto> ChangeStatusAsync(Guid id, ChangeServiceOrderStatusInput input);

    /// <summary>Posts every line with something left: parts leave stock, and the charged lines are invoiced.</summary>
    Task<ServicePostingResultDto> RunPostingAsync(Guid id, ServicePostingInput input);
}

public interface IServiceItemLineAppService : ICrudAppService<ServiceItemLineDto, Guid, GetServiceOrderLineListInput, CreateUpdateServiceItemLineDto> { }

public interface IServiceLineAppService : ICrudAppService<ServiceLineDto, Guid, GetServiceOrderLineListInput, CreateUpdateServiceLineDto> { }

// Ledgers

public class ServiceLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public ServiceLedgerEntryType EntryType { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public string ServiceOrderNo { get; set; }
    public string ServiceContractNo { get; set; }
    public string ServiceItemNo { get; set; }
    public string CustomerNo { get; set; }
    public ServiceLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostAmount { get; set; }
    public decimal Amount { get; set; }
    public bool Warranty { get; set; }
}

public class WarrantyLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public string ServiceOrderNo { get; set; }
    public string ServiceItemNo { get; set; }
    public string ItemNo { get; set; }
    public string SerialNo { get; set; }
    public string CustomerNo { get; set; }
    public ServiceLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public decimal CostAmount { get; set; }
}

public class GetServiceLedgerListInput : ErpPagedListInput
{
    public string ServiceOrderNo { get; set; }
    public string ServiceItemNo { get; set; }
}

public interface IServiceLedgerEntryAppService : IReadOnlyAppService<ServiceLedgerEntryDto, Guid, GetServiceLedgerListInput> { }

public interface IWarrantyLedgerEntryAppService : IReadOnlyAppService<WarrantyLedgerEntryDto, Guid, GetServiceLedgerListInput> { }
