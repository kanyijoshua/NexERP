using System;
using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Purchasing;

public class StandardPurchaseCodeDto : CodeTableDto
{
    public string CurrencyCode { get; set; }
    public int LineCount { get; set; }
}

public class CreateUpdateStandardPurchaseCodeDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; }
}

public interface IStandardPurchaseCodeAppService
    : ICrudAppService<StandardPurchaseCodeDto, Guid, GetCodeTableListInput, CreateUpdateStandardPurchaseCodeDto> { }

public class StandardPurchaseLineDto : EntityDto<Guid>
{
    public Guid StandardPurchaseCodeId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitAmount { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class CreateUpdateStandardPurchaseLineDto
{
    public Guid StandardPurchaseCodeId { get; set; }
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

public class GetStandardPurchaseLineListInput : ErpPagedListInput
{
    public Guid? StandardPurchaseCodeId { get; set; }
}

public interface IStandardPurchaseLineAppService
    : ICrudAppService<StandardPurchaseLineDto, Guid, GetStandardPurchaseLineListInput, CreateUpdateStandardPurchaseLineDto> { }

public class StandardVendorPurchaseCodeDto : FullAuditedEntityDto<Guid>
{
    public string VendorNo { get; set; }
    public string Code { get; set; }
    public StandardCodeInsertMode InsertOnQuotes { get; set; }
    public StandardCodeInsertMode InsertOnOrders { get; set; }
    public StandardCodeInsertMode InsertOnInvoices { get; set; }
}

public class CreateUpdateStandardVendorPurchaseCodeDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VendorNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    public StandardCodeInsertMode InsertOnQuotes { get; set; }
    public StandardCodeInsertMode InsertOnOrders { get; set; }
    public StandardCodeInsertMode InsertOnInvoices { get; set; }
}

public class GetStandardVendorPurchaseCodeListInput : ErpPagedListInput
{
    public string VendorNo { get; set; }
}

public interface IStandardVendorPurchaseCodeAppService
    : ICrudAppService<StandardVendorPurchaseCodeDto, Guid, GetStandardVendorPurchaseCodeListInput, CreateUpdateStandardVendorPurchaseCodeDto> { }
