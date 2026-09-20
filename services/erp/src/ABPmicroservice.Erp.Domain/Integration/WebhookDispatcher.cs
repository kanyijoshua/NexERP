using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// Turns changes into queued webhook calls.
/// <para>
/// Each notification carries the row as it stands after the change, so a subscriber does not have
/// to call back to find out what happened — which is the difference between a useful webhook and
/// one that only says "something changed".
/// </para>
/// </summary>
public class WebhookDispatcher : DomainService
{
    private static readonly JsonSerializerOptions PayloadOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly IRepository<WebhookSubscription, Guid> _subscriptionRepository;
    private readonly IRepository<WebhookDelivery, Guid> _deliveryRepository;
    private readonly IEntityQueryExecutor _queryExecutor;
    private readonly ICurrentCompany _currentCompany;

    public WebhookDispatcher(
        IRepository<WebhookSubscription, Guid> subscriptionRepository,
        IRepository<WebhookDelivery, Guid> deliveryRepository,
        IEntityQueryExecutor queryExecutor,
        ICurrentCompany currentCompany
    )
    {
        _subscriptionRepository = subscriptionRepository;
        _deliveryRepository = deliveryRepository;
        _queryExecutor = queryExecutor;
        _currentCompany = currentCompany;
    }

    public async Task<int> RecordAsync(IReadOnlyList<EntityChangeNotification> changes)
    {
        if (changes.Count == 0)
        {
            return 0;
        }

        var entityNames = changes.Select(c => c.EntityName).Distinct().ToList();

        var subscriptions = (await _subscriptionRepository.GetListAsync(s => s.Active))
            .Where(s => entityNames.Contains(s.EntityName))
            .ToList();

        if (subscriptions.Count == 0)
        {
            return 0;
        }

        var now = Clock.Now;
        var deliveries = new List<WebhookDelivery>();

        foreach (var change in changes)
        {
            var interested = subscriptions
                .Where(s => s.EntityName == change.EntityName && s.WantsChange(change.Kind))
                .ToList();

            if (interested.Count == 0)
            {
                continue;
            }

            // Read the row once however many subscribers want it.
            var data = change.Kind == EntityChangeKind.Deleted ? null : await ReadRowAsync(change);

            foreach (var subscription in interested)
            {
                deliveries.Add(
                    new WebhookDelivery(
                        GuidGenerator.Create(),
                        subscription.Id,
                        change.EntityName,
                        change.Kind,
                        change.EntityId,
                        BuildPayload(subscription, change, data, now),
                        now
                    )
                );
            }
        }

        if (deliveries.Count > 0)
        {
            await _deliveryRepository.InsertManyAsync(deliveries, autoSave: true);
        }

        return deliveries.Count;
    }

    /// <summary>
    /// Queues a notification a user asked for from the subscription page, so an endpoint can be
    /// proved to work before anyone relies on it.
    /// </summary>
    public async Task<WebhookDelivery> RecordTestAsync(Guid subscriptionId)
    {
        var subscription = await _subscriptionRepository.FindAsync(subscriptionId);
        if (subscription == null)
        {
            throw new BusinessException(ErpErrorCodes.Integration.SubscriptionNotFound)
                .WithData("subscriptionId", subscriptionId);
        }

        var now = Clock.Now;

        var payload = JsonSerializer.Serialize(
            new
            {
                subscriptionId = subscription.Id,
                subscription = subscription.Name,
                entity = subscription.EntityName,
                changeKind = "Test",
                companyId = _currentCompany.Id,
                occurredAt = now,
                data = (object)null,
            },
            PayloadOptions
        );

        var delivery = new WebhookDelivery(
            GuidGenerator.Create(),
            subscription.Id,
            subscription.EntityName,
            EntityChangeKind.None,
            Guid.Empty,
            payload,
            now
        );

        await _deliveryRepository.InsertAsync(delivery, autoSave: true);
        return delivery;
    }

    private async Task<Dictionary<string, object>> ReadRowAsync(EntityChangeNotification change)
    {
        try
        {
            var result = await _queryExecutor.QueryAsync(
                new EntityQueryRequest
                {
                    EntityName = change.EntityName,
                    Filters =
                    [
                        new EntityFilter
                        {
                            Field = "Id",
                            Operator = EntityFilterOperator.Equals,
                            Value = change.EntityId.ToString(),
                        },
                    ],
                    MaxResultCount = 1,
                }
            );

            return result.Items.FirstOrDefault();
        }
        catch (Exception exception)
        {
            // A payload without its data is still worth sending; losing the notification is not.
            Logger.LogWarning(
                exception,
                "Could not read {EntityName} {EntityId} for a webhook payload.",
                change.EntityName,
                change.EntityId
            );
            return null;
        }
    }

    private string BuildPayload(
        WebhookSubscription subscription,
        EntityChangeNotification change,
        Dictionary<string, object> data,
        DateTime now
    )
    {
        return JsonSerializer.Serialize(
            new
            {
                subscriptionId = subscription.Id,
                subscription = subscription.Name,
                entity = change.EntityName,
                changeKind = change.Kind.ToString(),
                entityId = change.EntityId,
                companyId = _currentCompany.Id,
                occurredAt = now,
                data,
            },
            PayloadOptions
        );
    }
}
