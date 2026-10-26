namespace ABPmicroservice.Erp.Auditing;

/// <summary>Type of change recorded in the Change Log.</summary>
public enum ChangeLogEntryType
{
    Insertion = 0,
    Modification = 1,
    Deletion = 2,
}

/// <summary>Which fields of a table the Change Log tracks for Insert, Modify, or Delete.</summary>
public enum ChangeLogFieldFilterType
{
    None = 0,
    AllFields = 1,
    SomeFields = 2,
}
