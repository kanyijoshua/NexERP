using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Academics;

/// <summary>What the campus documents share: numbering from the Academic Setup and looking up their headers.</summary>
public abstract class CampusDocumentAppService<TEntity, TDto, TListInput, TInput> : ErpTableAppService<TEntity, TDto, TListInput, TInput>
    where TEntity : class, IEntity<Guid>, IHasNo
    where TDto : Volo.Abp.Application.Dtos.IEntityDto<Guid>
    where TListInput : ErpPagedListInput
{
    protected AcademicSetupManager SetupManager => LazyServiceProvider.LazyGetRequiredService<AcademicSetupManager>();

    protected NoSeriesManager NoSeriesManager => LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>();

    protected AcademicCalendar Calendar => LazyServiceProvider.LazyGetRequiredService<AcademicCalendar>();

    /// <summary>The table's caption in "already exists" messages.</summary>
    protected abstract string Caption { get; }

    protected CampusDocumentAppService(IRepository<TEntity, Guid> repository)
        : base(repository, ErpPermissions.Academics.Default) { }

    protected async Task<string> ResolveNoAsync(string seriesCode, string typedNo, DateTime date)
    {
        var no = (await NoSeriesManager.ResolveNoAsync(seriesCode, typedNo, date)).ToUpperInvariant();
        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", Caption).WithData("key", no);
        }

        return no;
    }

    protected async Task<CourseUnit> GetUnitAsync(string programmeCode, string unitCode)
    {
        var (programme, code) = (CodeTableEntity.NormalizeCode(programmeCode), CodeTableEntity.NormalizeCode(unitCode));
        return await LazyServiceProvider.LazyGetRequiredService<IRepository<CourseUnit, Guid>>().FirstOrDefaultAsync(u => u.ProgrammeCode == programme && u.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Academics.UnitNotInProgramme).WithData("unit", code ?? string.Empty).WithData("programme", programme ?? string.Empty);
    }

    protected async Task<Student> GetStudentAsync(string studentNo)
    {
        var no = CodeTableEntity.NormalizeCode(studentNo);
        return await LazyServiceProvider.LazyGetRequiredService<IRepository<Student, Guid>>().FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", no ?? string.Empty);
    }
}

/// <summary>Finds a document's header for its lines, and the next line number.</summary>
internal static class DocumentLines
{
    public static async Task<THeader> GetHeaderAsync<THeader>(IRepository<THeader, Guid> headers, string documentNo, string caption)
        where THeader : class, IEntity<Guid>, IHasNo
    {
        var no = CodeTableEntity.NormalizeCode(documentNo);
        return await headers.FirstOrDefaultAsync(h => h.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", caption).WithData("code", no ?? string.Empty);
    }

    public static async Task<int> NextLineNoAsync<TLine>(IRepository<TLine, Guid> lines, System.Linq.Expressions.Expression<Func<TLine, bool>> ofDocument, Func<TLine, int> lineNo)
        where TLine : class, IEntity<Guid>
    {
        var existing = await lines.GetListAsync(ofDocument);
        return (existing.Count == 0 ? 0 : existing.Max(lineNo)) + 10000;
    }
}

// ---------------------------------------------------------------------------- Timetable

public class LectureRoomAppService : AcademicCodeTableAppService<LectureRoom, LectureRoomDto, CreateUpdateLectureRoomDto>, ILectureRoomAppService
{
    private readonly IRepository<TimetableEntry, Guid> _entries;

    public LectureRoomAppService(IRepository<LectureRoom, Guid> repository, IRepository<TimetableEntry, Guid> entries)
        : base(repository)
    {
        _entries = entries;
    }

