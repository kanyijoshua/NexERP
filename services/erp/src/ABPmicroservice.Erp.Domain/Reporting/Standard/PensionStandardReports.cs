using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Pensions;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// What the pension reports share. Their "No. Filter" is on the member number, and their scheme is
/// chosen with <see cref="StandardReportRequest.SchemeCode"/>; blank runs
/// across every scheme, which is what keeping schemes as a dimension makes possible.
/// </summary>
public abstract class PensionReportBase : StandardReportBase
{
    protected IRepository<PensionMember, Guid> Members => LazyServiceProvider.LazyGetRequiredService<IRepository<PensionMember, Guid>>();

    protected IRepository<MemberLedgerEntry, Guid> Entries => LazyServiceProvider.LazyGetRequiredService<IRepository<MemberLedgerEntry, Guid>>();

    protected static StandardReportDefinition Define(int id, string code, string name, StandardReportParameters parameters) =>
        new(id, code, name, StandardReportAreas.Pensions, parameters | StandardReportParameters.Scheme);

    protected static string SchemeOf(StandardReportRequest request) =>
        request.SchemeCode.IsNullOrWhiteSpace() ? null : request.SchemeCode.Trim().ToUpperInvariant();

    protected async Task<List<PensionMember>> GetMembersAsync(StandardReportRequest request)
    {
        var filter = Filter(request);
        var scheme = SchemeOf(request);

        return (await Members.GetListAsync(m => scheme == null || m.SchemeCode == scheme))
            .Where(m => filter.Matches(m.No))
            .OrderBy(m => m.SchemeCode, StringComparer.Ordinal)
            .ThenBy(m => m.No, StringComparer.Ordinal)
            .ToList();
    }

    protected async Task<ILookup<string, MemberLedgerEntry>> GetEntriesAsync(StandardReportRequest request)
    {
        var scheme = SchemeOf(request);
        var to = request.To;

        return (await Entries.GetListAsync(e => e.PostingDate <= to && (scheme == null || e.SchemeCode == scheme))).ToLookup(e => e.MemberNo, StringComparer.Ordinal);
    }
}

/// <summary>
/// Member Statement: the member's fund brought forward,
/// every movement of the period and the running balance after it.
/// </summary>
public class MemberStatementReport : PensionReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520011, "MemberStatement", "Member Statement", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "postingDate", "Posting Date");
        Text(result, "documentNo", "Document No.");
        Text(result, "description", "Description");
        Text(result, "transactionType", "Transaction Type");
        Text(result, "contributionType", "Contribution Type");
        Text(result, "exemptionType", "Exemption Type");
        Number(result, "amount", "Amount");
        Number(result, "balance", "Balance");

        var entries = await GetEntriesAsync(request);

        foreach (var member in await GetMembersAsync(request))
        {
            var balance = entries[member.No].Where(e => e.PostingDate < request.From).Sum(e => e.Amount);
            var inPeriod = entries[member.No].Where(e => e.PostingDate >= request.From).OrderBy(e => e.PostingDate).ThenBy(e => e.EntryNo).ToList();
            if (balance == 0m && inPeriod.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("documentNo", member.No), ("description", $"{member.FullName} ({member.SchemeCode})"), ("balance", balance));

            foreach (var entry in inPeriod)
            {
                balance += entry.Amount;
                Row(
                    result,
                    ("postingDate", entry.PostingDate),
                    ("documentNo", entry.DocumentNo),
                    ("description", entry.Description),
                    ("transactionType", ErpEntityFieldNames.Humanize(entry.TransactionType)),
                    ("contributionType", ErpEntityFieldNames.Humanize(entry.ContributionType)),
                    ("exemptionType", ErpEntityFieldNames.Humanize(entry.ExemptionType)),
                    ("amount", entry.Amount),
                    ("balance", balance)
                ).Indentation = 1;
            }
        }

        return result;
    }
}

