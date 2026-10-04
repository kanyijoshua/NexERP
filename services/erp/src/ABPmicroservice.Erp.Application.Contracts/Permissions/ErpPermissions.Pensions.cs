namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Sponsors, members, contribution schedules, exits and the member ledger.</summary>
    public static class Pensions
    {
        public const string Default = GroupName + ".Pensions";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Posting schedules and exits and allocating interest, all of which write to members' funds and the G/L.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>The Pension Setup, schemes, exit reasons, lump sum tax tables and declared interest.</summary>
    public static class PensionSetup
    {
        public const string Default = GroupName + ".PensionSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
