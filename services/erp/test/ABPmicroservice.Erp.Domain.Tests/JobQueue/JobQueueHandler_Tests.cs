using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Integration;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace ABPmicroservice.Erp.JobQueue;

public class JobQueueHandler_Tests : ErpDomainTestBase
{
    private readonly CleanupJobQueueLogsHandler _cleanupHandler;
    private readonly WebhookRetrySweepJobHandler _sweepHandler;
    private readonly IRepository<JobQueueLogEntry, Guid> _logRepository;
    private readonly IRepository<WebhookDelivery, Guid> _deliveryRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public JobQueueHandler_Tests()
    {
        _cleanupHandler = GetRequiredService<CleanupJobQueueLogsHandler>();
        _sweepHandler = GetRequiredService<WebhookRetrySweepJobHandler>();
        _logRepository = GetRequiredService<IRepository<JobQueueLogEntry, Guid>>();
        _deliveryRepository = GetRequiredService<IRepository<WebhookDelivery, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _clock = GetRequiredService<IClock>();
    }

    [Fact]
    public async Task CleanupHandler_Deletes_Logs_Older_Than_RetentionDays()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var jobId = _guidGenerator.Create();
            var now = _clock.Now;

            // Log 1: 45 days old (should be deleted)
            var oldLog = new JobQueueLogEntry(_guidGenerator.Create(), jobId, "Old Log", "CleanupJobQueueLogs", now.AddDays(-45));
            // Log 2: 15 days old (should be deleted when retention is 10)
            var midLog = new JobQueueLogEntry(_guidGenerator.Create(), jobId, "Mid Log", "CleanupJobQueueLogs", now.AddDays(-15));
            // Log 3: 2 days old (should be kept)
            var newLog = new JobQueueLogEntry(_guidGenerator.Create(), jobId, "Recent Log", "CleanupJobQueueLogs", now.AddDays(-2));

            await _logRepository.InsertAsync(oldLog, autoSave: true);
            await _logRepository.InsertAsync(midLog, autoSave: true);
            await _logRepository.InsertAsync(newLog, autoSave: true);

            var context = new JobExecutionContext(
                jobId,
                CurrentCompany.Id.Value,
                "CleanupJobQueueLogs",
                "{\"RetentionDays\": 10}",
                CancellationToken.None
            );

            var result = await _cleanupHandler.ExecuteAsync(context);
            result.Success.ShouldBeTrue();
            result.ProcessedRecords.ShouldBeGreaterThanOrEqualTo(2);

            var remaining = await _logRepository.GetListAsync(l => l.JobQueueEntryId == jobId);
            remaining.ShouldNotContain(l => l.Id == oldLog.Id);
            remaining.ShouldNotContain(l => l.Id == midLog.Id);
            remaining.ShouldContain(l => l.Id == newLog.Id);
        });
    }

    [Fact]
    public async Task CleanupHandler_Falls_Back_To_30_Days_When_Json_Is_Invalid_Or_Empty()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var jobId = _guidGenerator.Create();
            var now = _clock.Now;

            // Log: 40 days old (should be deleted with default 30 days)
            var oldLog = new JobQueueLogEntry(_guidGenerator.Create(), jobId, "Old Log", "CleanupJobQueueLogs", now.AddDays(-40));
            // Log: 10 days old (should be kept with default 30 days)
            var recentLog = new JobQueueLogEntry(_guidGenerator.Create(), jobId, "Recent Log", "CleanupJobQueueLogs", now.AddDays(-10));

            await _logRepository.InsertAsync(oldLog, autoSave: true);
            await _logRepository.InsertAsync(recentLog, autoSave: true);

            var context = new JobExecutionContext(
                jobId,
                CurrentCompany.Id.Value,
                "CleanupJobQueueLogs",
                "INVALID_JSON_STRING",
                CancellationToken.None
            );

            var result = await _cleanupHandler.ExecuteAsync(context);
            result.Success.ShouldBeTrue();

            var remaining = await _logRepository.GetListAsync(l => l.JobQueueEntryId == jobId);
            remaining.ShouldNotContain(l => l.Id == oldLog.Id);
            remaining.ShouldContain(l => l.Id == recentLog.Id);
        });
    }

    [Fact]
    public async Task WebhookRetrySweepHandler_Requeues_Failed_Deliveries_Under_Max_Attempts()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var now = _clock.Now;
            var subId = _guidGenerator.Create();

            // 1. Failed delivery (attempt 1) -> should be requeued to Pending
            var failedDelivery = new WebhookDelivery(
                _guidGenerator.Create(),
                subId,
                "SalesOrder",
                EntityChangeKind.Created,
                _guidGenerator.Create(),
                "{}",
                now
            );
            failedDelivery.MarkFailed("Network timeout", 504, now);
            failedDelivery.Status.ShouldBe(WebhookDeliveryStatus.Failed);

            // 2. Delivered delivery -> should remain Delivered
            var deliveredDelivery = new WebhookDelivery(
                _guidGenerator.Create(),
                subId,
                "SalesOrder",
                EntityChangeKind.Created,
                _guidGenerator.Create(),
                "{}",
                now
            );
            deliveredDelivery.MarkDelivered(200, now);

            await _deliveryRepository.InsertAsync(failedDelivery, autoSave: true);
            await _deliveryRepository.InsertAsync(deliveredDelivery, autoSave: true);

            var context = new JobExecutionContext(
                _guidGenerator.Create(),
                CurrentCompany.Id.Value,
                "WebhookRetrySweep",
                "{}",
                CancellationToken.None
            );

            var result = await _sweepHandler.ExecuteAsync(context);
            result.Success.ShouldBeTrue();
            result.ProcessedRecords.ShouldBeGreaterThanOrEqualTo(1);

            var reloadedFailed = await _deliveryRepository.GetAsync(failedDelivery.Id);
            reloadedFailed.Status.ShouldBe(WebhookDeliveryStatus.Pending);
            reloadedFailed.AttemptCount.ShouldBe(0);

            var reloadedDelivered = await _deliveryRepository.GetAsync(deliveredDelivery.Id);
            reloadedDelivered.Status.ShouldBe(WebhookDeliveryStatus.Delivered);
        });
    }
}
