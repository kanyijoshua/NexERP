using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>
/// Leave types are seeded (ANNUAL 21 days with 5 carried forward, MATERNITY for women only, and
/// more), applications are numbered from the LEAVE series, and working days come from the
/// seeded STANDARD calendar: weekends, 1 January and 25 and 26 December off.
/// </summary>
public class LeaveManagement_Tests : ErpApplicationTestBase
{
    private readonly ILeaveApplicationAppService _leave;
    private readonly IEmployeeAppService _employees;
    private readonly IRepository<EmployeeAbsence, Guid> _absences;

    public LeaveManagement_Tests()
    {
        _leave = GetRequiredService<ILeaveApplicationAppService>();
        _employees = GetRequiredService<IEmployeeAppService>();
        _absences = GetRequiredService<IRepository<EmployeeAbsence, Guid>>();
    }

    [Fact]
    public async Task Leave_Is_Applied_For_Approved_Posted_And_Recalled()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employee = await EmployeeAsync("Grace", EmployeeGender.Female);
            (await _leave.AccrueAsync(new AccrueLeaveInput { LeaveTypeCode = "ANNUAL", LeavePeriodYear = 2026, PostingDate = new DateTime(2026, 1, 1), EmployeeId = employee.Id }))
                .EmployeesProcessed.ShouldBe(1);

            // Five working days from Wednesday 23 December 2026 skip Christmas, Boxing Day and the weekend.
            var application = await _leave.CreateAsync(new CreateUpdateLeaveApplicationDto
            {
                EmployeeId = employee.Id,
                LeaveTypeCode = "annual",
                StartDate = new DateTime(2026, 12, 23),
                DaysApplied = 5,
            });
            application.No.ShouldBe("LV0001");
            application.LeaveTypeCode.ShouldBe("ANNUAL");
            application.EndDate.ShouldBe(new DateTime(2026, 12, 30));
            application.ReturnDate.ShouldBe(new DateTime(2026, 12, 31));
            application.BalanceBefore.ShouldBe(21m);

            // Unapproved leave cannot be posted.
            var early = await Should.ThrowAsync<BusinessException>(() => _leave.RunPostingAsync(application.Id));
            early.Code.ShouldBe(ErpErrorCodes.HumanResources.LeaveApplicationNotReleased);

            await _leave.SendApprovalRequestAsync(application.Id);
            (await _leave.ApproveAsync(application.Id, new ApproveLeaveApplicationInput { Comment = "Enjoy" })).LeaveStatus.ShouldBe(LeaveApplicationStatus.Released);

            var posted = await _leave.RunPostingAsync(application.Id);
            posted.LeaveStatus.ShouldBe(LeaveApplicationStatus.Posted);
            var absence = await _absences.GetAsync(posted.PostedAbsenceId!.Value);
            absence.CauseOfAbsenceCode.ShouldBe("HOLIDAY");
            absence.Quantity.ShouldBe(5m);

            var balance = (await _leave.GetBalancesAsync(new GetLeaveBalancesInput { EmployeeId = employee.Id, LeaveTypeCode = "ANNUAL", LeavePeriodYear = 2026 })).ShouldHaveSingleItem();
            balance.EntitledDays.ShouldBe(21m);
            balance.TakenDays.ShouldBe(5m);
            balance.BalanceDays.ShouldBe(16m);

            // Called back for two days: they go back on the balance and off the absence.
            var tooMany = await Should.ThrowAsync<BusinessException>(() => _leave.RecallAsync(posted.Id, new RecallLeaveApplicationInput { RecallDays = 6 }));
            tooMany.Code.ShouldBe(ErpErrorCodes.HumanResources.InvalidRecallDays);

            var recalled = await _leave.RecallAsync(posted.Id, new RecallLeaveApplicationInput { RecallDays = 2, RecallDate = new DateTime(2026, 12, 29), Reason = "Year-end close" });
            recalled.LeaveStatus.ShouldBe(LeaveApplicationStatus.Recalled);
            recalled.RecalledDays.ShouldBe(2m);
            (await _absences.GetAsync(absence.Id)).Quantity.ShouldBe(3m);

            var after = (await _leave.GetBalancesAsync(new GetLeaveBalancesInput { EmployeeId = employee.Id, LeaveTypeCode = "ANNUAL", LeavePeriodYear = 2026 })).Single();
            after.RecalledDays.ShouldBe(2m);
            after.BalanceDays.ShouldBe(18m);

