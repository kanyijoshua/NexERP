using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// One queued call to a subscriber's endpoint.
/// <para>
/// Notifications are written to this table inside the same transaction as the change that caused
/// them, and sent afterwards by a background worker. That is what keeps a slow or unreachable
/// subscriber from holding up, or rolling back, the posting that triggered it.
/// </para>
/// </summary>
public class WebhookDelivery : CompanyEntity
{
    public Guid SubscriptionId { get; private set; }

    public string EntityName { get; private set; }

    public EntityChangeKind ChangeKind { get; private set; }

    public Guid EntityId { get; private set; }

    /// <summary>The exact body that is POSTed, so a redelivery sends what the first attempt did.</summary>
    public string Payload { get; private set; }

    public WebhookDeliveryStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTime? LastAttemptTime { get; private set; }

    /// <summary>When the worker may try again. Set by the back-off after a failure.</summary>
    public DateTime NextAttemptTime { get; private set; }

    public int? ResponseStatusCode { get; private set; }

    public string Error { get; private set; }

    protected WebhookDelivery() { }

    public WebhookDelivery(
        Guid id,
        Guid subscriptionId,
        string entityName,
        EntityChangeKind changeKind,
        Guid entityId,
        string payload,
        DateTime now
    )
        : base(id)
    {
        SubscriptionId = subscriptionId;
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        ChangeKind = changeKind;
        EntityId = entityId;
        Payload = payload;
        Status = WebhookDeliveryStatus.Pending;
        NextAttemptTime = now;
    }

    public bool IsDue(DateTime now) =>
        Status is WebhookDeliveryStatus.Pending or WebhookDeliveryStatus.Failed && NextAttemptTime <= now;

    public void MarkDelivered(int statusCode, DateTime now)
    {
        AttemptCount++;
        LastAttemptTime = now;
        Status = WebhookDeliveryStatus.Delivered;
        ResponseStatusCode = statusCode;
        Error = null;
    }

    /// <summary>
    /// Backs off exponentially — one minute, then two, four, eight — and gives up after
    /// <see cref="ErpDomainConsts.MaxWebhookAttempts"/> tries, so a dead endpoint stops costing
    /// anything but still leaves its failures on record.
    /// </summary>
    public void MarkFailed(string error, int? statusCode, DateTime now)
    {
        AttemptCount++;
        LastAttemptTime = now;
        ResponseStatusCode = statusCode;
        Error = error.IsNullOrWhiteSpace()
            ? null
            : error[..Math.Min(error.Length, ErpDomainConsts.MaxDescriptionLength)];

        if (AttemptCount >= ErpDomainConsts.MaxWebhookAttempts)
        {
            Status = WebhookDeliveryStatus.Abandoned;
            return;
        }

        Status = WebhookDeliveryStatus.Failed;
        NextAttemptTime = now.AddMinutes(Math.Pow(2, AttemptCount - 1));
    }

    /// <summary>Puts an abandoned or failed delivery back in the queue, at the user's request.</summary>
    public void Requeue(DateTime now)
    {
        if (Status == WebhookDeliveryStatus.Delivered)
        {
            throw new BusinessException(ErpErrorCodes.Integration.DeliveryNotRetryable).WithData("deliveryId", Id);
        }

        Status = WebhookDeliveryStatus.Pending;
        AttemptCount = 0;
        NextAttemptTime = now;
        Error = null;
    }
}
