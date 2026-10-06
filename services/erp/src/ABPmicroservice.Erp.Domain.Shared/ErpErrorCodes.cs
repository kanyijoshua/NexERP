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
        public const string AppliesToEntryNotFound = Prefix + ":Journals:00016";
        public const string AppliesToEntryMismatch = Prefix + ":Journals:00017";
        public const string VatOnlyOnGLAccounts = Prefix + ":Journals:00018";
        public const string CurrencyMismatch = Prefix + ":Journals:00019";
        public const string EmployeeDocumentTypeNotAllowed = Prefix + ":Journals:00020";
        public const string EmployeeInLocalCurrencyOnly = Prefix + ":Journals:00021";
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
        public const string LayoutNameAlreadyExists = Prefix + ":Reports:00009";
        public const string LayoutTypeNotRenderable = Prefix + ":Reports:00010";
        public const string LayoutTemplateEmpty = Prefix + ":Reports:00011";
        public const string LayoutTemplateNotValid = Prefix + ":Reports:00012";
        public const string LayoutNotFound = Prefix + ":Reports:00013";
        public const string LayoutBelongsToAnotherReport = Prefix + ":Reports:00014";
        public const string UnknownStandardReport = Prefix + ":Reports:00015";
        public const string RdlcNotValid = Prefix + ":Reports:00016";
        public const string RdlcTooLarge = Prefix + ":Reports:00017";
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

    public static class RapidStart
    {
        public const string PackageCodeAlreadyExists = Prefix + ":RapidStart:00001";
        public const string TableAlreadyInPackage = Prefix + ":RapidStart:00002";
        public const string TableNotImportable = Prefix + ":RapidStart:00003";
        public const string FileNotValid = Prefix + ":RapidStart:00004";
        public const string FileTooLarge = Prefix + ":RapidStart:00005";
        public const string TemplateCodeAlreadyExists = Prefix + ":RapidStart:00006";
        public const string TableNotInPackage = Prefix + ":RapidStart:00007";
        public const string FileHasNoRows = Prefix + ":RapidStart:00008";
        public const string FieldNotImportable = Prefix + ":RapidStart:00009";
        public const string ColumnMappedTwice = Prefix + ":RapidStart:00010";
        public const string KeyFieldNotMapped = Prefix + ":RapidStart:00011";
        public const string ImportHasErrors = Prefix + ":RapidStart:00012";
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

    public static class Modules
    {
        public const string UnknownModule = Prefix + ":Modules:00001";
        public const string ModuleIsDisabled = Prefix + ":Modules:00002";
        public const string CoreModuleCannotBeDisabled = Prefix + ":Modules:00003";
        public const string DependencyIsDisabled = Prefix + ":Modules:00004";
        public const string RequiredByAnotherModule = Prefix + ":Modules:00005";
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

    public static class PostingSetup
    {
        public const string CodeAlreadyExists = Prefix + ":PostingSetup:00001";
        public const string CodeNotFound = Prefix + ":PostingSetup:00002";
        public const string SetupAlreadyExists = Prefix + ":PostingSetup:00003";
        public const string GeneralPostingSetupMissing = Prefix + ":PostingSetup:00004";
        public const string AccountMissing = Prefix + ":PostingSetup:00005";
        public const string InventoryPostingSetupMissing = Prefix + ":PostingSetup:00006";
        public const string GLAccountNotFound = Prefix + ":PostingSetup:00007";
        public const string InvalidPercent = Prefix + ":PostingSetup:00008";
        public const string VatPostingSetupMissing = Prefix + ":PostingSetup:00009";
        public const string ExchangeRateAlreadyExists = Prefix + ":PostingSetup:00010";
        public const string InvalidExchangeRate = Prefix + ":PostingSetup:00011";
    }

    public static class GeneralLedger
    {
        public const string PostingDateNotAllowed = Prefix + ":GeneralLedger:00001";
        public const string InvalidPostingDateRange = Prefix + ":GeneralLedger:00002";
        public const string InvalidRoundingPrecision = Prefix + ":GeneralLedger:00003";
        public const string SameGlobalDimensions = Prefix + ":GeneralLedger:00004";
        public const string DimensionNotFound = Prefix + ":GeneralLedger:00005";
        public const string AccountingPeriodExists = Prefix + ":GeneralLedger:00006";
        public const string NoOpenFiscalYear = Prefix + ":GeneralLedger:00007";
        public const string FiscalYearNotComplete = Prefix + ":GeneralLedger:00008";
        public const string InvalidPeriodCount = Prefix + ":GeneralLedger:00009";
        public const string PeriodClosed = Prefix + ":GeneralLedger:00010";
        public const string ExchangeRateNotFound = Prefix + ":GeneralLedger:00011";
        public const string CurrencyAccountMissing = Prefix + ":GeneralLedger:00012";
        public const string NothingToSettle = Prefix + ":GeneralLedger:00013";
        public const string InvalidDateRange = Prefix + ":GeneralLedger:00014";
        public const string NothingToAdjust = Prefix + ":GeneralLedger:00015";
    }

    public static class CashManagement
    {
        public const string BankAccountNotFound = Prefix + ":CashManagement:00001";
        public const string BankAccountBlocked = Prefix + ":CashManagement:00002";
        public const string BankAccountAlreadyExists = Prefix + ":CashManagement:00003";
        public const string PostingGroupNotFound = Prefix + ":CashManagement:00004";
        public const string CannotDeleteWithEntries = Prefix + ":CashManagement:00005";
        public const string InvalidBalAccountType = Prefix + ":CashManagement:00006";
        public const string VoucherNotOpen = Prefix + ":CashManagement:00007";
        public const string VoucherNotReleased = Prefix + ":CashManagement:00008";
        public const string VoucherHasNoLines = Prefix + ":CashManagement:00009";
        public const string VoucherFieldMissing = Prefix + ":CashManagement:00010";
        public const string VoucherAmountNotPositive = Prefix + ":CashManagement:00011";
        public const string DeductionExceedsAmount = Prefix + ":CashManagement:00012";
        public const string DeductionAccountMissing = Prefix + ":CashManagement:00013";
        public const string WrongDeductionType = Prefix + ":CashManagement:00014";
        public const string PaymentTypeBlocked = Prefix + ":CashManagement:00015";
        public const string VoucherAccountNotFound = Prefix + ":CashManagement:00016";
        public const string VoucherNosMissing = Prefix + ":CashManagement:00017";
        public const string DeductionCodeInUse = Prefix + ":CashManagement:00018";
        public const string InvalidVoucherAccountType = Prefix + ":CashManagement:00019";
    }

    public static class Inventory
    {
        public const string InsufficientInventory = Prefix + ":Inventory:00001";
        public const string LocationMandatory = Prefix + ":Inventory:00002";
    }

    public static class Purchasing
    {
        public const string VendorInvoiceNoRequired = Prefix + ":Purchasing:00001";
    }

    public static class Sales
    {
        public const string ExternalDocumentNoRequired = Prefix + ":Sales:00001";
        public const string CreditLimitExceeded = Prefix + ":Sales:00002";
    }

    public static class HumanResources
    {
        public const string EmployeeAlreadyExists = Prefix + ":HumanResources:00001";
        public const string InvalidAbsencePeriod = Prefix + ":HumanResources:00002";
        public const string EmployeeNotFound = Prefix + ":HumanResources:00003";
        public const string EmployeeBlocked = Prefix + ":HumanResources:00004";
        public const string PostingGroupNotFound = Prefix + ":HumanResources:00005";
        public const string CannotDeleteWithEntries = Prefix + ":HumanResources:00006";
    }

    public static class Querying
    {
        public const string InvalidFilter = Prefix + ":Querying:00001";
        public const string FieldNotFilterable = Prefix + ":Querying:00002";
        public const string InvalidValue = Prefix + ":Querying:00003";
        public const string OperatorNotSupported = Prefix + ":Querying:00004";
    }

    public static class Profiles
    {
        public const string UnknownProfile = Prefix + ":Profiles:00001";
    }

    public static class Pensions
    {
        public const string InvalidRetirementAge = Prefix + ":Pensions:00001";
        public const string SchemeClosed = Prefix + ":Pensions:00002";
        public const string NegativeAmount = Prefix + ":Pensions:00003";
        public const string DocumentNotOpen = Prefix + ":Pensions:00004";
        public const string DocumentNotReleased = Prefix + ":Pensions:00005";
        public const string NothingToPost = Prefix + ":Pensions:00006";
        public const string MemberDuplicated = Prefix + ":Pensions:00007";
        public const string MemberNotInScheme = Prefix + ":Pensions:00008";
        public const string MemberNotContributing = Prefix + ":Pensions:00009";
        public const string PeriodAlreadyPosted = Prefix + ":Pensions:00010";
        public const string InterestAlreadyAllocated = Prefix + ":Pensions:00011";
        public const string InvalidTaxBand = Prefix + ":Pensions:00012";
        public const string ProjectionCannotBePosted = Prefix + ":Pensions:00013";
        public const string BenefitOutOfDate = Prefix + ":Pensions:00014";
        public const string MemberHasEntries = Prefix + ":Pensions:00015";
        public const string SchemeInUse = Prefix + ":Pensions:00016";
        public const string SponsorHasMembers = Prefix + ":Pensions:00017";
        public const string SponsorBlocked = Prefix + ":Pensions:00018";
        public const string ScheduleHasLines = Prefix + ":Pensions:00019";
        public const string InterestPeriodOverlaps = Prefix + ":Pensions:00020";
        public const string VoucherAlreadyRaised = Prefix + ":Pensions:00021";
        public const string DocumentNotPosted = Prefix + ":Pensions:00022";
        public const string PayrollPeriodAlreadyPosted = Prefix + ":Pensions:00023";
        public const string PensionerDuplicated = Prefix + ":Pensions:00024";
        public const string PensionerNotInScheme = Prefix + ":Pensions:00025";
        public const string PensionerHasPayroll = Prefix + ":Pensions:00026";
        public const string NotDefinedBenefitScheme = Prefix + ":Pensions:00027";
        public const string MemberDataMissing = Prefix + ":Pensions:00028";
        public const string RetirementTooEarly = Prefix + ":Pensions:00029";
        public const string CommutationAboveMaximum = Prefix + ":Pensions:00030";
        public const string BeneficiaryShareExceeded = Prefix + ":Pensions:00031";
        public const string BeneficiarySharesIncomplete = Prefix + ":Pensions:00032";
        public const string ContributionRatePeriodOverlaps = Prefix + ":Pensions:00033";
        public const string PensionerNotSuspended = Prefix + ":Pensions:00034";
        public const string PensionerNotActive = Prefix + ":Pensions:00035";
        public const string IncrementAlreadyApplied = Prefix + ":Pensions:00036";
        public const string ExitDocumentsOutstanding = Prefix + ":Pensions:00037";
        public const string DeductionsExceedPension = Prefix + ":Pensions:00038";
        public const string BankBranchNotFound = Prefix + ":Pensions:00039";
        public const string PayItemInUse = Prefix + ":Pensions:00040";
    }

    public static class Academics
    {
        public const string NegativeAmount = Prefix + ":Academics:00001";
        public const string PeriodReversed = Prefix + ":Academics:00002";
        public const string InvalidGradingBand = Prefix + ":Academics:00003";
        public const string InvalidExamComponent = Prefix + ":Academics:00004";
        public const string InvalidCapacity = Prefix + ":Academics:00005";
        public const string UnitIsOwnPrerequisite = Prefix + ":Academics:00006";
        public const string ApplicationStatusWrong = Prefix + ":Academics:00007";
        public const string ProgrammeNotActive = Prefix + ":Academics:00008";
        public const string DocumentNotOpen = Prefix + ":Academics:00009";
        public const string StageRequired = Prefix + ":Academics:00010";
        public const string NothingToPost = Prefix + ":Academics:00011";
        public const string StudentNotActive = Prefix + ":Academics:00012";
        public const string RegistrationClosed = Prefix + ":Academics:00013";
        public const string StudentHasBalance = Prefix + ":Academics:00014";
        public const string NoUnitsSelected = Prefix + ":Academics:00015";
        public const string UnitCountOutOfRange = Prefix + ":Academics:00016";
        public const string UnitAlreadyRegistered = Prefix + ":Academics:00017";
        public const string PrerequisiteNotPassed = Prefix + ":Academics:00018";
        public const string ResultsEntryBlocked = Prefix + ":Academics:00019";
        public const string ExamComponentMissing = Prefix + ":Academics:00020";
        public const string StudentDuplicated = Prefix + ":Academics:00021";
        public const string StudentNotRegisteredForUnit = Prefix + ":Academics:00022";
        public const string MarkAlreadyAssigned = Prefix + ":Academics:00023";
        public const string MarkAboveMaximum = Prefix + ":Academics:00024";
        public const string RecordInUse = Prefix + ":Academics:00025";
        public const string UnitNotInProgramme = Prefix + ":Academics:00026";
        public const string DocumentHasLines = Prefix + ":Academics:00027";
        public const string RefundExceedsPrepayment = Prefix + ":Academics:00028";
        public const string StatusChangeNotAllowed = Prefix + ":Academics:00029";
        public const string StudentNotCleared = Prefix + ":Academics:00030";
        public const string BillNotOfStudent = Prefix + ":Academics:00031";
        public const string AmountNotPositive = Prefix + ":Academics:00032";
        public const string TimetableClash = Prefix + ":Academics:00033";
        public const string InvalidTimeRange = Prefix + ":Academics:00034";
        public const string ClassExceedsRoom = Prefix + ":Academics:00035";
        public const string NotEligibleForExam = Prefix + ":Academics:00036";
        public const string HostelGenderMismatch = Prefix + ":Academics:00037";
        public const string RoomFull = Prefix + ":Academics:00038";
        public const string RoomUnavailable = Prefix + ":Academics:00039";
        public const string StudentAlreadyAllocated = Prefix + ":Academics:00040";
        public const string StudentNotRegisteredForSemester = Prefix + ":Academics:00041";
        public const string DocumentStatusWrong = Prefix + ":Academics:00042";
        public const string ParticipantCountWrong = Prefix + ":Academics:00043";
        public const string ReasonRequired = Prefix + ":Academics:00044";
        public const string ExamDateRequired = Prefix + ":Academics:00045";
        public const string ItemNotInStock = Prefix + ":Academics:00046";
    }

    public static class Payroll
    {
        public const string NetPayNegative = Prefix + ":Payroll:00001";
        public const string RunStatusWrong = Prefix + ":Payroll:00002";
        public const string NothingToPost = Prefix + ":Payroll:00003";
        public const string PeriodAlreadyPosted = Prefix + ":Payroll:00004";
        public const string InvalidTaxBand = Prefix + ":Payroll:00005";
        public const string RecordInUse = Prefix + ":Payroll:00006";
        public const string VoucherAlreadyRaised = Prefix + ":Payroll:00007";
        public const string NegativeAmount = Prefix + ":Payroll:00008";
        public const string PayItemDuplicated = Prefix + ":Payroll:00009";
    }

    public static class Attachments
    {
        public const string FileEmpty = Prefix + ":Attachments:00001";
        public const string FileTooLarge = Prefix + ":Attachments:00002";
        public const string FileTypeNotAllowed = Prefix + ":Attachments:00003";
        public const string RecordNotFound = Prefix + ":Attachments:00004";
    }

    /// <summary>The plain base tables: setup, sub-ledgers and registers.</summary>
    public static class BaseTables
    {
        public const string RecordAlreadyExists = Prefix + ":BaseTables:00001";
    }

    public static class JobQueue
    {
        public const string JobQueueEntryNotFound = Prefix + ":JobQueue:00001";
        public const string JobQueueCategoryNotFound = Prefix + ":JobQueue:00002";
        public const string JobQueueCategoryCodeAlreadyExists = Prefix + ":JobQueue:00003";
        public const string JobQueueCannotRunInProcessJob = Prefix + ":JobQueue:00004";
        public const string JobQueueHandlerNotFound = Prefix + ":JobQueue:00005";
        public const string JobQueueCannotDeleteInProcessJob = Prefix + ":JobQueue:00006";
    }
}
