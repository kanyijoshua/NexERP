using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.HumanResources;

public class JobVacancyDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Title { get; set; }
    public string Department { get; set; }
    public string Description { get; set; }
    public int Positions { get; set; }
    public int HiredCount { get; set; }
    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public JobVacancyStatus Status { get; set; }
}

public class CreateUpdateJobVacancyDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Title { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Department { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Range(1, 1000)]
    public int Positions { get; set; } = 1;

    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
}

public class GetRecruitmentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string VacancyNo { get; set; }
}

public interface IJobVacancyAppService : ICrudAppService<JobVacancyDto, Guid, GetRecruitmentListInput, CreateUpdateJobVacancyDto>
{
    Task<JobVacancyDto> OpenAsync(Guid id);

    Task<JobVacancyDto> CloseAsync(Guid id);
}

public class JobApplicantDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string VacancyNo { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public DateTime AppliedDate { get; set; }
    public ApplicantStage Stage { get; set; }
    public int Rating { get; set; }
    public decimal ExpectedSalary { get; set; }
    public string Notes { get; set; }
    public string EmployeeNo { get; set; }
}

public class CreateUpdateJobApplicantDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VacancyNo { get; set; }

    [Required]
    [StringLength(30)]
    public string FirstName { get; set; }

    [StringLength(30)]
    public string LastName { get; set; }

    [StringLength(80)]
    public string Email { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    public DateTime? AppliedDate { get; set; }

    [Range(0, 5)]
    public int Rating { get; set; }

    public decimal ExpectedSalary { get; set; }

    [StringLength(2000)]
    public string Notes { get; set; }
}

public class ChangeApplicantStageInput
{
    public ApplicantStage Stage { get; set; }
}

public class HireApplicantInput
{
    public DateTime? EmploymentDate { get; set; }

    /// <summary>Blank takes the next number of the HR Setup's employee series.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string EmployeeNo { get; set; }
}

public interface IJobApplicantAppService : ICrudAppService<JobApplicantDto, Guid, GetRecruitmentListInput, CreateUpdateJobApplicantDto>
{
    Task<JobApplicantDto> ChangeStageAsync(Guid id, ChangeApplicantStageInput input);

    Task<JobApplicantDto> HireAsync(Guid id, HireApplicantInput input);
}

public class AppraisalPeriodDto : CodeTableDto
{
    public DateTime StartingDate { get; set; }
    public DateTime EndingDate { get; set; }
    public bool Closed { get; set; }
}

public class CreateUpdateAppraisalPeriodDto : CreateUpdateCodeTableDto
{
    public DateTime StartingDate { get; set; }
    public DateTime EndingDate { get; set; }
    public bool Closed { get; set; }
}

public interface IAppraisalPeriodAppService : ICrudAppService<AppraisalPeriodDto, Guid, GetCodeTableListInput, CreateUpdateAppraisalPeriodDto> { }

public class AppraisalDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string EmployeeNo { get; set; }
    public string EmployeeName { get; set; }
    public string PeriodCode { get; set; }
    public string ReviewerEmployeeNo { get; set; }
    public AppraisalStatus Status { get; set; }
    public decimal OverallScore { get; set; }
    public string EmployeeComment { get; set; }
    public string ReviewerComment { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class CreateUpdateAppraisalDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string EmployeeNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PeriodCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ReviewerEmployeeNo { get; set; }

    [StringLength(2000)]
    public string EmployeeComment { get; set; }

    [StringLength(2000)]
    public string ReviewerComment { get; set; }
}

public class GetAppraisalListInput : ErpPagedListInput
{
    public string EmployeeNo { get; set; }
    public string PeriodCode { get; set; }
}

public interface IAppraisalAppService : ICrudAppService<AppraisalDto, Guid, GetAppraisalListInput, CreateUpdateAppraisalDto>
{
    Task<AppraisalDto> StartSelfReviewAsync(Guid id);

    Task<AppraisalDto> SubmitToReviewerAsync(Guid id);

    Task<AppraisalDto> CompleteAsync(Guid id);

    Task<AppraisalDto> ReopenAsync(Guid id);
}

public class AppraisalLineDto : EntityDto<Guid>
{
    public Guid AppraisalId { get; set; }
    public int LineNo { get; set; }
    public string Objective { get; set; }
    public decimal Weight { get; set; }
    public string Target { get; set; }
    public decimal SelfScore { get; set; }
    public decimal ManagerScore { get; set; }
    public string Comment { get; set; }
}

public class CreateUpdateAppraisalLineDto
{
    public Guid AppraisalId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Objective { get; set; }

    public decimal Weight { get; set; } = 1m;

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Target { get; set; }

    public decimal SelfScore { get; set; }
    public decimal ManagerScore { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Comment { get; set; }
}

public class GetAppraisalLineListInput : ErpPagedListInput
{
    public Guid? AppraisalId { get; set; }
}

public interface IAppraisalLineAppService : ICrudAppService<AppraisalLineDto, Guid, GetAppraisalLineListInput, CreateUpdateAppraisalLineDto> { }
