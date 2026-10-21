using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>
/// Base Calendar: the working and nonworking days of a company or an employee. Every day is a
/// working day unless one of the calendar's changes says otherwise.
/// </summary>
public class BaseCalendar : CodeTableEntity
{
    protected BaseCalendar() { }

    public BaseCalendar(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>
/// Base Calendar Change: one date, a date every year, or a weekday every week, and whether it is
/// a nonworking day. A change can also make a day working again, e.g. a Saturday worked once.
/// </summary>
public class BaseCalendarChange : CompanyEntity
{
    public string BaseCalendarCode { get; private set; }
    public CalendarRecurringSystem RecurringSystem { get; private set; }

    /// <summary>The date, or for an annual change the day and month; blank for a weekly change.</summary>
    public DateTime? Date { get; private set; }

    /// <summary>The weekday of a weekly change; for the others, the weekday of the date.</summary>
    public CalendarWeekday Day { get; private set; }

    public string Description { get; private set; }
    public bool Nonworking { get; private set; }

    protected BaseCalendarChange() { }

    public BaseCalendarChange(Guid id, string baseCalendarCode)
        : base(id)
    {
        BaseCalendarCode = CodeTableEntity.NormalizeCode(
            Check.NotNullOrWhiteSpace(baseCalendarCode, nameof(baseCalendarCode), ErpDomainConsts.MaxCodeLength));
    }

    public void Set(CalendarRecurringSystem recurringSystem, DateTime? date, CalendarWeekday day, string description, bool nonworking)
    {
        if (recurringSystem == CalendarRecurringSystem.WeeklyRecurring)
        {
            if (day == CalendarWeekday.None)
            {
                throw new BusinessException(ErpErrorCodes.HumanResources.CalendarChangeNeedsDay);
            }

            Date = null;
            Day = day;
        }
        else
        {
            if (!date.HasValue)
            {
                throw new BusinessException(ErpErrorCodes.HumanResources.CalendarChangeNeedsDate);
            }

            Date = date.Value.Date;
            Day = BaseCalendarManager.WeekdayOf(Date.Value);
        }

        RecurringSystem = recurringSystem;
        Description = Check.Length(description?.Trim(), nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Nonworking = nonworking;
    }

    /// <summary>Whether this change is about the given date.</summary>
    public bool Applies(DateTime date) => RecurringSystem switch
    {
        CalendarRecurringSystem.WeeklyRecurring => Day == BaseCalendarManager.WeekdayOf(date),
        CalendarRecurringSystem.AnnualRecurring => Date.HasValue && Date.Value.Month == date.Month && Date.Value.Day == date.Day,
        _ => Date.HasValue && Date.Value.Date == date.Date,
    };
}

/// <summary>One day of a calendar, as the calendar makes it.</summary>
public class CalendarDate
{
    public DateTime Date { get; init; }
    public CalendarWeekday Weekday { get; init; }
    public bool Nonworking { get; init; }
    public string Description { get; init; }
}

/// <summary>Where a period of working days ends and when the employee is back.</summary>
public class WorkingPeriod
{
    /// <summary>The first working day on or after the start date.</summary>
    public DateTime StartDate { get; init; }

    /// <summary>The last working day of the period.</summary>
    public DateTime EndDate { get; init; }

    /// <summary>The first working day after the period.</summary>
    public DateTime ReturnDate { get; init; }

    public int WorkingDays { get; init; }

    /// <summary>The nonworking days the period passes over.</summary>
    public IReadOnlyList<CalendarDate> NonworkingDates { get; init; }
}

/// <summary>
/// Reads calendars: whether a date is a working day, the days of a range, working days between two
/// dates, and where a number of working days from a start date ends (leave end and return dates).
/// A one-date change wins over an annual one, which wins over a weekly one.
/// </summary>
public class BaseCalendarManager : DomainService
{
    // A calendar where every day is off would never end a period.
    private const int MaxDaysScanned = 3660;

    private readonly IRepository<BaseCalendarChange, Guid> _changeRepository;
    private readonly IRepository<BaseCalendar, Guid> _calendarRepository;
    private readonly HumanResourcesSetupManager _setupManager;

    public BaseCalendarManager(
        IRepository<BaseCalendarChange, Guid> changeRepository,
        IRepository<BaseCalendar, Guid> calendarRepository,
        HumanResourcesSetupManager setupManager
    )
    {
        _changeRepository = changeRepository;
        _calendarRepository = calendarRepository;
        _setupManager = setupManager;
    }

    public static CalendarWeekday WeekdayOf(DateTime date) =>
        date.DayOfWeek == DayOfWeek.Sunday ? CalendarWeekday.Sunday : (CalendarWeekday)(int)date.DayOfWeek;

    /// <summary>The employee's own calendar, else the Human Resources Setup's; null when neither is set.</summary>
    public async Task<string> ResolveCalendarCodeAsync(Employee employee = null)
    {
        return CodeTableEntity.NormalizeCode(employee?.BaseCalendarCode)
            ?? CodeTableEntity.NormalizeCode((await _setupManager.GetAsync()).BaseCalendarCode);
    }

    public async Task<IReadOnlyList<CalendarDate>> GetDatesAsync(string calendarCode, DateTime fromDate, DateTime toDate)
    {
        if (toDate.Date < fromDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.InvalidAbsencePeriod);
        }

        var changes = await GetChangesAsync(calendarCode);
        var dates = new List<CalendarDate>();
        for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        {
            dates.Add(Evaluate(changes, date));
        }

        return dates;
    }

    public async Task<bool> IsNonworkingAsync(string calendarCode, DateTime date)
    {
        return Evaluate(await GetChangesAsync(calendarCode), date.Date).Nonworking;
    }

    public async Task<int> CountWorkingDaysAsync(string calendarCode, DateTime fromDate, DateTime toDate)
    {
        return (await GetDatesAsync(calendarCode, fromDate, toDate)).Count(d => !d.Nonworking);
    }

    /// <summary>
    /// A period of <paramref name="workingDays"/> working days from <paramref name="startDate"/>: it
    /// starts on the first working day on or after it and skips the calendar's nonworking days.
    /// </summary>
    public async Task<WorkingPeriod> CalculatePeriodAsync(string calendarCode, DateTime startDate, int workingDays)
    {
        if (workingDays < 1)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.WorkingDaysMustBePositive);
        }

        var changes = await GetChangesAsync(calendarCode);
        var skipped = new List<CalendarDate>();
        DateTime? first = null;
        var end = startDate.Date;
        var counted = 0;
        var date = startDate.Date;

        for (var scanned = 0; ; scanned++, date = date.AddDays(1))
        {
            if (scanned > MaxDaysScanned)
            {
                throw new BusinessException(ErpErrorCodes.HumanResources.CalendarHasNoWorkingDays).WithData("code", calendarCode);
            }

            var day = Evaluate(changes, date);
            if (day.Nonworking)
            {
                // Days off before the period starts are not part of it.
                if (first.HasValue && counted < workingDays)
                {
                    skipped.Add(day);
                }

                continue;
            }

            if (counted == workingDays)
            {
                return new WorkingPeriod
                {
                    StartDate = first!.Value,
                    EndDate = end,
                    ReturnDate = date,
                    WorkingDays = workingDays,
                    NonworkingDates = skipped,
                };
            }

            first ??= date;
            end = date;
            counted++;
        }
    }

    private async Task<List<BaseCalendarChange>> GetChangesAsync(string calendarCode)
    {
        var code = CodeTableEntity.NormalizeCode(calendarCode);
        if (code == null)
        {
            return new List<BaseCalendarChange>();
        }

        if (!await _calendarRepository.AnyAsync(c => c.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.BaseCalendarNotFound).WithData("code", code);
        }

        return (await _changeRepository.GetListAsync(c => c.BaseCalendarCode == code))
            .OrderBy(c => c.RecurringSystem)
            .ToList();
    }

    private static CalendarDate Evaluate(IEnumerable<BaseCalendarChange> changes, DateTime date)
    {
        // Ordered one-date, annual, weekly: the most specific change decides.
        var change = changes.FirstOrDefault(c => c.Applies(date));
        return new CalendarDate
        {
            Date = date,
            Weekday = WeekdayOf(date),
            Nonworking = change?.Nonworking ?? false,
            Description = change?.Description,
        };
    }
}
