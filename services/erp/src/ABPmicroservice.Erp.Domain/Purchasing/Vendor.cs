using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Vendor card. Mirrors Business Central table 23 "Vendor".
/// </summary>
public class Vendor : CompanyAggregateRoot
{
    /// <summary>Business key. Mirrors BC field "No.".</summary>
    public string No { get; private set; }

    public string Name { get; private set; }

    public string Address { get; private set; }

    public string City { get; private set; }

    public string PostCode { get; private set; }

    public string CountryRegionCode { get; private set; }

    public string PhoneNo { get; private set; }

    public string Email { get; private set; }

    /// <summary>Outstanding balance owed (denormalized, maintained by posting).</summary>
    public decimal Balance { get; internal set; }

    public string PaymentTermsCode { get; private set; }

    public string VendorPostingGroup { get; private set; }

    public string GenBusPostingGroup { get; private set; }

    public string VatBusPostingGroup { get; private set; }

    public string PurchaserCode { get; private set; }

    public string PaymentMethodCode { get; private set; }

    public string CurrencyCode { get; private set; }

    public bool Blocked { get; private set; }

    /// <summary>Search Name. Mirrors BC field 3 "Search Name".</summary>
    public string SearchName { get; private set; }

    /// <summary>Name 2. Mirrors BC field 4 "Name 2".</summary>
    public string Name2 { get; private set; }

    /// <summary>Address 2. Mirrors BC field 6 "Address 2".</summary>
    public string Address2 { get; private set; }

    /// <summary>Contact person. Mirrors BC field 8 "Contact".</summary>
    public string Contact { get; private set; }

    /// <summary>Our account number with the vendor. Mirrors BC field 14 "Our Account No.".</summary>
    public string OurAccountNo { get; private set; }

    /// <summary>Shipment Method Code. Mirrors BC field 30 "Shipment Method Code".</summary>
    public string ShipmentMethodCode { get; private set; }

    /// <summary>Shipping Agent Code. Mirrors BC field 31 "Shipping Agent Code".</summary>
    public string ShippingAgentCode { get; private set; }

    /// <summary>Invoice Disc. Code. Mirrors BC field 33 "Invoice Disc. Code".</summary>
    public string InvoiceDiscCode { get; private set; }

    /// <summary>Prices Including VAT. Mirrors BC field 82 "Prices Including VAT".</summary>
    public bool PricesIncludingVAT { get; private set; }

    /// <summary>VAT Registration No. Mirrors BC field 86 "VAT Registration No.".</summary>
    public string VATRegistrationNo { get; private set; }

    /// <summary>Home Page. Mirrors BC field 103 "Home Page".</summary>
    public string HomePage { get; private set; }

    /// <summary>Tax Area Code. Mirrors BC field 108 "Tax Area Code".</summary>
    public string TaxAreaCode { get; private set; }

    /// <summary>Tax Liable. Mirrors BC field 109 "Tax Liable".</summary>
    public bool TaxLiable { get; private set; }

    /// <summary>Block Payment Tolerance. Mirrors BC field 116 "Block Payment Tolerance".</summary>
    public bool BlockPaymentTolerance { get; private set; }

    /// <summary>Prepayment %. Mirrors BC field 124 "Prepayment %".</summary>
    public decimal PrepaymentPct { get; private set; }

    /// <summary>Allow Multiple Posting Groups. Mirrors BC field 175 "Allow Multiple Posting Groups".</summary>
    public bool AllowMultiplePostingGroups { get; private set; }

    /// <summary>Mobile Phone No. Mirrors BC field 5061 "Mobile Phone No.".</summary>
    public string MobilePhoneNo { get; private set; }

    /// <summary>Default Receiving Location Code. Mirrors BC field 5701 "Location Code".</summary>
    public string LocationCode { get; private set; }

    /// <summary>Lead Time Calculation formula. Mirrors BC field 5790 "Lead Time Calculation".</summary>
    public string LeadTimeCalculation { get; private set; }

    protected Vendor() { }

    public Vendor(
        Guid id,
        string no,
        string name,
        string searchName = null,
        string name2 = null,
        string address = null,
        string address2 = null,
        string city = null,
        string postCode = null,
        string countryRegionCode = null,
        string contact = null,
        string phoneNo = null,
        string mobilePhoneNo = null,
        string email = null,
        string homePage = null,
        string vatRegistrationNo = null,
        string taxAreaCode = null,
        bool taxLiable = false,
        string ourAccountNo = null,
        string shipmentMethodCode = null,
        string shippingAgentCode = null,
        string invoiceDiscCode = null,
        bool pricesIncludingVat = false,
        bool blockPaymentTolerance = false,
        decimal prepaymentPct = 0m,
        string locationCode = null,
        string leadTimeCalc = null,
        bool allowMultiplePostingGroups = false
    )
        : base(id)
    {
        SetNo(no);
        SetName(name);
        Name2 = Check.Length(name2, nameof(name2), ErpDomainConsts.MaxCityLength);
        SetSearchName(searchName ?? name);
        Balance = 0m;
        SetAddress(address, address2, city, postCode, countryRegionCode);
        SetContact(contact, phoneNo, mobilePhoneNo, email, homePage);
        SetTaxDetails(vatRegistrationNo, taxAreaCode, taxLiable, pricesIncludingVat);
        SetShipping(locationCode, shipmentMethodCode, shippingAgentCode, leadTimeCalc);
        SetPaymentPreferences(ourAccountNo, blockPaymentTolerance, prepaymentPct, allowMultiplePostingGroups);
        SetInvoiceDiscCode(invoiceDiscCode);
    }

