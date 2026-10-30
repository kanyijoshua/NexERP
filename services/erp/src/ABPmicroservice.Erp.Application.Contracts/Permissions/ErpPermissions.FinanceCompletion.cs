namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Suggesting vendor payments in a payment journal and exporting them to a bank file.</summary>
    public static class PaymentExport
    {
        public const string Default = GroupName + ".PaymentExport";
    }

    /// <summary>Direct debit mandates and collections.</summary>
    public static class DirectDebits
    {
        public const string Default = GroupName + ".DirectDebits";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Adjusting the cost of item entries and posting inventory cost to the G/L.</summary>
    public static class InventoryCosting
    {
        public const string Default = GroupName + ".InventoryCosting";
    }

    /// <summary>Business units and consolidation runs.</summary>
    public static class Consolidation
    {
        public const string Default = GroupName + ".Consolidation";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Run = Default + ".Run";
    }

    /// <summary>IC partners and the intercompany inbox and outbox.</summary>
    public static class Intercompany
    {
        public const string Default = GroupName + ".Intercompany";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Sending outbox transactions and accepting or rejecting inbox transactions.</summary>
        public const string Process = Default + ".Process";
    }

    /// <summary>The cost chart, cost centers and objects, allocations and cost entries.</summary>
    public static class CostAccounting
    {
        public const string Default = GroupName + ".CostAccounting";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Transferring G/L entries and running allocations.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>Standard sales and purchase codes and the customers and vendors they belong to.</summary>
    public static class StandardCodes
    {
        public const string Default = GroupName + ".StandardCodes";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
