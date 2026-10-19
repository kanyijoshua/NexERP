using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Deferral Template: how much of an amount to defer, to which balance sheet account, and over how
/// many monthly periods to release it to the income statement.
/// </summary>
public class DeferralTemplate : CodeTableEntity
{
    protected override int MaxCodeLength => ErpDomainConsts.MaxDeferralTemplateCodeLength;

    /// <summary>The balance sheet account the deferred amount waits on (deferred revenue or prepaid expenses).</summary>
    public string DeferralAccountNo { get; private set; }

    /// <summary>The share of the line amount that is deferred; the rest is recognised at once.</summary>
    public decimal DeferralPct { get; private set; } = 100m;

    public DeferralCalculationMethod CalcMethod { get; private set; }

    public DeferralStartDate StartDate { get; private set; }

    public int NoOfPeriods { get; private set; } = 1;

    /// <summary>The description of each release; "%1" becomes the period, "%2" the document no.</summary>
    public string PeriodDescription { get; private set; }

    protected DeferralTemplate() { }

    public DeferralTemplate(Guid id, string code, string description, string deferralAccountNo, int noOfPeriods)
        : base(id, code, description)
    {
        SetRules(deferralAccountNo, 100m, DeferralCalculationMethod.StraightLine, DeferralStartDate.PostingDate, noOfPeriods, null);
    }

    public void SetRules(
        string deferralAccountNo,
        decimal deferralPct,
        DeferralCalculationMethod calcMethod,
        DeferralStartDate startDate,
        int noOfPeriods,
        string periodDescription
    )
    {
        if (noOfPeriods < 1 || noOfPeriods > ErpDomainConsts.MaxDeferralPeriods)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.DeferralInvalidPeriods).WithData("max", ErpDomainConsts.MaxDeferralPeriods);
        }
        if (deferralPct <= 0m || deferralPct > 100m)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", deferralPct);
        }

        DeferralAccountNo = Check.NotNullOrWhiteSpace(deferralAccountNo, nameof(deferralAccountNo), ErpDomainConsts.MaxNoLength).Trim();
        DeferralPct = deferralPct;
        CalcMethod = calcMethod;
        StartDate = startDate;
        NoOfPeriods = noOfPeriods;
        PeriodDescription = Check.Length(periodDescription, nameof(periodDescription), ErpDomainConsts.MaxDescriptionLength);
    }
}

/// <summary>A posted deferral schedule: what a line deferred and when each part of it was released.</summary>
public class PostedDeferralHeader : CompanyAggregateRoot
{
    public DeferralSourceType SourceType { get; private set; }
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string DeferralCode { get; private set; }

    /// <summary>The income statement account the line posted to.</summary>
    public string AccountNo { get; private set; }

    public string DeferralAccountNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public int NoOfPeriods { get; private set; }

    /// <summary>The deferred amount in LCY, with the sign the line posted to its account.</summary>
    public decimal AmountToDefer { get; private set; }

    public string Description { get; private set; }

    public List<PostedDeferralLine> Lines { get; private set; } = new();

    protected PostedDeferralHeader() { }

    internal PostedDeferralHeader(
        Guid id,
        DeferralSourceType sourceType,
        string documentNo,
        int lineNo,
        DeferralTemplate template,
        string accountNo,
        DateTime postingDate,
        DateTime startDate,
        decimal amountToDefer,
        string description
    )
        : base(id)
    {
        SourceType = sourceType;
        DocumentNo = documentNo;
        LineNo = lineNo;
        DeferralCode = template.Code;
        AccountNo = accountNo;
        DeferralAccountNo = template.DeferralAccountNo;
        PostingDate = postingDate.Date;
        StartDate = startDate.Date;
        NoOfPeriods = template.NoOfPeriods;
        AmountToDefer = amountToDefer;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }
}

public class PostedDeferralLine : Entity<Guid>
{
    public Guid PostedDeferralHeaderId { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }

