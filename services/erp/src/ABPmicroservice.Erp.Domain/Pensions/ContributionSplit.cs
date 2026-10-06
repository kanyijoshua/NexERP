using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// The most a member's contributions for one month may come to and still be registered (tax
/// exempt), from a date until the next limit takes over. What is contributed above it is
/// unregistered money, which is taxed differently on exit.
/// </summary>
public class PensionTaxReliefLimit : CompanyEntity
{
    public DateTime EffectiveDate { get; private set; }
    public decimal MonthlyLimit { get; private set; }

    protected PensionTaxReliefLimit() { }

    public PensionTaxReliefLimit(Guid id, DateTime effectiveDate, decimal monthlyLimit)
        : base(id)
    {
        Set(effectiveDate, monthlyLimit);
    }

    public void Set(DateTime effectiveDate, decimal monthlyLimit)
    {
        if (monthlyLimit < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Monthly Limit");
        }

        EffectiveDate = effectiveDate.Date;
        MonthlyLimit = monthlyLimit;
    }
}

/// <summary>What a member contributes for a month, before it is split into registered and unregistered money.</summary>
public readonly record struct ContributionAmounts(decimal Employee, decimal EmployeeAvc, decimal Employer, decimal EmployerAvc)
{
    public decimal Total => Employee + EmployeeAvc + Employer + EmployerAvc;

    /// <summary>The amounts of a schedule line, whichever way they are split now.</summary>
    public static ContributionAmounts Of(IReadOnlyList<decimal> amounts) =>
        new(amounts[0] + amounts[1], amounts[2] + amounts[3], amounts[4] + amounts[5], amounts[6] + amounts[7]);
}

/// <summary>
/// Splits contributions into registered and unregistered money by the monthly tax relief limit.
/// <para>
/// Under employee priority the limit is used up by the employee contribution, then the employee's
/// voluntary contributions, then the employer's, then the employer's voluntary ones; whatever finds
/// no room under the limit is unregistered. Under contribution rate the limit is first shared
/// between the employee and the employer in proportion to their rates, and each side uses its own part.
/// </para>
/// </summary>
public static class ContributionSplitter
{
    /// <summary>
    /// The eight amounts in the order of <see cref="MoneyType.All"/>. A null limit means none is in
    /// force, and everything is registered.
    /// </summary>
    public static decimal[] Split(
        ContributionAmounts amounts,
        decimal? limit,
        ExcessContributionAllocation allocation,
        decimal employeeRatePct,
        decimal employerRatePct
    )
    {
        if (!limit.HasValue)
        {
            return [amounts.Employee, 0m, amounts.EmployeeAvc, 0m, amounts.Employer, 0m, amounts.EmployerAvc, 0m];
        }

        var room = Math.Max(0m, limit.Value);
        var result = new decimal[8];

        if (allocation == ExcessContributionAllocation.ContributionRate)
        {
            var rates = employeeRatePct + employerRatePct;
            var employeeShare = rates > 0m
                ? employeeRatePct / rates
                : amounts.Total > 0m ? (amounts.Employee + amounts.EmployeeAvc) / amounts.Total : 0.5m;

            var employeeRoom = Math.Round(room * employeeShare, 2, MidpointRounding.AwayFromZero);
            var employerRoom = room - employeeRoom;

            Fill(result, 0, amounts.Employee, ref employeeRoom);
            Fill(result, 2, amounts.EmployeeAvc, ref employeeRoom);
            Fill(result, 4, amounts.Employer, ref employerRoom);
            Fill(result, 6, amounts.EmployerAvc, ref employerRoom);
        }
        else
        {
            Fill(result, 0, amounts.Employee, ref room);
            Fill(result, 2, amounts.EmployeeAvc, ref room);
            Fill(result, 4, amounts.Employer, ref room);
            Fill(result, 6, amounts.EmployerAvc, ref room);
        }

        return result;
    }

    private static void Fill(decimal[] result, int index, decimal amount, ref decimal room)
    {
        var registered = Math.Min(amount, room);
        result[index] = registered;
        result[index + 1] = amount - registered;
        room -= registered;
    }
}

/// <summary>Finds the tax relief limit for a month and what of it a member has already used.</summary>
public class TaxReliefLimitManager : DomainService
{
    private readonly IRepository<PensionTaxReliefLimit, Guid> _limits;
    private readonly IRepository<MemberLedgerEntry, Guid> _entries;

    public TaxReliefLimitManager(IRepository<PensionTaxReliefLimit, Guid> limits, IRepository<MemberLedgerEntry, Guid> entries)
    {
        _limits = limits;
        _entries = entries;
    }

    /// <summary>The monthly limit in force for a contribution period, or null when no limit has been set.</summary>
    public async Task<decimal?> GetLimitAsync(DateTime period)
    {
        var date = period.Date;
        var limit = (await _limits.GetListAsync(l => l.EffectiveDate <= date)).OrderByDescending(l => l.EffectiveDate).FirstOrDefault();
        return limit?.MonthlyLimit;
    }

    /// <summary>
    /// The registered contributions already posted for each member for the month, so that a second
    /// schedule for the same month (arrears, voluntary contributions) only uses what is left of the limit.
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetUsedAsync(IReadOnlyCollection<string> memberNos, DateTime period)
    {
        var month = new DateTime(period.Year, period.Month, 1);
        var numbers = memberNos.ToList();

        return (await _entries.GetListAsync(e =>
                numbers.Contains(e.MemberNo)
                && e.ContributionPeriod == month
                && e.TransactionType == PensionTransactionType.Contribution
                && e.ExemptionType == PensionExemptionType.TaxExempt
            ))
            .GroupBy(e => e.MemberNo)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount), StringComparer.Ordinal);
    }
}
