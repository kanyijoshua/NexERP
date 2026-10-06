using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Pensions;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Beneficiaries: each member's nominees and their shares, with members whose shares do not add up
/// to 100% marked, since their death benefit cannot be approved until they do.
/// </summary>
public class BeneficiaryListingReport : PensionReportBase
{
    private readonly IRepository<PensionBeneficiary, Guid> _beneficiaries;

    public BeneficiaryListingReport(IRepository<PensionBeneficiary, Guid> beneficiaries)
    {
        _beneficiaries = beneficiaries;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(51520103, "Beneficiaries", "Beneficiaries", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "memberNo", "Member No.");
        Text(result, "name", "Name");
        Text(result, "relationship", "Relationship");
        Date(result, "dateOfBirth", "Date of Birth");
        Text(result, "nationalId", "National ID");
        Text(result, "guardian", "Guardian");
        Text(result, "status", "Status");
        Number(result, "benefitPct", "Benefit %");

        var beneficiaries = (await _beneficiaries.GetListAsync()).ToLookup(b => b.MemberNo, StringComparer.Ordinal);

        foreach (var member in await GetMembersAsync(request))
        {
            var nominees = beneficiaries[member.No].OrderBy(b => b.LineNo).ToList();
            if (nominees.Count == 0)
            {
                continue;
            }

            var total = nominees.Where(b => b.Status == BeneficiaryStatus.Active).Sum(b => b.BenefitPct);
            BoldRow(
                result,
                ("memberNo", member.No),
                ("name", total == 100m ? member.FullName : $"{member.FullName} (shares come to {total}%)"),
                ("benefitPct", total)
            );

            foreach (var beneficiary in nominees)
            {
                Row(
                    result,
                    ("name", beneficiary.Name),
                    ("relationship", ErpEntityFieldNames.Humanize(beneficiary.Relationship)),
                    ("dateOfBirth", beneficiary.DateOfBirth),
                    ("nationalId", beneficiary.NationalId),
                    ("guardian", beneficiary.IsMinorOn(request.To) ? beneficiary.GuardianName : null),
                    ("status", ErpEntityFieldNames.Humanize(beneficiary.Status)),
                    ("benefitPct", beneficiary.BenefitPct)
                ).Indentation = 1;
            }
        }

        return result;
    }
}

/// <summary>
/// Membership Movement: the status changes of the period, one line each, then how many members
/// moved into each status. The "No. Filter" is on the member number.
/// </summary>
public class MembershipMovementReport : PensionReportBase
{
    private readonly IRepository<MemberStatusEntry, Guid> _entries;

    public MembershipMovementReport(IRepository<MemberStatusEntry, Guid> entries)
    {
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(51520294, "MembershipMovement", "Membership Movement", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "date", "Effective Date");
        Text(result, "scheme", "Scheme");
        Text(result, "memberNo", "Member No.");
        Text(result, "sponsorNo", "Sponsor No.");
        Text(result, "fromStatus", "From Status");
        Text(result, "toStatus", "To Status");
        Text(result, "documentNo", "Document No.");
        Number(result, "count", "Members");

        var filter = Filter(request);
        var scheme = SchemeOf(request);
        var entries = (await _entries.GetListAsync(e => e.EffectiveDate >= request.From && e.EffectiveDate <= request.To && (scheme == null || e.SchemeCode == scheme)))
            .Where(e => filter.Matches(e.MemberNo))
            .OrderBy(e => e.EffectiveDate)
            .ThenBy(e => e.MemberNo, StringComparer.Ordinal)
            .ToList();

        foreach (var entry in entries)
        {
            Row(
                result,
                ("date", entry.EffectiveDate),
                ("scheme", entry.SchemeCode),
                ("memberNo", entry.MemberNo),
                ("sponsorNo", entry.SponsorNo),
                ("fromStatus", ErpEntityFieldNames.Humanize(entry.FromStatus)),
                ("toStatus", ErpEntityFieldNames.Humanize(entry.ToStatus)),
                ("documentNo", entry.DocumentNo)
            );
        }

        foreach (var group in entries.GroupBy(e => e.ToStatus).OrderBy(g => g.Key))
        {
            BoldRow(result, ("toStatus", ErpEntityFieldNames.Humanize(group.Key)), ("count", group.Select(e => e.MemberNo).Distinct().Count()));
        }

        return result;
    }
}

/// <summary>
/// Pensioners Master Roll: every pensioner of the scheme with the pension, its status, the arrears
/// still owed and when the next life certificate is due. The "No. Filter" is on the pensioner number.
/// </summary>
public class PensionersMasterRollReport : PensionReportBase
{
    private readonly IRepository<Pensioner, Guid> _pensioners;

