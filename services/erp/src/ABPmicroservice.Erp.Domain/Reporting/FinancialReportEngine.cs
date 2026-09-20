using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>Parameters shared by the ledger-based financial reports.</summary>
public class FinancialReportRequest
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    /// <summary>Account filter in Totaling syntax ("1000..1999|2100"). Blank means every account.</summary>
    public string AccountFilter { get; set; }

    /// <summary>Leaves out accounts with no movement and no balance.</summary>
    public bool ExcludeZeroBalances { get; set; } = true;
}

/// <summary>
/// The standard financial statements, calculated straight from the G/L entries.
/// <para>
/// Business Central builds these from account schedules; the same is possible here through
/// <see cref="AccountScheduleEngine"/>. These fixed reports use the account categories on the
/// chart of accounts instead, so a new company gets a balance sheet and an income statement
/// before anyone has defined a schedule.
/// </para>
/// </summary>
public class FinancialReportEngine : DomainService
{
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;

    public FinancialReportEngine(
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<GLEntry, Guid> glEntryRepository
    )
    {
        _glAccountRepository = glAccountRepository;
        _glEntryRepository = glEntryRepository;
    }

    /// <summary>
    /// Trial balance: opening balance, the period's debits and credits, and the closing balance,
    /// per account. Mirrors BC report 6 "Trial Balance".
    /// </summary>
    public async Task<ReportResult> GenerateTrialBalanceAsync(FinancialReportRequest request)
    {
        EnsurePeriodIsForwards(request);

        var filter = ParseFilter(request.AccountFilter);
        var accounts = (await _glAccountRepository.GetListAsync())
            .Where(a => a.AccountType == GLAccountType.Posting && filter.Matches(a.No))
            .OrderBy(a => a.No, StringComparer.Ordinal)
            .ToList();

        var entries = await _glEntryRepository.GetListAsync(e => e.PostingDate <= request.ToDate);
        var byAccount = entries.ToLookup(e => e.GLAccountNo);

        var result = NewResult("Trial Balance", request.FromDate, request.ToDate);
        result.Columns.Add(new ReportColumnDefinition("accountNo", "Account", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("name", "Name", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("openingBalance", "Opening Balance"));
        result.Columns.Add(new ReportColumnDefinition("debit", "Debit"));
        result.Columns.Add(new ReportColumnDefinition("credit", "Credit"));
        result.Columns.Add(new ReportColumnDefinition("netChange", "Net Change"));
        result.Columns.Add(new ReportColumnDefinition("closingBalance", "Closing Balance"));

        var totals = new decimal[5];

        foreach (var account in accounts)
        {
            var accountEntries = byAccount[account.No].ToList();
            var opening = accountEntries.Where(e => e.PostingDate < request.FromDate).Sum(e => e.Amount);
            var inPeriod = accountEntries
                .Where(e => e.PostingDate >= request.FromDate && e.PostingDate <= request.ToDate)
                .ToList();

            var debit = inPeriod.Sum(e => e.DebitAmount);
            var credit = inPeriod.Sum(e => e.CreditAmount);
            var netChange = debit - credit;
            var closing = opening + netChange;

            if (request.ExcludeZeroBalances && opening == 0m && netChange == 0m && closing == 0m)
            {
                continue;
            }

            var row = result.AddRow();
            row.Values["accountNo"] = account.No;
            row.Values["name"] = account.Name;
            row.Values["openingBalance"] = opening;
            row.Values["debit"] = debit;
            row.Values["credit"] = credit;
            row.Values["netChange"] = netChange;
            row.Values["closingBalance"] = closing;
            row.DrillDownFilter = account.No;

            totals[0] += opening;
            totals[1] += debit;
            totals[2] += credit;
            totals[3] += netChange;
            totals[4] += closing;
        }

        // The totals row is the check: a set of balanced books nets to zero in every column.
        var totalRow = result.AddRow();
        totalRow.Bold = true;
        totalRow.Values["accountNo"] = string.Empty;
        totalRow.Values["name"] = "Total";
        totalRow.Values["openingBalance"] = totals[0];
        totalRow.Values["debit"] = totals[1];
        totalRow.Values["credit"] = totals[2];
        totalRow.Values["netChange"] = totals[3];
        totalRow.Values["closingBalance"] = totals[4];

        return result;
    }

    /// <summary>
    /// Income statement over the period, grouped by account category and shown the way people
    /// read it: income positive, costs positive, profit at the bottom.
    /// </summary>
    public async Task<ReportResult> GenerateIncomeStatementAsync(FinancialReportRequest request)
    {
        EnsurePeriodIsForwards(request);

        var accounts = (await _glAccountRepository.GetListAsync())
            .Where(a => a.AccountType == GLAccountType.Posting && a.IncomeBalance == IncomeBalanceType.IncomeStatement)
            .ToList();

        var entries = await _glEntryRepository.GetListAsync(e =>
            e.PostingDate >= request.FromDate && e.PostingDate <= request.ToDate
        );

        var result = NewResult("Income Statement", request.FromDate, request.ToDate);
        result.Columns.Add(new ReportColumnDefinition("accountNo", "Account", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("name", "Name", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("amount", "Amount"));

        var categories = new[]
        {
            (Category: GLAccountCategory.Income, Caption: "Income"),
            (Category: GLAccountCategory.CostOfGoodsSold, Caption: "Cost of Goods Sold"),
            (Category: GLAccountCategory.Expense, Caption: "Expense"),
        };

        var netIncome = 0m;

        foreach (var (category, caption) in categories)
        {
            // Income carries a credit balance, so it is flipped to read as a positive figure.
            var flip = category == GLAccountCategory.Income;
            var subtotal = AppendCategory(result, accounts, entries, category, caption, flip, request.ExcludeZeroBalances);

            netIncome += flip ? subtotal : -subtotal;
        }

        var net = result.AddRow();
        net.Bold = true;
        net.Values["accountNo"] = string.Empty;
        net.Values["name"] = "Net Income";
        net.Values["amount"] = netIncome;

        return result;
    }

    /// <summary>
    /// Balance sheet as at the end of the period, including the result of the year so far, which
    /// is what makes the two sides agree before a year-end close has run.
    /// </summary>
    public async Task<ReportResult> GenerateBalanceSheetAsync(FinancialReportRequest request)
    {
        var accounts = (await _glAccountRepository.GetListAsync())
            .Where(a => a.AccountType == GLAccountType.Posting)
            .ToList();

        var entries = await _glEntryRepository.GetListAsync(e => e.PostingDate <= request.ToDate);

        var result = NewResult("Balance Sheet", request.FromDate, request.ToDate);
        result.Columns.Add(new ReportColumnDefinition("accountNo", "Account", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("name", "Name", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("amount", "Amount"));

        var balanceSheetAccounts = accounts.Where(a => a.IncomeBalance == IncomeBalanceType.BalanceSheet).ToList();

        var assets = AppendCategory(
            result,
            balanceSheetAccounts,
            entries,
            GLAccountCategory.Assets,
            "Assets",
            flipSign: false,
            request.ExcludeZeroBalances
        );

        var liabilities = AppendCategory(
            result,
            balanceSheetAccounts,
            entries,
            GLAccountCategory.Liabilities,
            "Liabilities",
            flipSign: true,
            request.ExcludeZeroBalances
        );

        var equity = AppendCategory(
            result,
            balanceSheetAccounts,
            entries,
            GLAccountCategory.Equity,
            "Equity",
            flipSign: true,
            request.ExcludeZeroBalances
        );

        // Income and expense accounts are not closed to equity until the year end, so the result
        // of the year to date is shown on its own line to make the sheet balance meanwhile.
        var yearStart = new DateTime(request.ToDate.Year, 1, 1, 0, 0, 0, request.ToDate.Kind);
        var resultAccountNos = accounts
            .Where(a => a.IncomeBalance == IncomeBalanceType.IncomeStatement)
            .Select(a => a.No)
            .ToHashSet(StringComparer.Ordinal);

        var yearResult = -entries
            .Where(e => e.PostingDate >= yearStart && resultAccountNos.Contains(e.GLAccountNo))
            .Sum(e => e.Amount);

        var resultRow = result.AddRow();
        resultRow.Indentation = 1;
        resultRow.Values["accountNo"] = string.Empty;
        resultRow.Values["name"] = "Result for the Year";
        resultRow.Values["amount"] = yearResult;

        var total = result.AddRow();
        total.Bold = true;
        total.Values["accountNo"] = string.Empty;
        total.Values["name"] = "Total Liabilities and Equity";
        total.Values["amount"] = liabilities + equity + yearResult;

        // Assets against the other side: the two are equal exactly when the books balance.
        var check = result.AddRow();
        check.Bold = true;
        check.Values["accountNo"] = string.Empty;
        check.Values["name"] = "Difference";
        check.Values["amount"] = assets - (liabilities + equity + yearResult);

        return result;
    }

    /// <summary>
    /// Every G/L entry of the period for the accounts asked for, with a running balance.
    /// This is what a drill-down from any of the other reports lands on.
    /// </summary>
    public async Task<ReportResult> GenerateGLDetailAsync(FinancialReportRequest request)
    {
        EnsurePeriodIsForwards(request);

        var filter = ParseFilter(request.AccountFilter);

        var entries = (await _glEntryRepository.GetListAsync(e =>
                e.PostingDate >= request.FromDate && e.PostingDate <= request.ToDate
            ))
            .Where(e => filter.Matches(e.GLAccountNo))
            .OrderBy(e => e.GLAccountNo, StringComparer.Ordinal)
            .ThenBy(e => e.PostingDate)
            .ThenBy(e => e.EntryNo)
            .ToList();

        var result = NewResult("General Ledger Detail", request.FromDate, request.ToDate);
        result.Columns.Add(new ReportColumnDefinition("entryNo", "Entry No.", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("postingDate", "Posting Date", ReportColumnKind.Date));
        result.Columns.Add(new ReportColumnDefinition("accountNo", "Account", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("documentNo", "Document No.", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("description", "Description", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("debit", "Debit"));
        result.Columns.Add(new ReportColumnDefinition("credit", "Credit"));
        result.Columns.Add(new ReportColumnDefinition("balance", "Balance"));

        string currentAccount = null;
        var runningBalance = 0m;

        foreach (var entry in entries)
        {
            if (entry.GLAccountNo != currentAccount)
            {
                currentAccount = entry.GLAccountNo;
                runningBalance = 0m;
            }

            runningBalance += entry.Amount;

            var row = result.AddRow();
            row.Values["entryNo"] = entry.EntryNo.ToString();
            row.Values["postingDate"] = entry.PostingDate;
            row.Values["accountNo"] = entry.GLAccountNo;
            row.Values["documentNo"] = entry.DocumentNo;
            row.Values["description"] = entry.Description;
            row.Values["debit"] = entry.DebitAmount;
            row.Values["credit"] = entry.CreditAmount;
            row.Values["balance"] = runningBalance;
            row.Italic = entry.Reversed;
        }

        return result;
    }

    /// <summary>Adds the accounts of one category and their subtotal; returns the subtotal.</summary>
    private static decimal AppendCategory(
        ReportResult result,
        IEnumerable<GLAccount> accounts,
        IEnumerable<GLEntry> entries,
        GLAccountCategory category,
        string caption,
        bool flipSign,
        bool excludeZeroBalances
    )
    {
        var byAccount = entries.ToLookup(e => e.GLAccountNo);

        var heading = result.AddRow();
        heading.Bold = true;
        heading.Values["accountNo"] = string.Empty;
        heading.Values["name"] = caption;

        var subtotal = 0m;

        foreach (var account in accounts.Where(a => a.AccountCategory == category).OrderBy(a => a.No, StringComparer.Ordinal))
        {
            var amount = byAccount[account.No].Sum(e => e.Amount);
            subtotal += amount;

            if (excludeZeroBalances && amount == 0m)
            {
                continue;
            }

            var row = result.AddRow();
            row.Indentation = 1;
            row.Values["accountNo"] = account.No;
            row.Values["name"] = account.Name;
            row.Values["amount"] = flipSign ? -amount : amount;
            row.DrillDownFilter = account.No;
        }

        var totalRow = result.AddRow();
        totalRow.Bold = true;
        totalRow.Values["accountNo"] = string.Empty;
        totalRow.Values["name"] = $"Total {caption}";
        totalRow.Values["amount"] = flipSign ? -subtotal : subtotal;

        return flipSign ? -subtotal : subtotal;
    }

    private static AccountTotaling ParseFilter(string accountFilter)
    {
        return accountFilter.IsNullOrWhiteSpace() ? AccountTotaling.All : AccountTotaling.Parse(accountFilter);
    }

    private static void EnsurePeriodIsForwards(FinancialReportRequest request)
    {
        if (request.ToDate < request.FromDate)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }
    }

    private static ReportResult NewResult(string title, DateTime from, DateTime to)
    {
        return new ReportResult
        {
            Title = title,
            FromDate = from,
            ToDate = to,
        };
    }
}
