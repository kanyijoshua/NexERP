namespace ABPmicroservice.Erp;

public static class ErpErrorCodes
{
    // Namespace prefix: Erp
    public const string Prefix = "Erp";

    public static class Companies
    {
        public const string CompanyNameAlreadyExists = Prefix + ":Companies:00001";
        public const string CompanyRequired = Prefix + ":Companies:00002";
        public const string CompanyNotFound = Prefix + ":Companies:00003";
    }

    public static class Dimensions
    {
        public const string DimensionCodeAlreadyExists = Prefix + ":Dimensions:00001";
        public const string DimensionValueCodeAlreadyExists = Prefix + ":Dimensions:00002";
    }

    public static class Journals
    {
        public const string NothingToPost = Prefix + ":Journals:00001";
        public const string DocumentOutOfBalance = Prefix + ":Journals:00002";
    }

    public static class Ledgers
    {
        public const string LedgerEntryIsImmutable = Prefix + ":Ledgers:00001";
    }

    public static class Items
    {
        public const string ItemAlreadyExists = Prefix + ":Items:00001";
        public const string ItemNotFound = Prefix + ":Items:00002";
        public const string CannotDeleteItemWithLedgerEntries = Prefix + ":Items:00003";
        public const string CodeAlreadyExists = Prefix + ":Items:00004";
    }

    public static class Customers
    {
        public const string CustomerAlreadyExists = Prefix + ":Customers:00001";
        public const string CustomerNotFound = Prefix + ":Customers:00002";
        public const string CreditLimitExceeded = Prefix + ":Customers:00003";
        public const string CustomerBlocked = Prefix + ":Customers:00004";
    }

    public static class Vendors
    {
        public const string VendorAlreadyExists = Prefix + ":Vendors:00001";
        public const string VendorNotFound = Prefix + ":Vendors:00002";
        public const string VendorBlocked = Prefix + ":Vendors:00003";
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
        public const string DocumentNoAlreadyExists = Prefix + ":Documents:00006";
    }
}
