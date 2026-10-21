using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>
/// Base Calendars, and what they say about dates: the dates of a range, where a number of
/// working days ends, and making a single date working or nonworking.
/// </summary>
public class BaseCalendarAppService
    : HumanResourcesCodeTableAppService<BaseCalendar, CodeTableDto, CreateUpdateCodeTableDto>,
        IBaseCalendarAppService
{
    // A year and a month: enough for a year view, small enough to send.
    private const int MaxDatesPerRequest = 400;

    private readonly IRepository<BaseCalendarChange, Guid> _changeRepository;
    private readonly IRepository<Employee, Guid> _employeeRepository;
    private readonly BaseCalendarManager _calendarManager;

    public BaseCalendarAppService(
        IRepository<BaseCalendar, Guid> repository,
        IRepository<BaseCalendarChange, Guid> changeRepository,
        IRepository<Employee, Guid> employeeRepository,
        BaseCalendarManager calendarManager
    )
        : base(repository)
    {
        _changeRepository = changeRepository;
        _employeeRepository = employeeRepository;
        _calendarManager = calendarManager;
    }

    protected override BaseCalendar NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);

    /// <summary>A calendar goes together with its changes.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        var calendar = await GetEntityByIdAsync(id);
        await base.DeleteAsync(id);
        await _changeRepository.DeleteAsync(c => c.BaseCalendarCode == calendar.Code, autoSave: true);
    }

    public async Task<CalendarDatesDto> GetDatesAsync(GetCalendarDatesInput input)
    {
        await CheckGetListPolicyAsync();

        var from = input.FromDate.Date;
        var to = input.ToDate.Date;
        if (to < from || (to - from).TotalDays > MaxDatesPerRequest)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.InvalidAbsencePeriod);
        }

        var code = await ResolveCodeAsync(input.CalendarCode, input.EmployeeNo);
        var dates = await _calendarManager.GetDatesAsync(code, from, to);
        return new CalendarDatesDto
        {
            CalendarCode = code,
            WorkingDays = dates.Count(d => !d.Nonworking),
            NonworkingDays = dates.Count(d => d.Nonworking),
            Dates = dates.Select(Map).ToList(),
        };
    }

    public async Task<WorkingPeriodDto> CalculateWorkingPeriodAsync(CalculateWorkingPeriodInput input)
    {
        await CheckGetListPolicyAsync();

        var code = await ResolveCodeAsync(input.CalendarCode, input.EmployeeNo);
        var period = await _calendarManager.CalculatePeriodAsync(code, input.StartDate, input.WorkingDays);
        return new WorkingPeriodDto
        {
            CalendarCode = code,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            ReturnDate = period.ReturnDate,
            WorkingDays = period.WorkingDays,
            NonworkingDates = period.NonworkingDates.Select(Map).ToList(),
        };
    }

    /// <summary>
    /// Adds, changes or removes the one-date change of that date so the date ends up as asked;
    /// a one-date change that only repeats what the recurring changes say is not kept.
    /// </summary>
    [Authorize(ErpPermissions.HumanResourcesSetup.Update)]
    public async Task SetDateStatusAsync(SetCalendarDateStatusInput input)
    {
        var code = CodeTableEntity.NormalizeCode(input.CalendarCode);
        await CodeTableChecker.EnsureExistsAsync<BaseCalendar>(code);

        var date = input.Date.Date;
        var existing = (await _changeRepository.GetListAsync(c =>
                c.BaseCalendarCode == code && c.RecurringSystem == CalendarRecurringSystem.None && c.Date == date))
            .FirstOrDefault();
        if (existing != null)
        {
            await _changeRepository.DeleteAsync(existing, autoSave: true);
        }

        if (await _calendarManager.IsNonworkingAsync(code, date) == input.Nonworking)
        {
            return;
        }

        var change = new BaseCalendarChange(GuidGenerator.Create(), code);
        change.Set(CalendarRecurringSystem.None, date, CalendarWeekday.None, input.Description, input.Nonworking);
        await _changeRepository.InsertAsync(change, autoSave: true);
    }

    private async Task<string> ResolveCodeAsync(string calendarCode, string employeeNo)
    {
        var code = CodeTableEntity.NormalizeCode(calendarCode);
        if (code != null)
        {
            return code;
        }

        Employee employee = null;
        if (!employeeNo.IsNullOrWhiteSpace())
        {
            var no = employeeNo.Trim();
            employee = await _employeeRepository.FirstOrDefaultAsync(e => e.No == no)
                ?? throw new BusinessException(ErpErrorCodes.HumanResources.EmployeeNotFound).WithData("no", no);
        }

        return await _calendarManager.ResolveCalendarCodeAsync(employee);
    }

    private static CalendarDateDto Map(CalendarDate date) => new()
    {
        Date = date.Date,
        Weekday = date.Weekday,
        Nonworking = date.Nonworking,
        Description = date.Description,
    };
}

/// <summary>Base Calendar Changes: the dates and weekdays that differ from a working day.</summary>
public class BaseCalendarChangeAppService
    : ErpTableAppService<BaseCalendarChange, BaseCalendarChangeDto, GetBaseCalendarChangeListInput, CreateUpdateBaseCalendarChangeDto>,
        IBaseCalendarChangeAppService
{
    public BaseCalendarChangeAppService(IRepository<BaseCalendarChange, Guid> repository)
        : base(repository, ErpPermissions.HumanResourcesSetup.Default) { }

    public override async Task<BaseCalendarChangeDto> CreateAsync(CreateUpdateBaseCalendarChangeDto input)
    {
        await CheckCreatePolicyAsync();
        await CodeTableChecker.EnsureExistsAsync<BaseCalendar>(input.BaseCalendarCode);

        var entity = new BaseCalendarChange(GuidGenerator.Create(), input.BaseCalendarCode);
        entity.Set(input.RecurringSystem, input.Date, input.Day, input.Description, input.Nonworking);
        await EnsureNotDuplicateAsync(entity, null);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<BaseCalendarChangeDto> UpdateAsync(Guid id, CreateUpdateBaseCalendarChangeDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        entity.Set(input.RecurringSystem, input.Date, input.Day, input.Description, input.Nonworking);
        await EnsureNotDuplicateAsync(entity, id);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<BaseCalendarChange>> CreateFilteredQueryAsync(GetBaseCalendarChangeListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var calendarCode = input.BaseCalendarCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!calendarCode.IsNullOrEmpty(), x => x.BaseCalendarCode == calendarCode)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.BaseCalendarCode.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<BaseCalendarChange> ApplyDefaultSorting(IQueryable<BaseCalendarChange> query) =>
        query.OrderBy(x => x.BaseCalendarCode).ThenBy(x => x.RecurringSystem).ThenBy(x => x.Date).ThenBy(x => x.Day);

    // The key: calendar, recurring system, date and weekday.
    private async Task EnsureNotDuplicateAsync(BaseCalendarChange change, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.BaseCalendarCode == change.BaseCalendarCode
                && x.RecurringSystem == change.RecurringSystem
                && x.Date == change.Date
                && x.Day == change.Day
                && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Base Calendar Change")
                .WithData("key", $"{change.BaseCalendarCode} {change.RecurringSystem} {change.Date:yyyy-MM-dd} {change.Day}");
        }
    }
}
