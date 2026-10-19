using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Pricing;

/// <summary>Sales and purchase price lists, and the price a document line gets from them.</summary>
[Authorize]
public class PriceListAppService
    : ErpCrudAppService<PriceList, PriceListDto, Guid, GetPriceListListInput, CreateUpdatePriceListDto, CreateUpdatePriceListDto>,
        IPriceListAppService
{
    private readonly PriceCalculator _calculator;

    public PriceListAppService(IRepository<PriceList, Guid> repository, PriceCalculator calculator)
        : base(repository)
    {
        _calculator = calculator;
        GetPolicyName = ErpPermissions.PriceLists.Default;
        GetListPolicyName = ErpPermissions.PriceLists.Default;
        CreatePolicyName = ErpPermissions.PriceLists.Create;
        UpdatePolicyName = ErpPermissions.PriceLists.Update;
        DeletePolicyName = ErpPermissions.PriceLists.Delete;
    }

    public override async Task<PriceListDto> CreateAsync(CreateUpdatePriceListDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureCodeIsUniqueAsync(input.Code, null);

        var list = new PriceList(GuidGenerator.Create(), input.Code, input.PriceType);
        await ApplyAsync(list, input);

        await Repository.InsertAsync(list, autoSave: true);
        return await MapToGetOutputDtoAsync(list);
    }

    public override async Task<PriceListDto> UpdateAsync(Guid id, CreateUpdatePriceListDto input)
    {
        await CheckUpdatePolicyAsync();
        await EnsureCodeIsUniqueAsync(input.Code, id);

        var list = await GetEntityByIdAsync(id);
        list.SetCode(input.Code);
        await ApplyAsync(list, input);

        await Repository.UpdateAsync(list, autoSave: true);
        return await MapToGetOutputDtoAsync(list);
    }

    /// <summary>Any user who can work with sales or purchase documents may ask for a price.</summary>
    public async Task<PriceQuoteDto> GetPriceQuoteAsync(GetPriceInput input)
    {
        await AuthorizationService.CheckAsync(input.PriceType == PriceType.Sale ? ErpPermissions.SalesDocuments.Default : ErpPermissions.PurchaseDocuments.Default);

        var quote = await _calculator.FindPriceAsync(new PriceQuery
        {
            PriceType = input.PriceType,
            PartyNo = input.PartyNo,
            CustomerPriceGroup = input.PriceType == PriceType.Sale ? await PriceGroupOfAsync(input.PartyNo) : null,
            ItemNo = input.ItemNo,
            UnitOfMeasureCode = input.UnitOfMeasureCode,
            Quantity = input.Quantity,
            CurrencyCode = input.CurrencyCode,
            Date = input.Date ?? Clock.Now.Date,
        });

        return new PriceQuoteDto { UnitPrice = quote.UnitPrice, LineDiscountPercent = quote.LineDiscountPercent, PriceListCode = quote.PriceListCode };
    }

    private async Task<string> PriceGroupOfAsync(string customerNo)
    {
        if (customerNo.IsNullOrWhiteSpace())
        {
            return null;
        }

        var customers = LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>();
        var no = customerNo.Trim();
        return (await customers.FirstOrDefaultAsync(c => c.No == no))?.CustomerPriceGroup;
    }

    private async Task ApplyAsync(PriceList list, CreateUpdatePriceListDto input)
    {
        await CheckSourceAsync(input.SourceType, input.SourceNo);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<Currency>(input.CurrencyCode);

        list.SetDescription(input.Description);
        list.SetSource(input.SourceType, input.SourceNo);
        list.SetCurrency(input.CurrencyCode);
        list.SetPeriod(input.StartingDate, input.EndingDate);
        list.SetStatus(input.Status);
    }

    /// <summary>The customer, vendor or price group a list is for has to exist.</summary>
    private async Task CheckSourceAsync(PriceSourceType sourceType, string sourceNo)
    {
        if (sourceNo.IsNullOrWhiteSpace())
        {
            return;
        }

        var no = sourceNo.Trim().ToUpperInvariant();
        var exists = sourceType switch
        {
            PriceSourceType.Customer => await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().AnyAsync(c => c.No == no),
            PriceSourceType.Vendor => await LazyServiceProvider.LazyGetRequiredService<IRepository<Vendor, Guid>>().AnyAsync(v => v.No == no),
            PriceSourceType.CustomerPriceGroup => await LazyServiceProvider.LazyGetRequiredService<IRepository<CustomerPriceGroup, Guid>>().AnyAsync(g => g.Code == no),
            _ => true,
        };
        if (!exists)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound)
                .WithData("table", sourceType.ToString())
                .WithData("code", no);
        }
    }

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? id)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(x => x.Code == normalized && x.Id != id))
        {
            throw new BusinessException(ErpErrorCodes.Pricing.PriceListCodeAlreadyExists).WithData("code", normalized);
        }
    }

    protected override async Task<IQueryable<PriceList>> CreateFilteredQueryAsync(GetPriceListListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!filter.IsNullOrEmpty(), x => x.Code.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter))
                || (x.SourceNo != null && x.SourceNo.ToLower().Contains(filter)))
            .WhereIf(input.PriceType.HasValue, x => x.PriceType == input.PriceType.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value);
    }

    protected override IQueryable<PriceList> ApplyDefaultSorting(IQueryable<PriceList> query)
    {
        return query.OrderBy(x => x.Code);
    }
}

