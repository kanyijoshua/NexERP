namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Applications, students, semester registrations, student bills and exam results.</summary>
    public static class Academics
    {
        public const string Default = GroupName + ".Academics";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Admitting applicants, submitting registrations and posting bills and results, none of which can be undone.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>The Academic Setup, the calendar, programmes with their stages and units, grading and the fee structure.</summary>
    public static class AcademicSetup
    {
        public const string Default = GroupName + ".AcademicSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
