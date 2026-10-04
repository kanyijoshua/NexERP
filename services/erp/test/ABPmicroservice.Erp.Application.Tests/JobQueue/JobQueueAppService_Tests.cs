using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.JobQueue;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.JobQueue;

public class JobQueueAppService_Tests : ErpApplicationTestBase
{
    private readonly IJobQueueAppService _jobQueueAppService;

    public JobQueueAppService_Tests()
    {
        _jobQueueAppService = GetRequiredService<IJobQueueAppService>();
    }

    [Fact]
    public async Task Can_Manage_Job_Queue_Categories()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Create
            var category = await _jobQueueAppService.CreateCategoryAsync(new CreateUpdateJobQueueCategoryDto
            {
                Code = "TEST-CAT",
                Description = "Test Job Queue Category"
            });
            category.Id.ShouldNotBe(Guid.Empty);
            category.Code.ShouldBe("TEST-CAT");

            // List
            var list = await _jobQueueAppService.GetCategoriesAsync();
            list.Items.ShouldContain(c => c.Code == "TEST-CAT");

            // Update
            var updated = await _jobQueueAppService.UpdateCategoryAsync(category.Id, new CreateUpdateJobQueueCategoryDto
            {
                Code = "TEST-CAT-2",
                Description = "Updated description"
            });
            updated.Description.ShouldBe("Updated description");