    public void SetNo(string no) =>
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);

    public void SetName(string name) =>
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);

    public void SetNames(string name, string name2 = null)
    {
        SetName(name);
        Name2 = Check.Length(name2, nameof(name2), ErpDomainConsts.MaxCityLength);
    }

    public void SetSearchName(string searchName) =>
        SearchName = searchName.IsNullOrWhiteSpace()
            ? Name
            : Check.Length(searchName.Trim(), nameof(searchName), ErpDomainConsts.MaxNameLength);

    public void SetAddress(string address, string address2, string city, string postCode, string countryRegionCode)
    {
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        Address2 = Check.Length(address2, nameof(address2), ErpDomainConsts.MaxCityLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        PostCode = Check.Length(postCode, nameof(postCode), ErpDomainConsts.MaxPostCodeLength);
        CountryRegionCode = Check.Length(
            countryRegionCode,
            nameof(countryRegionCode),
            ErpDomainConsts.MaxCountryRegionCodeLength
        );
    }

    public void SetAddress(string address, string city, string postCode, string countryRegionCode) =>
        SetAddress(address, null, city, postCode, countryRegionCode);

    public void SetContact(string contact, string phoneNo, string mobilePhoneNo, string email, string homePage)
    {
        Contact = Check.Length(contact, nameof(contact), ErpDomainConsts.MaxContactLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        MobilePhoneNo = Check.Length(mobilePhoneNo, nameof(mobilePhoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        HomePage = Check.Length(homePage, nameof(homePage), ErpDomainConsts.MaxHomePageLength);
    }

    public void SetContact(string phoneNo, string email) =>
        SetContact(Contact, phoneNo, MobilePhoneNo, email, HomePage);

    public void SetTaxDetails(string vatRegistrationNo, string taxAreaCode, bool taxLiable, bool pricesIncludingVat)
    {
        VATRegistrationNo = Check.Length(vatRegistrationNo, nameof(vatRegistrationNo), ErpDomainConsts.MaxVatRegistrationNoLength);
        TaxAreaCode = CodeTableEntity.NormalizeCode(Check.Length(taxAreaCode, nameof(taxAreaCode), ErpDomainConsts.MaxTaxAreaCodeLength));
        TaxLiable = taxLiable;
        PricesIncludingVAT = pricesIncludingVat;
    }

    public void SetShipping(string locationCode, string shipmentMethodCode, string shippingAgentCode, string leadTimeCalc)
    {
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
        ShipmentMethodCode = CodeTableEntity.NormalizeCode(Check.Length(shipmentMethodCode, nameof(shipmentMethodCode), ErpDomainConsts.MaxShipmentMethodCodeLength));
        ShippingAgentCode = CodeTableEntity.NormalizeCode(Check.Length(shippingAgentCode, nameof(shippingAgentCode), ErpDomainConsts.MaxShippingAgentCodeLength));
        LeadTimeCalculation = Check.Length(leadTimeCalc, nameof(leadTimeCalc), ErpDomainConsts.MaxDateFormulaLength);
    }

    public void SetPaymentPreferences(string ourAccountNo, bool blockPaymentTolerance, decimal prepaymentPct, bool allowMultiplePostingGroups)
    {
        OurAccountNo = Check.Length(ourAccountNo, nameof(ourAccountNo), ErpDomainConsts.MaxNoLength);
        BlockPaymentTolerance = blockPaymentTolerance;
        PrepaymentPct = prepaymentPct >= 0 ? prepaymentPct : 0m;
        AllowMultiplePostingGroups = allowMultiplePostingGroups;
    }

    public void SetInvoiceDiscCode(string invoiceDiscCode) =>
        InvoiceDiscCode = CodeTableEntity.NormalizeCode(Check.Length(invoiceDiscCode, nameof(invoiceDiscCode), ErpDomainConsts.MaxPostingGroupLength));

    public void SetPaymentTerms(string paymentTermsCode) =>
        PaymentTermsCode = Check.Length(
            paymentTermsCode,
            nameof(paymentTermsCode),
            ErpDomainConsts.MaxPaymentTermsCodeLength
        );

    public void SetPostingGroups(string vendorPostingGroup, string genBusPostingGroup)
    {
        VendorPostingGroup = Check.Length(
            vendorPostingGroup,
            nameof(vendorPostingGroup),
            ErpDomainConsts.MaxPostingGroupLength
        );
        GenBusPostingGroup = Check.Length(
            genBusPostingGroup,
            nameof(genBusPostingGroup),
            ErpDomainConsts.MaxPostingGroupLength
        );
    }

    public void SetCurrency(string currencyCode) =>
        CurrencyCode = Check.Length(
            currencyCode,
            nameof(currencyCode),
            ErpDomainConsts.MaxCurrencyCodeLength
        );

    public void Block() => Blocked = true;

    public void Unblock() => Blocked = false;

    internal void ApplyBalance(decimal amount) => Balance += amount;

    public void SetVatBusPostingGroup(string vatBusPostingGroup) =>
        VatBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatBusPostingGroup, nameof(vatBusPostingGroup), ErpDomainConsts.MaxVatBusPostingGroupLength));

    public void SetPurchaserCode(string purchaserCode) =>
        PurchaserCode = CodeTableEntity.NormalizeCode(Check.Length(purchaserCode, nameof(purchaserCode), ErpDomainConsts.MaxCodeLength));

    public void SetPaymentMethodCode(string paymentMethodCode) =>
        PaymentMethodCode = CodeTableEntity.NormalizeCode(Check.Length(paymentMethodCode, nameof(paymentMethodCode), ErpDomainConsts.MaxCodeLength));
}
