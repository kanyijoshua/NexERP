using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Business Unit: a subsidiary company whose general ledger the consolidation company takes in,
/// at a share of ownership and, when the subsidiary keeps its books in another currency, at a
/// translated rate.
/// </summary>
public class BusinessUnit : CodeTableEntity
{
    public Guid SubsidiaryCompanyId { get; private set; }
    public bool Consolidate { get; private set; } = true;

    /// <summary>The share of the subsidiary that is consolidated, in percent.</summary>
    public decimal ConsolidationPct { get; private set; } = 100m;

    /// <summary>The subsidiary's local currency as this company knows it; blank is the same currency.</summary>
    public string CurrencyCode { get; private set; }

    /// <summary>Takes what translating at different rates leaves unbalanced.</summary>
    public string ResidualAccountNo { get; private set; }

    public DateTime? LastRunStartingDate { get; private set; }
    public DateTime? LastRunEndingDate { get; private set; }

    protected BusinessUnit() { }

    public BusinessUnit(Guid id, string code, string description, Guid subsidiaryCompanyId)
        : base(id, code, description)
    {
        SubsidiaryCompanyId = subsidiaryCompanyId;
    }

    public void Set(Guid subsidiaryCompanyId, bool consolidate, decimal consolidationPct, string currencyCode, string residualAccountNo)
    {
        SubsidiaryCompanyId = subsidiaryCompanyId;
        Consolidate = consolidate;
        ConsolidationPct = Math.Clamp(consolidationPct, 0m, 100m);
        CurrencyCode = NormalizeCode(Check.Length(currencyCode, nameof(currencyCode), ErpDomainConsts.MaxCurrencyCodeLength));
        ResidualAccountNo = residualAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(residualAccountNo.Trim(), nameof(residualAccountNo), ErpDomainConsts.MaxNoLength);
    }

    internal void RecordRun(DateTime startingDate, DateTime endingDate)
    {
        LastRunStartingDate = startingDate.Date;
        LastRunEndingDate = endingDate.Date;
    }
}

/// <summary>One account of a business unit as it consolidates.</summary>
public sealed class ConsolidationLine
{
    public string BusinessUnitCode { get; init; }
    public string SubsidiaryAccountNo { get; init; }
    public string ConsolidationAccountNo { get; init; }
    public ConsolidationTranslationMethod TranslationMethod { get; init; }

    /// <summary>The net change in the subsidiary's currency.</summary>
    public decimal SubsidiaryAmount { get; init; }

    /// <summary>What is posted here: translated, at the consolidation share.</summary>
    public decimal Amount { get; init; }
}

/// <summary>
/// Consolidation: posts the net change of every G/L account of each business unit over a period
/// into this company, on the account's consolidation debit or credit account (the same account
/// number when it has none). Income statement accounts are translated at the period's average
/// rate and balance sheet accounts at the closing rate, as each account's translation method
/// says; what that leaves unbalanced goes to the unit's residual account.
/// </summary>
public class ConsolidationManager : DomainService
{
    private const string SourceCode = "CONSOLID";

    private readonly IRepository<BusinessUnit, Guid> _unitRepository;
    private readonly IRepository<GLEntry, Guid> _entryRepository;
    private readonly IRepository<GLAccount, Guid> _accountRepository;

    private ICurrentCompany CurrentCompany => LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>();
    private CurrencyExchangeRateManager CurrencyManager => LazyServiceProvider.LazyGetRequiredService<CurrencyExchangeRateManager>();
    private GenJnlPostLine GenJnlPostLine => LazyServiceProvider.LazyGetRequiredService<GenJnlPostLine>();
    private GLRegisterManager RegisterManager => LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
    private GeneralLedgerSetupManager GLSetupManager => LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();

    public ConsolidationManager(IRepository<BusinessUnit, Guid> unitRepository, IRepository<GLEntry, Guid> entryRepository, IRepository<GLAccount, Guid> accountRepository)
    {
        _unitRepository = unitRepository;
        _entryRepository = entryRepository;
        _accountRepository = accountRepository;
    }

    /// <summary>What consolidating a business unit over the period would post, without posting it.</summary>
    public async Task<List<ConsolidationLine>> PreviewAsync(Guid businessUnitId, DateTime startingDate, DateTime endingDate)
    {
        var unit = await _unitRepository.GetAsync(businessUnitId);
        return await LinesAsync(unit, startingDate.Date, endingDate.Date);
    }

