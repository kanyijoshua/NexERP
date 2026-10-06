using System;
using System.Collections.Generic;
using System.Globalization;
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
/// Member Balances: each member's fund as at a date, split into what was contributed and the
/// interest it earned, for the member's own money, the employer's and the voluntary contributions.
/// </summary>
public class MemberBalancesReport : PensionReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520141, "MemberBalances", "Member Balances", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    private static readonly (string Key, string Header)[] Figures =
    [
        ("employeeContribution", "Employee Contribution"),
        ("employeeInterest", "Employee Interest"),
        ("employerContribution", "Employer Contribution"),
        ("employerInterest", "Employer Interest"),
        ("avcContribution", "AVC Contribution"),
        ("avcInterest", "AVC Interest"),
        ("total", "Total Balance"),
    ];

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "PF No.");
        Text(result, "name", "Name");
        foreach (var (key, header) in Figures)
        {
            Number(result, key, header);
        }

        var entries = await GetEntriesAsync(request);
        var totals = new decimal[Figures.Length];

        foreach (var member in await GetMembersAsync(request))
        {
            var own = entries[member.No].ToList();
            if (own.Count == 0)
            {
                continue;
            }

            var values = new[]
            {
                Of(own, PensionShare.Employee, interest: false),
                Of(own, PensionShare.Employee, interest: true),
                Of(own, PensionShare.Employer, interest: false),
                Of(own, PensionShare.Employer, interest: true),
                Of(own, PensionShare.Voluntary, interest: false),
                Of(own, PensionShare.Voluntary, interest: true),
                own.Sum(e => e.Amount),
            };

            var row = Row(result, ("no", member.No), ("name", member.FullName));
            for (var i = 0; i < values.Length; i++)
            {
                row.Values[Figures[i].Key] = values[i];
                totals[i] += values[i];
            }
        }

        var total = BoldRow(result, ("no", string.Empty), ("name", "Total"));
        for (var i = 0; i < totals.Length; i++)
        {
            total.Values[Figures[i].Key] = totals[i];
        }

        return result;
    }

    private static decimal Of(IEnumerable<MemberLedgerEntry> entries, PensionShare share, bool interest) =>
        entries
            .Where(e => PensionShares.Of(e.ContributionType) == share && PensionShares.IsInterest(e.TransactionType) == interest)
            .Sum(e => e.Amount);
}

/// <summary>
/// Contributions Register: for each member, what the period brought in and paid out of each kind of
/// money, registered and unregistered, and the balance of each at the end of the period. Transfers
/// from other schemes are shown apart from the contributions.
/// </summary>
public class ContributionsRegisterReport : PensionReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520132, "ContributionsRegister", "Contributions Register", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    private static readonly (string Key, string Header)[] Kinds =
    [
        ("Employee", "Employee"),
        ("Employer", "Employer"),
        ("EmployeeAvc", "Employee AVC"),
        ("EmployerAvc", "Employer AVC"),
        ("TransfersIn", "Transfers In"),
    ];

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Name");

        var keys = new List<string>();
        foreach (var (prefix, label) in new[] { ("reg", "Reg."), ("unreg", "Unreg.") })
        {
            foreach (var (kind, header) in Kinds)
            {
                Number(result, prefix + kind, $"{label} {header}");
                keys.Add(prefix + kind);
            }

            Number(result, prefix + "Withdrawals", $"{label} Withdrawals");
            keys.Add(prefix + "Withdrawals");
        }

        foreach (var (prefix, label) in new[] { ("closingReg", "Closing Reg."), ("closingUnreg", "Closing Unreg.") })
        {
            foreach (var (kind, header) in Kinds)
            {
                Number(result, prefix + kind, $"{label} {header}");
                keys.Add(prefix + kind);
            }
        }

        var entries = await GetEntriesAsync(request);
        var totals = keys.ToDictionary(k => k, _ => 0m);

        foreach (var member in await GetMembersAsync(request))
        {
            var own = entries[member.No].ToList();
            var inPeriod = own.Where(e => e.PostingDate >= request.From).ToList();
            if (inPeriod.Count == 0)
            {
                continue;
            }

            var values = new Dictionary<string, decimal>();
            foreach (var (prefix, exemption) in new[] { ("reg", PensionExemptionType.TaxExempt), ("unreg", PensionExemptionType.NonTaxExempt) })
            {
                var movements = inPeriod.Where(e => e.ExemptionType == exemption).ToList();
                var contributions = movements.Where(e => e.TransactionType == PensionTransactionType.Contribution).ToList();

                foreach (var (kind, _) in Kinds)
                {
                    values[prefix + kind] = contributions.Where(e => KindOf(e) == kind).Sum(e => e.Amount);
                }

                values[prefix + "Withdrawals"] = -movements.Where(e => e.TransactionType == PensionTransactionType.Withdrawal).Sum(e => e.Amount);

                var closingPrefix = prefix == "reg" ? "closingReg" : "closingUnreg";
                foreach (var (kind, _) in Kinds)
                {
                    values[closingPrefix + kind] = own.Where(e => e.ExemptionType == exemption && KindOf(e) == kind).Sum(e => e.Amount);
                }
            }

            var row = Row(result, ("no", member.No), ("name", member.FullName));
            foreach (var key in keys)
            {
                row.Values[key] = values[key];
                totals[key] += values[key];
            }
        }

        var total = BoldRow(result, ("no", string.Empty), ("name", "Total"));
        foreach (var key in keys)
        {
            total.Values[key] = totals[key];
        }

        return result;
    }

    /// <summary>
    /// Which column an entry belongs to: money that came in by transfer stays with the transfers,
    /// everything else (contributions, interest, withdrawals) with the kind of money it is.
    /// </summary>
    private static string KindOf(MemberLedgerEntry entry)
    {
        if (entry.ContributionMode == PensionContributionMode.TransferIn)
        {
            return "TransfersIn";
        }

        return entry.ContributionType switch
        {
            PensionContributionType.EmployeeAdditional => "EmployeeAvc",
            PensionContributionType.EmployerAdditional => "EmployerAvc",
            _ => PensionShares.Of(entry.ContributionType) == PensionShare.Employer ? "Employer" : "Employee",
        };
    }
}

