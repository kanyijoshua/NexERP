namespace ABPmicroservice.Erp.HumanResources;

/// <summary>How leave entitlement is accrued for a leave type.</summary>
public enum LeaveAccrualFrequency
{
    /// <summary>Granted in full once per leave year.</summary>
    Annual = 0,

    /// <summary>Accrued monthly (1/12 of annual entitlement per month).</summary>
    Monthly = 1,

    /// <summary>Allocated manually on demand (e.g. compassionate, study, maternity).</summary>
    Manual = 2,
}

/// <summary>The kind of leave ledger entry.</summary>
public enum LeaveEntryType
{
    Entitlement = 0,
    Accrual = 1,
    CarryOver = 2,
    Usage = 3,
    Recall = 4,
    Adjustment = 5,
}

/// <summary>Lifecycle status of a leave application.</summary>
public enum LeaveApplicationStatus
{
    Open = 0,
    PendingApproval = 1,
    Released = 2,
    Rejected = 3,
    Posted = 4,
    Recalled = 5,
}
