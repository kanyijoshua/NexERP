using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.HumanResources;

public class BaseCalendarChangeDto : FullAuditedEntityDto<Guid>
{
    public string BaseCalendarCode { get; set; }
    public CalendarRecurringSystem RecurringSystem { get; set; }
    public DateTime? Date { get; set; }
    public CalendarWeekday Day { get; set; }
    public string Description { get; set; }
    public bool Nonworking { get; set; }
}

public class CreateUpdateBaseCalendarChangeDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BaseCalendarCode { get; set; }

    public CalendarRecurringSystem RecurringSystem { get; set; }

    /// <summary>Required unless the change is weekly.</summary>
    public DateTime? Date { get; set; }

    /// <summary>Required for a weekly change.</summary>
    public CalendarWeekday Day { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public bool Nonworking { get; set; } = true;
}

public class GetBaseCalendarChangeListInput : ErpPagedListInput
{
    /// <summary>Matches the calendar code or the description, ignoring case.</summary>
    public string Filter { get; set; }
    public string BaseCalendarCode { get; set; }
}

public class CalendarDateDto
{
    public DateTime Date { get; set; }
    public CalendarWeekday Weekday { get; set; }
    public bool Nonworking { get; set; }
    public string Description { get; set; }
}

public class GetCalendarDatesInput
{
    /// <summary>Blank uses the employee's calendar, then the Human Resources Setup's.</summary>
    public string CalendarCode { get; set; }

    public string EmployeeNo { get; set; }

    public DateTime FromDate { get; set; }

    /// <summary>At most 400 days after the from date.</summary>
    public DateTime ToDate { get; set; }
}

public class CalendarDatesDto
{
    /// <summary>The calendar the dates come from; blank when none is set up and every day is a working day.</summary>
    public string CalendarCode { get; set; }

    public int WorkingDays { get; set; }
    public int NonworkingDays { get; set; }
    public List<CalendarDateDto> Dates { get; set; } = new();
}

public class CalculateWorkingPeriodInput
{
    public string CalendarCode { get; set; }
    public string EmployeeNo { get; set; }
    public DateTime StartDate { get; set; }

    [Range(1, 3650)]
    public int WorkingDays { get; set; }
}

public class WorkingPeriodDto
{
    public string CalendarCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public int WorkingDays { get; set; }
    public List<CalendarDateDto> NonworkingDates { get; set; } = new();
}

/// <summary>Makes one date working or nonworking, as clicking it on the calendar does.</summary>
public class SetCalendarDateStatusInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CalendarCode { get; set; }

    public DateTime Date { get; set; }
    public bool Nonworking { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public interface IBaseCalendarAppService
    : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto>
{
    /// <summary>Each date of a range with its status. Routed as GET /api/erp/base-calendar/dates.</summary>
    Task<CalendarDatesDto> GetDatesAsync(GetCalendarDatesInput input);

    /// <summary>End and return date of a number of working days. Routed as POST /api/erp/base-calendar/calculate-working-period.</summary>
    Task<WorkingPeriodDto> CalculateWorkingPeriodAsync(CalculateWorkingPeriodInput input);

    /// <summary>Routed as POST /api/erp/base-calendar/set-date-status.</summary>
    Task SetDateStatusAsync(SetCalendarDateStatusInput input);
}

public interface IBaseCalendarChangeAppService
    : ICrudAppService<BaseCalendarChangeDto, Guid, GetBaseCalendarChangeListInput, CreateUpdateBaseCalendarChangeDto, CreateUpdateBaseCalendarChangeDto> { }
