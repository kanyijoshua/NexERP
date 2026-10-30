namespace ABPmicroservice.Erp.CashManagement;

/// <summary>The kind of file vendor payments are exported to for the bank.</summary>
public enum PaymentFileFormat
{
    /// <summary>One delimited row per payment, for banks that take a spreadsheet upload.</summary>
    Csv = 0,

    /// <summary>An ISO 20022 customer credit transfer (pain.001.001.03).</summary>
    Iso20022CreditTransfer = 1,
}

public enum DirectDebitMandateType
{
    Recurrent = 0,
    OneOff = 1,
}

/// <summary>Where a direct debit collection is: made, sent to the bank as a file, then paid by its receipt lines.</summary>
public enum DirectDebitCollectionStatus
{
    New = 0,
    FileCreated = 1,
    Posted = 2,
    Closed = 3,
}

/// <summary>The sequence type a collection reports for a mandate, as the bank needs it.</summary>
public enum DirectDebitSequenceType
{
    First = 0,
    Recurrent = 1,
    Final = 2,
    OneOff = 3,
}
