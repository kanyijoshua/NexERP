using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Customer card. Mirrors Business Central table 18 "Customer".
/// </summary>
public class Customer : CompanyAggregateRoot
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

    public decimal CreditLimit { get; private set; }

    /// <summary>Outstanding balance (denormalized, maintained by posting).</summary>
    public decimal Balance { get; internal set; }

    public string PaymentTermsCode { get; private set; }

    public string CustomerPostingGroup { get; private set; }

    public string GenBusPostingGroup { get; private set; }

    public string CurrencyCode { get; private set; }

    public bool Blocked { get; private set; }

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
}
