using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Academics;

// ---------------------------------------------------------------------------- Setup

public class AcademicSetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ApplicationNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string StudentNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string RegistrationNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string BillingNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ExamResultNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ReceiptNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string RefundNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string StatusChangeNos { get; set; }

    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string StudentPostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string StudentGenBusPostingGroup { get; set; }

    public bool CheckStudentBalance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MaxFeeBalanceToRegister { get; set; }

    public bool BillOnRegistration { get; set; } = true;

    [Range(0, 2)]
    public int ExamRoundingDecimals { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string AttendanceNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string HostelAllocationNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ClinicVisitNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string LaundryNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ShortCourseNos { get; set; }

    [Range(0, 100)]
    public decimal MinAttendancePct { get; set; }

    [Range(0, 100)]
    public decimal LaundryExpressChargePct { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string MedicalFeeItemCode { get; set; }
}

public interface IAcademicSetupAppService : IApplicationService
{
    Task<AcademicSetupDto> GetAsync();

    Task<AcademicSetupDto> UpdateAsync(AcademicSetupDto input);
}

// ---------------------------------------------------------------------------- Calendar

public class AcademicYearDto : CodeTableDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Current { get; set; }
}

public class CreateUpdateAcademicYearDto : CreateUpdateCodeTableDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>Marking a year current takes the mark off the year that had it.</summary>
    public bool Current { get; set; }
}

public interface IAcademicYearAppService
    : ICrudAppService<AcademicYearDto, Guid, GetCodeTableListInput, CreateUpdateAcademicYearDto, CreateUpdateAcademicYearDto> { }

public class SemesterDto : CodeTableDto
{
    public string AcademicYearCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RegistrationFrom { get; set; }
    public DateTime? RegistrationTo { get; set; }
    public bool Current { get; set; }
}

public class CreateUpdateSemesterDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RegistrationFrom { get; set; }
    public DateTime? RegistrationTo { get; set; }
    public bool Current { get; set; }
}

public interface ISemesterAppService
    : ICrudAppService<SemesterDto, Guid, GetCodeTableListInput, CreateUpdateSemesterDto, CreateUpdateSemesterDto> { }

public class IntakeDto : CodeTableDto
{
    public string AcademicYearCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Current { get; set; }
}

public class CreateUpdateIntakeDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string AcademicYearCode { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool Current { get; set; }
}

public interface IIntakeAppService
    : ICrudAppService<IntakeDto, Guid, GetCodeTableListInput, CreateUpdateIntakeDto, CreateUpdateIntakeDto> { }

// ---------------------------------------------------------------------------- Grading

public class ExamCategoryDto : CodeTableDto
{
    public bool BlockResultsEntry { get; set; }
}

public class CreateUpdateExamCategoryDto : CreateUpdateCodeTableDto
{
    public bool BlockResultsEntry { get; set; }
}

public interface IExamCategoryAppService
    : ICrudAppService<ExamCategoryDto, Guid, GetCodeTableListInput, CreateUpdateExamCategoryDto, CreateUpdateExamCategoryDto> { }

public class GradingBandDto : FullAuditedEntityDto<Guid>
{
    public string ExamCategoryCode { get; set; }
    public string Grade { get; set; }
    public string Description { get; set; }
    public decimal FromMark { get; set; }
    public decimal ToMark { get; set; }
    public decimal Points { get; set; }
    public string Remarks { get; set; }
    public bool Passed { get; set; }
}

public class CreateUpdateGradingBandDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ExamCategoryCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Grade { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Range(0, 100)]
    public decimal FromMark { get; set; }

    [Range(0, 100)]
    public decimal ToMark { get; set; }

    [Range(0, 100)]
    public decimal Points { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Remarks { get; set; }

    public bool Passed { get; set; } = true;
}

public class GetExamCategoryTableListInput : ErpPagedListInput
{
    /// <summary>Matches the exam category code.</summary>
    public string Filter { get; set; }
    public string ExamCategoryCode { get; set; }
}

public interface IGradingBandAppService
    : ICrudAppService<GradingBandDto, Guid, GetExamCategoryTableListInput, CreateUpdateGradingBandDto, CreateUpdateGradingBandDto> { }

public class ExamComponentDto : FullAuditedEntityDto<Guid>
{
    public string ExamCategoryCode { get; set; }
    public ExamType ExamType { get; set; }
    public string Description { get; set; }
    public decimal MaxScore { get; set; }
    public decimal ContributionPct { get; set; }
}

public class CreateUpdateExamComponentDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ExamCategoryCode { get; set; }

    public ExamType ExamType { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Range(0.01, 1000)]
    public decimal MaxScore { get; set; } = 100m;

    [Range(0, 100)]
    public decimal ContributionPct { get; set; }
}

public interface IExamComponentAppService
    : ICrudAppService<ExamComponentDto, Guid, GetExamCategoryTableListInput, CreateUpdateExamComponentDto, CreateUpdateExamComponentDto> { }

