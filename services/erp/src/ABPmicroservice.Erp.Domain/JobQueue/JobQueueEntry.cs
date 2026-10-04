using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.JobQueue;

/// <summary>
/// A scheduled or recurring task to be executed in the background.
/// </summary>
public class JobQueueEntry : CompanyAggregateRoot
{
    public string Description { get; private set; }

    /// <summary>Category grouping, referencing <see cref="JobQueueCategory.Code"/>.</summary>
    public string CategoryCode { get; private set; }

    /// <summary>Identifier of the job handler to execute (e.g. "SalesPostBatch", "WebhookRetrySweep").</summary>
    public string JobType { get; private set; }

    /// <summary>Optional JSON payload passed to the job handler.</summary>
    public string ParameterString { get; private set; }

    public JobQueueStatus Status { get; private set; }

    /// <summary>Execution priority: 1 (Highest) to 10 (Lowest). Default is 5 (Normal).</summary>
    public int Priority { get; private set; }

    /// <summary>Earliest time this entry is eligible to run.</summary>
    public DateTime? EarliestStartDateTime { get; private set; }

    /// <summary>Time after which this entry must not run anymore.</summary>
    public DateTime? ExpirationDateTime { get; private set; }

    /// <summary>Whether this entry runs repeatedly on a schedule.</summary>
    public bool RecurringJob { get; private set; }

    public bool RunOnMondays { get; private set; }
    public bool RunOnTuesdays { get; private set; }
    public bool RunOnWednesdays { get; private set; }
    public bool RunOnThursdays { get; private set; }
    public bool RunOnFridays { get; private set; }
    public bool RunOnSaturdays { get; private set; }
    public bool RunOnSundays { get; private set; }

    /// <summary>Earliest time of day the job is allowed to execute.</summary>
    public TimeSpan? DailyStartingTime { get; private set; }

    /// <summary>Latest time of day the job is allowed to execute.</summary>
    public TimeSpan? DailyEndingTime { get; private set; }

    public JobQueueIntervalType IntervalType { get; private set; }

    /// <summary>Interval value in units of <see cref="IntervalType"/> (e.g. 5 minutes, 2 hours, 1 day).</summary>
    public int IntervalMinutes { get; private set; }

    /// <summary>Computed next timestamp for execution.</summary>
    public DateTime? NextRunTime { get; private set; }

    public DateTime? LastRunTime { get; private set; }

    public DateTime? LastHeartbeat { get; private set; }

    /// <summary>Number of consecutive failed execution attempts.</summary>
    public int NoOfAttemptsToRun { get; private set; }

    /// <summary>Maximum consecutive failures allowed before transitioning to Error / OnHold.</summary>
    public int MaxNoOfAttemptsToRun { get; private set; }

    /// <summary>Delay before retrying a failed attempt (in seconds).</summary>
    public int RerunDelaySeconds { get; private set; }

    /// <summary>Maximum allowed execution time (in seconds) before the job is considered timed out.</summary>
    public int TimeoutSeconds { get; private set; }

    public string LastErrorMessage { get; private set; }

    public string LastErrorStackTrace { get; private set; }

    /// <summary>User ID that created or owns this entry.</summary>
    public string UserId { get; private set; }

    /// <summary>Entity record ID or reference to process.</summary>
    public string RecordIdToProcess { get; private set; }

    /// <summary>Recurring date formula (e.g. 1D, 1W, 1M+CM).</summary>
    public string NextRunDateFormula { get; private set; }

    /// <summary>Reference baseline starting timestamp.</summary>
    public DateTime? ReferenceStartingTime { get; private set; }

    /// <summary>Timestamp when entry was transitioned to Ready.</summary>
    public DateTime? LastReadyState { get; private set; }

    /// <summary>Whether to trigger notification upon successful execution.</summary>
    public bool NotifyOnSuccess { get; private set; }

    /// <summary>Whether this entry is currently scheduled by the background task scheduler.</summary>
    public bool Scheduled { get; private set; }

    /// <summary>Whether recurrence is manually managed rather than automatic.</summary>
    public bool ManualRecurrence { get; private set; }

    /// <summary>Minutes of inactivity allowed before entry is set on hold.</summary>
    public int? InactivityTimeoutPeriod { get; private set; }

    /// <summary>Background runtime / scheduler task tracking GUID.</summary>
    public Guid? SystemTaskId { get; private set; }

