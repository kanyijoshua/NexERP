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

/// <summary>
/// Column layouts: the periods a financial report is shown across.
/// Mirrors Business Central pages 331 and 332.
/// </summary>
[Authorize(ErpPermissions.AccountSchedules.Default)]
public class ColumnLayoutAppService : ErpAppService, IColumnLayoutAppService
{
    private readonly IRepository<ColumnLayout, Guid> _layoutRepository;
    private readonly IRepository<ColumnLayoutLine, Guid> _lineRepository;

    public ColumnLayoutAppService(
        IRepository<ColumnLayout, Guid> layoutRepository,
        IRepository<ColumnLayoutLine, Guid> lineRepository
    )
    {
        _layoutRepository = layoutRepository;
        _lineRepository = lineRepository;
    }

    public async Task<ListResultDto<ColumnLayoutDto>> GetListAsync()
    {
        var layouts = (await _layoutRepository.GetListAsync()).OrderBy(l => l.Name).ToList();
        var lines = await _lineRepository.GetListAsync();

        var dtos = ObjectMapper.Map<List<ColumnLayout>, List<ColumnLayoutDto>>(layouts);

        for (var i = 0; i < layouts.Count; i++)
        {
            dtos[i].LineCount = lines.Count(l => l.ColumnLayoutId == layouts[i].Id);
        }

        return new ListResultDto<ColumnLayoutDto>(dtos);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<ColumnLayoutDto> CreateAsync(CreateUpdateColumnLayoutDto input)
    {
        if (await _layoutRepository.AnyAsync(l => l.Name == input.Name))
        {
            throw new BusinessException(ErpErrorCodes.Reports.ColumnLayoutNameAlreadyExists)
                .WithData("name", input.Name);
        }

        var layout = new ColumnLayout(GuidGenerator.Create(), input.Name, input.Description);

        await _layoutRepository.InsertAsync(layout, autoSave: true);
        return ObjectMapper.Map<ColumnLayout, ColumnLayoutDto>(layout);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<ColumnLayoutDto> UpdateAsync(Guid id, CreateUpdateColumnLayoutDto input)
    {
        var layout = await _layoutRepository.GetAsync(id);

        layout.SetDescription(input.Description);

        await _layoutRepository.UpdateAsync(layout, autoSave: true);
        return ObjectMapper.Map<ColumnLayout, ColumnLayoutDto>(layout);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _layoutRepository.DeleteAsync(id);
    }

    public async Task<ListResultDto<ColumnLayoutLineDto>> GetLinesAsync(Guid columnLayoutId)
    {
        var lines = (await _lineRepository.GetListAsync(l => l.ColumnLayoutId == columnLayoutId))
            .OrderBy(l => l.LineNo)
            .ToList();

        return new ListResultDto<ColumnLayoutLineDto>(
            ObjectMapper.Map<List<ColumnLayoutLine>, List<ColumnLayoutLineDto>>(lines)
        );
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<ColumnLayoutLineDto> CreateLineAsync(CreateUpdateColumnLayoutLineDto input)
    {
        await _layoutRepository.GetAsync(input.ColumnLayoutId);

        var existing = await _lineRepository.GetListAsync(l => l.ColumnLayoutId == input.ColumnLayoutId);
        var nextLineNo = existing.Count == 0 ? 10 : existing.Max(l => l.LineNo) + 10;

        var line = new ColumnLayoutLine(
            GuidGenerator.Create(),
            input.ColumnLayoutId,
            nextLineNo,
            input.ColumnNo,
            input.ColumnHeader,
            input.ColumnType,
            input.ComparisonDateFormula,
            input.ShowOppositeSign
        );

        await _lineRepository.InsertAsync(line, autoSave: true);
        return ObjectMapper.Map<ColumnLayoutLine, ColumnLayoutLineDto>(line);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<ColumnLayoutLineDto> UpdateLineAsync(Guid id, CreateUpdateColumnLayoutLineDto input)
    {
        var line = await _lineRepository.GetAsync(id);

        line.Update(input.ColumnHeader, input.ColumnType, input.ComparisonDateFormula, input.ShowOppositeSign);

        await _lineRepository.UpdateAsync(line, autoSave: true);
        return ObjectMapper.Map<ColumnLayoutLine, ColumnLayoutLineDto>(line);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task DeleteLineAsync(Guid id)
    {
        await _lineRepository.DeleteAsync(id);
    }
}
