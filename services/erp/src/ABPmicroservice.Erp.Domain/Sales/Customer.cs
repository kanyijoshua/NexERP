using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Customer card.
/// </summary>
public class Customer : CompanyAggregateRoot, IHasNo
{
    /// <summary>Business key.</summary>
    public string No { get; private set; }

    public string Name { get; private set; }

    public string Address { get; private set; }

    public string City { get; private set; }

    public string PostCode { get; private set; }

    public string CountryRegionCode { get; private set; }

    public string PhoneNo { get; private set; }

    public string Email { get; private set; }

    public decimal CreditLimit { get; private set; }

    /// <summary>Outstanding balance (denormalized, maintained by posting).</summary>
    public decimal Balance { get; internal set; }

    public string PaymentTermsCode { get; private set; }

    public string CustomerPostingGroup { get; private set; }

    public string GenBusPostingGroup { get; private set; }

    public string VatBusPostingGroup { get; private set; }

    public string SalespersonCode { get; private set; }

    public string PaymentMethodCode { get; private set; }

    public string CurrencyCode { get; private set; }

    public bool Blocked { get; private set; }

    /// <summary>Search Name.</summary>
    public string SearchName { get; private set; }

    /// <summary>Name 2.</summary>
    public string Name2 { get; private set; }

    /// <summary>Address 2.</summary>
    public string Address2 { get; private set; }

    /// <summary>County.</summary>
    public string County { get; private set; }

    /// <summary>Contact.</summary>
    public string Contact { get; private set; }

    /// <summary>Mobile Phone No..</summary>
    public string MobilePhoneNo { get; private set; }

    /// <summary>Home Page.</summary>
    public string HomePage { get; private set; }

    /// <summary>VAT Registration No..</summary>
    public string VatRegistrationNo { get; private set; }

    /// <summary>Registration Number.</summary>
    public string RegistrationNumber { get; private set; }

    /// <summary>Global Dimension 1 Code.</summary>
    public string GlobalDimension1Code { get; private set; }

    /// <summary>Global Dimension 2 Code.</summary>
    public string GlobalDimension2Code { get; private set; }

    /// <summary>Language Code.</summary>
    public string LanguageCode { get; private set; }

    /// <summary>Location Code.</summary>
    public string LocationCode { get; private set; }

    /// <summary>Shipment Method Code.</summary>
    public string ShipmentMethodCode { get; private set; }

    /// <summary>Responsibility Center.</summary>
    public string ResponsibilityCenter { get; private set; }

    /// <summary>Customer Price Group.</summary>
    public string CustomerPriceGroup { get; private set; }

    /// <summary>Customer Disc. Group.</summary>
    public string CustomerDiscGroup { get; private set; }

    /// <summary>Invoice Disc. Code.</summary>
    public string InvoiceDiscCode { get; private set; }

    /// <summary>Fin. Charge Terms Code.</summary>
    public string FinChargeTermsCode { get; private set; }

    /// <summary>Reminder Terms Code.</summary>
    public string ReminderTermsCode { get; private set; }

    /// <summary>Application Method.</summary>
    public ApplicationMethod ApplicationMethod { get; private set; }

    /// <summary>Prices Including VAT.</summary>
    public bool PricesIncludingVat { get; private set; }

    /// <summary>Tax Area Code.</summary>
    public string TaxAreaCode { get; private set; }

    /// <summary>Tax Liable.</summary>
    public bool TaxLiable { get; private set; }

    /// <summary>Block Payment Tolerance.</summary>
    public bool BlockPaymentTolerance { get; private set; }

    /// <summary>Prepayment %.</summary>
    public decimal PrepaymentPct { get; private set; }

    /// <summary>Print Statements.</summary>
    public bool PrintStatements { get; private set; }

    /// <summary>Last Statement No..</summary>
    public int LastStatementNo { get; private set; }

    /// <summary>Combine Shipments.</summary>
    public bool CombineShipments { get; private set; }

    /// <summary>Preferred Bank Account Code.</summary>
    public string PreferredBankAccountCode { get; private set; }

    /// <summary>Primary Contact No..</summary>
    public string PrimaryContactNo { get; private set; }

    /// <summary>Privacy Blocked.</summary>
    public bool PrivacyBlocked { get; private set; }

    protected Customer() { }

    public Customer(Guid id, string no, string name)
        : base(id)
    {
        SetNo(no);
        SetName(name);
        Balance = 0m;
    }

