using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

public class StandardSalesCodeAppService
    : CodeTableAppServiceBase<StandardSalesCode, StandardSalesCodeDto, CreateUpdateStandardSalesCodeDto>,
        IStandardSalesCodeAppService
{
    public StandardSalesCodeAppService(IRepository<StandardSalesCode, Guid> repository)
        : base(repository, ErpPermissions.StandardCodes.Default) { }

    protected override StandardSalesCode NewEntity(Guid id, CreateUpdateStandardSalesCodeDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(StandardSalesCode entity, CreateUpdateStandardSalesCodeDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<Currency>(input.CurrencyCode);
        entity.SetCurrency(input.CurrencyCode);
    }

    protected override async Task<StandardSalesCode> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    protected override async Task<IQueryable<StandardSalesCode>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.WithDetailsAsync(c => c.Lines);
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), c => c.Code.ToLower().Contains(filter) || (c.Description != null && c.Description.ToLower().Contains(filter)));
    }
}

[Authorize(ErpPermissions.StandardCodes.Default)]
public class StandardSalesLineAppService
    : ErpCrudAppService<StandardSalesLine, StandardSalesLineDto, Guid, GetStandardSalesLineListInput, CreateUpdateStandardSalesLineDto, CreateUpdateStandardSalesLineDto>,
        IStandardSalesLineAppService
{
    private readonly IRepository<StandardSalesCode, Guid> _codes;

    public StandardSalesLineAppService(IRepository<StandardSalesLine, Guid> repository, IRepository<StandardSalesCode, Guid> codes)
        : base(repository)
    {
        _codes = codes;
        GetPolicyName = ErpPermissions.StandardCodes.Default;
        GetListPolicyName = ErpPermissions.StandardCodes.Default;
        CreatePolicyName = ErpPermissions.StandardCodes.Update;
        UpdatePolicyName = ErpPermissions.StandardCodes.Update;
        DeletePolicyName = ErpPermissions.StandardCodes.Update;
    }

    public override async Task<StandardSalesLineDto> CreateAsync(CreateUpdateStandardSalesLineDto input)
    {
        await CheckCreatePolicyAsync();
        var code = await _codes.GetAsync(input.StandardSalesCodeId, includeDetails: true);
        var line = code.AddLine(GuidGenerator.Create(), input.Type, input.No, input.Description, input.Quantity, input.UnitAmount, input.UnitOfMeasureCode);
        await _codes.UpdateAsync(code, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<StandardSalesLineDto> UpdateAsync(Guid id, CreateUpdateStandardSalesLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        line.Set(input.Type, input.No, input.Description, input.Quantity, input.UnitAmount, input.UnitOfMeasureCode);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    protected override async Task<IQueryable<StandardSalesLine>> CreateFilteredQueryAsync(GetStandardSalesLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.StandardSalesCodeId.HasValue, l => l.StandardSalesCodeId == input.StandardSalesCodeId.Value);
    }

    protected override IQueryable<StandardSalesLine> ApplyDefaultSorting(IQueryable<StandardSalesLine> query) => query.OrderBy(l => l.LineNo);
}

[Authorize(ErpPermissions.StandardCodes.Default)]
public class StandardCustomerSalesCodeAppService
    : ErpTableAppService<StandardCustomerSalesCode, StandardCustomerSalesCodeDto, GetStandardCustomerSalesCodeListInput, CreateUpdateStandardCustomerSalesCodeDto>,
        IStandardCustomerSalesCodeAppService
{
    public StandardCustomerSalesCodeAppService(IRepository<StandardCustomerSalesCode, Guid> repository)
        : base(repository, ErpPermissions.StandardCodes.Default) { }

    public override async Task<StandardCustomerSalesCodeDto> CreateAsync(CreateUpdateStandardCustomerSalesCodeDto input)
    {
        await CheckCreatePolicyAsync();
        var customerNo = input.CustomerNo.Trim();
        if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().AnyAsync(c => c.No == customerNo))
        {
            throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", customerNo);
        }

        await CodeTableChecker.EnsureExistsAsync<StandardSalesCode>(input.Code);
        var code = CodeTableEntity.NormalizeCode(input.Code);
        if (await Repository.AnyAsync(c => c.CustomerNo == customerNo && c.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeAlreadyExists).WithData("code", code);
        }

        var entity = new StandardCustomerSalesCode(GuidGenerator.Create(), customerNo, code);
        entity.Set(input.InsertOnQuotes, input.InsertOnOrders, input.InsertOnInvoices);
        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>Only when the lines go on new documents changes; another customer or code is a new record.</summary>
    public override async Task<StandardCustomerSalesCodeDto> UpdateAsync(Guid id, CreateUpdateStandardCustomerSalesCodeDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await GetEntityByIdAsync(id);
        entity.Set(input.InsertOnQuotes, input.InsertOnOrders, input.InsertOnInvoices);
        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<StandardCustomerSalesCode>> CreateFilteredQueryAsync(GetStandardCustomerSalesCodeListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var customerNo = input.CustomerNo?.Trim();
        return query.WhereIf(!customerNo.IsNullOrEmpty(), c => c.CustomerNo == customerNo);
    }

    protected override IQueryable<StandardCustomerSalesCode> ApplyDefaultSorting(IQueryable<StandardCustomerSalesCode> query) =>
        query.OrderBy(c => c.CustomerNo).ThenBy(c => c.Code);
}
