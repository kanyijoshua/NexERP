using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Content;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// The financial statements. Every one of them returns the same shape, and any of them can be
/// exported through the same action, which is what lets one screen in the client run them all.
/// </summary>
[Authorize(ErpPermissions.Reports.Default)]
public class FinancialReportAppService : ErpAppService, IFinancialReportAppService
{
    private readonly FinancialReportEngine _reportEngine;
    private readonly AgedAccountsEngine _agedAccountsEngine;
    private readonly AccountScheduleEngine _scheduleEngine;
    private readonly DataExportEngine _exportEngine;

    public FinancialReportAppService(
        FinancialReportEngine reportEngine,
        AgedAccountsEngine agedAccountsEngine,
        AccountScheduleEngine scheduleEngine,
        DataExportEngine exportEngine
    )
    {
        _reportEngine = reportEngine;
        _agedAccountsEngine = agedAccountsEngine;
        _scheduleEngine = scheduleEngine;
        _exportEngine = exportEngine;
    }

    public async Task<ReportResultDto> GetTrialBalanceAsync(FinancialReportPeriodInput input)
    {
        return Map(await _reportEngine.GenerateTrialBalanceAsync(ToRequest(input)));
    }

    public async Task<ReportResultDto> GetIncomeStatementAsync(FinancialReportPeriodInput input)
    {
        return Map(await _reportEngine.GenerateIncomeStatementAsync(ToRequest(input)));
    }

    public async Task<ReportResultDto> GetBalanceSheetAsync(FinancialReportPeriodInput input)
    {
        return Map(await _reportEngine.GenerateBalanceSheetAsync(ToRequest(input)));
    }

    public async Task<ReportResultDto> GetGLDetailAsync(FinancialReportPeriodInput input)
    {
        return Map(await _reportEngine.GenerateGLDetailAsync(ToRequest(input)));
    }

    public async Task<ReportResultDto> GetAgedAccountsAsync(AgedAccountsInput input)
    {
        await AuthorizeAgedAsync(input.Kind);

        return Map(await _agedAccountsEngine.GenerateAsync(ToRequest(input)));
    }

    [Authorize(ErpPermissions.AccountSchedules.Default)]
    public async Task<ReportResultDto> RunScheduleAsync(RunAccountScheduleInput input)
    {
        return Map(await _scheduleEngine.RunAsync(ToRequest(input)));
    }

    [Authorize(ErpPermissions.Reports.ExportExcel)]
    public async Task<IRemoteStreamContent> RunExportAsync(ReportExportInput input)
    {
        var result = await RunAsync(input);

        var columns = result.Columns.Select(c => new ExportColumn(c.Key, c.Header)).ToList();
        var rows = result.Rows.Select(r => (IReadOnlyDictionary<string, object>)r.Values).ToList();

        var file = _exportEngine.Write(result.Title, columns, rows, input.Format);

        return new RemoteStreamContent(new MemoryStream(file.Content), file.FileName, file.ContentType);
    }

    /// <summary>Runs whichever report the export asked for, with that report's own parameters.</summary>
    private async Task<ReportResult> RunAsync(ReportExportInput input)
    {
        var period = new FinancialReportRequest
        {
            FromDate = input.FromDate,
            ToDate = input.ToDate,
            AccountFilter = input.AccountFilter,
            ExcludeZeroBalances = input.ExcludeZeroBalances,
        };

        switch (input.Report)
        {
            case ReportKind.TrialBalance:
                return await _reportEngine.GenerateTrialBalanceAsync(period);

            case ReportKind.IncomeStatement:
                return await _reportEngine.GenerateIncomeStatementAsync(period);

            case ReportKind.BalanceSheet:
                return await _reportEngine.GenerateBalanceSheetAsync(period);

            case ReportKind.GeneralLedgerDetail:
                return await _reportEngine.GenerateGLDetailAsync(period);

            case ReportKind.AgedReceivables:
            case ReportKind.AgedPayables:
                var kind =
                    input.Report == ReportKind.AgedReceivables ? AgedLedgerKind.Receivables : AgedLedgerKind.Payables;
                await AuthorizeAgedAsync(kind);

                return await _agedAccountsEngine.GenerateAsync(
                    new AgedAccountsRequest
                    {
                        Kind = kind,
                        AsOfDate = input.ToDate,
                        AgingMethod = input.AgingMethod,
                        PeriodLengthDays = input.PeriodLengthDays,
                        ExcludeZeroBalances = input.ExcludeZeroBalances,
                    }
                );

            default:
                await AuthorizationService.CheckAsync(ErpPermissions.AccountSchedules.Default);

                return await _scheduleEngine.RunAsync(
                    new AccountScheduleRunRequest
                    {
                        ScheduleName = input.ScheduleName,
                        ColumnLayoutName = input.ColumnLayoutName,
                        FromDate = input.FromDate,
                        ToDate = input.ToDate,
                    }
                );
        }
    }

    /// <summary>
    /// Aged balances are customer and vendor data, so they need the same permission as the lists
    /// they summarise rather than the general reporting one.
    /// </summary>
    private async Task AuthorizeAgedAsync(AgedLedgerKind kind)
    {
        await AuthorizationService.CheckAsync(
            kind == AgedLedgerKind.Receivables ? ErpPermissions.Customers.Default : ErpPermissions.Vendors.Default
        );
    }

    private static FinancialReportRequest ToRequest(FinancialReportPeriodInput input)
    {
        return new FinancialReportRequest
        {
            FromDate = input.FromDate,
            ToDate = input.ToDate,
            AccountFilter = input.AccountFilter,
            ExcludeZeroBalances = input.ExcludeZeroBalances,
        };
    }

    private static AgedAccountsRequest ToRequest(AgedAccountsInput input)
    {
        return new AgedAccountsRequest
        {
            Kind = input.Kind,
            AsOfDate = input.AsOfDate,
            AgingMethod = input.AgingMethod,
            PeriodLengthDays = input.PeriodLengthDays,
            ExcludeZeroBalances = input.ExcludeZeroBalances,
        };
    }

    private static AccountScheduleRunRequest ToRequest(RunAccountScheduleInput input)
    {
        return new AccountScheduleRunRequest
        {
            ScheduleName = input.ScheduleName,
            ColumnLayoutName = input.ColumnLayoutName,
            FromDate = input.FromDate,
            ToDate = input.ToDate,
        };
    }

    private ReportResultDto Map(ReportResult result)
    {
        return ObjectMapper.Map<ReportResult, ReportResultDto>(result);
    }
}
