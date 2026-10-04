using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Academics;

/// <summary>
/// A student's registration for one semester: the stage the student is in and the units taken.
/// It is prepared (Open) and submitted; submitting it is what makes the student current, bills
/// the semester's fees and puts the units up for marks.
/// </summary>
public class SemesterRegistration : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public string ProgrammeCode { get; private set; }
    public string StageCode { get; private set; }
    public string SemesterCode { get; private set; }
    public string AcademicYearCode { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public RegisterFor RegisterFor { get; private set; }
    public string Remarks { get; private set; }

    public RegistrationStatus Status { get; private set; }

    public int NoOfUnits { get; internal set; }

    /// <summary>The bill raised when the registration was submitted.</summary>
    public string BillNo { get; internal set; }

    public decimal BilledAmount { get; internal set; }

    protected SemesterRegistration() { }

    public SemesterRegistration(Guid id, string no, Student student, string stageCode, string semesterCode, DateTime registrationDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        SetStudent(student);
        Set(stageCode, semesterCode, null, registrationDate, RegisterFor.Stage, null);
    }

    public bool IsOpen => Status == RegistrationStatus.Open;

    public void SetStudent(Student student)
    {
        EnsureOpen();
        StudentNo = student.No;
        StudentName = student.FullName;
        ProgrammeCode = student.ProgrammeCode;
    }

    public void Set(string stageCode, string semesterCode, string academicYearCode, DateTime registrationDate, RegisterFor registerFor, string remarks)
    {
        EnsureOpen();
        StageCode = Check.NotNullOrWhiteSpace(stageCode, nameof(stageCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        SemesterCode = Check.NotNullOrWhiteSpace(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        AcademicYearCode = CodeTableEntity.NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        RegistrationDate = registrationDate.Date;
        RegisterFor = registerFor;
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void MarkSubmitted() => Status = RegistrationStatus.Submitted;

    /// <summary>Only an open registration may be changed.</summary>
    public void EnsureOpen()
    {
        if (Status != RegistrationStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>
/// A unit a student is registered for, and the student's marks in it. The marks arrive from posted
/// exam results, one assessment part at a time; the final score and grade are worked out from them.
/// </summary>
public class StudentUnit : CompanyEntity
{
    public string RegistrationNo { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public string ProgrammeCode { get; private set; }
    public string StageCode { get; private set; }
    public string SemesterCode { get; private set; }
    public string AcademicYearCode { get; private set; }

    public string UnitCode { get; private set; }
    public string UnitDescription { get; private set; }
    public UnitType UnitType { get; private set; }
    public decimal CreditHours { get; private set; }

    /// <summary>Marks as entered, out of the maximum of their assessment part; blank until results are posted.</summary>
    public decimal? AssignmentMark { get; private set; }

    public decimal? CatMark { get; private set; }
    public decimal? Cat2Mark { get; private set; }
    public decimal? ExamMark { get; private set; }

    /// <summary>The weighted total of the marks entered so far, out of 100.</summary>
    public decimal FinalScore { get; private set; }

    /// <summary>Blank until every assessment part of the exam category has a mark.</summary>
    public string Grade { get; private set; }

    public decimal Points { get; private set; }
    public string ResultRemarks { get; private set; }
    public bool Passed { get; private set; }

    /// <summary>Class sessions held for the student (excused ones left out) and attended, from posted attendance registers.</summary>
    public int SessionsHeld { get; private set; }

    public int SessionsAttended { get; private set; }

    public decimal AttendancePct { get; private set; }

    internal void RecordAttendance(AttendanceMark mark)
    {
        if (mark == AttendanceMark.Excused)
        {
            return;
        }

        SessionsHeld++;
        if (mark is AttendanceMark.Present or AttendanceMark.Late)
        {
            SessionsAttended++;
        }

        AttendancePct = Math.Round(SessionsAttended * 100m / SessionsHeld, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Whether the student attended enough of the unit to sit its exam; a unit with no sessions recorded is not held against anyone.</summary>
    public bool MeetsAttendance(decimal minimumPct) => minimumPct <= 0m || SessionsHeld == 0 || AttendancePct >= minimumPct;

    protected StudentUnit() { }

    public StudentUnit(Guid id, SemesterRegistration registration, CourseUnit unit)
        : base(id)
    {
        RegistrationNo = registration.No;
        SetRegistration(registration);
        SetUnit(unit);
    }

    /// <summary>The unit's copy of what its registration says, kept in step while the registration is open.</summary>
    public void SetRegistration(SemesterRegistration registration)
    {
        StudentNo = registration.StudentNo;
        StudentName = registration.StudentName;
        ProgrammeCode = registration.ProgrammeCode;
        StageCode = registration.StageCode;
        SemesterCode = registration.SemesterCode;
        AcademicYearCode = registration.AcademicYearCode;
    }

    public void SetUnit(CourseUnit unit)
    {
        UnitCode = unit.Code;
        UnitDescription = unit.Description;
        UnitType = unit.UnitType;
        CreditHours = unit.CreditHours;
    }

    public decimal? MarkOf(ExamType examType) =>
        examType switch
        {
            ExamType.Assignment => AssignmentMark,
            ExamType.Cat => CatMark,
            ExamType.Cat2 => Cat2Mark,
            _ => ExamMark,
        };

    public bool HasMarks => AssignmentMark.HasValue || CatMark.HasValue || Cat2Mark.HasValue || ExamMark.HasValue;

    internal void SetMark(ExamType examType, decimal mark)
    {
        switch (examType)
        {
            case ExamType.Assignment:
                AssignmentMark = mark;
                break;
            case ExamType.Cat:
                CatMark = mark;
                break;
            case ExamType.Cat2:
                Cat2Mark = mark;
                break;
            default:
                ExamMark = mark;
                break;
        }
    }

    /// <param name="band">The grade the score earns; null while the assessment is incomplete.</param>
    internal void SetResult(decimal finalScore, GradingBand band)
    {
        FinalScore = finalScore;
        Grade = band?.Grade;
        Points = band?.Points ?? 0m;
        ResultRemarks = band == null ? null : band.Remarks.IsNullOrWhiteSpace() ? band.Description : band.Remarks;
        Passed = band?.Passed ?? false;
    }
}

/// <summary>Fills, checks and submits semester registrations.</summary>
public class SemesterRegistrationEngine : DomainService
{
    private readonly IRepository<SemesterRegistration, Guid> _registrations;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<CourseUnit, Guid> _courseUnits;
    private readonly IRepository<ProgrammeStage, Guid> _stages;
    private readonly IRepository<Semester, Guid> _semesters;
    private readonly IRepository<StudentBillHeader, Guid> _bills;
    private readonly AcademicSetupManager _setupManager;
    private readonly StudentManager _studentManager;
    private readonly StudentBillingEngine _billingEngine;
    private readonly NoSeriesManager _noSeriesManager;

    public SemesterRegistrationEngine(
        IRepository<SemesterRegistration, Guid> registrations,
        IRepository<StudentUnit, Guid> studentUnits,
        IRepository<Student, Guid> students,
        IRepository<CourseUnit, Guid> courseUnits,
        IRepository<ProgrammeStage, Guid> stages,
        IRepository<Semester, Guid> semesters,
        IRepository<StudentBillHeader, Guid> bills,
        AcademicSetupManager setupManager,
        StudentManager studentManager,
        StudentBillingEngine billingEngine,
        NoSeriesManager noSeriesManager
    )
    {
        _registrations = registrations;
        _studentUnits = studentUnits;
        _students = students;
        _courseUnits = courseUnits;
        _stages = stages;
        _semesters = semesters;
        _bills = bills;
        _setupManager = setupManager;
        _studentManager = studentManager;
        _billingEngine = billingEngine;
        _noSeriesManager = noSeriesManager;
    }

    /// <summary>Stores the number of units on the header.</summary>
    public async Task UpdateTotalsAsync(SemesterRegistration registration)
    {
        registration.NoOfUnits = await _studentUnits.CountAsync(u => u.RegistrationNo == registration.No);
        await _registrations.UpdateAsync(registration, autoSave: true);
    }

    /// <summary>
    /// Adds every unit of the registration's stage that is taught in its semester. Units already on
    /// the registration are left as they are. Returns the number of units added.
    /// </summary>
    public async Task<int> FillUnitsAsync(SemesterRegistration registration)
    {
        registration.EnsureOpen();

        var onRegistration = (await _studentUnits.GetListAsync(u => u.RegistrationNo == registration.No)).Select(u => u.UnitCode).ToHashSet(StringComparer.Ordinal);
        var units = (await _courseUnits.GetListAsync(u => u.ProgrammeCode == registration.ProgrammeCode && u.StageCode == registration.StageCode))
            .Where(u => !u.Blocked && u.IsTaughtIn(registration.SemesterCode) && !onRegistration.Contains(u.Code))
            .OrderBy(u => u.Code, StringComparer.Ordinal)
            .ToList();

        foreach (var unit in units)
        {
            await _studentUnits.InsertAsync(new StudentUnit(GuidGenerator.Create(), registration, unit), autoSave: true);
        }

        await UpdateTotalsAsync(registration);
        return units.Count;
    }

    /// <summary>
    /// Submits the registration after checking that the student may register and may take each
    /// unit, bills the semester's fees when the Academic Setup says so, and moves the student to
    /// the registration's stage and semester.
    /// </summary>
    public async Task SubmitAsync(SemesterRegistration registration)
    {
        registration.EnsureOpen();

        var setup = await _setupManager.GetAsync();
        var student = await _students.FirstOrDefaultAsync(s => s.No == registration.StudentNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", registration.StudentNo);

        if (!student.IsActive)
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentNotActive).WithData("studentNo", student.No).WithData("status", student.Status);
        }

        await _studentManager.GetActiveProgrammeAsync(registration.ProgrammeCode);

        var stage = await _stages.FirstOrDefaultAsync(s => s.ProgrammeCode == registration.ProgrammeCode && s.Code == registration.StageCode)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Programme Stage").WithData("code", registration.StageCode);

        var semester = await _semesters.FirstOrDefaultAsync(s => s.Code == registration.SemesterCode)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Semester").WithData("code", registration.SemesterCode);

        if (!semester.IsRegistrationOpen(registration.RegistrationDate))
        {
            throw new BusinessException(ErpErrorCodes.Academics.RegistrationClosed).WithData("semester", semester.Code);
        }

        var customer = await _studentManager.EnsureCustomerAsync(student);
        if (setup.CheckStudentBalance && customer.Balance > setup.MaxFeeBalanceToRegister)
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentHasBalance)
                .WithData("studentNo", student.No)
                .WithData("balance", customer.Balance.ToString("N2"));
        }

        var units = await _studentUnits.GetListAsync(u => u.RegistrationNo == registration.No);
        await CheckUnitsAsync(registration, stage, units);

        if (setup.BillOnRegistration && registration.RegisterFor is RegisterFor.Stage or RegisterFor.Units)
        {
            await BillAsync(registration, student, setup);
        }

        student.Register(registration.StageCode, registration.SemesterCode);
        await _students.UpdateAsync(student, autoSave: true);

        registration.NoOfUnits = units.Count;
        registration.MarkSubmitted();
        await _registrations.UpdateAsync(registration, autoSave: true);
    }

    private async Task CheckUnitsAsync(SemesterRegistration registration, ProgrammeStage stage, List<StudentUnit> units)
    {
        if (units.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NoUnitsSelected).WithData("documentNo", registration.No);
        }

        if ((stage.MinimumUnits > 0 && units.Count < stage.MinimumUnits) || (stage.MaximumUnits > 0 && units.Count > stage.MaximumUnits))
        {
            throw new BusinessException(ErpErrorCodes.Academics.UnitCountOutOfRange)
                .WithData("stage", stage.Code)
                .WithData("minimum", stage.MinimumUnits)
                .WithData("maximum", stage.MaximumUnits);
        }

        var isFirstSitting = registration.RegisterFor is RegisterFor.Stage or RegisterFor.Units;

        // Everything the student took before this registration, to find repeats and unmet prerequisites.
        var history = (await _studentUnits.GetListAsync(u => u.StudentNo == registration.StudentNo && u.RegistrationNo != registration.No)).ToList();
        var submitted = (await _registrations.GetListAsync(r => r.StudentNo == registration.StudentNo && r.Status == RegistrationStatus.Submitted))
            .Select(r => r.No)
            .ToHashSet(StringComparer.Ordinal);
        history = history.Where(u => submitted.Contains(u.RegistrationNo)).ToList();

        var courseUnits = (await _courseUnits.GetListAsync(u => u.ProgrammeCode == registration.ProgrammeCode)).ToDictionary(u => u.Code, StringComparer.Ordinal);

        foreach (var unit in units)
        {
            if (isFirstSitting)
            {
                var taken = history.FirstOrDefault(h =>
                    h.UnitCode == unit.UnitCode && (h.Passed || (h.SemesterCode == registration.SemesterCode && h.AcademicYearCode == registration.AcademicYearCode))
                );

                if (taken != null)
                {
                    throw new BusinessException(ErpErrorCodes.Academics.UnitAlreadyRegistered)
                        .WithData("studentNo", registration.StudentNo)
                        .WithData("unit", unit.UnitCode)
                        .WithData("documentNo", taken.RegistrationNo);
                }
            }

            if (courseUnits.TryGetValue(unit.UnitCode, out var courseUnit)
                && courseUnit.PrerequisiteUnitCode != null
                && !history.Any(h => h.UnitCode == courseUnit.PrerequisiteUnitCode && h.Passed))
            {
                throw new BusinessException(ErpErrorCodes.Academics.PrerequisiteNotPassed)
                    .WithData("unit", unit.UnitCode)
                    .WithData("prerequisite", courseUnit.PrerequisiteUnitCode);
            }
        }
    }

    /// <summary>Raises and posts the bill for the registration's stage and semester; nothing when the fee structure has no fees for them.</summary>
    private async Task BillAsync(SemesterRegistration registration, Student student, AcademicSetup setup)
    {
        if (!await _billingEngine.HasFeesAsync(registration.ProgrammeCode, registration.StageCode, registration.SemesterCode, student.StudyMode))
        {
            return;
        }

        var no = (await _noSeriesManager.ResolveNoAsync(setup.BillingNos, null, registration.RegistrationDate)).ToUpperInvariant();
        var bill = new StudentBillHeader(GuidGenerator.Create(), no, student, registration.RegistrationDate);
        bill.SetDetails(
            registration.RegistrationDate,
            registration.StageCode,
            registration.SemesterCode,
            registration.AcademicYearCode,
            $"Fees {registration.StageCode} {registration.SemesterCode} {registration.AcademicYearCode}".Trim()
        );
        bill.RegistrationNo = registration.No;
        await _bills.InsertAsync(bill, autoSave: true);

        await _billingEngine.SuggestLinesAsync(bill);
        await _billingEngine.PostAsync(bill);

        registration.BillNo = bill.No;
        registration.BilledAmount = bill.TotalAmount;
    }
}
