using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Payroll;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Employee Report: one line per employee with what payroll and HR keep on file: the bank details
/// salaries are paid to, the social security number, the job, the cost centre and the basic pay.
/// </summary>
public class EmployeeRegisterReport : EmployeeReportBase
{
    private readonly IRepository<EmployeePayItem, Guid> _payItems;
    private readonly IRepository<PayrollEarning, Guid> _earnings;

    public EmployeeRegisterReport(IRepository<EmployeePayItem, Guid> payItems, IRepository<PayrollEarning, Guid> earnings)
    {
        _payItems = payItems;
        _earnings = earnings;
    }

    public override StandardReportDefinition Definition { get; } = Define(51519142, "EmployeeReport", "Employee Report", StandardReportParameters.AsOfDate | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "Employee No");
        Text(result, "name", "Name");
        Date(result, "birthDate", "Birth Date");
        Text(result, "bankAccountNo", "Bank Account");
        Text(result, "bankBranchNo", "Bank Code");
        Text(result, "socialSecurityNo", "Social Security No.");
        Text(result, "gender", "Gender");
        Text(result, "jobTitle", "Job Title");
        Date(result, "employmentDate", "Appointment Date");
        Text(result, "costCentre", "Cost Centre");
        Number(result, "basicPay", "Basic Pay");

        // Basic pay is what the employee's pay items hold for the earnings marked as basic pay.
        var basicCodes = (await _earnings.GetListAsync(e => e.BasicPay)).Select(e => e.Code).ToHashSet(StringComparer.Ordinal);
        var asOf = request.To;
        var basicPay = (await _payItems.GetListAsync(i => i.ItemType == PayItemType.Earning))
            .Where(i => basicCodes.Contains(i.Code) && (!i.StartDate.HasValue || i.StartDate <= asOf) && (!i.EndDate.HasValue || i.EndDate >= asOf))
            .GroupBy(i => i.EmployeeNo, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount), StringComparer.Ordinal);

        var employees = await GetEmployeesAsync(request);
        foreach (var employee in employees)
        {
            Row(
                result,
                ("no", employee.No),
                ("name", NameOf(employee)),
                ("birthDate", employee.BirthDate),
                ("bankAccountNo", employee.BankAccountNo),
                ("bankBranchNo", employee.BankBranchNo),
                ("socialSecurityNo", employee.SocialSecurityNo),
                ("gender", employee.Gender.ToString()),
                ("jobTitle", employee.JobTitle),
                ("employmentDate", employee.EmploymentDate),
                ("costCentre", employee.GlobalDimension1Code),
                ("basicPay", basicPay.GetValueOrDefault(employee.No))
            );
        }

        BoldRow(result, ("no", string.Empty), ("name", $"Total: {employees.Count} employee(s)"), ("basicPay", basicPay.Where(kv => employees.Any(e => e.No == kv.Key)).Sum(kv => kv.Value)));
        return result;
    }
}

/// <summary>
/// Fixed Assets Register: the assets by subclass, each with its year and date of purchase, its
/// cost at the start of the period, additions and disposals, cost at the end, accumulated
/// depreciation before and after the period's charge, and the net book value carried forward,
/// with a subtotal for each subclass.
/// </summary>
public class FixedAssetsRegisterReport : StandardReportBase
{
    private readonly IRepository<FixedAsset, Guid> _assets;
    private readonly IRepository<FALedgerEntry, Guid> _entries;

    public FixedAssetsRegisterReport(IRepository<FixedAsset, Guid> assets, IRepository<FALedgerEntry, Guid> entries)
    {
        _assets = assets;
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            51519048,
            "FixedAssetsRegister",
            "Fixed Assets Register",
            StandardReportAreas.FixedAssets,
            StandardReportParameters.Period | StandardReportParameters.NoFilter | StandardReportParameters.DepreciationBook
        );

