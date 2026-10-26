using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Workflows;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.HumanResources;

[Authorize(ErpPermissions.HumanResourcesSetup.Default)]
public class LeaveTypeAppService
    : HumanResourcesCodeTableAppService<LeaveType, LeaveTypeDto, CreateUpdateLeaveTypeDto>,
        ILeaveTypeAppService
{
    public LeaveTypeAppService(IRepository<LeaveType, Guid> repository)
        : base(repository) { }

    protected override LeaveType NewEntity(Guid id, CreateUpdateLeaveTypeDto input) =>
        new(
            id,
            input.Code,
            input.Description,
            input.DefaultEntitlementDays,
            input.MaxCarryForwardDays,
            input.AccrualFrequency,
            input.CauseOfAbsenceCode,
            input.Paid,
            input.RequiresApproval,
            input.GenderRestriction
        );

    protected override async Task ApplyAsync(LeaveType entity, CreateUpdateLeaveTypeDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<CauseOfAbsence>(input.CauseOfAbsenceCode);
        entity.SetDetails(
            input.DefaultEntitlementDays,
            input.MaxCarryForwardDays,
            input.AccrualFrequency,
            input.CauseOfAbsenceCode,
            input.Paid,
            input.RequiresApproval,
            input.GenderRestriction
        );
    }
}