/// <summary>
/// Member Contributions Statement: each member's contributions month by month, registered and
/// unregistered, for the member and the employer, with arrears apart from the contributions of the
/// month, a subtotal for each year and a grand total.
/// </summary>
public class MemberContributionsStatementReport : PensionReportBase
{
    private IRepository<PensionSponsor, Guid> Sponsors => LazyServiceProvider.LazyGetRequiredService<IRepository<PensionSponsor, Guid>>();

    public override StandardReportDefinition Definition { get; } =
        Define(51520155, "MemberContributionsStatement", "Member Contributions Statement", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    private static readonly (string Key, PensionExemptionType Exemption, bool Employee, bool Arrears, string Header)[] Figures =
    [
        ("regEmployee", PensionExemptionType.TaxExempt, true, false, "Reg. Employee Contrib."),
        ("regEmployeeArrears", PensionExemptionType.TaxExempt, true, true, "Reg. Employee Arrears"),
        ("regEmployer", PensionExemptionType.TaxExempt, false, false, "Reg. Employer Contrib."),
        ("regEmployerArrears", PensionExemptionType.TaxExempt, false, true, "Reg. Employer Arrears"),
        ("unregEmployee", PensionExemptionType.NonTaxExempt, true, false, "Unreg. Employee Contrib."),
        ("unregEmployeeArrears", PensionExemptionType.NonTaxExempt, true, true, "Unreg. Employee Arrears"),
        ("unregEmployer", PensionExemptionType.NonTaxExempt, false, false, "Unreg. Employer Contrib."),
        ("unregEmployerArrears", PensionExemptionType.NonTaxExempt, false, true, "Unreg. Employer Arrears"),
    ];

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "period", "Period");
        foreach (var figure in Figures)
        {
            Number(result, figure.Key, figure.Header);
        }

        Number(result, "total", "Total");

        var sponsors = (await Sponsors.GetListAsync()).ToDictionary(s => s.No, s => s.Name, StringComparer.Ordinal);
        var entries = await GetEntriesAsync(request);

        foreach (var member in await GetMembersAsync(request))
        {
            var contributions = entries[member.No]
                .Where(e => e.TransactionType == PensionTransactionType.Contribution && e.PostingDate >= request.From)
                .ToList();
            if (contributions.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("period", $"PF No.: {member.No}   Name: {member.FullName}"));
            BoldRow(result, ("period", $"Sponsor: {sponsors.GetValueOrDefault(member.SponsorNo, member.SponsorNo)}   Designation: {member.Designation}"));

            var grand = new decimal[Figures.Length + 1];

            foreach (var year in contributions.GroupBy(e => MonthOf(e).Year).OrderBy(g => g.Key))
            {
                BoldRow(result, ("period", year.Key.ToString(CultureInfo.InvariantCulture))).Indentation = 1;
                var subtotal = new decimal[Figures.Length + 1];

                foreach (var month in year.GroupBy(MonthOf).OrderBy(g => g.Key))
                {
                    var row = Row(result, ("period", month.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture)));
                    row.Indentation = 2;
                    Fill(row, month, subtotal);
                }

                var sub = BoldRow(result, ("period", "Sub totals"));
                sub.Indentation = 1;
                Put(sub, subtotal);

                for (var i = 0; i < grand.Length; i++)
                {
                    grand[i] += subtotal[i];
                }
            }