    protected override LectureRoom NewEntity(Guid id, CreateUpdateLectureRoomDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(LectureRoom entity, CreateUpdateLectureRoomDto input)
    {
        entity.Set(input.RoomType, input.BuildingCode, input.MaximumCapacity, input.Blocked);
        return Task.CompletedTask;
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var room = await Repository.GetAsync(id);
        EnsureNotInUse(await _entries.AnyAsync(e => e.RoomCode == room.Code), "Lecture Room", room.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>Teaching and exam timetables. Every save is checked for clashes.</summary>
public class TimetableEntryAppService
    : ErpTableAppService<TimetableEntry, TimetableEntryDto, GetTimetableEntryListInput, CreateUpdateTimetableEntryDto>,
        ITimetableEntryAppService
{
    private readonly TimetableManager _manager;
    private readonly AcademicCalendar _calendar;
    private readonly IRepository<CourseUnit, Guid> _units;

    public TimetableEntryAppService(IRepository<TimetableEntry, Guid> repository, TimetableManager manager, AcademicCalendar calendar, IRepository<CourseUnit, Guid> units)
        : base(repository, ErpPermissions.Academics.Default)
    {
        _manager = manager;
        _calendar = calendar;
        _units = units;
    }

    public override async Task<TimetableEntryDto> CreateAsync(CreateUpdateTimetableEntryDto input)
    {
        await CheckCreatePolicyAsync();

        var unit = await GetUnitAsync(input);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode);
        var entry = new TimetableEntry(GuidGenerator.Create(), semester.Code, unit);
        await ApplyAsync(entry, unit, semester, input);

        await Repository.InsertAsync(entry, autoSave: true);
        return await MapToGetOutputDtoAsync(entry);
    }

    public override async Task<TimetableEntryDto> UpdateAsync(Guid id, CreateUpdateTimetableEntryDto input)
    {
        await CheckUpdatePolicyAsync();

        var entry = await GetEntityByIdAsync(id);
        var unit = await GetUnitAsync(input);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode ?? entry.SemesterCode);
        await ApplyAsync(entry, unit, semester, input);

        await Repository.UpdateAsync(entry, autoSave: true);
        return await MapToGetOutputDtoAsync(entry);
    }

    protected override async Task<IQueryable<TimetableEntry>> CreateFilteredQueryAsync(GetTimetableEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var semester = input.SemesterCode?.Trim().ToUpperInvariant();
        var programme = input.ProgrammeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!semester.IsNullOrEmpty(), x => x.SemesterCode == semester)
            .WhereIf(!programme.IsNullOrEmpty(), x => x.ProgrammeCode == programme)
            .WhereIf(input.TimetableType.HasValue, x => x.TimetableType == input.TimetableType.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.UnitCode.ToLower().Contains(filter)
                    || x.ProgrammeCode.ToLower().Contains(filter)
                    || (x.RoomCode != null && x.RoomCode.ToLower().Contains(filter))
                    || (x.LecturerNo != null && x.LecturerNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<TimetableEntry> ApplyDefaultSorting(IQueryable<TimetableEntry> query) =>
        query.OrderBy(x => x.SemesterCode).ThenBy(x => x.TimetableType).ThenBy(x => x.ExamDate).ThenBy(x => x.Day).ThenBy(x => x.StartTime);

    private async Task<CourseUnit> GetUnitAsync(CreateUpdateTimetableEntryDto input)
    {
        var (programme, code) = (CodeTableEntity.NormalizeCode(input.ProgrammeCode), CodeTableEntity.NormalizeCode(input.UnitCode));
        return await _units.FirstOrDefaultAsync(u => u.ProgrammeCode == programme && u.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Academics.UnitNotInProgramme).WithData("unit", code ?? string.Empty).WithData("programme", programme ?? string.Empty);
    }

    private async Task ApplyAsync(TimetableEntry entry, CourseUnit unit, Semester semester, CreateUpdateTimetableEntryDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<LectureRoom>(input.RoomCode);
        await Relations.EnsureNoExistsAsync<Employee>(input.LecturerNo);

        entry.SetUnit(semester.Code, input.AcademicYearCode ?? semester.AcademicYearCode, unit);
        entry.SetSlot(input.TimetableType, input.Day, input.ExamDate, input.StartTime, input.EndTime);
        entry.SetPlace(input.RoomCode, input.LecturerNo, input.Remarks);

        await _manager.CheckAsync(entry);
    }
}

// ---------------------------------------------------------------------------- Class attendance

/// <summary>Class attendance registers: filled from the unit's students and posted to their attendance counts.</summary>
public class AttendanceRegisterAppService
    : CampusDocumentAppService<AttendanceRegister, AttendanceRegisterDto, GetCampusDocumentListInput, CreateUpdateAttendanceRegisterDto>,
        IAttendanceRegisterAppService
{
    private readonly AttendanceEngine _engine;
    private readonly IRepository<AttendanceLine, Guid> _lines;

    public AttendanceRegisterAppService(IRepository<AttendanceRegister, Guid> repository, AttendanceEngine engine, IRepository<AttendanceLine, Guid> lines)
        : base(repository)
    {
        _engine = engine;
        _lines = lines;
    }

    protected override string Caption => "Attendance Register";

    public override async Task<AttendanceRegisterDto> CreateAsync(CreateUpdateAttendanceRegisterDto input)
    {
        await CheckCreatePolicyAsync();

        var unit = await GetUnitAsync(input.ProgrammeCode, input.UnitCode);
        var semester = await Calendar.SemesterOrCurrentAsync(input.SemesterCode);
        var date = input.LessonDate == default ? Clock.Now.Date : input.LessonDate;
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).AttendanceNos, input.No, date);

        var register = new AttendanceRegister(GuidGenerator.Create(), no, unit, semester.Code, date);
        await ApplyAsync(register, unit, semester, input, date);

        await Repository.InsertAsync(register, autoSave: true);
        return await MapToGetOutputDtoAsync(register);
    }

    public override async Task<AttendanceRegisterDto> UpdateAsync(Guid id, CreateUpdateAttendanceRegisterDto input)
    {
        await CheckUpdatePolicyAsync();

        var register = await GetEntityByIdAsync(id);
        var unit = await GetUnitAsync(input.ProgrammeCode, input.UnitCode);

        // The lines are students of the unit; another unit would leave them registered for the wrong one.
        if ((unit.Code != register.UnitCode || unit.ProgrammeCode != register.ProgrammeCode) && await _lines.AnyAsync(l => l.DocumentNo == register.No))
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentHasLines).WithData("documentNo", register.No);
        }

        var semester = await Calendar.SemesterOrCurrentAsync(input.SemesterCode ?? register.SemesterCode);
        await ApplyAsync(register, unit, semester, input, input.LessonDate == default ? register.LessonDate : input.LessonDate);

        await Repository.UpdateAsync(register, autoSave: true);
        return await MapToGetOutputDtoAsync(register);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var register = await GetEntityByIdAsync(id);
        register.EnsureOpen();

        await _lines.DeleteAsync(l => l.DocumentNo == register.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<AttendanceRegisterDto> SuggestLinesAsync(Guid id)
    {
        var register = await GetEntityByIdAsync(id);
        await _engine.SuggestLinesAsync(register);
        return await MapToGetOutputDtoAsync(register);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<AttendanceRegisterDto> RunPostingAsync(Guid id)
    {
        var register = await GetEntityByIdAsync(id);
        await _engine.PostAsync(register);
        return await MapToGetOutputDtoAsync(register);
    }

    protected override async Task<IQueryable<AttendanceRegister>> CreateFilteredQueryAsync(GetCampusDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.No.ToLower().Contains(filter) || x.UnitCode.ToLower().Contains(filter) || x.ProgrammeCode.ToLower().Contains(filter) || x.SemesterCode.ToLower().Contains(filter)
        );
    }

    protected override IQueryable<AttendanceRegister> ApplyDefaultSorting(IQueryable<AttendanceRegister> query) => query.OrderByDescending(x => x.No);

    private async Task ApplyAsync(AttendanceRegister register, CourseUnit unit, Semester semester, CreateUpdateAttendanceRegisterDto input, DateTime date)
    {
        await Relations.EnsureNoExistsAsync<Employee>(input.LecturerNo);
        register.Set(
            unit,
            semester.Code,
            await Calendar.YearOrCurrentAsync(input.AcademicYearCode ?? semester.AcademicYearCode),
            input.LecturerNo,
            date,
            input.StartTime,
            input.Hours,
            input.Remarks
        );
    }
}

/// <summary>The students on attendance registers. They can be changed only while their register is open.</summary>
public class AttendanceLineAppService
    : ErpTableAppService<AttendanceLine, AttendanceLineDto, GetDocumentLineListInput, CreateUpdateAttendanceLineDto>,
        IAttendanceLineAppService
{
    private readonly AttendanceEngine _engine;
    private readonly IRepository<AttendanceRegister, Guid> _headers;
    private readonly IRepository<Student, Guid> _students;

    public AttendanceLineAppService(
        IRepository<AttendanceLine, Guid> repository,
        AttendanceEngine engine,
        IRepository<AttendanceRegister, Guid> headers,
        IRepository<Student, Guid> students
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _engine = engine;
        _headers = headers;
        _students = students;
    }

    public override async Task<AttendanceLineDto> CreateAsync(CreateUpdateAttendanceLineDto input)
    {
        await CheckCreatePolicyAsync();

        var header = await GetOpenHeaderAsync(input.DocumentNo);
        var student = await GetStudentAsync(input.StudentNo);
        var lineNo = input.LineNo > 0 ? input.LineNo : await DocumentLines.NextLineNoAsync(Repository, l => l.DocumentNo == header.No, l => l.LineNo);

        var line = new AttendanceLine(GuidGenerator.Create(), header.No, lineNo, student.No, student.FullName);
        line.SetMark(input.Mark);

        await Repository.InsertAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<AttendanceLineDto> UpdateAsync(Guid id, CreateUpdateAttendanceLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);
        var student = await GetStudentAsync(input.StudentNo);
        line.SetStudent(student.No, student.FullName);
        line.SetMark(input.Mark);

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

    protected override async Task<IQueryable<AttendanceLine>> CreateFilteredQueryAsync(GetDocumentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter) || x.StudentNo.ToLower().Contains(filter) || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<AttendanceLine> ApplyDefaultSorting(IQueryable<AttendanceLine> query) => query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private async Task<AttendanceRegister> GetOpenHeaderAsync(string documentNo)
    {
        var header = await DocumentLines.GetHeaderAsync(_headers, documentNo, "Attendance Register");
        header.EnsureOpen();
        return header;
    }

    private async Task<Student> GetStudentAsync(string studentNo)
    {
        var no = CodeTableEntity.NormalizeCode(studentNo);
        return await _students.FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", no ?? string.Empty);
    }
}

// ---------------------------------------------------------------------------- Hostels

public class HostelAppService : AcademicCodeTableAppService<Hostel, HostelDto, CreateUpdateHostelDto>, IHostelAppService
{
    private readonly IRepository<HostelRoom, Guid> _rooms;

    public HostelAppService(IRepository<Hostel, Guid> repository, IRepository<HostelRoom, Guid> rooms)
        : base(repository)
    {
        _rooms = rooms;
    }

    protected override Hostel NewEntity(Guid id, CreateUpdateHostelDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(Hostel entity, CreateUpdateHostelDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<FeeItem>(input.FeeItemCode);
        entity.Set(input.Gender, input.CostPerOccupant, input.FeeItemCode, input.Blocked);
    }

    public override async Task<HostelDto> UpdateAsync(Guid id, CreateUpdateHostelDto input)
    {
        var existing = await Repository.GetAsync(id);
        if (existing.Code != CodeTableEntity.NormalizeCode(input.Code))
        {
            EnsureNotInUse(await _rooms.AnyAsync(r => r.HostelCode == existing.Code), "Hostel", existing.Code);
        }

        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var hostel = await Repository.GetAsync(id);
        EnsureNotInUse(await _rooms.AnyAsync(r => r.HostelCode == hostel.Code), "Hostel", hostel.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>The rooms of the hostels.</summary>
public class HostelRoomAppService
    : ErpTableAppService<HostelRoom, HostelRoomDto, GetHostelRoomListInput, CreateUpdateHostelRoomDto>,
        IHostelRoomAppService
{
    private readonly IRepository<HostelAllocation, Guid> _allocations;

    public HostelRoomAppService(IRepository<HostelRoom, Guid> repository, IRepository<HostelAllocation, Guid> allocations)
        : base(repository, ErpPermissions.AcademicSetup.Default)
    {
        _allocations = allocations;
    }

    public override async Task<HostelRoomDto> CreateAsync(CreateUpdateHostelRoomDto input)
    {
        await CheckCreatePolicyAsync();
        await CodeTableChecker.EnsureExistsAsync<Hostel>(input.HostelCode);

        var room = new HostelRoom(GuidGenerator.Create(), input.HostelCode, input.RoomNo);
        await EnsureKeyIsUniqueAsync(room);
        room.Set(input.BedSpaces, input.RoomCost, input.OutOfOrder);

        await Repository.InsertAsync(room, autoSave: true);
        return await MapToGetOutputDtoAsync(room);
    }

    public override async Task<HostelRoomDto> UpdateAsync(Guid id, CreateUpdateHostelRoomDto input)
    {
        await CheckUpdatePolicyAsync();

        var room = await GetEntityByIdAsync(id);
        var (hostel, roomNo) = (room.HostelCode, room.RoomNo);

        await CodeTableChecker.EnsureExistsAsync<Hostel>(input.HostelCode);
        room.SetKey(input.HostelCode, input.RoomNo);
        if (room.HostelCode != hostel || room.RoomNo != roomNo)
        {
            await EnsureNotInUseAsync(hostel, roomNo);
            await EnsureKeyIsUniqueAsync(room);
        }

        room.Set(input.BedSpaces, input.RoomCost, input.OutOfOrder);

        await Repository.UpdateAsync(room, autoSave: true);
        return await MapToGetOutputDtoAsync(room);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var room = await GetEntityByIdAsync(id);
        await EnsureNotInUseAsync(room.HostelCode, room.RoomNo);

        await Repository.DeleteAsync(id, autoSave: true);
    }

    protected override async Task<IQueryable<HostelRoom>> CreateFilteredQueryAsync(GetHostelRoomListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var hostel = input.HostelCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!hostel.IsNullOrEmpty(), x => x.HostelCode == hostel)
            .WhereIf(input.VacantOnly == true, x => !x.OutOfOrder && x.OccupiedSpaces < x.BedSpaces)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.HostelCode.ToLower().Contains(filter) || x.RoomNo.ToLower().Contains(filter));
    }

    protected override IQueryable<HostelRoom> ApplyDefaultSorting(IQueryable<HostelRoom> query) => query.OrderBy(x => x.HostelCode).ThenBy(x => x.RoomNo);

    private async Task EnsureKeyIsUniqueAsync(HostelRoom room)
    {
        if (await Repository.AnyAsync(x => x.HostelCode == room.HostelCode && x.RoomNo == room.RoomNo && x.Id != room.Id))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Hostel Room").WithData("key", $"{room.HostelCode} {room.RoomNo}");
        }
    }

    private async Task EnsureNotInUseAsync(string hostel, string roomNo)
    {
        if (await _allocations.AnyAsync(a => a.HostelCode == hostel && a.RoomNo == roomNo))
        {
            throw new BusinessException(ErpErrorCodes.Academics.RecordInUse).WithData("table", "Hostel Room").WithData("code", $"{hostel} {roomNo}");
        }
    }
}

/// <summary>Hostel allocations: booked, allocated (and billed), and cleared.</summary>
public class HostelAllocationAppService
    : StudentDocumentAppService<HostelAllocation, HostelAllocationDto, CreateUpdateHostelAllocationDto>,
        IHostelAllocationAppService
{
    private readonly HostelManager _manager;
    private readonly AcademicCalendar _calendar;

    public HostelAllocationAppService(IRepository<HostelAllocation, Guid> repository, HostelManager manager, AcademicCalendar calendar)
        : base(repository)
    {
        _manager = manager;
        _calendar = calendar;
    }

    protected override string Caption => "Hostel Allocation";

    public override async Task<HostelAllocationDto> CreateAsync(CreateUpdateHostelAllocationDto input)
    {
        await CheckCreatePolicyAsync();

        var student = await GetStudentAsync(input.StudentNo);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode);
        var date = input.AllocationDate == default ? Clock.Now.Date : input.AllocationDate;
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).HostelAllocationNos, input.No, date);

        var allocation = new HostelAllocation(GuidGenerator.Create(), no, student, input.HostelCode, input.RoomNo, semester.Code, date);
        allocation.Set(student, input.HostelCode, input.RoomNo, semester.Code, date, input.Remarks);
        await _manager.GetRoomAsync(allocation.HostelCode, allocation.RoomNo);

        await Repository.InsertAsync(allocation, autoSave: true);
        return await MapToGetOutputDtoAsync(allocation);
    }

    public override async Task<HostelAllocationDto> UpdateAsync(Guid id, CreateUpdateHostelAllocationDto input)
    {
        await CheckUpdatePolicyAsync();

        var allocation = await GetEntityByIdAsync(id);
        var student = await GetStudentAsync(input.StudentNo);
        var semester = await _calendar.SemesterOrCurrentAsync(input.SemesterCode ?? allocation.SemesterCode);
        allocation.Set(
            student,
            input.HostelCode,
            input.RoomNo,
            semester.Code,
            input.AllocationDate == default ? allocation.AllocationDate : input.AllocationDate,
            input.Remarks
        );
        await _manager.GetRoomAsync(allocation.HostelCode, allocation.RoomNo);

        await Repository.UpdateAsync(allocation, autoSave: true);
        return await MapToGetOutputDtoAsync(allocation);
    }

    /// <summary>Only a booking may go; an allocation has been billed and holds a bed space.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        (await GetEntityByIdAsync(id)).EnsureStatus(HostelAllocationStatus.Booking);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<HostelAllocationDto> AllocateAsync(Guid id)
    {
        var allocation = await GetEntityByIdAsync(id);
        await _manager.AllocateAsync(allocation);
        return await MapToGetOutputDtoAsync(allocation);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<HostelAllocationDto> ClearAsync(Guid id)
    {
        var allocation = await GetEntityByIdAsync(id);
        await _manager.ClearAsync(allocation, Clock.Now);
        return await MapToGetOutputDtoAsync(allocation);
    }

    protected override async Task<IQueryable<HostelAllocation>> CreateFilteredQueryAsync(GetStudentDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var student = input.StudentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!student.IsNullOrEmpty(), x => x.StudentNo == student)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
                    || x.HostelCode.ToLower().Contains(filter)
                    || x.RoomNo.ToLower().Contains(filter)
            );
    }