    public void SetNo(string no) =>
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);

    public void SetName(string name) =>
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);

    public void SetAddress(string address, string city, string postCode, string countryRegionCode)
    {
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        PostCode = Check.Length(postCode, nameof(postCode), ErpDomainConsts.MaxPostCodeLength);
        CountryRegionCode = Check.Length(
            countryRegionCode,
            nameof(countryRegionCode),
            ErpDomainConsts.MaxCountryRegionCodeLength
        );
    }

    public void SetContact(string phoneNo, string email)
    {
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
    }

    public void SetCreditLimit(decimal creditLimit) => CreditLimit = creditLimit;

    public void SetPaymentTerms(string paymentTermsCode) =>
        PaymentTermsCode = Check.Length(
            paymentTermsCode,
            nameof(paymentTermsCode),
            ErpDomainConsts.MaxPaymentTermsCodeLength
        );

    public void SetPostingGroups(string customerPostingGroup, string genBusPostingGroup)
    {
        CustomerPostingGroup = Check.Length(
            customerPostingGroup,
            nameof(customerPostingGroup),
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

    public void SetSalespersonCode(string salespersonCode) =>
        SalespersonCode = CodeTableEntity.NormalizeCode(Check.Length(salespersonCode, nameof(salespersonCode), ErpDomainConsts.MaxCodeLength));

    public void SetPaymentMethodCode(string paymentMethodCode) =>
        PaymentMethodCode = CodeTableEntity.NormalizeCode(Check.Length(paymentMethodCode, nameof(paymentMethodCode), ErpDomainConsts.MaxCodeLength));

    /// <summary>The card fields beyond those the posting routines read.</summary>
    public void SetAdditionalFields(
        string searchName,
        string name2,
        string address2,
        string county,
        string contact,
        string mobilePhoneNo,
        string homePage,
        string vatRegistrationNo,
        string registrationNumber,
        string globalDimension1Code,
        string globalDimension2Code,
        string languageCode,
        string locationCode,
        string shipmentMethodCode,
        string responsibilityCenter,
        string customerPriceGroup,
        string customerDiscGroup,
        string invoiceDiscCode,
        string finChargeTermsCode,
        string reminderTermsCode,
        ApplicationMethod applicationMethod,
        bool pricesIncludingVat,
        string taxAreaCode,
        bool taxLiable,
        bool blockPaymentTolerance,
        decimal prepaymentPct,
        bool printStatements,
        int lastStatementNo,
        bool combineShipments,
        string preferredBankAccountCode,
        string primaryContactNo,
        bool privacyBlocked
    )
    {
        SearchName = CodeTableEntity.NormalizeCode(Check.Length(searchName, nameof(searchName), 100));
        Name2 = Check.Length(name2, nameof(name2), 50);
        Address2 = Check.Length(address2, nameof(address2), 50);
        County = Check.Length(county, nameof(county), 30);
        Contact = Check.Length(contact, nameof(contact), 100);
        MobilePhoneNo = Check.Length(mobilePhoneNo, nameof(mobilePhoneNo), 30);
        HomePage = Check.Length(homePage, nameof(homePage), 80);
        VatRegistrationNo = Check.Length(vatRegistrationNo, nameof(vatRegistrationNo), 20);
        RegistrationNumber = Check.Length(registrationNumber, nameof(registrationNumber), 50);
        GlobalDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension1Code, nameof(globalDimension1Code), 20));
        GlobalDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension2Code, nameof(globalDimension2Code), 20));
        LanguageCode = CodeTableEntity.NormalizeCode(Check.Length(languageCode, nameof(languageCode), 10));
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), 10));
        ShipmentMethodCode = CodeTableEntity.NormalizeCode(Check.Length(shipmentMethodCode, nameof(shipmentMethodCode), 10));
        ResponsibilityCenter = CodeTableEntity.NormalizeCode(Check.Length(responsibilityCenter, nameof(responsibilityCenter), 10));
        CustomerPriceGroup = CodeTableEntity.NormalizeCode(Check.Length(customerPriceGroup, nameof(customerPriceGroup), 10));
        CustomerDiscGroup = CodeTableEntity.NormalizeCode(Check.Length(customerDiscGroup, nameof(customerDiscGroup), 20));
        InvoiceDiscCode = CodeTableEntity.NormalizeCode(Check.Length(invoiceDiscCode, nameof(invoiceDiscCode), 20));
        FinChargeTermsCode = CodeTableEntity.NormalizeCode(Check.Length(finChargeTermsCode, nameof(finChargeTermsCode), 10));
        ReminderTermsCode = CodeTableEntity.NormalizeCode(Check.Length(reminderTermsCode, nameof(reminderTermsCode), 10));
        ApplicationMethod = applicationMethod;
        PricesIncludingVat = pricesIncludingVat;
        TaxAreaCode = CodeTableEntity.NormalizeCode(Check.Length(taxAreaCode, nameof(taxAreaCode), 20));
        TaxLiable = taxLiable;
        BlockPaymentTolerance = blockPaymentTolerance;
        PrepaymentPct = prepaymentPct;
        PrintStatements = printStatements;
        LastStatementNo = lastStatementNo;
        CombineShipments = combineShipments;
        PreferredBankAccountCode = CodeTableEntity.NormalizeCode(Check.Length(preferredBankAccountCode, nameof(preferredBankAccountCode), 20));
        PrimaryContactNo = CodeTableEntity.NormalizeCode(Check.Length(primaryContactNo, nameof(primaryContactNo), 20));
        PrivacyBlocked = privacyBlocked;
    }
}
