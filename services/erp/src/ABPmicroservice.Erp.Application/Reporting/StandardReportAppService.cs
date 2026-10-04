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
/// Runs the reports of the standard catalog. A report shows the same data as the lists of its
/// area, so it needs the permission that guards those lists rather than a reporting one.
/// </summary>
[Authorize]
public class StandardReportAppService : ErpAppService, IStandardReportAppService
{
    private static readonly Dictionary<string, string> AreaPermissions = new()
    {
        [StandardReportAreas.Finance] = ErpPermissions.Reports.Default,
        [StandardReportAreas.Sales] = ErpPermissions.Customers.Default,
        [StandardReportAreas.Purchasing] = ErpPermissions.Vendors.Default,
        [StandardReportAreas.CashManagement] = ErpPermissions.BankAccounts.Default,
        [StandardReportAreas.HumanResources] = ErpPermissions.Employees.Default,
        [StandardReportAreas.FixedAssets] = ErpPermissions.FixedAssets.Default,
        [StandardReportAreas.Pensions] = ErpPermissions.Pensions.Default,
        [StandardReportAreas.Academics] = ErpPermissions.Academics.Default,
        [StandardReportAreas.Payroll] = ErpPermissions.Payroll.Default,
    };

    private readonly StandardReportCatalog _catalog;
    private readonly DataExportEngine _exportEngine;
    private readonly ReportLayoutRenderer _layoutRenderer;

    public StandardReportAppService(StandardReportCatalog catalog, DataExportEngine exportEngine, ReportLayoutRenderer layoutRenderer)
    {
        _catalog = catalog;
        _exportEngine = exportEngine;
        _layoutRenderer = layoutRenderer;
    }

    /// <summary>The permission a report of <paramref name="area"/> needs.</summary>
    public static string PermissionOf(string area) => AreaPermissions[area];

    public async Task<List<StandardReportDto>> GetListAsync()
    {
        var reports = new List<StandardReportDto>();

        foreach (var definition in _catalog.GetAll())
        {
            if (await AuthorizationService.IsGrantedAsync(PermissionOf(definition.Area)))
            {
                reports.Add(Map(definition));
            }
        }

        return reports;
    }

    public async Task<ReportResultDto> RunAsync(RunStandardReportInput input)
    {
        return ObjectMapper.Map<ReportResult, ReportResultDto>(await RunReportAsync(input));
    }

    public async Task<IRemoteStreamContent> RunExportAsync(StandardReportExportInput input)
    {
        await AuthorizationService.CheckAsync(ErpPermissions.Reports.ExportExcel);

        var result = await RunReportAsync(input);

        if (input.Format == ExportFormat.Html)
        {
            var html = await _layoutRenderer.RenderAsync(ReportLayoutNames.ForStandardReport(_catalog.Get(input.Code).Definition.Code), result);

            return new RemoteStreamContent(
                new MemoryStream(ReportLayoutRenderer.ToBytes(html)),
                $"{DataExportEngine.Sanitize(result.Title)}.html",
                "text/html"
            );
        }

        var columns = result.Columns.Select(c => new ExportColumn(c.Key, c.Header)).ToList();
        var rows = result.Rows.Select(r => (IReadOnlyDictionary<string, object>)r.Values).ToList();
        var file = _exportEngine.Write(result.Title, columns, rows, input.Format);

        return new RemoteStreamContent(new MemoryStream(file.Content), file.FileName, file.ContentType);
    }

    private async Task<ReportResult> RunReportAsync(RunStandardReportInput input)
    {
        var report = _catalog.Get(input.Code);
        await AuthorizationService.CheckAsync(PermissionOf(report.Definition.Area));

        return await report.RunAsync(
            new StandardReportRequest
            {
                FromDate = input.FromDate,
                ToDate = input.ToDate,
                NoFilter = input.NoFilter,
                BudgetName = input.BudgetName,
                DepreciationBookCode = input.DepreciationBookCode,
                SchemeCode = input.SchemeCode,
            }
        );
    }

    private static StandardReportDto Map(StandardReportDefinition definition)
    {
        return new StandardReportDto
        {
            Id = definition.Id,
            Code = definition.Code,
            Name = definition.Name,
            Area = definition.Area,
            HasPeriod = definition.Parameters.HasFlag(StandardReportParameters.Period),
            HasAsOfDate = definition.Parameters.HasFlag(StandardReportParameters.AsOfDate),
            HasNoFilter = definition.Parameters.HasFlag(StandardReportParameters.NoFilter),
            HasBudgetName = definition.Parameters.HasFlag(StandardReportParameters.BudgetName),
            HasDepreciationBook = definition.Parameters.HasFlag(StandardReportParameters.DepreciationBook),
            HasScheme = definition.Parameters.HasFlag(StandardReportParameters.Scheme),
        };
    }
}
