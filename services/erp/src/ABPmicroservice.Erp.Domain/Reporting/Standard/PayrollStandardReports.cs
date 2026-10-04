using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Payroll;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// What the payroll reports share: the calculated or posted runs whose month falls in the period,
/// and a "No. Filter" on the employee number.
/// </summary>
public abstract class PayrollReportBase : StandardReportBase
{
    protected IRepository<PayrollRun, Guid> Runs => LazyServiceProvider.LazyGetRequiredService<IRepository<PayrollRun, Guid>>();

    protected IRepository<Payslip, Guid> Payslips => LazyServiceProvider.LazyGetRequiredService<IRepository<Payslip, Guid>>();

    protected IRepository<PayslipLine, Guid> Lines => LazyServiceProvider.LazyGetRequiredService<IRepository<PayslipLine, Guid>>();

    protected static StandardReportDefinition Define(int id, string code, string name) =>
        new(id, code, name, StandardReportAreas.Payroll, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    /// <summary>The runs of the period, oldest month first; a run still open has no payslips worth reporting.</summary>
    protected async Task<List<PayrollRun>> GetRunsAsync(StandardReportRequest request)
    {
        var (from, to) = (request.From, request.To);
        var fromMonth = from == DateTime.MinValue ? from : new DateTime(from.Year, from.Month, 1);

        return (await Runs.GetListAsync(r => r.Status != PayrollRunStatus.Open && r.PayPeriod >= fromMonth && r.PayPeriod <= to))
            .OrderBy(r => r.PayPeriod)
            .ThenBy(r => r.No, StringComparer.Ordinal)
            .ToList();
    }

    protected async Task<List<Payslip>> GetPayslipsAsync(StandardReportRequest request, IEnumerable<PayrollRun> runs)
    {
        var filter = Filter(request);
        var numbers = runs.Select(r => r.No).ToList();

        return (await Payslips.GetListAsync(p => numbers.Contains(p.PayrollRunNo)))
            .Where(p => filter.Matches(p.EmployeeNo))
            .OrderBy(p => p.PayPeriod)
            .ThenBy(p => p.EmployeeNo, StringComparer.Ordinal)
            .ToList();
    }

    protected async Task<List<PayslipLine>> GetLinesAsync(IEnumerable<Payslip> payslips)
    {
        var runs = payslips.Select(p => p.PayrollRunNo).Distinct().ToList();
        var employees = payslips.Select(p => (p.PayrollRunNo, p.EmployeeNo)).ToHashSet();

        return (await Lines.GetListAsync(l => runs.Contains(l.PayrollRunNo))).Where(l => employees.Contains((l.PayrollRunNo, l.EmployeeNo))).ToList();
    }
}

/// <summary>Payroll Summary: what each run paid, deducted and cost, by earning and deduction.</summary>
public class PayrollSummaryReport : PayrollReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(51519001, "PayrollSummary", "Payroll Summary");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "code", "Code");
        Text(result, "description", "Description");
        Number(result, "employees", "Employees");
        Number(result, "amount", "Amount");

        foreach (var run in await GetRunsAsync(request))
        {
            var payslips = await GetPayslipsAsync(request, [run]);
            if (payslips.Count == 0)
            {
                continue;
            }

            var lines = await GetLinesAsync(payslips);
            BoldRow(result, ("code", run.No), ("description", $"{run.PayPeriod:MMMM yyyy} ({ErpEntityFieldNames.Humanize(run.Status)})"));

            foreach (var (type, caption) in new[] { (PayslipLineType.Earning, "Earnings"), (PayslipLineType.Deduction, "Deductions"), (PayslipLineType.EmployerContribution, "Employer contributions") })
            {
                var ofType = lines.Where(l => l.LineType == type).ToList();
                if (ofType.Count == 0)
                {
                    continue;
                }

                Row(result, ("description", caption)).Indentation = 1;
                foreach (var group in ofType.GroupBy(l => l.Code, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal))
                {
                    Row(
                        result,
                        ("code", group.Key),
                        ("description", group.First().Description),
                        ("employees", group.Select(l => l.EmployeeNo).Distinct().Count()),
                        ("amount", group.Sum(l => l.Amount))
                    ).Indentation = 2;
                }

                BoldRow(result, ("description", $"Total {caption.ToLowerInvariant()}"), ("amount", ofType.Sum(l => l.Amount))).Indentation = 1;
            }

            BoldRow(result, ("description", "Net pay"), ("employees", payslips.Count), ("amount", payslips.Sum(p => p.NetPay))).Indentation = 1;
        }

        return result;
    }
}

