using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Timing;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Renders a report through the layout this company has chosen for it.
/// <para>
/// Mirrors what Business Central does when it prints: look up the Report Layout Selection, fall
/// back to the built-in layout if there is none, and render the dataset through it.
/// </para>
/// </summary>
public class ReportLayoutRenderer : DomainService
{
    private readonly IRepository<CustomReportLayout, Guid> _layoutRepository;
    private readonly IRepository<ReportLayoutSelection, Guid> _selectionRepository;
    private readonly ICurrentCompany _currentCompany;
    private readonly IClock _clock;

    public ReportLayoutRenderer(
        IRepository<CustomReportLayout, Guid> layoutRepository,
        IRepository<ReportLayoutSelection, Guid> selectionRepository,
        ICurrentCompany currentCompany,
        IClock clock
    )
    {
        _layoutRepository = layoutRepository;
        _selectionRepository = selectionRepository;
        _currentCompany = currentCompany;
        _clock = clock;
    }

    /// <summary>Renders the report as a standalone HTML document.</summary>
    public async Task<string> RenderAsync(string reportName, ReportResult result)
    {
        var template = ReportTemplate.Parse(await GetTemplateAsync(reportName));

        return template.Render(
            result,
            new ReportRenderContext { CompanyName = _currentCompany.Name, PrintedOn = _clock.Now }
        );
    }

    /// <summary>Renders a result through the built-in layout, for output that has no layout of its own.</summary>
    public string RenderBuiltIn(ReportResult result)
    {
        return ReportTemplate.Parse(ReportLayoutTemplates.BuiltIn).Render(
            result,
            new ReportRenderContext { CompanyName = _currentCompany.Name, PrintedOn = _clock.Now }
        );
    }

    public static byte[] ToBytes(string html)
    {
        return Encoding.UTF8.GetBytes(html);
    }

    /// <summary>
    /// The layout this company prints the report through: the selected one, else the one marked
    /// default, else the built-in.
    /// </summary>
    public async Task<string> GetTemplateAsync(string reportName)
    {
        if (reportName.IsNullOrWhiteSpace())
        {
            return ReportLayoutTemplates.BuiltIn;
        }

        var layouts = await _layoutRepository.GetListAsync(l => l.ReportName == reportName);
        if (layouts.Count == 0)
        {
            return ReportLayoutTemplates.BuiltIn;
        }

        var selection = await _selectionRepository.FirstOrDefaultAsync(s => s.ReportName == reportName);

        var selected =
            (selection == null ? null : layouts.FirstOrDefault(l => l.Id == selection.SelectedLayoutId))
            ?? layouts.FirstOrDefault(l => l.IsDefault);

        // A layout of a type nothing can render yet must not stop the report printing.
        return selected == null || selected.LayoutType != ReportLayoutType.Html
            ? ReportLayoutTemplates.BuiltIn
            : selected.TemplateContent;
    }
}
