namespace ABPmicroservice.Erp.ServiceManagement;

/// <summary>Where a service order stands: waiting, being worked on, held up or done.</summary>
public enum ServiceOrderStatus
{
    Pending = 0,
    InProcess = 1,
    OnHold = 2,
    Finished = 3,
}

public enum ServicePriority
{
    Low = 0,
    Medium = 1,
    High = 2,
}

/// <summary>What a service line charges for: parts (items), labour (resources) or other costs (a G/L account).</summary>
public enum ServiceLineType
{
    Item = 0,
    Resource = 1,
    Cost = 2,
}

/// <summary>A contract is drawn up, then signed (and invoiced every period) until it is cancelled.</summary>
public enum ServiceContractStatus
{
    Draft = 0,
    Signed = 1,
    Cancelled = 2,
}

/// <summary>How often a signed contract is invoiced; the value is the number of months.</summary>
public enum ServiceInvoicePeriod
{
    Month = 1,
    TwoMonths = 2,
    Quarter = 3,
    HalfYear = 6,
    Year = 12,
}

/// <summary>What a service ledger entry records: a service order line posted, or a contract period invoiced.</summary>
public enum ServiceLedgerEntryType
{
    Sale = 0,
    Contract = 1,
}
