using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Academics;

/// <summary>What documents and cards fall back on when a period is left blank: the one marked current.</summary>
public class AcademicCalendar : ITransientDependency
{
    private readonly IRepository<AcademicYear, Guid> _years;
    private readonly IRepository<Semester, Guid> _semesters;
    private readonly IRepository<Intake, Guid> _intakes;

    public AcademicCalendar(IRepository<AcademicYear, Guid> years, IRepository<Semester, Guid> semesters, IRepository<Intake, Guid> intakes)
    {
        _years = years;
        _semesters = semesters;
        _intakes = intakes;
    }

    public async Task<string> YearOrCurrentAsync(string academicYearCode)
    {
        var code = CodeTableEntity.NormalizeCode(academicYearCode);
        if (code == null)
        {
            return (await _years.FirstOrDefaultAsync(y => y.Current))?.Code;
        }

        return await _years.AnyAsync(y => y.Code == code) ? code : throw NotFound("Academic Year", code);
    }

    public async Task<string> IntakeOrCurrentAsync(string intakeCode)
    {
        var code = CodeTableEntity.NormalizeCode(intakeCode);
        if (code == null)
        {
            return (await _intakes.FirstOrDefaultAsync(i => i.Current))?.Code;
        }

        return await _intakes.AnyAsync(i => i.Code == code) ? code : throw NotFound("Intake", code);
    }

    /// <summary>The semester named, or the current one; a document that needs a semester cannot do without both.</summary>
    public async Task<Semester> SemesterOrCurrentAsync(string semesterCode)
    {
        var code = CodeTableEntity.NormalizeCode(semesterCode);
        var semester = code == null ? await _semesters.FirstOrDefaultAsync(s => s.Current) : await _semesters.FirstOrDefaultAsync(s => s.Code == code);

        return semester ?? throw NotFound("Semester", code ?? string.Empty);
    }

    private static BusinessException NotFound(string table, string code) =>
        new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", table).WithData("code", code);
}

