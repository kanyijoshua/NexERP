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
        public const string TemplateNameAlreadyExists = Prefix + ":Journals:00003";
        public const string BatchNameAlreadyExists = Prefix + ":Journals:00004";
        public const string TemplateNotFound = Prefix + ":Journals:00005";
        public const string AccountNoRequired = Prefix + ":Journals:00006";
        public const string SameAccountAndBalAccount = Prefix + ":Journals:00007";
        public const string PostingDateRequired = Prefix + ":Journals:00008";
        public const string RecurringFrequencyRequired = Prefix + ":Journals:00009";
        public const string RecurringNotAllowedHere = Prefix + ":Journals:00010";
        public const string InvalidDateFormula = Prefix + ":Journals:00011";
        public const string BalancingMethodNeedsAllocation = Prefix + ":Journals:00012";
        public const string StandardJournalCodeAlreadyExists = Prefix + ":Journals:00013";
        public const string BatchNotEmpty = Prefix + ":Journals:00014";
        public const string PreviewNeedsATransaction = Prefix + ":Journals:00015";
    }

    public static class Registers
    {
        public const string RegisterNotFound = Prefix + ":Registers:00001";
        public const string AlreadyReversed = Prefix + ":Registers:00002";
        public const string NotReversible = Prefix + ":Registers:00003";
        public const string EntryAlreadyReversed = Prefix + ":Registers:00004";
        public const string AppliedEntryCannotBeReversed = Prefix + ":Registers:00005";
    }

    public static class Reports
    {
        public const string ScheduleNameAlreadyExists = Prefix + ":Reports:00001";
        public const string ColumnLayoutNameAlreadyExists = Prefix + ":Reports:00002";
        public const string UnknownRowReference = Prefix + ":Reports:00003";
        public const string CircularRowFormula = Prefix + ":Reports:00004";
        public const string InvalidRowFormula = Prefix + ":Reports:00005";
        public const string InvalidAccountRange = Prefix + ":Reports:00006";
        public const string ScheduleNotFound = Prefix + ":Reports:00007";
        public const string PeriodReversed = Prefix + ":Reports:00008";
    }

    public static class Exporting
    {
        public const string UnknownEntity = Prefix + ":Exporting:00001";
        public const string UnknownField = Prefix + ":Exporting:00002";
        public const string NoFieldsSelected = Prefix + ":Exporting:00003";
        public const string FilterValueNotValid = Prefix + ":Exporting:00004";
        public const string OperatorNotSupportedForField = Prefix + ":Exporting:00005";
        public const string TooManyRows = Prefix + ":Exporting:00006";
        public const string TemplateNameAlreadyExists = Prefix + ":Exporting:00007";
    }

    public static class Integration
    {
        public const string ServiceNameAlreadyExists = Prefix + ":Integration:00001";
        public const string EntityNotPublished = Prefix + ":Integration:00002";
        public const string EndpointNotHttps = Prefix + ":Integration:00003";
        public const string SubscriptionNotFound = Prefix + ":Integration:00004";
        public const string DeliveryNotRetryable = Prefix + ":Integration:00005";
        public const string EndpointNotValid = Prefix + ":Integration:00006";
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
        public const string PostingGroupNotFound = Prefix + ":Customers:00005";
    }

    public static class Vendors
    {
        public const string VendorAlreadyExists = Prefix + ":Vendors:00001";
        public const string VendorNotFound = Prefix + ":Vendors:00002";
        public const string VendorBlocked = Prefix + ":Vendors:00003";
        public const string PostingGroupNotFound = Prefix + ":Vendors:00004";
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
