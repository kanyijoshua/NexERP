using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

[Authorize(ErpPermissions.Vendors.Default)]
public class VendorAppService
    : CrudAppService<
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

    private static void ApplyInput(Vendor vendor, CreateUpdateVendorDto input)
    {
        vendor.SetAddress(input.Address, input.City, input.PostCode, input.CountryRegionCode);
        vendor.SetContact(input.PhoneNo, input.Email);
        vendor.SetPaymentTerms(input.PaymentTermsCode);
        vendor.SetPostingGroups(input.VendorPostingGroup, input.GenBusPostingGroup);
        vendor.SetCurrency(input.CurrencyCode);
    }

    protected override async Task<IQueryable<Vendor>> CreateFilteredQueryAsync(
        GetVendorListInput input
    )
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(
                !input.Filter.IsNullOrWhiteSpace(),
                x => x.No.Contains(input.Filter) || x.Name.Contains(input.Filter)
            )
            .WhereIf(input.Blocked.HasValue, x => x.Blocked == input.Blocked.Value);
    }
}
