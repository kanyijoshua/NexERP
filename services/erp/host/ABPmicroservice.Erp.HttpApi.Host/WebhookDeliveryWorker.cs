using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace ABPmicroservice.Erp;

/// <summary>
/// Sends the webhook calls that the outbox has queued.
/// <para>
/// Delivery runs here rather than inside the request that caused the change, so a slow or broken
/// subscriber can neither delay a posting nor roll one back. Failures back off and are retried,
/// and what is left after the last attempt stays in the log for someone to look at.
/// </para>
/// </summary>
public class WebhookDeliveryWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    private const int BatchSize = 25;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDeliveryWorker> _logger;

    public WebhookDeliveryWorker(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookDeliveryWorker> logger
    )
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueDeliveriesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // The worker must survive anything one round throws, or webhooks stop for good.
                _logger.LogError(exception, "The webhook delivery worker failed a round.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task SendDueDeliveriesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var unitOfWorkManager = provider.GetRequiredService<IUnitOfWorkManager>();
        var dataFilter = provider.GetRequiredService<IDataFilter>();
        var currentTenant = provider.GetRequiredService<ICurrentTenant>();
        var clock = provider.GetRequiredService<IClock>();

        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);

        // The worker belongs to no tenant and no company, so the ambient filters would hide every
        // row. They are switched off deliberately, for reading a queue that spans all of them.
        using (currentTenant.Change(null))
        using (dataFilter.Disable<IMultiTenant>())
        using (dataFilter.Disable<ICompanyScoped>())
        {
            var deliveryRepository = provider.GetRequiredService<IRepository<WebhookDelivery, Guid>>();
            var subscriptionRepository = provider.GetRequiredService<IRepository<WebhookSubscription, Guid>>();

            var now = clock.Now;

            var due = (
                await deliveryRepository.GetListAsync(d =>
                    (d.Status == WebhookDeliveryStatus.Pending || d.Status == WebhookDeliveryStatus.Failed)
                    && d.NextAttemptTime <= now
                )
            )
                .OrderBy(d => d.CreationTime)
                .Take(BatchSize)
                .ToList();

            if (due.Count == 0)
            {
                await unitOfWork.CompleteAsync(cancellationToken);
                return;
            }

            var subscriptionIds = due.Select(d => d.SubscriptionId).Distinct().ToList();
            var subscriptions = (await subscriptionRepository.GetListAsync(s => subscriptionIds.Contains(s.Id)))
                .ToDictionary(s => s.Id);

            foreach (var delivery in due)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!subscriptions.TryGetValue(delivery.SubscriptionId, out var subscription))
                {
                    delivery.MarkFailed("The subscription no longer exists.", null, clock.Now);
                    await deliveryRepository.UpdateAsync(delivery);
                    continue;
                }

                await AttemptAsync(delivery, subscription, deliveryRepository, subscriptionRepository, clock, cancellationToken);
            }

            await unitOfWork.CompleteAsync(cancellationToken);
        }
    }

    private async Task AttemptAsync(
        WebhookDelivery delivery,
        WebhookSubscription subscription,
        IRepository<WebhookDelivery, Guid> deliveryRepository,
        IRepository<WebhookSubscription, Guid> subscriptionRepository,
        IClock clock,
        CancellationToken cancellationToken
    )
    {
        var client = _httpClientFactory.CreateClient("ErpWebhooks");
        client.Timeout = RequestTimeout;

        using var request = new HttpRequestMessage(HttpMethod.Post, subscription.EndpointUrl)
        {
            Content = new StringContent(delivery.Payload ?? string.Empty, Encoding.UTF8, "application/json"),
        };

        request.Headers.TryAddWithoutValidation(
            WebhookSignature.HeaderName,
            WebhookSignature.Compute(subscription.Secret, delivery.Payload)
        );
        request.Headers.TryAddWithoutValidation("X-Erp-Delivery-Id", delivery.Id.ToString());
        request.Headers.TryAddWithoutValidation("X-Erp-Entity", delivery.EntityName);
        request.Headers.TryAddWithoutValidation("X-Erp-Change-Kind", delivery.ChangeKind.ToString());

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                delivery.MarkDelivered(statusCode, clock.Now);
                subscription.RecordSuccess(clock.Now);
            }
            else
            {
                delivery.MarkFailed($"The endpoint answered {statusCode}.", statusCode, clock.Now);
                subscription.RecordFailure(clock.Now, $"The endpoint answered {statusCode}.");
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            delivery.MarkFailed(exception.Message, null, clock.Now);
            subscription.RecordFailure(clock.Now, exception.Message);
        }

        await deliveryRepository.UpdateAsync(delivery);
        await subscriptionRepository.UpdateAsync(subscription);
    }
}

/// <summary>Registers the worker and the HTTP client it sends with.</summary>
public static class WebhookDeliveryWorkerExtensions
{
    public static IServiceCollection AddErpWebhookDelivery(this IServiceCollection services)
    {
        services.AddHttpClient("ErpWebhooks");
        services.AddHostedService<WebhookDeliveryWorker>();
        return services;
    }
}
