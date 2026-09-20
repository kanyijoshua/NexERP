namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Mirrors Business Central "Gen. Journal Template Type" (table 80 field 3).
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
/// Mirrors Business Central "Gen. Journal Account Type" (table 81 field 3).
/// Only the three account types the posting engine can post to are listed;
/// Bank Account and Fixed Asset arrive with their own ledgers.
/// </summary>
public enum GenJournalAccountType
{
    GLAccount = 0,
    Customer = 1,
    Vendor = 2,
}

/// <summary>
/// Mirrors Business Central "Recurring Method" (table 81 field 46).
/// <para>
/// Fixed keeps the amount on the line after posting, so the same amount is posted again next
/// period; Variable blanks it so it is typed afresh. The Reversing variants additionally write a
/// reversing entry on the day after the posting date, which is how accruals are booked.
/// </para>
/// <para>
/// BC's Balance and Reversing Balance methods share an account balance over allocation lines
/// (table 221). They are left out until allocation lines exist, rather than offered and refused.
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
