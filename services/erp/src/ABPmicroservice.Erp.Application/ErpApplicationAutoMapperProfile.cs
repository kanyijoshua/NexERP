using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using AutoMapper;

namespace ABPmicroservice.Erp;

public class ErpApplicationAutoMapperProfile : Profile
{
    public ErpApplicationAutoMapperProfile()
    {
        // Finance
        CreateMap<GLAccount, GLAccountDto>();
        CreateMap<CreateUpdateGLAccountDto, GLAccount>();
        CreateMap<GLEntry, GLEntryDto>();

        // Inventory
        CreateMap<Item, ItemDto>();
        CreateMap<ItemCategory, ItemCategoryDto>();
        CreateMap<UnitOfMeasure, UnitOfMeasureDto>();
        CreateMap<ItemLedgerEntry, ItemLedgerEntryDto>();

        // Sales
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateUpdateCustomerDto, Customer>();
        CreateMap<SalesHeader, SalesHeaderDto>();
        CreateMap<SalesLine, SalesLineDto>();

        // Purchasing
        CreateMap<Vendor, VendorDto>();
        CreateMap<CreateUpdateVendorDto, Vendor>();
        CreateMap<PurchaseHeader, PurchaseHeaderDto>();
        CreateMap<PurchaseLine, PurchaseLineDto>();
    }
}
