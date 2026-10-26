using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Workflows;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.HumanResources;

public class LeaveTypeDto : CodeTableDto
{
    public string CauseOfAbsenceCode { get; set; }
    public decimal DefaultEntitlementDays { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public LeaveAccrualFrequency AccrualFrequency { get; set; }
    public bool Paid { get; set; }
    public bool RequiresApproval { get; set; }
    public EmployeeGender? GenderRestriction { get; set; }
}

public class CreateUpdateLeaveTypeDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CauseOfAbsenceCode { get; set; }

    public decimal DefaultEntitlementDays { get; set; } = 21m;

    public decimal MaxCarryForwardDays { get; set; }

    public LeaveAccrualFrequency AccrualFrequency { get; set; } = LeaveAccrualFrequency.Annual;

    public bool Paid { get; set; } = true;

    public bool RequiresApproval { get; set; } = true;

    public EmployeeGender? GenderRestriction { get; set; }
}

public interface ILeaveTypeAppService
    : ICrudAppService<LeaveTypeDto, Guid, GetCodeTableListInput, CreateUpdateLeaveTypeDto, CreateUpdateLeaveTypeDto> { }

public class LeaveLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeNo { get; set; }
    public string EmployeeName { get; set; }
    public string LeaveTypeCode { get; set; }
    public int LeavePeriodYear { get; set; }
    public DateTime PostingDate { get; set; }
    public LeaveEntryType EntryType { get; set; }
    public string DocumentNo { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal Days { get; set; }
    public string Description { get; set; }
}

public class GetLeaveLedgerEntryListInput : ErpPagedListInput
{
    public Guid? EmployeeId { get; set; }
    public string EmployeeNo { get; set; }
    public string LeaveTypeCode { get; set; }
    public int? LeavePeriodYear { get; set; }
    public string Filter { get; set; }
}

public class LeaveBalanceDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeNo { get; set; }
    public string EmployeeName { get; set; }
    public string LeaveTypeCode { get; set; }
    public string LeaveTypeDescription { get; set; }
    public int LeavePeriodYear { get; set; }
    public decimal EntitledDays { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal CarriedForwardDays { get; set; }
    public decimal TakenDays { get; set; }
    public decimal RecalledDays { get; set; }
    public decimal AdjustedDays { get; set; }
    public decimal BalanceDays { get; set; }
}

public class GetLeaveBalancesInput
{
    public Guid? EmployeeId { get; set; }
    public string LeaveTypeCode { get; set; }
    public int? LeavePeriodYear { get; set; }
}

public class AccrueLeaveInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string LeaveTypeCode { get; set; }

    public int LeavePeriodYear { get; set; }

    public DateTime? PostingDate { get; set; }

    public Guid? EmployeeId { get; set; }

    public decimal? DaysOverride { get; set; }

    public bool CarryForwardFromPreviousYear { get; set; } = true;
}

public class AccrueLeaveResultDto
{
    public int EmployeesProcessed { get; set; }
}

public class LeaveApplicationDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeNo { get; set; }
    public string EmployeeName { get; set; }
    public string LeaveTypeCode { get; set; }
    public int LeavePeriodYear { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal DaysApplied { get; set; }
    public decimal DaysApproved { get; set; }
    public decimal BalanceBefore { get; set; }
    public string Reason { get; set; }
    public string RelieverEmployeeNo { get; set; }
    public string ApproverComment { get; set; }
    public LeaveApplicationStatus LeaveStatus { get; set; }
    public Guid? PostedAbsenceId { get; set; }
    public decimal RecalledDays { get; set; }
    public DateTime? RecallDate { get; set; }
    public string RecallReason { get; set; }
}

public class GetLeaveApplicationListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public Guid? EmployeeId { get; set; }
    public string LeaveTypeCode { get; set; }
    public LeaveApplicationStatus? LeaveStatus { get; set; }
    public int? LeavePeriodYear { get; set; }
}

public class CreateUpdateLeaveApplicationDto
{
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string LeaveTypeCode { get; set; }

    public DateTime? ApplicationDate { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public decimal DaysApplied { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Reason { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string RelieverEmployeeNo { get; set; }
}

public class ApproveLeaveApplicationInput
{
    public decimal? DaysApproved { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Comment { get; set; }
}

public class RejectLeaveApplicationInput
{
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Comment { get; set; }
}

public class RecallLeaveApplicationInput
{
    public decimal RecallDays { get; set; }
    public DateTime? RecallDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Reason { get; set; }
}

public class CalculateLeaveDatesInput
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public decimal DaysApplied { get; set; }
}

public interface ILeaveApplicationAppService
    : ICrudAppService<
        LeaveApplicationDto,
        Guid,
        GetLeaveApplicationListInput,
        CreateUpdateLeaveApplicationDto,
        CreateUpdateLeaveApplicationDto
    >
{
    Task<WorkingPeriodDto> CalculateDatesAsync(CalculateLeaveDatesInput input);
    Task<ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id);
    Task CancelApprovalRequestAsync(Guid id);
    Task<LeaveApplicationDto> ApproveAsync(Guid id, ApproveLeaveApplicationInput input);
    Task<LeaveApplicationDto> RejectAsync(Guid id, RejectLeaveApplicationInput input);
    Task<LeaveApplicationDto> ReopenAsync(Guid id);
    Task<LeaveApplicationDto> RunPostingAsync(Guid id);
    Task<LeaveApplicationDto> RecallAsync(Guid id, RecallLeaveApplicationInput input);
    Task<AccrueLeaveResultDto> AccrueAsync(AccrueLeaveInput input);
    Task<List<LeaveBalanceDto>> GetBalancesAsync(GetLeaveBalancesInput input);
    Task<PagedResultDto<LeaveLedgerEntryDto>> GetLedgerEntriesAsync(GetLeaveLedgerEntryListInput input);
}