/// <summary>Applications for admission: filled in, submitted, approved or rejected, and admitted.</summary>
public class StudentApplicationAppService
    : ErpTableAppService<StudentApplication, StudentApplicationDto, GetStudentApplicationListInput, CreateUpdateStudentApplicationDto>,
        IStudentApplicationAppService
{
    private readonly AcademicSetupManager _setupManager;
    private readonly StudentManager _studentManager;
    private readonly AcademicCalendar _calendar;
    private readonly NoSeriesManager _noSeriesManager;

    public StudentApplicationAppService(
        IRepository<StudentApplication, Guid> repository,
        AcademicSetupManager setupManager,
        StudentManager studentManager,
        AcademicCalendar calendar,
        NoSeriesManager noSeriesManager
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _setupManager = setupManager;
        _studentManager = studentManager;
        _calendar = calendar;
        _noSeriesManager = noSeriesManager;
    }

    public override async Task<StudentApplicationDto> CreateAsync(CreateUpdateStudentApplicationDto input)
    {
        await CheckCreatePolicyAsync();

        var programme = await _studentManager.GetActiveProgrammeAsync(input.ProgrammeCode);
        var setup = await _setupManager.GetAsync();
        var date = input.ApplicationDate == default ? Clock.Now.Date : input.ApplicationDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.ApplicationNos, input.No, date)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Student Application").WithData("key", no);
        }

        var application = new StudentApplication(GuidGenerator.Create(), no, date, input.FirstName, input.LastName, programme.Code);
        await ApplyAsync(application, input, date);

        await Repository.InsertAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    public override async Task<StudentApplicationDto> UpdateAsync(Guid id, CreateUpdateStudentApplicationDto input)
    {
        await CheckUpdatePolicyAsync();

        var application = await GetEntityByIdAsync(id);
        application.EnsureOpen();
        await _studentManager.GetActiveProgrammeAsync(input.ProgrammeCode);
        await ApplyAsync(application, input, input.ApplicationDate == default ? application.ApplicationDate : input.ApplicationDate);

        await Repository.UpdateAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    /// <summary>An admitted application is the record of where a student came from and stays.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var application = await GetEntityByIdAsync(id);
        if (application.Status == ApplicationStatus.Admitted)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ApplicationStatusWrong).WithData("applicationNo", application.No).WithData("status", application.Status);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<StudentApplicationDto> SubmitAsync(Guid id)
    {
        var application = await GetEntityByIdAsync(id);
        application.Submit();
        return await SaveAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<StudentApplicationDto> ApproveAsync(Guid id)
    {
        var application = await GetEntityByIdAsync(id);
        application.Approve();
        return await SaveAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<StudentApplicationDto> RejectAsync(Guid id, RejectApplicationInput input)
    {
        var application = await GetEntityByIdAsync(id);
        application.Reject(input?.Reason);
        return await SaveAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<StudentApplicationDto> ReopenAsync(Guid id)
    {
        var application = await GetEntityByIdAsync(id);
        application.Reopen();
        return await SaveAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<StudentApplicationDto> AdmitAsync(Guid id, AdmitApplicationInput input)
    {
        var application = await GetEntityByIdAsync(id);
        await _studentManager.AdmitAsync(application, input?.AdmissionDate ?? Clock.Now.Date);
        return await MapToGetOutputDtoAsync(application);
    }

    protected override async Task<IQueryable<StudentApplication>> CreateFilteredQueryAsync(GetStudentApplicationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var programme = input.ProgrammeCode?.Trim().ToUpperInvariant();
        var intake = input.IntakeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!programme.IsNullOrEmpty(), x => x.ProgrammeCode == programme)
            .WhereIf(!intake.IsNullOrEmpty(), x => x.IntakeCode == intake)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.FullName.ToLower().Contains(filter)
                    || x.ProgrammeCode.ToLower().Contains(filter)
                    || (x.NationalId != null && x.NationalId.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<StudentApplication> ApplyDefaultSorting(IQueryable<StudentApplication> query) => query.OrderByDescending(x => x.No);

    private async Task<StudentApplicationDto> SaveAsync(StudentApplication application)
    {
        await Repository.UpdateAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    private async Task ApplyAsync(StudentApplication application, CreateUpdateStudentApplicationDto input, DateTime applicationDate)
    {
        application.SetName(input.FirstName, input.OtherName, input.LastName);
        application.SetPersonal(input.Gender, input.DateOfBirth, input.NationalId);
        application.SetContact(input.PhoneNo, input.Email, input.Address, input.City);
        application.SetCourse(
            applicationDate,
            input.ProgrammeCode,
            await _calendar.IntakeOrCurrentAsync(input.IntakeCode),
            await _calendar.YearOrCurrentAsync(input.AcademicYearCode),
            input.StudyMode
        );
        application.SetBackground(input.FormerSchool, input.IndexNumber, input.MeanGrade, input.Sponsorship, input.GuardianName, input.GuardianPhoneNo);
    }
}

/// <summary>Students. A student has a customer account under the same number, which is where fees are charged.</summary>
public class StudentAppService
    : ErpTableAppService<Student, StudentDto, GetStudentListInput, CreateUpdateStudentDto>,
        IStudentAppService
{
    private readonly StudentManager _studentManager;
    private readonly AcademicCalendar _calendar;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;
    private readonly IRepository<StudentBillHeader, Guid> _bills;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;

    public StudentAppService(
        IRepository<Student, Guid> repository,
        StudentManager studentManager,
        AcademicCalendar calendar,
        IRepository<Customer, Guid> customers,
        IRepository<SemesterRegistration, Guid> registrations,
        IRepository<StudentBillHeader, Guid> bills,
        IRepository<StudentUnit, Guid> studentUnits
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _studentManager = studentManager;
        _calendar = calendar;
        _customers = customers;
        _registrations = registrations;
        _bills = bills;
        _studentUnits = studentUnits;
    }

    public override async Task<StudentDto> CreateAsync(CreateUpdateStudentDto input)
    {
        await CheckCreatePolicyAsync();

        var programme = await _studentManager.GetActiveProgrammeAsync(input.ProgrammeCode);
        var admissionDate = input.AdmissionDate ?? Clock.Now.Date;
        var no = await _studentManager.ResolveStudentNoAsync(programme, input.No, admissionDate);

        var student = new Student(GuidGenerator.Create(), no, input.FirstName, input.LastName, programme.Code);
        await ApplyAsync(student, input, admissionDate);
        await _studentManager.EnsureCustomerAsync(student);

        await Repository.InsertAsync(student, autoSave: true);
        return await MapToGetOutputDtoAsync(student);
    }

    public override async Task<StudentDto> UpdateAsync(Guid id, CreateUpdateStudentDto input)
    {
        await CheckUpdatePolicyAsync();

        var student = await GetEntityByIdAsync(id);
        var programme = CodeTableEntity.NormalizeCode(input.ProgrammeCode);

        // A student's registrations and marks are in the programme they were taken in.
        if (student.ProgrammeCode != programme)
        {
            await _studentManager.GetActiveProgrammeAsync(programme);
            if (await _registrations.AnyAsync(r => r.StudentNo == student.No))
            {
                throw new BusinessException(ErpErrorCodes.Academics.RecordInUse).WithData("table", "Student").WithData("code", student.No);
            }
        }

        await ApplyAsync(student, input, input.AdmissionDate ?? student.AdmissionDate);
        await _studentManager.EnsureCustomerAsync(student);

        await Repository.UpdateAsync(student, autoSave: true);
        return await MapToGetOutputDtoAsync(student);
    }

    /// <summary>A student who has registered or been billed keeps their record.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var student = await GetEntityByIdAsync(id);
        if (await _registrations.AnyAsync(r => r.StudentNo == student.No) || await _bills.AnyAsync(b => b.StudentNo == student.No))
        {
            throw new BusinessException(ErpErrorCodes.Academics.RecordInUse).WithData("table", "Student").WithData("code", student.No);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }

    public async Task<StudentBalanceDto> GetBalanceAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var student = await GetEntityByIdAsync(id);
        var customer = student.CustomerNo == null ? null : await _customers.FirstOrDefaultAsync(c => c.No == student.CustomerNo);
        var billed = (await _bills.GetListAsync(b => b.StudentNo == student.No && b.Status == AcademicDocumentStatus.Posted)).Sum(b => b.TotalAmount);

        return new StudentBalanceDto
        {
            StudentNo = student.No,
            CustomerNo = student.CustomerNo,
            TotalBilled = billed,
            Balance = customer?.Balance ?? 0m,
            Prepayment = StudentAccountEngine.PrepaymentOf(customer),
        };
    }

    public async Task<StudentTranscriptDto> GetTranscriptAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var student = await GetEntityByIdAsync(id);
        var graded = (await _studentUnits.GetListAsync(u => u.StudentNo == student.No && u.Grade != null))
            .OrderBy(u => u.AcademicYearCode, StringComparer.Ordinal)
            .ThenBy(u => u.SemesterCode, StringComparer.Ordinal)
            .ThenBy(u => u.UnitCode, StringComparer.Ordinal)
            .ToList();

        // Units without credit hours count equally.
        var weights = graded.Sum(u => u.CreditHours) > 0m ? graded.Select(u => u.CreditHours).ToList() : graded.Select(_ => 1m).ToList();
        var totalWeight = weights.Sum();

        return new StudentTranscriptDto
        {
            StudentNo = student.No,
            StudentName = student.FullName,
            ProgrammeCode = student.ProgrammeCode,
            UnitsTaken = graded.Count,
            UnitsPassed = graded.Count(u => u.Passed),
            MeanScore = graded.Count == 0 ? 0m : Math.Round(graded.Average(u => u.FinalScore), 2, MidpointRounding.AwayFromZero),
            MeanPoints = totalWeight == 0m ? 0m : Math.Round(graded.Select((u, i) => u.Points * weights[i]).Sum() / totalWeight, 2, MidpointRounding.AwayFromZero),
            Lines = graded
                .Select(u => new TranscriptLineDto
                {
                    AcademicYearCode = u.AcademicYearCode,
                    SemesterCode = u.SemesterCode,
                    StageCode = u.StageCode,
                    UnitCode = u.UnitCode,
                    UnitDescription = u.UnitDescription,
                    CreditHours = u.CreditHours,
                    FinalScore = u.FinalScore,
                    Grade = u.Grade,
                    Points = u.Points,
                    ResultRemarks = u.ResultRemarks,
                    Passed = u.Passed,
                })
                .ToList(),
        };
    }

    protected override async Task<IQueryable<Student>> CreateFilteredQueryAsync(GetStudentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var programme = input.ProgrammeCode?.Trim().ToUpperInvariant();
        var stage = input.StageCode?.Trim().ToUpperInvariant();
        var intake = input.IntakeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!programme.IsNullOrEmpty(), x => x.ProgrammeCode == programme)
            .WhereIf(!stage.IsNullOrEmpty(), x => x.CurrentStageCode == stage)
            .WhereIf(!intake.IsNullOrEmpty(), x => x.IntakeCode == intake)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.FullName.ToLower().Contains(filter)
                    || x.ProgrammeCode.ToLower().Contains(filter)
                    || (x.NationalId != null && x.NationalId.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<Student> ApplyDefaultSorting(IQueryable<Student> query) => query.OrderBy(x => x.No);

    private async Task ApplyAsync(Student student, CreateUpdateStudentDto input, DateTime? admissionDate)
    {
        student.SetName(input.FirstName, input.OtherName, input.LastName);
        student.SetPersonal(input.Gender, input.DateOfBirth, input.NationalId);
        student.SetContact(input.PhoneNo, input.Email, input.Address, input.City);
        student.SetCourse(
            input.ProgrammeCode,
            await _calendar.IntakeOrCurrentAsync(input.IntakeCode),
            await _calendar.YearOrCurrentAsync(input.AcademicYearCode),
            input.StudyMode,
            admissionDate
        );
        student.SetSponsor(input.Sponsorship, input.GuardianName, input.GuardianPhoneNo);
        student.SetStatus(input.Status);
    }
}

/// <summary>Semester registrations: prepared, filled with units and submitted.</summary>
public class SemesterRegistrationAppService
    : ErpTableAppService<SemesterRegistration, SemesterRegistrationDto, GetSemesterRegistrationListInput, CreateUpdateSemesterRegistrationDto>,
        ISemesterRegistrationAppService
{
    private readonly SemesterRegistrationEngine _engine;
    private readonly AcademicSetupManager _setupManager;
    private readonly AcademicCalendar _calendar;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;

    public SemesterRegistrationAppService(
        IRepository<SemesterRegistration, Guid> repository,
        SemesterRegistrationEngine engine,
        AcademicSetupManager setupManager,
        AcademicCalendar calendar,
        NoSeriesManager noSeriesManager,
        IRepository<Student, Guid> students,
        IRepository<StudentUnit, Guid> studentUnits
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _engine = engine;
        _setupManager = setupManager;
        _calendar = calendar;
        _noSeriesManager = noSeriesManager;
        _students = students;
        _studentUnits = studentUnits;
    }

    public override async Task<SemesterRegistrationDto> CreateAsync(CreateUpdateSemesterRegistrationDto input)
    {
        await CheckCreatePolicyAsync();

        var student = await GetStudentAsync(input.StudentNo);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode);
        await EnsureStageExistsAsync(student.ProgrammeCode, input.StageCode);

        var setup = await _setupManager.GetAsync();
        var date = input.RegistrationDate == default ? Clock.Now.Date : input.RegistrationDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.RegistrationNos, input.No, date)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Semester Registration").WithData("key", no);
        }

        var registration = new SemesterRegistration(GuidGenerator.Create(), no, student, input.StageCode, semester.Code, date);
        registration.Set(input.StageCode, semester.Code, await YearOfAsync(input.AcademicYearCode, semester), date, input.RegisterFor, input.Remarks);

        await Repository.InsertAsync(registration, autoSave: true);
        return await MapToGetOutputDtoAsync(registration);
    }

    public override async Task<SemesterRegistrationDto> UpdateAsync(Guid id, CreateUpdateSemesterRegistrationDto input)
    {
        await CheckUpdatePolicyAsync();

        var registration = await GetEntityByIdAsync(id);
        registration.EnsureOpen();

        var student = await GetStudentAsync(input.StudentNo);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode);
        await EnsureStageExistsAsync(student.ProgrammeCode, input.StageCode);

        var units = await _studentUnits.GetListAsync(u => u.RegistrationNo == registration.No);

        // The units are those of the registration's programme and stage; another would leave them out of place.
        if (units.Count > 0 && (registration.ProgrammeCode != student.ProgrammeCode || registration.StageCode != CodeTableEntity.NormalizeCode(input.StageCode)))
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentHasLines).WithData("documentNo", registration.No);
        }

        registration.SetStudent(student);
        registration.Set(
            input.StageCode,
            semester.Code,
            await YearOfAsync(input.AcademicYearCode, semester),
            input.RegistrationDate == default ? registration.RegistrationDate : input.RegistrationDate,
            input.RegisterFor,
            input.Remarks
        );

        foreach (var unit in units)
        {
            unit.SetRegistration(registration);
            await _studentUnits.UpdateAsync(unit, autoSave: true);
        }

        await Repository.UpdateAsync(registration, autoSave: true);
        return await MapToGetOutputDtoAsync(registration);
    }

    /// <summary>A submitted registration is on the student's record and stays; an open one goes with its units.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var registration = await GetEntityByIdAsync(id);
        registration.EnsureOpen();

        await _studentUnits.DeleteAsync(u => u.RegistrationNo == registration.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<SemesterRegistrationDto> FillUnitsAsync(Guid id)
    {
        var registration = await GetEntityByIdAsync(id);
        await _engine.FillUnitsAsync(registration);
        return await MapToGetOutputDtoAsync(registration);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<SemesterRegistrationDto> SubmitAsync(Guid id)
    {
        var registration = await GetEntityByIdAsync(id);
        await _engine.SubmitAsync(registration);
        return await MapToGetOutputDtoAsync(registration);
    }

    protected override async Task<IQueryable<SemesterRegistration>> CreateFilteredQueryAsync(GetSemesterRegistrationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var student = input.StudentNo?.Trim().ToUpperInvariant();
        var semester = input.SemesterCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!student.IsNullOrEmpty(), x => x.StudentNo == student)
            .WhereIf(!semester.IsNullOrEmpty(), x => x.SemesterCode == semester)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || x.ProgrammeCode.ToLower().Contains(filter)
                    || x.SemesterCode.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<SemesterRegistration> ApplyDefaultSorting(IQueryable<SemesterRegistration> query) => query.OrderByDescending(x => x.No);

    private async Task<string> YearOfAsync(string academicYearCode, Semester semester)
    {
        return academicYearCode.IsNullOrWhiteSpace() && semester.AcademicYearCode != null
            ? semester.AcademicYearCode
            : await _calendar.YearOrCurrentAsync(academicYearCode);
    }

    private async Task<Student> GetStudentAsync(string studentNo)
    {
        var no = CodeTableEntity.NormalizeCode(studentNo);
        return await _students.FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", no ?? string.Empty);
    }

    private async Task EnsureStageExistsAsync(string programmeCode, string stageCode)
    {
        var stage = CodeTableEntity.NormalizeCode(stageCode);
        await Relations.EnsureExistsAsync<ProgrammeStage>(s => s.ProgrammeCode == programmeCode && s.Code == stage, "Programme Stage", stage);
    }
}

/// <summary>
/// The units of semester registrations, with the marks posted to them. Units can be added and
/// removed only while their registration is open; marks come from exam results, never from here.
/// </summary>
public class StudentUnitAppService
    : ErpTableAppService<StudentUnit, StudentUnitDto, GetStudentUnitListInput, CreateUpdateStudentUnitDto>,
        IStudentUnitAppService
{
    private readonly SemesterRegistrationEngine _engine;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;
    private readonly IRepository<CourseUnit, Guid> _courseUnits;

    public StudentUnitAppService(
        IRepository<StudentUnit, Guid> repository,
        SemesterRegistrationEngine engine,
        IRepository<SemesterRegistration, Guid> registrations,
        IRepository<CourseUnit, Guid> courseUnits
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _engine = engine;
        _registrations = registrations;
        _courseUnits = courseUnits;
    }

    public override async Task<StudentUnitDto> CreateAsync(CreateUpdateStudentUnitDto input)
    {
        await CheckCreatePolicyAsync();

        var registration = await GetOpenRegistrationAsync(input.RegistrationNo);
        var courseUnit = await GetCourseUnitAsync(registration, input.UnitCode);
        await EnsureUnitOnceAsync(registration.No, courseUnit.Code, null);

        var unit = new StudentUnit(GuidGenerator.Create(), registration, courseUnit);

        await Repository.InsertAsync(unit, autoSave: true);
        await _engine.UpdateTotalsAsync(registration);
        return await MapToGetOutputDtoAsync(unit);
    }

    public override async Task<StudentUnitDto> UpdateAsync(Guid id, CreateUpdateStudentUnitDto input)
    {
        await CheckUpdatePolicyAsync();

        var unit = await GetEntityByIdAsync(id);
        var registration = await GetOpenRegistrationAsync(unit.RegistrationNo);
        var courseUnit = await GetCourseUnitAsync(registration, input.UnitCode);
        await EnsureUnitOnceAsync(registration.No, courseUnit.Code, id);

        unit.SetUnit(courseUnit);

        await Repository.UpdateAsync(unit, autoSave: true);
        return await MapToGetOutputDtoAsync(unit);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var unit = await GetEntityByIdAsync(id);
        var registration = await GetOpenRegistrationAsync(unit.RegistrationNo);

        await Repository.DeleteAsync(id, autoSave: true);
        await _engine.UpdateTotalsAsync(registration);
    }

    protected override async Task<IQueryable<StudentUnit>> CreateFilteredQueryAsync(GetStudentUnitListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var registration = input.RegistrationNo?.Trim().ToUpperInvariant();
        var student = input.StudentNo?.Trim().ToUpperInvariant();
        var unit = input.UnitCode?.Trim().ToUpperInvariant();
        var semester = input.SemesterCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!registration.IsNullOrEmpty(), x => x.RegistrationNo == registration)
            .WhereIf(!student.IsNullOrEmpty(), x => x.StudentNo == student)
            .WhereIf(!unit.IsNullOrEmpty(), x => x.UnitCode == unit)
            .WhereIf(!semester.IsNullOrEmpty(), x => x.SemesterCode == semester)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.RegistrationNo.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || x.UnitCode.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<StudentUnit> ApplyDefaultSorting(IQueryable<StudentUnit> query) =>
        query.OrderBy(x => x.RegistrationNo).ThenBy(x => x.UnitCode);

    private async Task<SemesterRegistration> GetOpenRegistrationAsync(string registrationNo)
    {
        var no = CodeTableEntity.NormalizeCode(registrationNo);
        var registration = await _registrations.FirstOrDefaultAsync(r => r.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Semester Registration").WithData("code", no ?? string.Empty);

        registration.EnsureOpen();
        return registration;
    }

    private async Task<CourseUnit> GetCourseUnitAsync(SemesterRegistration registration, string unitCode)
    {
        var code = CodeTableEntity.NormalizeCode(unitCode);
        var unit = await _courseUnits.FirstOrDefaultAsync(u => u.ProgrammeCode == registration.ProgrammeCode && u.Code == code);

        return unit == null || unit.Blocked
            ? throw new BusinessException(ErpErrorCodes.Academics.UnitNotInProgramme).WithData("unit", code ?? string.Empty).WithData("programme", registration.ProgrammeCode)
            : unit;
    }

    private async Task EnsureUnitOnceAsync(string registrationNo, string unitCode, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.RegistrationNo == registrationNo && x.UnitCode == unitCode && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Student Unit").WithData("key", $"{registrationNo} {unitCode}");
        }
    }
}

/// <summary>Student bills: prepared, filled from the fee structure and posted.</summary>
public class StudentBillAppService
    : ErpTableAppService<StudentBillHeader, StudentBillHeaderDto, GetStudentBillListInput, CreateUpdateStudentBillHeaderDto>,
        IStudentBillAppService
{
    private readonly StudentBillingEngine _engine;
    private readonly AcademicSetupManager _setupManager;
    private readonly AcademicCalendar _calendar;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<StudentBillLine, Guid> _lines;

    public StudentBillAppService(
        IRepository<StudentBillHeader, Guid> repository,
        StudentBillingEngine engine,
        AcademicSetupManager setupManager,
        AcademicCalendar calendar,
        NoSeriesManager noSeriesManager,
        IRepository<Student, Guid> students,
        IRepository<StudentBillLine, Guid> lines
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _engine = engine;
        _setupManager = setupManager;
        _calendar = calendar;
        _noSeriesManager = noSeriesManager;
        _students = students;
        _lines = lines;
    }

    public override async Task<StudentBillHeaderDto> CreateAsync(CreateUpdateStudentBillHeaderDto input)
    {
        await CheckCreatePolicyAsync();

        var student = await GetStudentAsync(input.StudentNo);
        var setup = await _setupManager.GetAsync();
        var postingDate = input.PostingDate == default ? Clock.Now.Date : input.PostingDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.BillingNos, input.No, postingDate)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Student Bill").WithData("key", no);
        }

        var header = new StudentBillHeader(GuidGenerator.Create(), no, student, postingDate);
        await ApplyAsync(header, student, input, postingDate);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<StudentBillHeaderDto> UpdateAsync(Guid id, CreateUpdateStudentBillHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        var student = await GetStudentAsync(input.StudentNo);
        header.SetStudent(student);
        await ApplyAsync(header, student, input, input.PostingDate == default ? header.PostingDate : input.PostingDate);

        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    /// <summary>A posted bill is the source of ledger entries and stays; an open one goes with its lines.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        await _lines.DeleteAsync(l => l.DocumentNo == header.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<StudentBillHeaderDto> SuggestLinesAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.SuggestLinesAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<StudentBillHeaderDto> RunPostingAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.PostAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    protected override async Task<IQueryable<StudentBillHeader>> CreateFilteredQueryAsync(GetStudentBillListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var student = input.StudentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!student.IsNullOrEmpty(), x => x.StudentNo == student)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<StudentBillHeader> ApplyDefaultSorting(IQueryable<StudentBillHeader> query) => query.OrderByDescending(x => x.No);

    private async Task ApplyAsync(StudentBillHeader header, Student student, CreateUpdateStudentBillHeaderDto input, DateTime postingDate)
    {
        var stage = CodeTableEntity.NormalizeCode(input.StageCode) ?? student.CurrentStageCode;
        var programme = student.ProgrammeCode;
        await Relations.EnsureExistsAsync<ProgrammeStage>(s => s.ProgrammeCode == programme && s.Code == stage, "Programme Stage", stage);
        await CodeTableChecker.EnsureExistsAsync<Semester>(input.SemesterCode);

        header.SetDetails(
            postingDate,
            stage,
            CodeTableEntity.NormalizeCode(input.SemesterCode) ?? student.CurrentSemesterCode,
            await _calendar.YearOrCurrentAsync(input.AcademicYearCode),
            input.Description
        );
    }

    private async Task<Student> GetStudentAsync(string studentNo)
    {
        var no = CodeTableEntity.NormalizeCode(studentNo);
        return await _students.FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", no ?? string.Empty);
    }
}

/// <summary>The lines of student bills. They can be changed only while their bill is open.</summary>
public class StudentBillLineAppService
    : ErpTableAppService<StudentBillLine, StudentBillLineDto, GetDocumentLineListInput, CreateUpdateStudentBillLineDto>,
        IStudentBillLineAppService
{
    private readonly StudentBillingEngine _engine;
    private readonly IRepository<StudentBillHeader, Guid> _headers;
    private readonly IRepository<FeeItem, Guid> _feeItems;

    public StudentBillLineAppService(
        IRepository<StudentBillLine, Guid> repository,
        StudentBillingEngine engine,
        IRepository<StudentBillHeader, Guid> headers,
        IRepository<FeeItem, Guid> feeItems
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _engine = engine;
        _headers = headers;
        _feeItems = feeItems;
    }

    public override async Task<StudentBillLineDto> CreateAsync(CreateUpdateStudentBillLineDto input)
    {
        await CheckCreatePolicyAsync();

        var header = await GetOpenHeaderAsync(input.DocumentNo);
        var feeItem = await GetFeeItemAsync(input.FeeItemCode);

        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.DocumentNo == header.No).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        var line = new StudentBillLine(GuidGenerator.Create(), header.No, lineNo, feeItem, input.Amount ?? feeItem.DefaultAmount);
        line.Set(feeItem, input.Description, input.Amount ?? feeItem.DefaultAmount);

        await Repository.InsertAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<StudentBillLineDto> UpdateAsync(Guid id, CreateUpdateStudentBillLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);
        var feeItem = await GetFeeItemAsync(input.FeeItemCode);

        line.Set(feeItem, input.Description, input.Amount ?? feeItem.DefaultAmount);

        await Repository.UpdateAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);

        await Repository.DeleteAsync(id, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
    }

    protected override async Task<IQueryable<StudentBillLine>> CreateFilteredQueryAsync(GetDocumentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                    || x.FeeItemCode.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<StudentBillLine> ApplyDefaultSorting(IQueryable<StudentBillLine> query) =>
        query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private async Task<StudentBillHeader> GetOpenHeaderAsync(string documentNo)
    {
        var no = CodeTableEntity.NormalizeCode(documentNo);
        var header = await _headers.FirstOrDefaultAsync(h => h.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student Bill").WithData("code", no ?? string.Empty);

        header.EnsureOpen();
        return header;
    }

    private async Task<FeeItem> GetFeeItemAsync(string feeItemCode)
    {
        var code = CodeTableEntity.NormalizeCode(feeItemCode);
        return await _feeItems.FirstOrDefaultAsync(f => f.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Fee Item").WithData("code", code ?? string.Empty);
    }
}

/// <summary>Exam results: one assessment part of one unit, entered and posted to the students' units.</summary>
public class ExamResultAppService
    : ErpTableAppService<ExamResultHeader, ExamResultHeaderDto, GetExamResultListInput, CreateUpdateExamResultHeaderDto>,
        IExamResultAppService
{
    private readonly ExamResultEngine _engine;
    private readonly AcademicSetupManager _setupManager;
    private readonly AcademicCalendar _calendar;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IRepository<CourseUnit, Guid> _courseUnits;
    private readonly IRepository<ExamResultLine, Guid> _lines;

    public ExamResultAppService(
        IRepository<ExamResultHeader, Guid> repository,
        ExamResultEngine engine,
        AcademicSetupManager setupManager,
        AcademicCalendar calendar,
        NoSeriesManager noSeriesManager,
        IRepository<CourseUnit, Guid> courseUnits,
        IRepository<ExamResultLine, Guid> lines
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _engine = engine;
        _setupManager = setupManager;
        _calendar = calendar;
        _noSeriesManager = noSeriesManager;
        _courseUnits = courseUnits;
        _lines = lines;
    }

    public override async Task<ExamResultHeaderDto> CreateAsync(CreateUpdateExamResultHeaderDto input)
    {
        await CheckCreatePolicyAsync();

        var unit = await GetCourseUnitAsync(input.ProgrammeCode, input.UnitCode);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode);
        await Relations.EnsureNoExistsAsync<Employee>(input.LecturerNo);

        var setup = await _setupManager.GetAsync();
        var date = input.DocumentDate == default ? Clock.Now.Date : input.DocumentDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.ExamResultNos, input.No, date)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Exam Results").WithData("key", no);
        }

        var header = new ExamResultHeader(GuidGenerator.Create(), no, unit, semester.Code, input.ExamType, date);
        header.Set(unit, semester.Code, await YearOfAsync(input.AcademicYearCode, semester), input.ExamType, input.LecturerNo, date);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<ExamResultHeaderDto> UpdateAsync(Guid id, CreateUpdateExamResultHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        var unit = await GetCourseUnitAsync(input.ProgrammeCode, input.UnitCode);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode);
        await Relations.EnsureNoExistsAsync<Employee>(input.LecturerNo);

        // The lines are the students of the document's unit and semester; another would mark the wrong class.
        if ((header.ProgrammeCode != unit.ProgrammeCode || header.UnitCode != unit.Code || header.SemesterCode != semester.Code)
            && await _lines.AnyAsync(l => l.DocumentNo == header.No))
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentHasLines).WithData("documentNo", header.No);
        }

        header.Set(
            unit,
            semester.Code,
            await YearOfAsync(input.AcademicYearCode, semester),
            input.ExamType,
            input.LecturerNo,
            input.DocumentDate == default ? header.DocumentDate : input.DocumentDate
        );

        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    /// <summary>Posted results are on the students' records and stay; open ones go with their lines.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        await _lines.DeleteAsync(l => l.DocumentNo == header.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<ExamResultHeaderDto> SuggestLinesAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.SuggestLinesAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<ExamResultHeaderDto> RunPostingAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.PostAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    protected override async Task<IQueryable<ExamResultHeader>> CreateFilteredQueryAsync(GetExamResultListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var programme = input.ProgrammeCode?.Trim().ToUpperInvariant();
        var unit = input.UnitCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!programme.IsNullOrEmpty(), x => x.ProgrammeCode == programme)
            .WhereIf(!unit.IsNullOrEmpty(), x => x.UnitCode == unit)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.ProgrammeCode.ToLower().Contains(filter)
                    || x.UnitCode.ToLower().Contains(filter)
                    || x.SemesterCode.ToLower().Contains(filter)
            );
    }

    protected override IQueryable<ExamResultHeader> ApplyDefaultSorting(IQueryable<ExamResultHeader> query) => query.OrderByDescending(x => x.No);

    private async Task<string> YearOfAsync(string academicYearCode, Semester semester)
    {
        return academicYearCode.IsNullOrWhiteSpace() && semester.AcademicYearCode != null
            ? semester.AcademicYearCode
            : await _calendar.YearOrCurrentAsync(academicYearCode);
    }

    private async Task<CourseUnit> GetCourseUnitAsync(string programmeCode, string unitCode)
    {
        var programme = CodeTableEntity.NormalizeCode(programmeCode);
        var code = CodeTableEntity.NormalizeCode(unitCode);

        return await _courseUnits.FirstOrDefaultAsync(u => u.ProgrammeCode == programme && u.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Academics.UnitNotInProgramme).WithData("unit", code ?? string.Empty).WithData("programme", programme ?? string.Empty);
    }
}