    protected PostedDeferralLine() { }

    internal PostedDeferralLine(Guid id, Guid headerId, DateTime postingDate, string description, decimal amount)
        : base(id)
    {
        PostedDeferralHeaderId = headerId;
        PostingDate = postingDate.Date;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
    }
}

/// <summary>One release of a deferral schedule.</summary>
public sealed record DeferralPeriod(DateTime PostingDate, decimal Amount);

/// <summary>
/// Splits a deferred amount over monthly periods. Pure, so the schedule can be shown before
/// posting. The last period takes the rounding, so the parts always add up to the amount.
/// </summary>
public static class DeferralScheduleCalculator
{
    public static DateTime FirstDate(DeferralStartDate startDate, DateTime postingDate)
    {
        var date = postingDate.Date;
        var monthStart = new DateTime(date.Year, date.Month, 1);
        return startDate switch
        {
            DeferralStartDate.BeginningOfPeriod => monthStart,
            DeferralStartDate.EndOfPeriod => monthStart.AddMonths(1).AddDays(-1),
            DeferralStartDate.BeginningOfNextPeriod => monthStart.AddMonths(1),
            _ => date,
        };
    }

    public static List<DeferralPeriod> Calculate(DeferralCalculationMethod method, decimal amount, DateTime startDate, int noOfPeriods)
    {
        if (noOfPeriods < 1)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.DeferralInvalidPeriods).WithData("max", ErpDomainConsts.MaxDeferralPeriods);
        }

        var start = startDate.Date;
        var monthStart = new DateTime(start.Year, start.Month, 1);
        var dates = Enumerable.Range(0, noOfPeriods).Select(i => i == 0 ? start : monthStart.AddMonths(i)).ToList();
        List<decimal> weights;

        switch (method)
        {
            case DeferralCalculationMethod.EqualPerPeriod:
                weights = dates.Select(_ => 1m).ToList();
                break;

            case DeferralCalculationMethod.DaysPerPeriod:
                weights = dates.Select((d, i) => (decimal)DaysInPeriod(d, i == 0)).ToList();
                break;

            default:
                // Straight-line: full months weigh one; a first period that starts mid-month weighs its
                // share of days, and the remainder is released in an extra period at the end.
                var firstShare = start.Day == 1 ? 1m : DaysInPeriod(start, true) / (decimal)DateTime.DaysInMonth(start.Year, start.Month);
                weights = dates.Select((_, i) => i == 0 ? firstShare : 1m).ToList();
                if (firstShare < 1m)
                {
                    dates.Add(monthStart.AddMonths(noOfPeriods));
                    weights.Add(1m - firstShare);
                }
                break;
        }

        var total = weights.Sum();
        var periods = new List<DeferralPeriod>();
        var released = 0m;
        for (var i = 0; i < dates.Count; i++)
        {
            var part = i == dates.Count - 1 ? amount - released : Math.Round(amount * weights[i] / total, 2, MidpointRounding.AwayFromZero);
            released += part;
            periods.Add(new DeferralPeriod(dates[i], part));
        }

        return periods;
    }

    private static int DaysInPeriod(DateTime date, bool first)
    {
        var days = DateTime.DaysInMonth(date.Year, date.Month);
        return first ? days - date.Day + 1 : days;
    }
}

/// <summary>
/// Posts a deferral: the deferred share of a line moves from its income statement account to the
/// deferral account on the posting date, then comes back period by period, each release dated in
/// its own period. Everything posts in the same run as the line, so the schedule cannot be lost.
/// </summary>
public class DeferralPoster : DomainService
{
    private readonly IRepository<DeferralTemplate, Guid> _templateRepository;
    private readonly IRepository<PostedDeferralHeader, Guid> _postedRepository;

    private GenJnlPostLine GenJnlPostLine => LazyServiceProvider.LazyGetRequiredService<GenJnlPostLine>();