/// <summary>Payslips: each employee's earnings, deductions and net pay for each month.</summary>
public class PayslipReport : PayrollReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(51519002, "Payslips", "Payslips");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "code", "Code");
        Text(result, "description", "Description");
        Number(result, "earning", "Earnings");
        Number(result, "deduction", "Deductions");

        var payslips = await GetPayslipsAsync(request, await GetRunsAsync(request));
        var lines = (await GetLinesAsync(payslips)).ToLookup(l => (l.PayrollRunNo, l.EmployeeNo));

        foreach (var payslip in payslips)
        {
            BoldRow(
                result,
                ("code", payslip.EmployeeNo),
                ("description", $"{payslip.EmployeeName}, {payslip.PayPeriod:MMMM yyyy}{(payslip.JobTitle.IsNullOrWhiteSpace() ? string.Empty : ", " + payslip.JobTitle)}")
            );

            foreach (var line in lines[(payslip.PayrollRunNo, payslip.EmployeeNo)].Where(l => l.LineType != PayslipLineType.EmployerContribution).OrderBy(l => l.LineType).ThenBy(l => l.Code, StringComparer.Ordinal))
            {
                Row(
                    result,
                    ("code", line.Code),
                    ("description", line.Description),
                    ("earning", line.LineType == PayslipLineType.Earning ? line.Amount : (decimal?)null),
                    ("deduction", line.LineType == PayslipLineType.Deduction ? line.Amount : (decimal?)null)
                ).Indentation = 1;
            }

            Row(result, ("description", "Taxable pay"), ("earning", payslip.TaxablePay)).Indentation = 1;
            BoldRow(result, ("description", "Totals"), ("earning", payslip.GrossPay), ("deduction", payslip.TotalDeductions)).Indentation = 1;
            BoldRow(result, ("description", $"Net pay{(payslip.BankAccountNo.IsNullOrWhiteSpace() ? string.Empty : " to account " + payslip.BankAccountNo)}"), ("earning", payslip.NetPay)).Indentation = 1;
        }

        return result;
    }
}

/// <summary>Payroll Register: one row per employee and month with gross pay, tax, other deductions, net pay and the employer's cost.</summary>
public class PayrollRegisterReport : PayrollReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(51519003, "PayrollRegister", "Payroll Register");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "period", "Period");
        Text(result, "no", "Employee No.");
        Text(result, "name", "Name");
        Number(result, "basic", "Basic Pay");
        Number(result, "gross", "Gross Pay");
        Number(result, "taxable", "Taxable Pay");
        Number(result, "tax", "Income Tax");
        Number(result, "other", "Other Deductions");
        Number(result, "net", "Net Pay");
        Number(result, "employer", "Employer Contributions");

        var payslips = await GetPayslipsAsync(request, await GetRunsAsync(request));
        foreach (var payslip in payslips)
        {
            Row(
                result,
                ("period", payslip.PayPeriod.ToString("MMM yyyy")),
                ("no", payslip.EmployeeNo),
                ("name", payslip.EmployeeName),
                ("basic", payslip.BasicPay),
                ("gross", payslip.GrossPay),
                ("taxable", payslip.TaxablePay),
                ("tax", payslip.IncomeTax),
                ("other", payslip.TotalDeductions - payslip.IncomeTax),
                ("net", payslip.NetPay),
                ("employer", payslip.EmployerContributions)
            );
        }

        BoldRow(
            result,
            ("name", "Total"),
            ("basic", payslips.Sum(p => p.BasicPay)),
            ("gross", payslips.Sum(p => p.GrossPay)),
            ("taxable", payslips.Sum(p => p.TaxablePay)),
            ("tax", payslips.Sum(p => p.IncomeTax)),
            ("other", payslips.Sum(p => p.TotalDeductions - p.IncomeTax)),
            ("net", payslips.Sum(p => p.NetPay)),
            ("employer", payslips.Sum(p => p.EmployerContributions))
        );

        return result;
    }
}

