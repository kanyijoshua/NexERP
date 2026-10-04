namespace ABPmicroservice.Erp.Finance;

public enum VatCalculationType
{
    /// <summary>VAT is a percentage of the line, charged to the customer or paid to the vendor.</summary>
    NormalVat = 0,

    /// <summary>The buyer self-assesses: purchase VAT and output VAT are posted and cancel out.</summary>
    ReverseChargeVat = 1,

    /// <summary>The whole line is VAT, e.g. import VAT invoiced by customs.</summary>
    FullVat = 2,
}

public enum VatEntryType
{
    Purchase = 1,
    Sale = 2,

    /// <summary>Written by a VAT settlement: clears the settled entries against the settlement account.</summary>
    Settlement = 3,
}

/// <summary>
/// The general posting type: whether a G/L account line
/// is a purchase or a sale, and so which VAT it carries. None means no VAT.
/// </summary>
public enum GeneralPostingType
{
    None = 0,
    Purchase = 1,
    Sale = 2,
}

/// <summary>Which VAT entries a VAT statement counts.</summary>
public enum VatEntrySelection
{
    Open = 0,
    Closed = 1,
    OpenAndClosed = 2,
}
