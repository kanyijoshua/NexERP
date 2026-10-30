using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

public class StandardPurchaseCodeAppService
    : CodeTableAppServiceBase<StandardPurchaseCode, StandardPurchaseCodeDto, CreateUpdateStandardPurchaseCodeDto>,
        IStandardPurchaseCodeAppService
{
    public StandardPurchaseCodeAppService(IRepository<StandardPurchaseCode, Guid> repository)
        : base(repository, ErpPermissions.StandardCodes.Default) { }

    protected override StandardPurchaseCode NewEntity(Guid id, CreateUpdateStandardPurchaseCodeDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(StandardPurchaseCode entity, CreateUpdateStandardPurchaseCodeDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<Currency>(input.CurrencyCode);
        entity.SetCurrency(input.CurrencyCode);
    }

    protected override async Task<StandardPurchaseCode> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    protected override async Task<IQueryable<StandardPurchaseCode>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.WithDetailsAsync(c => c.Lines);
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), c => c.Code.ToLower().Contains(filter) || (c.Description != null && c.Description.ToLower().Contains(filter)));
    }
}

[Authorize(ErpPermissions.StandardCodes.Default)]
public class StandardPurchaseLineAppService
    : ErpCrudAppService<StandardPurchaseLine, StandardPurchaseLineDto, Guid, GetStandardPurchaseLineListInput, CreateUpdateStandardPurchaseLineDto, CreateUpdateStandardPurchaseLineDto>,
        IStandardPurchaseLineAppService
{
    private readonly IRepository<StandardPurchaseCode, Guid> _codes;

    public StandardPurchaseLineAppService(IRepository<StandardPurchaseLine, Guid> repository, IRepository<StandardPurchaseCode, Guid> codes)
        : base(repository)
    {
        _codes = codes;
        GetPolicyName = ErpPermissions.StandardCodes.Default;
        GetListPolicyName = ErpPermissions.StandardCodes.Default;
        CreatePolicyName = ErpPermissions.StandardCodes.Update;
        UpdatePolicyName = ErpPermissions.StandardCodes.Update;
        DeletePolicyName = ErpPermissions.StandardCodes.Update;
    }

    public override async Task<StandardPurchaseLineDto> CreateAsync(CreateUpdateStandardPurchaseLineDto input)
    {
        await CheckCreatePolicyAsync();
        var code = await _codes.GetAsync(input.StandardPurchaseCodeId, includeDetails: true);
        var line = code.AddLine(GuidGenerator.Create(), input.Type, input.No, input.Description, input.Quantity, input.UnitAmount, input.UnitOfMeasureCode);
        await _codes.UpdateAsync(code, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<StandardPurchaseLineDto> UpdateAsync(Guid id, CreateUpdateStandardPurchaseLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        line.Set(input.Type, input.No, input.Description, input.Quantity, input.UnitAmount, input.UnitOfMeasureCode);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    protected override async Task<IQueryable<StandardPurchaseLine>> CreateFilteredQueryAsync(GetStandardPurchaseLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.StandardPurchaseCodeId.HasValue, l => l.StandardPurchaseCodeId == input.StandardPurchaseCodeId.Value);
    }

    protected override IQueryable<StandardPurchaseLine> ApplyDefaultSorting(IQueryable<StandardPurchaseLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.StandardCodes.Default)]
public class StandardVendorPurchaseCodeAppService
    : ErpTableAppService<StandardVendorPurchaseCode, StandardVendorPurchaseCodeDto, GetStandardVendorPurchaseCodeListInput, CreateUpdateStandardVendorPurchaseCodeDto>,
        IStandardVendorPurchaseCodeAppService
{
    public StandardVendorPurchaseCodeAppService(IRepository<StandardVendorPurchaseCode, Guid> repository)
        : base(repository, ErpPermissions.StandardCodes.Default) { }

    public override async Task<StandardVendorPurchaseCodeDto> CreateAsync(CreateUpdateStandardVendorPurchaseCodeDto input)
    {
        await CheckCreatePolicyAsync();
        var vendorNo = input.VendorNo.Trim();
        if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<Vendor, Guid>>().AnyAsync(c => c.No == vendorNo))
        {
            throw new BusinessException(ErpErrorCodes.Vendors.VendorNotFound).WithData("accountNo", vendorNo);
        }

        await CodeTableChecker.EnsureExistsAsync<StandardPurchaseCode>(input.Code);
        var code = CodeTableEntity.NormalizeCode(input.Code);
        if (await Repository.AnyAsync(c => c.VendorNo == vendorNo && c.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeAlreadyExists).WithData("code", code);
        }

        var entity = new StandardVendorPurchaseCode(GuidGenerator.Create(), vendorNo, code);
        entity.Set(input.InsertOnQuotes, input.InsertOnOrders, input.InsertOnInvoices);
        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>Only when the lines go on new documents changes; another vendor or code is a new record.</summary>
    public override async Task<StandardVendorPurchaseCodeDto> UpdateAsync(Guid id, CreateUpdateStandardVendorPurchaseCodeDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await GetEntityByIdAsync(id);
        entity.Set(input.InsertOnQuotes, input.InsertOnOrders, input.InsertOnInvoices);
        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<StandardVendorPurchaseCode>> CreateFilteredQueryAsync(GetStandardVendorPurchaseCodeListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var vendorNo = input.VendorNo?.Trim();
        return query.WhereIf(!vendorNo.IsNullOrEmpty(), c => c.VendorNo == vendorNo);
    }

    protected override IQueryable<StandardVendorPurchaseCode> ApplyDefaultSorting(IQueryable<StandardVendorPurchaseCode> query) =>
        query.OrderBy(c => c.VendorNo).ThenBy(c => c.Code);
}