            Put(BoldRow(result, ("period", "Grand Totals")), grand);
        }

        return result;
    }

    private static DateTime MonthOf(MemberLedgerEntry entry)
    {
        var date = entry.ContributionPeriod ?? entry.PostingDate;
        return new DateTime(date.Year, date.Month, 1);
    }

    private static void Fill(ReportRow row, IEnumerable<MemberLedgerEntry> entries, decimal[] subtotal)
    {
        var list = entries.ToList();
        var values = new decimal[Figures.Length + 1];

        for (var i = 0; i < Figures.Length; i++)
        {
            var figure = Figures[i];
            values[i] = list
                .Where(e =>
                    e.ExemptionType == figure.Exemption
                    && e.MoneyType.IsEmployee == figure.Employee
                    && (e.ContributionMode == PensionContributionMode.Arrears) == figure.Arrears
                )
                .Sum(e => e.Amount);
        }

        values[^1] = list.Sum(e => e.Amount);
        Put(row, values);

        for (var i = 0; i < values.Length; i++)
        {
            subtotal[i] += values[i];
        }
    }

    private static void Put(ReportRow row, decimal[] values)
    {
        for (var i = 0; i < Figures.Length; i++)
        {
            row.Values[Figures[i].Key] = values[i];
        }

        row.Values["total"] = values[^1];
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

/// <summary>One column of a member listing: where its value comes from.</summary>
public sealed record MemberListingColumn(string Key, string Header, ReportColumnKind Kind, Func<MemberListingRow, object> Value, bool Totalled = false);

/// <summary>What a member listing knows about one member when it fills a row.</summary>
public sealed record MemberListingRow(PensionMember Member, string SponsorName, MemberBalances Balances);

/// <summary>
/// What the member listings share: the members of the scheme the listing picks, one row each in
/// the listing's own columns, and a total line counting them.
/// </summary>
public abstract class MemberListingReportBase : PensionReportBase
{
    private IRepository<PensionSponsor, Guid> Sponsors => LazyServiceProvider.LazyGetRequiredService<IRepository<PensionSponsor, Guid>>();

    /// <summary>Which of the scheme's members the listing shows.</summary>
    protected abstract bool Includes(PensionMember member, StandardReportRequest request);

    /// <summary>The listing's columns, in order. The first text column after the first one carries the count.</summary>
    protected abstract IReadOnlyList<MemberListingColumn> Columns { get; }

    /// <summary>The column the total line writes "Total: n member(s)" in.</summary>
    protected virtual string CountColumn => "name";

    protected static MemberListingColumn No => new("no", "No.", ReportColumnKind.Text, r => r.Member.No);
    protected static MemberListingColumn Name => new("name", "Name", ReportColumnKind.Text, r => r.Member.FullName.ToUpperInvariant());
    protected static MemberListingColumn Sponsor => new("sponsorName", "Name of Sponsor", ReportColumnKind.Text, r => r.SponsorName?.ToUpperInvariant());
    protected static MemberListingColumn BirthDate => new("dateOfBirth", "Date of Birth", ReportColumnKind.Date, r => r.Member.DateOfBirth);
    protected static MemberListingColumn Gender => new("gender", "Gender", ReportColumnKind.Text, r => ErpEntityFieldNames.Humanize(r.Member.Gender));
    protected static MemberListingColumn NationalId => new("nationalId", "National ID", ReportColumnKind.Text, r => r.Member.NationalId);
    protected static MemberListingColumn PayrollNo => new("payrollNo", "Payroll No.", ReportColumnKind.Text, r => r.Member.PayrollNo);
    protected static MemberListingColumn JoinSchemeDate => new("joinSchemeDate", "Join Scheme Date", ReportColumnKind.Date, r => r.Member.JoinSchemeDate);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        var columns = Columns;
        foreach (var column in columns)
        {
            result.Columns.Add(new ReportColumnDefinition(column.Key, column.Header, column.Kind));
        }

        var sponsors = (await Sponsors.GetListAsync()).ToDictionary(s => s.No, s => s.Name, StringComparer.Ordinal);
        var entries = await GetEntriesAsync(request);
        var members = (await GetMembersAsync(request)).Where(m => Includes(m, request)).ToList();
        var totals = columns.Where(c => c.Totalled).ToDictionary(c => c.Key, _ => 0m);

        foreach (var member in members)
        {
            var data = new MemberListingRow(member, sponsors.GetValueOrDefault(member.SponsorNo, member.SponsorNo), new MemberBalances(entries[member.No]));
            var row = result.AddRow();

            foreach (var column in columns)
            {
                var value = column.Value(data);
                row.Values[column.Key] = value;
                if (column.Totalled)
                {
                    totals[column.Key] += Convert.ToDecimal(value ?? 0m, CultureInfo.InvariantCulture);
                }
            }
        }

        var total = BoldRow(result, (columns[0].Key, string.Empty), (CountColumn, $"Total: {members.Count} member(s)"));
        foreach (var (key, value) in totals)
        {
            total.Values[key] = value;
        }

        return result;
    }
}

