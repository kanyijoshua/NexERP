namespace ABPmicroservice.Erp.Finance;

/// <summary>Mirrors Business Central "Tax Calculation Type" (table 325 field 3), without US sales tax.</summary>
public enum VatCalculationType
{
    /// <summary>VAT is a percentage of the line, charged to the customer or paid to the vendor.</summary>
    NormalVat = 0,

    /// <summary>The buyer self-assesses: purchase VAT and output VAT are posted and cancel out.</summary>
    ReverseChargeVat = 1,

    /// <summary>The whole line is VAT, e.g. import VAT invoiced by customs.</summary>
    FullVat = 2,
}

/// <summary>Mirrors Business Central "General Posting Type" as used on VAT entries.</summary>
public enum VatEntryType
{
    Purchase = 1,
    Sale = 2,

    /// <summary>Written by a VAT settlement: clears the settled entries against the settlement account.</summary>
    Settlement = 3,
}

/// <summary>
/// Mirrors Business Central "Gen. Posting Type" (table 81 field 57): whether a G/L account line
/// is a purchase or a sale, and so which VAT it carries. None means no VAT.
/// </summary>
public enum GeneralPostingType
{
    None = 0,
    Purchase = 1,
    Sale = 2,
}

/// <summary>Which VAT entries a VAT statement counts. Mirrors BC "Selection" on the VAT statement.</summary>
public enum VatEntrySelection
{
    Open = 0,
    Closed = 1,
    OpenAndClosed = 2,
}
