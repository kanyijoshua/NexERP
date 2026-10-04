using System;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.JobQueue;

public class JobQueueEntry_Tests
{
    [Fact]
    public void Non_Recurring_Job_Does_Not_Reschedule_After_Finish()
    {
        var entry = new JobQueueEntry(
            Guid.NewGuid(),
            "Single Run Backup",
            "CleanupJobQueueLogs",
            recurringJob: false
        );

        entry.Status.ShouldBe(JobQueueStatus.Ready);
        entry.NextRunTime.ShouldNotBeNull();

        var now = new DateTime(2026, 10, 3, 10, 0, 0, DateTimeKind.Utc);
        entry.MarkInProcess(now);
        entry.Status.ShouldBe(JobQueueStatus.InProcess);

        entry.MarkFinished(now.AddSeconds(5));
        entry.Status.ShouldBe(JobQueueStatus.Finished);
        entry.NextRunTime.ShouldBeNull();
    }

    [Fact]
    public void Recurring_Job_Calculates_Next_Run_Based_On_Interval()
    {
        var entry = new JobQueueEntry(
            Guid.NewGuid(),
            "Periodic Prune",
            "CleanupJobQueueLogs",
            recurringJob: true,
            intervalType: JobQueueIntervalType.Minutes,
            intervalMinutes: 15
        );

        // Enable all days
        entry.UpdateRecurrence(true, JobQueueIntervalType.Minutes, 15, true, true, true, true, true, true, true, null, null);

        var now = new DateTime(2026, 10, 3, 10, 0, 0, DateTimeKind.Utc);
        var next = entry.CalculateNextRun(now);

        next.ShouldNotBeNull();
        next.Value.ShouldBe(now.AddMinutes(15));
    }

    [Fact]
    public void Recurring_Job_Skips_Disabled_Weekdays()
    {
        var entry = new JobQueueEntry(
            Guid.NewGuid(),
            "Weekday Only Job",
            "SalesPostBatch",
            recurringJob: true,
            intervalType: JobQueueIntervalType.Days,
            intervalMinutes: 1
        );

        // Run Mon-Fri only
        entry.UpdateRecurrence(true, JobQueueIntervalType.Days, 1, true, true, true, true, true, false, false, null, null);

        // 2026-10-02 is a Friday
        var friday = new DateTime(2026, 10, 2, 18, 0, 0, DateTimeKind.Utc);
        var next = entry.CalculateNextRun(friday);

        next.ShouldNotBeNull();
        // Should advance past Saturday and Sunday to Monday 2026-10-05
        next.Value.DayOfWeek.ShouldBe(DayOfWeek.Monday);
        next.Value.Date.ShouldBe(new DateTime(2026, 10, 5));
    }

    [Fact]
    public void Enforces_Daily_Time_Window()
    {
        var entry = new JobQueueEntry(
            Guid.NewGuid(),
            "Business Hours Job",
            "WebhookRetrySweep",
            recurringJob: true,
            intervalType: JobQueueIntervalType.Minutes,
            intervalMinutes: 30
        );

        var startWindow = new TimeSpan(9, 0, 0); // 09:00
        var endWindow = new TimeSpan(17, 0, 0);   // 17:00

        entry.UpdateRecurrence(true, JobQueueIntervalType.Minutes, 30, true, true, true, true, true, true, true, startWindow, endWindow);

        // At 17:15, after the daily window
        var late = new DateTime(2026, 10, 3, 17, 15, 0, DateTimeKind.Utc);
        var next = entry.CalculateNextRun(late);

        next.ShouldNotBeNull();
        // Next run should move to next day at 09:00
        next.Value.Date.ShouldBe(late.Date.AddDays(1));
        next.Value.TimeOfDay.ShouldBe(startWindow);
    }

