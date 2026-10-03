using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABPmicroservice.Erp.JobQueue;

public class JobExecutionContext
{
    public Guid JobQueueEntryId { get; }
    public Guid CompanyId { get; }
    public string JobType { get; }
    public string ParameterString { get; }
    public CancellationToken CancellationToken { get; }

    public JobExecutionContext(
        Guid jobQueueEntryId,
        Guid companyId,
        string jobType,
        string parameterString,
        CancellationToken cancellationToken = default
    )
    {
        JobQueueEntryId = jobQueueEntryId;
        CompanyId = companyId;
        JobType = jobType;
        ParameterString = parameterString;
        CancellationToken = cancellationToken;
    }
}

public class JobExecutionResult
{
    public bool Success { get; }
    public int ProcessedRecords { get; }
    public string OutputDetails { get; }
    public string ErrorMessage { get; }
    public string ErrorStackTrace { get; }

    private JobExecutionResult(bool success, int processedRecords, string outputDetails, string errorMessage, string errorStackTrace)
    {
        Success = success;
        ProcessedRecords = processedRecords;
        OutputDetails = outputDetails;
        ErrorMessage = errorMessage;
        ErrorStackTrace = errorStackTrace;
    }

    public static JobExecutionResult Ok(int processedRecords = 0, string outputDetails = null)
        => new(true, processedRecords, outputDetails, null, null);

    public static JobExecutionResult Fail(string errorMessage, string errorStackTrace = null)
        => new(false, 0, null, errorMessage, errorStackTrace);
}

/// <summary>
/// Interface implemented by any background job execution handler.
/// Mirrors Business Central Codeunits invoked by Table 472 "Object ID to Run"
/// and Odoo scheduled action target methods.
/// </summary>
public interface IJobHandler
{
    string JobType { get; }
    string DisplayName { get; }
    string Description { get; }
    string DefaultParametersJson { get; }

    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context);
}
