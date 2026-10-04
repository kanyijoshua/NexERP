using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Academics;

/// <summary>A lecture room, hall or laboratory classes and exams are held in.</summary>
public class LectureRoom : CodeTableEntity
{
    public RoomType RoomType { get; private set; }
    public string BuildingCode { get; private set; }

    /// <summary>The most students the room seats; 0 is not checked.</summary>
    public int MaximumCapacity { get; private set; }

    /// <summary>A blocked room is out of use and cannot be timetabled.</summary>
    public bool Blocked { get; private set; }

    protected LectureRoom() { }

    public LectureRoom(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(RoomType roomType, string buildingCode, int maximumCapacity, bool blocked)
    {
        if (maximumCapacity < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidCapacity).WithData("code", Code);
        }

        RoomType = roomType;
        BuildingCode = NormalizeCode(Check.Length(buildingCode, nameof(buildingCode), ErpDomainConsts.MaxCodeLength));
        MaximumCapacity = maximumCapacity;
        Blocked = blocked;
    }
}

/// <summary>
/// One session of a timetable: a unit taught (weekly, on a day) or examined (on a date), at a
/// time, in a room, by a lecturer. Saving an entry checks it against the rest of the semester's
/// timetable: a class, a lecturer, a room and, for exams, a student can be in one place at a time.
/// </summary>
public class TimetableEntry : CompanyEntity
{
    public string SemesterCode { get; private set; }
    public string AcademicYearCode { get; private set; }
    public TimetableType TimetableType { get; private set; }
    public string ProgrammeCode { get; private set; }
    public string StageCode { get; private set; }
    public string UnitCode { get; private set; }
    public string UnitDescription { get; private set; }

    /// <summary>The weekday of a teaching session; for an exam, the weekday of its date.</summary>
    public TimetableDay Day { get; private set; }

    /// <summary>The date an exam is sat; blank on a teaching session.</summary>
    public DateTime? ExamDate { get; private set; }

    /// <summary>"HH:mm", so that times sort and compare as text.</summary>
    public string StartTime { get; private set; }

    public string EndTime { get; private set; }

    public string RoomCode { get; private set; }

    /// <summary>The employee who teaches or invigilates.</summary>
    public string LecturerNo { get; private set; }

    public string Remarks { get; private set; }

    protected TimetableEntry() { }

    public TimetableEntry(Guid id, string semesterCode, CourseUnit unit)
        : base(id)
    {
        SetUnit(semesterCode, null, unit);
    }

    public void SetUnit(string semesterCode, string academicYearCode, CourseUnit unit)
    {
        SemesterCode = Check.NotNullOrWhiteSpace(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        AcademicYearCode = CodeTableEntity.NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        ProgrammeCode = unit.ProgrammeCode;
        StageCode = unit.StageCode;
        UnitCode = unit.Code;
        UnitDescription = unit.Description;
    }

    public void SetSlot(TimetableType timetableType, TimetableDay day, DateTime? examDate, string startTime, string endTime)
    {
        var start = ParseTime(startTime);
        var end = ParseTime(endTime);
        if (start == null || end == null || string.CompareOrdinal(end, start) <= 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidTimeRange).WithData("start", startTime ?? string.Empty).WithData("end", endTime ?? string.Empty);
        }

        if (timetableType == TimetableType.Exam && !examDate.HasValue)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ExamDateRequired).WithData("unit", UnitCode);
        }

        TimetableType = timetableType;
        ExamDate = timetableType == TimetableType.Exam ? examDate.Value.Date : null;
        Day = ExamDate.HasValue ? DayOf(ExamDate.Value) : day;
        StartTime = start;
        EndTime = end;
    }

    public void SetPlace(string roomCode, string lecturerNo, string remarks)
    {
        RoomCode = CodeTableEntity.NormalizeCode(Check.Length(roomCode, nameof(roomCode), ErpDomainConsts.MaxCodeLength));
        LecturerNo = lecturerNo.IsNullOrWhiteSpace() ? null : Check.Length(lecturerNo.Trim(), nameof(lecturerNo), ErpDomainConsts.MaxNoLength).ToUpperInvariant();
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>Whether two sessions of the same kind are held at the same time: same day (or date) and overlapping hours.</summary>
    public bool Overlaps(TimetableEntry other) =>
        other.TimetableType == TimetableType
        && (TimetableType == TimetableType.Exam ? other.ExamDate == ExamDate : other.Day == Day)
        && string.CompareOrdinal(StartTime, other.EndTime) < 0
        && string.CompareOrdinal(other.StartTime, EndTime) < 0;

    public static TimetableDay DayOf(DateTime date) => date.DayOfWeek == DayOfWeek.Sunday ? TimetableDay.Sunday : (TimetableDay)(int)date.DayOfWeek;

    /// <summary>"8:00", "08:00" or "08:00:00" as "08:00"; null when it is not a time of day.</summary>
    public static string ParseTime(string value)
    {
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        return TimeSpan.TryParse(value.Trim(), CultureInfo.InvariantCulture, out var time) && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1)
            ? time.ToString(@"hh\:mm", CultureInfo.InvariantCulture)
            : null;
    }
}

