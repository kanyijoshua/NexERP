using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Academics;

// ---------------------------------------------------------------------------- Applications

public class StudentApplicationDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string FirstName { get; set; }
    public string OtherName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public StudentGender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string NationalId { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ProgrammeCode { get; set; }
    public string IntakeCode { get; set; }
    public string AcademicYearCode { get; set; }
    public StudyMode StudyMode { get; set; }
    public string FormerSchool { get; set; }
    public string IndexNumber { get; set; }
    public string MeanGrade { get; set; }
    public StudentSponsorship Sponsorship { get; set; }
    public string GuardianName { get; set; }
    public string GuardianPhoneNo { get; set; }
    public ApplicationStatus Status { get; set; }
    public string RejectionReason { get; set; }
    public string StudentNo { get; set; }
    public DateTime? AdmissionDate { get; set; }
}

/// <summary>What an application and a student card share: who the person is and how to reach them.</summary>
public abstract class PersonInputDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string FirstName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string OtherName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string LastName { get; set; }

    public StudentGender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength * 2)]
    public string NationalId { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxAddressLength)]
    public string Address { get; set; }

    [StringLength(ErpDomainConsts.MaxCityLength)]
    public string City { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProgrammeCode { get; set; }

    /// <summary>Blank takes the current intake.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string IntakeCode { get; set; }

    /// <summary>Blank takes the current academic year.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    public StudyMode StudyMode { get; set; } = StudyMode.FullTime;

    public StudentSponsorship Sponsorship { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string GuardianName { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string GuardianPhoneNo { get; set; }
}

public class CreateUpdateStudentApplicationDto : PersonInputDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Application Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    public DateTime ApplicationDate { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string FormerSchool { get; set; }

    [StringLength(ErpDomainConsts.MaxExternalDocumentNoLength)]
    public string IndexNumber { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string MeanGrade { get; set; }
}

public class GetStudentApplicationListInput : ErpPagedListInput
{
    /// <summary>Matches the application number, the name, the national ID or the programme.</summary>
    public string Filter { get; set; }
    public string ProgrammeCode { get; set; }
    public string IntakeCode { get; set; }
    public ApplicationStatus? Status { get; set; }
}

public class RejectApplicationInput
{
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Reason { get; set; }
}

public class AdmitApplicationInput
{
    /// <summary>Blank admits as at today.</summary>
    public DateTime? AdmissionDate { get; set; }
}

public interface IStudentApplicationAppService
    : ICrudAppService<StudentApplicationDto, Guid, GetStudentApplicationListInput, CreateUpdateStudentApplicationDto, CreateUpdateStudentApplicationDto>
{
    Task<StudentApplicationDto> SubmitAsync(Guid id);

    Task<StudentApplicationDto> ApproveAsync(Guid id);

    Task<StudentApplicationDto> RejectAsync(Guid id, RejectApplicationInput input);

    Task<StudentApplicationDto> ReopenAsync(Guid id);

    /// <summary>Turns the applicant into a student. Routed as POST /api/erp/student-application/{id}/admit.</summary>
    Task<StudentApplicationDto> AdmitAsync(Guid id, AdmitApplicationInput input);
}

// ---------------------------------------------------------------------------- Students

public class StudentDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string FirstName { get; set; }
    public string OtherName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public StudentGender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string NationalId { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ProgrammeCode { get; set; }
    public string CurrentStageCode { get; set; }
    public string CurrentSemesterCode { get; set; }
    public string IntakeCode { get; set; }
    public string AcademicYearCode { get; set; }
    public StudyMode StudyMode { get; set; }
    public StudentStatus Status { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string CustomerNo { get; set; }
    public string ApplicationNo { get; set; }
    public StudentSponsorship Sponsorship { get; set; }
    public string GuardianName { get; set; }
    public string GuardianPhoneNo { get; set; }
}

public class CreateUpdateStudentDto : PersonInputDto
{
    /// <summary>Blank takes the next number of the programme's series, or of the Academic Setup's Student Nos.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    public StudentStatus Status { get; set; }
    public DateTime? AdmissionDate { get; set; }
}

public class GetStudentListInput : ErpPagedListInput
{
    /// <summary>Matches the student number, the name, the national ID, the programme or the stage.</summary>
    public string Filter { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string IntakeCode { get; set; }
    public StudentStatus? Status { get; set; }
}

/// <summary>What a student owes: the balance of the student's customer account.</summary>
public class StudentBalanceDto
{
    public string StudentNo { get; set; }
    public string CustomerNo { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal Balance { get; set; }

    /// <summary>What the student has paid ahead: the account's credit balance, zero when the student owes.</summary>
    public decimal Prepayment { get; set; }
}

public class TranscriptLineDto
{
    public string AcademicYearCode { get; set; }
    public string SemesterCode { get; set; }
    public string StageCode { get; set; }
    public string UnitCode { get; set; }
    public string UnitDescription { get; set; }
    public decimal CreditHours { get; set; }
    public decimal FinalScore { get; set; }
    public string Grade { get; set; }
    public decimal Points { get; set; }
    public string ResultRemarks { get; set; }
    public bool Passed { get; set; }
}

/// <summary>Every graded unit of a student, and the means over them.</summary>
public class StudentTranscriptDto
{
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public string ProgrammeCode { get; set; }
    public int UnitsTaken { get; set; }
    public int UnitsPassed { get; set; }
    public decimal MeanScore { get; set; }

    /// <summary>The mean of grade points, weighted by credit hours when the units carry them.</summary>
    public decimal MeanPoints { get; set; }

    public List<TranscriptLineDto> Lines { get; set; } = new();
}

public interface IStudentAppService
    : ICrudAppService<StudentDto, Guid, GetStudentListInput, CreateUpdateStudentDto, CreateUpdateStudentDto>
{
    /// <summary>Routed as GET /api/erp/student/{id}/balance.</summary>
    Task<StudentBalanceDto> GetBalanceAsync(Guid id);

    /// <summary>Routed as GET /api/erp/student/{id}/transcript.</summary>
    Task<StudentTranscriptDto> GetTranscriptAsync(Guid id);
}

// ---------------------------------------------------------------------------- Registration

public class SemesterRegistrationDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string SemesterCode { get; set; }
    public string AcademicYearCode { get; set; }
    public DateTime RegistrationDate { get; set; }
    public RegisterFor RegisterFor { get; set; }
    public string Remarks { get; set; }
    public RegistrationStatus Status { get; set; }
    public int NoOfUnits { get; set; }
    public string BillNo { get; set; }
    public decimal BilledAmount { get; set; }
}

public class CreateUpdateSemesterRegistrationDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Registration Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string StageCode { get; set; }

    /// <summary>Blank takes the current semester.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SemesterCode { get; set; }

    /// <summary>Blank takes the semester's academic year.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    public DateTime RegistrationDate { get; set; }

    public RegisterFor RegisterFor { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Remarks { get; set; }
}

public class GetSemesterRegistrationListInput : ErpPagedListInput
{
    /// <summary>Matches the registration number, the student number or name, the programme or the semester.</summary>
    public string Filter { get; set; }
    public string StudentNo { get; set; }
    public string SemesterCode { get; set; }
    public RegistrationStatus? Status { get; set; }
}

public interface ISemesterRegistrationAppService
    : ICrudAppService<SemesterRegistrationDto, Guid, GetSemesterRegistrationListInput, CreateUpdateSemesterRegistrationDto, CreateUpdateSemesterRegistrationDto>
{
    /// <summary>Adds every unit of the stage taught in the semester. Routed as POST /api/erp/semester-registration/{id}/fill-units.</summary>
    Task<SemesterRegistrationDto> FillUnitsAsync(Guid id);

    Task<SemesterRegistrationDto> SubmitAsync(Guid id);
}

public class StudentUnitDto : FullAuditedEntityDto<Guid>
{
    public string RegistrationNo { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string SemesterCode { get; set; }
    public string AcademicYearCode { get; set; }
    public string UnitCode { get; set; }
    public string UnitDescription { get; set; }
    public UnitType UnitType { get; set; }
    public decimal CreditHours { get; set; }
    public decimal? AssignmentMark { get; set; }
    public decimal? CatMark { get; set; }
    public decimal? Cat2Mark { get; set; }
    public decimal? ExamMark { get; set; }
    public decimal FinalScore { get; set; }
    public string Grade { get; set; }
    public decimal Points { get; set; }
    public string ResultRemarks { get; set; }
    public bool Passed { get; set; }
    public int SessionsHeld { get; set; }
    public int SessionsAttended { get; set; }
    public decimal AttendancePct { get; set; }
}

public class CreateUpdateStudentUnitDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string RegistrationNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string UnitCode { get; set; }
}

public class GetStudentUnitListInput : ErpPagedListInput
{
    /// <summary>Matches the registration number, the student number or name, or the unit.</summary>
    public string Filter { get; set; }
    public string RegistrationNo { get; set; }
    public string StudentNo { get; set; }
    public string UnitCode { get; set; }
    public string SemesterCode { get; set; }
}

public interface IStudentUnitAppService
    : ICrudAppService<StudentUnitDto, Guid, GetStudentUnitListInput, CreateUpdateStudentUnitDto, CreateUpdateStudentUnitDto> { }

// ---------------------------------------------------------------------------- Billing

public class StudentBillHeaderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public DateTime PostingDate { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string SemesterCode { get; set; }
    public string AcademicYearCode { get; set; }
    public string Description { get; set; }
    public string RegistrationNo { get; set; }
    public AcademicDocumentStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreateUpdateStudentBillHeaderDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Billing Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    public DateTime PostingDate { get; set; }

    /// <summary>Blank takes the student's current stage.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string StageCode { get; set; }

    /// <summary>Blank takes the student's current semester.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SemesterCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GetStudentBillListInput : ErpPagedListInput
{
    /// <summary>Matches the bill number, the student number or name, or the description.</summary>
    public string Filter { get; set; }
    public string StudentNo { get; set; }
    public AcademicDocumentStatus? Status { get; set; }
}

public interface IStudentBillAppService
    : ICrudAppService<StudentBillHeaderDto, Guid, GetStudentBillListInput, CreateUpdateStudentBillHeaderDto, CreateUpdateStudentBillHeaderDto>
{
    /// <summary>Fills the bill from the fee structure. Routed as POST /api/erp/student-bill/{id}/suggest-lines.</summary>
    Task<StudentBillHeaderDto> SuggestLinesAsync(Guid id);

    /// <summary>Routed as POST /api/erp/student-bill/{id}/run-posting.</summary>
    Task<StudentBillHeaderDto> RunPostingAsync(Guid id);
}

public class StudentBillLineDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string FeeItemCode { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

public class CreateUpdateStudentBillLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>0 puts the line after the last one.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string FeeItemCode { get; set; }

    /// <summary>Blank takes the fee item's description.</summary>
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>Left out, the fee item's default amount.</summary>
    [Range(0, double.MaxValue)]
    public decimal? Amount { get; set; }
}

public class GetDocumentLineListInput : ErpPagedListInput
{
    /// <summary>Matches the document number or what the line is for.</summary>
    public string Filter { get; set; }
    public string DocumentNo { get; set; }
}

public interface IStudentBillLineAppService
    : ICrudAppService<StudentBillLineDto, Guid, GetDocumentLineListInput, CreateUpdateStudentBillLineDto, CreateUpdateStudentBillLineDto> { }

// ---------------------------------------------------------------------------- Exam results

public class ExamResultHeaderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string SemesterCode { get; set; }
    public string AcademicYearCode { get; set; }
    public string UnitCode { get; set; }
    public string UnitDescription { get; set; }
    public ExamType ExamType { get; set; }
    public string LecturerNo { get; set; }
    public DateTime DocumentDate { get; set; }
    public AcademicDocumentStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public int NoOfStudents { get; set; }
}

public class CreateUpdateExamResultHeaderDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Exam Result Nos.</summary>
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

    /// <summary>Blank takes the semester's academic year.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    public ExamType ExamType { get; set; } = ExamType.FinalExam;

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string LecturerNo { get; set; }

    public DateTime DocumentDate { get; set; }
}

public class GetExamResultListInput : ErpPagedListInput
{
    /// <summary>Matches the document number, the programme, the unit or the semester.</summary>
    public string Filter { get; set; }
    public string ProgrammeCode { get; set; }
    public string UnitCode { get; set; }
    public AcademicDocumentStatus? Status { get; set; }
}

public interface IExamResultAppService
    : ICrudAppService<ExamResultHeaderDto, Guid, GetExamResultListInput, CreateUpdateExamResultHeaderDto, CreateUpdateExamResultHeaderDto>
{
    /// <summary>Adds a line for every student registered for the unit. Routed as POST /api/erp/exam-result/{id}/suggest-lines.</summary>
    Task<ExamResultHeaderDto> SuggestLinesAsync(Guid id);

    /// <summary>Assigns the marks to the students' units. Routed as POST /api/erp/exam-result/{id}/run-posting.</summary>
    Task<ExamResultHeaderDto> RunPostingAsync(Guid id);
}

public class ExamResultLineDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public decimal Mark { get; set; }
    public bool NotDone { get; set; }
}

public class CreateUpdateExamResultLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>0 puts the line after the last one.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    [Range(0, 1000)]
    public decimal Mark { get; set; }

    public bool NotDone { get; set; }
}

public interface IExamResultLineAppService
    : ICrudAppService<ExamResultLineDto, Guid, GetDocumentLineListInput, CreateUpdateExamResultLineDto, CreateUpdateExamResultLineDto> { }

// ---------------------------------------------------------------------------- Receipts and refunds

public class StudentReceiptDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public DateTime PostingDate { get; set; }
    public string BankAccountNo { get; set; }
    public string PayMode { get; set; }
    public string ExternalDocumentNo { get; set; }
    public decimal Amount { get; set; }
    public string AppliesToBillNo { get; set; }
    public string Description { get; set; }
    public AcademicDocumentStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
}

public class CreateUpdateStudentReceiptDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Receipt Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    public DateTime PostingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BankAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PayMode { get; set; }

    [StringLength(ErpDomainConsts.MaxExternalDocumentNoLength)]
    public string ExternalDocumentNo { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>Blank leaves the whole amount on the student's account as a prepayment.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string AppliesToBillNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GetStudentDocumentListInput : ErpPagedListInput
{
    /// <summary>Matches the document number, the student number or name, or a reference on the document.</summary>
    public string Filter { get; set; }
    public string StudentNo { get; set; }
}

public interface IStudentReceiptAppService
    : ICrudAppService<StudentReceiptDto, Guid, GetStudentDocumentListInput, CreateUpdateStudentReceiptDto, CreateUpdateStudentReceiptDto>
{
    /// <summary>Routed as POST /api/erp/student-receipt/{id}/run-posting.</summary>
    Task<StudentReceiptDto> RunPostingAsync(Guid id);
}

public class StudentRefundDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public DateTime DocumentDate { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; }
    public AcademicRequestStatus Status { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string ApprovedBy { get; set; }
    public string PaymentVoucherNo { get; set; }
}

public class CreateUpdateStudentRefundDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Refund Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    public DateTime DocumentDate { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Reason { get; set; }
}

public interface IStudentRefundAppService
    : ICrudAppService<StudentRefundDto, Guid, GetStudentDocumentListInput, CreateUpdateStudentRefundDto, CreateUpdateStudentRefundDto>
{
    /// <summary>Approves the refund and raises its payment voucher. Routed as POST /api/erp/student-refund/{id}/approve.</summary>
    Task<StudentRefundDto> ApproveAsync(Guid id);
}

// ---------------------------------------------------------------------------- Status changes

public class StudentStatusChangeDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string StudentNo { get; set; }
    public string StudentName { get; set; }
    public StudentChangeType ChangeType { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ResumeDate { get; set; }
    public string Reason { get; set; }
    public AcademicRequestStatus Status { get; set; }
    public StudentStatus PreviousStatus { get; set; }
    public StudentStatus NewStatus { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string ApprovedBy { get; set; }
}

public class CreateUpdateStudentStatusChangeDto
{
    /// <summary>Blank takes the next number of the Academic Setup's Status Change Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string StudentNo { get; set; }

    public StudentChangeType ChangeType { get; set; }

    public DateTime EffectiveDate { get; set; }

    /// <summary>When a deferred or suspended student is expected back.</summary>
    public DateTime? ResumeDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Reason { get; set; }
}

public interface IStudentStatusChangeAppService
    : ICrudAppService<StudentStatusChangeDto, Guid, GetStudentDocumentListInput, CreateUpdateStudentStatusChangeDto, CreateUpdateStudentStatusChangeDto>
{
    /// <summary>Applies the change to the student. Routed as POST /api/erp/student-status-change/{id}/approve.</summary>
    Task<StudentStatusChangeDto> ApproveAsync(Guid id);
}
