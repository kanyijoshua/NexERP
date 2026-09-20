namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Mirrors Business Central "Totaling Type" on an Acc. Schedule Line (table 85 field 8).
/// </summary>
public enum AccountScheduleTotalingType
{
    /// <summary>Sums the posting accounts listed in Totaling, e.g. "1000..1999|2100".</summary>
    PostingAccounts = 0,

    /// <summary>Sums total accounts; evaluated the same way here, kept apart to mirror BC.</summary>
    TotalAccounts = 1,

    /// <summary>Adds and subtracts other rows by their row number, e.g. "R10+R20-R30".</summary>
    Formula = 2,

    /// <summary>A caption or a blank spacer line; contributes no amount.</summary>
    Description = 3,
}

/// <summary>
/// Mirrors Business Central "Column Type" on a Column Layout line (table 334 field 4).
/// </summary>
public enum ColumnLayoutType
{
    /// <summary>Movement inside the period.</summary>
    NetChange = 0,

    /// <summary>Cumulative balance up to and including the period end.</summary>
    BalanceAtDate = 1,

    /// <summary>Cumulative balance up to the day before the period starts.</summary>
    BeginningBalance = 2,

    /// <summary>Movement from the start of the fiscal year to the period end.</summary>
    YearToDateNetChange = 3,
}

/// <summary>
/// Mirrors the "Aged as of" choice on Business Central's Aged Accounts Receivable/Payable reports.
/// </summary>
public enum AgingMethod
{
    DueDate = 0,
    PostingDate = 1,
}

/// <summary>How a report column should be rendered, and how it should be exported.</summary>
public enum ReportColumnKind
{
    Text = 0,
    Number = 1,
    Date = 2,
}

/// <summary>Which ledger a balance report reads. Mirrors the BC report request pages.</summary>
public enum AgedLedgerKind
{
    Receivables = 0,
    Payables = 1,
}

/// <summary>
/// The reports the viewer can run and export. Every one of them produces the same shape, so one
/// screen and one exporter serve all of them.
/// </summary>
public enum ReportKind
{
    TrialBalance = 0,
    IncomeStatement = 1,
    BalanceSheet = 2,
    GeneralLedgerDetail = 3,
    AgedReceivables = 4,
    AgedPayables = 5,

    /// <summary>A user-defined account schedule, named in the request.</summary>
    AccountSchedule = 6,
}
