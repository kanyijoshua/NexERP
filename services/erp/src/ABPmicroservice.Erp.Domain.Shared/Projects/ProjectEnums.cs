namespace ABPmicroservice.Erp.Projects;

public enum ProjectStatus
{
    Planning = 0,
    Quote = 1,
    Open = 2,
    Completed = 3,
}

/// <summary>How work in progress is valued: at cost, or at the sales value of the work done (percentage of completion).</summary>
public enum ProjectWipMethod
{
    CostValue = 0,
    SalesValue = 1,
}

public enum ProjectTaskType
{
    Posting = 0,
    Heading = 1,
    Total = 2,
    BeginTotal = 3,
    EndTotal = 4,
}

/// <summary>Whether a planning line is a budget, something to bill, or both.</summary>
public enum ProjectPlanningLineType
{
    Budget = 0,
    Billable = 1,
    BothBudgetAndBillable = 2,
}

/// <summary>What a project line uses: a resource, an item or a G/L account.</summary>
public enum ProjectLineType
{
    Resource = 0,
    Item = 1,
    GLAccount = 2,
}

public enum ProjectLedgerEntryType
{
    Usage = 0,
    Sale = 1,
}

public enum ResourceType
{
    Person = 0,
    Machine = 1,
}

public enum TimeSheetStatus
{
    Open = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
}

public enum ProjectWipEntryType
{
    Costs = 0,
    Sales = 1,
}
