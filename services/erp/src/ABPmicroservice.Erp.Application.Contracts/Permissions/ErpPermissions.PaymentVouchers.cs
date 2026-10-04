namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Payment vouchers and their lines.</summary>
    public static class PaymentVouchers
    {
        public const string Default = GroupName + ".PaymentVouchers";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Posting a voucher pays it: the money leaves the bank account.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>The Cash Management Setup, payment types and deduction codes.</summary>
    public static class PaymentVoucherSetup
    {
        public const string Default = GroupName + ".PaymentVoucherSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
