namespace ABPmicroservice.Erp.Pricing;

/// <summary>Whether a price list holds selling prices or purchase costs.</summary>
public enum PriceType
{
    Sale = 0,
    Purchase = 1,
}

/// <summary>Who a price list applies to.</summary>
public enum PriceSourceType
{
    AllCustomers = 0,
    CustomerPriceGroup = 1,
    Customer = 2,
    AllVendors = 10,
    Vendor = 11,
}

/// <summary>What a price list line prices.</summary>
public enum PriceAssetType
{
    Item = 0,
    ItemCategory = 1,
}

/// <summary>Only an active price list is used; a draft is being prepared and an inactive one is kept for reference.</summary>
public enum PriceListStatus
{
    Draft = 0,
    Active = 1,
    Inactive = 2,
}
