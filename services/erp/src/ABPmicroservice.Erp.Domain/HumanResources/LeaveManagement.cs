using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>
/// Leave Type: defines a category of leave (Annual, Sick, Maternity, Paternity, Study, Compassionate),
/// its default annual entitlement, maximum carry-forward days, accrual frequency, and the Cause of Absence
/// it posts to when taken.
/// </summary>
public class LeaveType : CodeTableEntity
{
    public string CauseOfAbsenceCode { get; private set; }
    public decimal DefaultEntitlementDays { get; private set; }
    public decimal MaxCarryForwardDays { get; private set; }
    public LeaveAccrualFrequency AccrualFrequency { get; private set; }
    public bool Paid { get; private set; } = true;
    public bool RequiresApproval { get; private set; } = true;
    public EmployeeGender? GenderRestriction { get; private set; }

    protected LeaveType() { }

    public LeaveType(
        Guid id,
        string code,
        string description,
        decimal defaultEntitlementDays = 21m,
        decimal maxCarryForwardDays = 0m,
        LeaveAccrualFrequency accrualFrequency = LeaveAccrualFrequency.Annual,
        string causeOfAbsenceCode = null,
        bool paid = true,
        bool requiresApproval = true,
        EmployeeGender? genderRestriction = null
    )
        : base(id, code, description)
    {
        SetDetails(
            defaultEntitlementDays,
            maxCarryForwardDays,
            accrualFrequency,
            causeOfAbsenceCode,
            paid,
            requiresApproval,
            genderRestriction
        );
    }

    public void SetDetails(
        decimal defaultEntitlementDays,
        decimal maxCarryForwardDays,
        LeaveAccrualFrequency accrualFrequency,
        string causeOfAbsenceCode,
        bool paid,
        bool requiresApproval,
        EmployeeGender? genderRestriction
    )
    {
        if (defaultEntitlementDays < 0m)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.NegativeLeaveDays).WithData("value", defaultEntitlementDays);
        }

        if (maxCarryForwardDays < 0m)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.NegativeLeaveDays).WithData("value", maxCarryForwardDays);
        }

        DefaultEntitlementDays = defaultEntitlementDays;
        MaxCarryForwardDays = maxCarryForwardDays;
        AccrualFrequency = accrualFrequency;
        CauseOfAbsenceCode = NormalizeCode(Check.Length(causeOfAbsenceCode, nameof(causeOfAbsenceCode), ErpDomainConsts.MaxCodeLength));
        Paid = paid;
        RequiresApproval = requiresApproval;
        GenderRestriction = genderRestriction == EmployeeGender.None ? null : genderRestriction;
    }
}

/// <summary>
/// Leave Ledger Entry: append-only ledger tracking every leave entitlement, monthly accrual,
/// carry-over, usage (application posted), recall, and adjustment for an employee.
/// </summary>
public class LeaveLedgerEntry : LedgerEntryBase
{
    public Guid EmployeeId { get; private set; }
    public string EmployeeNo { get; private set; }
    public string EmployeeName { get; private set; }
    public string LeaveTypeCode { get; private set; }
    public int LeavePeriodYear { get; private set; }
    public DateTime PostingDate { get; private set; }
    public LeaveEntryType EntryType { get; private set; }
    public string DocumentNo { get; private set; }
    public DateTime? FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }

    /// <summary>Positive for entitlements, accruals, carry-overs and recalls; negative for usage.</summary>
    public decimal Days { get; private set; }

    public string Description { get; private set; }

    protected LeaveLedgerEntry() { }

    public LeaveLedgerEntry(
        Guid id,
        Guid employeeId,
        string employeeNo,
        string employeeName,
        string leaveTypeCode,
        int leavePeriodYear,
        DateTime postingDate,
        LeaveEntryType entryType,
        string documentNo,
        decimal days,
        string description = null,
        DateTime? fromDate = null,
        DateTime? toDate = null
    )
        : base(id)
    {
        EmployeeId = employeeId;
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), ErpDomainConsts.MaxNoLength);
        EmployeeName = Check.Length(employeeName, nameof(employeeName), ErpDomainConsts.MaxNameLength);
        LeaveTypeCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(leaveTypeCode, nameof(leaveTypeCode), ErpDomainConsts.MaxCodeLength));
        LeavePeriodYear = leavePeriodYear;
        PostingDate = postingDate.Date;
        EntryType = entryType;
        DocumentNo = Check.Length(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Days = days;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        FromDate = fromDate?.Date;
        ToDate = toDate?.Date;
    }
}