/// <summary>Checks timetable entries for clashes and for rooms too small for the class.</summary>
public class TimetableManager : DomainService
{
    private readonly IRepository<TimetableEntry, Guid> _entries;
    private readonly IRepository<LectureRoom, Guid> _rooms;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;

    public TimetableManager(
        IRepository<TimetableEntry, Guid> entries,
        IRepository<LectureRoom, Guid> rooms,
        IRepository<StudentUnit, Guid> studentUnits,
        IRepository<SemesterRegistration, Guid> registrations
    )
    {
        _entries = entries;
        _rooms = rooms;
        _studentUnits = studentUnits;
        _registrations = registrations;
    }

    public async Task CheckAsync(TimetableEntry entry)
    {
        var (id, semester) = (entry.Id, entry.SemesterCode);
        var others = (await _entries.GetListAsync(e => e.SemesterCode == semester && e.Id != id)).Where(entry.Overlaps).ToList();

        foreach (var other in others)
        {
            if (entry.TimetableType == TimetableType.Teaching && other.ProgrammeCode == entry.ProgrammeCode && other.StageCode == entry.StageCode)
            {
                throw Clash(entry, other, $"class {entry.ProgrammeCode} {entry.StageCode}");
            }

            if (entry.LecturerNo != null && other.LecturerNo == entry.LecturerNo)
            {
                throw Clash(entry, other, $"lecturer {entry.LecturerNo}");
            }

            if (entry.RoomCode != null && other.RoomCode == entry.RoomCode)
            {
                throw Clash(entry, other, $"room {entry.RoomCode}");
            }
        }

        var students = await GetStudentsAsync(entry.ProgrammeCode, entry.UnitCode, entry.SemesterCode);

        // An exam is sat by every student of the unit, so two exams at once may not share one.
        if (entry.TimetableType == TimetableType.Exam)
        {
            foreach (var other in others)
            {
                var shared = (await GetStudentsAsync(other.ProgrammeCode, other.UnitCode, other.SemesterCode)).FirstOrDefault(students.Contains);
                if (shared != null)
                {
                    throw Clash(entry, other, $"student {shared}");
                }
            }
        }

        if (entry.RoomCode == null)
        {
            return;
        }

        var code = entry.RoomCode;
        var room = await _rooms.FirstOrDefaultAsync(r => r.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Lecture Room").WithData("code", code);

        if (room.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.Academics.RoomUnavailable).WithData("hostel", string.Empty).WithData("room", room.Code);
        }

        if (room.MaximumCapacity > 0 && students.Count > room.MaximumCapacity)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ClassExceedsRoom)
                .WithData("room", room.Code)
                .WithData("capacity", room.MaximumCapacity)
                .WithData("students", students.Count);
        }
    }

    /// <summary>The students registered (on submitted registrations) for a unit in a semester.</summary>
    public async Task<HashSet<string>> GetStudentsAsync(string programmeCode, string unitCode, string semesterCode)
    {
        var units = await _studentUnits.GetListAsync(u => u.ProgrammeCode == programmeCode && u.UnitCode == unitCode && u.SemesterCode == semesterCode);
        var numbers = units.Select(u => u.RegistrationNo).Distinct().ToList();
        var submitted = (await _registrations.GetListAsync(r => numbers.Contains(r.No) && r.Status == RegistrationStatus.Submitted))
            .Select(r => r.No)
            .ToHashSet(StringComparer.Ordinal);

        return units.Where(u => submitted.Contains(u.RegistrationNo)).Select(u => u.StudentNo).ToHashSet(StringComparer.Ordinal);
    }

    private static BusinessException Clash(TimetableEntry entry, TimetableEntry other, string what) =>
        new BusinessException(ErpErrorCodes.Academics.TimetableClash)
            .WithData("what", what)
            .WithData("unit", other.UnitCode)
            .WithData("day", other.ExamDate?.ToString("yyyy-MM-dd") ?? other.Day.ToString())
            .WithData("time", $"{other.StartTime}-{other.EndTime}");
}