/// <summary>The lines of exam results. They can be changed only while their document is open.</summary>
public class ExamResultLineAppService
    : ErpTableAppService<ExamResultLine, ExamResultLineDto, GetDocumentLineListInput, CreateUpdateExamResultLineDto>,
        IExamResultLineAppService
{
    private readonly ExamResultEngine _engine;
    private readonly IRepository<ExamResultHeader, Guid> _headers;
    private readonly IRepository<Student, Guid> _students;

    public ExamResultLineAppService(
        IRepository<ExamResultLine, Guid> repository,
        ExamResultEngine engine,
        IRepository<ExamResultHeader, Guid> headers,
        IRepository<Student, Guid> students
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _engine = engine;
        _headers = headers;
        _students = students;
    }

    public override async Task<ExamResultLineDto> CreateAsync(CreateUpdateExamResultLineDto input)
    {
        await CheckCreatePolicyAsync();

        var header = await GetOpenHeaderAsync(input.DocumentNo);
        var student = await GetStudentAsync(input.StudentNo);
        await EnsureStudentOnceAsync(header.No, student.No, null);

        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.DocumentNo == header.No).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        var line = new ExamResultLine(GuidGenerator.Create(), header.No, lineNo, student.No, student.FullName);
        line.SetMark(input.Mark, input.NotDone);

        await Repository.InsertAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ExamResultLineDto> UpdateAsync(Guid id, CreateUpdateExamResultLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);
        var student = await GetStudentAsync(input.StudentNo);
        await EnsureStudentOnceAsync(header.No, student.No, id);

        line.SetStudent(student.No, student.FullName);
        line.SetMark(input.Mark, input.NotDone);

        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);

        await Repository.DeleteAsync(id, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
    }

    protected override async Task<IQueryable<ExamResultLine>> CreateFilteredQueryAsync(GetDocumentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ExamResultLine> ApplyDefaultSorting(IQueryable<ExamResultLine> query) =>
        query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private async Task<ExamResultHeader> GetOpenHeaderAsync(string documentNo)
    {
        var no = CodeTableEntity.NormalizeCode(documentNo);
        var header = await _headers.FirstOrDefaultAsync(h => h.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Exam Results").WithData("code", no ?? string.Empty);

        header.EnsureOpen();
        return header;
    }

    private async Task<Student> GetStudentAsync(string studentNo)
    {
        var no = CodeTableEntity.NormalizeCode(studentNo);
        return await _students.FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", no ?? string.Empty);
    }

    private async Task EnsureStudentOnceAsync(string documentNo, string studentNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.DocumentNo == documentNo && x.StudentNo == studentNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentDuplicated).WithData("studentNo", studentNo).WithData("documentNo", documentNo);
        }
    }
}
