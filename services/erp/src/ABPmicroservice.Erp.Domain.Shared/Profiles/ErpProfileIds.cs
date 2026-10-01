namespace ABPmicroservice.Erp.Profiles;

/// <summary>
/// The profiles (roles) a user can work as. Mirrors Business Central's shipped profiles: each one
/// opens a role center with its own navigation bar.
/// </summary>
public static class ErpProfileIds
{
    public const string BusinessManager = "BUSINESS_MANAGER";
    public const string Accountant = "ACCOUNTANT";
    public const string SalesOrderProcessor = "SALES_ORDER_PROCESSOR";
    public const string PurchasingAgent = "PURCHASING_AGENT";
    public const string HumanResourcesManager = "HR_MANAGER";
    public const string Administrator = "ADMINISTRATOR";

    /// <summary>The profile of a user who has neither chosen one nor been given one through a role.</summary>
    public const string Default = BusinessManager;
}
