using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Applying posted customer and vendor entries to each other, and undoing it.</summary>
[Authorize(ErpPermissions.EntryApplication.Default)]
public class EntryApplicationAppService : ErpAppService, IEntryApplicationAppService
{
    private readonly EntryApplicationManager _manager;
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerEntries;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorEntries;

    public EntryApplicationAppService(
        EntryApplicationManager manager,
        IRepository<CustomerLedgerEntry, Guid> customerEntries,
        IRepository<VendorLedgerEntry, Guid> vendorEntries
    )
    {
        _manager = manager;
        _customerEntries = customerEntries;
        _vendorEntries = vendorEntries;
    }

    public async Task<List<PartyLedgerEntryDto>> GetApplicableEntriesAsync(GetApplicableEntriesInput input)
    {
        if (input.PartyType == PartyLedgerType.Customer)
        {
            var applying = await _customerEntries.GetAsync(input.ApplyingEntryId);
            var entries = await _customerEntries.GetListAsync(e => e.CustomerId == applying.CustomerId && e.Open && e.Id != applying.Id);
            return ObjectMapper.Map<List<CustomerLedgerEntry>, List<PartyLedgerEntryDto>>(
                entries.Where(e => Math.Sign(e.RemainingAmount) != Math.Sign(applying.RemainingAmount) && e.CurrencyCode == applying.CurrencyCode).OrderBy(e => e.DueDate).ThenBy(e => e.EntryNo).ToList());
        }

        var vendorApplying = await _vendorEntries.GetAsync(input.ApplyingEntryId);
        var vendorEntries = await _vendorEntries.GetListAsync(e => e.VendorId == vendorApplying.VendorId && e.Open && e.Id != vendorApplying.Id);
        return ObjectMapper.Map<List<VendorLedgerEntry>, List<PartyLedgerEntryDto>>(
            vendorEntries.Where(e => Math.Sign(e.RemainingAmount) != Math.Sign(vendorApplying.RemainingAmount) && e.CurrencyCode == vendorApplying.CurrencyCode).OrderBy(e => e.DueDate).ThenBy(e => e.EntryNo).ToList());
    }

    public async Task<ApplicationResultDto> ApplyAsync(ApplyEntriesInput input)
    {
        var applicationNo = await _manager.ApplyPostedEntriesAsync(
            input.PartyType,
            input.ApplyingEntryId,
            input.Entries.Select(e => new EntryToApply(e.EntryId, e.AmountToApply)).ToList(),
            input.PostingDate
        );
        await CurrentUnitOfWork.SaveChangesAsync();

        return new ApplicationResultDto { ApplicationNo = applicationNo, ApplyingEntry = await EntryAsync(input.PartyType, input.ApplyingEntryId) };
    }

    [Authorize(ErpPermissions.EntryApplication.Unapply)]
    public async Task<int> UnapplyAsync(UnapplyEntryInput input)
    {
        return await _manager.UnapplyAsync(input.PartyType, input.EntryId, input.PostingDate);
    }

    public async Task<List<DetailedLedgerEntryDto>> GetDetailedEntriesAsync(GetDetailedEntriesInput input)
    {
        if (input.PartyType == PartyLedgerType.Customer)
        {
            var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<DetailedCustLedgEntry, Guid>>();
            var details = await repository.GetListAsync(d => d.CustLedgerEntryNo == input.LedgerEntryNo);
            return details.OrderBy(d => d.EntryNo).Select(d => new DetailedLedgerEntryDto
            {
                Id = d.Id, EntryNo = d.EntryNo, LedgerEntryNo = d.CustLedgerEntryNo, EntryType = d.EntryType, PostingDate = d.PostingDate,
                DocumentType = d.DocumentType, DocumentNo = d.DocumentNo, Amount = d.Amount, AmountLcy = d.AmountLcy,
                AppliedEntryNo = d.AppliedCustLedgerEntryNo, ApplicationNo = d.ApplicationNo, Unapplied = d.Unapplied,
            }).ToList();
        }

        var vendorRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<DetailedVendorLedgEntry, Guid>>();
        var vendorDetails = await vendorRepository.GetListAsync(d => d.VendorLedgerEntryNo == input.LedgerEntryNo);
        return vendorDetails.OrderBy(d => d.EntryNo).Select(d => new DetailedLedgerEntryDto
        {
            Id = d.Id, EntryNo = d.EntryNo, LedgerEntryNo = d.VendorLedgerEntryNo, EntryType = d.EntryType, PostingDate = d.PostingDate,
            DocumentType = d.DocumentType, DocumentNo = d.DocumentNo, Amount = d.Amount, AmountLcy = d.AmountLcy,
            AppliedEntryNo = d.AppliedVendLedgerEntryNo, ApplicationNo = d.ApplicationNo, Unapplied = d.Unapplied,
        }).ToList();
    }

