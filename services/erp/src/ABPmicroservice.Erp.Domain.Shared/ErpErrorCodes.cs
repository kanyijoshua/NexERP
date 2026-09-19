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

    public static class NoSeries
    {
        public const string NoSeriesNotFound = Prefix + ":NoSeries:00001";
        public const string NoOpenLine = Prefix + ":NoSeries:00002";
        public const string SeriesExhausted = Prefix + ":NoSeries:00003";
        public const string ManualNumbersNotAllowed = Prefix + ":NoSeries:00004";
        public const string DateOrderViolation = Prefix + ":NoSeries:00005";
        public const string NumberCannotBeIncremented = Prefix + ":NoSeries:00006";
        public const string NoSeriesCodeAlreadyExists = Prefix + ":NoSeries:00007";
        public const string NumberRequired = Prefix + ":NoSeries:00008";
        public const string InvalidLine = Prefix + ":NoSeries:00009";
    }

    public static class Approvals
    {
        public const string UserNotInApprovalSetup = Prefix + ":Approvals:00001";
        public const string NoApproverDefined = Prefix + ":Approvals:00002";
        public const string NoQualifiedApprover = Prefix + ":Approvals:00003";
        public const string ApprovalAlreadyRequested = Prefix + ":Approvals:00004";
        public const string ApprovalRequired = Prefix + ":Approvals:00005";
        public const string EntryNotOpen = Prefix + ":Approvals:00006";
        public const string NotTheApprover = Prefix + ":Approvals:00007";
        public const string NothingToCancel = Prefix + ":Approvals:00008";
        public const string OnlySenderCanCancel = Prefix + ":Approvals:00009";
        public const string NoSubstituteOrApprover = Prefix + ":Approvals:00010";
        public const string PendingApproval = Prefix + ":Approvals:00011";
        public const string ApprovalChainLoop = Prefix + ":Approvals:00012";
        public const string WorkflowCodeAlreadyExists = Prefix + ":Approvals:00013";
        public const string UserSetupAlreadyExists = Prefix + ":Approvals:00014";
        public const string NoWorkflowApplies = Prefix + ":Approvals:00015";
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
