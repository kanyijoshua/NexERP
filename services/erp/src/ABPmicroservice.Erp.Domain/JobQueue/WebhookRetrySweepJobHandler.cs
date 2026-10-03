using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Integration;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.JobQueue;

/// <summary>
/// Background job that inspects stalled or failed webhook deliveries and queues retries.
/// Mirrors Odoo mail queue / webhook sweep worker.
/// </summary>
public class WebhookRetrySweepJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "WebhookRetrySweep";
    public string DisplayName => "Webhook Outbox Retry Sweep";
    public string Description => "Sweeps the webhook outbox for deliveries in failed state and resets their next attempt time.";
    public string DefaultParametersJson => "{}";

    private readonly IRepository<WebhookDelivery, Guid> _deliveryRepository;
    private readonly ILogger<WebhookRetrySweepJobHandler> _logger;

    public WebhookRetrySweepJobHandler(
        IRepository<WebhookDelivery, Guid> deliveryRepository,
        ILogger<WebhookRetrySweepJobHandler> logger
    )
    {
        _deliveryRepository = deliveryRepository;
        _logger = logger;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var failedDeliveries = await _deliveryRepository.GetListAsync(d =>
            d.Status == WebhookDeliveryStatus.Failed &&
            d.AttemptCount < ErpDomainConsts.MaxWebhookAttempts
        );

        var count = failedDeliveries.Count;
        if (count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var delivery in failedDeliveries)
            {
                // Push next attempt immediately
                delivery.Requeue(now);
                await _deliveryRepository.UpdateAsync(delivery);
            }
            _logger.LogInformation("Reset {Count} failed webhook deliveries for immediate retry.", count);
        }

        return JobExecutionResult.Ok(count, $"Inspected outbox; {count} failed deliveries queued for immediate retry.");
    }
}
