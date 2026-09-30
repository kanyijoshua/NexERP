using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Location. Mirrors Business Central table 14: a warehouse or store stock is kept at. The
/// description is the location's name.
/// </summary>
public class Location : CodeTableEntity
{
    protected override int MaxCodeLength => ErpDomainConsts.MaxLocationCodeLength;

    public string Address { get; private set; }
    public string City { get; private set; }
    public string PostCode { get; private set; }
    public string CountryRegionCode { get; private set; }
    public string PhoneNo { get; private set; }
    public string Contact { get; private set; }

    protected Location() { }

    public Location(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetAddress(string address, string city, string postCode, string countryRegionCode)
    {
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        PostCode = Check.Length(postCode, nameof(postCode), ErpDomainConsts.MaxPostCodeLength);
        CountryRegionCode = Check.Length(countryRegionCode, nameof(countryRegionCode), ErpDomainConsts.MaxCountryRegionCodeLength);
    }

    public void SetContact(string contact, string phoneNo)
    {
        Contact = Check.Length(contact, nameof(contact), ErpDomainConsts.MaxNameLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
    }
}