    public DeferralPoster(IRepository<DeferralTemplate, Guid> templateRepository, IRepository<PostedDeferralHeader, Guid> postedRepository)
    {
        _templateRepository = templateRepository;
        _postedRepository = postedRepository;
    }

    public async Task<DeferralTemplate> GetTemplateAsync(string deferralCode)
    {
        var code = CodeTableEntity.NormalizeCode(deferralCode);
        return await _templateRepository.FirstOrDefaultAsync(t => t.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.FinanceOperations.DeferralTemplateNotFound).WithData("code", code ?? "");
    }

    /// <summary>The schedule a line would get, without posting it.</summary>
    public async Task<List<DeferralPeriod>> PreviewAsync(string deferralCode, decimal amount, DateTime postingDate)
    {
        var template = await GetTemplateAsync(deferralCode);
        var toDefer = Math.Round(amount * template.DeferralPct / 100m, 2, MidpointRounding.AwayFromZero);
        return DeferralScheduleCalculator.Calculate(template.CalcMethod, toDefer, DeferralScheduleCalculator.FirstDate(template.StartDate, postingDate), template.NoOfPeriods);
    }

    /// <summary>
    /// Defers <paramref name="glAmountLcy"/>, which the caller has just posted to
    /// <paramref name="accountNo"/>, and releases it over the template's periods.
    /// </summary>
    public async Task<PostedDeferralHeader> PostAsync(
        string deferralCode,
        DeferralSourceType sourceType,
        string accountNo,
        decimal glAmountLcy,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        int lineNo,
        string description,
        string sourceNo,
        Guid dimensionSetId,
        GLPostingContext context
    )
    {
        var template = await GetTemplateAsync(deferralCode);
        var toDefer = Math.Round(glAmountLcy * template.DeferralPct / 100m, 2, MidpointRounding.AwayFromZero);
        var start = DeferralScheduleCalculator.FirstDate(template.StartDate, postingDate);
        var periods = DeferralScheduleCalculator.Calculate(template.CalcMethod, toDefer, start, template.NoOfPeriods);

        var header = new PostedDeferralHeader(GuidGenerator.Create(), sourceType, documentNo, lineNo, template, accountNo, postingDate, start, toDefer, description);

        // Off the income statement and onto the balance sheet on the posting date.
        await PostAsync(accountNo, -toDefer, postingDate, documentType, documentNo, description, sourceNo, dimensionSetId, context);
        await PostAsync(template.DeferralAccountNo, toDefer, postingDate, documentType, documentNo, description, sourceNo, dimensionSetId, context);

        foreach (var period in periods.Where(p => p.Amount != 0m))
        {
            var text = PeriodText(template, period.PostingDate, documentNo) ?? description;
            await PostAsync(template.DeferralAccountNo, -period.Amount, period.PostingDate, documentType, documentNo, text, sourceNo, dimensionSetId, context);
            await PostAsync(accountNo, period.Amount, period.PostingDate, documentType, documentNo, text, sourceNo, dimensionSetId, context);
            header.Lines.Add(new PostedDeferralLine(GuidGenerator.Create(), header.Id, period.PostingDate, text, period.Amount));
        }

        await _postedRepository.InsertAsync(header);
        return header;
    }

    private Task<GLEntry> PostAsync(string accountNo, decimal amount, DateTime date, GLEntryDocumentType documentType, string documentNo, string description, string sourceNo, Guid dimensionSetId, GLPostingContext context) =>
        GenJnlPostLine.PostGLDirectAsync(accountNo, date, documentType, documentNo, description, amount, sourceNo, dimensionSetId, context);

    private static string PeriodText(DeferralTemplate template, DateTime date, string documentNo) =>
        template.PeriodDescription.IsNullOrWhiteSpace()
            ? null
            : template.PeriodDescription.Replace("%1", date.ToString("MMM yyyy")).Replace("%2", documentNo);
}
