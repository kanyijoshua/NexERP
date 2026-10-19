namespace ABPmicroservice.Erp.Finance;

/// <summary>Which subledger an application or a collection document works on.</summary>
public enum PartyLedgerType
{
    Customer = 0,
    Vendor = 1,
}

/// <summary>Where a bank ledger entry stands in the reconciliation of the bank account.</summary>
public enum BankEntryStatementStatus
{
    /// <summary>Not yet on any statement.</summary>
    Open = 0,

    /// <summary>Matched to a line of a statement that is still being reconciled.</summary>
    BankAccEntryApplied = 1,

    /// <summary>On a posted statement.</summary>
    Closed = 2,
}

/// <summary>Whether a reminder or finance charge memo is still a draft or has been issued.</summary>
public enum CollectionDocumentStatus
{
    Open = 0,
    Issued = 1,
}

/// <summary>What one line of a reminder or finance charge memo stands for.</summary>
public enum CollectionLineType
{
    /// <summary>A line of text only.</summary>
    Text = 0,

    /// <summary>An overdue customer entry, with the interest it carries.</summary>
    CustomerLedgerEntry = 1,

    /// <summary>A fee posted to a G/L account.</summary>
    GLAccount = 2,
}

/// <summary>How a deferral spreads an amount over its periods.</summary>
public enum DeferralCalculationMethod
{
    /// <summary>Equal amounts per full period, the first and last prorated by days.</summary>
    StraightLine = 0,

    /// <summary>The same amount in every period, whatever its length.</summary>
    EqualPerPeriod = 1,

    /// <summary>In proportion to the days of each period.</summary>
    DaysPerPeriod = 2,
}

/// <summary>When the first deferral period starts.</summary>
public enum DeferralStartDate
{
    PostingDate = 0,
    BeginningOfPeriod = 1,
    EndOfPeriod = 2,
    BeginningOfNextPeriod = 3,
}

/// <summary>What a posted deferral schedule came from.</summary>
public enum DeferralSourceType
{
    GLJournal = 0,
    Sales = 1,
    Purchase = 2,
}

/// <summary>Where a cash flow forecast line comes from.</summary>
public enum CashFlowSourceType
{
    None = 0,
    Receivables = 1,
    Payables = 2,
    SalesOrders = 3,
    PurchaseOrders = 4,
    Budget = 5,
    Manual = 6,
    LiquidFunds = 7,
}

/// <summary>Money a manual cash flow line brings in or pays out.</summary>
public enum CashFlowManualKind
{
    Revenue = 0,
    Expense = 1,
}

/// <summary>The length of one column of a budget or forecast matrix.</summary>
public enum AnalysisPeriodType
{
    Day = 0,
    Week = 1,
    Month = 2,
    Quarter = 3,
    Year = 4,
}
