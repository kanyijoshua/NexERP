using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// The columns a report produces, by the name a layout is attached to it under. A layout that is
/// converted from somewhere else has to be fitted to these.
/// </summary>
public class ReportColumnsResolver : DomainService
{
    private readonly StandardReportCatalog _catalog;
    private readonly FinancialReportEngine _financialReports;
    private readonly AgedAccountsEngine _agedAccounts;

    public ReportColumnsResolver(StandardReportCatalog catalog, FinancialReportEngine financialReports, AgedAccountsEngine agedAccounts)
    {
        _catalog = catalog;
        _financialReports = financialReports;
        _agedAccounts = agedAccounts;
    }

    /// <summary>
    /// The report's columns, found by running it for a single day; empty for a report whose
    /// columns are the user's own (an account schedule) or that is not known.
    /// </summary>
    public async Task<IReadOnlyList<ReportColumnDefinition>> GetAsync(string reportName)
    {
        var today = Clock.Now.Date;

        var standard = ReportLayoutNames.StandardReportOf(reportName);
        if (standard != null)
        {
            var report = _catalog.Find(standard);
            return report == null ? [] : (await report.RunAsync(new StandardReportRequest { FromDate = today, ToDate = today })).Columns;
        }

        if (!Enum.TryParse<ReportKind>(reportName, ignoreCase: true, out var kind))
        {
            return [];
        }

        var period = new FinancialReportRequest { FromDate = today, ToDate = today };

        return kind switch
        {
            ReportKind.TrialBalance => (await _financialReports.GenerateTrialBalanceAsync(period)).Columns,
            ReportKind.IncomeStatement => (await _financialReports.GenerateIncomeStatementAsync(period)).Columns,
            ReportKind.BalanceSheet => (await _financialReports.GenerateBalanceSheetAsync(period)).Columns,
            ReportKind.GeneralLedgerDetail => (await _financialReports.GenerateGLDetailAsync(period)).Columns,
            ReportKind.AgedReceivables => (await _agedAccounts.GenerateAsync(new AgedAccountsRequest { Kind = AgedLedgerKind.Receivables, AsOfDate = today })).Columns,
            ReportKind.AgedPayables => (await _agedAccounts.GenerateAsync(new AgedAccountsRequest { Kind = AgedLedgerKind.Payables, AsOfDate = today })).Columns,
            _ => [],
        };
    }
}
