using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// The contribution rates a sponsor pays at from a date. When a sponsor's rates change, the old
/// schedules keep the old rates: a schedule takes the rates in force for its contribution period,
/// and the sponsor's own rates only when no dated rate covers the period.
/// </summary>
public class PensionContributionRate : CompanyEntity
{
    public string SponsorNo { get; private set; }
    public DateTime StartDate { get; private set; }

    /// <summary>Blank keeps the rates in force until a later rate starts.</summary>
    public DateTime? EndDate { get; private set; }

    public decimal EmployeeRatePct { get; private set; }
    public decimal EmployerRatePct { get; private set; }

    protected PensionContributionRate() { }

    public PensionContributionRate(Guid id, string sponsorNo, DateTime startDate)
        : base(id)
    {
        Set(sponsorNo, startDate, null, 0m, 0m);
    }

    public void Set(string sponsorNo, DateTime startDate, DateTime? endDate, decimal employeeRatePct, decimal employerRatePct)
    {
        if (employeeRatePct is < 0 or > 100 || employerRatePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", employeeRatePct is < 0 or > 100 ? employeeRatePct : employerRatePct);
        }

        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }

        SponsorNo = Check.NotNullOrWhiteSpace(sponsorNo, nameof(sponsorNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        StartDate = startDate.Date;
        EndDate = endDate?.Date;
        EmployeeRatePct = employeeRatePct;
        EmployerRatePct = employerRatePct;
    }

    public bool Covers(DateTime date) => StartDate <= date.Date && (!EndDate.HasValue || EndDate.Value >= date.Date);

    public bool Overlaps(PensionContributionRate other) =>
        StartDate <= (other.EndDate ?? DateTime.MaxValue) && (EndDate ?? DateTime.MaxValue) >= other.StartDate;
}

/// <summary>
/// One band of a sponsor's vesting scale: from this many years of pensionable service, a leaving
/// member takes this share of the employer's money. A sponsor without a scale vests by the exit
/// reason's employer portion.
/// </summary>
public class PensionVestingScale : CompanyEntity
{
    public string SponsorNo { get; private set; }

    /// <summary>The band applies from this much service (in years) until the next band starts.</summary>
    public decimal FromServiceYears { get; private set; }

    public decimal EmployerVestedPct { get; private set; }

    protected PensionVestingScale() { }

    public PensionVestingScale(Guid id, string sponsorNo, decimal fromServiceYears)
        : base(id)
    {
        SetKey(sponsorNo, fromServiceYears);
    }

    public void SetKey(string sponsorNo, decimal fromServiceYears)
    {
        if (fromServiceYears < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "From Service Years");
        }

        SponsorNo = Check.NotNullOrWhiteSpace(sponsorNo, nameof(sponsorNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        FromServiceYears = fromServiceYears;
    }

    public void Set(decimal employerVestedPct)
    {
        EmployerVestedPct = employerVestedPct is < 0 or > 100
            ? throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", employerVestedPct)
            : employerVestedPct;
    }
}

/// <summary>The rates and vesting a sponsor's members are under at a given time.</summary>
public class SponsorTermsManager : DomainService
{
    private readonly IRepository<PensionContributionRate, Guid> _rates;
    private readonly IRepository<PensionVestingScale, Guid> _vesting;

    public SponsorTermsManager(IRepository<PensionContributionRate, Guid> rates, IRepository<PensionVestingScale, Guid> vesting)
    {
        _rates = rates;
        _vesting = vesting;
    }

    /// <summary>The employee and employer rates in force for a contribution period.</summary>
    public async Task<(decimal EmployeeRatePct, decimal EmployerRatePct)> GetRatesAsync(PensionSponsor sponsor, DateTime period)
    {
        var rate = (await _rates.GetListAsync(r => r.SponsorNo == sponsor.No))
            .Where(r => r.Covers(period))
            .OrderByDescending(r => r.StartDate)
            .FirstOrDefault();

        return rate == null ? (sponsor.EmployeeRatePct, sponsor.EmployerRatePct) : (rate.EmployeeRatePct, rate.EmployerRatePct);
    }

    /// <summary>
    /// The share of the employer's money vested after a length of service, as a percentage, or null
    /// when the sponsor has no vesting scale.
    /// </summary>
    public async Task<decimal?> GetEmployerVestedPctAsync(string sponsorNo, decimal serviceYears)
    {
        var scale = await _vesting.GetListAsync(v => v.SponsorNo == sponsorNo);
        if (scale.Count == 0)
        {
            return null;
        }

        return scale.Where(v => v.FromServiceYears <= serviceYears).OrderByDescending(v => v.FromServiceYears).FirstOrDefault()?.EmployerVestedPct ?? 0m;
    }
}
