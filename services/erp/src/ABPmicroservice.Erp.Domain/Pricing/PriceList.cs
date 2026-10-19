using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Pricing;

/// <summary>
/// A price list: selling prices for all customers, a customer price group or one customer, or
/// purchase costs for all vendors or one vendor, in one currency and for a period. Each line prices
/// an item or every item of a category, from a minimum quantity, with an optional line discount.
/// </summary>
public class PriceList : CompanyAggregateRoot
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public PriceType PriceType { get; private set; }
    public PriceSourceType SourceType { get; private set; }

    /// <summary>The price group, customer or vendor the list is for; blank for all customers or vendors.</summary>
    public string SourceNo { get; private set; }

    /// <summary>Blank is local currency.</summary>
    public string CurrencyCode { get; private set; }

    public DateTime? StartingDate { get; private set; }
    public DateTime? EndingDate { get; private set; }
    public PriceListStatus Status { get; private set; }

    public Collection<PriceListLine> Lines { get; private set; }

    protected PriceList()
    {
        Lines = new Collection<PriceListLine>();
    }

    public PriceList(Guid id, string code, PriceType priceType)
        : base(id)
    {
        SetCode(code);
        PriceType = priceType;
        SourceType = priceType == PriceType.Sale ? PriceSourceType.AllCustomers : PriceSourceType.AllVendors;
        Status = PriceListStatus.Draft;
        Lines = new Collection<PriceListLine>();
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code?.Trim(), nameof(code), ErpDomainConsts.MaxCodeLength).ToUpperInvariant();
    }

    public void SetDescription(string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>Who the list is for. A sales list has customer sources, a purchase list vendor sources.</summary>
    public void SetSource(PriceSourceType sourceType, string sourceNo)
    {
        var forSales = sourceType is PriceSourceType.AllCustomers or PriceSourceType.CustomerPriceGroup or PriceSourceType.Customer;
        if (forSales != (PriceType == PriceType.Sale))
        {
            sourceType = PriceType == PriceType.Sale ? PriceSourceType.AllCustomers : PriceSourceType.AllVendors;
        }

        var needsNo = sourceType is not (PriceSourceType.AllCustomers or PriceSourceType.AllVendors);
        if (needsNo && sourceNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Pricing.SourceRequired).WithData("sourceType", sourceType.ToString());
        }

        SourceType = sourceType;
        SourceNo = needsNo ? CodeTableEntity.NormalizeCode(Check.Length(sourceNo, nameof(sourceNo), ErpDomainConsts.MaxNoLength)) : null;
    }

    public void SetCurrency(string currencyCode)
    {
        CurrencyCode = CodeTableEntity.NormalizeCode(Check.Length(currencyCode, nameof(currencyCode), ErpDomainConsts.MaxCurrencyCodeLength));
    }

    public void SetPeriod(DateTime? startingDate, DateTime? endingDate)
    {
        if (startingDate.HasValue && endingDate.HasValue && endingDate.Value.Date < startingDate.Value.Date)
        {
            throw new BusinessException(ErpErrorCodes.Pricing.InvalidPeriod).WithData("code", Code);
        }

        StartingDate = startingDate?.Date;
        EndingDate = endingDate?.Date;
    }

    public void SetStatus(PriceListStatus status)
    {
        Status = status;
    }

    public bool AppliesOn(DateTime date) =>
        Status == PriceListStatus.Active
        && (!StartingDate.HasValue || StartingDate.Value <= date.Date)
        && (!EndingDate.HasValue || EndingDate.Value >= date.Date);

    /// <summary>Lines can be changed only while the list is not active, so prices never move under a document.</summary>
    public void EnsureEditable()
    {
        if (Status == PriceListStatus.Active)
        {
            throw new BusinessException(ErpErrorCodes.Pricing.PriceListNotEditable).WithData("code", Code);
        }
    }

    public PriceListLine AddLine(Guid lineId, PriceAssetType assetType, string assetNo)
    {
        EnsureEditable();
        var line = new PriceListLine(lineId, Id, Lines.Count == 0 ? 1 : Lines.Max(l => l.LineNo) + 1, assetType, assetNo);
        Lines.Add(line);
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureEditable();
        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            Lines.Remove(line);
        }
    }
}

/// <summary>One price on a price list.</summary>
public class PriceListLine : Entity<Guid>
{
    public Guid PriceListId { get; private set; }
    public int LineNo { get; private set; }
    public PriceAssetType AssetType { get; private set; }

    /// <summary>The item number or item category code.</summary>
    public string AssetNo { get; private set; }

    public string Description { get; private set; }

    /// <summary>Blank prices every unit of measure.</summary>
    public string UnitOfMeasureCode { get; private set; }

    /// <summary>The price applies from this quantity on.</summary>
    public decimal MinimumQuantity { get; private set; }

    /// <summary>The unit price on a sales list, the direct unit cost on a purchase list.</summary>
    public decimal UnitPrice { get; private set; }

    public decimal LineDiscountPercent { get; private set; }

    protected PriceListLine() { }

    public PriceListLine(Guid id, Guid priceListId, int lineNo, PriceAssetType assetType, string assetNo)
        : base(id)
    {
        PriceListId = priceListId;
        LineNo = lineNo;
        SetAsset(assetType, assetNo);
    }

    public void SetAsset(PriceAssetType assetType, string assetNo)
    {
        if (assetNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Pricing.AssetRequired);
        }

        AssetType = assetType;
        AssetNo = Check.Length(assetNo.Trim(), nameof(assetNo), ErpDomainConsts.MaxNoLength);
    }

    public void SetPrice(string description, string unitOfMeasureCode, decimal minimumQuantity, decimal unitPrice, decimal lineDiscountPercent)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        UnitOfMeasureCode = CodeTableEntity.NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
        MinimumQuantity = Math.Max(0m, minimumQuantity);
        UnitPrice = Math.Max(0m, unitPrice);
        LineDiscountPercent = Math.Clamp(lineDiscountPercent, 0m, 100m);
    }
}

/// <summary>A group of customers that share a sales price list.</summary>
public class CustomerPriceGroup : CodeTableEntity
{
    /// <summary>Matches the length of the price group field on a customer.</summary>
    protected override int MaxCodeLength => 10;

    /// <summary>Whether line discounts are given on top of the group's prices.</summary>
    public bool AllowLineDiscount { get; private set; } = true;

    protected CustomerPriceGroup() { }

    public CustomerPriceGroup(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetAllowLineDiscount(bool allowLineDiscount)
    {
        AllowLineDiscount = allowLineDiscount;
    }
}
