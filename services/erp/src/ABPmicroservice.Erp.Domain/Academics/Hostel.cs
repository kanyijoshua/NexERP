using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Academics;

/// <summary>A hostel (or a block of one): who it houses and what a place in it costs per semester.</summary>
public class Hostel : CodeTableEntity
{
    /// <summary>The students it houses; None takes either.</summary>
    public StudentGender Gender { get; private set; }

    /// <summary>What a place costs for a semester when its room has no cost of its own.</summary>
    public decimal CostPerOccupant { get; private set; }

    /// <summary>The fee item allocations are billed under.</summary>
    public string FeeItemCode { get; private set; }

    public bool Blocked { get; private set; }

    protected Hostel() { }

    public Hostel(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(StudentGender gender, decimal costPerOccupant, string feeItemCode, bool blocked)
    {
        if (costPerOccupant < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Cost Per Occupant");
        }

        Gender = gender;
        CostPerOccupant = costPerOccupant;
        FeeItemCode = NormalizeCode(Check.Length(feeItemCode, nameof(feeItemCode), ErpDomainConsts.MaxCodeLength));
        Blocked = blocked;
    }
}

/// <summary>A room of a hostel and the bed spaces in it.</summary>
public class HostelRoom : CompanyEntity
{
    public string HostelCode { get; private set; }
    public string RoomNo { get; private set; }
    public int BedSpaces { get; private set; }

    /// <summary>What a place in the room costs for a semester; 0 takes the hostel's cost per occupant.</summary>
    public decimal RoomCost { get; private set; }

    /// <summary>A room out of order takes no new allocations.</summary>
    public bool OutOfOrder { get; private set; }

    /// <summary>Students allocated to the room and not yet cleared out of it.</summary>
    public int OccupiedSpaces { get; internal set; }

    protected HostelRoom() { }

    public HostelRoom(Guid id, string hostelCode, string roomNo)
        : base(id)
    {
        SetKey(hostelCode, roomNo);
    }

