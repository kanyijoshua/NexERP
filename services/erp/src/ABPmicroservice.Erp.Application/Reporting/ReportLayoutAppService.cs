using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

[Authorize(ErpPermissions.ReportLayouts.Default)]
public class ReportLayoutAppService : ErpAppService, IReportLayoutAppService
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

    public async Task<ListResultDto<ReportLayoutDto>> GetListAsync(GetReportLayoutsInput input)
    {
        var layouts = await _layoutRepository.GetListAsync(l => l.ReportName == input.ReportName);

        return new ListResultDto<ReportLayoutDto>(
            ObjectMapper.Map<List<CustomReportLayout>, List<ReportLayoutDto>>(
                layouts.OrderByDescending(l => l.IsDefault).ThenBy(l => l.LayoutName).ToList()
            )
        );
    }

    [Authorize(ErpPermissions.ReportLayouts.Manage)]
    public async Task<ReportLayoutDto> CreateAsync(CreateReportLayoutDto input)
    {
        var layout = new CustomReportLayout(
            GuidGenerator.Create(),
            input.ReportName,
            input.LayoutName,
            input.LayoutType,
            input.Description
        );

        await _layoutRepository.InsertAsync(layout, autoSave: true);
        return ObjectMapper.Map<CustomReportLayout, ReportLayoutDto>(layout);
    }

    [Authorize(ErpPermissions.ReportLayouts.Manage)]
    public async Task SetDefaultAsync(SetDefaultReportLayoutInput input)
    {
        var layouts = await _layoutRepository.GetListAsync(l => l.ReportName == input.ReportName);

        var selected = layouts.FirstOrDefault(l => l.Id == input.LayoutId);
        if (selected == null)
        {
            throw new UserFriendlyException(
                $"Layout '{input.LayoutId}' does not belong to report '{input.ReportName}'."
            );
        }

        foreach (var layout in layouts.Where(l => l.IsDefault != (l.Id == input.LayoutId)))
        {
            layout.SetDefault(layout.Id == input.LayoutId);
            await _layoutRepository.UpdateAsync(layout);
        }

        var selection = await _selectionRepository.FirstOrDefaultAsync(s => s.ReportName == input.ReportName);
        if (selection == null)
        {
            await _selectionRepository.InsertAsync(
                new ReportLayoutSelection(GuidGenerator.Create(), input.ReportName, selected.Id, selected.LayoutType)
            );
        }
        else
        {
            selection.SetSelectedLayout(selected.Id, selected.LayoutType);
            await _selectionRepository.UpdateAsync(selection);
        }
    }
}