/// <summary>
/// Member Balances: each member's fund as at a date,
/// split into the member's and the employer's money and into registered and unregistered.
/// </summary>
public class MemberBalancesReport : PensionReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520141, "MemberBalances", "Member Balances", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "scheme", "Scheme");
        Text(result, "no", "Member No.");
        Text(result, "name", "Name");
        Text(result, "sponsorNo", "Sponsor No.");
        Text(result, "status", "Status");
        Number(result, "employee", "Employee");
        Number(result, "employer", "Employer");
        Number(result, "registered", "Registered");
        Number(result, "unregistered", "Unregistered");
        Number(result, "total", "Fund Value");

        var entries = await GetEntriesAsync(request);
        var totals = new decimal[5];

        foreach (var member in await GetMembersAsync(request))
        {
            var balances = new MemberBalances(entries[member.No]);
            if (balances.Total == 0m && !entries[member.No].Any())
            {
                continue;
            }

            Row(
                result,
                ("scheme", member.SchemeCode),
                ("no", member.No),
                ("name", member.FullName),
                ("sponsorNo", member.SponsorNo),
                ("status", ErpEntityFieldNames.Humanize(member.Status)),
                ("employee", balances.Employee),
                ("employer", balances.Employer),
                ("registered", balances.Registered),
                ("unregistered", balances.Unregistered),
                ("total", balances.Total)
            );

            totals[0] += balances.Employee;
            totals[1] += balances.Employer;
            totals[2] += balances.Registered;
            totals[3] += balances.Unregistered;
            totals[4] += balances.Total;
        }

        BoldRow(
            result,
            ("scheme", string.Empty),
            ("name", "Total"),
            ("employee", totals[0]),
            ("employer", totals[1]),
            ("registered", totals[2]),
            ("unregistered", totals[3]),
            ("total", totals[4])
        );
        return result;
    }
}

/// <summary>
/// Contributions Register: what was contributed
/// for each member in the period, by whose money it is.
/// </summary>
public class ContributionsRegisterReport : PensionReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520132, "ContributionsRegister", "Contributions Register", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "scheme", "Scheme");
        Text(result, "no", "Member No.");
        Text(result, "name", "Name");
        Text(result, "sponsorNo", "Sponsor No.");
        Number(result, "employee", "Employee");
        Number(result, "employeeAvc", "Employee AVC");
        Number(result, "employer", "Employer");
        Number(result, "employerAvc", "Employer AVC");
        Number(result, "total", "Total");

        var entries = await GetEntriesAsync(request);
        var totals = new decimal[5];

        foreach (var member in await GetMembersAsync(request))
        {
            var contributions = entries[member.No]
                .Where(e => e.TransactionType == PensionTransactionType.Contribution && e.PostingDate >= request.From)
                .ToList();
            if (contributions.Count == 0)
            {
                continue;
            }

            decimal Of(PensionContributionType type) => contributions.Where(e => e.ContributionType == type).Sum(e => e.Amount);

            var values = new[]
            {
                Of(PensionContributionType.EmployeeContribution),
                Of(PensionContributionType.EmployeeAdditional),
                Of(PensionContributionType.EmployerContribution),
                Of(PensionContributionType.EmployerAdditional),
                contributions.Sum(e => e.Amount),
            };

            Row(
                result,
                ("scheme", member.SchemeCode),
                ("no", member.No),
                ("name", member.FullName),
                ("sponsorNo", member.SponsorNo),
                ("employee", values[0]),
                ("employeeAvc", values[1]),
                ("employer", values[2]),
                ("employerAvc", values[3]),
                ("total", values[4])
            );

            for (var i = 0; i < values.Length; i++)
            {
                totals[i] += values[i];
            }
        }

        BoldRow(
            result,
            ("scheme", string.Empty),
            ("name", "Total"),
            ("employee", totals[0]),
            ("employeeAvc", totals[1]),
            ("employer", totals[2]),
            ("employerAvc", totals[3]),
            ("total", totals[4])
        );
        return result;
    }
}

/// <summary>Exits Worksheet: the exits of the period and what each one pays.</summary>
public class ExitsWorksheetReport : PensionReportBase
{
    private readonly IRepository<MemberExit, Guid> _exits;

    public ExitsWorksheetReport(IRepository<MemberExit, Guid> exits)
    {
        _exits = exits;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(51520026, "ExitsWorksheet", "Exits Worksheet", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Date(result, "exitDate", "Exit Date");
        Text(result, "scheme", "Scheme");
        Text(result, "memberNo", "Member No.");
        Text(result, "memberName", "Name");
        Text(result, "reason", "Reason for Exit Code");
        Text(result, "status", "Status");
        Number(result, "gross", "Lumpsum Amount");
        Number(result, "tax", "Tax on Lumpsum Amount");
        Number(result, "net", "Net Payable");
        Number(result, "deferred", "Deferred Amount");

        var filter = Filter(request);
        var scheme = SchemeOf(request);
        var exits = (await _exits.GetListAsync(e => e.ExitDate >= request.From && e.ExitDate <= request.To && (scheme == null || e.SchemeCode == scheme)))
            .Where(e => filter.Matches(e.MemberNo) && e.Status != MemberExitStatus.Canceled)
            .OrderBy(e => e.ExitDate)
            .ThenBy(e => e.No, StringComparer.Ordinal)
            .ToList();

        foreach (var exit in exits)
        {
            Row(
                result,
                ("no", exit.No),
                ("exitDate", exit.ExitDate),
                ("scheme", exit.SchemeCode),
                ("memberNo", exit.MemberNo),
                ("memberName", exit.MemberName),
                ("reason", exit.ReasonCode),
                ("status", ErpEntityFieldNames.Humanize(exit.Status)),
                ("gross", exit.GrossLumpsum),
                ("tax", exit.TaxOnLumpsum),
                ("net", exit.NetPayable),
                ("deferred", exit.DeferredAmount)
            );
        }

        BoldRow(
            result,
            ("memberName", "Total"),
            ("gross", exits.Sum(e => e.GrossLumpsum)),
            ("tax", exits.Sum(e => e.TaxOnLumpsum)),
            ("net", exits.Sum(e => e.NetPayable)),
            ("deferred", exits.Sum(e => e.DeferredAmount))
        );
        return result;
    }
}

/// <summary>What the member listings share: the columns of a member, and each member's contributions to date.</summary>
public abstract class MemberListingReportBase : PensionReportBase
{
    private IRepository<PensionSponsor, Guid> Sponsors => LazyServiceProvider.LazyGetRequiredService<IRepository<PensionSponsor, Guid>>();

