namespace ABPmicroservice.Erp.Inventory;

/// <summary>How the cost of stock leaving is worked out.</summary>
public enum CostingMethod
{
    /// <summary>The oldest receipts still in stock go first, at their own cost.</summary>
    Fifo = 0,

    /// <summary>Everything leaves at the item's running average cost.</summary>
    Average = 1,

    /// <summary>Each unit leaves at the cost it came in at, found by its serial number.</summary>
    Specific = 2,
}

/// <summary>What a value entry adjusts.</summary>
public enum ValueEntryType
{
    DirectCost = 0,
    Revaluation = 1,
    Rounding = 2,
}

/// <summary>The kind of item journal a template is for.</summary>
public enum ItemJournalTemplateType
{
    /// <summary>Positive and negative adjustments.</summary>
    Item = 0,

    /// <summary>Moves stock to another location (reclassification).</summary>
    Transfer = 1,

    /// <summary>Counts stock and posts the difference.</summary>
    PhysicalInventory = 2,

    /// <summary>Changes the cost of the stock on hand.</summary>
    Revaluation = 3,
}

/// <summary>The line an item tracking line belongs to.</summary>
public enum ItemTrackingSourceType
{
    SalesLine = 0,
    PurchaseLine = 1,
    ItemJournalLine = 2,
    TransferLine = 3,
}

public enum TransferStatus
{
    Open = 0,
    Released = 1,
}