    public void SetKey(string hostelCode, string roomNo)
    {
        HostelCode = Check.NotNullOrWhiteSpace(hostelCode, nameof(hostelCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        RoomNo = Check.NotNullOrWhiteSpace(roomNo, nameof(roomNo), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void Set(int bedSpaces, decimal roomCost, bool outOfOrder)
    {
        if (bedSpaces < 0 || bedSpaces < OccupiedSpaces)
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidCapacity).WithData("code", $"{HostelCode} {RoomNo}");
        }

        if (roomCost < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Room Cost");
        }

        BedSpaces = bedSpaces;
        RoomCost = roomCost;
        OutOfOrder = outOfOrder;
    }

    public int VacantSpaces => Math.Max(0, BedSpaces - OccupiedSpaces);
}

/// <summary>
/// A student's place in a hostel room for a semester. It is booked, then allocated (which bills the
/// student for the place and takes a bed space) and cleared when the student moves out.
/// </summary>
public class HostelAllocation : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public string HostelCode { get; private set; }
    public string RoomNo { get; private set; }
    public string SemesterCode { get; private set; }
    public DateTime AllocationDate { get; private set; }
    public string Remarks { get; private set; }

    public HostelAllocationStatus Status { get; private set; }

    /// <summary>What the place was billed at, and the bill.</summary>
    public decimal Charges { get; private set; }

    public string BillNo { get; private set; }
    public DateTime? ClearanceDate { get; private set; }
    public string ProcessedBy { get; private set; }

    protected HostelAllocation() { }

    public HostelAllocation(Guid id, string no, Student student, string hostelCode, string roomNo, string semesterCode, DateTime allocationDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(student, hostelCode, roomNo, semesterCode, allocationDate, null);
    }

    public void Set(Student student, string hostelCode, string roomNo, string semesterCode, DateTime allocationDate, string remarks)
    {
        EnsureStatus(HostelAllocationStatus.Booking);
        StudentNo = student.No;
        StudentName = student.FullName;
        HostelCode = Check.NotNullOrWhiteSpace(hostelCode, nameof(hostelCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        RoomNo = Check.NotNullOrWhiteSpace(roomNo, nameof(roomNo), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        SemesterCode = Check.NotNullOrWhiteSpace(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        AllocationDate = allocationDate.Date;
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void MarkAllocated(decimal charges, string billNo, string by)
    {
        Status = HostelAllocationStatus.Allocated;
        Charges = charges;
        BillNo = billNo;
        ProcessedBy = by;
    }

    internal void MarkCleared(DateTime clearanceDate, string by)
    {
        Status = HostelAllocationStatus.Cleared;
        ClearanceDate = clearanceDate.Date;
        ProcessedBy = by;
    }

    public void EnsureStatus(HostelAllocationStatus status)
    {
        if (Status != status)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentStatusWrong).WithData("documentNo", No ?? string.Empty).WithData("status", Status);
        }
    }
}

/// <summary>Allocates hostel places and clears students out of them.</summary>
public class HostelManager : DomainService
{
    private readonly IRepository<HostelAllocation, Guid> _allocations;
    private readonly IRepository<Hostel, Guid> _hostels;
    private readonly IRepository<HostelRoom, Guid> _rooms;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;
    private readonly StudentBillingEngine _billingEngine;
    private readonly ICurrentUser _currentUser;

    public HostelManager(
        IRepository<HostelAllocation, Guid> allocations,
        IRepository<Hostel, Guid> hostels,
        IRepository<HostelRoom, Guid> rooms,
        IRepository<Student, Guid> students,
        IRepository<SemesterRegistration, Guid> registrations,
        StudentBillingEngine billingEngine,
        ICurrentUser currentUser
    )
    {
        _allocations = allocations;
        _hostels = hostels;
        _rooms = rooms;
        _students = students;
        _registrations = registrations;
        _billingEngine = billingEngine;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Gives the student the place: the student must be registered for the semester, of the
    /// hostel's gender and without another place that semester, and the room must have a free bed.
    /// The place is billed to the student at the room's cost.
    /// </summary>
    public async Task AllocateAsync(HostelAllocation allocation)
    {
        allocation.EnsureStatus(HostelAllocationStatus.Booking);

        var student = await _students.FirstOrDefaultAsync(s => s.No == allocation.StudentNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", allocation.StudentNo);

        if (!student.IsActive)
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentNotActive).WithData("studentNo", student.No).WithData("status", student.Status);
        }

        var (studentNo, semester, id) = (student.No, allocation.SemesterCode, allocation.Id);
        if (!await _registrations.AnyAsync(r => r.StudentNo == studentNo && r.SemesterCode == semester && r.Status == RegistrationStatus.Submitted))
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentNotRegisteredForSemester).WithData("studentNo", studentNo).WithData("semester", semester);
        }

        var other = await _allocations.FirstOrDefaultAsync(a => a.Id != id && a.StudentNo == studentNo && a.SemesterCode == semester && a.Status == HostelAllocationStatus.Allocated);
        if (other != null)
        {
            throw new BusinessException(ErpErrorCodes.Academics.StudentAlreadyAllocated)
                .WithData("studentNo", studentNo)
                .WithData("semester", semester)
                .WithData("documentNo", other.No);
        }

        var (hostel, room) = await GetRoomAsync(allocation.HostelCode, allocation.RoomNo);

        if (hostel.Gender != StudentGender.None && student.Gender != hostel.Gender)
        {
            throw new BusinessException(ErpErrorCodes.Academics.HostelGenderMismatch).WithData("hostel", hostel.Code).WithData("studentNo", studentNo);
        }

        if (hostel.Blocked || room.OutOfOrder)
        {
            throw new BusinessException(ErpErrorCodes.Academics.RoomUnavailable).WithData("hostel", hostel.Code).WithData("room", room.RoomNo);
        }

        if (room.VacantSpaces == 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.RoomFull).WithData("hostel", hostel.Code).WithData("room", room.RoomNo);
        }

        var charge = room.RoomCost > 0m ? room.RoomCost : hostel.CostPerOccupant;
        string billNo = null;
        if (charge > 0m)
        {
            if (hostel.FeeItemCode.IsNullOrWhiteSpace())
            {
                throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", "Fee Item Code").WithData("setup", $"Hostel {hostel.Code}");
            }

            var description = $"Accommodation {hostel.Code} {room.RoomNo} {semester}";
            var bill = await _billingEngine.ChargeAsync(student, allocation.AllocationDate, semester, description, [new StudentCharge(hostel.FeeItemCode, description, charge)]);
            billNo = bill.No;
        }

        room.OccupiedSpaces++;
        await _rooms.UpdateAsync(room, autoSave: true);

        allocation.MarkAllocated(charge, billNo, _currentUser.UserName);
        await _allocations.UpdateAsync(allocation, autoSave: true);
    }

    /// <summary>Clears the student out of the room, which frees the bed space.</summary>
    public async Task ClearAsync(HostelAllocation allocation, DateTime clearanceDate)
    {
        allocation.EnsureStatus(HostelAllocationStatus.Allocated);

        var (_, room) = await GetRoomAsync(allocation.HostelCode, allocation.RoomNo);
        room.OccupiedSpaces = Math.Max(0, room.OccupiedSpaces - 1);
        await _rooms.UpdateAsync(room, autoSave: true);

        allocation.MarkCleared(clearanceDate, _currentUser.UserName);
        await _allocations.UpdateAsync(allocation, autoSave: true);
    }

    public async Task<(Hostel Hostel, HostelRoom Room)> GetRoomAsync(string hostelCode, string roomNo)
    {
        var hostel = await _hostels.FirstOrDefaultAsync(h => h.Code == hostelCode)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Hostel").WithData("code", hostelCode ?? string.Empty);

        var room = await _rooms.FirstOrDefaultAsync(r => r.HostelCode == hostelCode && r.RoomNo == roomNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Hostel Room").WithData("code", $"{hostelCode} {roomNo}");

        return (hostel, room);
    }
}