            // Delete
            await _jobQueueAppService.DeleteCategoryAsync(category.Id);
            var afterDelete = await _jobQueueAppService.GetCategoriesAsync();
            afterDelete.Items.ShouldNotContain(c => c.Id == category.Id);
        });
    }

    [Fact]
    public async Task Available_Job_Types_Returns_Registered_Handlers()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var result = await _jobQueueAppService.GetAvailableJobTypesAsync();
            result.Items.ShouldNotBeEmpty();
            result.Items.ShouldContain(h => h.JobType == "CleanupJobQueueLogs");
            result.Items.ShouldContain(h => h.JobType == "SalesPostBatch");
            result.Items.ShouldContain(h => h.JobType == "PurchasingPostBatch");
            result.Items.ShouldContain(h => h.JobType == "WebhookRetrySweep");
        });
    }

    [Fact]
    public async Task Can_Create_Update_And_Change_Status_Of_JobQueueEntry()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Create
            var entry = await _jobQueueAppService.CreateAsync(new CreateJobQueueEntryDto
            {
                Description = "Log Cleanup Daily",
                JobType = "CleanupJobQueueLogs",
                ParameterString = "{\"retentionDays\": 30}",
                RecurringJob = true,
                IntervalType = JobQueueIntervalType.Days,
                IntervalMinutes = 1440,
                Priority = 5,
                MaxNoOfAttemptsToRun = 3,
                RerunDelaySeconds = 300
            });

            entry.Id.ShouldNotBe(Guid.Empty);
            entry.JobType.ShouldBe("CleanupJobQueueLogs");
            entry.Status.ShouldBe(JobQueueStatus.Ready);

            // Change to OnHold
            var onHold = await _jobQueueAppService.SetStatusOnHoldAsync(entry.Id);
            onHold.Status.ShouldBe(JobQueueStatus.OnHold);

            // Change to Ready
            var ready = await _jobQueueAppService.SetStatusReadyAsync(entry.Id);
            ready.Status.ShouldBe(JobQueueStatus.Ready);
            ready.NextRunTime.ShouldNotBeNull();

            // Update
            var updated = await _jobQueueAppService.UpdateAsync(entry.Id, new UpdateJobQueueEntryDto
            {
                Description = "Updated Log Cleanup",
                JobType = "CleanupJobQueueLogs",
                RecurringJob = true,
                IntervalType = JobQueueIntervalType.Hours,
                IntervalMinutes = 720,
                Priority = 3
            });
            updated.Description.ShouldBe("Updated Log Cleanup");
            updated.IntervalMinutes.ShouldBe(720);

            // Delete
            await _jobQueueAppService.DeleteAsync(entry.Id);
            var list = await _jobQueueAppService.GetListAsync(new GetJobQueueEntriesInput());
            list.Items.ShouldNotContain(e => e.Id == entry.Id);
        });
    }

    [Fact]
    public async Task RunOnce_Executes_Job_And_Creates_Audit_Log()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = await _jobQueueAppService.CreateAsync(new CreateJobQueueEntryDto
            {
                Description = "Run Once Test Job",
                JobType = "CleanupJobQueueLogs",
                ParameterString = "{\"retentionDays\": 7}",
                RecurringJob = false
            });

            // Execute on-demand
            var runResult = await _jobQueueAppService.RunOnceAsync(entry.Id);
            runResult.Success.ShouldBeTrue();
            runResult.DurationMs.ShouldBeGreaterThanOrEqualTo(0);

            // Inspect entry
            var reloaded = await _jobQueueAppService.GetAsync(entry.Id);
            reloaded.LastRunTime.ShouldNotBeNull();
            // Since RecurringJob is false, status transitions to Finished
            reloaded.Status.ShouldBe(JobQueueStatus.Finished);

            // Inspect logs
            var logs = await _jobQueueAppService.GetLogsAsync(new GetJobQueueLogsInput
            {
                JobQueueEntryId = entry.Id
            });
            logs.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
            var log = logs.Items.First(l => l.JobQueueEntryId == entry.Id);
            log.Status.ShouldBe(JobQueueLogStatus.Success);
            log.JobType.ShouldBe("CleanupJobQueueLogs");
        });
    }

    [Fact]
    public async Task Creating_Duplicate_Category_Code_Throws_BusinessException()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _jobQueueAppService.CreateCategoryAsync(new CreateUpdateJobQueueCategoryDto
            {
                Code = "FINANCE-DUP",
                Description = "First finance category"
            });

            var ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _jobQueueAppService.CreateCategoryAsync(new CreateUpdateJobQueueCategoryDto
                {
                    Code = "FINANCE-DUP",
                    Description = "Duplicate finance category"
                });
            });

            ex.Code.ShouldBe(ErpErrorCodes.JobQueue.JobQueueCategoryCodeAlreadyExists);
        });
    }

    [Fact]
    public async Task Deleting_InProcess_Job_Throws_BusinessException()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = await _jobQueueAppService.CreateAsync(new CreateJobQueueEntryDto
            {
                Description = "In Process Test Job",
                JobType = "CleanupJobQueueLogs",
                RecurringJob = false
            });

            // Put into InProcess state via domain repository
            var entryRepo = GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<JobQueueEntry, Guid>>();
            var domainEntry = await entryRepo.GetAsync(entry.Id);
            domainEntry.MarkInProcess(DateTime.UtcNow);
            await entryRepo.UpdateAsync(domainEntry);

            var ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _jobQueueAppService.DeleteAsync(entry.Id);
            });

            ex.Code.ShouldBe(ErpErrorCodes.JobQueue.JobQueueCannotDeleteInProcessJob);
        });
    }

    [Fact]
    public async Task GetList_Filters_By_Category_And_Status()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _jobQueueAppService.CreateCategoryAsync(new CreateUpdateJobQueueCategoryDto
            {
                Code = "CAT-FILTER-A",
                Description = "Category A"
            });

            var entryA = await _jobQueueAppService.CreateAsync(new CreateJobQueueEntryDto
            {
                Description = "Entry A Filter Test",
                CategoryCode = "CAT-FILTER-A",
                JobType = "CleanupJobQueueLogs"
            });
            await _jobQueueAppService.SetStatusOnHoldAsync(entryA.Id);

            var entryB = await _jobQueueAppService.CreateAsync(new CreateJobQueueEntryDto
            {
                Description = "Entry B Filter Test",
                CategoryCode = null,
                JobType = "CleanupJobQueueLogs"
            });

            // Filter by CategoryCode
            var byCategory = await _jobQueueAppService.GetListAsync(new GetJobQueueEntriesInput
            {
                CategoryCode = "CAT-FILTER-A"
            });
            byCategory.Items.ShouldContain(e => e.Id == entryA.Id);
            byCategory.Items.ShouldNotContain(e => e.Id == entryB.Id);

            // Filter by Status OnHold
            var byStatus = await _jobQueueAppService.GetListAsync(new GetJobQueueEntriesInput
            {
                Status = JobQueueStatus.OnHold
            });
            byStatus.Items.ShouldContain(e => e.Id == entryA.Id);
            byStatus.Items.ShouldNotContain(e => e.Id == entryB.Id);
        });
    }

    [Fact]
    public async Task ClearLogs_Prunes_Logs_For_Specified_Entry()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var entry = await _jobQueueAppService.CreateAsync(new CreateJobQueueEntryDto
            {
                Description = "Log Clear Test Job",
                JobType = "CleanupJobQueueLogs",
                RecurringJob = false
            });

            await _jobQueueAppService.RunOnceAsync(entry.Id);

            var logsBefore = await _jobQueueAppService.GetLogsAsync(new GetJobQueueLogsInput
            {
                JobQueueEntryId = entry.Id
            });
            logsBefore.TotalCount.ShouldBeGreaterThan(0);

            // Clear logs for this entry
            await _jobQueueAppService.ClearLogsAsync(entry.Id);

            var logsAfter = await _jobQueueAppService.GetLogsAsync(new GetJobQueueLogsInput
            {
                JobQueueEntryId = entry.Id
            });
            logsAfter.TotalCount.ShouldBe(0);
        });
    }

    [Fact]
    public async Task Can_Manage_Job_Queue_With_Base_Columns()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var refTime = new DateTime(2026, 1, 1, 8, 30, 0, DateTimeKind.Utc);
            var entry = await _jobQueueAppService.CreateAsync(new CreateJobQueueEntryDto
            {
                Description = "Base Columns Test",
                JobType = "CleanupJobQueueLogs",
                UserId = "ADMIN_USER",
                RecordIdToProcess = "Customer: 10000",
                NextRunDateFormula = "1D",
                ReferenceStartingTime = refTime,
                NotifyOnSuccess = true,
                ManualRecurrence = false,
                InactivityTimeoutPeriod = 60,
                RecurringJob = true,
                IntervalType = JobQueueIntervalType.Days,
                IntervalMinutes = 1440
            });

            entry.UserId.ShouldBe("ADMIN_USER");
            entry.RecordIdToProcess.ShouldBe("Customer: 10000");
            entry.NextRunDateFormula.ShouldBe("1D");
            entry.ReferenceStartingTime.ShouldBe(refTime);
            entry.NotifyOnSuccess.ShouldBeTrue();
            entry.Scheduled.ShouldBeTrue();
            entry.InactivityTimeoutPeriod.ShouldBe(60);

            // Update
            var updated = await _jobQueueAppService.UpdateAsync(entry.Id, new UpdateJobQueueEntryDto
            {
                Description = "Base Columns Test Updated",
                JobType = "CleanupJobQueueLogs",
                UserId = "SUPERVISOR",
                RecordIdToProcess = "Customer: 20000",
                NextRunDateFormula = "1W",
                NotifyOnSuccess = false,
                InactivityTimeoutPeriod = 120,
                RecurringJob = true,
                IntervalType = JobQueueIntervalType.Weeks,
                IntervalMinutes = 10080
            });

            updated.UserId.ShouldBe("SUPERVISOR");
            updated.RecordIdToProcess.ShouldBe("Customer: 20000");
            updated.NextRunDateFormula.ShouldBe("1W");
            updated.NotifyOnSuccess.ShouldBeFalse();
            updated.InactivityTimeoutPeriod.ShouldBe(120);

            // Execute once and verify audit log has the base fields
            var runResult = await _jobQueueAppService.RunOnceAsync(entry.Id);
            runResult.Success.ShouldBeTrue();

            var logs = await _jobQueueAppService.GetLogsAsync(new GetJobQueueLogsInput
            {
                JobQueueEntryId = entry.Id
            });
            var log = logs.Items.First(l => l.JobQueueEntryId == entry.Id);
            log.UserId.ShouldBe("SUPERVISOR");
            log.SystemTaskId.ShouldNotBeNull();
        });
    }
}

