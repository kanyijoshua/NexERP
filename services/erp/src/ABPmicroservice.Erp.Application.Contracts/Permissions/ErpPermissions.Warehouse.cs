namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Bins, bin contents, warehouse entries, and put-aways, picks and movements.</summary>
    public static class Warehouse
    {
        public const string Default = GroupName + ".Warehouse";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Registering a put-away, pick or movement, which moves the stock between bins.</summary>
        public const string Register = Default + ".Register";
    }

    /// <summary>Stock held on hand for sales lines and assembly components.</summary>
    public static class Reservations
    {
        public const string Default = GroupName + ".Reservations";

        /// <summary>Reserving and cancelling reservations.</summary>
        public const string Reserve = Default + ".Reserve";
    }

    /// <summary>Physical inventory orders and counting periods.</summary>
    public static class PhysInventory
    {
        public const string Default = GroupName + ".PhysInventory";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Finishing an order, which posts the counted differences.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>Assembly BOMs and assembly orders.</summary>
    public static class Assembly
    {
        public const string Default = GroupName + ".Assembly";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
    }
}
