using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.JobQueue;

/// <summary>
/// Historical record of a job queue execution.
/// </summary>
public class JobQueueLogEntry : CompanyEntity
{
    public Guid JobQueueEntryId { get; private set; }
    public string JobDescription { get; private set; }
    public string JobType { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime? EndDateTime { get; private set; }
    public long DurationMs { get; private set; }
    public JobQueueLogStatus Status { get; private set; }
    public string ErrorMessage { get; private set; }
    public string ErrorStackTrace { get; private set; }
    public int ProcessedRecords { get; private set; }
    public string OutputDetails { get; private set; }

    /// <summary>User ID that ran or scheduled this execution.</summary>
    public string UserId { get; private set; }

    /// <summary>Job queue category code.</summary>
    public string CategoryCode { get; private set; }

    /// <summary>Execution parameter payload.</summary>
    public string ParameterString { get; private set; }

    /// <summary>Background system task GUID.</summary>
    public Guid? SystemTaskId { get; private set; }

    protected JobQueueLogEntry() { }

    public JobQueueLogEntry(
        Guid id,
        Guid jobQueueEntryId,
        string jobDescription,
        string jobType,
        DateTime startDateTime,
        string userId = null,
        string categoryCode = null,
        string parameterString = null,
        Guid? systemTaskId = null
    ) : base(id)
    {
        JobQueueEntryId = jobQueueEntryId;
        JobDescription = Check.NotNullOrWhiteSpace(jobDescription, nameof(jobDescription), ErpDomainConsts.MaxDescriptionLength);
        JobType = Check.NotNullOrWhiteSpace(jobType, nameof(jobType), ErpDomainConsts.MaxJobTypeLength);
        StartDateTime = startDateTime;
        UserId = Check.Length(userId, nameof(userId), ErpDomainConsts.MaxUserNameLength);
        CategoryCode = Check.Length(categoryCode, nameof(categoryCode), ErpDomainConsts.MaxJobCategoryCodeLength);
        ParameterString = Check.Length(parameterString, nameof(parameterString), ErpDomainConsts.MaxJobParameterLength);
        SystemTaskId = systemTaskId;
        Status = JobQueueLogStatus.InProcess;
    }

    public void MarkSuccess(DateTime endDateTime, int processedRecords = 0, string outputDetails = null)
    {
        EndDateTime = endDateTime;
        DurationMs = Math.Max(0, (long)(endDateTime - StartDateTime).TotalMilliseconds);
        Status = JobQueueLogStatus.Success;
        ProcessedRecords = processedRecords;
        OutputDetails = Check.Length(outputDetails, nameof(outputDetails), ErpDomainConsts.MaxJobParameterLength);
    }

    public void MarkError(DateTime endDateTime, string errorMessage, string errorStackTrace = null)
    {
        EndDateTime = endDateTime;
        DurationMs = Math.Max(0, (long)(endDateTime - StartDateTime).TotalMilliseconds);
        Status = JobQueueLogStatus.Error;
        ErrorMessage = Check.Length(errorMessage, nameof(errorMessage), ErpDomainConsts.MaxJobErrorLength);
        ErrorStackTrace = Check.Length(errorStackTrace, nameof(errorStackTrace), ErpDomainConsts.MaxJobErrorLength);
    }
}
