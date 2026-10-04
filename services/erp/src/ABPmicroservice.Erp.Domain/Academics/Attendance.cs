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
/// A class attendance register: who attended one session of a unit. Posting it counts the session
/// on each student's unit, and the attendance percentage that builds up there decides who may sit
/// the final exam.
/// </summary>
public class AttendanceRegister : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string ProgrammeCode { get; private set; }
    public string StageCode { get; private set; }
    public string SemesterCode { get; private set; }
    public string AcademicYearCode { get; private set; }
    public string UnitCode { get; private set; }
    public string UnitDescription { get; private set; }

    /// <summary>The employee who taught the session.</summary>
    public string LecturerNo { get; private set; }

    public DateTime LessonDate { get; private set; }

    /// <summary>"HH:mm"; blank when the session time is not recorded.</summary>
    public string StartTime { get; private set; }

    public decimal Hours { get; private set; }
    public string Remarks { get; private set; }

    public AcademicDocumentStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    public int NoOfStudents { get; internal set; }
    public int NoPresent { get; internal set; }

    protected AttendanceRegister() { }

    public AttendanceRegister(Guid id, string no, CourseUnit unit, string semesterCode, DateTime lessonDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(unit, semesterCode, null, null, lessonDate, null, 0m, null);
    }

    public bool IsOpen => Status == AcademicDocumentStatus.Open;

    public void Set(
        CourseUnit unit,
        string semesterCode,
        string academicYearCode,
        string lecturerNo,
        DateTime lessonDate,
        string startTime,
        decimal hours,
        string remarks
    )
    {
        EnsureOpen();
        if (hours < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Hours");
        }

        var time = TimetableEntry.ParseTime(startTime);
        if (!startTime.IsNullOrWhiteSpace() && time == null)
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidTimeRange).WithData("start", startTime).WithData("end", string.Empty);
        }

        ProgrammeCode = unit.ProgrammeCode;
        StageCode = unit.StageCode;
        UnitCode = unit.Code;
        UnitDescription = unit.Description;
        SemesterCode = Check.NotNullOrWhiteSpace(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        AcademicYearCode = CodeTableEntity.NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        LecturerNo = lecturerNo.IsNullOrWhiteSpace() ? null : Check.Length(lecturerNo.Trim(), nameof(lecturerNo), ErpDomainConsts.MaxNoLength).ToUpperInvariant();
        LessonDate = lessonDate.Date;
        StartTime = time;
        Hours = hours;
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = AcademicDocumentStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    public void EnsureOpen()
    {
        if (Status != AcademicDocumentStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>One student on an attendance register.</summary>
public class AttendanceLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public AttendanceMark Mark { get; private set; }

    protected AttendanceLine() { }

    public AttendanceLine(Guid id, string documentNo, int lineNo, string studentNo, string studentName)
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

    public void SetMark(AttendanceMark mark) => Mark = mark;
}

/// <summary>Fills and posts attendance registers.</summary>
public class AttendanceEngine : DomainService
{
    private readonly IRepository<AttendanceRegister, Guid> _headers;
    private readonly IRepository<AttendanceLine, Guid> _lines;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;
    private readonly ICurrentUser _currentUser;

    public AttendanceEngine(
        IRepository<AttendanceRegister, Guid> headers,
        IRepository<AttendanceLine, Guid> lines,
        IRepository<StudentUnit, Guid> studentUnits,
        IRepository<SemesterRegistration, Guid> registrations,
        ICurrentUser currentUser
    )
    {
        _headers = headers;
        _lines = lines;
        _studentUnits = studentUnits;
        _registrations = registrations;
        _currentUser = currentUser;
    }

    public async Task UpdateTotalsAsync(AttendanceRegister header)
    {
        var lines = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        header.NoOfStudents = lines.Count;
        header.NoPresent = lines.Count(l => l.Mark is AttendanceMark.Present or AttendanceMark.Late);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>Adds every student registered for the unit in the semester, marked present. Returns the number added.</summary>
    public async Task<int> SuggestLinesAsync(AttendanceRegister header)
    {
        header.EnsureOpen();

        var existing = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        var onRegister = existing.Select(l => l.StudentNo).ToHashSet(StringComparer.Ordinal);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);

        var units = (await GetRegisteredUnitsAsync(header)).Where(u => !onRegister.Contains(u.StudentNo)).OrderBy(u => u.StudentNo, StringComparer.Ordinal).ToList();
        foreach (var unit in units)
        {
            lineNo += 10000;
            await _lines.InsertAsync(new AttendanceLine(GuidGenerator.Create(), header.No, lineNo, unit.StudentNo, unit.StudentName), autoSave: true);
        }

        await UpdateTotalsAsync(header);
        return units.Count;
    }

    public async Task PostAsync(AttendanceRegister header)
    {
        header.EnsureOpen();

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
        var stranger = lines.FirstOrDefault(l => !units.ContainsKey(l.StudentNo));
        if (stranger != null)
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentNotRegisteredForUnit)
                .WithData("studentNo", stranger.StudentNo)
                .WithData("unit", header.UnitCode)
                .WithData("semester", header.SemesterCode);
        }

        foreach (var line in lines)
        {
            var unit = units[line.StudentNo];
            unit.RecordAttendance(line.Mark);
            await _studentUnits.UpdateAsync(unit, autoSave: true);
        }

        header.NoOfStudents = lines.Count;
        header.NoPresent = lines.Count(l => l.Mark is AttendanceMark.Present or AttendanceMark.Late);
        header.MarkPosted(Clock.Now, _currentUser.UserName);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>The units of submitted registrations the session counts for, the latest per student.</summary>
    private async Task<List<StudentUnit>> GetRegisteredUnitsAsync(AttendanceRegister header)
    {
        var (programme, unitCode, semester, year) = (header.ProgrammeCode, header.UnitCode, header.SemesterCode, header.AcademicYearCode);
        var units = await _studentUnits.GetListAsync(u =>
            u.ProgrammeCode == programme && u.UnitCode == unitCode && u.SemesterCode == semester && (year == null || u.AcademicYearCode == year)
        );

        var numbers = units.Select(u => u.RegistrationNo).Distinct().ToList();
        var submitted = (await _registrations.GetListAsync(r => numbers.Contains(r.No) && r.Status == RegistrationStatus.Submitted))
            .Select(r => r.No)
            .ToHashSet(StringComparer.Ordinal);

        return units
            .Where(u => submitted.Contains(u.RegistrationNo))
            .GroupBy(u => u.StudentNo, StringComparer.Ordinal)
            .Select(g => g.OrderByDescending(u => u.RegistrationNo, StringComparer.Ordinal).First())
            .ToList();
    }
}
