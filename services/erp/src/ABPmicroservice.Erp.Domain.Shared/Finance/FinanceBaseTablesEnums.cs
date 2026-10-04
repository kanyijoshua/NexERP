namespace ABPmicroservice.Erp.Finance;

public enum CountryRegionAddressFormat
{
    PostCodeCity = 0,
    CityPostCode = 1,
    CityCountyPostCode = 2,
    BlankLinePostCodeCity = 3,
    Custom = 13,
}

public enum CountryRegionContactAddressFormat
{
    First = 0,
    AfterCompanyName = 1,
    Last = 2,
}

public enum InvoicePostingPolicy
{
    Allowed = 0,
    Prohibited = 1,
    Mandatory = 2,
}

public enum CommentLineTableName
{
    GLAccount = 0,
    Customer = 1,
    Vendor = 2,
    Item = 3,
    Resource = 4,
    Job = 5,
    ResourceGroup = 7,
    BankAccount = 8,
    Campaign = 9,
    FixedAsset = 10,
    Insurance = 11,
    NonstockItem = 12,
    ICPartner = 13,
    VendorAgreement = 23,
    CustomerAgreement = 24,
}
