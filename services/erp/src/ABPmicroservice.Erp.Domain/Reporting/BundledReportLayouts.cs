using System;
using System.Collections.Generic;
using System.IO;
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

/// <summary>One RDLC layout that ships with the system, and the report it is offered for.</summary>
public sealed record BundledReportLayout(string FileName, string ReportName, string LayoutCode, string LayoutName, int SourceReportId);

/// <summary>
/// The RDLC layouts that ship with the system. They are kept as the files they were drawn as, and
/// converted when a company is set up, so that what a company gets is what the
/// converter makes of them today rather than a frozen copy.
/// </summary>
public static class BundledReportLayouts
{
    private const string ResourcePrefix = "ABPmicroservice.Erp.Reporting.BundledLayouts.";

    public static readonly IReadOnlyList<BundledReportLayout> All =
    [
        new("MemberBalances.rdlc", ReportLayoutNames.ForStandardReport("MemberBalances"), "CLASSIC-MEMBAL", "Classic Member Balances", 51520141),
        new("MemberStatement.rdl", ReportLayoutNames.ForStandardReport("MemberStatement"), "CLASSIC-MEMSTMT", "Classic Member Statement", 51520011),
        new("ContributionsRegister.rdlc", ReportLayoutNames.ForStandardReport("ContributionsRegister"), "CLASSIC-CONTREG", "Classic Contributions Register", 51520132),
        new("MemberListingDC.rdlc", ReportLayoutNames.ForStandardReport("MemberListing"), "CLASSIC-MEMLIST", "Classic Member Listing", 51520116),
        new("ActiveMembers.rdlc", ReportLayoutNames.ForStandardReport("ActiveMembers"), "CLASSIC-ACTIVE", "Classic Active Members", 51520118),
        new("ExpectedRetirees.rdlc", ReportLayoutNames.ForStandardReport("ExpectedRetirees"), "CLASSIC-RETIREES", "Classic Expected Retirees", 51520108),
        new("EmployeeReport.rdl", ReportLayoutNames.ForStandardReport("EmployeeList"), "CLASSIC-EMPLOYEE", "Classic Employee Report", 51519142),
        new("FixedAssetRegister.rdl", ReportLayoutNames.ForStandardReport("FixedAssetBookValue"), "CLASSIC-FAREG", "Classic Fixed Assets Register", 51519048),
        new("BankStatementReport.rdl", ReportLayoutNames.ForStandardReport("BankAccountDetailTrialBalance"), "CLASSIC-BANKSTMT", "Classic Bank Statement", 51521657),
        new("TrialBalanceModified.rdl", ReportLayoutNames.For(ReportKind.TrialBalance), "CLASSIC-TRIALBAL", "Classic Trial Balance", 51519043),
    ];

    /// <summary>The RDLC file as it was drawn.</summary>
    public static string Read(BundledReportLayout layout)
    {
        var assembly = typeof(BundledReportLayouts).Assembly;
        var name = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Equals(ResourcePrefix + layout.FileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException("The layout is not embedded in the assembly.", layout.FileName);

        using var stream = assembly.GetManifestResourceStream(name);
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
}