    private static readonly string[] Figures =
        ["costStart", "additions", "disposals", "costEnd", "depreciationStart", "charge", "depreciationEnd", "netBookValue"];

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "asset", "Assets Listings");
        Text(result, "yearOfPurchase", "Year Of Purchase");
        Number(result, "costStart", $"Cost as at {request.From:d}");
        Number(result, "additions", "Additions in the year");
        Date(result, "dateOfPurchase", "Date of Purchase");
        Number(result, "disposals", "Disposals in the year");
        Number(result, "costEnd", $"Cost as at {request.To:d}");
        Number(result, "depreciationStart", $"Acc Depr as at {request.From:d}");
        Number(result, "charge", "Charge");
        Number(result, "depreciationEnd", $"Acc Depr as at {request.To:d}");
        Number(result, "netBookValue", "NBV C/F");

        var filter = Filter(request);
        var bookCode = request.DepreciationBookCode?.Trim().ToUpperInvariant();
        var entries = (await _entries.GetListAsync(e => e.FAPostingDate <= request.To && !e.Reversed))
            .Where(e => bookCode.IsNullOrEmpty() || e.DepreciationBookCode == bookCode)
            .ToLookup(e => e.FANo, StringComparer.Ordinal);

        var assets = (await _assets.GetListAsync()).Where(a => filter.Matches(a.No) && entries[a.No].Any()).ToList();
        var grand = new decimal[Figures.Length];

        foreach (var subclass in assets.GroupBy(a => a.FASubclassCode ?? string.Empty).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            BoldRow(result, ("asset", subclass.Key.Length == 0 ? "(No subclass)" : subclass.Key));
            var subtotal = new decimal[Figures.Length];

            foreach (var asset in subclass.OrderBy(a => a.No, StringComparer.Ordinal))
            {
                var own = entries[asset.No].ToList();
                bool InPeriod(FALedgerEntry e) => e.FAPostingDate >= request.From;
                var acquisitions = own.Where(e => e.FAPostingType == FALedgerEntryFAPostingType.AcquisitionCost).ToList();
                var depreciation = own.Where(e => e.FAPostingType == FALedgerEntryFAPostingType.Depreciation).ToList();
                var purchased = acquisitions.Where(e => e.Amount > 0).Min(e => e.FAPostingDate);

                var costStart = acquisitions.Where(e => !InPeriod(e)).Sum(e => e.Amount);
                var additions = acquisitions.Where(e => InPeriod(e) && e.Amount > 0).Sum(e => e.Amount);
                var disposals = acquisitions.Where(e => InPeriod(e) && e.Amount < 0).Sum(e => e.Amount);
                var depreciationStart = depreciation.Where(e => !InPeriod(e)).Sum(e => e.Amount);
                var charge = depreciation.Where(InPeriod).Sum(e => e.Amount);

                var values = new[]
                {
                    costStart,
                    additions,
                    disposals,
                    costStart + additions + disposals,
                    depreciationStart,
                    charge,
                    depreciationStart + charge,
                    own.Where(e => e.PartOfBookValue).Sum(e => e.Amount),
                };

                var row = Row(result, ("asset", asset.Description), ("yearOfPurchase", purchased?.Year.ToString()), ("dateOfPurchase", purchased));
                row.Indentation = 1;
                Put(row, values, subtotal);
            }

            var sub = BoldRow(result, ("asset", "Sub total"));
            Put(sub, subtotal, grand);
        }

        Put(BoldRow(result, ("asset", "Total")), grand, null);
        return result;
    }

    private static void Put(ReportRow row, decimal[] values, decimal[] addTo)
    {
        for (var i = 0; i < Figures.Length; i++)
        {
            row.Values[Figures[i]] = values[i];
            if (addTo != null)
            {
                addTo[i] += values[i];
            }
        }
    }
}

/// <summary>
/// Cash Book: each bank account's opening balance, every receipt and payment of the period with
/// the payee it was paid to, the running balance, and the closing balance.
/// </summary>
public class CashBookReport : StandardReportBase
{
    private readonly IRepository<BankAccount, Guid> _bankAccounts;
    private readonly IRepository<BankAccountLedgerEntry, Guid> _entries;
    private readonly IRepository<PaymentVoucherHeader, Guid> _vouchers;

    public CashBookReport(IRepository<BankAccount, Guid> bankAccounts, IRepository<BankAccountLedgerEntry, Guid> entries, IRepository<PaymentVoucherHeader, Guid> vouchers)
    {
        _bankAccounts = bankAccounts;
        _entries = entries;
        _vouchers = vouchers;
    }

    public override StandardReportDefinition Definition { get; } =
        new(51521657, "CashBook", "Cash Book", StandardReportAreas.CashManagement, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "postingDate", "Posting Date");
        Text(result, "documentNo", "Document No.");
        Text(result, "payee", "Payee");
        Text(result, "description", "Description");
        Number(result, "debit", "Debit Amount");
        Number(result, "credit", "Credit Amount");
        Number(result, "balance", "Balance");

        var filter = Filter(request);
        var entries = (await _entries.GetListAsync(e => e.PostingDate <= request.To)).ToLookup(e => e.BankAccountNo, StringComparer.Ordinal);
        var documents = entries.SelectMany(g => g).Where(e => e.PostingDate >= request.From).Select(e => e.DocumentNo).Distinct().ToList();
        var payees = (await _vouchers.GetListAsync(v => documents.Contains(v.No))).ToDictionary(v => v.No, v => v.Payee, StringComparer.Ordinal);