    protected JobQueueEntry() { }

    public JobQueueEntry(
        Guid id,
        string description,
        string jobType,
        string categoryCode = null,
        string parameterString = null,
        int priority = 5,
        DateTime? earliestStartDateTime = null,
        DateTime? expirationDateTime = null,
        bool recurringJob = false,
        JobQueueIntervalType intervalType = JobQueueIntervalType.Minutes,
        int intervalMinutes = 60,
        int maxNoOfAttemptsToRun = 3,
        int rerunDelaySeconds = 60,
        int timeoutSeconds = 300,
        string userId = null,
        string recordIdToProcess = null,
        string nextRunDateFormula = null,
        DateTime? referenceStartingTime = null,
        bool notifyOnSuccess = false,
        bool manualRecurrence = false,
        int? inactivityTimeoutPeriod = null,
        Guid? systemTaskId = null
    ) : base(id)
    {
        SetDescription(description);
        SetJobType(jobType);
        CategoryCode = categoryCode.IsNullOrWhiteSpace()
            ? null
            : Check.Length(categoryCode.Trim().ToUpperInvariant(), nameof(categoryCode), ErpDomainConsts.MaxJobCategoryCodeLength);
        ParameterString = Check.Length(parameterString, nameof(parameterString), ErpDomainConsts.MaxJobParameterLength);
        Priority = Math.Clamp(priority, 1, 10);
        EarliestStartDateTime = earliestStartDateTime;
        ExpirationDateTime = expirationDateTime;
        RecurringJob = recurringJob;
        IntervalType = intervalType;
        IntervalMinutes = Math.Max(1, intervalMinutes);
        MaxNoOfAttemptsToRun = Math.Max(1, maxNoOfAttemptsToRun);
        RerunDelaySeconds = Math.Max(5, rerunDelaySeconds);
        TimeoutSeconds = Math.Max(30, timeoutSeconds);

        UserId = Check.Length(userId, nameof(userId), ErpDomainConsts.MaxUserNameLength);
        RecordIdToProcess = Check.Length(recordIdToProcess, nameof(recordIdToProcess), ErpDomainConsts.MaxRecordIdLength);
        NextRunDateFormula = Check.Length(nextRunDateFormula, nameof(nextRunDateFormula), ErpDomainConsts.MaxDateFormulaLength);
        ReferenceStartingTime = referenceStartingTime;
        NotifyOnSuccess = notifyOnSuccess;
        ManualRecurrence = manualRecurrence;
        InactivityTimeoutPeriod = inactivityTimeoutPeriod.HasValue ? Math.Max(1, inactivityTimeoutPeriod.Value) : null;
        SystemTaskId = systemTaskId;

        // Default weekdays enabled for recurring jobs
        RunOnMondays = true;
        RunOnTuesdays = true;
        RunOnWednesdays = true;
        RunOnThursdays = true;
        RunOnFridays = true;
        RunOnSaturdays = false;
        RunOnSundays = false;

        Status = JobQueueStatus.Ready;
        LastReadyState = DateTime.UtcNow;
        Scheduled = true;
        NoOfAttemptsToRun = 0;
        NextRunTime = earliestStartDateTime ?? DateTime.UtcNow;
    }

    public void UpdateGeneral(
        string description,
        string categoryCode,
        string jobType,
        string parameterString,
        int priority,
        DateTime? earliestStartDateTime,
        DateTime? expirationDateTime,
        string userId = null,
        string recordIdToProcess = null,
        bool notifyOnSuccess = false,
        bool manualRecurrence = false,
        int? inactivityTimeoutPeriod = null
    )
    {
        SetDescription(description);
        SetJobType(jobType);
        CategoryCode = categoryCode.IsNullOrWhiteSpace()
            ? null
            : Check.Length(categoryCode.Trim().ToUpperInvariant(), nameof(categoryCode), ErpDomainConsts.MaxJobCategoryCodeLength);
        ParameterString = Check.Length(parameterString, nameof(parameterString), ErpDomainConsts.MaxJobParameterLength);
        Priority = Math.Clamp(priority, 1, 10);
        EarliestStartDateTime = earliestStartDateTime;
        ExpirationDateTime = expirationDateTime;

        UserId = Check.Length(userId, nameof(userId), ErpDomainConsts.MaxUserNameLength);
        RecordIdToProcess = Check.Length(recordIdToProcess, nameof(recordIdToProcess), ErpDomainConsts.MaxRecordIdLength);
        NotifyOnSuccess = notifyOnSuccess;
        ManualRecurrence = manualRecurrence;
        InactivityTimeoutPeriod = inactivityTimeoutPeriod.HasValue ? Math.Max(1, inactivityTimeoutPeriod.Value) : null;

        if (Status == JobQueueStatus.Ready && (NextRunTime == null || NextRunTime < earliestStartDateTime))
        {
            NextRunTime = earliestStartDateTime ?? DateTime.UtcNow;
        }
    }

