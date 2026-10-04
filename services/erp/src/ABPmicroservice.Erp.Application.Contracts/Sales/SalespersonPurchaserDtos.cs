using System;
using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Salesperson/Purchaser. The description is the person's name.</summary>
public class SalespersonPurchaserDto : CodeTableDto
{
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public string JobTitle { get; set; }
    public decimal CommissionPercent { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateSalespersonPurchaserDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxJobTitleLength)]
    public string JobTitle { get; set; }

    [Range(0, 100)]
    public decimal CommissionPercent { get; set; }

    public bool Blocked { get; set; }
}

public interface ISalespersonPurchaserAppService
    : ICrudAppService<SalespersonPurchaserDto, Guid, GetCodeTableListInput, CreateUpdateSalespersonPurchaserDto, CreateUpdateSalespersonPurchaserDto> { }
