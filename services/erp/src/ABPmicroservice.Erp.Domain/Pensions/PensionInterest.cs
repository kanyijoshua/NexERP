using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// The interest a scheme's trustees declare for a period: an annual rate for registered money and
/// one for unregistered money, and the tax withheld on unregistered interest. Allocating it credits
/// every member's fund once; a declaration that has been allocated cannot be changed.
/// </summary>
public class PensionInterestRate : CompanyEntity
{
    public string SchemeCode { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime? DateDeclared { get; private set; }

    /// <summary>Annual rate on registered (tax exempt) balances, as a percentage.</summary>
    public decimal RegisteredRatePct { get; private set; }

    /// <summary>Annual rate on unregistered (non tax exempt) balances, as a percentage.</summary>
    public decimal UnregisteredRatePct { get; private set; }

    /// <summary>Tax withheld from interest on unregistered balances, as a percentage of that interest.</summary>
    public decimal TaxRatePct { get; private set; }

    public bool Posted { get; private set; }
    public string PostedDocumentNo { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public decimal TotalInterest { get; private set; }
    public decimal TotalTax { get; private set; }

    protected PensionInterestRate() { }

    public PensionInterestRate(Guid id, string schemeCode, DateTime startDate, DateTime endDate)
        : base(id)
    {
        Set(schemeCode, startDate, endDate, null, 0m, 0m, 0m);
    }

    public void Set(
        string schemeCode,
        DateTime startDate,
        DateTime endDate,
        DateTime? dateDeclared,
        decimal registeredRatePct,
        decimal unregisteredRatePct,
        decimal taxRatePct
    )
    {
        if (Posted)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.InterestAlreadyAllocated).WithData("documentNo", PostedDocumentNo);
        }

        if (endDate.Date < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }

        foreach (var rate in new[] { registeredRatePct, unregisteredRatePct, taxRatePct })
        {
            if (rate is < 0 or > 100)
            {
                throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", rate);
            }
        }

        SchemeCode = PensionSponsor.SchemeOf(schemeCode);
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        DateDeclared = dateDeclared?.Date;
        RegisteredRatePct = registeredRatePct;
        UnregisteredRatePct = unregisteredRatePct;
        TaxRatePct = taxRatePct;
    }

    internal void MarkPosted(string documentNo, DateTime postingDate, decimal totalInterest, decimal totalTax)
    {
        Posted = true;
        PostedDocumentNo = documentNo;
        PostedDate = postingDate.Date;
        TotalInterest = totalInterest;
        TotalTax = totalTax;
    }
}

/// <summary>What one member earns on one money type for a declared period.</summary>
public class InterestAllocationLine
{
    public string MemberNo { get; set; }
    public string MemberName { get; set; }
    public MoneyType MoneyType { get; set; }

    /// <summary>The balance at the end of the period, before the interest.</summary>
    public decimal Balance { get; set; }

    public decimal Interest { get; set; }
    public decimal Tax { get; set; }
}

/// <summary>
/// Works out and posts declared interest.
/// <para>
/// A balance brought into the period earns for the whole period. Money that arrives during the
/// period earns from the month after it was posted ("in arrears"), or from the day after under
/// daily compounding. Interest on unregistered money is taxed at the declared rate, and the tax
/// comes out of the member's fund.
/// </para>
/// </summary>
public class PensionInterestEngine : DomainService
{
    public const string SourceCode = "PENINT";

    private readonly IRepository<PensionInterestRate, Guid> _rates;
    private readonly IRepository<MemberLedgerEntry, Guid> _entries;
    private readonly IRepository<PensionMember, Guid> _members;
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly MemberLedger _memberLedger;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly ICurrentUser _currentUser;