            var ledger = await _leave.GetLedgerEntriesAsync(new GetLeaveLedgerEntryListInput { EmployeeId = employee.Id, MaxResultCount = 10 });
            ledger.Items.Select(e => e.EntryType).OrderBy(t => t).ShouldBe([LeaveEntryType.Entitlement, LeaveEntryType.Usage, LeaveEntryType.Recall]);
        });
    }

    [Fact]
    public async Task Leave_Beyond_The_Balance_Is_Not_Posted()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employee = await EmployeeAsync("Peter", EmployeeGender.Male);
            await _leave.AccrueAsync(new AccrueLeaveInput { LeaveTypeCode = "SICK", LeavePeriodYear = 2026, EmployeeId = employee.Id });

            var application = await _leave.CreateAsync(new CreateUpdateLeaveApplicationDto
            {
                EmployeeId = employee.Id,
                LeaveTypeCode = "SICK",
                StartDate = new DateTime(2026, 3, 2),
                DaysApplied = 15,
            });
            await _leave.ApproveAsync(application.Id, new ApproveLeaveApplicationInput());

            var refused = await Should.ThrowAsync<BusinessException>(() => _leave.RunPostingAsync(application.Id));
            refused.Code.ShouldBe(ErpErrorCodes.HumanResources.InsufficientLeaveBalance);

            // Approving fewer days than applied for makes it fit.
            await _leave.ReopenAsync(application.Id);
            await _leave.ApproveAsync(application.Id, new ApproveLeaveApplicationInput { DaysApproved = 14 });
            (await _leave.RunPostingAsync(application.Id)).DaysApproved.ShouldBe(14m);
        });
    }

    [Fact]
    public async Task Gender_Restricted_And_Overlapping_Leave_Is_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employee = await EmployeeAsync("James", EmployeeGender.Male);

            var maternity = await Should.ThrowAsync<BusinessException>(() => _leave.CreateAsync(new CreateUpdateLeaveApplicationDto
            {
                EmployeeId = employee.Id,
                LeaveTypeCode = "MATERNITY",
                StartDate = new DateTime(2026, 4, 1),
                DaysApplied = 10,
            }));
            maternity.Code.ShouldBe(ErpErrorCodes.HumanResources.LeaveGenderMismatch);

            await _leave.CreateAsync(new CreateUpdateLeaveApplicationDto { EmployeeId = employee.Id, LeaveTypeCode = "ANNUAL", StartDate = new DateTime(2026, 4, 6), DaysApplied = 5 });
            var overlap = await Should.ThrowAsync<BusinessException>(() => _leave.CreateAsync(new CreateUpdateLeaveApplicationDto
            {
                EmployeeId = employee.Id,
                LeaveTypeCode = "SICK",
                StartDate = new DateTime(2026, 4, 8),
                DaysApplied = 1,
            }));
            overlap.Code.ShouldBe(ErpErrorCodes.HumanResources.LeaveOverlap);
        });
    }

    [Fact]
    public async Task Unused_Leave_Is_Carried_Forward_Up_To_The_Maximum()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employee = await EmployeeAsync("Wanjiru", EmployeeGender.Female);
            await _leave.AccrueAsync(new AccrueLeaveInput { LeaveTypeCode = "ANNUAL", LeavePeriodYear = 2025, PostingDate = new DateTime(2025, 1, 1), EmployeeId = employee.Id });

            await _leave.AccrueAsync(new AccrueLeaveInput { LeaveTypeCode = "ANNUAL", LeavePeriodYear = 2026, PostingDate = new DateTime(2026, 1, 1), EmployeeId = employee.Id });
            // Allocating again does not carry forward twice.
            await _leave.AccrueAsync(new AccrueLeaveInput { LeaveTypeCode = "ANNUAL", LeavePeriodYear = 2026, PostingDate = new DateTime(2026, 1, 1), EmployeeId = employee.Id, DaysOverride = 1 });

            var balance = (await _leave.GetBalancesAsync(new GetLeaveBalancesInput { EmployeeId = employee.Id, LeaveTypeCode = "ANNUAL", LeavePeriodYear = 2026 })).Single();
            balance.CarriedForwardDays.ShouldBe(5m);
            balance.EntitledDays.ShouldBe(22m);
            balance.BalanceDays.ShouldBe(27m);
        });
    }

    private async Task<EmployeeDto> EmployeeAsync(string firstName, EmployeeGender gender) =>
        await _employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = firstName, LastName = "Leave", Gender = gender });
}
