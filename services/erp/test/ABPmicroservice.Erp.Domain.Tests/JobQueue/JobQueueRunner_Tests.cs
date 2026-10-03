using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace ABPmicroservice.Erp.JobQueue;

public class JobQueueRunner_Tests : ErpDomainTestBase
{
    private readonly IJobQueueRunner _runner;
    private readonly IRepository<JobQueueEntry, Guid> _entryRepository;
    private readonly IRepository<JobQueueLogEntry, Guid> _logRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public JobQueueRunner_Tests()
    {
        _runner = GetRequiredService<IJobQueueRunner>();
        _entryRepository = GetRequiredService<IRepository<JobQueueEntry, Guid>>();
        _logRepository = GetRequiredService<IRepository<JobQueueLogEntry, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _clock = GetRequiredService<IClock>();
    }

    [Fact]
    public async Task Concurrent_Execution_On_InProcess_Job_Throws_BusinessException()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = new JobQueueEntry(
                _guidGenerator.Create(),
                "In-flight Job",
                "CleanupJobQueueLogs",
                timeoutSeconds: 300
            );

            // Mark as currently InProcess with fresh heartbeat
            entry.MarkInProcess(_clock.Now);
            await _entryRepository.InsertAsync(entry, autoSave: true);

            var ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _runner.ExecuteEntryAsync(entry.Id);
            });

            ex.Code.ShouldBe(ErpErrorCodes.JobQueue.JobQueueCannotRunInProcessJob);
        });
    }

    [Fact]
    public async Task Stale_InProcess_Job_With_Expired_Heartbeat_Allows_Execution()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = new JobQueueEntry(
                _guidGenerator.Create(),
                "Stale InProcess Job",
                "CleanupJobQueueLogs",
                timeoutSeconds: 60
            );

            // Heartbeat is 10 minutes ago, exceeding timeout of 60 seconds
            entry.MarkInProcess(_clock.Now.AddMinutes(-10));
            await _entryRepository.InsertAsync(entry, autoSave: true);

            // Should not throw JobQueueCannotRunInProcessJob; should execute and recover
            var result = await _runner.ExecuteEntryAsync(entry.Id);
            result.Success.ShouldBeTrue();

            var reloaded = await _entryRepository.GetAsync(entry.Id);
            reloaded.Status.ShouldBe(JobQueueStatus.Finished);
        });
    }

    [Fact]
    public async Task Execution_With_Unregistered_JobType_Records_Error()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = new JobQueueEntry(
                _guidGenerator.Create(),
                "Invalid Handler Job",
                "NonExistentUnknownJobType",
                maxNoOfAttemptsToRun: 1
            );
            await _entryRepository.InsertAsync(entry, autoSave: true);

            var result = await _runner.ExecuteEntryAsync(entry.Id);
            result.Success.ShouldBeFalse();
            result.ErrorMessage.ShouldContain("No registered job handler found");

            var reloaded = await _entryRepository.GetAsync(entry.Id);
            reloaded.Status.ShouldBe(JobQueueStatus.Error);
            reloaded.LastErrorMessage.ShouldContain("NonExistentUnknownJobType");

            var log = await _logRepository.GetAsync(result.LogEntryId);
            log.Status.ShouldBe(JobQueueLogStatus.Error);
            log.ErrorMessage.ShouldContain("NonExistentUnknownJobType");
        });
    }

    [Fact]
    public async Task Successful_Execution_Updates_Status_And_Creates_Success_Log()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = new JobQueueEntry(
                _guidGenerator.Create(),
                "Single Run Job",
                "CleanupJobQueueLogs",
                recurringJob: false
            );
            await _entryRepository.InsertAsync(entry, autoSave: true);

            var result = await _runner.ExecuteEntryAsync(entry.Id);
            result.Success.ShouldBeTrue();
            result.NewStatus.ShouldBe(JobQueueStatus.Finished);

            var reloaded = await _entryRepository.GetAsync(entry.Id);
            reloaded.Status.ShouldBe(JobQueueStatus.Finished);
            reloaded.LastRunTime.ShouldNotBeNull();
            reloaded.NextRunTime.ShouldBeNull();

            var log = await _logRepository.GetAsync(result.LogEntryId);
            log.Status.ShouldBe(JobQueueLogStatus.Success);
            log.DurationMs.ShouldBeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public async Task Recurring_Job_Computes_NextRunTime_On_Success()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = new JobQueueEntry(
                _guidGenerator.Create(),
                "Periodic Job",
                "CleanupJobQueueLogs",
                recurringJob: true,
                intervalType: JobQueueIntervalType.Minutes,
                intervalMinutes: 15
            );
            entry.UpdateRecurrence(true, JobQueueIntervalType.Minutes, 15, true, true, true, true, true, true, true, null, null);
            await _entryRepository.InsertAsync(entry, autoSave: true);

            var result = await _runner.ExecuteEntryAsync(entry.Id);
            result.Success.ShouldBeTrue();
            result.NewStatus.ShouldBe(JobQueueStatus.Ready);

            var reloaded = await _entryRepository.GetAsync(entry.Id);
            reloaded.Status.ShouldBe(JobQueueStatus.Ready);
            reloaded.NextRunTime.ShouldNotBeNull();
            reloaded.NextRunTime.Value.ShouldBeGreaterThan(_clock.Now);
        });
    }
}
