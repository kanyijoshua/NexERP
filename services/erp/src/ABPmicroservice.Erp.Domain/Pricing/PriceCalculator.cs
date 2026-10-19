using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Pricing;

/// <summary>What a document line asks the price lists.</summary>
public sealed class PriceQuery
{
    public PriceType PriceType { get; init; }

    /// <summary>The customer or vendor number.</summary>
    public string PartyNo { get; init; }

    /// <summary>The customer's price group; not used for purchases.</summary>
    public string CustomerPriceGroup { get; init; }

    public string ItemNo { get; init; }
    public string UnitOfMeasureCode { get; init; }
    public decimal Quantity { get; init; }

    /// <summary>Blank is local currency.</summary>
    public string CurrencyCode { get; init; }

    public DateTime Date { get; init; }
}

/// <summary>The price a document line gets, and where it came from.</summary>
public sealed class PriceQuote
{
    public decimal UnitPrice { get; init; }
    public decimal LineDiscountPercent { get; init; }

    /// <summary>The price list the unit price came from; null when it is the item card's price.</summary>
    public string PriceListCode { get; init; }
}

/// <summary>
/// Finds the price of an item for a customer or vendor: of every active price list line that
/// applies on the date to the party (directly, through its price group, or as everyone), to the
/// item (or its category), the currency, the unit of measure and the quantity, it takes the lowest
/// price and, separately, the highest line discount. With no price on any list the item card's unit
/// price (sales) or unit cost (purchases) is used, in local currency only.
/// </summary>
public class PriceCalculator : DomainService
{
    private readonly IRepository<PriceList, Guid> _priceListRepository;
    private readonly IRepository<CustomerPriceGroup, Guid> _priceGroupRepository;
    private readonly IRepository<Item, Guid> _itemRepository;

    public PriceCalculator(
        IRepository<PriceList, Guid> priceListRepository,
        IRepository<CustomerPriceGroup, Guid> priceGroupRepository,
        IRepository<Item, Guid> itemRepository
    )
    {
        _priceListRepository = priceListRepository;
        _priceGroupRepository = priceGroupRepository;
        _itemRepository = itemRepository;
    }

    public async Task<PriceQuote> FindPriceAsync(PriceQuery query)
    {
        var itemNo = query.ItemNo?.Trim();
        var item = itemNo.IsNullOrWhiteSpace() ? null : await _itemRepository.FirstOrDefaultAsync(i => i.No == itemNo);
        if (item == null)
        {
            return new PriceQuote();
        }

        var currencyCode = CodeTableEntity.NormalizeCode(query.CurrencyCode);
        var partyNo = CodeTableEntity.NormalizeCode(query.PartyNo);
        var priceGroup = CodeTableEntity.NormalizeCode(query.CustomerPriceGroup);
        var unitOfMeasure = CodeTableEntity.NormalizeCode(query.UnitOfMeasureCode);
        var quantity = Math.Abs(query.Quantity);

        var lists = await _priceListRepository.GetListAsync(
            l => l.PriceType == query.PriceType && l.Status == PriceListStatus.Active && l.CurrencyCode == currencyCode,
            includeDetails: true
        );

        var candidates = new List<PriceListLine>();
        var codeOf = new Dictionary<Guid, string>();
        foreach (var list in lists.Where(l => l.AppliesOn(query.Date) && AppliesTo(l, partyNo, priceGroup)))
        {
            foreach (var line in list.Lines.Where(l => Prices(l, item) && l.MinimumQuantity <= quantity && (l.UnitOfMeasureCode == null || l.UnitOfMeasureCode == unitOfMeasure)))
            {
                candidates.Add(line);
                codeOf[line.Id] = list.Code;
            }
        }

        var allowDiscount = true;
        if (priceGroup != null && query.PriceType == PriceType.Sale)
        {
            var group = await _priceGroupRepository.FirstOrDefaultAsync(g => g.Code == priceGroup);
            allowDiscount = group?.AllowLineDiscount ?? true;
        }

        var priced = candidates.Where(l => l.UnitPrice > 0m).OrderBy(l => l.UnitPrice).FirstOrDefault();
        var discount = allowDiscount && candidates.Count > 0 ? candidates.Max(l => l.LineDiscountPercent) : 0m;

        if (priced != null)
        {
            return new PriceQuote { UnitPrice = priced.UnitPrice, LineDiscountPercent = discount, PriceListCode = codeOf[priced.Id] };
        }

        // The item card holds local currency prices only.
        var cardPrice = currencyCode == null ? (query.PriceType == PriceType.Sale ? item.UnitPrice : item.UnitCost) : 0m;
        return new PriceQuote { UnitPrice = cardPrice, LineDiscountPercent = discount };
    }

    private static bool AppliesTo(PriceList list, string partyNo, string priceGroup)
    {
        return list.SourceType switch
        {
            PriceSourceType.AllCustomers or PriceSourceType.AllVendors => true,
            PriceSourceType.CustomerPriceGroup => priceGroup != null && list.SourceNo == priceGroup,
            _ => partyNo != null && list.SourceNo == partyNo,
        };
    }

    private static bool Prices(PriceListLine line, Item item)
    {
        return line.AssetType == PriceAssetType.Item
            ? string.Equals(line.AssetNo, item.No, StringComparison.OrdinalIgnoreCase)
            : item.ItemCategoryCode != null && string.Equals(line.AssetNo, item.ItemCategoryCode, StringComparison.OrdinalIgnoreCase);
    }
}