    protected override IQueryable<HostelAllocation> ApplyDefaultSorting(IQueryable<HostelAllocation> query) => query.OrderByDescending(x => x.No);
}

// ---------------------------------------------------------------------------- Infirmary

/// <summary>Infirmary visits: recorded, then completed, which issues the drugs and bills students.</summary>
public class ClinicVisitAppService
    : CampusDocumentAppService<ClinicVisit, ClinicVisitDto, GetCampusDocumentListInput, CreateUpdateClinicVisitDto>,
        IClinicVisitAppService
{
    private readonly ClinicManager _manager;
    private readonly IRepository<ClinicPrescription, Guid> _prescriptions;
    private readonly IRepository<Employee, Guid> _employees;

    public ClinicVisitAppService(
        IRepository<ClinicVisit, Guid> repository,
        ClinicManager manager,
        IRepository<ClinicPrescription, Guid> prescriptions,
        IRepository<Employee, Guid> employees
    )
        : base(repository)
    {
        _manager = manager;
        _prescriptions = prescriptions;
        _employees = employees;
    }

    protected override string Caption => "Clinic Visit";

    public override async Task<ClinicVisitDto> CreateAsync(CreateUpdateClinicVisitDto input)
    {
        await CheckCreatePolicyAsync();

        var date = input.VisitDate == default ? Clock.Now.Date : input.VisitDate;
        var (patientNo, patientName) = await ResolvePatientAsync(input);
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).ClinicVisitNos, input.No, date);

