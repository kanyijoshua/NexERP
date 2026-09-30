using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Payment Terms (BC page 4).</summary>
[Authorize(ErpPermissions.FinanceSetup.Default)]
public class PaymentTermsAppService
    : CodeTableAppServiceBase<PaymentTerms, PaymentTermsDto, CreateUpdatePaymentTermsDto>,
        IPaymentTermsAppService
{
    public PaymentTermsAppService(IRepository<PaymentTerms, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override PaymentTerms NewEntity(Guid id, CreateUpdatePaymentTermsDto input) =>
        new(id, input.Code, input.Description, input.DueDateCalculation, input.DiscountDateCalculation, input.DiscountPercent);

    protected override Task ApplyAsync(PaymentTerms entity, CreateUpdatePaymentTermsDto input)
    {
        entity.SetCalculations(input.DueDateCalculation, input.DiscountDateCalculation, input.DiscountPercent);
        return Task.CompletedTask;
    }
}

/// <summary>Currencies (BC page 5).</summary>
[Authorize(ErpPermissions.FinanceSetup.Default)]
public class CurrencyAppService
    : CodeTableAppServiceBase<Currency, CurrencyDto, CreateUpdateCurrencyDto>,
        ICurrencyAppService
{
    public CurrencyAppService(IRepository<Currency, Guid> repository)
        : base(repository, ErpPermissions.FinanceSetup.Default) { }

    protected override Currency NewEntity(Guid id, CreateUpdateCurrencyDto input) =>
        new(id, input.Code, input.Description, input.Symbol);

    protected override async Task ApplyAsync(Currency entity, CreateUpdateCurrencyDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>()
            .EnsureGLAccountsExistAsync(
                input.RealizedGainsAccountNo,
                input.RealizedLossesAccountNo,
                input.UnrealizedGainsAccountNo,
                input.UnrealizedLossesAccountNo
            );

        entity.SetSymbol(input.Symbol);
        entity.SetAmountRoundingPrecision(input.AmountRoundingPrecision);
        entity.SetGainLossAccounts(input.RealizedGainsAccountNo, input.RealizedLossesAccountNo);
        entity.SetUnrealizedAccounts(input.UnrealizedGainsAccountNo, input.UnrealizedLossesAccountNo);
    }
}

/// <summary>Currency Exchange Rates (BC page 483): one rate per currency and starting date.</summary>
[Authorize(ErpPermissions.FinanceSetup.Default)]
public class CurrencyExchangeRateAppService
    : ErpCrudAppService<
        CurrencyExchangeRate,
        CurrencyExchangeRateDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateCurrencyExchangeRateDto,
        CreateUpdateCurrencyExchangeRateDto
    >,
        ICurrencyExchangeRateAppService
{
    private readonly CodeTableChecker _codeTableChecker;

    public CurrencyExchangeRateAppService(IRepository<CurrencyExchangeRate, Guid> repository, CodeTableChecker codeTableChecker)
        : base(repository)
    {
        _codeTableChecker = codeTableChecker;
        GetPolicyName = ErpPermissions.FinanceSetup.Default;
        GetListPolicyName = ErpPermissions.FinanceSetup.Default;
        CreatePolicyName = ErpPermissions.FinanceSetup.Create;
        UpdatePolicyName = ErpPermissions.FinanceSetup.Update;
        DeletePolicyName = ErpPermissions.FinanceSetup.Delete;
    }

    public override async Task<CurrencyExchangeRateDto> CreateAsync(CreateUpdateCurrencyExchangeRateDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);

        var rate = new CurrencyExchangeRate(
            GuidGenerator.Create(),
            input.CurrencyCode,
            input.StartingDate,
            input.ExchangeRateAmount,
            input.RelationalExchangeRateAmount
        );

        await Repository.InsertAsync(rate, autoSave: true);
        return await MapToGetOutputDtoAsync(rate);
    }

    public override async Task<CurrencyExchangeRateDto> UpdateAsync(Guid id, CreateUpdateCurrencyExchangeRateDto input)
    {
        await CheckUpdatePolicyAsync();

        var rate = await GetEntityByIdAsync(id);
        await ValidateAsync(input, id);
        rate.Set(input.CurrencyCode, input.StartingDate, input.ExchangeRateAmount, input.RelationalExchangeRateAmount);

        await Repository.UpdateAsync(rate, autoSave: true);
        return await MapToGetOutputDtoAsync(rate);
    }

    protected override async Task<IQueryable<CurrencyExchangeRate>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(!filter.IsNullOrEmpty(), x => x.CurrencyCode.ToLower().Contains(filter));
    }

    protected override IQueryable<CurrencyExchangeRate> ApplyDefaultSorting(IQueryable<CurrencyExchangeRate> query)
    {
        return query.OrderBy(x => x.CurrencyCode).ThenByDescending(x => x.StartingDate);
    }

    private async Task ValidateAsync(CreateUpdateCurrencyExchangeRateDto input, Guid? exceptId)
    {
        await _codeTableChecker.EnsureExistsAsync<Currency>(input.CurrencyCode);

        var code = CodeTableEntity.NormalizeCode(input.CurrencyCode);
        var date = input.StartingDate.Date;
        if (await Repository.AnyAsync(x => x.CurrencyCode == code && x.StartingDate == date && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.ExchangeRateAlreadyExists)
                .WithData("currencyCode", code)
                .WithData("startingDate", date.ToString("yyyy-MM-dd"));
        }
    }
}

