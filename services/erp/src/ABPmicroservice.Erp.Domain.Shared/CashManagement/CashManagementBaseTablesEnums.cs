namespace ABPmicroservice.Erp.CashManagement;

public enum BankAccRecStmtType
{
    BankReconciliation = 0,
    PaymentApplication = 1,
}

public enum BankAccStatementLineType
{
    BankAccountLedgerEntry = 0,
    CheckLedgerEntry = 1,
    Difference = 2,
}

public enum CheckLedgerEntryCheckType
{
    TotalCheck = 0,
    PartialCheck = 1,
}

public enum BankPaymentType
{
    None = 0,
    ComputerCheck = 1,
    ManualCheck = 2,
    ElectronicPayment = 3,
    ElectronicPaymentIat = 4,
}

public enum CheckLedgerEntryEntryStatus
{
    None = 0,
    Printed = 1,
    Voided = 2,
    Posted = 3,
    FinanciallyVoided = 4,
    TestPrint = 5,
    Exported = 6,
    Transmitted = 7,
}

public enum CheckLedgerEntryOriginalEntryStatus
{
    None = 0,
    Printed = 1,
    Voided = 2,
    Posted = 3,
    FinanciallyVoided = 4,
}

public enum CheckLedgerEntryStatementStatus
{
    Open = 0,
    BankAccEntryApplied = 1,
    CheckEntryApplied = 2,
    Closed = 3,
}