// ---------------------------------------------------------------------------- Programmes

public class ProgrammeDto : CodeTableDto
{
    public ProgrammeLevel Level { get; set; }
    public string ExamCategoryCode { get; set; }
    public int DurationMonths { get; set; }
    public int MinimumCapacity { get; set; }
    public int MaximumCapacity { get; set; }
    public string StudentNos { get; set; }
    public string StudentNoPrefix { get; set; }
    public string StudentNoSuffix { get; set; }
    public bool Active { get; set; }
}

public class CreateUpdateProgrammeDto : CreateUpdateCodeTableDto
{
    public ProgrammeLevel Level { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ExamCategoryCode { get; set; }

    [Range(0, 240)]
    public int DurationMonths { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumCapacity { get; set; }

    [Range(0, int.MaxValue)]
    public int MaximumCapacity { get; set; }

    /// <summary>Blank numbers the programme's students from the Academic Setup's Student Nos.</summary>
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string StudentNos { get; set; }

    [StringLength(5)]
    public string StudentNoPrefix { get; set; }

    [StringLength(5)]
    public string StudentNoSuffix { get; set; }

    public bool Active { get; set; } = true;
}

public interface IProgrammeAppService
    : ICrudAppService<ProgrammeDto, Guid, GetCodeTableListInput, CreateUpdateProgrammeDto, CreateUpdateProgrammeDto> { }

public class ProgrammeStageDto : FullAuditedEntityDto<Guid>
{
    public string ProgrammeCode { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public int Sequence { get; set; }
    public bool FinalStage { get; set; }
    public int MinimumUnits { get; set; }
    public int MaximumUnits { get; set; }
}

public class CreateUpdateProgrammeStageDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProgrammeCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public int Sequence { get; set; }
    public bool FinalStage { get; set; }

    [Range(0, 100)]
    public int MinimumUnits { get; set; }

    [Range(0, 100)]
    public int MaximumUnits { get; set; }
}

public class GetProgrammeTableListInput : ErpPagedListInput
{
    /// <summary>Matches the programme code, the record's own code or its description.</summary>
    public string Filter { get; set; }
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
}

public interface IProgrammeStageAppService
    : ICrudAppService<ProgrammeStageDto, Guid, GetProgrammeTableListInput, CreateUpdateProgrammeStageDto, CreateUpdateProgrammeStageDto> { }

public class CourseUnitDto : FullAuditedEntityDto<Guid>
{
    public string ProgrammeCode { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string StageCode { get; set; }
    public string SemesterCode { get; set; }
    public UnitType UnitType { get; set; }
    public decimal CreditHours { get; set; }
    public string PrerequisiteUnitCode { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateCourseUnitDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProgrammeCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string StageCode { get; set; }

    /// <summary>Blank teaches the unit in every semester of its stage.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SemesterCode { get; set; }

    public UnitType UnitType { get; set; }

    [Range(0, 1000)]
    public decimal CreditHours { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PrerequisiteUnitCode { get; set; }

    public bool Blocked { get; set; }
}

public interface ICourseUnitAppService
    : ICrudAppService<CourseUnitDto, Guid, GetProgrammeTableListInput, CreateUpdateCourseUnitDto, CreateUpdateCourseUnitDto> { }

// ---------------------------------------------------------------------------- Fees

public class FeeItemDto : CodeTableDto
{
    public FeeType FeeType { get; set; }
    public string GLAccountNo { get; set; }
    public decimal DefaultAmount { get; set; }
    public bool TuitionFee { get; set; }
}

public class CreateUpdateFeeItemDto : CreateUpdateCodeTableDto
{
    public FeeType FeeType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string GLAccountNo { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DefaultAmount { get; set; }

    public bool TuitionFee { get; set; }
}

public interface IFeeItemAppService
    : ICrudAppService<FeeItemDto, Guid, GetCodeTableListInput, CreateUpdateFeeItemDto, CreateUpdateFeeItemDto> { }

public class FeeStructureLineDto : FullAuditedEntityDto<Guid>
{
    public string ProgrammeCode { get; set; }
    public string StageCode { get; set; }
    public string SemesterCode { get; set; }
    public StudyMode StudyMode { get; set; }
    public string FeeItemCode { get; set; }
    public decimal Amount { get; set; }
}

public class CreateUpdateFeeStructureLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProgrammeCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string StageCode { get; set; }

    /// <summary>Blank charges the fee in every semester of the stage.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SemesterCode { get; set; }

    /// <summary>None charges the fee whatever the student's mode of study.</summary>
    public StudyMode StudyMode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string FeeItemCode { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
}

public interface IFeeStructureLineAppService
    : ICrudAppService<FeeStructureLineDto, Guid, GetProgrammeTableListInput, CreateUpdateFeeStructureLineDto, CreateUpdateFeeStructureLineDto> { }