/// <summary>
/// Leave Application: an employee's request for leave over a working period, supporting workflow
/// approval, posting to the Leave Ledger and Employee Absences, and post-approval recall.
/// </summary>
public class LeaveApplication : CompanyAggregateRoot, IApprovalDocument, IHasNo
{
    public string No { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string EmployeeNo { get; private set; }
    public string EmployeeName { get; private set; }
    public string LeaveTypeCode { get; private set; }
    public int LeavePeriodYear { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime ReturnDate { get; private set; }
    public decimal DaysApplied { get; private set; }
    public decimal DaysApproved { get; private set; }
    public decimal BalanceBefore { get; private set; }
    public string Reason { get; private set; }
    public string RelieverEmployeeNo { get; private set; }
    public string ApproverComment { get; private set; }
    public LeaveApplicationStatus LeaveStatus { get; private set; }
    public Guid? PostedAbsenceId { get; private set; }
    public decimal RecalledDays { get; private set; }
    public DateTime? RecallDate { get; private set; }
    public string RecallReason { get; private set; }

    // IApprovalDocument implementation
    public DocumentStatus Status => LeaveStatus switch
    {
        LeaveApplicationStatus.PendingApproval => DocumentStatus.PendingApproval,
        LeaveApplicationStatus.Released or LeaveApplicationStatus.Posted or LeaveApplicationStatus.Recalled => DocumentStatus.Released,
        _ => DocumentStatus.Open,
    };

    public decimal ApprovalAmount => DaysApplied;
    public bool HasLines => DaysApplied > 0m;

    protected LeaveApplication() { }

    public LeaveApplication(
        Guid id,
        string no,
        Guid employeeId,
        string employeeNo,
        string employeeName,
        string leaveTypeCode,
        DateTime applicationDate,
        DateTime startDate,
        DateTime endDate,
        DateTime returnDate,
        decimal daysApplied,
        decimal balanceBefore,
        string reason = null,
        string relieverEmployeeNo = null
    )
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        LeaveStatus = LeaveApplicationStatus.Open;
        SetDetails(
            employeeId,
            employeeNo,
            employeeName,
            leaveTypeCode,
            applicationDate,
            startDate,
            endDate,
            returnDate,
            daysApplied,
            balanceBefore,
            reason,
            relieverEmployeeNo
        );
    }

    public void SetDetails(
        Guid employeeId,
        string employeeNo,
        string employeeName,
        string leaveTypeCode,
        DateTime applicationDate,
        DateTime startDate,
        DateTime endDate,
        DateTime returnDate,
        decimal daysApplied,
        decimal balanceBefore,
        string reason = null,
        string relieverEmployeeNo = null
    )
    {
        if (LeaveStatus is not (LeaveApplicationStatus.Open or LeaveApplicationStatus.Rejected))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveApplicationNotOpen).WithData("no", No);
        }

