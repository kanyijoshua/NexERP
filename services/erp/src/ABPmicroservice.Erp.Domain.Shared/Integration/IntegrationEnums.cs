using System;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// Mirrors Business Central "Object Type" on a Web Service (table 7700 field 1).
/// A Page is a table exposed as readable data; a Query is a read-only projection.
/// </summary>
public enum WebServiceObjectType
{
    Page = 0,
    Query = 1,
}

/// <summary>
/// Which changes a webhook subscriber wants. Mirrors the change types of Business Central's
/// webhook subscriptions (table 2000000199) and Odoo's automated-action triggers.
/// </summary>
[Flags]
public enum EntityChangeKind
{
    None = 0,
    Created = 1,
    Updated = 2,
    Deleted = 4,
    All = Created | Updated | Deleted,
}

/// <summary>Life cycle of one queued webhook call.</summary>
public enum WebhookDeliveryStatus
{
    /// <summary>Waiting for its first attempt, or for the back-off of a failed one to elapse.</summary>
    Pending = 0,

    Delivered = 1,

    /// <summary>The last attempt failed; it will be tried again.</summary>
    Failed = 2,

    /// <summary>Out of attempts. Only a manual retry moves it on.</summary>
    Abandoned = 3,
}