    public PensionInterestEngine(
        IRepository<PensionInterestRate, Guid> rates,
        IRepository<MemberLedgerEntry, Guid> entries,
        IRepository<PensionMember, Guid> members,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        MemberLedger memberLedger,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        NoSeriesManager noSeriesManager,
        ICurrentUser currentUser
    )
    {
        _rates = rates;
        _entries = entries;
        _members = members;
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _memberLedger = memberLedger;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _noSeriesManager = noSeriesManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// The fraction of itself an amount earns from <paramref name="from"/> to <paramref name="to"/>
    /// (both inclusive) at an annual rate.
    /// </summary>
    public static decimal GrowthFactor(decimal annualRatePct, DateTime from, DateTime to, InterestCalculationMode mode, int daysInYear)
    {
        if (to < from || annualRatePct == 0m)
        {
            return 0m;
        }

        var rate = (double)annualRatePct / 100d;
        var days = (to.Date - from.Date).Days + 1;

        switch (mode)
        {
            case InterestCalculationMode.CompoundDaily:
                return (decimal)(Math.Pow(1d + rate, days / (double)daysInYear) - 1d);

            case InterestCalculationMode.Simple:
                return (decimal)(rate * days / daysInYear);

            default:
                var months = (to.Year - from.Year) * 12 + to.Month - from.Month + 1;
                return (decimal)(Math.Pow(1d + rate, months / 12d) - 1d);
        }
    }

    /// <summary>What allocating the declaration would credit, member by member, without posting anything.</summary>
    public async Task<List<InterestAllocationLine>> CalculateAsync(PensionInterestRate rate)
    {
        var scheme = await _schemeManager.GetAsync(rate.SchemeCode);
        var setup = await _setupManager.GetAsync();
        var names = (await _members.GetListAsync(m => m.SchemeCode == rate.SchemeCode)).ToDictionary(m => m.No, m => m.FullName, StringComparer.Ordinal);

        var entries = await _entries.GetListAsync(e => e.SchemeCode == rate.SchemeCode && e.PostingDate <= rate.EndDate);
        var lines = new List<InterestAllocationLine>();

        foreach (var group in entries.GroupBy(e => (e.MemberNo, e.MoneyType)).OrderBy(g => g.Key.MemberNo, StringComparer.Ordinal))
        {
            var moneyType = group.Key.MoneyType;
            var annualRate = moneyType.IsRegistered ? rate.RegisteredRatePct : rate.UnregisteredRatePct;

            var opening = group.Where(e => e.PostingDate < rate.StartDate).Sum(e => e.Amount);
            var interest = opening * GrowthFactor(annualRate, rate.StartDate, rate.EndDate, scheme.InterestCalculationMode, setup.NoOfDaysInAYear);

            foreach (var entry in group.Where(e => e.PostingDate >= rate.StartDate))
            {
                // In arrears: money earns from the period after the one it arrived in.
                var from = scheme.InterestCalculationMode == InterestCalculationMode.CompoundMonthly
                    ? new DateTime(entry.PostingDate.Year, entry.PostingDate.Month, 1).AddMonths(1)
                    : entry.PostingDate.AddDays(1);

                interest += entry.Amount * GrowthFactor(annualRate, from, rate.EndDate, scheme.InterestCalculationMode, setup.NoOfDaysInAYear);
            }

            interest = Math.Round(interest, 2, MidpointRounding.AwayFromZero);
            if (interest <= 0m)
            {
                continue;
            }

            lines.Add(
                new InterestAllocationLine
                {
                    MemberNo = group.Key.MemberNo,
                    MemberName = names.GetValueOrDefault(group.Key.MemberNo, string.Empty),
                    MoneyType = moneyType,
                    Balance = group.Sum(e => e.Amount),
                    Interest = interest,
                    Tax = moneyType.IsRegistered ? 0m : Math.Round(interest * rate.TaxRatePct / 100m, 2, MidpointRounding.AwayFromZero),
                }
            );
        }

        return lines;
    }

    /// <summary>
    /// Credits every member with the declared interest. In the G/L the interest account is debited
    /// with the gross interest, the member funds liability credited with the interest net of tax
    /// and the tax account with the tax, all under the scheme's dimension.
    /// </summary>
    public async Task<string> PostAsync(PensionInterestRate rate, DateTime postingDate)
    {
        if (rate.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.InterestAlreadyAllocated).WithData("documentNo", rate.PostedDocumentNo);
        }

        await _glSetupManager.CheckPostingDateAsync(postingDate);

        var lines = await CalculateAsync(rate);
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NothingToPost).WithData("documentNo", rate.SchemeCode);
        }

        var setup = await _setupManager.GetAsync();
        var memberFunds = setup.Require(setup.MemberFundsAccountNo, "Member Funds Account No.");
        var interestAccount = setup.Require(setup.InterestAccountNo, "Interest Account No.");
        var totalInterest = lines.Sum(l => l.Interest);
        var totalTax = lines.Sum(l => l.Tax);
        var taxAccount = totalTax == 0m ? null : setup.Require(setup.TaxAccountNo, "Tax Account No.");

        var documentNo = setup.InterestBatchNos.IsNullOrWhiteSpace()
            ? $"INT-{rate.SchemeCode}-{rate.EndDate:yyyyMM}"
            : await _noSeriesManager.GetNextNoAsync(setup.InterestBatchNos, postingDate);

        var dimensionSetId = await _schemeManager.GetDimensionSetIdAsync(rate.SchemeCode);
        var register = await _registerManager.OpenAsync(postingDate, SourceCode, documentNo);
        var context = new GLPostingContext(register, SourceCode);
        var description = $"Interest {rate.StartDate:dd MMM yyyy} to {rate.EndDate:dd MMM yyyy}";
        var period = new DateTime(rate.EndDate.Year, rate.EndDate.Month, 1);

        var members = (await _members.GetListAsync(m => m.SchemeCode == rate.SchemeCode)).ToDictionary(m => m.No, StringComparer.Ordinal);

        foreach (var line in lines)
        {
            if (!members.TryGetValue(line.MemberNo, out var member))
            {
                continue;
            }

            await _memberLedger.PostAsync(
                member, postingDate, period, documentNo, description, PensionTransactionType.Interest, line.MoneyType,
                PensionContributionMode.Normal, line.Interest, 0m, dimensionSetId, context, _currentUser.UserName
            );
            await _memberLedger.PostAsync(
                member, postingDate, period, documentNo, "Tax on " + description, PensionTransactionType.TaxOnInterest, line.MoneyType,
                PensionContributionMode.Normal, -line.Tax, 0m, dimensionSetId, context, _currentUser.UserName
            );
        }

        await _genJnlPostLine.PostGLDirectAsync(interestAccount, postingDate, GLEntryDocumentType.None, documentNo, description, totalInterest, null, dimensionSetId, context);
        await _genJnlPostLine.PostGLDirectAsync(memberFunds, postingDate, GLEntryDocumentType.None, documentNo, description, -(totalInterest - totalTax), null, dimensionSetId, context);

        if (totalTax != 0m)
        {
            await _genJnlPostLine.PostGLDirectAsync(taxAccount, postingDate, GLEntryDocumentType.None, documentNo, "Tax on " + description, -totalTax, null, dimensionSetId, context);
        }

        await _registerManager.CloseAsync(register);

        rate.MarkPosted(documentNo, postingDate, totalInterest, totalTax);
        await _rates.UpdateAsync(rate, autoSave: true);

        return documentNo;
    }
}
