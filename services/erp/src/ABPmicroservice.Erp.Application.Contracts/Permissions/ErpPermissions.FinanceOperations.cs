namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Applying posted customer and vendor entries to each other.</summary>
    public static class EntryApplication
    {
        public const string Default = GroupName + ".EntryApplication";

        /// <summary>Undoing an application, which reopens the entries and reverses its gains, losses and discounts.</summary>
        public const string Unapply = Default + ".Unapply";
    }

    /// <summary>Posting a reconciled bank statement, which closes the matched entries for good.</summary>
    public static class BankReconciliationPosting
    {
        public const string Default = GroupName + ".BankReconciliations.Post";
    }

    public static class Reminders
    {
        public const string Default = GroupName + ".Reminders";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Issue = Default + ".Issue";
    }

    public static class FinanceChargeMemos
    {
        public const string Default = GroupName + ".FinanceChargeMemos";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Issue = Default + ".Issue";
    }

    /// <summary>Reminder terms, their levels and finance charge terms.</summary>
    public static class CollectionSetup
    {
        public const string Default = GroupName + ".CollectionSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Deferral templates and the posted deferral schedules.</summary>
    public static class Deferrals
    {
        public const string Default = GroupName + ".Deferrals";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Cash flow forecasts, their manual lines, the worksheet and its registration.</summary>
    public static class CashFlow
    {
        public const string Default = GroupName + ".CashFlow";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Register = Default + ".Register";
    }

    /// <summary>FA journals: acquiring, depreciating and disposing of fixed assets.</summary>
    public static class FAJournals
    {
        public const string Default = GroupName + ".FAJournals";
        public const string Post = Default + ".Post";
    }

    /// <summary>Service items, contracts and orders, and the service and warranty ledgers.</summary>
    public static class Service
    {
        public const string Default = GroupName + ".Service";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Posting service orders and invoicing contracts.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>The service setup, order types, fault and resolution codes.</summary>
    public static class ServiceSetup
    {
        public const string Default = GroupName + ".ServiceSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>BOMs, routings, production orders and the planning worksheet.</summary>
    public static class Manufacturing
    {
        public const string Default = GroupName + ".Manufacturing";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Posting consumption, output and finished orders, and carrying out the plan.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>The manufacturing setup, work centers and machine centers.</summary>
    public static class ManufacturingSetup
    {
        public const string Default = GroupName + ".ManufacturingSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Projects, their tasks, plans and journals, resources and the project ledger.</summary>
    public static class Projects
    {
        public const string Default = GroupName + ".Projects";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Posting project journals and WIP, and invoicing projects.</summary>
        public const string Post = Default + ".Post";
    }

    public static class TimeSheets
    {
        public const string Default = GroupName + ".TimeSheets";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
    }

    /// <summary>The projects setup, project posting groups and resources.</summary>
    public static class ProjectsSetup
    {
        public const string Default = GroupName + ".ProjectsSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Contacts, leads, opportunities, campaigns and interactions.</summary>
    public static class Crm
    {
        public const string Default = GroupName + ".Crm";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Pipeline stages, close codes, activity types and the marketing setup.</summary>
    public static class CrmSetup
    {
        public const string Default = GroupName + ".CrmSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
