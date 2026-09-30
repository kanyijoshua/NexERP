using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// The VAT statement and the VAT settlement. Mirrors Business Central page 317 "VAT Statement"
/// and report 20 "Calc. and Post VAT Settlement".
/// </summary>
[Authorize(ErpPermissions.VatEntries.Default)]
public class VatReportingAppService : ErpAppService, IVatReportingAppService
{
    private readonly VatReturnEngine _returnEngine;
    private readonly VatSettlementEngine _settlementEngine;

    public VatReportingAppService(VatReturnEngine returnEngine, VatSettlementEngine settlementEngine)
    {
        _returnEngine = returnEngine;
        _settlementEngine = settlementEngine;
    }

    public async Task<VatReturnDto> CalculateReturnAsync(VatReturnInput input)
    {
        var result = await _returnEngine.CalculateAsync(new VatReturnRequest
        {
            StartingDate = input.StartingDate,
            EndingDate = input.EndingDate,
            Selection = input.Selection,
        });

        return ObjectMapper.Map<VatReturnResult, VatReturnDto>(result);
    }

    [Authorize(ErpPermissions.PeriodicActivities.SettleVat)]
    public async Task<VatSettlementDto> CalculateSettlementAsync(VatSettlementInput input)
    {
        var result = await _settlementEngine.CalculateAsync(ToRequest(input));
        return ObjectMapper.Map<VatSettlementResult, VatSettlementDto>(result);
    }

    [Authorize(ErpPermissions.PeriodicActivities.SettleVat)]
    public async Task<VatSettlementDto> SettleAsync(VatSettlementInput input)
    {
        var result = await _settlementEngine.PostAsync(ToRequest(input));
        return ObjectMapper.Map<VatSettlementResult, VatSettlementDto>(result);
    }

    private static VatSettlementRequest ToRequest(VatSettlementInput input) => new()
    {
        StartingDate = input.StartingDate,
        EndingDate = input.EndingDate,
        PostingDate = input.PostingDate == default ? input.EndingDate : input.PostingDate,
        DocumentNo = input.DocumentNo,
        SettlementAccountNo = input.SettlementAccountNo,
    };
}

/// <summary>
/// Adjust Exchange Rates. Mirrors Business Central report 596 and page 106 "Exch. Rate Adjmt.
/// Register".
/// </summary>
[Authorize(ErpPermissions.PeriodicActivities.Default)]
public class ExchRateAdjustmentAppService : ErpAppService, IExchRateAdjustmentAppService
{
    private readonly ExchRateAdjustmentEngine _engine;
    private readonly IRepository<ExchRateAdjmtRegister, Guid> _registerRepository;

    public ExchRateAdjustmentAppService(ExchRateAdjustmentEngine engine, IRepository<ExchRateAdjmtRegister, Guid> registerRepository)
    {
        _engine = engine;
        _registerRepository = registerRepository;
    }

    [Authorize(ErpPermissions.PeriodicActivities.AdjustExchangeRates)]
    public async Task<ExchRateAdjustmentDto> CalculateAsync(ExchRateAdjustmentInput input)
    {
        var result = await _engine.CalculateAsync(ToRequest(input));
        return ObjectMapper.Map<ExchRateAdjustmentResult, ExchRateAdjustmentDto>(result);
    }

    [Authorize(ErpPermissions.PeriodicActivities.AdjustExchangeRates)]
    public async Task<ExchRateAdjustmentDto> AdjustAsync(ExchRateAdjustmentInput input)
    {
        var result = await _engine.PostAsync(ToRequest(input));
        return ObjectMapper.Map<ExchRateAdjustmentResult, ExchRateAdjustmentDto>(result);
    }