/// <summary>Accounting Periods (BC page 100) with Create Year and Close Year.</summary>
[Authorize(ErpPermissions.FinanceSetup.Default)]
public class AccountingPeriodAppService : ErpAppService, IAccountingPeriodAppService
{
    private readonly IRepository<AccountingPeriod, Guid> _repository;
    private readonly AccountingPeriodManager _manager;

    public AccountingPeriodAppService(IRepository<AccountingPeriod, Guid> repository, AccountingPeriodManager manager)
    {
        _repository = repository;
        _manager = manager;
    }

    public async Task<PagedResultDto<AccountingPeriodDto>> GetListAsync(GetAccountingPeriodListInput input)
    {
        var query = await _repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        query = query.WhereIf(!filter.IsNullOrEmpty(), p => p.Name.ToLower().Contains(filter));
        query = ErpListQuery.Filter(query, input, Clock.Now, null);

        var total = await AsyncExecuter.CountAsync(query);
        query = input.Sorting.IsNullOrWhiteSpace()
            ? query.OrderBy(p => p.StartingDate)
            : DynamicQueryableExtensions.OrderBy(query, input.Sorting);
        var periods = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<AccountingPeriodDto>(total, ObjectMapper.Map<List<AccountingPeriod>, List<AccountingPeriodDto>>(periods));
    }

    [Authorize(ErpPermissions.FinanceSetup.Create)]
    public async Task<ListResultDto<AccountingPeriodDto>> NewFiscalYearAsync(NewFiscalYearDto input)
    {
        var created = await _manager.CreateFiscalYearAsync(input.StartingDate, input.NoOfPeriods, input.PeriodLength);
        return new ListResultDto<AccountingPeriodDto>(ObjectMapper.Map<List<AccountingPeriod>, List<AccountingPeriodDto>>(created));
    }

    [Authorize(ErpPermissions.FinanceSetup.Update)]
    public async Task<FiscalYearClosedDto> CloseFiscalYearAsync()
    {
        var (from, to) = await _manager.CloseFiscalYearAsync();
        return new FiscalYearClosedDto { FromDate = from, ToDate = to };
    }

    /// <summary>A period of a closed year stays, as BC keeps closed periods.</summary>
    [Authorize(ErpPermissions.FinanceSetup.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var period = await _repository.GetAsync(id);
        if (period.Closed)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.PeriodClosed)
                .WithData("startingDate", period.StartingDate.ToString("yyyy-MM-dd"));
        }

        await _repository.DeleteAsync(period, autoSave: true);
    }
}
