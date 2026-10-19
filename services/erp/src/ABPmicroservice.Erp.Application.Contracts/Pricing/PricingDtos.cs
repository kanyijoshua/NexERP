using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Pricing;

public class PriceListDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public PriceType PriceType { get; set; }
    public PriceSourceType SourceType { get; set; }
    public string SourceNo { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime? StartingDate { get; set; }
    public DateTime? EndingDate { get; set; }
    public PriceListStatus Status { get; set; }
}

public class CreateUpdatePriceListDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public PriceType PriceType { get; set; }
    public PriceSourceType SourceType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SourceNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; }

    public DateTime? StartingDate { get; set; }
    public DateTime? EndingDate { get; set; }
    public PriceListStatus Status { get; set; }
}

public class GetPriceListListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public PriceType? PriceType { get; set; }
    public PriceListStatus? Status { get; set; }
}

public class PriceListLineDto : EntityDto<Guid>
{
    public Guid PriceListId { get; set; }
    public int LineNo { get; set; }
    public PriceAssetType AssetType { get; set; }
    public string AssetNo { get; set; }
    public string Description { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
}

public class CreateUpdatePriceListLineDto
{
    public Guid PriceListId { get; set; }
    public PriceAssetType AssetType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string AssetNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string UnitOfMeasureCode { get; set; }

    public decimal MinimumQuantity { get; set; }
    public decimal UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal LineDiscountPercent { get; set; }
}

public class GetPriceListLineListInput : ErpPagedListInput
{
    public Guid? PriceListId { get; set; }
    public string Filter { get; set; }
}

/// <summary>What a document line asks for: the price of an item for a party.</summary>
public class GetPriceInput
{
    public PriceType PriceType { get; set; }

    /// <summary>The customer or vendor number.</summary>
    public string PartyNo { get; set; }

    [Required]
    public string ItemNo { get; set; }

    public string UnitOfMeasureCode { get; set; }
    public decimal Quantity { get; set; } = 1m;
    public string CurrencyCode { get; set; }
    public DateTime? Date { get; set; }
}

public class PriceQuoteDto
{
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }

    /// <summary>Null when the price is the item card's.</summary>
    public string PriceListCode { get; set; }
}

public class CustomerPriceGroupDto : CodeTableDto
{
    public bool AllowLineDiscount { get; set; }
}

public class CreateUpdateCustomerPriceGroupDto : CreateUpdateCodeTableDto
{
    public bool AllowLineDiscount { get; set; } = true;
}

public interface IPriceListAppService
    : ICrudAppService<PriceListDto, Guid, GetPriceListListInput, CreateUpdatePriceListDto, CreateUpdatePriceListDto>
{
    /// <summary>The price a document line gets. Routed as GET .../price-quote.</summary>
    Task<PriceQuoteDto> GetPriceQuoteAsync(GetPriceInput input);
}

public interface IPriceListLineAppService
    : ICrudAppService<PriceListLineDto, Guid, GetPriceListLineListInput, CreateUpdatePriceListLineDto, CreateUpdatePriceListLineDto> { }

public interface ICustomerPriceGroupAppService
    : ICrudAppService<CustomerPriceGroupDto, Guid, GetCodeTableListInput, CreateUpdateCustomerPriceGroupDto, CreateUpdateCustomerPriceGroupDto> { }