    public async Task<PagedResultDto<ExchRateAdjmtRegisterDto>> GetRegistersAsync(GetExchRateAdjmtRegisterListInput input)
    {
        var filter = input.Filter?.Trim().ToLower();
        var query = (await _registerRepository.GetQueryableAsync())
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter) || x.AccountNo.ToLower().Contains(filter) || x.CurrencyCode.ToLower().Contains(filter)
            );

        var executer = LazyServiceProvider.LazyGetRequiredService<IAsyncQueryableExecuter>();
        var total = await executer.CountAsync(query);
        var items = await executer.ToListAsync(
            query.OrderByDescending(x => x.GLRegisterNo).ThenBy(x => x.AccountNo).PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<ExchRateAdjmtRegisterDto>(total, ObjectMapper.Map<List<ExchRateAdjmtRegister>, List<ExchRateAdjmtRegisterDto>>(items));
    }

    private static ExchRateAdjustmentRequest ToRequest(ExchRateAdjustmentInput input) => new()
    {
        EndingDate = input.EndingDate,
        PostingDate = input.PostingDate == default ? input.EndingDate : input.PostingDate,
        DocumentNo = input.DocumentNo,
        CurrencyCode = input.CurrencyCode,
        AdjustCustomers = input.AdjustCustomers,
        AdjustVendors = input.AdjustVendors,
        AdjustBankAccounts = input.AdjustBankAccounts,
    };
}

/// <summary>Customer Ledger Entries (BC page 25), with their currency and what is still open.</summary>
public class CustomerLedgerEntryAppService
    : ErpReadOnlyAppService<CustomerLedgerEntry, PartyLedgerEntryDto, Guid, GetPartyLedgerEntryListInput>,
        ICustomerLedgerEntryAppService
{
    public CustomerLedgerEntryAppService(IRepository<CustomerLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Customers.Default;
        GetListPolicyName = ErpPermissions.Customers.Default;
    }

    protected override async Task<IQueryable<CustomerLedgerEntry>> CreateFilteredQueryAsync(GetPartyLedgerEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var partyNo = input.PartyNo?.Trim();

        return query
            .WhereIf(!partyNo.IsNullOrEmpty(), x => x.CustomerNo == partyNo)
            .WhereIf(input.OnlyOpen, x => x.Open)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.DocumentNo.ToLower().Contains(filter) || x.CustomerNo.ToLower().Contains(filter));
    }

    // The page shows the customer or vendor no. as the shared "partyNo" column.
    protected override IReadOnlyDictionary<string, string> DynamicFilterAliases { get; } =
        new Dictionary<string, string> { ["partyNo"] = nameof(CustomerLedgerEntry.CustomerNo) };

    protected override IQueryable<CustomerLedgerEntry> ApplyDefaultSorting(IQueryable<CustomerLedgerEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}

/// <summary>Vendor Ledger Entries (BC page 29).</summary>
public class VendorLedgerEntryAppService
    : ErpReadOnlyAppService<VendorLedgerEntry, PartyLedgerEntryDto, Guid, GetPartyLedgerEntryListInput>,
        IVendorLedgerEntryAppService
{
    public VendorLedgerEntryAppService(IRepository<VendorLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Vendors.Default;
        GetListPolicyName = ErpPermissions.Vendors.Default;
    }

    protected override async Task<IQueryable<VendorLedgerEntry>> CreateFilteredQueryAsync(GetPartyLedgerEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var partyNo = input.PartyNo?.Trim();

        return query
            .WhereIf(!partyNo.IsNullOrEmpty(), x => x.VendorNo == partyNo)
            .WhereIf(input.OnlyOpen, x => x.Open)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.DocumentNo.ToLower().Contains(filter) || x.VendorNo.ToLower().Contains(filter));
    }

    // The page shows the customer or vendor no. as the shared "partyNo" column.
    protected override IReadOnlyDictionary<string, string> DynamicFilterAliases { get; } =
        new Dictionary<string, string> { ["partyNo"] = nameof(VendorLedgerEntry.VendorNo) };

    protected override IQueryable<VendorLedgerEntry> ApplyDefaultSorting(IQueryable<VendorLedgerEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}
