using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

[Authorize(ErpPermissions.Vendors.Default)]
public class VendorAppService
    : ErpCrudAppService<
        Vendor,
        VendorDto,
        Guid,
        GetVendorListInput,
        CreateUpdateVendorDto,
        CreateUpdateVendorDto
    >,
        IVendorAppService
{
    private readonly VendorManager _vendorManager;
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;

    public VendorAppService(
        IRepository<Vendor, Guid> repository,
        VendorManager vendorManager,
        PurchasesPayablesSetupManager setupManager,
        NoSeriesManager noSeriesManager
    )
        : base(repository)
    {
        _vendorManager = vendorManager;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        GetPolicyName = ErpPermissions.Vendors.Default;
        GetListPolicyName = ErpPermissions.Vendors.Default;
        CreatePolicyName = ErpPermissions.Vendors.Create;
        UpdatePolicyName = ErpPermissions.Vendors.Update;
        DeletePolicyName = ErpPermissions.Vendors.Delete;
    }

    public override async Task<VendorDto> CreateAsync(CreateUpdateVendorDto input)
    {
        // Blank takes the next number of the Vendor Nos. series (BC: InitSeries).
        var setup = await _setupManager.GetAsync();
        var no = await _noSeriesManager.ResolveNoAsync(setup.VendorNos, input.No, Clock.Now);

        var vendor = await _vendorManager.CreateAsync(no, input.Name);
        await EnsurePostingGroupsExistAsync(input);
        ApplyInput(vendor, input);

        await Repository.InsertAsync(vendor, autoSave: true);
        return await MapToGetOutputDtoAsync(vendor);
    }

    public override async Task<VendorDto> UpdateAsync(Guid id, CreateUpdateVendorDto input)
    {
        var vendor = await GetEntityByIdAsync(id);

        if (!string.Equals(vendor.No, input.No, StringComparison.OrdinalIgnoreCase))
        {
            await _vendorManager.EnsureNoIsUniqueAsync(input.No, id);
            vendor.SetNo(input.No);
        }

        vendor.SetName(input.Name);
        await EnsurePostingGroupsExistAsync(input);
        ApplyInput(vendor, input);

        await Repository.UpdateAsync(vendor, autoSave: true);
        return await MapToGetOutputDtoAsync(vendor);
    }

    public async Task<VendorDto> GetByNoAsync(string no)
    {
        var vendor = await Repository.FirstOrDefaultAsync(x => x.No == no);
        return await MapToGetOutputDtoAsync(vendor);
    }

    [Authorize(ErpPermissions.Vendors.Update)]
    public async Task BlockAsync(Guid id)
    {
        var vendor = await GetEntityByIdAsync(id);
        vendor.Block();
        await Repository.UpdateAsync(vendor, autoSave: true);
    }

    [Authorize(ErpPermissions.Vendors.Update)]
    public async Task UnblockAsync(Guid id)
    {
        var vendor = await GetEntityByIdAsync(id);
        vendor.Unblock();
        await Repository.UpdateAsync(vendor, autoSave: true);
    }

    /// <summary>A posting group on the card must exist (BC TableRelation); blank means none.</summary>
    private async Task EnsurePostingGroupsExistAsync(CreateUpdateVendorDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<VendorPostingGroup>(input.VendorPostingGroup);
        await codes.EnsureExistsAsync<GenBusinessPostingGroup>(input.GenBusPostingGroup);
        await codes.EnsureExistsAsync<VatBusinessPostingGroup>(input.VatBusPostingGroup);
        await codes.EnsureExistsAsync<PaymentTerms>(input.PaymentTermsCode);
        await codes.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await codes.EnsureExistsAsync<SalespersonPurchaser>(input.PurchaserCode);
        await codes.EnsureExistsAsync<PaymentMethod>(input.PaymentMethodCode);
    }

    private static void ApplyInput(Vendor vendor, CreateUpdateVendorDto input)
    {
        vendor.SetNames(input.Name, input.Name2);
        vendor.SetSearchName(input.SearchName);
        vendor.SetAddress(input.Address, input.Address2, input.City, input.PostCode, input.CountryRegionCode);
        vendor.SetContact(input.Contact, input.PhoneNo, input.MobilePhoneNo, input.Email, input.HomePage);
        vendor.SetPaymentTerms(CodeTableEntity.NormalizeCode(input.PaymentTermsCode));
        vendor.SetPostingGroups(
            PostingGroupBase.NormalizeCode(input.VendorPostingGroup),
            PostingGroupBase.NormalizeCode(input.GenBusPostingGroup)
        );
        vendor.SetCurrency(CodeTableEntity.NormalizeCode(input.CurrencyCode));
        vendor.SetVatBusPostingGroup(input.VatBusPostingGroup);
        vendor.SetPurchaserCode(input.PurchaserCode);
        vendor.SetPaymentMethodCode(input.PaymentMethodCode);
        vendor.SetShipping(input.LocationCode, input.ShipmentMethodCode, input.ShippingAgentCode, input.LeadTimeCalculation);
        vendor.SetInvoiceDiscCode(input.InvoiceDiscCode);
        vendor.SetTaxDetails(input.VATRegistrationNo, input.TaxAreaCode, input.TaxLiable, input.PricesIncludingVAT);
        vendor.SetPaymentPreferences(input.OurAccountNo, input.BlockPaymentTolerance, input.PrepaymentPct, input.AllowMultiplePostingGroups);
    }

    protected override async Task<IQueryable<Vendor>> CreateFilteredQueryAsync(
        GetVendorListInput input
    )
    {
        var query = await base.CreateFilteredQueryAsync(input);

        // Lower-cased on both sides: lookups search the way Odoo's ilike does, on any provider.
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter) || (x.Name != null && x.Name.ToLower().Contains(filter))
            )
            .WhereIf(input.Blocked.HasValue, x => x.Blocked == input.Blocked.Value);
    }
}