    [Fact]
    public void Transient_Failure_Retries_Up_To_Max_Attempts()
    {
        var entry = new JobQueueEntry(
            Guid.NewGuid(),
            "Retrying Job",
            "SalesPostBatch",
            maxNoOfAttemptsToRun: 3,
            rerunDelaySeconds: 30
        );

        var now = new DateTime(2026, 10, 3, 12, 0, 0, DateTimeKind.Utc);

        // Attempt 1 fails
        entry.MarkInProcess(now);
        entry.MarkFailed("Network timeout", null, now);
        entry.Status.ShouldBe(JobQueueStatus.Ready);
        entry.NoOfAttemptsToRun.ShouldBe(1);
        entry.NextRunTime.ShouldBe(now.AddSeconds(30));

        // Attempt 2 fails
        var attempt2Time = now.AddSeconds(30);
        entry.MarkInProcess(attempt2Time);
        entry.MarkFailed("Network timeout", null, attempt2Time);
        entry.Status.ShouldBe(JobQueueStatus.Ready);
        entry.NoOfAttemptsToRun.ShouldBe(2);

        // Attempt 3 fails -> reaches MaxNoOfAttemptsToRun (3)
        var attempt3Time = attempt2Time.AddSeconds(30);
        entry.MarkInProcess(attempt3Time);
        entry.MarkFailed("Fatal database error", "stack trace", attempt3Time);
        entry.Status.ShouldBe(JobQueueStatus.Error);
        entry.NoOfAttemptsToRun.ShouldBe(3);
        entry.NextRunTime.ShouldBeNull();
        entry.LastErrorMessage.ShouldBe("Fatal database error");
    }

    [Fact]
    public void Restart_Resets_Failed_Job_To_Ready()
    {
        var entry = new JobQueueEntry(
            Guid.NewGuid(),
            "Failed Job",
            "SalesPostBatch"
        );

        var now = DateTime.UtcNow;
        entry.MarkFailed("Error", null, now);
        entry.MarkFailed("Error", null, now);
        entry.MarkFailed("Error", null, now);
        entry.Status.ShouldBe(JobQueueStatus.Error);

        entry.Restart();

        entry.Status.ShouldBe(JobQueueStatus.Ready);
        entry.NoOfAttemptsToRun.ShouldBe(0);
        entry.LastErrorMessage.ShouldBeNull();
        entry.NextRunTime.ShouldNotBeNull();
    }

    [Fact]
    public void Initializes_And_Updates_Business_Central_Base_Fields()
    {
        var entryId = Guid.NewGuid();
        var entry = new JobQueueEntry(
            entryId,
            "Base Object Job",
            "SalesPostBatch",
            categoryCode: "SALES",
            userId: "admin@domain.com",
            recordIdToProcess: "SalesHeader: SO-0001",
            nextRunDateFormula: "1W",
            notifyOnSuccess: true,
            manualRecurrence: false,
            inactivityTimeoutPeriod: 30
        );

        entry.UserId.ShouldBe("admin@domain.com");
        entry.RecordIdToProcess.ShouldBe("SalesHeader: SO-0001");
        entry.NextRunDateFormula.ShouldBe("1W");
        entry.NotifyOnSuccess.ShouldBeTrue();
        entry.Scheduled.ShouldBeTrue();
        entry.LastReadyState.ShouldNotBeNull();
        entry.InactivityTimeoutPeriod.ShouldBe(30);

        // Put on hold: Scheduled should become false
        entry.SetStatusOnHold();
        entry.Status.ShouldBe(JobQueueStatus.OnHold);
        entry.Scheduled.ShouldBeFalse();

        // Ready again: Scheduled becomes true and LastReadyState updates
        entry.SetStatusReady();
        entry.Status.ShouldBe(JobQueueStatus.Ready);
        entry.Scheduled.ShouldBeTrue();
    }

    [Fact]
    public void Recurring_Job_Calculates_Next_Run_Using_DateFormula()
    {
        var baseDate = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var entry = new JobQueueEntry(
            Guid.NewGuid(),
            "Monthly Run Job",
            "SalesPostBatch",
            recurringJob: true,
            nextRunDateFormula: "1M+CM",
            referenceStartingTime: baseDate
        );

        entry.UpdateRecurrence(true, JobQueueIntervalType.Days, 1, true, true, true, true, true, true, true, null, null, "1M+CM", baseDate);

        var next = entry.CalculateNextRun(baseDate);
        next.ShouldNotBeNull();
        // 1M+CM from 2026-10-01 should be 2026-11-30 (last day of next month)
        next.Value.Date.ShouldBe(new DateTime(2026, 11, 30));
    }
}
