namespace ABPmicroservice.Erp.JobQueue;

/// <summary>
/// Status of a job queue entry.
/// Mirrors Business Central Table 472 "Job Queue Entry" Status (Ready, In Process, Error, On Hold, Finished)
/// and Odoo queue.job states.
/// </summary>
public enum JobQueueStatus
{
    /// <summary>Scheduled and waiting for execution time or runner pickup.</summary>
    Ready = 0,

    /// <summary>Currently executing in a background or foreground session.</summary>
    InProcess = 1,

    /// <summary>Failed with an error. Stays in error or retries until max attempts reached.</summary>
    Error = 2,

    /// <summary>Paused by the user or an administrator. Will not run automatically.</summary>
    OnHold = 3,

    /// <summary>Completed successfully (for non-recurring jobs).</summary>
    Finished = 4,
}

/// <summary>
/// Status of an execution in the job queue log.
/// Mirrors Business Central Table 474 "Job Queue Log Entry" Status.
/// </summary>
public enum JobQueueLogStatus
{
    Success = 0,
    Error = 1,
    InProcess = 2,
}

/// <summary>
/// Frequency interval unit for recurring jobs.
/// Combines Business Central "No. of Minutes between Runs" with Odoo ir.cron "interval_type".
/// </summary>
public enum JobQueueIntervalType
{
    Minutes = 0,
    Hours = 1,
    Days = 2,
    Weeks = 3,
}