    public PensionersMasterRollReport(IRepository<Pensioner, Guid> pensioners)
    {
        _pensioners = pensioners;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(51520128, "PensionersMasterRoll", "Pensioners Master Roll", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Name");
        Text(result, "scheme", "Scheme");
        Text(result, "memberNo", "Member No.");
        Date(result, "startDate", "Start Date");
        Text(result, "status", "Status");
        Date(result, "lastPaid", "Last Paid Period");
        Date(result, "certificateDue", "Life Certificate Due");
        Number(result, "monthlyPension", "Monthly Pension");
        Number(result, "arrears", "Arrears Owed");

        var filter = Filter(request);
        var scheme = SchemeOf(request);
        var pensioners = (await _pensioners.GetListAsync(p => scheme == null || p.SchemeCode == scheme))
            .Where(p => filter.Matches(p.No) && p.StartDate <= request.To)
            .OrderBy(p => p.SchemeCode, StringComparer.Ordinal)
            .ThenBy(p => p.No, StringComparer.Ordinal)
            .ToList();

        foreach (var pensioner in pensioners)
        {
            Row(
                result,
                ("no", pensioner.No),
                ("name", pensioner.Name),
                ("scheme", pensioner.SchemeCode),
                ("memberNo", pensioner.MemberNo),
                ("startDate", pensioner.StartDate),
                ("status", pensioner.Status == PensionerStatus.Suspended ? $"Suspended: {pensioner.SuspensionReason}" : pensioner.Status.ToString()),
                ("lastPaid", pensioner.LastPaidPeriod),
                ("certificateDue", pensioner.LifeCertificateDueDate),
                ("monthlyPension", pensioner.MonthlyPension),
                ("arrears", pensioner.ArrearsAmount)
            );
        }

        var active = pensioners.Where(p => p.Status == PensionerStatus.Active).ToList();
        BoldRow(
            result,
            ("no", string.Empty),
            ("name", $"Total: {active.Count} active of {pensioners.Count} pensioner(s)"),
            ("monthlyPension", active.Sum(p => p.MonthlyPension)),
            ("arrears", pensioners.Sum(p => p.ArrearsAmount))
        );
        return result;
    }
}

/// <summary>
/// Pensioner Payslips: what each pensioner was paid by the posted payrolls of the period, pension,
/// arrears, tax and net. The "No. Filter" is on the pensioner number.
/// </summary>
public class PensionerPayslipReport : PensionReportBase
{
    private readonly IRepository<PensionPayrollHeader, Guid> _payrolls;
    private readonly IRepository<PensionPayrollLine, Guid> _lines;

    public PensionerPayslipReport(IRepository<PensionPayrollHeader, Guid> payrolls, IRepository<PensionPayrollLine, Guid> lines)
    {
        _payrolls = payrolls;
        _lines = lines;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(51520079, "PensionerPayslip", "Pensioner Payslip", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "period", "Pay Period");
        Text(result, "documentNo", "Payroll No.");
        Text(result, "pensionerNo", "Pensioner No.");
        Text(result, "name", "Name");
        Number(result, "pension", "Monthly Pension");
        Number(result, "arrears", "Arrears");
        Number(result, "gross", "Gross Pension");
        Number(result, "tax", "Tax");
        Number(result, "net", "Net Pension");

        var filter = Filter(request);
        var scheme = SchemeOf(request);
        var payrolls = (await _payrolls.GetListAsync(h =>
                h.Status == PensionDocumentStatus.Posted && h.PayPeriod >= request.From && h.PayPeriod <= request.To && (scheme == null || h.SchemeCode == scheme)
            ))
            .ToDictionary(h => h.No, StringComparer.Ordinal);
        var numbers = payrolls.Keys.ToList();

        var lines = (await _lines.GetListAsync(l => numbers.Contains(l.DocumentNo)))
            .Where(l => filter.Matches(l.PensionerNo))
            .OrderBy(l => l.PensionerNo, StringComparer.Ordinal)
            .ThenBy(l => payrolls[l.DocumentNo].PayPeriod)
            .ToList();

        foreach (var pensioner in lines.GroupBy(l => l.PensionerNo))
        {
            var slips = pensioner.ToList();
            BoldRow(result, ("pensionerNo", pensioner.Key), ("name", slips[0].PensionerName));

            foreach (var line in slips)
            {
                Row(
                    result,
                    ("period", payrolls[line.DocumentNo].PayPeriod),
                    ("documentNo", line.DocumentNo),
                    ("pension", line.GrossPension - line.ArrearsAmount),
                    ("arrears", line.ArrearsAmount),
                    ("gross", line.GrossPension),
                    ("tax", line.TaxAmount),
                    ("net", line.NetPension)
                ).Indentation = 1;
            }
        }

        BoldRow(
            result,
            ("name", "Total"),
            ("gross", lines.Sum(l => l.GrossPension)),
            ("arrears", lines.Sum(l => l.ArrearsAmount)),
            ("tax", lines.Sum(l => l.TaxAmount)),
            ("net", lines.Sum(l => l.NetPension))
        );
        return result;
    }
}
