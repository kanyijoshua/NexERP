using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Location: a warehouse or store stock is kept at. The
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

    /// <summary>Holds stock on its way between two locations; transfer orders ship into it and receive out of it.</summary>
    public bool UseAsInTransit { get; private set; }

    /// <summary>Stock at the location is kept in bins: every movement names the bin it comes from or goes to.</summary>
    public bool BinMandatory { get; private set; }

    /// <summary>Received stock waits in the receipt bin until a put-away moves it to where it is stored.</summary>
    public bool RequirePutAway { get; private set; }

    /// <summary>Stock is shipped from the shipment bin only, so it has to be picked there first.</summary>
    public bool RequirePick { get; private set; }

    public string ReceiptBinCode { get; private set; }
    public string ShipmentBinCode { get; private set; }

    /// <summary>Where stock goes, and is taken from, when a movement does not name a bin.</summary>
    public string DefaultBinCode { get; private set; }

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

    public void SetUseAsInTransit(bool useAsInTransit) => UseAsInTransit = useAsInTransit;

    /// <summary>
    /// The bin rules. Put-aways and picks need bins, and a location that requires them needs the
    /// bin they start or end in; the bins themselves are checked by the caller.
    /// </summary>
    public void SetWarehouse(bool binMandatory, bool requirePutAway, bool requirePick, string receiptBinCode, string shipmentBinCode, string defaultBinCode)
    {
        if (!binMandatory && (requirePutAway || requirePick))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.BinsNotUsed).WithData("locationCode", Code);
        }

        BinMandatory = binMandatory;
        RequirePutAway = requirePutAway;
        RequirePick = requirePick;
        ReceiptBinCode = binMandatory ? Bin.NormalizeCode(receiptBinCode) : null;
        ShipmentBinCode = binMandatory ? Bin.NormalizeCode(shipmentBinCode) : null;
        DefaultBinCode = binMandatory ? Bin.NormalizeCode(defaultBinCode) : null;

        if (RequirePutAway && ReceiptBinCode == null)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.BinRequired).WithData("locationCode", Code).WithData("itemNo", "");
        }

        if (RequirePick && ShipmentBinCode == null)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.BinRequired).WithData("locationCode", Code).WithData("itemNo", "");
        }
    }

    public void SetContact(string contact, string phoneNo)
    {
        Contact = Check.Length(contact, nameof(contact), ErpDomainConsts.MaxNameLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
    }
}