    private async Task<PartyLedgerEntryDto> EntryAsync(PartyLedgerType type, Guid id) =>
        type == PartyLedgerType.Customer
            ? ObjectMapper.Map<CustomerLedgerEntry, PartyLedgerEntryDto>(await _customerEntries.GetAsync(id))
            : ObjectMapper.Map<VendorLedgerEntry, PartyLedgerEntryDto>(await _vendorEntries.GetAsync(id));
}

/// <summary>The bank reconciliation worksheet: import, match, post the differences and post the statement.</summary>
[Authorize(ErpPermissions.BankReconciliations.Default)]
public class BankReconciliationAppService : ErpAppService, IBankReconciliationAppService
{
    private readonly BankReconciliationManager _manager;
    private readonly IRepository<BankAccReconciliation, Guid> _repository;

    public BankReconciliationAppService(BankReconciliationManager manager, IRepository<BankAccReconciliation, Guid> repository)
    {
        _manager = manager;
        _repository = repository;
    }

    public async Task<BankRecWorksheetDto> GetWorksheetAsync(Guid id)
    {
        var rec = await _repository.GetAsync(id);
        var lines = await _manager.GetLinesAsync(rec);
        var entries = await _manager.GetCandidateEntriesAsync(rec);
        var total = lines.Sum(l => l.StatementAmount);

        return new BankRecWorksheetDto
        {
            Id = rec.Id,
            BankAccountNo = rec.BankAccountNo,
            StatementNo = rec.StatementNo,
            StatementDate = rec.StatementDate,
            BalanceLastStatement = rec.BalanceLastStatement,
            StatementEndingBalance = rec.StatementEndingBalance,
            TotalStatementAmount = total,
            Difference = rec.StatementEndingBalance - rec.BalanceLastStatement - total,
            Lines = lines.Select(l => new BankRecLineDto
            {
                Id = l.Id, StatementLineNo = l.StatementLineNo, TransactionDate = l.TransactionDate, DocumentNo = l.DocumentNo,
                Description = l.Description, StatementAmount = l.StatementAmount, AppliedAmount = l.AppliedAmount, Difference = l.Difference,
                AccountType = l.AccountType, AccountNo = l.AccountNo,
            }).ToList(),
            Entries = entries.Select(e => new BankRecEntryDto
            {
                Id = e.Id, EntryNo = e.EntryNo, PostingDate = e.PostingDate, DocumentNo = e.DocumentNo, Description = e.Description,
                Amount = e.Amount, StatementStatus = e.StatementStatus, StatementLineNo = e.StatementLineNo,
            }).ToList(),
        };
    }

    [Authorize(ErpPermissions.BankReconciliations.Update)]
    public Task<int> ImportStatementAsync(Guid id, ImportBankStatementInput input) => _manager.ImportStatementAsync(id, input.Content);

    [Authorize(ErpPermissions.BankReconciliations.Update)]
    public Task<int> MatchAutomaticallyAsync(Guid id, MatchAutomaticallyInput input) => _manager.MatchAutomaticallyAsync(id, input.DateTolerance);

    [Authorize(ErpPermissions.BankReconciliations.Update)]
    public Task MatchManuallyAsync(Guid id, MatchManuallyInput input) => _manager.MatchManuallyAsync(id, input.LineId, input.EntryIds);

    [Authorize(ErpPermissions.BankReconciliations.Update)]
    public Task UnmatchAsync(Guid id, RemoveMatchInput input) => _manager.RemoveMatchAsync(id, input.LineId);

    [Authorize(ErpPermissions.BankReconciliationPosting.Default)]
    public Task<int> TransferDifferencesAsync(Guid id) => _manager.PostDifferencesAsync(id);

