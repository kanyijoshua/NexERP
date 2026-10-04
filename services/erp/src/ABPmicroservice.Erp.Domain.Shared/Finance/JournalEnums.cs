namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// The type decides which journal page a batch belongs to and which defaults it gets.
/// </summary>
public enum GenJournalTemplateType
{
    General = 0,
    Sales = 1,
    Purchases = 2,
    CashReceipts = 3,
    Payments = 4,
}

/// <summary>
/// Only the account types the posting engine can post to are listed; Fixed Asset arrives with its
/// own ledger.
/// </summary>
public enum GenJournalAccountType
{
    GLAccount = 0,
    Customer = 1,
    Vendor = 2,
    BankAccount = 3,

    /// <summary>An employee's expense payables: what the company owes the employee back.</summary>
    Employee = 4,
}

/// <summary>What an exchange rate adjustment revalued.</summary>
public enum ExchRateAdjmtAccountType
{
    Customer = 0,
    Vendor = 1,
    BankAccount = 2,
}

/// <summary>
/// <para>
/// Fixed keeps the amount on the line after posting, so the same amount is posted again next
/// period; Variable blanks it so it is typed afresh. The Reversing variants additionally write a
/// reversing entry on the day after the posting date, which is how accruals are booked.
/// </para>
/// <para>
/// The Balance and Reversing Balance methods share an account balance over allocation lines.
/// They are left out until allocation lines exist, rather than offered and refused.
/// </para>
/// </summary>
public enum RecurringMethod
{
    None = 0,
    Fixed = 1,
    Variable = 2,
    ReversingFixed = 3,
    ReversingVariable = 4,
}