        var visit = new ClinicVisit(GuidGenerator.Create(), no, input.PatientType, patientNo, patientName, date);
        await ApplyAsync(visit, input);

        await Repository.InsertAsync(visit, autoSave: true);
        return await MapToGetOutputDtoAsync(visit);
    }

    public override async Task<ClinicVisitDto> UpdateAsync(Guid id, CreateUpdateClinicVisitDto input)
    {
        await CheckUpdatePolicyAsync();

        var visit = await GetEntityByIdAsync(id);
        var (patientNo, patientName) = await ResolvePatientAsync(input);
        visit.SetPatient(input.PatientType, patientNo, patientName, input.VisitDate == default ? visit.VisitDate : input.VisitDate);
        await ApplyAsync(visit, input);

        await Repository.UpdateAsync(visit, autoSave: true);
        return await MapToGetOutputDtoAsync(visit);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var visit = await GetEntityByIdAsync(id);
        visit.EnsureOpen();

        await _prescriptions.DeleteAsync(p => p.DocumentNo == visit.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<ClinicVisitDto> CompleteAsync(Guid id)
    {
        var visit = await GetEntityByIdAsync(id);
        await _manager.CompleteAsync(visit);
        return await MapToGetOutputDtoAsync(visit);
    }

    protected override async Task<IQueryable<ClinicVisit>> CreateFilteredQueryAsync(GetCampusDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.No.ToLower().Contains(filter)
                || (x.PatientNo != null && x.PatientNo.ToLower().Contains(filter))
                || x.PatientName.ToLower().Contains(filter)
                || (x.Diagnosis != null && x.Diagnosis.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<ClinicVisit> ApplyDefaultSorting(IQueryable<ClinicVisit> query) => query.OrderByDescending(x => x.No);

    /// <summary>A student's or an employee's name is taken from their record; anyone else's is typed.</summary>
    private async Task<(string No, string Name)> ResolvePatientAsync(CreateUpdateClinicVisitDto input)
    {
        switch (input.PatientType)
        {
            case PatientType.Student:
                var student = await GetStudentAsync(input.PatientNo);
                return (student.No, student.FullName);

            case PatientType.Employee:
                var no = input.PatientNo?.Trim();
                var employee = await _employees.FirstOrDefaultAsync(e => e.No == no)
                    ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Employee").WithData("code", no ?? string.Empty);
                return (employee.No, employee.FullName);

            default:
                return (input.PatientNo, input.PatientName);
        }
    }

    private async Task ApplyAsync(ClinicVisit visit, CreateUpdateClinicVisitDto input)
    {
        await Relations.EnsureNoExistsAsync<Employee>(input.AttendedBy);
        visit.SetClinical(input.TreatmentType, input.Complaint, input.Diagnosis, input.Treatment, input.AttendedBy, input.ReferredTo, input.Charge);
        visit.SetOffDuty(input.OffDutyFrom, input.OffDutyTo, input.LightDutyDays, input.OffDutyComments);
    }
}

/// <summary>The drugs prescribed on infirmary visits.</summary>
public class ClinicPrescriptionAppService
    : ErpTableAppService<ClinicPrescription, ClinicPrescriptionDto, GetDocumentLineListInput, CreateUpdateClinicPrescriptionDto>,
        IClinicPrescriptionAppService
{
    private readonly IRepository<ClinicVisit, Guid> _visits;
    private readonly IRepository<Item, Guid> _items;

    public ClinicPrescriptionAppService(IRepository<ClinicPrescription, Guid> repository, IRepository<ClinicVisit, Guid> visits, IRepository<Item, Guid> items)
        : base(repository, ErpPermissions.Academics.Default)
    {
        _visits = visits;
        _items = items;
    }

    public override async Task<ClinicPrescriptionDto> CreateAsync(CreateUpdateClinicPrescriptionDto input)
    {
        await CheckCreatePolicyAsync();

        var visit = await GetOpenVisitAsync(input.DocumentNo);
        var lineNo = input.LineNo > 0 ? input.LineNo : await DocumentLines.NextLineNoAsync(Repository, l => l.DocumentNo == visit.No, l => l.LineNo);

        var line = new ClinicPrescription(GuidGenerator.Create(), visit.No, lineNo);
        await ApplyAsync(line, input);

        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<ClinicPrescriptionDto> UpdateAsync(Guid id, CreateUpdateClinicPrescriptionDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        await GetOpenVisitAsync(line.DocumentNo);
        await ApplyAsync(line, input);

        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        await GetOpenVisitAsync(line.DocumentNo);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    protected override async Task<IQueryable<ClinicPrescription>> CreateFilteredQueryAsync(GetDocumentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                    || (x.ItemNo != null && x.ItemNo.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ClinicPrescription> ApplyDefaultSorting(IQueryable<ClinicPrescription> query) => query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private async Task<ClinicVisit> GetOpenVisitAsync(string documentNo)
    {
        var visit = await DocumentLines.GetHeaderAsync(_visits, documentNo, "Clinic Visit");
        visit.EnsureOpen();
        return visit;
    }

    private async Task ApplyAsync(ClinicPrescription line, CreateUpdateClinicPrescriptionDto input)
    {
        var description = input.Description;
        if (!input.ItemNo.IsNullOrWhiteSpace())
        {
            var no = input.ItemNo.Trim();
            var item = await _items.FirstOrDefaultAsync(i => i.No == no)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Item").WithData("code", no);
            description = description.IsNullOrWhiteSpace() ? item.Description : description;
        }

        line.Set(input.ItemNo, description, input.Quantity, input.Dosage, input.LocationCode);
    }
}

// ---------------------------------------------------------------------------- Laundry

public class LaundryItemAppService : AcademicCodeTableAppService<LaundryItem, LaundryItemDto, CreateUpdateLaundryItemDto>, ILaundryItemAppService
{
    private readonly TableRelationChecker _relations;
    private readonly IRepository<LaundryOrderLine, Guid> _lines;

    public LaundryItemAppService(IRepository<LaundryItem, Guid> repository, TableRelationChecker relations, IRepository<LaundryOrderLine, Guid> lines)
        : base(repository)
    {
        _relations = relations;
        _lines = lines;
    }

    protected override LaundryItem NewEntity(Guid id, CreateUpdateLaundryItemDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(LaundryItem entity, CreateUpdateLaundryItemDto input)
    {
        await _relations.EnsureGLAccountsExistAsync(input.GLAccountNo);
        entity.Set(input.RatePerItem, input.GLAccountNo);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var item = await Repository.GetAsync(id);
        EnsureNotInUse(await _lines.AnyAsync(l => l.LaundryItemCode == item.Code), "Laundry Item", item.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>Laundry orders: received, invoiced, ready and collected.</summary>
public class LaundryOrderAppService
    : CampusDocumentAppService<LaundryOrder, LaundryOrderDto, GetCampusDocumentListInput, CreateUpdateLaundryOrderDto>,
        ILaundryOrderAppService
{
    private readonly LaundryManager _manager;
    private readonly IRepository<LaundryOrderLine, Guid> _lines;
    private readonly IRepository<Customer, Guid> _customers;

    public LaundryOrderAppService(IRepository<LaundryOrder, Guid> repository, LaundryManager manager, IRepository<LaundryOrderLine, Guid> lines, IRepository<Customer, Guid> customers)
        : base(repository)
    {
        _manager = manager;
        _lines = lines;
        _customers = customers;
    }

    protected override string Caption => "Laundry Order";

    public override async Task<LaundryOrderDto> CreateAsync(CreateUpdateLaundryOrderDto input)
    {
        await CheckCreatePolicyAsync();

        var customer = await GetCustomerAsync(input.CustomerNo);
        var date = input.ReceivedDate == default ? Clock.Now.Date : input.ReceivedDate;
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).LaundryNos, input.No, date);

        var order = new LaundryOrder(GuidGenerator.Create(), no, customer, date);
        order.Set(customer, date, input.PromisedDate, input.Express, input.DiscountPct, input.Remarks);

        await Repository.InsertAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    public override async Task<LaundryOrderDto> UpdateAsync(Guid id, CreateUpdateLaundryOrderDto input)
    {
        await CheckUpdatePolicyAsync();

        var order = await GetEntityByIdAsync(id);
        var customer = await GetCustomerAsync(input.CustomerNo);
        order.Set(customer, input.ReceivedDate == default ? order.ReceivedDate : input.ReceivedDate, input.PromisedDate, input.Express, input.DiscountPct, input.Remarks);

        await Repository.UpdateAsync(order, autoSave: true);

        // The express charge and the discount price every line.
        await _manager.UpdateTotalsAsync(order);
        return await MapToGetOutputDtoAsync(order);
    }

    /// <summary>An invoiced order is the source of ledger entries and stays.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var order = await GetEntityByIdAsync(id);
        order.EnsureStatus(LaundryStatus.Received);

        await _lines.DeleteAsync(l => l.DocumentNo == order.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<LaundryOrderDto> InvoiceAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        await _manager.InvoiceAsync(order, Clock.Now.Date);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<LaundryOrderDto> MarkReadyAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        order.MarkReady();
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<LaundryOrderDto> MarkCollectedAsync(Guid id)
    {
        var order = await GetEntityByIdAsync(id);
        order.MarkCollected(Clock.Now, CurrentUser.UserName);
        await Repository.UpdateAsync(order, autoSave: true);
        return await MapToGetOutputDtoAsync(order);
    }

    protected override async Task<IQueryable<LaundryOrder>> CreateFilteredQueryAsync(GetCampusDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.No.ToLower().Contains(filter) || x.CustomerNo.ToLower().Contains(filter) || (x.CustomerName != null && x.CustomerName.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<LaundryOrder> ApplyDefaultSorting(IQueryable<LaundryOrder> query) => query.OrderByDescending(x => x.No);

    private async Task<Customer> GetCustomerAsync(string customerNo)
    {
        var no = customerNo?.Trim();
        return await _customers.FirstOrDefaultAsync(c => c.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", no ?? string.Empty);
    }
}

/// <summary>The pieces on laundry orders. They can be changed only while the order has not been invoiced.</summary>
public class LaundryOrderLineAppService
    : ErpTableAppService<LaundryOrderLine, LaundryOrderLineDto, GetDocumentLineListInput, CreateUpdateLaundryOrderLineDto>,
        ILaundryOrderLineAppService
{
    private readonly LaundryManager _manager;
    private readonly IRepository<LaundryOrder, Guid> _orders;
    private readonly IRepository<LaundryItem, Guid> _items;

    public LaundryOrderLineAppService(
        IRepository<LaundryOrderLine, Guid> repository,
        LaundryManager manager,
        IRepository<LaundryOrder, Guid> orders,
        IRepository<LaundryItem, Guid> items
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _manager = manager;
        _orders = orders;
        _items = items;
    }

    public override async Task<LaundryOrderLineDto> CreateAsync(CreateUpdateLaundryOrderLineDto input)
    {
        await CheckCreatePolicyAsync();

        var order = await GetOpenOrderAsync(input.DocumentNo);
        var lineNo = input.LineNo > 0 ? input.LineNo : await DocumentLines.NextLineNoAsync(Repository, l => l.DocumentNo == order.No, l => l.LineNo);

        var line = new LaundryOrderLine(GuidGenerator.Create(), order.No, lineNo);
        line.Set(await GetItemAsync(input.LaundryItemCode), input.Description, input.Quantity, input.UnitPrice);

        await Repository.InsertAsync(line, autoSave: true);
        await _manager.UpdateTotalsAsync(order);
        return await MapToGetOutputDtoAsync(await Repository.GetAsync(line.Id));
    }

    public override async Task<LaundryOrderLineDto> UpdateAsync(Guid id, CreateUpdateLaundryOrderLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var order = await GetOpenOrderAsync(line.DocumentNo);
        line.Set(await GetItemAsync(input.LaundryItemCode), input.Description, input.Quantity, input.UnitPrice);

        await Repository.UpdateAsync(line, autoSave: true);
        await _manager.UpdateTotalsAsync(order);
        return await MapToGetOutputDtoAsync(await Repository.GetAsync(line.Id));
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var order = await GetOpenOrderAsync(line.DocumentNo);

        await Repository.DeleteAsync(id, autoSave: true);
        await _manager.UpdateTotalsAsync(order);
    }

    protected override async Task<IQueryable<LaundryOrderLine>> CreateFilteredQueryAsync(GetDocumentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.DocumentNo.ToLower().Contains(filter) || x.LaundryItemCode.ToLower().Contains(filter));
    }

    protected override IQueryable<LaundryOrderLine> ApplyDefaultSorting(IQueryable<LaundryOrderLine> query) => query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private async Task<LaundryOrder> GetOpenOrderAsync(string documentNo)
    {
        var order = await DocumentLines.GetHeaderAsync(_orders, documentNo, "Laundry Order");
        order.EnsureStatus(LaundryStatus.Received);
        return order;
    }

    private async Task<LaundryItem> GetItemAsync(string code)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        return await _items.FirstOrDefaultAsync(i => i.Code == normalized)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Laundry Item").WithData("code", normalized ?? string.Empty);
    }
}

// ---------------------------------------------------------------------------- Short courses

public class ShortCourseAppService : AcademicCodeTableAppService<ShortCourse, ShortCourseDto, CreateUpdateShortCourseDto>, IShortCourseAppService
{
    private readonly TableRelationChecker _relations;
    private readonly IRepository<ShortCourseApplication, Guid> _applications;

    public ShortCourseAppService(IRepository<ShortCourse, Guid> repository, TableRelationChecker relations, IRepository<ShortCourseApplication, Guid> applications)
        : base(repository)
    {
        _relations = relations;
        _applications = applications;
    }

    protected override ShortCourse NewEntity(Guid id, CreateUpdateShortCourseDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(ShortCourse entity, CreateUpdateShortCourseDto input)
    {
        await _relations.EnsureGLAccountsExistAsync(input.GLAccountNo);
        entity.Set(input.DurationDays, input.FeePerParticipant, input.GLAccountNo, input.MaxParticipants, input.Active);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var course = await Repository.GetAsync(id);
        EnsureNotInUse(await _applications.AnyAsync(a => a.CourseCode == course.Code), "Short Course", course.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>Short course applications: submitted, approved or rejected, and registered.</summary>
public class ShortCourseApplicationAppService
    : CampusDocumentAppService<ShortCourseApplication, ShortCourseApplicationDto, GetCampusDocumentListInput, CreateUpdateShortCourseApplicationDto>,
        IShortCourseApplicationAppService
{
    private readonly ShortCourseManager _manager;
    private readonly IRepository<ShortCourseParticipant, Guid> _participants;
    private readonly IRepository<Customer, Guid> _customers;

    public ShortCourseApplicationAppService(
        IRepository<ShortCourseApplication, Guid> repository,
        ShortCourseManager manager,
        IRepository<ShortCourseParticipant, Guid> participants,
        IRepository<Customer, Guid> customers
    )
        : base(repository)
    {
        _manager = manager;
        _participants = participants;
        _customers = customers;
    }

    protected override string Caption => "Short Course Application";

    public override async Task<ShortCourseApplicationDto> CreateAsync(CreateUpdateShortCourseApplicationDto input)
    {
        await CheckCreatePolicyAsync();

        var course = await _manager.GetActiveCourseAsync(CodeTableEntity.NormalizeCode(input.CourseCode));
        var date = input.ApplicationDate == default ? Clock.Now.Date : input.ApplicationDate;
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).ShortCourseNos, input.No, date);

        var application = new ShortCourseApplication(GuidGenerator.Create(), no, input.ApplicationType, course, date);
        await ApplyAsync(application, course, input, date);

        await Repository.InsertAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    public override async Task<ShortCourseApplicationDto> UpdateAsync(Guid id, CreateUpdateShortCourseApplicationDto input)
    {
        await CheckUpdatePolicyAsync();

        var application = await GetEntityByIdAsync(id);
        var course = await _manager.GetActiveCourseAsync(CodeTableEntity.NormalizeCode(input.CourseCode));
        await ApplyAsync(application, course, input, input.ApplicationDate == default ? application.ApplicationDate : input.ApplicationDate);

        await Repository.UpdateAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    /// <summary>A registered application has been billed and stays.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var application = await GetEntityByIdAsync(id);
        application.EnsureStatus(ShortCourseApplicationStatus.Open);

        await _participants.DeleteAsync(p => p.DocumentNo == application.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<ShortCourseApplicationDto> SubmitAsync(Guid id)
    {
        var application = await GetEntityByIdAsync(id);
        await _manager.SubmitAsync(application);
        return await MapToGetOutputDtoAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<ShortCourseApplicationDto> ApproveAsync(Guid id)
    {
        var application = await GetEntityByIdAsync(id);
        application.Approve();
        await Repository.UpdateAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<ShortCourseApplicationDto> RejectAsync(Guid id, RejectShortCourseApplicationInput input)
    {
        var application = await GetEntityByIdAsync(id);
        application.Reject(input?.Reason);
        await Repository.UpdateAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Update)]
    public async Task<ShortCourseApplicationDto> ReopenAsync(Guid id)
    {
        var application = await GetEntityByIdAsync(id);
        application.Reopen();
        await Repository.UpdateAsync(application, autoSave: true);
        return await MapToGetOutputDtoAsync(application);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<ShortCourseApplicationDto> RegisterAsync(Guid id)
    {
        var application = await GetEntityByIdAsync(id);
        await _manager.RegisterAsync(application, Clock.Now.Date);
        return await MapToGetOutputDtoAsync(application);
    }

    protected override async Task<IQueryable<ShortCourseApplication>> CreateFilteredQueryAsync(GetCampusDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.No.ToLower().Contains(filter)
                || x.CourseCode.ToLower().Contains(filter)
                || (x.CustomerNo != null && x.CustomerNo.ToLower().Contains(filter))
                || (x.CustomerName != null && x.CustomerName.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<ShortCourseApplication> ApplyDefaultSorting(IQueryable<ShortCourseApplication> query) => query.OrderByDescending(x => x.No);

    private async Task ApplyAsync(ShortCourseApplication application, ShortCourse course, CreateUpdateShortCourseApplicationDto input, DateTime date)
    {
        Customer sponsor = null;
        if (input.ApplicationType == ShortCourseApplicationType.Corporate && !input.CustomerNo.IsNullOrWhiteSpace())
        {
            var no = input.CustomerNo.Trim();
            sponsor = await _customers.FirstOrDefaultAsync(c => c.No == no)
                ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", no);
        }

        application.Set(
            input.ApplicationType,
            course,
            date,
            input.StartDate == default ? date : input.StartDate,
            input.EndDate,
            sponsor,
            input.FeePerParticipant,
            input.Remarks
        );
    }
}

/// <summary>The participants of short course applications. They can be changed only while the application is open.</summary>
public class ShortCourseParticipantAppService
    : ErpTableAppService<ShortCourseParticipant, ShortCourseParticipantDto, GetDocumentLineListInput, CreateUpdateShortCourseParticipantDto>,
        IShortCourseParticipantAppService
{
    private readonly ShortCourseManager _manager;
    private readonly IRepository<ShortCourseApplication, Guid> _applications;

    public ShortCourseParticipantAppService(
        IRepository<ShortCourseParticipant, Guid> repository,
        ShortCourseManager manager,
        IRepository<ShortCourseApplication, Guid> applications
    )
        : base(repository, ErpPermissions.Academics.Default)
    {
        _manager = manager;
        _applications = applications;
    }

    public override async Task<ShortCourseParticipantDto> CreateAsync(CreateUpdateShortCourseParticipantDto input)
    {
        await CheckCreatePolicyAsync();

        var application = await GetOpenApplicationAsync(input.DocumentNo);
        var lineNo = input.LineNo > 0 ? input.LineNo : await DocumentLines.NextLineNoAsync(Repository, l => l.DocumentNo == application.No, l => l.LineNo);

        var participant = new ShortCourseParticipant(GuidGenerator.Create(), application.No, lineNo, input.Name);
        participant.Set(input.Name, input.NationalId, input.PhoneNo, input.Email);

        await Repository.InsertAsync(participant, autoSave: true);
        await _manager.UpdateTotalsAsync(application);
        return await MapToGetOutputDtoAsync(participant);
    }

    public override async Task<ShortCourseParticipantDto> UpdateAsync(Guid id, CreateUpdateShortCourseParticipantDto input)
    {
        await CheckUpdatePolicyAsync();

        var participant = await GetEntityByIdAsync(id);
        await GetOpenApplicationAsync(participant.DocumentNo);
        participant.Set(input.Name, input.NationalId, input.PhoneNo, input.Email);

        await Repository.UpdateAsync(participant, autoSave: true);
        return await MapToGetOutputDtoAsync(participant);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var participant = await GetEntityByIdAsync(id);
        var application = await GetOpenApplicationAsync(participant.DocumentNo);

        await Repository.DeleteAsync(id, autoSave: true);
        await _manager.UpdateTotalsAsync(application);
    }

    protected override async Task<IQueryable<ShortCourseParticipant>> CreateFilteredQueryAsync(GetDocumentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter) || x.Name.ToLower().Contains(filter) || (x.CertificateNo != null && x.CertificateNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ShortCourseParticipant> ApplyDefaultSorting(IQueryable<ShortCourseParticipant> query) => query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    private async Task<ShortCourseApplication> GetOpenApplicationAsync(string documentNo)
    {
        var application = await DocumentLines.GetHeaderAsync(_applications, documentNo, "Short Course Application");
        application.EnsureStatus(ShortCourseApplicationStatus.Open);
        return application;
    }
}