[Authorize(ErpPermissions.LeaveManagement.Default)]
public class LeaveApplicationAppService
    : ErpCrudAppService<
        LeaveApplication,
        LeaveApplicationDto,
        Guid,
        GetLeaveApplicationListInput,
        CreateUpdateLeaveApplicationDto,
        CreateUpdateLeaveApplicationDto
    >,
        ILeaveApplicationAppService
{
    private readonly IRepository<Employee, Guid> _employeeRepository;
    private readonly IRepository<LeaveType, Guid> _leaveTypeRepository;
    private readonly IRepository<LeaveLedgerEntry, Guid> _ledgerRepository;
    private readonly LeaveManager _leaveManager;
    private readonly ApprovalsManager _approvalsManager;

    private const ApprovalDocumentKind ApprovalKind = ApprovalDocumentKind.LeaveApplication;

    public LeaveApplicationAppService(
        IRepository<LeaveApplication, Guid> repository,
        IRepository<Employee, Guid> employeeRepository,
        IRepository<LeaveType, Guid> leaveTypeRepository,
        IRepository<LeaveLedgerEntry, Guid> ledgerRepository,
        LeaveManager leaveManager,
        ApprovalsManager approvalsManager
    )
        : base(repository)
    {
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _ledgerRepository = ledgerRepository;
        _leaveManager = leaveManager;
        _approvalsManager = approvalsManager;

        GetPolicyName = ErpPermissions.LeaveManagement.Default;
        GetListPolicyName = ErpPermissions.LeaveManagement.Default;
        CreatePolicyName = ErpPermissions.LeaveManagement.Create;
        UpdatePolicyName = ErpPermissions.LeaveManagement.Update;
        DeletePolicyName = ErpPermissions.LeaveManagement.Delete;
    }

    public override async Task<LeaveApplicationDto> CreateAsync(CreateUpdateLeaveApplicationDto input)
    {
        await CheckCreatePolicyAsync();

        var employee = await GetActiveEmployeeAsync(input.EmployeeId);
        var leaveType = await _leaveManager.GetLeaveTypeAsync(input.LeaveTypeCode);
        LeaveManager.EnsureGenderAllowed(employee, leaveType);

        var (startDate, endDate, returnDate, daysApplied) = await ResolvePeriodAsync(employee, input);
        await EnsureNoOverlapAsync(employee.Id, startDate, endDate, null);

        var balance = await _leaveManager.GetBalanceAsync(employee.Id, leaveType.Code, startDate.Year);
        var no = await ResolveNoAsync(input.No, startDate);

        var application = new LeaveApplication(
            GuidGenerator.Create(),
            no,
            employee.Id,
            employee.No,
            employee.FullName,
            leaveType.Code,
            input.ApplicationDate ?? Clock.Now.Date,
            startDate,
            endDate,
            returnDate,
            daysApplied,
            balance,
            input.Reason,
            input.RelieverEmployeeNo
        );

        await Repository.InsertAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    public override async Task<LeaveApplicationDto> UpdateAsync(Guid id, CreateUpdateLeaveApplicationDto input)
    {
        await CheckUpdatePolicyAsync();

        var application = await GetEntityByIdAsync(id);
        var employee = await GetActiveEmployeeAsync(input.EmployeeId);
        var leaveType = await _leaveManager.GetLeaveTypeAsync(input.LeaveTypeCode);
        LeaveManager.EnsureGenderAllowed(employee, leaveType);

        var (startDate, endDate, returnDate, daysApplied) = await ResolvePeriodAsync(employee, input);
        await EnsureNoOverlapAsync(employee.Id, startDate, endDate, id);

        var balance = await _leaveManager.GetBalanceAsync(employee.Id, leaveType.Code, startDate.Year);

        application.SetDetails(
            employee.Id,
            employee.No,
            employee.FullName,
            leaveType.Code,
            input.ApplicationDate ?? application.ApplicationDate,
            startDate,
            endDate,
            returnDate,
            daysApplied,
            balance,
            input.Reason,
            input.RelieverEmployeeNo
        );

        await Repository.UpdateAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var application = await GetEntityByIdAsync(id);
        if (application.LeaveStatus is LeaveApplicationStatus.Posted or LeaveApplicationStatus.Recalled)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentAlreadyPosted).WithData("documentNo", application.No);
        }

        await Repository.DeleteAsync(application, autoSave: true);
    }

    public async Task<WorkingPeriodDto> CalculateDatesAsync(CalculateLeaveDatesInput input)
    {
        var employee = await _employeeRepository.GetAsync(input.EmployeeId);
        var period = await _leaveManager.CalculateDatesAsync(employee, input.StartDate, input.DaysApplied);
        return new WorkingPeriodDto
        {
            CalendarCode = await LazyServiceProvider.LazyGetRequiredService<BaseCalendarManager>().ResolveCalendarCodeAsync(employee),
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            ReturnDate = period.ReturnDate,
            WorkingDays = period.WorkingDays,
            NonworkingDates = period.NonworkingDates.Select(BaseCalendarAppService.Map).ToList(),
        };
    }

    [Authorize(ErpPermissions.LeaveManagement.Update)]
    public async Task<ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id)
    {
        var app = await GetEntityByIdAsync(id);
        var workflow = await _approvalsManager.FindWorkflowAsync(ApprovalKind, app.ApprovalAmount, app);
        if (workflow == null)
        {
            // When no workflow is enabled for LeaveApplication, move to PendingApproval for direct manager/HR approval.
            app.SendForApproval();
            await Repository.UpdateAsync(app, autoSave: true);
            return new ApprovalRequestResultDto
            {
                AutoApproved = false,
                ApproverCount = 1,
                FirstApproverUserName = "HR / Manager",
            };
        }

        var result = await _approvalsManager.SendApprovalRequestAsync(ApprovalKind, id, GetUserId());
        return new ApprovalRequestResultDto
        {
            AutoApproved = result.AutoApproved,
            ApproverCount = result.ApproverCount,
            FirstApproverUserName = result.FirstApproverUserName,
        };
    }

    [Authorize(ErpPermissions.LeaveManagement.Update)]
    public async Task CancelApprovalRequestAsync(Guid id)
    {
        var app = await GetEntityByIdAsync(id);
        if (app.LeaveStatus == LeaveApplicationStatus.PendingApproval)
        {
            app.Reopen();
            await Repository.UpdateAsync(app, autoSave: true);
        }
    }

    [Authorize(ErpPermissions.LeaveManagement.Approve)]
    public async Task<LeaveApplicationDto> ApproveAsync(Guid id, ApproveLeaveApplicationInput input)
    {
        var app = await GetEntityByIdAsync(id);
        app.Approve(input?.DaysApproved, input?.Comment);
        await Repository.UpdateAsync(app, autoSave: true);
        return await MapToGetOutputDtoAsync(app);
    }

    [Authorize(ErpPermissions.LeaveManagement.Approve)]
    public async Task<LeaveApplicationDto> RejectAsync(Guid id, RejectLeaveApplicationInput input)
    {
        var app = await GetEntityByIdAsync(id);
        app.Reject(input?.Comment);
        await Repository.UpdateAsync(app, autoSave: true);
        return await MapToGetOutputDtoAsync(app);
    }

    [Authorize(ErpPermissions.LeaveManagement.Update)]
    public async Task<LeaveApplicationDto> ReopenAsync(Guid id)
    {
        var app = await GetEntityByIdAsync(id);
        app.Reopen();
        await Repository.UpdateAsync(app, autoSave: true);
        return await MapToGetOutputDtoAsync(app);
    }

    [Authorize(ErpPermissions.LeaveManagement.Post)]
    public async Task<LeaveApplicationDto> RunPostingAsync(Guid id)
    {
        var app = await _leaveManager.PostApplicationAsync(id);
        return await MapToGetOutputDtoAsync(app);
    }

    [Authorize(ErpPermissions.LeaveManagement.Post)]
    public async Task<LeaveApplicationDto> RecallAsync(Guid id, RecallLeaveApplicationInput input)
    {
        var app = await _leaveManager.RecallApplicationAsync(
            id,
            input.RecallDays,
            input.RecallDate ?? Clock.Now.Date,
            input.Reason
        );
        return await MapToGetOutputDtoAsync(app);
    }

    [Authorize(ErpPermissions.LeaveManagement.Post)]
    public async Task<AccrueLeaveResultDto> AccrueAsync(AccrueLeaveInput input)
    {
        var year = input.LeavePeriodYear > 0 ? input.LeavePeriodYear : Clock.Now.Year;
        var processed = await _leaveManager.AccrueLeaveAsync(
            input.LeaveTypeCode,
            year,
            input.PostingDate ?? Clock.Now.Date,
            input.EmployeeId,
            input.DaysOverride,
            input.CarryForwardFromPreviousYear
        );

        return new AccrueLeaveResultDto { EmployeesProcessed = processed };
    }

    public async Task<List<LeaveBalanceDto>> GetBalancesAsync(GetLeaveBalancesInput input)
    {
        input ??= new GetLeaveBalancesInput();
        var year = input.LeavePeriodYear ?? Clock.Now.Year;
        var leaveTypeFilter = CodeTableEntity.NormalizeCode(input.LeaveTypeCode);

        var employees = input.EmployeeId.HasValue
            ? new List<Employee> { await _employeeRepository.GetAsync(input.EmployeeId.Value) }
            : (await _employeeRepository.GetListAsync()).OrderBy(e => e.No).ToList();

        var leaveTypes = (await _leaveTypeRepository.GetListAsync(t => leaveTypeFilter == null || t.Code == leaveTypeFilter))
            .OrderBy(t => t.Code)
            .ToList();

        var entries = await _ledgerRepository.GetListAsync(e =>
            e.LeavePeriodYear == year
            && (!input.EmployeeId.HasValue || e.EmployeeId == input.EmployeeId.Value)
            && (leaveTypeFilter == null || e.LeaveTypeCode == leaveTypeFilter));

        var result = new List<LeaveBalanceDto>();
        foreach (var emp in employees)
        {
            foreach (var lt in leaveTypes)
            {
                if (lt.GenderRestriction.HasValue && emp.Gender != lt.GenderRestriction.Value)
                {
                    continue;
                }

                var cell = entries.Where(e => e.EmployeeId == emp.Id && e.LeaveTypeCode == lt.Code).ToList();
                var entitled = cell.Where(e => e.EntryType == LeaveEntryType.Entitlement).Sum(e => e.Days);
                var accrued = cell.Where(e => e.EntryType == LeaveEntryType.Accrual).Sum(e => e.Days);
                var carried = cell.Where(e => e.EntryType == LeaveEntryType.CarryOver).Sum(e => e.Days);
                var taken = Math.Abs(cell.Where(e => e.EntryType == LeaveEntryType.Usage).Sum(e => e.Days));
                var recalled = cell.Where(e => e.EntryType == LeaveEntryType.Recall).Sum(e => e.Days);
                var adjusted = cell.Where(e => e.EntryType == LeaveEntryType.Adjustment).Sum(e => e.Days);
                var balance = cell.Sum(e => e.Days);

                result.Add(new LeaveBalanceDto
                {
                    EmployeeId = emp.Id,
                    EmployeeNo = emp.No,
                    EmployeeName = emp.FullName,
                    LeaveTypeCode = lt.Code,
                    LeaveTypeDescription = lt.Description,
                    LeavePeriodYear = year,
                    EntitledDays = entitled,
                    AccruedDays = accrued,
                    CarriedForwardDays = carried,
                    TakenDays = taken,
                    RecalledDays = recalled,
                    AdjustedDays = adjusted,
                    BalanceDays = balance,
                });
            }
        }

        return result;
    }

    public async Task<PagedResultDto<LeaveLedgerEntryDto>> GetLedgerEntriesAsync(GetLeaveLedgerEntryListInput input)
    {
        var query = await _ledgerRepository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        var leaveType = CodeTableEntity.NormalizeCode(input.LeaveTypeCode);

        query = query
            .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId.Value)
            .WhereIf(!input.EmployeeNo.IsNullOrWhiteSpace(), x => x.EmployeeNo == input.EmployeeNo.Trim())
            .WhereIf(leaveType != null, x => x.LeaveTypeCode == leaveType)
            .WhereIf(input.LeavePeriodYear.HasValue, x => x.LeavePeriodYear == input.LeavePeriodYear.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EmployeeNo.ToLower().Contains(filter)
                    || (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(filter))
                    || x.LeaveTypeCode.ToLower().Contains(filter)
                    || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.EntryNo)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount > 0 ? input.MaxResultCount : 50)
        );

        return new PagedResultDto<LeaveLedgerEntryDto>(
            totalCount,
            ObjectMapper.Map<List<LeaveLedgerEntry>, List<LeaveLedgerEntryDto>>(items)
        );
    }

    protected override async Task<IQueryable<LeaveApplication>> CreateFilteredQueryAsync(GetLeaveApplicationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var leaveType = CodeTableEntity.NormalizeCode(input.LeaveTypeCode);

        return query
            .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId.Value)
            .WhereIf(leaveType != null, x => x.LeaveTypeCode == leaveType)
            .WhereIf(input.LeaveStatus.HasValue, x => x.LeaveStatus == input.LeaveStatus.Value)
            .WhereIf(input.LeavePeriodYear.HasValue, x => x.LeavePeriodYear == input.LeavePeriodYear.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.EmployeeNo.ToLower().Contains(filter)
                    || (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(filter))
                    || x.LeaveTypeCode.ToLower().Contains(filter)
                    || (x.Reason != null && x.Reason.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<LeaveApplication> ApplyDefaultSorting(IQueryable<LeaveApplication> query) =>
        query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.No);

    private async Task<Employee> GetActiveEmployeeAsync(Guid employeeId)
    {
        var employee = await _employeeRepository.GetAsync(employeeId);
        if (employee.Status != EmployeeStatus.Active)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.EmployeeBlocked).WithData("no", employee.No);
        }

        return employee;
    }

    private async Task<(DateTime StartDate, DateTime EndDate, DateTime ReturnDate, decimal DaysApplied)> ResolvePeriodAsync(
        Employee employee,
        CreateUpdateLeaveApplicationDto input
    )
    {
        var days = input.DaysApplied;
        if (days > 0m)
        {
            var period = await _leaveManager.CalculateDatesAsync(employee, input.StartDate, days);
            return (
                period.StartDate,
                input.EndDate?.Date ?? period.EndDate,
                input.ReturnDate?.Date ?? period.ReturnDate,
                days
            );
        }

        var end = input.EndDate?.Date ?? input.StartDate.Date;
        var calendarManager = LazyServiceProvider.LazyGetRequiredService<BaseCalendarManager>();
        var calendarCode = await calendarManager.ResolveCalendarCodeAsync(employee);
        var counted = await calendarManager.CountWorkingDaysAsync(calendarCode, input.StartDate, end);
        var computedDays = Math.Max(1, counted);
        var calcPeriod = await _leaveManager.CalculateDatesAsync(employee, input.StartDate, computedDays);
        return (input.StartDate.Date, end, input.ReturnDate?.Date ?? calcPeriod.ReturnDate, computedDays);
    }

    private async Task EnsureNoOverlapAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid? exceptId)
    {
        var existing = await Repository.GetListAsync(a =>
            a.EmployeeId == employeeId
            && a.Id != exceptId
            && a.LeaveStatus != LeaveApplicationStatus.Rejected);

        var overlap = existing.FirstOrDefault(a => startDate.Date <= a.EndDate.Date && endDate.Date >= a.StartDate.Date);
        if (overlap != null)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveOverlap)
                .WithData("no", overlap.No)
                .WithData("startDate", overlap.StartDate.ToString("yyyy-MM-dd"))
                .WithData("endDate", overlap.EndDate.ToString("yyyy-MM-dd"));
        }
    }

    /// <summary>
    /// The next number of HR Setup's leave application series; a typed number needs a series that
    /// allows manual numbers. A company without a series gets LV-0001, LV-0002 and so on.
    /// </summary>
    private async Task<string> ResolveNoAsync(string requestedNo, DateTime usageDate)
    {
        var setup = await LazyServiceProvider.LazyGetRequiredService<HumanResourcesSetupManager>().GetAsync();
        string no;
        if (!setup.LeaveApplicationNos.IsNullOrWhiteSpace())
        {
            no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup.LeaveApplicationNos, requestedNo, usageDate);
        }
        else if (!requestedNo.IsNullOrWhiteSpace())
        {
            no = requestedNo.Trim().ToUpperInvariant();
        }
        else
        {
            var next = await Repository.CountAsync() + 1;
            while (await Repository.AnyAsync(x => x.No == $"LV-{next:D4}"))
            {
                next++;
            }

            return $"LV-{next:D4}";
        }

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        return no;
    }

    private Guid GetUserId() => CurrentUser.Id ?? throw new AbpAuthorizationException();
}
