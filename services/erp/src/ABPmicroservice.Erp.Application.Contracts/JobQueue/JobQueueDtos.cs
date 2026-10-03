using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.JobQueue;

public class JobQueueCategoryDto : EntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateUpdateJobQueueCategoryDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxJobCategoryCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class JobQueueEntryDto : FullAuditedEntityDto<Guid>
{
    public string Description { get; set; }
    public string CategoryCode { get; set; }
    public string JobType { get; set; }
    public string ParameterString { get; set; }
    public JobQueueStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime? EarliestStartDateTime { get; set; }
    public DateTime? ExpirationDateTime { get; set; }
    public bool RecurringJob { get; set; }
    public bool RunOnMondays { get; set; }
    public bool RunOnTuesdays { get; set; }
    public bool RunOnWednesdays { get; set; }
    public bool RunOnThursdays { get; set; }
    public bool RunOnFridays { get; set; }
    public bool RunOnSaturdays { get; set; }
    public bool RunOnSundays { get; set; }
    public TimeSpan? DailyStartingTime { get; set; }
    public TimeSpan? DailyEndingTime { get; set; }
    public JobQueueIntervalType IntervalType { get; set; }
    public int IntervalMinutes { get; set; }
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public int NoOfAttemptsToRun { get; set; }
    public int MaxNoOfAttemptsToRun { get; set; }
    public int RerunDelaySeconds { get; set; }
    public int TimeoutSeconds { get; set; }
    public string LastErrorMessage { get; set; }
    public string LastErrorStackTrace { get; set; }
    public string UserId { get; set; }
    public string RecordIdToProcess { get; set; }
    public string NextRunDateFormula { get; set; }
    public DateTime? ReferenceStartingTime { get; set; }
    public DateTime? LastReadyState { get; set; }
    public bool NotifyOnSuccess { get; set; }
    public bool Scheduled { get; set; }
    public bool ManualRecurrence { get; set; }
    public int? InactivityTimeoutPeriod { get; set; }
    public Guid? SystemTaskId { get; set; }
}

public class CreateJobQueueEntryDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxJobCategoryCodeLength)]
    public string CategoryCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxJobTypeLength)]
    public string JobType { get; set; }

    [StringLength(ErpDomainConsts.MaxJobParameterLength)]
    public string ParameterString { get; set; }

    [Range(1, 10)]
    public int Priority { get; set; } = 5;

    public DateTime? EarliestStartDateTime { get; set; }
    public DateTime? ExpirationDateTime { get; set; }

    public bool RecurringJob { get; set; } = false;
    public JobQueueIntervalType IntervalType { get; set; } = JobQueueIntervalType.Minutes;
    public int IntervalMinutes { get; set; } = 60;

    public bool RunOnMondays { get; set; } = true;
    public bool RunOnTuesdays { get; set; } = true;
    public bool RunOnWednesdays { get; set; } = true;
    public bool RunOnThursdays { get; set; } = true;
    public bool RunOnFridays { get; set; } = true;
    public bool RunOnSaturdays { get; set; } = false;
    public bool RunOnSundays { get; set; } = false;

    public TimeSpan? DailyStartingTime { get; set; }
    public TimeSpan? DailyEndingTime { get; set; }

    public int MaxNoOfAttemptsToRun { get; set; } = 3;
    public int RerunDelaySeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 300;

    [StringLength(ErpDomainConsts.MaxUserNameLength)]
    public string UserId { get; set; }

    [StringLength(ErpDomainConsts.MaxRecordIdLength)]
    public string RecordIdToProcess { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string NextRunDateFormula { get; set; }

    public DateTime? ReferenceStartingTime { get; set; }
    public bool NotifyOnSuccess { get; set; }
    public bool ManualRecurrence { get; set; }
    public int? InactivityTimeoutPeriod { get; set; }
}

public class UpdateJobQueueEntryDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxJobCategoryCodeLength)]
    public string CategoryCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxJobTypeLength)]
    public string JobType { get; set; }

    [StringLength(ErpDomainConsts.MaxJobParameterLength)]
    public string ParameterString { get; set; }

    [Range(1, 10)]
    public int Priority { get; set; } = 5;

    public DateTime? EarliestStartDateTime { get; set; }
    public DateTime? ExpirationDateTime { get; set; }

    public bool RecurringJob { get; set; }
    public JobQueueIntervalType IntervalType { get; set; }
    public int IntervalMinutes { get; set; }

    public bool RunOnMondays { get; set; }
    public bool RunOnTuesdays { get; set; }
    public bool RunOnWednesdays { get; set; }
    public bool RunOnThursdays { get; set; }
    public bool RunOnFridays { get; set; }
    public bool RunOnSaturdays { get; set; }
    public bool RunOnSundays { get; set; }

    public TimeSpan? DailyStartingTime { get; set; }
    public TimeSpan? DailyEndingTime { get; set; }

    public int MaxNoOfAttemptsToRun { get; set; }
    public int RerunDelaySeconds { get; set; }
    public int TimeoutSeconds { get; set; }

    [StringLength(ErpDomainConsts.MaxUserNameLength)]
    public string UserId { get; set; }

    [StringLength(ErpDomainConsts.MaxRecordIdLength)]
    public string RecordIdToProcess { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string NextRunDateFormula { get; set; }

    public DateTime? ReferenceStartingTime { get; set; }
    public bool NotifyOnSuccess { get; set; }
    public bool ManualRecurrence { get; set; }
    public int? InactivityTimeoutPeriod { get; set; }
}

public class GetJobQueueEntriesInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string CategoryCode { get; set; }
    public JobQueueStatus? Status { get; set; }
    public string JobType { get; set; }
}

public class JobQueueLogEntryDto : EntityDto<Guid>
{
    public Guid JobQueueEntryId { get; set; }
    public string JobDescription { get; set; }
    public string JobType { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public long DurationMs { get; set; }
    public JobQueueLogStatus Status { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorStackTrace { get; set; }
    public int ProcessedRecords { get; set; }
    public string OutputDetails { get; set; }
    public string UserId { get; set; }
    public string CategoryCode { get; set; }
    public string ParameterString { get; set; }
    public Guid? SystemTaskId { get; set; }
}

public class GetJobQueueLogsInput : ErpPagedListInput
{
    public Guid? JobQueueEntryId { get; set; }
    public JobQueueLogStatus? Status { get; set; }
}

public class JobTypeInfoDto
{
    public string JobType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string DefaultParametersJson { get; set; }
}

public class JobQueueRunResultDto
{
    public Guid JobQueueEntryId { get; set; }
    public Guid LogEntryId { get; set; }
    public bool Success { get; set; }
    public JobQueueStatus NewStatus { get; set; }
    public int ProcessedRecords { get; set; }
    public string OutputDetails { get; set; }
    public string ErrorMessage { get; set; }
    public long DurationMs { get; set; }
}