    [Authorize(ErpPermissions.BankReconciliationPosting.Default)]
    public async Task<string> RunPostingAsync(Guid id) => (await _manager.PostAsync(id)).StatementNo;
}

/// <summary>Reminders and finance charge memos share one service shape; the two concrete services pick the table.</summary>
public abstract class CollectionDocumentAppServiceBase<THeader> : ErpAppService, ICollectionDocumentAppService
    where THeader : CollectionDocument
{
    protected IRepository<THeader, Guid> Repository { get; }
    protected CollectionDocumentManager Manager => LazyServiceProvider.LazyGetRequiredService<CollectionDocumentManager>();

    protected abstract string DefaultPolicy { get; }
    protected abstract string CreatePolicy { get; }
    protected abstract string UpdatePolicy { get; }
    protected abstract string DeletePolicy { get; }
    protected abstract string IssuePolicy { get; }

    protected CollectionDocumentAppServiceBase(IRepository<THeader, Guid> repository)
    {
        Repository = repository;
    }

    public async Task<CollectionDocumentDto> GetAsync(Guid id)
    {
        await AuthorizationService.CheckAsync(DefaultPolicy);
        return Map(await Repository.GetAsync(id));
    }

    public async Task<PagedResultDto<CollectionDocumentDto>> GetListAsync(GetCollectionDocumentListInput input)
    {
        await AuthorizationService.CheckAsync(DefaultPolicy);
        var filter = input.Filter?.Trim().ToLower();
        var query = (await Repository.WithDetailsAsync())
            .WhereIf(!input.CustomerNo.IsNullOrWhiteSpace(), d => d.CustomerNo == input.CustomerNo)
            .WhereIf(input.Status.HasValue, d => d.Status == input.Status.Value)
            .WhereIf(!filter.IsNullOrEmpty(), d => d.No.ToLower().Contains(filter) || d.CustomerNo.ToLower().Contains(filter) || (d.CustomerName != null && d.CustomerName.ToLower().Contains(filter)));
        query = ErpListQuery.Filter(query, input, Clock.Now, null);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderByDescending(d => d.DocumentDate).ThenByDescending(d => d.No).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<CollectionDocumentDto>(total, items.Select(Map).ToList());
    }

    public async Task<CollectionDocumentDto> UpdateAsync(Guid id, UpdateCollectionDocumentDto input)
    {
        await AuthorizationService.CheckAsync(UpdatePolicy);
        var document = await Repository.GetAsync(id);
        document.SetDates(input.PostingDate, input.DocumentDate, input.DueDate);
        await Repository.UpdateAsync(document, autoSave: true);
        return Map(document);
    }

    public async Task DeleteAsync(Guid id)
    {
        await AuthorizationService.CheckAsync(DeletePolicy);
        var document = await Repository.GetAsync(id);
        document.EnsureOpen();
        await Repository.DeleteAsync(document);
    }

    public async Task<int> SuggestAsync(SuggestCollectionDocumentsInput input)
    {
        await AuthorizationService.CheckAsync(CreatePolicy);
        return await SuggestCoreAsync(input.DocumentDate == default ? Clock.Now.Date : input.DocumentDate, input.CustomerNo);
    }

    public async Task<CollectionDocumentDto> IssueAsync(Guid id, IssueCollectionDocumentInput input)
    {
        await AuthorizationService.CheckAsync(IssuePolicy);
        await IssueCoreAsync(id, input.PostingDate);
        return Map(await Repository.GetAsync(id));
    }

    protected abstract Task<int> SuggestCoreAsync(DateTime documentDate, string customerNo);

    protected abstract Task IssueCoreAsync(Guid id, DateTime? postingDate);

    protected abstract (string TermsCode, int Level) Terms(THeader header);

    private CollectionDocumentDto Map(THeader header)
    {
        var (terms, level) = Terms(header);
        return new CollectionDocumentDto
        {
            Id = header.Id,
            No = header.No,
            CustomerId = header.CustomerId,
            CustomerNo = header.CustomerNo,
            CustomerName = header.CustomerName,
            PostingDate = header.PostingDate,
            DocumentDate = header.DocumentDate,
            DueDate = header.DueDate,
            Status = header.Status,
            IssuedNo = header.IssuedNo,
            IssuedDate = header.IssuedDate,
            TermsCode = terms,
            ReminderLevel = level,
            RemainingAmount = header.RemainingAmount,
            TotalAmount = header.TotalAmount,
            Lines = header.CollectionLines.OrderBy(l => l.LineNo).Select(l => new CollectionDocumentLineDto
            {
                Id = l.Id, LineNo = l.LineNo, Type = l.Type, EntryNo = l.EntryNo, DocumentType = l.DocumentType, DocumentNo = l.DocumentNo,
                DocumentDate = l.DocumentDate, DueDate = l.DueDate, OriginalAmount = l.OriginalAmount, RemainingAmount = l.RemainingAmount,
                AccountNo = l.AccountNo, Description = l.Description, Amount = l.Amount, OverdueDays = l.OverdueDays,
            }).ToList(),
        };
    }
}