/// <summary>Member Listing: every member of the scheme with the salary and what the member and the employer have contributed.</summary>
public class MemberListingReport : MemberListingReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520116, "MemberListing", "Member Listing", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    protected override IReadOnlyList<MemberListingColumn> Columns { get; } =
    [
        No,
        Name,
        Sponsor,
        BirthDate,
        Gender,
        NationalId,
        PayrollNo,
        new("salary", "Salary", ReportColumnKind.Number, r => r.Member.CurrentSalary, Totalled: true),
        new("employee", "EE", ReportColumnKind.Number, r => r.Balances.Employee, Totalled: true),
        new("employer", "ER", ReportColumnKind.Number, r => r.Balances.Employer, Totalled: true),
    ];

    protected override bool Includes(PensionMember member, StandardReportRequest request) => true;
}

/// <summary>Active Members: the members still being contributed for.</summary>
public class ActiveMembersReport : MemberListingReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520118, "ActiveMembers", "Active Members", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    protected override IReadOnlyList<MemberListingColumn> Columns { get; } =
    [
        No,
        new("payrollNo", "PF No", ReportColumnKind.Text, r => r.Member.PayrollNo),
        Name,
        BirthDate,
        JoinSchemeDate,
        Gender,
        Sponsor,
    ];

    protected override bool Includes(PensionMember member, StandardReportRequest request) => member.Status == MemberStatus.Active;
}

/// <summary>Expected Retirees: the members who reach normal retirement in the period.</summary>
public class ExpectedRetireesReport : MemberListingReportBase
{
    public override StandardReportDefinition Definition { get; } =
        Define(51520108, "ExpectedRetirees", "Expected Retirees", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    protected override IReadOnlyList<MemberListingColumn> Columns { get; } =
    [
        No,
        Name,
        Gender,
        BirthDate,
        NationalId,
        Sponsor,
        JoinSchemeDate,
        new("maritalStatus", "Marital Status", ReportColumnKind.Text, r => ErpEntityFieldNames.Humanize(r.Member.MaritalStatus)),
        PayrollNo,
        new("retirementDate", "Date of Normal Retirement", ReportColumnKind.Date, r => r.Member.ExpectedRetirementDate),
    ];

    protected override bool Includes(PensionMember member, StandardReportRequest request) =>
        member.ExpectedRetirementDate >= request.From && member.ExpectedRetirementDate <= request.To && member.Status is MemberStatus.Active or MemberStatus.Dormant or MemberStatus.Deferred;
}

/// <summary>Whose money a contribution type is, as the pension reports split it.</summary>
public enum PensionShare
{
    Employee,
    Employer,
    Voluntary,
}

internal static class PensionShares
{
    public static PensionShare Of(PensionContributionType type) =>
        type switch
        {
            PensionContributionType.EmployeeAdditional or PensionContributionType.EmployerAdditional => PensionShare.Voluntary,
            PensionContributionType.EmployerContribution or PensionContributionType.Pre90Employer => PensionShare.Employer,
            _ => PensionShare.Employee,
        };

    /// <summary>Interest credited, and the tax taken off it.</summary>
    public static bool IsInterest(PensionTransactionType type) => type is PensionTransactionType.Interest or PensionTransactionType.TaxOnInterest;
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