        foreach (var account in (await _bankAccounts.GetListAsync()).Where(a => filter.Matches(a.No)).OrderBy(a => a.No, StringComparer.Ordinal))
        {
            var balance = entries[account.No].Where(e => e.PostingDate < request.From).Sum(e => e.Amount);
            var inPeriod = entries[account.No].Where(e => e.PostingDate >= request.From).OrderBy(e => e.PostingDate).ThenBy(e => e.EntryNo).ToList();
            if (balance == 0m && inPeriod.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("documentNo", account.No), ("description", account.Name));
            Row(result, ("postingDate", request.From), ("description", "Opening balance"), ("balance", balance)).Indentation = 1;

            foreach (var entry in inPeriod)
            {
                balance += entry.Amount;
                var row = Row(
                    result,
                    ("postingDate", entry.PostingDate),
                    ("documentNo", entry.DocumentNo),
                    ("payee", entry.DocumentNo == null ? null : payees.GetValueOrDefault(entry.DocumentNo)),
                    ("description", entry.Description),
                    ("debit", Debit(entry.Amount)),
                    ("credit", Credit(entry.Amount)),
                    ("balance", balance)
                );
                row.Indentation = 1;
                row.Italic = entry.Reversed;
            }

            BoldRow(
                result,
                ("postingDate", request.To),
                ("description", "Closing balance"),
                ("debit", inPeriod.Sum(e => Debit(e.Amount))),
                ("credit", inPeriod.Sum(e => Credit(e.Amount))),
                ("balance", balance)
            ).Indentation = 1;
        }

        return result;
    }
}

/// <summary>
/// Trial Balance by account: the whole chart of accounts in its indentation, headings included,
/// with each account's opening balance, the period's debits and credits, the net change and the
/// balance at the end. Total accounts add up the posting accounts their totaling names.
/// </summary>
public class TrialBalanceByAccountReport : StandardReportBase
{
    private readonly IRepository<GLAccount, Guid> _accounts;
    private readonly IRepository<GLEntry, Guid> _entries;

    public TrialBalanceByAccountReport(IRepository<GLAccount, Guid> accounts, IRepository<GLEntry, Guid> entries)
    {
        _accounts = accounts;
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(51519043, "TrialBalanceByAccount", "Trial Balance by Account", StandardReportAreas.Finance, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    private static readonly string[] Figures = ["openingBalance", "debit", "credit", "netChange", "balance"];

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Name");
        Number(result, "openingBalance", "Opening Balance");
        Number(result, "debit", "Debit");
        Number(result, "credit", "Credit");
        Number(result, "netChange", "Net Change");
        Number(result, "balance", "Balance");

        var filter = Filter(request);
        var accounts = (await _accounts.GetListAsync()).OrderBy(a => a.No, StringComparer.Ordinal).ToList();
        var entries = (await _entries.GetListAsync(e => e.PostingDate <= request.To)).ToLookup(e => e.GLAccountNo, StringComparer.Ordinal);

        decimal[] FiguresOf(IEnumerable<GLEntry> own)
        {
            var list = own.ToList();
            var opening = list.Where(e => e.PostingDate < request.From).Sum(e => e.Amount);
            var inPeriod = list.Where(e => e.PostingDate >= request.From).ToList();
            var debit = inPeriod.Sum(e => e.DebitAmount);
            var credit = inPeriod.Sum(e => e.CreditAmount);
            return [opening, debit, credit, debit - credit, opening + debit - credit];
        }

        var posting = accounts.Where(a => a.AccountType == GLAccountType.Posting).ToList();
        var totals = new decimal[Figures.Length];

        foreach (var account in accounts.Where(a => filter.Matches(a.No)))
        {
            var row = Row(result, ("no", account.No), ("name", account.Name));
            row.Indentation = account.Indentation;

            switch (account.AccountType)
            {
                case GLAccountType.Posting:
                    var values = FiguresOf(entries[account.No]);
                    for (var i = 0; i < Figures.Length; i++)
                    {
                        row.Values[Figures[i]] = values[i];
                        totals[i] += values[i];
                    }

                    break;

                case GLAccountType.Total or GLAccountType.EndTotal when !account.Totaling.IsNullOrWhiteSpace():
                    var totaling = AccountTotaling.Parse(account.Totaling);
                    var summed = FiguresOf(posting.Where(p => totaling.Matches(p.No)).SelectMany(p => entries[p.No]));
                    for (var i = 0; i < Figures.Length; i++)
                    {
                        row.Values[Figures[i]] = summed[i];
                    }

                    row.Bold = true;
                    break;

                default:
                    row.Bold = true;
                    break;
            }
        }

        BoldRow(result, ("no", string.Empty), ("name", "Totals"), ("debit", totals[1]), ("credit", totals[2]));
        return result;
    }
}