[Authorize(ErpPermissions.Reminders.Default)]
public class ReminderAppService : CollectionDocumentAppServiceBase<ReminderHeader>, IReminderAppService
{
    public ReminderAppService(IRepository<ReminderHeader, Guid> repository)
        : base(repository) { }

    protected override string DefaultPolicy => ErpPermissions.Reminders.Default;
    protected override string CreatePolicy => ErpPermissions.Reminders.Create;
    protected override string UpdatePolicy => ErpPermissions.Reminders.Update;
    protected override string DeletePolicy => ErpPermissions.Reminders.Delete;
    protected override string IssuePolicy => ErpPermissions.Reminders.Issue;

    protected override async Task<int> SuggestCoreAsync(DateTime documentDate, string customerNo) => (await Manager.CreateRemindersAsync(documentDate, customerNo)).Count;

    protected override Task IssueCoreAsync(Guid id, DateTime? postingDate) => Manager.IssueReminderAsync(id, postingDate);

    protected override (string TermsCode, int Level) Terms(ReminderHeader header) => (header.ReminderTermsCode, header.ReminderLevel);
}

[Authorize(ErpPermissions.FinanceChargeMemos.Default)]
public class FinanceChargeMemoAppService : CollectionDocumentAppServiceBase<FinanceChargeMemoHeader>, IFinanceChargeMemoAppService
{
    public FinanceChargeMemoAppService(IRepository<FinanceChargeMemoHeader, Guid> repository)
        : base(repository) { }

    protected override string DefaultPolicy => ErpPermissions.FinanceChargeMemos.Default;
    protected override string CreatePolicy => ErpPermissions.FinanceChargeMemos.Create;
    protected override string UpdatePolicy => ErpPermissions.FinanceChargeMemos.Update;
    protected override string DeletePolicy => ErpPermissions.FinanceChargeMemos.Delete;
    protected override string IssuePolicy => ErpPermissions.FinanceChargeMemos.Issue;

    protected override async Task<int> SuggestCoreAsync(DateTime documentDate, string customerNo) => (await Manager.CreateFinanceChargeMemosAsync(documentDate, customerNo)).Count;

    protected override Task IssueCoreAsync(Guid id, DateTime? postingDate) => Manager.IssueFinanceChargeMemoAsync(id, postingDate);

    protected override (string TermsCode, int Level) Terms(FinanceChargeMemoHeader header) => (header.FinChargeTermsCode, 0);
}

public class ReminderTermsAppService : CodeTableAppServiceBase<ReminderTerms, ReminderTermsDto, CreateUpdateReminderTermsDto>, IReminderTermsAppService
{
    public ReminderTermsAppService(IRepository<ReminderTerms, Guid> repository)
        : base(repository, ErpPermissions.CollectionSetup.Default) { }

