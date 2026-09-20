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
/// Account schedules: the rows of a financial report.
/// Mirrors Business Central pages 103 and 104.
/// </summary>
[Authorize(ErpPermissions.AccountSchedules.Default)]
public class AccountScheduleAppService : ErpAppService, IAccountScheduleAppService
{
    private readonly IRepository<AccountSchedule, Guid> _scheduleRepository;
    private readonly IRepository<AccountScheduleLine, Guid> _lineRepository;

    public AccountScheduleAppService(
        IRepository<AccountSchedule, Guid> scheduleRepository,
        IRepository<AccountScheduleLine, Guid> lineRepository
    )
    {
        _scheduleRepository = scheduleRepository;
        _lineRepository = lineRepository;
    }

    public async Task<ListResultDto<AccountScheduleDto>> GetListAsync()
    {
        var schedules = (await _scheduleRepository.GetListAsync()).OrderBy(s => s.Name).ToList();
        var lines = await _lineRepository.GetListAsync();

        var dtos = ObjectMapper.Map<List<AccountSchedule>, List<AccountScheduleDto>>(schedules);

        for (var i = 0; i < schedules.Count; i++)
        {
            dtos[i].LineCount = lines.Count(l => l.AccountScheduleId == schedules[i].Id);
        }

        return new ListResultDto<AccountScheduleDto>(dtos);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<AccountScheduleDto> CreateAsync(CreateUpdateAccountScheduleDto input)
    {
        if (await _scheduleRepository.AnyAsync(s => s.Name == input.Name))
        {
            throw new BusinessException(ErpErrorCodes.Reports.ScheduleNameAlreadyExists).WithData("name", input.Name);
        }

        var schedule = new AccountSchedule(
            GuidGenerator.Create(),
            input.Name,
            input.Description,
            input.DefaultColumnLayoutName
        );

        await _scheduleRepository.InsertAsync(schedule, autoSave: true);
        return ObjectMapper.Map<AccountSchedule, AccountScheduleDto>(schedule);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<AccountScheduleDto> UpdateAsync(Guid id, CreateUpdateAccountScheduleDto input)
    {
        var schedule = await _scheduleRepository.GetAsync(id);

        schedule.Update(input.Description, input.DefaultColumnLayoutName);

        await _scheduleRepository.UpdateAsync(schedule, autoSave: true);
        return ObjectMapper.Map<AccountSchedule, AccountScheduleDto>(schedule);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _scheduleRepository.DeleteAsync(id);
    }

    public async Task<ListResultDto<AccountScheduleLineDto>> GetLinesAsync(Guid scheduleId)
    {
        var lines = (await _lineRepository.GetListAsync(l => l.AccountScheduleId == scheduleId))
            .OrderBy(l => l.LineNo)
            .ToList();

        return new ListResultDto<AccountScheduleLineDto>(
            ObjectMapper.Map<List<AccountScheduleLine>, List<AccountScheduleLineDto>>(lines)
        );
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<AccountScheduleLineDto> CreateLineAsync(CreateUpdateAccountScheduleLineDto input)
    {
        await _scheduleRepository.GetAsync(input.AccountScheduleId);

        var existing = await _lineRepository.GetListAsync(l => l.AccountScheduleId == input.AccountScheduleId);
        var nextLineNo = existing.Count == 0 ? 10 : existing.Max(l => l.LineNo) + 10;

        var line = new AccountScheduleLine(
            GuidGenerator.Create(),
            input.AccountScheduleId,
            nextLineNo,
            input.RowNo,
            input.Description,
            input.TotalingType,
            input.Totaling,
            input.ShowOppositeSign,
            input.Bold,
            input.Italic,
            input.Indentation,
            input.HideIfZero
        );

        await _lineRepository.InsertAsync(line, autoSave: true);
        return ObjectMapper.Map<AccountScheduleLine, AccountScheduleLineDto>(line);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task<AccountScheduleLineDto> UpdateLineAsync(Guid id, CreateUpdateAccountScheduleLineDto input)
    {
        var line = await _lineRepository.GetAsync(id);

        line.Update(
            input.Description,
            input.TotalingType,
            input.Totaling,
            input.ShowOppositeSign,
            input.Bold,
            input.Italic,
            input.Indentation,
            input.HideIfZero
        );

        await _lineRepository.UpdateAsync(line, autoSave: true);
        return ObjectMapper.Map<AccountScheduleLine, AccountScheduleLineDto>(line);
    }

    [Authorize(ErpPermissions.AccountSchedules.Manage)]
    public async Task DeleteLineAsync(Guid id)
    {
        await _lineRepository.DeleteAsync(id);
    }
}
