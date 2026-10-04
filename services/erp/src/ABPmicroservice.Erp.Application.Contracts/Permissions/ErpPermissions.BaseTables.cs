namespace ABPmicroservice.Erp.Permissions;

/// <summary>Permissions of the base tables added alongside the posting kernel.</summary>
public partial class ErpPermissions
{
    public static class FixedAssets
    {
        public const string Default = GroupName + ".FixedAssets";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class FixedAssetSetup
    {
        public const string Default = GroupName + ".FixedAssetSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Budgets
    {
        public const string Default = GroupName + ".Budgets";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class BankReconciliations
    {
        public const string Default = GroupName + ".BankReconciliations";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class UserSetup
    {
        public const string Default = GroupName + ".UserSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Comments
    {
        public const string Default = GroupName + ".Comments";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class ReportSelections
    {
        public const string Default = GroupName + ".ReportSelections";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class WorkflowUserGroups
    {
        public const string Default = GroupName + ".WorkflowUserGroups";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class ApprovalComments
    {
        public const string Default = GroupName + ".ApprovalComments";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