    protected override ReminderTerms NewEntity(Guid id, CreateUpdateReminderTermsDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(ReminderTerms entity, CreateUpdateReminderTermsDto input)
    {
        entity.SetRules(input.PostInterest, input.PostAdditionalFee, input.MaxNoOfReminders, input.MinimumAmountLcy);
        return Task.CompletedTask;
    }
}

[Authorize(ErpPermissions.CollectionSetup.Default)]
public class ReminderLevelAppService
    : ErpCrudAppService<ReminderLevel, ReminderLevelDto, Guid, GetReminderLevelListInput, CreateUpdateReminderLevelDto, CreateUpdateReminderLevelDto>,
        IReminderLevelAppService
{
    public ReminderLevelAppService(IRepository<ReminderLevel, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.CollectionSetup.Default;
        GetListPolicyName = ErpPermissions.CollectionSetup.Default;
        CreatePolicyName = ErpPermissions.CollectionSetup.Create;
        UpdatePolicyName = ErpPermissions.CollectionSetup.Update;
        DeletePolicyName = ErpPermissions.CollectionSetup.Delete;
    }

    public override async Task<ReminderLevelDto> CreateAsync(CreateUpdateReminderLevelDto input)
    {
        await CheckCreatePolicyAsync();
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<ReminderTerms>(input.ReminderTermsCode);
        var no = input.No > 0 ? input.No : await NextNoAsync(input.ReminderTermsCode);
        var level = new ReminderLevel(GuidGenerator.Create(), input.ReminderTermsCode, no);
        level.SetRules(input.GracePeriod, input.DueDateCalculation, input.AdditionalFeeLcy, input.CalculateInterest);
        await Repository.InsertAsync(level, autoSave: true);
        return await MapToGetOutputDtoAsync(level);
    }

    public override async Task<ReminderLevelDto> UpdateAsync(Guid id, CreateUpdateReminderLevelDto input)
    {
        await CheckUpdatePolicyAsync();
        var level = await GetEntityByIdAsync(id);
        if (input.No > 0)
        {
            level.SetNo(input.No);
        }
        level.SetRules(input.GracePeriod, input.DueDateCalculation, input.AdditionalFeeLcy, input.CalculateInterest);
        await Repository.UpdateAsync(level, autoSave: true);
        return await MapToGetOutputDtoAsync(level);
    }

    private async Task<int> NextNoAsync(string termsCode)
    {
        var code = termsCode?.Trim().ToUpperInvariant();
        var levels = await Repository.GetListAsync(l => l.ReminderTermsCode == code);
        return levels.Count == 0 ? 1 : levels.Max(l => l.No) + 1;
    }

    protected override async Task<IQueryable<ReminderLevel>> CreateFilteredQueryAsync(GetReminderLevelListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var code = input.ReminderTermsCode?.Trim().ToUpperInvariant();
        return query.WhereIf(!code.IsNullOrEmpty(), l => l.ReminderTermsCode == code);
    }

    protected override IQueryable<ReminderLevel> ApplyDefaultSorting(IQueryable<ReminderLevel> query) => query.OrderBy(l => l.ReminderTermsCode).ThenBy(l => l.No);
}

public class FinanceChargeTermsAppService : CodeTableAppServiceBase<FinanceChargeTerms, FinanceChargeTermsDto, CreateUpdateFinanceChargeTermsDto>, IFinanceChargeTermsAppService
{
    public FinanceChargeTermsAppService(IRepository<FinanceChargeTerms, Guid> repository)
        : base(repository, ErpPermissions.CollectionSetup.Default) { }

    protected override FinanceChargeTerms NewEntity(Guid id, CreateUpdateFinanceChargeTermsDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(FinanceChargeTerms entity, CreateUpdateFinanceChargeTermsDto input)
    {
        entity.SetRules(input.InterestRate, input.InterestPeriodDays, input.MinimumAmountLcy, input.AdditionalFeeLcy, input.GracePeriod, input.DueDateCalculation, input.PostInterest, input.PostAdditionalFee);
        return Task.CompletedTask;
    }
}

public class DeferralTemplateAppService : CodeTableAppServiceBase<DeferralTemplate, DeferralTemplateDto, CreateUpdateDeferralTemplateDto>, IDeferralTemplateAppService
{
    public DeferralTemplateAppService(IRepository<DeferralTemplate, Guid> repository)
        : base(repository, ErpPermissions.Deferrals.Default) { }

    protected override DeferralTemplate NewEntity(Guid id, CreateUpdateDeferralTemplateDto input) =>
        new(id, input.Code, input.Description, input.DeferralAccountNo, input.NoOfPeriods);

    protected override async Task ApplyAsync(DeferralTemplate entity, CreateUpdateDeferralTemplateDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(input.DeferralAccountNo);
        entity.SetRules(input.DeferralAccountNo, input.DeferralPct, input.CalcMethod, input.StartDate, input.NoOfPeriods, input.PeriodDescription);
    }

