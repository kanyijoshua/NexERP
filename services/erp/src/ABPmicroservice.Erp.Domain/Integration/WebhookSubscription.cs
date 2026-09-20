using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// Webhook Subscription. Mirrors Business Central table 2000000199 "Webhook Subscription" and
/// plays the part of Odoo's automated actions that call out to another system.
/// <para>
/// It says where to send a notification, for which table, and for which kinds of change. The
/// secret signs each call so the receiver can tell a genuine notification from anyone else's
/// POST to the same URL.
/// </para>
/// </summary>
public class WebhookSubscription : CompanyAggregateRoot
{
    public string Name { get; private set; }

    /// <summary>Table from the entity registry whose changes are notified.</summary>
    public string EntityName { get; private set; }

    public string EndpointUrl { get; private set; }

    public EntityChangeKind ChangeKinds { get; private set; }

    /// <summary>
    /// Shared secret the payload is signed with (HMAC-SHA256, sent as the X-Erp-Signature header).
    /// It is generated here and shown to the user once, like any other credential.
    /// </summary>
    public string Secret { get; private set; }

    public bool Active { get; private set; }

    /// <summary>Consecutive failures. Reset by the first delivery that gets through.</summary>
    public int FailureCount { get; private set; }

    public string LastError { get; private set; }

    public DateTime? LastDeliveryTime { get; private set; }

    protected WebhookSubscription() { }

    public WebhookSubscription(
        Guid id,
        string name,
        string entityName,
        string endpointUrl,
        EntityChangeKind changeKinds = EntityChangeKind.All,
        string secret = null,
        bool active = true
    )
        : base(id)
    {
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        Secret = Check.Length(secret, nameof(secret), ErpDomainConsts.MaxWebhookSecretLength);
        Update(name, endpointUrl, changeKinds, active);
    }

    public void Update(string name, string endpointUrl, EntityChangeKind changeKinds, bool active)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        EndpointUrl = ValidateEndpoint(endpointUrl);
        ChangeKinds = changeKinds;
        Active = active;
    }

    public void SetSecret(string secret)
    {
        Secret = Check.NotNullOrWhiteSpace(secret, nameof(secret), ErpDomainConsts.MaxWebhookSecretLength);
    }

    public bool WantsChange(EntityChangeKind kind) => Active && (ChangeKinds & kind) == kind;

    public void RecordSuccess(DateTime at)
    {
        FailureCount = 0;
        LastError = null;
        LastDeliveryTime = at;
    }

    public void RecordFailure(DateTime at, string error)
    {
        FailureCount++;
        LastError = Check.Length(error, nameof(error), ErpDomainConsts.MaxDescriptionLength);
        LastDeliveryTime = at;
    }

    /// <summary>
    /// Only absolute http(s) URLs are accepted, and plain http only for a loopback address:
    /// a signed payload sent in the clear to a remote host would leak the data it describes.
    /// </summary>
    private static string ValidateEndpoint(string endpointUrl)
    {
        var text = Check.NotNullOrWhiteSpace(endpointUrl, nameof(endpointUrl), ErpDomainConsts.MaxUrlLength).Trim();

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            throw new BusinessException(ErpErrorCodes.Integration.EndpointNotValid).WithData("url", text);
        }

        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
        {
            throw new BusinessException(ErpErrorCodes.Integration.EndpointNotValid).WithData("url", text);
        }

        if (uri.Scheme == Uri.UriSchemeHttp && !uri.IsLoopback)
        {
            throw new BusinessException(ErpErrorCodes.Integration.EndpointNotHttps).WithData("url", text);
        }

        return text;
    }
}
