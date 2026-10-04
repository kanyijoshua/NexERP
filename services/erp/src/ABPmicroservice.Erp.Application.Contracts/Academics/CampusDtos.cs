using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Academics;

/// <summary>The list filter of the campus documents that are not about one student only.</summary>
public class GetCampusDocumentListInput : ErpPagedListInput
{
    /// <summary>Matches the document number, the person or what the document is for.</summary>
    public string Filter { get; set; }
}

// ---------------------------------------------------------------------------- Timetable

public class LectureRoomDto : CodeTableDto
{
    public RoomType RoomType { get; set; }
    public string BuildingCode { get; set; }
    public int MaximumCapacity { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateLectureRoomDto : CreateUpdateCodeTableDto
{
    public RoomType RoomType { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BuildingCode { get; set; }

    [Range(0, 100000)]
    public int MaximumCapacity { get; set; }

    public bool Blocked { get; set; }
}

public interface ILectureRoomAppService
    : ICrudAppService<LectureRoomDto, Guid, GetCodeTableListInput, CreateUpdateLectureRoomDto, CreateUpdateLectureRoomDto> { }

public class TimetableEntryDto : FullAuditedEntityDto<Guid>
{
    public string SemesterCode { get; set; }
    public string AcademicYearCode { get; set; }
    public TimetableType TimetableType { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string UnitCode { get; set; }
    public string UnitDescription { get; set; }
    public TimetableDay Day { get; set; }
    public DateTime? ExamDate { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string RoomCode { get; set; }
    public string LecturerNo { get; set; }
    public string Remarks { get; set; }
}

public class CreateUpdateTimetableEntryDto
{
    /// <summary>Blank takes the current semester.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SemesterCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    public TimetableType TimetableType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProgrammeCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string UnitCode { get; set; }

    public TimetableDay Day { get; set; } = TimetableDay.Monday;

    /// <summary>Required on an exam timetable.</summary>
    public DateTime? ExamDate { get; set; }

    [Required]
    [StringLength(8)]
    public string StartTime { get; set; }

    [Required]
    [StringLength(8)]
    public string EndTime { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string RoomCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string LecturerNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Remarks { get; set; }
}

public class GetTimetableEntryListInput : ErpPagedListInput
{
    /// <summary>Matches the unit, the room or the lecturer.</summary>
    public string Filter { get; set; }

    public string SemesterCode { get; set; }
    public string ProgrammeCode { get; set; }
    public TimetableType? TimetableType { get; set; }
}

public interface ITimetableEntryAppService
    : ICrudAppService<TimetableEntryDto, Guid, GetTimetableEntryListInput, CreateUpdateTimetableEntryDto, CreateUpdateTimetableEntryDto> { }

// ---------------------------------------------------------------------------- Class attendance

public class AttendanceRegisterDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string SemesterCode { get; set; }
    public string AcademicYearCode { get; set; }
    public string UnitCode { get; set; }
    public string UnitDescription { get; set; }
    public string LecturerNo { get; set; }
    public DateTime LessonDate { get; set; }
    public string StartTime { get; set; }
    public decimal Hours { get; set; }
    public string Remarks { get; set; }
    public AcademicDocumentStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public int NoOfStudents { get; set; }
    public int NoPresent { get; set; }
}

public class CreateUpdateAttendanceRegisterDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Attendance Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProgrammeCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string UnitCode { get; set; }

    /// <summary>Blank takes the current semester.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SemesterCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string LecturerNo { get; set; }

    public DateTime LessonDate { get; set; }

    [StringLength(8)]
    public string StartTime { get; set; }

    [Range(0, 24)]
    public decimal Hours { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Remarks { get; set; }
}

public interface IAttendanceRegisterAppService
    : ICrudAppService<AttendanceRegisterDto, Guid, GetCampusDocumentListInput, CreateUpdateAttendanceRegisterDto, CreateUpdateAttendanceRegisterDto>
{
    /// <summary>Adds every student registered for the unit. Routed as POST /api/erp/attendance-register/{id}/suggest-lines.</summary>
    Task<AttendanceRegisterDto> SuggestLinesAsync(Guid id);

    /// <summary>Routed as POST /api/erp/attendance-register/{id}/run-posting.</summary>
    Task<AttendanceRegisterDto> RunPostingAsync(Guid id);
}

public class AttendanceLineDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public AttendanceMark Mark { get; set; }
}

public class CreateUpdateAttendanceLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>0 puts the line after the last one.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    public AttendanceMark Mark { get; set; }
}

public interface IAttendanceLineAppService
    : ICrudAppService<AttendanceLineDto, Guid, GetDocumentLineListInput, CreateUpdateAttendanceLineDto, CreateUpdateAttendanceLineDto> { }

// ---------------------------------------------------------------------------- Hostels

public class HostelDto : CodeTableDto
{
    public StudentGender Gender { get; set; }
    public decimal CostPerOccupant { get; set; }
    public string FeeItemCode { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateHostelDto : CreateUpdateCodeTableDto
{
    public StudentGender Gender { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CostPerOccupant { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string FeeItemCode { get; set; }

    public bool Blocked { get; set; }
}

public interface IHostelAppService : ICrudAppService<HostelDto, Guid, GetCodeTableListInput, CreateUpdateHostelDto, CreateUpdateHostelDto> { }

public class HostelRoomDto : FullAuditedEntityDto<Guid>
{
    public string HostelCode { get; set; }
    public string RoomNo { get; set; }
    public int BedSpaces { get; set; }
    public decimal RoomCost { get; set; }
    public bool OutOfOrder { get; set; }
    public int OccupiedSpaces { get; set; }
    public int VacantSpaces { get; set; }
}

public class CreateUpdateHostelRoomDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string HostelCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string RoomNo { get; set; }

    [Range(0, 1000)]
    public int BedSpaces { get; set; }

    [Range(0, double.MaxValue)]
    public decimal RoomCost { get; set; }

    public bool OutOfOrder { get; set; }
}

public class GetHostelRoomListInput : ErpPagedListInput
{
    /// <summary>Matches the hostel or the room.</summary>
    public string Filter { get; set; }

    public string HostelCode { get; set; }

    /// <summary>Only rooms with a free bed space.</summary>
    public bool? VacantOnly { get; set; }
}

public interface IHostelRoomAppService
    : ICrudAppService<HostelRoomDto, Guid, GetHostelRoomListInput, CreateUpdateHostelRoomDto, CreateUpdateHostelRoomDto> { }

public class HostelAllocationDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public string HostelCode { get; set; }
    public string RoomNo { get; set; }
    public string SemesterCode { get; set; }
    public DateTime AllocationDate { get; set; }
    public string Remarks { get; set; }
    public HostelAllocationStatus Status { get; set; }
    public decimal Charges { get; set; }
    public string BillNo { get; set; }
    public DateTime? ClearanceDate { get; set; }
    public string ProcessedBy { get; set; }
}

public class CreateUpdateHostelAllocationDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Hostel Allocation Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string HostelCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string RoomNo { get; set; }

    /// <summary>Blank takes the current semester.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SemesterCode { get; set; }

    public DateTime AllocationDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Remarks { get; set; }
}

public interface IHostelAllocationAppService
    : ICrudAppService<HostelAllocationDto, Guid, GetStudentDocumentListInput, CreateUpdateHostelAllocationDto, CreateUpdateHostelAllocationDto>
{
    /// <summary>Gives the student the place and bills it. Routed as POST /api/erp/hostel-allocation/{id}/allocate.</summary>
    Task<HostelAllocationDto> AllocateAsync(Guid id);

    /// <summary>Clears the student out of the room. Routed as POST /api/erp/hostel-allocation/{id}/clear.</summary>
    Task<HostelAllocationDto> ClearAsync(Guid id);
}

// ---------------------------------------------------------------------------- Infirmary

public class ClinicVisitDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public PatientType PatientType { get; set; }
    public string PatientNo { get; set; }
    public string PatientName { get; set; }
    public DateTime VisitDate { get; set; }
    public TreatmentType TreatmentType { get; set; }
    public string Complaint { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string AttendedBy { get; set; }
    public string ReferredTo { get; set; }
    public DateTime? OffDutyFrom { get; set; }
    public DateTime? OffDutyTo { get; set; }
    public int OffDutyDays { get; set; }
    public int LightDutyDays { get; set; }
    public string OffDutyComments { get; set; }
    public decimal Charge { get; set; }
    public ClinicVisitStatus Status { get; set; }
    public string BillNo { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string CompletedBy { get; set; }
}

public class CreateUpdateClinicVisitDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Clinic Visit Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    public PatientType PatientType { get; set; }

    /// <summary>The student or employee number; the name is taken from the record.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PatientNo { get; set; }

    /// <summary>Required for a patient who is neither a student nor an employee.</summary>
    [StringLength(ErpDomainConsts.MaxNameLength * 2)]
    public string PatientName { get; set; }

    public DateTime VisitDate { get; set; }
    public TreatmentType TreatmentType { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Complaint { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Diagnosis { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Treatment { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string AttendedBy { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ReferredTo { get; set; }

    public DateTime? OffDutyFrom { get; set; }
    public DateTime? OffDutyTo { get; set; }

    [Range(0, 365)]
    public int LightDutyDays { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string OffDutyComments { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Charge { get; set; }
}

public interface IClinicVisitAppService
    : ICrudAppService<ClinicVisitDto, Guid, GetCampusDocumentListInput, CreateUpdateClinicVisitDto, CreateUpdateClinicVisitDto>
{
    /// <summary>Issues the drugs and bills a student. Routed as POST /api/erp/clinic-visit/{id}/complete.</summary>
    Task<ClinicVisitDto> CompleteAsync(Guid id);
}

public class ClinicPrescriptionDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string Dosage { get; set; }
    public string LocationCode { get; set; }
    public bool Issued { get; set; }
}

public class CreateUpdateClinicPrescriptionDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    public int LineNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Dosage { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string LocationCode { get; set; }
}

public interface IClinicPrescriptionAppService
    : ICrudAppService<ClinicPrescriptionDto, Guid, GetDocumentLineListInput, CreateUpdateClinicPrescriptionDto, CreateUpdateClinicPrescriptionDto> { }

// ---------------------------------------------------------------------------- Laundry

public class LaundryItemDto : CodeTableDto
{
    public decimal RatePerItem { get; set; }
    public string GLAccountNo { get; set; }
}

public class CreateUpdateLaundryItemDto : CreateUpdateCodeTableDto
{
    [Range(0, double.MaxValue)]
    public decimal RatePerItem { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string GLAccountNo { get; set; }
}

public interface ILaundryItemAppService
    : ICrudAppService<LaundryItemDto, Guid, GetCodeTableListInput, CreateUpdateLaundryItemDto, CreateUpdateLaundryItemDto> { }

public class LaundryOrderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public DateTime ReceivedDate { get; set; }
    public DateTime? PromisedDate { get; set; }
    public bool Express { get; set; }
    public decimal DiscountPct { get; set; }
    public string Remarks { get; set; }
    public LaundryStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? InvoicedDate { get; set; }
    public DateTime? CollectedDate { get; set; }
    public string ProcessedBy { get; set; }
}

public class CreateUpdateLaundryOrderDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Laundry Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    /// <summary>A student's account has the student's number.</summary>
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    public DateTime ReceivedDate { get; set; }
    public DateTime? PromisedDate { get; set; }
    public bool Express { get; set; }

    [Range(0, 100)]
    public decimal DiscountPct { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Remarks { get; set; }
}

public interface ILaundryOrderAppService
    : ICrudAppService<LaundryOrderDto, Guid, GetCampusDocumentListInput, CreateUpdateLaundryOrderDto, CreateUpdateLaundryOrderDto>
{
    /// <summary>Invoices the order to the customer. Routed as POST /api/erp/laundry-order/{id}/invoice.</summary>
    Task<LaundryOrderDto> InvoiceAsync(Guid id);

    /// <summary>Routed as POST /api/erp/laundry-order/{id}/mark-ready.</summary>
    Task<LaundryOrderDto> MarkReadyAsync(Guid id);

    /// <summary>Routed as POST /api/erp/laundry-order/{id}/mark-collected.</summary>
    Task<LaundryOrderDto> MarkCollectedAsync(Guid id);
}

public class LaundryOrderLineDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string LaundryItemCode { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public class CreateUpdateLaundryOrderLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string LaundryItemCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; } = 1;

    /// <summary>Blank takes the item's rate.</summary>
    [Range(0, double.MaxValue)]
    public decimal? UnitPrice { get; set; }
}

public interface ILaundryOrderLineAppService
    : ICrudAppService<LaundryOrderLineDto, Guid, GetDocumentLineListInput, CreateUpdateLaundryOrderLineDto, CreateUpdateLaundryOrderLineDto> { }

// ---------------------------------------------------------------------------- Short courses

public class ShortCourseDto : CodeTableDto
{
    public int DurationDays { get; set; }
    public decimal FeePerParticipant { get; set; }
    public string GLAccountNo { get; set; }
    public int MaxParticipants { get; set; }
    public bool Active { get; set; }
}

public class CreateUpdateShortCourseDto : CreateUpdateCodeTableDto
{
    [Range(0, 3650)]
    public int DurationDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal FeePerParticipant { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string GLAccountNo { get; set; }

    [Range(0, 100000)]
    public int MaxParticipants { get; set; }

    public bool Active { get; set; } = true;
}

public interface IShortCourseAppService
    : ICrudAppService<ShortCourseDto, Guid, GetCodeTableListInput, CreateUpdateShortCourseDto, CreateUpdateShortCourseDto> { }

public class ShortCourseApplicationDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public ShortCourseApplicationType ApplicationType { get; set; }
    public string CourseCode { get; set; }
    public string CourseDescription { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public decimal FeePerParticipant { get; set; }
    public string Remarks { get; set; }
    public string RejectionReason { get; set; }
    public ShortCourseApplicationStatus Status { get; set; }
    public int NoOfParticipants { get; set; }
    public decimal BilledAmount { get; set; }
    public DateTime? RegisteredDate { get; set; }
    public string ProcessedBy { get; set; }
}

public class CreateUpdateShortCourseApplicationDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Short Course Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    public ShortCourseApplicationType ApplicationType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CourseCode { get; set; }

    public DateTime ApplicationDate { get; set; }
    public DateTime StartDate { get; set; }

    /// <summary>Blank runs the course for its duration.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>The sponsor of a corporate application.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [Range(0, double.MaxValue)]
    public decimal FeePerParticipant { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Remarks { get; set; }
}

public class RejectShortCourseApplicationInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Reason { get; set; }
}

public interface IShortCourseApplicationAppService
    : ICrudAppService<ShortCourseApplicationDto, Guid, GetCampusDocumentListInput, CreateUpdateShortCourseApplicationDto, CreateUpdateShortCourseApplicationDto>
{
    /// <summary>Routed as POST /api/erp/short-course-application/{id}/submit.</summary>
    Task<ShortCourseApplicationDto> SubmitAsync(Guid id);

    /// <summary>Routed as POST /api/erp/short-course-application/{id}/approve.</summary>
    Task<ShortCourseApplicationDto> ApproveAsync(Guid id);

    /// <summary>Routed as POST /api/erp/short-course-application/{id}/reject.</summary>
    Task<ShortCourseApplicationDto> RejectAsync(Guid id, RejectShortCourseApplicationInput input);

    /// <summary>Routed as POST /api/erp/short-course-application/{id}/reopen.</summary>
    Task<ShortCourseApplicationDto> ReopenAsync(Guid id);

    /// <summary>Bills the fee and gives certificate numbers. Routed as POST /api/erp/short-course-application/{id}/register.</summary>
    Task<ShortCourseApplicationDto> RegisterAsync(Guid id);
}

public class ShortCourseParticipantDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string Name { get; set; }
    public string NationalId { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string CertificateNo { get; set; }
}

public class CreateUpdateShortCourseParticipantDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength * 2)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength * 2)]
    public string NationalId { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }
}

public interface IShortCourseParticipantAppService
    : ICrudAppService<ShortCourseParticipantDto, Guid, GetDocumentLineListInput, CreateUpdateShortCourseParticipantDto, CreateUpdateShortCourseParticipantDto> { }