    public async Task<List<DeferralPeriodDto>> PreviewScheduleAsync(PreviewDeferralInput input)
    {
        await CheckGetPolicyAsync();
        var periods = await LazyServiceProvider.LazyGetRequiredService<DeferralPoster>().PreviewAsync(input.DeferralCode, input.Amount, input.PostingDate == default ? Clock.Now.Date : input.PostingDate);
        return periods.Select(p => new DeferralPeriodDto { PostingDate = p.PostingDate, Amount = p.Amount }).ToList();
    }
}

[Authorize(ErpPermissions.Deferrals.Default)]
public class PostedDeferralAppService : ErpReadOnlyAppService<PostedDeferralHeader, PostedDeferralHeaderDto, Guid, GetPostedDeferralListInput>, IPostedDeferralAppService
{
    public PostedDeferralAppService(IRepository<PostedDeferralHeader, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Deferrals.Default;
        GetListPolicyName = ErpPermissions.Deferrals.Default;
    }

    protected override async Task<PostedDeferralHeader> GetEntityByIdAsync(Guid id) =>
        await ((IRepository<PostedDeferralHeader, Guid>)ReadOnlyRepository).GetAsync(id, includeDetails: true);

    protected override async Task<IQueryable<PostedDeferralHeader>> CreateFilteredQueryAsync(GetPostedDeferralListInput input)
    {
        var query = await ReadOnlyRepository.WithDetailsAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), d => d.DocumentNo.ToLower().Contains(filter) || (d.DeferralCode != null && d.DeferralCode.ToLower().Contains(filter)));
    }

    protected override IQueryable<PostedDeferralHeader> ApplyDefaultSorting(IQueryable<PostedDeferralHeader> query) => query.OrderByDescending(d => d.PostingDate).ThenBy(d => d.DocumentNo);
}

[Authorize(ErpPermissions.CashFlow.Default)]
public class CashFlowForecastAppService
    : ErpCrudAppService<CashFlowForecast, CashFlowForecastDto, Guid, GetCodeTableListInput, CreateUpdateCashFlowForecastDto, CreateUpdateCashFlowForecastDto>,
        ICashFlowForecastAppService
{
    private readonly CashFlowManager _manager;

    public CashFlowForecastAppService(IRepository<CashFlowForecast, Guid> repository, CashFlowManager manager)
        : base(repository)
    {
        _manager = manager;
        GetPolicyName = ErpPermissions.CashFlow.Default;
        GetListPolicyName = ErpPermissions.CashFlow.Default;
        CreatePolicyName = ErpPermissions.CashFlow.Create;
        UpdatePolicyName = ErpPermissions.CashFlow.Update;
        DeletePolicyName = ErpPermissions.CashFlow.Delete;
    }

    public override async Task<CashFlowForecastDto> CreateAsync(CreateUpdateCashFlowForecastDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureUniqueAsync(input.No, null);
        var forecast = new CashFlowForecast(GuidGenerator.Create(), input.No, input.Description);
        await ApplyAsync(forecast, input);
        await Repository.InsertAsync(forecast, autoSave: true);
        return await MapToGetOutputDtoAsync(forecast);
    }

    public override async Task<CashFlowForecastDto> UpdateAsync(Guid id, CreateUpdateCashFlowForecastDto input)
    {
        await CheckUpdatePolicyAsync();
        var forecast = await GetEntityByIdAsync(id);
        await EnsureUniqueAsync(input.No, id);
        forecast.SetNo(input.No);
        forecast.SetDescription(input.Description);
        await ApplyAsync(forecast, input);
        await Repository.UpdateAsync(forecast, autoSave: true);
        return await MapToGetOutputDtoAsync(forecast);
    }

    [Authorize(ErpPermissions.CashFlow.Update)]
    public Task<int> SuggestWorksheetLinesAsync(Guid id, SuggestCashFlowInput input)
    {
        var from = input.FromDate == default ? Clock.Now.Date : input.FromDate;
        var to = input.ToDate == default ? from.AddMonths(12) : input.ToDate;
        return _manager.SuggestWorksheetLinesAsync(id, from, to);
    }

    [Authorize(ErpPermissions.CashFlow.Register)]
    public Task<int> RegisterAsync(Guid id) => _manager.RegisterAsync(id);

    public async Task<CashFlowByPeriodDto> GetByPeriodAsync(Guid id, AnalysisMatrixInput input)
    {
        var forecast = await GetEntityByIdAsync(id);
        var (periods, rows) = await _manager.ByPeriodAsync(forecast.No, input.StartDate == default ? Clock.Now.Date : input.StartDate, input.PeriodType, input.PeriodCount);
        var totals = periods.Select((_, i) => rows.Sum(r => r.Amounts[i])).ToList();
        var running = 0m;
        return new CashFlowByPeriodDto
        {
            Periods = periods.Select(p => new AnalysisPeriodDto { Start = p.Start, End = p.End, Name = p.Name }).ToList(),
            Rows = rows.Select(r => new CashFlowPeriodRowDto { SourceType = r.SourceType, Amounts = r.Amounts.ToList(), Total = r.Total }).ToList(),
            Totals = totals,
            Balances = totals.Select(t => running += t).ToList(),
        };
    }

    private async Task ApplyAsync(CashFlowForecast forecast, CreateUpdateCashFlowForecastDto input)
    {
        if (!input.GLBudgetName.IsNullOrWhiteSpace())
        {
            await LazyServiceProvider.LazyGetRequiredService<BudgetManager>().GetBudgetAsync(input.GLBudgetName);
        }

        forecast.SetSources(input.ConsiderReceivables, input.ConsiderPayables, input.ConsiderSalesOrders, input.ConsiderPurchaseOrders, input.ConsiderManual, input.ConsiderLiquidFunds, input.ManualPaymentsTo);
        forecast.SetBudget(input.GLBudgetName, input.GLBudgetFrom, input.GLBudgetTo);
    }

    private async Task EnsureUniqueAsync(string no, Guid? id)
    {
        var normalized = no?.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(f => f.No == normalized && f.Id != id))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Cash Flow Forecast").WithData("key", normalized);
        }
    }

    protected override async Task<IQueryable<CashFlowForecast>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), f => f.No.ToLower().Contains(filter) || (f.Description != null && f.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<CashFlowForecast> ApplyDefaultSorting(IQueryable<CashFlowForecast> query) => query.OrderBy(f => f.No);
}