    public void UpdateRecurrence(
        bool recurringJob,
        JobQueueIntervalType intervalType,
        int intervalMinutes,
        bool runOnMondays,
        bool runOnTuesdays,
        bool runOnWednesdays,
        bool runOnThursdays,
        bool runOnFridays,
        bool runOnSaturdays,
        bool runOnSundays,
        TimeSpan? dailyStartingTime,
        TimeSpan? dailyEndingTime,
        string nextRunDateFormula = null,
        DateTime? referenceStartingTime = null
    )
    {
        RecurringJob = recurringJob;
        IntervalType = intervalType;
        IntervalMinutes = Math.Max(1, intervalMinutes);
        RunOnMondays = runOnMondays;
        RunOnTuesdays = runOnTuesdays;
        RunOnWednesdays = runOnWednesdays;
        RunOnThursdays = runOnThursdays;
        RunOnFridays = runOnFridays;
        RunOnSaturdays = runOnSaturdays;
        RunOnSundays = runOnSundays;
        DailyStartingTime = dailyStartingTime;
        DailyEndingTime = dailyEndingTime;
        NextRunDateFormula = Check.Length(nextRunDateFormula, nameof(nextRunDateFormula), ErpDomainConsts.MaxDateFormulaLength);
        ReferenceStartingTime = referenceStartingTime;
    }

    public void UpdateResiliency(int maxNoOfAttemptsToRun, int rerunDelaySeconds, int timeoutSeconds)
    {
        MaxNoOfAttemptsToRun = Math.Max(1, maxNoOfAttemptsToRun);
        RerunDelaySeconds = Math.Max(5, rerunDelaySeconds);
        TimeoutSeconds = Math.Max(30, timeoutSeconds);
    }

    public void SetSystemTaskId(Guid? systemTaskId)
    {
        SystemTaskId = systemTaskId;
    }

    public void SetStatusReady()
    {
        Status = JobQueueStatus.Ready;
        LastReadyState = DateTime.UtcNow;
        Scheduled = true;
        NoOfAttemptsToRun = 0;
        LastErrorMessage = null;
        LastErrorStackTrace = null;
        if (NextRunTime == null || NextRunTime < DateTime.UtcNow)
        {
            NextRunTime = CalculateNextRun(DateTime.UtcNow) ?? DateTime.UtcNow;
        }
    }

    public void SetStatusOnHold()
    {
        Status = JobQueueStatus.OnHold;
        Scheduled = false;
    }

    public void Restart()
    {
        SetStatusReady();
    }

    public void MarkInProcess(DateTime now)
    {
        Status = JobQueueStatus.InProcess;
        LastHeartbeat = now;
    }

    public void UpdateHeartbeat(DateTime now)
    {
        LastHeartbeat = now;
    }

    public void MarkFinished(DateTime now)
    {
        LastRunTime = now;
        ReferenceStartingTime = now;
        NoOfAttemptsToRun = 0;
        LastErrorMessage = null;
        LastErrorStackTrace = null;

        if (RecurringJob)
        {
            var next = CalculateNextRun(now);
            if (next.HasValue)
            {
                NextRunTime = next.Value;
                Status = JobQueueStatus.Ready;
                Scheduled = true;
            }
            else
            {
                NextRunTime = null;
                Status = JobQueueStatus.Finished;
                Scheduled = false;
            }
        }
        else
        {
            NextRunTime = null;
            Status = JobQueueStatus.Finished;
            Scheduled = false;
        }
    }

