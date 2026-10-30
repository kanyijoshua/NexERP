using System;
using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Sales;

public class StandardSalesCodeDto : CodeTableDto
{
    public string CurrencyCode { get; set; }
    public int LineCount { get; set; }
}

public class CreateUpdateStandardSalesCodeDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; }
}

public interface IStandardSalesCodeAppService
    : ICrudAppService<StandardSalesCodeDto, Guid, GetCodeTableListInput, CreateUpdateStandardSalesCodeDto> { }

public class StandardSalesLineDto : EntityDto<Guid>
{
    public Guid StandardSalesCodeId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitAmount { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class CreateUpdateStandardSalesLineDto
{
    public Guid StandardSalesCodeId { get; set; }
    public DocumentLineType Type { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Quantity { get; set; } = 1m;
    public decimal UnitAmount { get; set; }

    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string UnitOfMeasureCode { get; set; }
}

public class GetStandardSalesLineListInput : ErpPagedListInput
{
    public Guid? StandardSalesCodeId { get; set; }
}

public interface IStandardSalesLineAppService
    : ICrudAppService<StandardSalesLineDto, Guid, GetStandardSalesLineListInput, CreateUpdateStandardSalesLineDto> { }

public class StandardCustomerSalesCodeDto : FullAuditedEntityDto<Guid>
{
    public string CustomerNo { get; set; }
    public string Code { get; set; }
    public StandardCodeInsertMode InsertOnQuotes { get; set; }
    public StandardCodeInsertMode InsertOnOrders { get; set; }
    public StandardCodeInsertMode InsertOnInvoices { get; set; }
}

public class CreateUpdateStandardCustomerSalesCodeDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    public StandardCodeInsertMode InsertOnQuotes { get; set; }
    public StandardCodeInsertMode InsertOnOrders { get; set; }
    public StandardCodeInsertMode InsertOnInvoices { get; set; }
}

public class GetStandardCustomerSalesCodeListInput : ErpPagedListInput
{
    public string CustomerNo { get; set; }
}

public interface IStandardCustomerSalesCodeAppService
    : ICrudAppService<StandardCustomerSalesCodeDto, Guid, GetStandardCustomerSalesCodeListInput, CreateUpdateStandardCustomerSalesCodeDto> { }

/// <summary>The standard code whose lines go on the document.</summary>
public class InsertStandardLinesInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }
}
