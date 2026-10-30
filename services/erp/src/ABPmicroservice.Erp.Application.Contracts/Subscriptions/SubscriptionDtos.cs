using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Subscriptions;

public class SubscriptionPlanDto : CodeTableDto
{
    public int BillingPeriodMonths { get; set; }
    public decimal Price { get; set; }
    public SubscriptionLineType LineType { get; set; }
    public string LineNo { get; set; }
    public int TrialDays { get; set; }
}

public class CreateUpdateSubscriptionPlanDto : CreateUpdateCodeTableDto
{
    [Range(1, 60)]
    public int BillingPeriodMonths { get; set; } = 1;

    public decimal Price { get; set; }
    public SubscriptionLineType LineType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string LineNo { get; set; }

    [Range(0, 365)]
    public int TrialDays { get; set; }
}

public interface ISubscriptionPlanAppService : ICrudAppService<SubscriptionPlanDto, Guid, GetCodeTableListInput, CreateUpdateSubscriptionPlanDto> { }

public class CustomerSubscriptionDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public string PlanCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime NextBillingDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public string LastInvoiceNo { get; set; }
    public DateTime? LastInvoiceDate { get; set; }
}

public class CreateUpdateCustomerSubscriptionDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PlanCode { get; set; }

    public decimal Quantity { get; set; } = 1m;

    /// <summary>Zero takes the plan's price.</summary>
    public decimal UnitPrice { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetCustomerSubscriptionListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public SubscriptionStatus? Status { get; set; }
}

public class InvoiceSubscriptionsInput
{
    public DateTime? AsOfDate { get; set; }

    /// <summary>Post the invoices at once; otherwise they wait as open invoices.</summary>
    public bool Post { get; set; } = true;
}

public class SubscriptionInvoicingResultDto
{
    public int InvoiceCount { get; set; }
}

public interface ICustomerSubscriptionAppService
    : ICrudAppService<CustomerSubscriptionDto, Guid, GetCustomerSubscriptionListInput, CreateUpdateCustomerSubscriptionDto>
{
    Task<CustomerSubscriptionDto> ActivateAsync(Guid id);

    Task<CustomerSubscriptionDto> PauseAsync(Guid id);

    Task<CustomerSubscriptionDto> CancelAsync(Guid id);

    Task<SubscriptionInvoicingResultDto> InvoiceDueAsync(Guid id, InvoiceSubscriptionsInput input);

    Task<SubscriptionInvoicingResultDto> InvoiceAllDueAsync(InvoiceSubscriptionsInput input);
}