        if (daysApplied <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.WorkingDaysMustBePositive);
        }

        if (endDate.Date < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.InvalidAbsencePeriod);
        }

        EmployeeId = employeeId;
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), ErpDomainConsts.MaxNoLength);
        EmployeeName = Check.Length(employeeName, nameof(employeeName), ErpDomainConsts.MaxNameLength);
        LeaveTypeCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(leaveTypeCode, nameof(leaveTypeCode), ErpDomainConsts.MaxCodeLength));
        ApplicationDate = applicationDate.Date;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        ReturnDate = returnDate.Date < endDate.Date ? endDate.Date.AddDays(1) : returnDate.Date;
        LeavePeriodYear = StartDate.Year;
        DaysApplied = daysApplied;
        DaysApproved = daysApplied;
        BalanceBefore = balanceBefore;
        Reason = Check.Length(reason?.Trim(), nameof(reason), ErpDomainConsts.MaxDescriptionLength);
        RelieverEmployeeNo = CodeTableEntity.NormalizeCode(Check.Length(relieverEmployeeNo, nameof(relieverEmployeeNo), ErpDomainConsts.MaxNoLength));
        LeaveStatus = LeaveApplicationStatus.Open;
    }

    public void SendForApproval()
    {
        if (LeaveStatus != LeaveApplicationStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveApplicationNotOpen).WithData("no", No);
        }

        LeaveStatus = LeaveApplicationStatus.PendingApproval;
    }

    public void Release()
    {
        Approve(DaysApplied, ApproverComment);
    }

    public void Approve(decimal? daysApproved = null, string comment = null)
    {
        if (LeaveStatus is not (LeaveApplicationStatus.Open or LeaveApplicationStatus.PendingApproval))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveApplicationNotOpen).WithData("no", No);
        }

        var approved = daysApproved ?? DaysApplied;
        if (approved <= 0m || approved > DaysApplied)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.WorkingDaysMustBePositive);
        }

        DaysApproved = approved;
        if (!comment.IsNullOrWhiteSpace())
        {
            ApproverComment = Check.Length(comment.Trim(), nameof(comment), ErpDomainConsts.MaxDescriptionLength);
        }

        LeaveStatus = LeaveApplicationStatus.Released;
    }

    public void Reject(string comment = null)
    {
        if (LeaveStatus is not (LeaveApplicationStatus.Open or LeaveApplicationStatus.PendingApproval))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveApplicationNotOpen).WithData("no", No);
        }

        if (!comment.IsNullOrWhiteSpace())
        {
            ApproverComment = Check.Length(comment.Trim(), nameof(comment), ErpDomainConsts.MaxDescriptionLength);
        }

        LeaveStatus = LeaveApplicationStatus.Rejected;
    }

    public void Reopen()
    {
        if (LeaveStatus is LeaveApplicationStatus.Posted or LeaveApplicationStatus.Recalled)
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentAlreadyPosted).WithData("documentNo", No);
        }

        LeaveStatus = LeaveApplicationStatus.Open;
    }

    internal void MarkPosted(Guid? absenceId)
    {
        if (LeaveStatus != LeaveApplicationStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveApplicationNotReleased).WithData("no", No);
        }

        PostedAbsenceId = absenceId;
        LeaveStatus = LeaveApplicationStatus.Posted;
    }

    internal void RecordRecall(decimal recallDays, DateTime recallDate, string reason)
    {
        if (LeaveStatus is not (LeaveApplicationStatus.Posted or LeaveApplicationStatus.Recalled))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveApplicationNotPosted).WithData("no", No);
        }

        var remainingTaken = DaysApproved - RecalledDays;
        if (recallDays <= 0m || recallDays > remainingTaken)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.InvalidRecallDays)
                .WithData("no", No)
                .WithData("recallDays", recallDays)
                .WithData("availableToRecall", remainingTaken);
        }

        RecalledDays += recallDays;
        RecallDate = recallDate.Date;
        RecallReason = Check.Length(reason?.Trim(), nameof(reason), ErpDomainConsts.MaxDescriptionLength);
        LeaveStatus = LeaveApplicationStatus.Recalled;
    }
}

/// <summary>Summary of an employee's leave balance for one leave type and year.</summary>
public class EmployeeLeaveBalanceSummary
{
    public Guid EmployeeId { get; init; }
    public string EmployeeNo { get; init; }
    public string EmployeeName { get; init; }
    public string LeaveTypeCode { get; init; }
    public string LeaveTypeDescription { get; init; }
    public int LeavePeriodYear { get; init; }
    public decimal EntitledDays { get; init; }
    public decimal AccruedDays { get; init; }
    public decimal CarriedForwardDays { get; init; }
    public decimal TakenDays { get; init; }
    public decimal RecalledDays { get; init; }
    public decimal AdjustedDays { get; init; }
    public decimal BalanceDays { get; init; }
}

/// <summary>
/// Domain service for Leave Management: calculates working-day dates from the employee's Base Calendar,
/// allocates annual/monthly entitlements and carry-overs, posts approved leave applications into
/// Leave Ledger Entries and Employee Absences, and processes leave recalls.
/// </summary>
public class LeaveManager : DomainService
{
    private readonly IRepository<LeaveType, Guid> _leaveTypeRepository;
    private readonly IRepository<LeaveLedgerEntry, Guid> _ledgerRepository;
    private readonly IRepository<LeaveApplication, Guid> _applicationRepository;
    private readonly IRepository<Employee, Guid> _employeeRepository;
    private readonly IRepository<EmployeeAbsence, Guid> _absenceRepository;
    private readonly BaseCalendarManager _calendarManager;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public LeaveManager(
        IRepository<LeaveType, Guid> leaveTypeRepository,
        IRepository<LeaveLedgerEntry, Guid> ledgerRepository,
        IRepository<LeaveApplication, Guid> applicationRepository,
        IRepository<Employee, Guid> employeeRepository,
        IRepository<EmployeeAbsence, Guid> absenceRepository,
        BaseCalendarManager calendarManager
    )
    {
        _leaveTypeRepository = leaveTypeRepository;
        _ledgerRepository = ledgerRepository;
        _applicationRepository = applicationRepository;
        _employeeRepository = employeeRepository;
        _absenceRepository = absenceRepository;
        _calendarManager = calendarManager;
    }

