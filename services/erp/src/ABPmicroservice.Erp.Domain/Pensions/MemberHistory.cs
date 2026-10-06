using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// One change of a member's status: when the member joined, went dormant, came back, was deferred or
/// left. The member card holds only the current status; this is how it came to be, which is what a
/// membership movement report counts.
/// </summary>
public class MemberStatusEntry : CompanyEntity
{
    public string MemberNo { get; private set; }
    public string SchemeCode { get; private set; }
    public string SponsorNo { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public MemberStatus FromStatus { get; private set; }
    public MemberStatus ToStatus { get; private set; }

    /// <summary>The exit, schedule or calculation that changed the status; blank for a change made on the card.</summary>
    public string DocumentNo { get; private set; }

    public string UserName { get; private set; }

    protected MemberStatusEntry() { }

    public MemberStatusEntry(Guid id, PensionMember member, MemberStatus fromStatus, DateTime effectiveDate, string documentNo, string userName)
        : base(id)
    {
        MemberNo = member.No;
        SchemeCode = member.SchemeCode;
        SponsorNo = member.SponsorNo;
        FromStatus = fromStatus;
        ToStatus = member.Status;
        EffectiveDate = effectiveDate.Date;
        DocumentNo = documentNo;
        UserName = userName;
    }
}

/// <summary>A member's pensionable salary for one month, as a posted schedule reported it.</summary>
public class MemberSalaryEntry : CompanyEntity
{
    public string MemberNo { get; private set; }
    public string SponsorNo { get; private set; }

    /// <summary>The first day of the month.</summary>
    public DateTime Period { get; private set; }

    public decimal Salary { get; private set; }
    public string DocumentNo { get; private set; }

    protected MemberSalaryEntry() { }

    public MemberSalaryEntry(Guid id, string memberNo, string sponsorNo, DateTime period)
        : base(id)
    {
        MemberNo = memberNo;
        SponsorNo = sponsorNo;
        Period = new DateTime(period.Year, period.Month, 1);
    }

    internal void Set(decimal salary, string documentNo)
    {
        Salary = salary;
        DocumentNo = documentNo;
    }
}

/// <summary>Writes the status and salary history of members.</summary>
public class MemberHistoryRecorder : DomainService
{
    private readonly IRepository<MemberStatusEntry, Guid> _statuses;
    private readonly IRepository<MemberSalaryEntry, Guid> _salaries;
    private readonly ICurrentUser _currentUser;

    public MemberHistoryRecorder(IRepository<MemberStatusEntry, Guid> statuses, IRepository<MemberSalaryEntry, Guid> salaries, ICurrentUser currentUser)
    {
        _statuses = statuses;
        _salaries = salaries;
        _currentUser = currentUser;
    }

    /// <summary>Records the member's current status, when it differs from <paramref name="fromStatus"/>.</summary>
    public async Task RecordStatusAsync(PensionMember member, MemberStatus fromStatus, DateTime effectiveDate, string documentNo = null)
    {
        if (member.Status == fromStatus)
        {
            return;
        }

        await _statuses.InsertAsync(new MemberStatusEntry(GuidGenerator.Create(), member, fromStatus, effectiveDate, documentNo, _currentUser.UserName));
    }

    /// <summary>Keeps one salary per member and month: a later schedule for the month replaces what an earlier one said.</summary>
    public async Task RecordSalaryAsync(PensionMember member, DateTime period, decimal salary, string documentNo)
    {
        if (salary <= 0m)
        {
            return;
        }

        var month = new DateTime(period.Year, period.Month, 1);
        var entry = await _salaries.FirstOrDefaultAsync(s => s.MemberNo == member.No && s.Period == month);

        if (entry == null)
        {
            entry = new MemberSalaryEntry(GuidGenerator.Create(), member.No, member.SponsorNo, month);
            entry.Set(salary, documentNo);
            await _salaries.InsertAsync(entry);
        }
        else
        {
            entry.Set(salary, documentNo);
            await _salaries.UpdateAsync(entry);
        }
    }
}