    public void MarkFailed(string error, string stackTrace, DateTime now)
    {
        NoOfAttemptsToRun++;
        LastErrorMessage = Check.Length(error, nameof(error), ErpDomainConsts.MaxJobErrorLength);
        LastErrorStackTrace = Check.Length(stackTrace, nameof(stackTrace), ErpDomainConsts.MaxJobErrorLength);

        if (NoOfAttemptsToRun < MaxNoOfAttemptsToRun)
        {
            // Transient failure: retry after delay
            NextRunTime = now.AddSeconds(RerunDelaySeconds);
            Status = JobQueueStatus.Ready;
            Scheduled = true;
        }
        else
        {
            // Exceeded max attempts: transition to Error state (stops automated retries until user intervenes)
            Status = JobQueueStatus.Error;
            Scheduled = false;
            NextRunTime = null;
        }
    }

    /// <summary>
    /// Computes the next scheduled execution timestamp based on recurrence settings,
    /// weekday filters, and allowed daily execution windows.
    ///schedule calculation.
    /// </summary>
    public DateTime? CalculateNextRun(DateTime from)
    {
        if (ExpirationDateTime.HasValue && from >= ExpirationDateTime.Value)
        {
            return null;
        }

        if (!RecurringJob)
        {
            return EarliestStartDateTime.HasValue && EarliestStartDateTime.Value > from
                ? EarliestStartDateTime.Value
                : from;
        }

        DateTime candidate;
        if (!string.IsNullOrWhiteSpace(NextRunDateFormula) && Finance.DateFormula.TryParse(NextRunDateFormula, out var formula))
        {
            candidate = formula.Apply(ReferenceStartingTime ?? from);
        }
        else
        {
            candidate = IntervalType switch
            {
                JobQueueIntervalType.Minutes => from.AddMinutes(IntervalMinutes),
                JobQueueIntervalType.Hours => from.AddHours(IntervalMinutes),
                JobQueueIntervalType.Days => from.AddDays(IntervalMinutes),
                JobQueueIntervalType.Weeks => from.AddDays(IntervalMinutes * 7),
                _ => from.AddMinutes(IntervalMinutes),
            };
        }

        if (EarliestStartDateTime.HasValue && candidate < EarliestStartDateTime.Value)
        {
            candidate = EarliestStartDateTime.Value;
        }

        // Fast-forward to the next enabled day of the week if day filters are active
        bool anyDayEnabled = RunOnMondays || RunOnTuesdays || RunOnWednesdays ||
                             RunOnThursdays || RunOnFridays || RunOnSaturdays || RunOnSundays;

        if (anyDayEnabled)
        {
            for (int i = 0; i < 14; i++)
            {
                if (IsDayEnabled(candidate.DayOfWeek))
                {
                    break;
                }
                candidate = candidate.Date.AddDays(1);
                if (DailyStartingTime.HasValue)
                {
                    candidate = candidate.Add(DailyStartingTime.Value);
                }
            }
        }

        // Enforce daily time window
        if (DailyStartingTime.HasValue || DailyEndingTime.HasValue)
        {
            var timeOfDay = candidate.TimeOfDay;
            if (DailyStartingTime.HasValue && timeOfDay < DailyStartingTime.Value)
            {
                candidate = candidate.Date.Add(DailyStartingTime.Value);
            }
            else if (DailyEndingTime.HasValue && timeOfDay > DailyEndingTime.Value)
            {
                // Advance to the next eligible day's start time
                candidate = candidate.Date.AddDays(1);
                if (DailyStartingTime.HasValue)
                {
                    candidate = candidate.Add(DailyStartingTime.Value);
                }
                if (anyDayEnabled)
                {
                    while (!IsDayEnabled(candidate.DayOfWeek))
                    {
                        candidate = candidate.AddDays(1);
                    }
                }
            }
        }

        if (ExpirationDateTime.HasValue && candidate > ExpirationDateTime.Value)
        {
            return null;
        }

        return candidate;
    }

    public bool IsDayEnabled(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => RunOnMondays,
        DayOfWeek.Tuesday => RunOnTuesdays,
        DayOfWeek.Wednesday => RunOnWednesdays,
        DayOfWeek.Thursday => RunOnThursdays,
        DayOfWeek.Friday => RunOnFridays,
        DayOfWeek.Saturday => RunOnSaturdays,
        DayOfWeek.Sunday => RunOnSundays,
        _ => false,
    };

    private void SetDescription(string description)
    {
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), ErpDomainConsts.MaxDescriptionLength).Trim();
    }

    private void SetJobType(string jobType)
    {
        JobType = Check.NotNullOrWhiteSpace(jobType, nameof(jobType), ErpDomainConsts.MaxJobTypeLength).Trim();
    }
}