    /// <summary>Which of the scheme's members the listing shows.</summary>
    protected abstract bool Includes(PensionMember member, StandardReportRequest request);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Name");
        Text(result, "sponsorName", "Name of Sponsor");
        Date(result, "dateOfBirth", "Date of Birth");
        Text(result, "gender", "Gender");
        Text(result, "nationalId", "National ID");
        Text(result, "payrollNo", "Payroll No.");
        Date(result, "joinSchemeDate", "Join Scheme Date");
        Text(result, "maritalStatus", "Marital Status");
        Date(result, "retirementDate", "Date of Normal Retirement");
        Text(result, "status", "Status");
        Number(result, "salary", "Salary");
        Number(result, "employee", "Employee Contribution");
        Number(result, "employer", "Employer Contribution");

        var sponsors = (await Sponsors.GetListAsync()).ToDictionary(s => s.No, s => s.Name, StringComparer.Ordinal);
        var entries = await GetEntriesAsync(request);
        var members = (await GetMembersAsync(request)).Where(m => Includes(m, request)).ToList();
        var totals = new decimal[3];

        foreach (var member in members)
        {
            var balances = new MemberBalances(entries[member.No]);

            Row(
                result,
                ("no", member.No),
                ("name", member.FullName),
                ("sponsorName", sponsors.GetValueOrDefault(member.SponsorNo, member.SponsorNo)),
                ("dateOfBirth", member.DateOfBirth),
                ("gender", ErpEntityFieldNames.Humanize(member.Gender)),
                ("nationalId", member.NationalId),
                ("payrollNo", member.PayrollNo),
                ("joinSchemeDate", member.JoinSchemeDate),
                ("maritalStatus", ErpEntityFieldNames.Humanize(member.MaritalStatus)),
                ("retirementDate", member.ExpectedRetirementDate),
                ("status", ErpEntityFieldNames.Humanize(member.Status)),
                ("salary", member.CurrentSalary),
                ("employee", balances.Employee),
                ("employer", balances.Employer)
            );

            totals[0] += member.CurrentSalary;
            totals[1] += balances.Employee;
            totals[2] += balances.Employer;
        }

        BoldRow(result, ("no", string.Empty), ("name", $"Total: {members.Count} member(s)"), ("salary", totals[0]), ("employee", totals[1]), ("employer", totals[2]));
        return result;
    }
}

/// <summary>Member Listing: every member of the scheme.</summary>
public class MemberListingReport : MemberListingReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520116, "MemberListing", "Member Listing", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    protected override bool Includes(PensionMember member, StandardReportRequest request) => true;
}

/// <summary>Active Members: the members still being contributed for.</summary>
public class ActiveMembersReport : MemberListingReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520118, "ActiveMembers", "Active Members", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    protected override bool Includes(PensionMember member, StandardReportRequest request) => member.Status == MemberStatus.Active;
}

/// <summary>Expected Retirees: the members who reach normal retirement in the period.</summary>
public class ExpectedRetireesReport : MemberListingReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520108, "ExpectedRetirees", "Expected Retirees", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    protected override bool Includes(PensionMember member, StandardReportRequest request) =>
        member.ExpectedRetirementDate >= request.From && member.ExpectedRetirementDate <= request.To && member.Status is MemberStatus.Active or MemberStatus.Dormant or MemberStatus.Deferred;
}

/// <summary>Enum values as people read them: "TaxOnInterest" becomes "Tax On Interest".</summary>
internal static class ErpEntityFieldNames
{
    public static string Humanize<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var name = value.ToString();
        return name == "None" ? string.Empty : Exporting.ErpEntityField.Humanize(name);
    }
}
