using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// General Ledger Setup. Mirrors Business Central table 98: one row per company holding the
/// ledger-wide rules, above all the range of dates anything may be posted on.
/// </summary>
public class GeneralLedgerSetup : CompanyEntity
{
    public const decimal DefaultAmountRoundingPrecision = 0.01m;
    public const decimal DefaultUnitAmountRoundingPrecision = 0.00001m;

    /// <summary>First date entries may be posted on; blank means no lower limit.</summary>
    public DateTime? AllowPostingFrom { get; private set; }

    /// <summary>Last date entries may be posted on; blank means no upper limit.</summary>
    public DateTime? AllowPostingTo { get; private set; }

    /// <summary>The local currency (LCY) every ledger amount is in, e.g. "KES".</summary>
    public string LcyCode { get; private set; }

    public decimal AmountRoundingPrecision { get; private set; } = DefaultAmountRoundingPrecision;
    public decimal UnitAmountRoundingPrecision { get; private set; } = DefaultUnitAmountRoundingPrecision;
    public decimal InvRoundingPrecisionLcy { get; private set; } = DefaultAmountRoundingPrecision;

    /// <summary>The two dimensions every entry carries as a column of its own (BC Global Dimension 1 and 2).</summary>
    public string GlobalDimension1Code { get; private set; }
    public string GlobalDimension2Code { get; private set; }

    /// <summary>The series a new bank account's number is drawn from when it is left blank.</summary>
    public string BankAccountNos { get; private set; }

    protected GeneralLedgerSetup() { }

    public GeneralLedgerSetup(Guid id)
        : base(id) { }

    public void SetAllowedPostingDates(DateTime? allowPostingFrom, DateTime? allowPostingTo)
    {
        if (allowPostingFrom.HasValue && allowPostingTo.HasValue && allowPostingTo.Value.Date < allowPostingFrom.Value.Date)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidPostingDateRange);
        }

        AllowPostingFrom = allowPostingFrom?.Date;
        AllowPostingTo = allowPostingTo?.Date;
    }

    public void SetLocalCurrency(string lcyCode)
    {
        LcyCode = lcyCode.IsNullOrWhiteSpace()
            ? null
            : Check.Length(lcyCode.Trim().ToUpperInvariant(), nameof(lcyCode), ErpDomainConsts.MaxCurrencyCodeLength);
    }

    public void SetRoundingPrecisions(decimal amount, decimal unitAmount, decimal invoiceRounding)
    {
        if (amount <= 0 || unitAmount <= 0 || invoiceRounding <= 0)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidRoundingPrecision);
        }

        AmountRoundingPrecision = amount;
        UnitAmountRoundingPrecision = unitAmount;
        InvRoundingPrecisionLcy = invoiceRounding;
    }

    public void SetGlobalDimensions(string globalDimension1Code, string globalDimension2Code)
    {
        var first = Dimension(globalDimension1Code, nameof(globalDimension1Code));
        var second = Dimension(globalDimension2Code, nameof(globalDimension2Code));

        if (first != null && first == second)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.SameGlobalDimensions).WithData("code", first);
        }

        GlobalDimension1Code = first;
        GlobalDimension2Code = second;
    }

    public void SetNumbering(string bankAccountNos)
    {
        BankAccountNos = bankAccountNos.IsNullOrWhiteSpace()
            ? null
            : Check.Length(bankAccountNos.Trim(), nameof(bankAccountNos), ErpDomainConsts.MaxNoSeriesCodeLength);
    }

    public bool IsPostingDateAllowed(DateTime postingDate)
    {
        var date = postingDate.Date;
        return (!AllowPostingFrom.HasValue || date >= AllowPostingFrom.Value)
            && (!AllowPostingTo.HasValue || date <= AllowPostingTo.Value);
    }

    private static string Dimension(string code, string name)
    {
        return code.IsNullOrWhiteSpace()
            ? null
            : Check.Length(code.Trim().ToUpperInvariant(), name, ErpDomainConsts.MaxDimensionCodeLength);
    }
}

public class GeneralLedgerSetupManager : DomainService
{
    private readonly IRepository<GeneralLedgerSetup, Guid> _repository;

    public GeneralLedgerSetupManager(IRepository<GeneralLedgerSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<GeneralLedgerSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new GeneralLedgerSetup(GuidGenerator.Create()), autoSave: true);
    }

    /// <summary>
    /// Refuses a posting date outside the allowed range (BC "is not within your range of allowed
    /// posting dates"). Read only: a company without a setup row may post on any date.
    /// </summary>
    public async Task CheckPostingDateAsync(DateTime postingDate)
    {
        var setup = await _repository.FirstOrDefaultAsync();
        if (setup != null && !setup.IsPostingDateAllowed(postingDate))
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.PostingDateNotAllowed)
                .WithData("postingDate", postingDate.ToString("yyyy-MM-dd"))
                .WithData("allowPostingFrom", setup.AllowPostingFrom?.ToString("yyyy-MM-dd") ?? "-")
                .WithData("allowPostingTo", setup.AllowPostingTo?.ToString("yyyy-MM-dd") ?? "-");
        }
    }
}
