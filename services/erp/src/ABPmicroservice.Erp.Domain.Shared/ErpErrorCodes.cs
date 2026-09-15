namespace ABPmicroservice.Erp;

public static class ErpErrorCodes
{
    // Namespace prefix: Erp
    public const string Prefix = "Erp";

    public static class Items
    {
        public const string ItemAlreadyExists = Prefix + ":Items:00001";
        public const string ItemNotFound = Prefix + ":Items:00002";
        public const string CannotDeleteItemWithLedgerEntries = Prefix + ":Items:00003";
    }

    public static class Customers
    {
        public const string CustomerAlreadyExists = Prefix + ":Customers:00001";
        public const string CustomerNotFound = Prefix + ":Customers:00002";
        public const string CreditLimitExceeded = Prefix + ":Customers:00003";
    }

    public static class Vendors
    {
        public const string VendorAlreadyExists = Prefix + ":Vendors:00001";
        public const string VendorNotFound = Prefix + ":Vendors:00002";
    }

    public static class GLAccounts
    {
        public const string GLAccountAlreadyExists = Prefix + ":GLAccounts:00001";
        public const string GLAccountNotFound = Prefix + ":GLAccounts:00002";
        public const string DirectPostingNotAllowed = Prefix + ":GLAccounts:00003";
        public const string AccountBlocked = Prefix + ":GLAccounts:00004";
    }

    public static class Documents
    {
        public const string DocumentNotFound = Prefix + ":Documents:00001";
        public const string DocumentAlreadyPosted = Prefix + ":Documents:00002";
        public const string DocumentHasNoLines = Prefix + ":Documents:00003";
        public const string DocumentNotReleased = Prefix + ":Documents:00004";
        public const string CannotModifyPostedDocument = Prefix + ":Documents:00005";
    }
}
