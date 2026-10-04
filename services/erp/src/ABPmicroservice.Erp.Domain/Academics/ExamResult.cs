using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Academics;

/// <summary>
/// An exam results document: the marks of one assessment part of one unit, for the students
/// registered for it in a semester. Posting it assigns the marks to the students' units.
/// </summary>
public class ExamResultHeader : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string ProgrammeCode { get; private set; }
    public string StageCode { get; private set; }
    public string SemesterCode { get; private set; }
    public string AcademicYearCode { get; private set; }
    public string UnitCode { get; private set; }
    public string UnitDescription { get; private set; }
    public ExamType ExamType { get; private set; }

    /// <summary>The employee who taught and marked the unit.</summary>
    public string LecturerNo { get; private set; }

    public DateTime DocumentDate { get; private set; }

    public AcademicDocumentStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    public int NoOfStudents { get; internal set; }

    protected ExamResultHeader() { }

    public ExamResultHeader(Guid id, string no, CourseUnit unit, string semesterCode, ExamType examType, DateTime documentDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(unit, semesterCode, null, examType, null, documentDate);
    }

    public bool IsOpen => Status == AcademicDocumentStatus.Open;

    public void Set(CourseUnit unit, string semesterCode, string academicYearCode, ExamType examType, string lecturerNo, DateTime documentDate)
    {
        EnsureOpen();
        ProgrammeCode = unit.ProgrammeCode;
        StageCode = unit.StageCode;
        UnitCode = unit.Code;
        UnitDescription = unit.Description;
        SemesterCode = Check.NotNullOrWhiteSpace(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        AcademicYearCode = CodeTableEntity.NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        ExamType = examType;
        LecturerNo = lecturerNo.IsNullOrWhiteSpace() ? null : Check.Length(lecturerNo.Trim(), nameof(lecturerNo), ErpDomainConsts.MaxNoLength);
        DocumentDate = documentDate.Date;
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = AcademicDocumentStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    /// <summary>Only open results may be changed; posted ones are on the students' records.</summary>
    public void EnsureOpen()
    {
        if (Status != AcademicDocumentStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>One student's mark on an exam results document.</summary>
public class ExamResultLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }

    /// <summary>Out of the maximum score of the document's assessment part.</summary>
    public decimal Mark { get; private set; }

    /// <summary>The student did not sit the assessment; no mark is assigned and the unit stays ungraded.</summary>
    public bool NotDone { get; private set; }

    protected ExamResultLine() { }

    public ExamResultLine(Guid id, string documentNo, int lineNo, string studentNo, string studentName)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
        SetStudent(studentNo, studentName);
    }

    public void SetStudent(string studentNo, string studentName)
    {
        StudentNo = Check.NotNullOrWhiteSpace(studentNo, nameof(studentNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        StudentName = studentName;
    }

    public void SetMark(decimal mark, bool notDone)
    {
        if (mark < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Mark");
        }

        Mark = notDone ? 0m : mark;
        NotDone = notDone;
    }
}

/// <summary>
/// Works out a unit's final score and grade from its marks: each assessment part of the
/// programme's exam category contributes its share of the mark out of its maximum, and the grade
/// is the band of the category the rounded total falls in.
/// </summary>
public class GradingManager : DomainService
{
    private readonly IRepository<ExamComponent, Guid> _components;
    private readonly IRepository<GradingBand, Guid> _bands;
    private readonly AcademicSetupManager _setupManager;

    public GradingManager(IRepository<ExamComponent, Guid> components, IRepository<GradingBand, Guid> bands, AcademicSetupManager setupManager)
    {
        _components = components;
        _bands = bands;
        _setupManager = setupManager;
    }

    public async Task<List<ExamComponent>> GetComponentsAsync(string examCategoryCode)
    {
        return (await _components.GetListAsync(c => c.ExamCategoryCode == examCategoryCode)).OrderBy(c => c.ExamType).ToList();
    }

    /// <summary>The grade a score earns in a category; null when no band reaches down to it.</summary>
    public async Task<GradingBand> FindBandAsync(string examCategoryCode, decimal score)
    {
        // The highest band that starts at or below the score, so a gap between two bands (69 and 70
        // with a score of 69.5) still grades.
        return (await _bands.GetListAsync(b => b.ExamCategoryCode == examCategoryCode && b.FromMark <= score))
            .OrderByDescending(b => b.FromMark)
            .FirstOrDefault();
    }

    /// <summary>Recalculates the unit's final score, and grades it once every part has a mark.</summary>
    public async Task GradeAsync(StudentUnit unit, string examCategoryCode)
    {
        var components = await GetComponentsAsync(examCategoryCode);
        var setup = await _setupManager.GetAsync();

        var score = components.Sum(c => (unit.MarkOf(c.ExamType) ?? 0m) / c.MaxScore * c.ContributionPct);
        score = Math.Round(score, setup.ExamRoundingDecimals, MidpointRounding.AwayFromZero);

        var complete = components.Count > 0 && components.All(c => unit.MarkOf(c.ExamType).HasValue);
        unit.SetResult(score, complete ? await FindBandAsync(examCategoryCode, score) : null);
    }
}

/// <summary>Prepares and posts exam results.</summary>
public class ExamResultEngine : DomainService
{
    private readonly IRepository<ExamResultHeader, Guid> _headers;
    private readonly IRepository<ExamResultLine, Guid> _lines;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;
    private readonly IRepository<ExamCategory, Guid> _categories;
    private readonly StudentManager _studentManager;
    private readonly GradingManager _gradingManager;
    private readonly AcademicSetupManager _setupManager;
    private readonly ICurrentUser _currentUser;

    public ExamResultEngine(
        IRepository<ExamResultHeader, Guid> headers,
        IRepository<ExamResultLine, Guid> lines,
        IRepository<StudentUnit, Guid> studentUnits,
        IRepository<SemesterRegistration, Guid> registrations,
        IRepository<ExamCategory, Guid> categories,
        StudentManager studentManager,
        GradingManager gradingManager,
        AcademicSetupManager setupManager,
        ICurrentUser currentUser
    )
    {
        _setupManager = setupManager;
        _headers = headers;
        _lines = lines;
        _studentUnits = studentUnits;
        _registrations = registrations;
        _categories = categories;
        _studentManager = studentManager;
        _gradingManager = gradingManager;
        _currentUser = currentUser;
    }

    /// <summary>Stores the number of lines on the header.</summary>
    public async Task UpdateTotalsAsync(ExamResultHeader header)
    {
        header.NoOfStudents = await _lines.CountAsync(l => l.DocumentNo == header.No);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>
    /// Adds a line for every student registered for the document's unit in its semester who has no
    /// mark yet for its assessment part. Students already on the document are left as they are.
    /// Returns the number of lines added.
    /// </summary>
    public async Task<int> SuggestLinesAsync(ExamResultHeader header)
    {
        header.EnsureOpen();

        var existing = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        var onDocument = existing.Select(l => l.StudentNo).ToHashSet(StringComparer.Ordinal);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);

        var units = (await GetRegisteredUnitsAsync(header))
            .Where(u => !u.MarkOf(header.ExamType).HasValue && !onDocument.Contains(u.StudentNo))
            .OrderBy(u => u.StudentNo, StringComparer.Ordinal)
            .ToList();

        foreach (var unit in units)
        {
            lineNo += 10000;
            await _lines.InsertAsync(new ExamResultLine(GuidGenerator.Create(), header.No, lineNo, unit.StudentNo, unit.StudentName), autoSave: true);
        }

        await UpdateTotalsAsync(header);
        return units.Count;
    }

    public async Task PostAsync(ExamResultHeader header)
    {
        header.EnsureOpen();

        var programme = await _studentManager.GetActiveProgrammeAsync(header.ProgrammeCode);
        if (programme.ExamCategoryCode.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", "Exam Category Code").WithData("setup", $"Programme {programme.Code}");
        }

        var category = await _categories.FirstOrDefaultAsync(c => c.Code == programme.ExamCategoryCode)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Exam Category").WithData("code", programme.ExamCategoryCode);

        if (category.BlockResultsEntry)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ResultsEntryBlocked).WithData("category", category.Code);
        }

        var component = (await _gradingManager.GetComponentsAsync(category.Code)).FirstOrDefault(c => c.ExamType == header.ExamType)
            ?? throw new BusinessException(ErpErrorCodes.Academics.ExamComponentMissing).WithData("examType", header.ExamType).WithData("category", category.Code);

        var lines = (await _lines.GetListAsync(l => l.DocumentNo == header.No)).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NothingToPost).WithData("documentNo", header.No);
        }

        var duplicate = lines.GroupBy(l => l.StudentNo).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentDuplicated).WithData("studentNo", duplicate.Key).WithData("documentNo", header.No);
        }

        var units = (await GetRegisteredUnitsAsync(header)).ToDictionary(u => u.StudentNo, StringComparer.Ordinal);
        var minimumAttendance = header.ExamType == ExamType.FinalExam ? (await _setupManager.GetAsync()).MinAttendancePct : 0m;

        // Checked for every line before any mark is assigned: results are posted whole or not at all.
        foreach (var line in lines)
        {
            if (!units.TryGetValue(line.StudentNo, out var unit))
            {
                throw new BusinessException(ErpErrorCodes.Academics.StudentNotRegisteredForUnit)
                    .WithData("studentNo", line.StudentNo)
                    .WithData("unit", header.UnitCode)
                    .WithData("semester", header.SemesterCode);
            }

            if (line.NotDone)
            {
                continue;
            }

            if (unit.MarkOf(header.ExamType).HasValue)
            {
                throw new BusinessException(ErpErrorCodes.Academics.MarkAlreadyAssigned)
                    .WithData("studentNo", line.StudentNo)
                    .WithData("unit", header.UnitCode)
                    .WithData("examType", header.ExamType);
            }

            if (!unit.MeetsAttendance(minimumAttendance))
            {
                throw new BusinessException(ErpErrorCodes.Academics.NotEligibleForExam)
                    .WithData("studentNo", line.StudentNo)
                    .WithData("unit", header.UnitCode)
                    .WithData("pct", unit.AttendancePct.ToString("0.##"))
                    .WithData("minimum", minimumAttendance.ToString("0.##"));
            }

            if (line.Mark > component.MaxScore)
            {
                throw new BusinessException(ErpErrorCodes.Academics.MarkAboveMaximum)
                    .WithData("studentNo", line.StudentNo)
                    .WithData("mark", line.Mark)
                    .WithData("maximum", component.MaxScore);
            }
        }

        foreach (var line in lines.Where(l => !l.NotDone))
        {
            var unit = units[line.StudentNo];
            unit.SetMark(header.ExamType, line.Mark);
            await _gradingManager.GradeAsync(unit, category.Code);
            await _studentUnits.UpdateAsync(unit, autoSave: true);
        }

        header.NoOfStudents = lines.Count;
        header.MarkPosted(Clock.Now, _currentUser.UserName);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>The units of submitted registrations that the document's marks can go to.</summary>
    private async Task<List<StudentUnit>> GetRegisteredUnitsAsync(ExamResultHeader header)
    {
        var (programme, unitCode, semester, year) = (header.ProgrammeCode, header.UnitCode, header.SemesterCode, header.AcademicYearCode);
        var units = await _studentUnits.GetListAsync(u =>
            u.ProgrammeCode == programme && u.UnitCode == unitCode && u.SemesterCode == semester && (year == null || u.AcademicYearCode == year)
        );

        var numbers = units.Select(u => u.RegistrationNo).Distinct().ToList();
        var submitted = (await _registrations.GetListAsync(r => numbers.Contains(r.No) && r.Status == RegistrationStatus.Submitted))
            .Select(r => r.No)
            .ToHashSet(StringComparer.Ordinal);

        // A student who took the unit twice in the semester (a retake) is marked on the latest registration.
        return units
            .Where(u => submitted.Contains(u.RegistrationNo))
            .GroupBy(u => u.StudentNo, StringComparer.Ordinal)
            .Select(g => g.OrderByDescending(u => u.RegistrationNo, StringComparer.Ordinal).First())
            .ToList();
    }
}
