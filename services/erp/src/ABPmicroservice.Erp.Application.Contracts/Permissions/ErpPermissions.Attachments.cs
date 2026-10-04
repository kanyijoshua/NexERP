namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Files attached to records. Each call also needs the permission to read the record.</summary>
    public static class Attachments
    {
        public const string Default = GroupName + ".Attachments";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
}