/// <summary>The lines of a price list; changed through the list, so an active list stays fixed.</summary>
[Authorize(ErpPermissions.PriceLists.Default)]
public class PriceListLineAppService
    : ErpCrudAppService<PriceListLine, PriceListLineDto, Guid, GetPriceListLineListInput, CreateUpdatePriceListLineDto, CreateUpdatePriceListLineDto>,
        IPriceListLineAppService
{
    private readonly IRepository<PriceList, Guid> _lists;

    public PriceListLineAppService(IRepository<PriceListLine, Guid> repository, IRepository<PriceList, Guid> lists)
        : base(repository)
    {
        _lists = lists;
        GetPolicyName = ErpPermissions.PriceLists.Default;
        GetListPolicyName = ErpPermissions.PriceLists.Default;
        CreatePolicyName = ErpPermissions.PriceLists.Update;
        UpdatePolicyName = ErpPermissions.PriceLists.Update;
        DeletePolicyName = ErpPermissions.PriceLists.Update;
    }

    public override async Task<PriceListLineDto> CreateAsync(CreateUpdatePriceListLineDto input)
    {
        await CheckCreatePolicyAsync();

        var list = await _lists.GetAsync(input.PriceListId);
        var line = list.AddLine(GuidGenerator.Create(), input.AssetType, input.AssetNo);
        line.SetPrice(input.Description, input.UnitOfMeasureCode, input.MinimumQuantity, input.UnitPrice, input.LineDiscountPercent);

        await _lists.UpdateAsync(list, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<PriceListLineDto> UpdateAsync(Guid id, CreateUpdatePriceListLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var existing = await GetEntityByIdAsync(id);
        var list = await _lists.GetAsync(existing.PriceListId);
        list.EnsureEditable();

        var line = list.Lines.First(l => l.Id == id);
        line.SetAsset(input.AssetType, input.AssetNo);
        line.SetPrice(input.Description, input.UnitOfMeasureCode, input.MinimumQuantity, input.UnitPrice, input.LineDiscountPercent);

        await _lists.UpdateAsync(list, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var existing = await GetEntityByIdAsync(id);
        var list = await _lists.GetAsync(existing.PriceListId);
        list.RemoveLine(id);
        await _lists.UpdateAsync(list, autoSave: true);
    }

    protected override async Task<IQueryable<PriceListLine>> CreateFilteredQueryAsync(GetPriceListLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(input.PriceListId.HasValue, x => x.PriceListId == input.PriceListId.Value)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.AssetNo.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<PriceListLine> ApplyDefaultSorting(IQueryable<PriceListLine> query)
    {
        return query.OrderBy(x => x.LineNo);
    }
}

/// <summary>Customer price groups: customers that share a sales price list.</summary>
public class CustomerPriceGroupAppService
    : CodeTableAppServiceBase<CustomerPriceGroup, CustomerPriceGroupDto, CreateUpdateCustomerPriceGroupDto>,
        ICustomerPriceGroupAppService
{
    public CustomerPriceGroupAppService(IRepository<CustomerPriceGroup, Guid> repository)
        : base(repository, ErpPermissions.PriceLists.Default) { }

    protected override CustomerPriceGroup NewEntity(Guid id, CreateUpdateCustomerPriceGroupDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(CustomerPriceGroup entity, CreateUpdateCustomerPriceGroupDto input)
    {
        entity.SetAllowLineDiscount(input.AllowLineDiscount);
        return Task.CompletedTask;
    }
}
