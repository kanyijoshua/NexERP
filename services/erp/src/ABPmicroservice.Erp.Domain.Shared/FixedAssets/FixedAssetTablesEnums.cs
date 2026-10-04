namespace ABPmicroservice.Erp.FixedAssets;

public enum DepreciationBookDisposalCalculationMethod
{
    Net = 0,
    Gross = 1,
}

public enum FAComponentType
{
    None = 0,
    MainAsset = 1,
    Component = 2,
}

public enum FADepreciationMethod
{
    StraightLine = 0,
    DecliningBalance1 = 1,
    DecliningBalance2 = 2,
    Db1SL = 3,
    Db2SL = 4,
    UserDefined = 5,
    Manual = 6,
}

public enum FALedgerEntryFAPostingCategory
{
    None = 0,
    Disposal = 1,
    BalDisposal = 2,
}

public enum FALedgerEntryFAPostingType
{
    AcquisitionCost = 0,
    Depreciation = 1,
    WriteDown = 2,
    Appreciation = 3,
    Custom1 = 4,
    Custom2 = 5,
    ProceedsOnDisposal = 6,
    SalvageValue = 7,
    GainLoss = 8,
    BookValueOnDisposal = 9,
}

public enum FALedgerEntryDisposalCalculationMethod
{
    None = 0,
    Net = 1,
    Gross = 2,
}
