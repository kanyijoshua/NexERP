namespace ABPmicroservice.Erp.Documents;

/// <summary>Whether a customer's or vendor's standard code goes on a new document by itself or only when asked for.</summary>
public enum StandardCodeInsertMode
{
    Manual = 0,
    Automatic = 1,
}