    public async Task<decimal> GetBalanceAsync(Guid employeeId, string leaveTypeCode, int year)
    {
        var code = CodeTableEntity.NormalizeCode(leaveTypeCode);
        var entries = await _ledgerRepository.GetListAsync(e =>
            e.EmployeeId == employeeId && e.LeaveTypeCode == code && e.LeavePeriodYear == year);
        return entries.Sum(e => e.Days);
    }

    public async Task<WorkingPeriod> CalculateDatesAsync(Employee employee, DateTime startDate, decimal days)
    {
        var wholeDays = Math.Max(1, (int)Math.Ceiling(days));
        var calendarCode = await _calendarManager.ResolveCalendarCodeAsync(employee);
        return await _calendarManager.CalculatePeriodAsync(calendarCode, startDate, wholeDays);
    }

    public async Task<LeaveLedgerEntry> PostLedgerEntryAsync(
        Employee employee,
        string leaveTypeCode,
        int year,
        DateTime postingDate,
        LeaveEntryType entryType,
        string documentNo,
        decimal days,
        string description = null,
        DateTime? fromDate = null,
        DateTime? toDate = null
    )
    {
        var leaveType = await GetLeaveTypeAsync(leaveTypeCode);
        EnsureGenderAllowed(employee, leaveType);

        var entry = new LeaveLedgerEntry(
            GuidGenerator.Create(),
            employee.Id,
            employee.No,
            employee.FullName,
            leaveType.Code,
            year,
            postingDate,
            entryType,
            documentNo,
            days,
            description ?? $"{entryType} - {leaveType.Description ?? leaveType.Code}",
            fromDate,
            toDate
        )
        {
            EntryNo = await EntryNos.NextAsync(ErpSequenceNames.LeaveLedgerEntry),
        };

        return await _ledgerRepository.InsertAsync(entry, autoSave: true);
    }

    /// <summary>
    /// Allocates annual entitlement (or monthly accrual) for all active employees or a single employee
    /// for the given leave type and year, carrying forward unused balance up to <see cref="LeaveType.MaxCarryForwardDays"/>.
    /// </summary>
    public async Task<int> AccrueLeaveAsync(
        string leaveTypeCode,
        int year,
        DateTime postingDate,
        Guid? employeeId = null,
        decimal? daysOverride = null,
        bool carryForwardFromPreviousYear = true
    )
    {
        var leaveType = await GetLeaveTypeAsync(leaveTypeCode);
        var employees = employeeId.HasValue
            ? new List<Employee> { await _employeeRepository.GetAsync(employeeId.Value) }
            : await _employeeRepository.GetListAsync(e => e.Status == EmployeeStatus.Active);

        var count = 0;
        foreach (var employee in employees)
        {
            if (leaveType.GenderRestriction.HasValue && employee.Gender != leaveType.GenderRestriction.Value)
            {
                continue;
            }

            // Carry forward from previous year if enabled and not yet carried over.
            if (carryForwardFromPreviousYear && leaveType.MaxCarryForwardDays > 0m)
            {
                var alreadyCarried = await _ledgerRepository.AnyAsync(e =>
                    e.EmployeeId == employee.Id
                    && e.LeaveTypeCode == leaveType.Code
                    && e.LeavePeriodYear == year
                    && e.EntryType == LeaveEntryType.CarryOver);

                if (!alreadyCarried)
                {
                    var prevBalance = await GetBalanceAsync(employee.Id, leaveType.Code, year - 1);
                    var carryDays = Math.Min(Math.Max(0m, prevBalance), leaveType.MaxCarryForwardDays);
                    if (carryDays > 0m)
                    {
                        await PostLedgerEntryAsync(
                            employee,
                            leaveType.Code,
                            year,
                            postingDate,
                            LeaveEntryType.CarryOver,
                            $"CF-{year}-{employee.No}",
                            carryDays,
                            $"Carried forward from {year - 1}"
                        );
                    }
                }
            }

            var entryType = leaveType.AccrualFrequency == LeaveAccrualFrequency.Monthly
                ? LeaveEntryType.Accrual
                : LeaveEntryType.Entitlement;

            var days = daysOverride ?? (leaveType.AccrualFrequency == LeaveAccrualFrequency.Monthly
                ? Math.Round(leaveType.DefaultEntitlementDays / 12m, 2, MidpointRounding.AwayFromZero)
                : leaveType.DefaultEntitlementDays);

            if (days <= 0m)
            {
                continue;
            }

            await PostLedgerEntryAsync(
                employee,
                leaveType.Code,
                year,
                postingDate,
                entryType,
                $"ENT-{year}-{employee.No}",
                days,
                $"{year} {entryType} ({leaveType.Description ?? leaveType.Code})"
            );
            count++;
        }

        return count;
    }

