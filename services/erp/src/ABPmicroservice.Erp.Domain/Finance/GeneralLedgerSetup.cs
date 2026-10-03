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

    /// <summary>Local currency symbol. Mirrors BC field 162 "Local Currency Symbol".</summary>
    public string LocalCurrencySymbol { get; private set; }

    /// <summary>Local currency description. Mirrors BC field 163 "Local Currency Description".</summary>
    public string LocalCurrencyDescription { get; private set; }

    /// <summary>Invoice rounding type. Mirrors BC field 59 "Inv. Rounding Type (LCY)".</summary>
    public RoundingType InvRoundingType { get; private set; } = RoundingType.Nearest;

    /// <summary>VAT rounding type. Mirrors BC field 90 "VAT Rounding Type".</summary>
    public RoundingType VATRoundingType { get; private set; } = RoundingType.Nearest;

    /// <summary>Payment discount excludes VAT. Mirrors BC field 28 "Pmt. Disc. Excl. VAT".</summary>
    public bool PmtDiscExclVAT { get; private set; }

    /// <summary>Unrealized VAT handling. Mirrors BC field 48 "Unrealized VAT".</summary>
    public bool UnrealizedVAT { get; private set; }

    /// <summary>Adjust for payment discount. Mirrors BC field 49 "Adjust for Payment Disc.".</summary>
    public bool AdjustForPaymentDisc { get; private set; }

    /// <summary>Mark credit memos as corrections. Mirrors BC field 56 "Mark Cr. Memos as Corrections".</summary>
    public bool MarkCrMemosAsCorrections { get; private set; }

    /// <summary>Additional reporting currency. Mirrors BC field 68 "Additional Reporting Currency".</summary>
    public string AdditionalReportingCurrency { get; private set; }

    /// <summary>Maximum VAT difference allowed. Mirrors BC field 89 "Max. VAT Difference Allowed".</summary>
    public decimal MaxVATDifferenceAllowed { get; private set; }

    /// <summary>Payment tolerance percent. Mirrors BC field 94 "Payment Tolerance %".</summary>
    public decimal PaymentTolerancePct { get; private set; }

    /// <summary>Maximum payment tolerance amount. Mirrors BC field 95 "Max. Payment Tolerance Amount".</summary>
    public decimal MaxPaymentToleranceAmount { get; private set; }

    /// <summary>Block deletion of G/L accounts. Mirrors BC field 104 "Block Deletion of G/L Accounts".</summary>
    public bool BlockDeletionOfGLAccounts { get; private set; }

    /// <summary>Post with job queue. Mirrors BC field 50 "Post with Job Queue".</summary>
    public bool PostWithJobQueue { get; private set; }

    /// <summary>Job queue category code for posting. Mirrors BC field 51 "Job Queue Category Code".</summary>
    public string JobQueueCategoryCode { get; private set; }

    /// <summary>Notify on job queue success. Mirrors BC field 55 "Notify On Success".</summary>
    public bool NotifyOnSuccess { get; private set; }

    /// <summary>Register user time. Mirrors BC field 4 "Register Time".</summary>
    public bool RegisterTime { get; private set; }

    protected GeneralLedgerSetup() { }

    public GeneralLedgerSetup(Guid id)
        : base(id) { }

    public void SetLocalCurrencyDetails(string symbol, string description)
    {
        LocalCurrencySymbol = symbol.IsNullOrWhiteSpace()
            ? null
            : Check.Length(symbol.Trim(), nameof(symbol), ErpDomainConsts.MaxCurrencySymbolLength);
        LocalCurrencyDescription = description.IsNullOrWhiteSpace()
            ? null
            : Check.Length(description.Trim(), nameof(description), ErpDomainConsts.MaxCurrencyDescriptionLength);
    }

    public void SetRoundingTypes(RoundingType invRoundingType, RoundingType vatRoundingType)
    {
        InvRoundingType = invRoundingType;
        VATRoundingType = vatRoundingType;
    }

    public void SetVatSetup(bool pmtDiscExclVat, bool unrealizedVat, bool adjustForPaymentDisc, bool markCrMemosAsCorrections, decimal maxVatDifferenceAllowed)
    {
        PmtDiscExclVAT = pmtDiscExclVat;
        UnrealizedVAT = unrealizedVat;
        AdjustForPaymentDisc = adjustForPaymentDisc;
        MarkCrMemosAsCorrections = markCrMemosAsCorrections;
        MaxVATDifferenceAllowed = maxVatDifferenceAllowed >= 0 ? maxVatDifferenceAllowed : 0;
    }

    public void SetAdditionalReportingCurrency(string currencyCode)
    {
        AdditionalReportingCurrency = currencyCode.IsNullOrWhiteSpace()
            ? null
            : Check.Length(currencyCode.Trim().ToUpperInvariant(), nameof(currencyCode), ErpDomainConsts.MaxCurrencyCodeLength);
    }

    public void SetPaymentTolerance(decimal tolerancePct, decimal maxToleranceAmount)
    {
        PaymentTolerancePct = tolerancePct >= 0 ? tolerancePct : 0;
        MaxPaymentToleranceAmount = maxToleranceAmount >= 0 ? maxToleranceAmount : 0;
    }

    public void SetJobQueuePost(bool postWithJobQueue, string categoryCode, bool notifyOnSuccess)
    {
        PostWithJobQueue = postWithJobQueue;
        JobQueueCategoryCode = categoryCode.IsNullOrWhiteSpace()
            ? null
            : Check.Length(categoryCode.Trim().ToUpperInvariant(), nameof(categoryCode), ErpDomainConsts.MaxJobCategoryCodeLength);
        NotifyOnSuccess = notifyOnSuccess;
    }

    public void SetFlags(bool blockDeletionOfGLAccounts, bool registerTime)
    {
        BlockDeletionOfGLAccounts = blockDeletionOfGLAccounts;
        RegisterTime = registerTime;
    }

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