public class CashFlowManualEntryAppService : CodeTableAppServiceBase<CashFlowManualEntry, CashFlowManualEntryDto, CreateUpdateCashFlowManualEntryDto>, ICashFlowManualEntryAppService
{
    public CashFlowManualEntryAppService(IRepository<CashFlowManualEntry, Guid> repository)
        : base(repository, ErpPermissions.CashFlow.Default) { }

    protected override CashFlowManualEntry NewEntity(Guid id, CreateUpdateCashFlowManualEntryDto input) =>
        new(id, input.Code, input.Description, input.Kind, input.Amount, input.StartingDate);

    protected override Task ApplyAsync(CashFlowManualEntry entity, CreateUpdateCashFlowManualEntryDto input)
    {
        entity.Set(input.Kind, input.Amount, input.StartingDate == default ? Clock.Now.Date : input.StartingDate, input.RecurringFrequency, input.EndingDate);
        return Task.CompletedTask;
    }
}

[Authorize(ErpPermissions.CashFlow.Default)]
public class CashFlowWorksheetLineAppService
    : ErpCrudAppService<CashFlowWorksheetLine, CashFlowWorksheetLineDto, Guid, GetCashFlowLineListInput, CreateUpdateCashFlowWorksheetLineDto, CreateUpdateCashFlowWorksheetLineDto>,
        ICashFlowWorksheetLineAppService
{
    public CashFlowWorksheetLineAppService(IRepository<CashFlowWorksheetLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.CashFlow.Default;
        GetListPolicyName = ErpPermissions.CashFlow.Default;
        CreatePolicyName = ErpPermissions.CashFlow.Update;
        UpdatePolicyName = ErpPermissions.CashFlow.Update;
        DeletePolicyName = ErpPermissions.CashFlow.Update;
    }

    public override async Task<CashFlowWorksheetLineDto> CreateAsync(CreateUpdateCashFlowWorksheetLineDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.CashFlowForecastNo.Trim().ToUpperInvariant();
        var lines = await Repository.GetListAsync(l => l.CashFlowForecastNo == no);
        var line = new CashFlowWorksheetLine(GuidGenerator.Create(), no, (lines.Count == 0 ? 0 : lines.Max(l => l.LineNo)) + 10000, input.CashFlowDate, input.SourceType, input.SourceNo, input.DocumentNo, input.Description, input.Amount);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<CashFlowWorksheetLineDto> UpdateAsync(Guid id, CreateUpdateCashFlowWorksheetLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        line.Update(input.CashFlowDate, input.SourceType, input.SourceNo, input.DocumentNo, input.Description, input.Amount);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    protected override async Task<IQueryable<CashFlowWorksheetLine>> CreateFilteredQueryAsync(GetCashFlowLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var no = input.CashFlowForecastNo?.Trim().ToUpperInvariant();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!no.IsNullOrEmpty(), l => l.CashFlowForecastNo == no)
            .WhereIf(!filter.IsNullOrEmpty(), l => (l.Description != null && l.Description.ToLower().Contains(filter)) || (l.DocumentNo != null && l.DocumentNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<CashFlowWorksheetLine> ApplyDefaultSorting(IQueryable<CashFlowWorksheetLine> query) => query.OrderBy(l => l.CashFlowForecastNo).ThenBy(l => l.LineNo);
}

[Authorize(ErpPermissions.CashFlow.Default)]
public class CashFlowForecastEntryAppService : ErpReadOnlyAppService<CashFlowForecastEntry, CashFlowForecastEntryDto, Guid, GetCashFlowLineListInput>, ICashFlowForecastEntryAppService
{
    public CashFlowForecastEntryAppService(IRepository<CashFlowForecastEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.CashFlow.Default;
        GetListPolicyName = ErpPermissions.CashFlow.Default;
    }

    protected override async Task<IQueryable<CashFlowForecastEntry>> CreateFilteredQueryAsync(GetCashFlowLineListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var no = input.CashFlowForecastNo?.Trim().ToUpperInvariant();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!no.IsNullOrEmpty(), e => e.CashFlowForecastNo == no)
            .WhereIf(!filter.IsNullOrEmpty(), e => (e.Description != null && e.Description.ToLower().Contains(filter)) || (e.DocumentNo != null && e.DocumentNo.ToLower().Contains(filter)));
    }

    protected override IQueryable<CashFlowForecastEntry> ApplyDefaultSorting(IQueryable<CashFlowForecastEntry> query) => query.OrderBy(e => e.CashFlowDate).ThenBy(e => e.EntryNo);
}

/// <summary>The G/L budget as a matrix, budget against actual, and copying budgets.</summary>
[Authorize(ErpPermissions.Budgets.Default)]
public class BudgetMatrixAppService : ErpAppService, IBudgetMatrixAppService
{
    private readonly BudgetManager _manager;

    public BudgetMatrixAppService(BudgetManager manager)
    {
        _manager = manager;
    }

    public async Task<BudgetMatrixDto> GetMatrixAsync(BudgetMatrixInput input)
    {
        var (periods, rows) = await _manager.GetMatrixAsync(
            input.BudgetName,
            input.StartDate == default ? new DateTime(Clock.Now.Year, 1, 1) : input.StartDate,
            input.PeriodType,
            input.PeriodCount,
            input.AccountFilter,
            input.WithActual,
            input.IncomeStatementOnly
        );

        return new BudgetMatrixDto
        {
            BudgetName = input.BudgetName.Trim().ToUpperInvariant(),
            Periods = periods.Select(p => new AnalysisPeriodDto { Start = p.Start, End = p.End, Name = p.Name }).ToList(),
            Rows = rows.Select(r => new BudgetMatrixRowDto
            {
                AccountNo = r.AccountNo,
                AccountName = r.AccountName,
                AccountType = r.AccountType,
                Indentation = r.Indentation,
                Budget = r.Budget.ToList(),
                Actual = r.Actual.ToList(),
                TotalBudget = r.Budget.Sum(),
                TotalActual = r.Actual.Sum(),
            }).ToList(),
        };
    }

    [Authorize(ErpPermissions.Budgets.Update)]
    public Task SetCellAsync(SetBudgetCellInput input) => _manager.SetCellAsync(input.BudgetName, input.AccountNo, input.PeriodStart, input.PeriodEnd, input.Amount);

    [Authorize(ErpPermissions.Budgets.Update)]
    public Task<int> CopyBudgetAsync(CopyBudgetInput input) => _manager.CopyAsync(input.FromBudgetName, input.ToBudgetName, input.DateChange, input.Factor, input.Replace);
}
