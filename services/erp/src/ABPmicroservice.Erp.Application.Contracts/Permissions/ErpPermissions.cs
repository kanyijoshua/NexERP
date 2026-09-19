using Volo.Abp.Reflection;

namespace ABPmicroservice.Erp.Permissions;

public class ErpPermissions
{
    public const string GroupName = "Erp";

    public static class Items
    {
        public const string Default = GroupName + ".Items";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class ItemCategories
    {
        public const string Default = GroupName + ".ItemCategories";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class UnitsOfMeasure
    {
        public const string Default = GroupName + ".UnitsOfMeasure";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Vendors
    {
        public const string Default = GroupName + ".Vendors";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class GLAccounts
    {
        public const string Default = GroupName + ".GLAccounts";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class GLEntries
    {
        public const string Default = GroupName + ".GLEntries";
    }

    public static class SalesDocuments
    {
        public const string Default = GroupName + ".SalesDocuments";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
    }

    public static class PurchaseDocuments
    {
        public const string Default = GroupName + ".PurchaseDocuments";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
    }

    public static class Dimensions
    {
        public const string Default = GroupName + ".Dimensions";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Journals
    {
        public const string Default = GroupName + ".Journals";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
    }

    public static class Workflows
    {
        public const string Default = GroupName + ".Workflows";
        public const string Manage = Default + ".Manage";
        public const string Approve = Default + ".Approve";
    }

    public static class RapidStart
    {
        public const string Default = GroupName + ".RapidStart";
        public const string Import = Default + ".Import";
        public const string Export = Default + ".Export";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
        public const string ExportExcel = Default + ".ExportExcel";
    }

    public static class Companies
    {
        public const string Default = GroupName + ".Companies";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Copy = Default + ".Copy";
    }

    public static class Kanban
    {
        public const string Default = GroupName + ".Kanban";
        public const string Manage = Default + ".Manage";
    }

    public static class Chatter
    {
        public const string Default = GroupName + ".Chatter";
        public const string Create = Default + ".Create";
    }

    public static class ReportLayouts
    {
        public const string Default = GroupName + ".ReportLayouts";
        public const string Manage = Default + ".Manage";
    }

    public static class NoSeries
    {
        public const string Default = GroupName + ".NoSeries";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class SalesSetup
    {
        public const string Default = GroupName + ".SalesSetup";
        public const string Update = Default + ".Update";
    }

    public static class PurchaseSetup
    {
        public const string Default = GroupName + ".PurchaseSetup";
        public const string Update = Default + ".Update";
    }

    public static class ApprovalUserSetup
    {
        public const string Default = GroupName + ".ApprovalUserSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ErpPermissions));
    }
}
