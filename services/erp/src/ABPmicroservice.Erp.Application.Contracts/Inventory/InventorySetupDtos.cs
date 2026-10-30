using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Inventory;

public class LocationDto : CodeTableDto
{
    public string Address { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string Contact { get; set; }
    public bool UseAsInTransit { get; set; }
    public bool BinMandatory { get; set; }
    public bool RequirePutAway { get; set; }
    public bool RequirePick { get; set; }
    public string ReceiptBinCode { get; set; }
    public string ShipmentBinCode { get; set; }
    public string DefaultBinCode { get; set; }
}

public class CreateUpdateLocationDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxAddressLength)]
    public string Address { get; set; }

    [StringLength(ErpDomainConsts.MaxCityLength)]
    public string City { get; set; }

    [StringLength(ErpDomainConsts.MaxPostCodeLength)]
    public string PostCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCountryRegionCodeLength)]
    public string CountryRegionCode { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Contact { get; set; }

    public bool UseAsInTransit { get; set; }
    public bool BinMandatory { get; set; }
    public bool RequirePutAway { get; set; }
    public bool RequirePick { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ReceiptBinCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ShipmentBinCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string DefaultBinCode { get; set; }
}

/// <summary>Inventory Setup.</summary>
public class InventorySetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ItemNos { get; set; }

    public bool LocationMandatory { get; set; }
    public bool PreventNegativeInventory { get; set; }
    public bool AutomaticCostPosting { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string TransferOrderNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PostedTransferShipmentNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PostedTransferReceiptNos { get; set; }
}

public interface ILocationAppService
    : ICrudAppService<LocationDto, Guid, GetCodeTableListInput, CreateUpdateLocationDto, CreateUpdateLocationDto> { }

public interface IInventorySetupAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/inventory-setup.</summary>
    Task<InventorySetupDto> GetAsync();

    Task<InventorySetupDto> UpdateAsync(InventorySetupDto input);
}
