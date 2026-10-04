namespace ABPmicroservice.Erp.Permissions;

public partial class ErpPermissions
{
    /// <summary>Payroll runs, payslips and employees' pay items.</summary>
    public static class Payroll
    {
        public const string Default = GroupName + ".Payroll";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";

        /// <summary>Posting payroll runs and raising their payment vouchers, which cannot be undone.</summary>
        public const string Post = Default + ".Post";
    }

    /// <summary>The Payroll Setup, earnings, deductions and the income tax bands.</summary>
    public static class PayrollSetup
    {
        public const string Default = GroupName + ".PayrollSetup";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
