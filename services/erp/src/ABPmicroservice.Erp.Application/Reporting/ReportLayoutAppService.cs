using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

public class CreateReportLayoutDto
{
    public string ReportName { get; set; }
    public string LayoutName { get; set; }
    public string LayoutType { get; set; }
    public string Description { get; set; }
}

public class ReportLayoutAppService : ApplicationService
{
    private readonly IRepository<CustomReportLayout, Guid> _layoutRepository;
    private readonly IRepository<ReportLayoutSelection, Guid> _selectionRepository;

    public ReportLayoutAppService(
        IRepository<CustomReportLayout, Guid> layoutRepository,
        IRepository<ReportLayoutSelection, Guid> selectionRepository
    )
    {
        _layoutRepository = layoutRepository;
        _selectionRepository = selectionRepository;
    }

    public async Task<List<CustomReportLayout>> GetLayoutsAsync(string reportName)
    {
        return await _layoutRepository.GetListAsync(l => l.ReportName == reportName);
    }

    public async Task<CustomReportLayout> CreateLayoutAsync(CreateReportLayoutDto input)
    {
        var layout = new CustomReportLayout(
            GuidGenerator.Create(),
            input.ReportName,
            input.LayoutName,
            input.LayoutType,
            input.Description
        );
        return await _layoutRepository.InsertAsync(layout, autoSave: true);
    }

    public async Task SetDefaultLayoutAsync(string reportName, Guid layoutId)
    {
        var layouts = await _layoutRepository.GetListAsync(l => l.ReportName == reportName);
        foreach (var l in layouts)
        {
            l.SetDefault(l.Id == layoutId);
            await _layoutRepository.UpdateAsync(l);
        }

        var selected = layouts.FirstOrDefault(l => l.Id == layoutId);
        if (selected != null)
        {
            var selection = await _selectionRepository.FirstOrDefaultAsync(s => s.ReportName == reportName);
            if (selection == null)
            {
                selection = new ReportLayoutSelection(GuidGenerator.Create(), reportName, layoutId, selected.LayoutType);
                await _selectionRepository.InsertAsync(selection);
            }
            else
            {
                selection.SetSelectedLayout(layoutId, selected.LayoutType);
                await _selectionRepository.UpdateAsync(selection);
            }
        }
    }
}
