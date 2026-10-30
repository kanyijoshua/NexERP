namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Vacancies and applicants.</summary>
    public static class Recruitment
    {
        public const string Default = GroupName + ".Recruitment";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Hiring an applicant, which makes the employee.</summary>
        public const string Hire = Default + ".Hire";
    }

    /// <summary>Appraisal periods and appraisals.</summary>
    public static class Appraisals
    {
        public const string Default = GroupName + ".Appraisals";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Reviewing and completing appraisals as the reviewer.</summary>
        public const string Review = Default + ".Review";
    }

    /// <summary>Subscription plans and customer subscriptions.</summary>
    public static class Subscriptions
    {
        public const string Default = GroupName + ".Subscriptions";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Invoicing the periods that have started.</summary>
        public const string Invoice = Default + ".Invoice";
    }

    /// <summary>Terminals, sessions and orders at the till.</summary>
    public static class PointOfSale
    {
        public const string Default = GroupName + ".PointOfSale";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Opening sessions, ringing up and taking payment.</summary>
        public const string Sell = Default + ".Sell";
    }

    /// <summary>Equipment, teams and maintenance requests.</summary>
    public static class Maintenance
    {
        public const string Default = GroupName + ".Maintenance";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Control points and quality checks.</summary>
    public static class Quality
    {
        public const string Default = GroupName + ".Quality";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Recording the result of a check.</summary>
        public const string Record = Default + ".Record";
    }

    /// <summary>Vehicles, their logs and contracts.</summary>
    public static class Fleet
    {
        public const string Default = GroupName + ".Fleet";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>Segments and mass mailings.</summary>
    public static class Marketing
    {
        public const string Default = GroupName + ".Marketing";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Sending a mailing to a campaign's targets.</summary>
        public const string Send = Default + ".Send";
    }
}
