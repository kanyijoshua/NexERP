namespace ABPmicroservice.Erp.Inventory;

/// <summary>What a warehouse entry did to the quantity in a bin.</summary>
public enum WarehouseEntryType
{
    /// <summary>Stock left the bin with an item ledger movement (a sale, a negative adjustment, a consumption).</summary>
    NegativeAdjmt = 0,

    /// <summary>Stock came into the bin with an item ledger movement (a purchase, a positive adjustment, an output).</summary>
    PositiveAdjmt = 1,

    /// <summary>Stock moved between two bins of the same location; the item ledger does not change.</summary>
    Movement = 2,
}

public enum WarehouseActivityType
{
    PutAway = 1,
    Pick = 2,
    Movement = 3,
}

public enum WarehouseActivityStatus
{
    Open = 0,
    Registered = 1,
}

/// <summary>The document a put-away or pick was made for.</summary>
public enum WarehouseSourceType
{
    None = 0,
    PurchaseReceipt = 1,
    SalesOrder = 2,
}

/// <summary>What holds a reservation of stock.</summary>
public enum ReservationSourceType
{
    SalesLine = 0,
    AssemblyLine = 1,
}

public enum PhysInvtOrderStatus
{
    Open = 0,
    Finished = 1,
}

public enum AssemblyOrderStatus
{
    Open = 0,
    Released = 1,
    Finished = 2,
}