/// <summary>
/// Deduction Schedule: for each deduction, what was taken from each employee and what the employer
/// added, the figures a tax or pension return is filed from.
/// </summary>
public class PayrollDeductionScheduleReport : PayrollReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(51519004, "PayrollDeductionSchedule", "Payroll Deduction Schedule");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "Employee No.");
        Text(result, "name", "Name");
        Text(result, "period", "Period");
        Number(result, "employee", "Employee");
        Number(result, "employer", "Employer");
        Number(result, "total", "Total");

        var payslips = await GetPayslipsAsync(request, await GetRunsAsync(request));
        var names = payslips.GroupBy(p => p.EmployeeNo, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().EmployeeName, StringComparer.Ordinal);
        var periods = payslips.Select(p => p.PayrollRunNo).Distinct().ToDictionary(n => n, n => payslips.First(p => p.PayrollRunNo == n).PayPeriod, StringComparer.Ordinal);
        var lines = (await GetLinesAsync(payslips)).Where(l => l.LineType != PayslipLineType.Earning).ToList();

        foreach (var deduction in lines.GroupBy(l => l.Code, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            BoldRow(result, ("no", deduction.Key), ("name", deduction.First().Description.Replace(" (employer)", string.Empty)));

            foreach (var row in deduction.GroupBy(l => (l.PayrollRunNo, l.EmployeeNo)).OrderBy(g => periods[g.Key.PayrollRunNo]).ThenBy(g => g.Key.EmployeeNo, StringComparer.Ordinal))
            {
                var employee = row.Where(l => l.LineType == PayslipLineType.Deduction).Sum(l => l.Amount);
                var employer = row.Where(l => l.LineType == PayslipLineType.EmployerContribution).Sum(l => l.Amount);

                Row(
                    result,
                    ("no", row.Key.EmployeeNo),
                    ("name", names[row.Key.EmployeeNo]),
                    ("period", periods[row.Key.PayrollRunNo].ToString("MMM yyyy")),
                    ("employee", employee),
                    ("employer", employer),
                    ("total", employee + employer)
                ).Indentation = 1;
            }

            var employeeTotal = deduction.Where(l => l.LineType == PayslipLineType.Deduction).Sum(l => l.Amount);
            var employerTotal = deduction.Where(l => l.LineType == PayslipLineType.EmployerContribution).Sum(l => l.Amount);
            BoldRow(result, ("name", $"Total {deduction.Key}"), ("employee", employeeTotal), ("employer", employerTotal), ("total", employeeTotal + employerTotal)).Indentation = 1;
        }

        return result;
    }
}

/// <summary>Net Pay Schedule: what is to be paid into each employee's bank account, the list a bank transfer is made from.</summary>
public class PayrollNetPayScheduleReport : PayrollReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(51519005, "PayrollNetPaySchedule", "Net Pay Schedule");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "Employee No.");
        Text(result, "name", "Name");
        Text(result, "bankAccount", "Bank Account No.");
        Number(result, "net", "Net Pay");

        var runs = await GetRunsAsync(request);
        foreach (var run in runs)
        {
            var payslips = (await GetPayslipsAsync(request, [run])).Where(p => p.NetPay != 0m).ToList();
            if (payslips.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("no", run.No), ("name", $"{run.PayPeriod:MMMM yyyy}"), ("bankAccount", run.PaymentVoucherNo == null ? null : $"Voucher {run.PaymentVoucherNo}"));

            foreach (var payslip in payslips)
            {
                Row(result, ("no", payslip.EmployeeNo), ("name", payslip.EmployeeName), ("bankAccount", payslip.BankAccountNo), ("net", payslip.NetPay)).Indentation = 1;
            }

            BoldRow(result, ("name", $"Total ({payslips.Count} employees)"), ("net", payslips.Sum(p => p.NetPay))).Indentation = 1;
        }

        return result;
    }
}
