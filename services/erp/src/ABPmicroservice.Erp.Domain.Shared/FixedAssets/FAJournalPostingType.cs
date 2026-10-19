namespace ABPmicroservice.Erp.FixedAssets;

/// <summary>What a fixed asset journal line posts.</summary>
public enum FAJournalPostingType
{
    AcquisitionCost = 0,
    Depreciation = 1,
    WriteDown = 2,
    Appreciation = 3,
    Disposal = 4,
}