    /// <summary>
    /// Posts a released leave application: checks balance, writes a negative Leave Ledger Entry,
    /// and creates the corresponding Employee Absence record.
    /// </summary>
    public async Task<LeaveApplication> PostApplicationAsync(Guid applicationId)
    {
        var app = await _applicationRepository.GetAsync(applicationId);
        var employee = await _employeeRepository.GetAsync(app.EmployeeId);
        var leaveType = await GetLeaveTypeAsync(app.LeaveTypeCode);

        EnsureGenderAllowed(employee, leaveType);

        if (!leaveType.RequiresApproval && app.LeaveStatus == LeaveApplicationStatus.Open)
        {
            app.Release();
        }

        if (app.LeaveStatus != LeaveApplicationStatus.Released)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveApplicationNotReleased).WithData("no", app.No);
        }

        var balance = await GetBalanceAsync(employee.Id, leaveType.Code, app.LeavePeriodYear);
        if (balance < app.DaysApproved)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.InsufficientLeaveBalance)
                .WithData("employeeNo", employee.No)
                .WithData("leaveType", leaveType.Code)
                .WithData("balance", balance)
                .WithData("requested", app.DaysApproved);
        }

        await PostLedgerEntryAsync(
            employee,
            leaveType.Code,
            app.LeavePeriodYear,
            app.StartDate,
            LeaveEntryType.Usage,
            app.No,
            -app.DaysApproved,
            $"Leave Application {app.No} ({app.StartDate:yyyy-MM-dd} to {app.EndDate:yyyy-MM-dd})",
            app.StartDate,
            app.EndDate
        );

        var absence = new EmployeeAbsence(GuidGenerator.Create(), employee.Id, employee.No);
        absence.Set(
            app.StartDate,
            app.EndDate,
            leaveType.CauseOfAbsenceCode,
            app.Reason ?? $"{leaveType.Description ?? leaveType.Code} ({app.No})",
            app.DaysApproved,
            "DAY",
            (int)Math.Ceiling(app.DaysApproved)
        );
        await _absenceRepository.InsertAsync(absence, autoSave: true);

        app.MarkPosted(absence.Id);
        return await _applicationRepository.UpdateAsync(app, autoSave: true);
    }

    /// <summary>
    /// Recalls an employee from a posted leave application, crediting back the recalled days to the
    /// Leave Ledger and reducing the Employee Absence record.
    /// </summary>
    public async Task<LeaveApplication> RecallApplicationAsync(Guid applicationId, decimal recallDays, DateTime recallDate, string reason)
    {
        var app = await _applicationRepository.GetAsync(applicationId);
        var employee = await _employeeRepository.GetAsync(app.EmployeeId);

        app.RecordRecall(recallDays, recallDate, reason);

        await PostLedgerEntryAsync(
            employee,
            app.LeaveTypeCode,
            app.LeavePeriodYear,
            recallDate,
            LeaveEntryType.Recall,
            $"RCL-{app.No}",
            recallDays,
            $"Leave Recall on {app.No}: {reason ?? "Recalled from leave"}",
            recallDate,
            app.EndDate
        );

        if (app.PostedAbsenceId.HasValue)
        {
            var absence = await _absenceRepository.FindAsync(app.PostedAbsenceId.Value);
            if (absence != null)
            {
                var netDays = Math.Max(0m, app.DaysApproved - app.RecalledDays);
                absence.Set(
                    absence.FromDate,
                    recallDate.Date < absence.ToDate ? recallDate.Date : absence.ToDate,
                    absence.CauseOfAbsenceCode,
                    absence.Description,
                    netDays,
                    absence.UnitOfMeasureCode,
                    (int)Math.Ceiling(netDays)
                );
                await _absenceRepository.UpdateAsync(absence, autoSave: true);
            }
        }

        return await _applicationRepository.UpdateAsync(app, autoSave: true);
    }

    public async Task<LeaveType> GetLeaveTypeAsync(string code)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        return await _leaveTypeRepository.FirstOrDefaultAsync(t => t.Code == normalized)
            ?? throw new BusinessException(ErpErrorCodes.HumanResources.LeaveTypeNotFound).WithData("code", code ?? "");
    }

    public static void EnsureGenderAllowed(Employee employee, LeaveType leaveType)
    {
        if (leaveType.GenderRestriction.HasValue && employee.Gender != leaveType.GenderRestriction.Value)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.LeaveGenderMismatch)
                .WithData("leaveType", leaveType.Code)
                .WithData("requiredGender", leaveType.GenderRestriction.Value.ToString());
        }
    }
}
