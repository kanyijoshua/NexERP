namespace ABPmicroservice.Erp.Crm;

public enum ContactType
{
    Company = 0,
    Person = 1,
}

/// <summary>A lead is a prospect not yet qualified; converting it makes it an opportunity.</summary>
public enum OpportunityKind
{
    Lead = 0,
    Opportunity = 1,
}

public enum OpportunityStatus
{
    Open = 0,
    Won = 1,
    Lost = 2,
}

/// <summary>Whether a close code explains a won or a lost opportunity.</summary>
public enum CloseOpportunityType
{
    Won = 0,
    Lost = 1,
}

public enum OpportunityPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
}
