using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// The RDLC report layouts that ship with the system: each file is converted for the report it belongs to and offered as one of that report's layouts. None is made the
/// default, so a report prints as before until someone chooses the borrowed layout.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedBundledReportLayoutsAsync()
    {
        var layouts = Repo<CustomReportLayout>();
        var resolver = _serviceProvider.GetRequiredService<ReportColumnsResolver>();

        foreach (var bundled in BundledReportLayouts.All)
        {
            if (await layouts.AnyAsync(l => l.ReportName == bundled.ReportName && l.LayoutName == bundled.LayoutName))
            {
                continue;
            }

            var conversion = RdlcLayoutConverter.Convert(BundledReportLayouts.Read(bundled), await resolver.GetAsync(bundled.ReportName));

            await layouts.InsertAsync(
                new CustomReportLayout(
                    NewId(),
                    bundled.ReportName,
                    bundled.LayoutName,
                    ReportLayoutType.Html,
                    conversion.Template,
                    $"Converted from the RDLC layout {bundled.FileName}; {conversion.MatchedColumns} column(s) matched.",
                    code: bundled.LayoutCode,
                    reportId: bundled.SourceReportId,
                    builtIn: true,
                    lastModifiedByUser: "System",
                    layoutLastUpdated: DateTime.UtcNow
                )
            );
        }
    }
}
