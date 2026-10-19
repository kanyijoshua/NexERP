namespace ABPmicroservice.Erp.Manufacturing;

/// <summary>How an item is replenished: bought, made, or moved from another location.</summary>
public enum ReplenishmentSystem
{
    Purchase = 0,
    ProdOrder = 1,
    Transfer = 2,
}

/// <summary>How planning decides what to order of an item.</summary>
public enum ReorderPolicy
{
    None = 0,
    FixedReorderQty = 1,
    MaximumQty = 2,
    LotForLot = 3,
}

public enum ProductionBomStatus
{
    New = 0,
    Certified = 1,
    Closed = 2,
}

public enum ProductionBomLineType
{
    Item = 0,
    ProductionBom = 1,
}

public enum RoutingLineType
{
    WorkCenter = 0,
    MachineCenter = 1,
}

public enum ProductionOrderStatus
{
    Planned = 0,
    FirmPlanned = 1,
    Released = 2,
    Finished = 3,
}

/// <summary>What a planning line proposes.</summary>
public enum PlanningActionMessage
{
    New = 0,
}
