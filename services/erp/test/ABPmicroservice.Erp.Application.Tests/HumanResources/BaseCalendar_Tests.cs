using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>
/// The seeded STANDARD calendar: Saturday and Sunday off every week, 1 January, 25 and 26 December
/// off every year. It is the Human Resources Setup's calendar.
/// </summary>
public class BaseCalendar_Tests : ErpApplicationTestBase
{
    private readonly IBaseCalendarAppService _calendars;
    private readonly IBaseCalendarChangeAppService _changes;
    private readonly IEmployeeAppService _employees;
    private readonly IEmployeeAbsenceAppService _absences;

    public BaseCalendar_Tests()
    {
        _calendars = GetRequiredService<IBaseCalendarAppService>();
        _changes = GetRequiredService<IBaseCalendarChangeAppService>();
        _employees = GetRequiredService<IEmployeeAppService>();
        _absences = GetRequiredService<IEmployeeAbsenceAppService>();
    }

    [Fact]
    public async Task Weekends_And_Annual_Holidays_Are_Nonworking()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Tuesday 22 to Monday 28 December 2026.
            var week = await _calendars.GetDatesAsync(new GetCalendarDatesInput { FromDate = new DateTime(2026, 12, 22), ToDate = new DateTime(2026, 12, 28) });

            week.CalendarCode.ShouldBe("STANDARD");
            week.Dates.Where(d => d.Nonworking).Select(d => d.Date.Day).ShouldBe(new[] { 25, 26, 27 });
            week.Dates.Single(d => d.Date.Day == 25).Description.ShouldBe("Christmas Day");
            week.Dates.Single(d => d.Date.Day == 26).Weekday.ShouldBe(CalendarWeekday.Saturday);
            week.WorkingDays.ShouldBe(4);
        });
    }

    [Fact]
    public async Task A_Leave_Period_Skips_Nonworking_Days_And_Gives_The_Return_Date()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Five working days from Wednesday 23 December 2026: 23, 24, 28, 29, 30; back on the 31st.
            var period = await _calendars.CalculateWorkingPeriodAsync(new CalculateWorkingPeriodInput
            {
                StartDate = new DateTime(2026, 12, 23),
                WorkingDays = 5,
            });

            period.EndDate.ShouldBe(new DateTime(2026, 12, 30));
            period.ReturnDate.ShouldBe(new DateTime(2026, 12, 31));
            period.NonworkingDates.Select(d => d.Date.Day).ShouldBe(new[] { 25, 26, 27 });

            // Starting on a Saturday, the period starts on Monday.
            var fromSaturday = await _calendars.CalculateWorkingPeriodAsync(new CalculateWorkingPeriodInput
            {
                StartDate = new DateTime(2026, 5, 2),
                WorkingDays = 1,
            });
            fromSaturday.StartDate.ShouldBe(new DateTime(2026, 5, 4));
            fromSaturday.ReturnDate.ShouldBe(new DateTime(2026, 5, 5));
        });
    }

    [Fact]
    public async Task One_Date_Overrides_The_Recurring_Changes()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // A Saturday worked, and a public holiday added on a Monday.
            await _calendars.SetDateStatusAsync(new SetCalendarDateStatusInput { CalendarCode = "standard", Date = new DateTime(2026, 6, 6), Nonworking = false });
            await _changes.CreateAsync(new CreateUpdateBaseCalendarChangeDto
            {
                BaseCalendarCode = "STANDARD",
                RecurringSystem = CalendarRecurringSystem.None,
                Date = new DateTime(2026, 6, 1),
                Description = "Madaraka Day",
                Nonworking = true,
            });

            var dates = await _calendars.GetDatesAsync(new GetCalendarDatesInput { CalendarCode = "STANDARD", FromDate = new DateTime(2026, 6, 1), ToDate = new DateTime(2026, 6, 7) });
            dates.Dates.Where(d => d.Nonworking).Select(d => d.Date.Day).ShouldBe(new[] { 1, 7 });

            // Setting a date back to what the weekly change says drops the one-date change.
            await _calendars.SetDateStatusAsync(new SetCalendarDateStatusInput { CalendarCode = "STANDARD", Date = new DateTime(2026, 6, 6), Nonworking = true });
            (await _changes.GetListAsync(new GetBaseCalendarChangeListInput { BaseCalendarCode = "STANDARD", MaxResultCount = 100 }))
                .Items.ShouldNotContain(c => c.Date == new DateTime(2026, 6, 6));
        });
    }

    [Fact]
    public async Task An_Absence_Without_A_Quantity_Counts_The_Employees_Working_Days()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employee = await _employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = "Mary", LastName = "Akinyi" });

            // Monday 21 to Monday 28 December 2026: 21-24 and 28.
            var absence = await _absences.CreateAsync(new CreateUpdateEmployeeAbsenceDto
            {
                EmployeeNo = employee.No,
                FromDate = new DateTime(2026, 12, 21),
                ToDate = new DateTime(2026, 12, 28),
                CauseOfAbsenceCode = "HOLIDAY",
            });
            absence.Quantity.ShouldBe(5m);

            // A shift worker on a calendar without days off counts every day.
            await _calendars.CreateAsync(new Companies.CreateUpdateCodeTableDto { Code = "SHIFT", Description = "Seven-day roster" });
            var input = new CreateUpdateEmployeeDto { FirstName = "Mary", LastName = "Akinyi", BaseCalendarCode = "shift" };
            var updated = await _employees.UpdateAsync(employee.Id, input);
            updated.BaseCalendarCode.ShouldBe("SHIFT");

            var shift = await _absences.CreateAsync(new CreateUpdateEmployeeAbsenceDto
            {
                EmployeeNo = employee.No,
                FromDate = new DateTime(2027, 1, 4),
                ToDate = new DateTime(2027, 1, 10),
            });
            shift.Quantity.ShouldBe(7m);
        });
    }

    [Fact]
    public async Task Changes_Are_Checked()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            (await Should.ThrowAsync<BusinessException>(() => _changes.CreateAsync(new CreateUpdateBaseCalendarChangeDto
            {
                BaseCalendarCode = "STANDARD",
                RecurringSystem = CalendarRecurringSystem.WeeklyRecurring,
            }))).Code.ShouldBe(ErpErrorCodes.HumanResources.CalendarChangeNeedsDay);

            (await Should.ThrowAsync<BusinessException>(() => _changes.CreateAsync(new CreateUpdateBaseCalendarChangeDto
            {
                BaseCalendarCode = "STANDARD",
                RecurringSystem = CalendarRecurringSystem.WeeklyRecurring,
                Day = CalendarWeekday.Sunday,
            }))).Code.ShouldBe(ErpErrorCodes.BaseTables.RecordAlreadyExists);

            (await Should.ThrowAsync<BusinessException>(() => _calendars.GetDatesAsync(new GetCalendarDatesInput
            {
                CalendarCode = "NOPE",
                FromDate = new DateTime(2026, 1, 1),
                ToDate = new DateTime(2026, 1, 2),
            }))).Code.ShouldBe(ErpErrorCodes.HumanResources.BaseCalendarNotFound);
        });
    }
}