    /// <summary>Consolidates every business unit set to consolidate (or just one) over the period; returns the lines posted.</summary>
    public async Task<List<ConsolidationLine>> RunAsync(DateTime startingDate, DateTime endingDate, string documentNo, Guid? businessUnitId)
    {
        var from = startingDate.Date;
        var to = endingDate.Date;
        await GLSetupManager.CheckPostingDateAsync(to);
        var units = businessUnitId.HasValue
            ? [await _unitRepository.GetAsync(businessUnitId.Value)]
            : (await _unitRepository.GetListAsync(u => u.Consolidate)).OrderBy(u => u.Code).ToList();

        var register = await RegisterManager.OpenAsync(to, SourceCode, documentNo);
        var context = new GLPostingContext(register, SourceCode);
        var posted = new List<ConsolidationLine>();
        foreach (var unit in units)
        {
            if (unit.LastRunEndingDate.HasValue && from <= unit.LastRunEndingDate.Value)
            {
                throw new BusinessException(ErpErrorCodes.Consolidation.PeriodAlreadyConsolidated)
                    .WithData("code", unit.Code)
                    .WithData("lastDate", unit.LastRunEndingDate.Value.ToString("yyyy-MM-dd"));
            }

            var lines = await LinesAsync(unit, from, to);
            if (lines.Count == 0)
            {
                throw new BusinessException(ErpErrorCodes.Consolidation.NothingToConsolidate)
                    .WithData("code", unit.Code)
                    .WithData("fromDate", from.ToString("yyyy-MM-dd"))
                    .WithData("toDate", to.ToString("yyyy-MM-dd"));
            }

            foreach (var line in lines.Where(l => l.Amount != 0m))
            {
                await GenJnlPostLine.PostGLDirectAsync(line.ConsolidationAccountNo, to, GLEntryDocumentType.None, documentNo, $"{unit.Code} {line.SubsidiaryAccountNo}", line.Amount, unit.Code, context: context);
            }

            var residual = lines.Sum(l => l.Amount);
            if (residual != 0m)
            {
                var account = unit.ResidualAccountNo
                    ?? throw new BusinessException(ErpErrorCodes.Consolidation.ConsolidationAccountMissing).WithData("code", unit.Code).WithData("accountNo", "residual");
                await GenJnlPostLine.PostGLDirectAsync(account, to, GLEntryDocumentType.None, documentNo, $"{unit.Code} residual", -residual, unit.Code, context: context);
            }

            unit.RecordRun(from, to);
            await _unitRepository.UpdateAsync(unit, autoSave: true);
            posted.AddRange(lines);
        }

        await RegisterManager.CloseAsync(register);
        return posted;
    }

    private async Task<List<ConsolidationLine>> LinesAsync(BusinessUnit unit, DateTime from, DateTime to)
    {
        if (unit.SubsidiaryCompanyId == Guid.Empty || unit.SubsidiaryCompanyId == CurrentCompany.Id)
        {
            throw new BusinessException(ErpErrorCodes.Consolidation.BusinessUnitCompanyMissing).WithData("code", unit.Code);
        }

        // The subsidiary's books are read in the subsidiary's own company.
        Dictionary<string, decimal> balances;
        Dictionary<string, GLAccount> subsidiaryAccounts;
        using (CurrentCompany.Change(unit.SubsidiaryCompanyId))
        {
            balances = (await _entryRepository.GetListAsync(e => e.PostingDate >= from && e.PostingDate <= to))
                .GroupBy(e => e.GLAccountNo)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
            subsidiaryAccounts = (await _accountRepository.GetListAsync()).ToDictionary(a => a.No);
        }

        var localAccounts = (await _accountRepository.GetListAsync()).Select(a => a.No).ToHashSet();
        var currency = await CurrencyManager.NormalizeAsync(unit.CurrencyCode);
        var closing = currency == null ? 1m : await CurrencyManager.GetCurrencyFactorAsync(currency, to);
        var average = currency == null ? 1m : (await CurrencyManager.GetCurrencyFactorAsync(currency, from) + closing) / 2m;

        var lines = new List<ConsolidationLine>();
        foreach (var (accountNo, amount) in balances.Where(b => b.Value != 0m).OrderBy(b => b.Key))
        {
            subsidiaryAccounts.TryGetValue(accountNo, out var account);
            var target = amount > 0m ? account?.ConsolDebitAcc : account?.ConsolCreditAcc;
            target = target.IsNullOrWhiteSpace() ? accountNo : target.Trim();
            if (!localAccounts.Contains(target))
            {
                throw new BusinessException(ErpErrorCodes.Consolidation.ConsolidationAccountMissing).WithData("code", unit.Code).WithData("accountNo", target);
            }

            var method = account?.ConsolTranslationMethod ?? ConsolidationTranslationMethod.Average;
            var factor = method == ConsolidationTranslationMethod.Average ? average : closing;
            var translated = CurrencyExchangeRateManager.ToLcy(amount, factor) * unit.ConsolidationPct / 100m;
            lines.Add(new ConsolidationLine
            {
                BusinessUnitCode = unit.Code,
                SubsidiaryAccountNo = accountNo,
                ConsolidationAccountNo = target,
                TranslationMethod = method,
                SubsidiaryAmount = amount,
                Amount = Math.Round(translated, 2, MidpointRounding.AwayFromZero),
            });
        }

        return lines;
    }
}
