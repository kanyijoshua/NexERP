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

    /// <summary>The posting groups and the setups that turn them into G/L accounts.</summary>
    public static class PostingSetup
    {
        public const string Default = GroupName + ".PostingSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class GeneralLedgerSetup
    {
        public const string Default = GroupName + ".GeneralLedgerSetup";
        public const string Update = Default + ".Update";
    }

    /// <summary>Payment terms, currencies, exchange rates, accounting periods and payment methods.</summary>
    public static class FinanceSetup
    {
        public const string Default = GroupName + ".FinanceSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class BankAccounts
    {
        public const string Default = GroupName + ".BankAccounts";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Locations
    {
        public const string Default = GroupName + ".Locations";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class SalespeoplePurchasers
    {
        public const string Default = GroupName + ".SalespeoplePurchasers";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Employees and their absences.</summary>
    public static class Employees
    {
        public const string Default = GroupName + ".Employees";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>The Human Resources Setup and the HR code tables.</summary>
    public static class HumanResourcesSetup
    {
        public const string Default = GroupName + ".HumanResourcesSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class VatEntries
    {
        public const string Default = GroupName + ".VatEntries";
    }

    /// <summary>The finance period-end jobs: exchange rate adjustment and VAT settlement.</summary>
    public static class PeriodicActivities
    {
        public const string Default = GroupName + ".PeriodicActivities";
        public const string AdjustExchangeRates = Default + ".AdjustExchangeRates";
        public const string SettleVat = Default + ".SettleVat";
    }

    public static class InventorySetup
    {
        public const string Default = GroupName + ".InventorySetup";
        public const string Update = Default + ".Update";
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
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";

        /// <summary>Creating templates, batches and standard journals, as opposed to filling them in.</summary>
        public const string Manage = Default + ".Manage";
    }

    public static class GLRegisters
    {
        public const string Default = GroupName + ".GLRegisters";
        public const string Reverse = Default + ".Reverse";
    }

    public static class AccountSchedules
    {
        public const string Default = GroupName + ".AccountSchedules";
        public const string Manage = Default + ".Manage";
    }

    /// <summary>Exporting any table, and the saved column sets that go with it.</summary>
    public static class DataExport
    {
        public const string Default = GroupName + ".DataExport";
        public const string ManageTemplates = Default + ".ManageTemplates";
    }

    public static class WebServices
    {
        public const string Default = GroupName + ".WebServices";
        public const string Manage = Default + ".Manage";
    }

    public static class Webhooks
    {
        public const string Default = GroupName + ".Webhooks";
        public const string Manage = Default + ".Manage";
    }

    /// <summary>Reading published data through the integration API.</summary>
    public static class Integration
    {
        public const string Default = GroupName + ".Integration";
    }

    public static class Workflows
    {
        public const string Default = GroupName + ".Workflows";
        public const string Manage = Default + ".Manage";
        public const string Approve = Default + ".Approve";
    }

    /// <summary>Configuration packages, templates, the configuration worksheet and the import wizard.</summary>
    public static class RapidStart
    {
        public const string Default = GroupName + ".RapidStart";

        /// <summary>Designing packages, data templates and the worksheet.</summary>
        public const string Manage = Default + ".Manage";

        /// <summary>Bringing data into staging: package files, Excel sheets, the database, record edits.</summary>
        public const string Import = Default + ".Import";

        public const string Export = Default + ".Export";

        /// <summary>Validating and applying staged data, and the import wizard, which write to tables.</summary>
        public const string Apply = Default + ".Apply";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
        public const string ExportExcel = Default + ".ExportExcel";
    }

    /// <summary>Which of the system's own apps this company runs.</summary>
    public static class Modules
    {
        public const string Default = GroupName + ".Modules";
        public const string Manage = Default + ".Manage";
    }

    /// <summary>The look of the app: colours, top bar and corners, for everyone in the tenant.</summary>
    public static class Theme
    {
        public const string Default = GroupName + ".Theme";
    }

    /// <summary>Which profile (role center) each role works as. Choosing one's own needs no permission.</summary>
    public static class Profiles
    {
        public const string Default = GroupName + ".Profiles";
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
