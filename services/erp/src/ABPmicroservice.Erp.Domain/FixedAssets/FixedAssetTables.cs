using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.FixedAssets;

/// <summary>FA Class.</summary>
public class FAClass : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected FAClass() { }

    public FAClass(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>FA Subclass.</summary>
public class FASubclass : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    public string FAClassCode { get; private set; }
    public string DefaultFAPostingGroup { get; private set; }

    protected FASubclass() { }

    public FASubclass(Guid id, string code, string description = null)
        : base(id, code, description) { }

    public void Set(string faClassCode, string defaultFAPostingGroup)
    {
        FAClassCode = CodeTableEntity.NormalizeCode(Check.Length(faClassCode, nameof(faClassCode), 10));
        DefaultFAPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(defaultFAPostingGroup, nameof(defaultFAPostingGroup), 20));
    }
}

/// <summary>FA Location.</summary>
public class FALocation : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected FALocation() { }

    public FALocation(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Maintenance.</summary>
public class Maintenance : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    protected Maintenance() { }

    public Maintenance(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Depreciation Book.</summary>
public class DepreciationBook : CodeTableEntity
{
    protected override int MaxCodeLength => 10;

    public bool GLIntegrationAcqCost { get; private set; }
    public bool GLIntegrationDepreciation { get; private set; }
    public bool GLIntegrationWriteDown { get; private set; }
    public bool GLIntegrationAppreciation { get; private set; }
    public bool GLIntegrationDisposal { get; private set; }
    public bool GLIntegrationMaintenance { get; private set; }
    public DepreciationBookDisposalCalculationMethod DisposalCalculationMethod { get; private set; }
    public bool AllowDeprBelowZero { get; private set; }
    public bool AllowIndexation { get; private set; }
    public bool UseSameFAAndGLPostingDates { get; private set; }
    public bool UseRoundingInPeriodicDepr { get; private set; }
    public bool AllowChangesInDeprFields { get; private set; }
    public decimal DefaultFinalRoundingAmount { get; private set; }
    public decimal DefaultEndingBookValue { get; private set; }
    public bool MarkErrorsAsCorrections { get; private set; }
    public bool AllowAcqCostBelowZero { get; private set; }
    public bool AllowIdenticalDocumentNo { get; private set; }
    public bool FiscalYear365Days { get; private set; }

    protected DepreciationBook() { }

    public DepreciationBook(Guid id, string code, string description = null)
        : base(id, code, description) { }

    public void Set(
        bool glIntegrationAcqCost,
        bool glIntegrationDepreciation,
        bool glIntegrationWriteDown,
        bool glIntegrationAppreciation,
        bool glIntegrationDisposal,
        bool glIntegrationMaintenance,
        DepreciationBookDisposalCalculationMethod disposalCalculationMethod,
        bool allowDeprBelowZero,
        bool allowIndexation,
        bool useSameFAAndGLPostingDates,
        bool useRoundingInPeriodicDepr,
        bool allowChangesInDeprFields,
        decimal defaultFinalRoundingAmount,
        decimal defaultEndingBookValue,
        bool markErrorsAsCorrections,
        bool allowAcqCostBelowZero,
        bool allowIdenticalDocumentNo,
        bool fiscalYear365Days
    )
    {
        GLIntegrationAcqCost = glIntegrationAcqCost;
        GLIntegrationDepreciation = glIntegrationDepreciation;
        GLIntegrationWriteDown = glIntegrationWriteDown;
        GLIntegrationAppreciation = glIntegrationAppreciation;
        GLIntegrationDisposal = glIntegrationDisposal;
        GLIntegrationMaintenance = glIntegrationMaintenance;
        DisposalCalculationMethod = disposalCalculationMethod;
        AllowDeprBelowZero = allowDeprBelowZero;
        AllowIndexation = allowIndexation;
        UseSameFAAndGLPostingDates = useSameFAAndGLPostingDates;
        UseRoundingInPeriodicDepr = useRoundingInPeriodicDepr;
        AllowChangesInDeprFields = allowChangesInDeprFields;
        DefaultFinalRoundingAmount = defaultFinalRoundingAmount;
        DefaultEndingBookValue = defaultEndingBookValue;
        MarkErrorsAsCorrections = markErrorsAsCorrections;
        AllowAcqCostBelowZero = allowAcqCostBelowZero;
        AllowIdenticalDocumentNo = allowIdenticalDocumentNo;
        FiscalYear365Days = fiscalYear365Days;
    }
}

/// <summary>FA Posting Group.</summary>
public class FAPostingGroup : CodeTableEntity
{
    public string AcquisitionCostAccount { get; private set; }
    public string AccumDepreciationAccount { get; private set; }
    public string WriteDownAccount { get; private set; }
    public string AppreciationAccount { get; private set; }
    public string AcqCostAccOnDisposal { get; private set; }
    public string AccumDeprAccOnDisposal { get; private set; }
    public string WriteDownAccOnDisposal { get; private set; }
    public string AppreciationAccOnDisposal { get; private set; }
    public string GainsAccOnDisposal { get; private set; }
    public string LossesAccOnDisposal { get; private set; }
    public string BookValAccOnDispGain { get; private set; }
    public string SalesAccOnDispGain { get; private set; }
    public string WriteDownBalAccOnDisp { get; private set; }
    public string ApprecBalAccOnDisp { get; private set; }
    public string MaintenanceExpenseAccount { get; private set; }
    public string MaintenanceBalAcc { get; private set; }
    public string AcquisitionCostBalAcc { get; private set; }
    public string DepreciationExpenseAcc { get; private set; }
    public string WriteDownExpenseAcc { get; private set; }
    public string AppreciationBalAccount { get; private set; }
    public string SalesBalAcc { get; private set; }
    public string SalesAccOnDispLoss { get; private set; }
    public string BookValAccOnDispLoss { get; private set; }

    protected FAPostingGroup() { }

    public FAPostingGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }

    public void Set(
        string acquisitionCostAccount,
        string accumDepreciationAccount,
        string writeDownAccount,
        string appreciationAccount,
        string acqCostAccOnDisposal,
        string accumDeprAccOnDisposal,
        string writeDownAccOnDisposal,
        string appreciationAccOnDisposal,
        string gainsAccOnDisposal,
        string lossesAccOnDisposal,
        string bookValAccOnDispGain,
        string salesAccOnDispGain,
        string writeDownBalAccOnDisp,
        string apprecBalAccOnDisp,
        string maintenanceExpenseAccount,
        string maintenanceBalAcc,
        string acquisitionCostBalAcc,
        string depreciationExpenseAcc,
        string writeDownExpenseAcc,
        string appreciationBalAccount,
        string salesBalAcc,
        string salesAccOnDispLoss,
        string bookValAccOnDispLoss
    )
    {
        AcquisitionCostAccount = CodeTableEntity.NormalizeCode(Check.Length(acquisitionCostAccount, nameof(acquisitionCostAccount), 20));
        AccumDepreciationAccount = CodeTableEntity.NormalizeCode(Check.Length(accumDepreciationAccount, nameof(accumDepreciationAccount), 20));
        WriteDownAccount = CodeTableEntity.NormalizeCode(Check.Length(writeDownAccount, nameof(writeDownAccount), 20));
        AppreciationAccount = CodeTableEntity.NormalizeCode(Check.Length(appreciationAccount, nameof(appreciationAccount), 20));
        AcqCostAccOnDisposal = CodeTableEntity.NormalizeCode(Check.Length(acqCostAccOnDisposal, nameof(acqCostAccOnDisposal), 20));
        AccumDeprAccOnDisposal = CodeTableEntity.NormalizeCode(Check.Length(accumDeprAccOnDisposal, nameof(accumDeprAccOnDisposal), 20));
        WriteDownAccOnDisposal = CodeTableEntity.NormalizeCode(Check.Length(writeDownAccOnDisposal, nameof(writeDownAccOnDisposal), 20));
        AppreciationAccOnDisposal = CodeTableEntity.NormalizeCode(Check.Length(appreciationAccOnDisposal, nameof(appreciationAccOnDisposal), 20));
        GainsAccOnDisposal = CodeTableEntity.NormalizeCode(Check.Length(gainsAccOnDisposal, nameof(gainsAccOnDisposal), 20));
        LossesAccOnDisposal = CodeTableEntity.NormalizeCode(Check.Length(lossesAccOnDisposal, nameof(lossesAccOnDisposal), 20));
        BookValAccOnDispGain = CodeTableEntity.NormalizeCode(Check.Length(bookValAccOnDispGain, nameof(bookValAccOnDispGain), 20));
        SalesAccOnDispGain = CodeTableEntity.NormalizeCode(Check.Length(salesAccOnDispGain, nameof(salesAccOnDispGain), 20));
        WriteDownBalAccOnDisp = CodeTableEntity.NormalizeCode(Check.Length(writeDownBalAccOnDisp, nameof(writeDownBalAccOnDisp), 20));
        ApprecBalAccOnDisp = CodeTableEntity.NormalizeCode(Check.Length(apprecBalAccOnDisp, nameof(apprecBalAccOnDisp), 20));
        MaintenanceExpenseAccount = CodeTableEntity.NormalizeCode(Check.Length(maintenanceExpenseAccount, nameof(maintenanceExpenseAccount), 20));
        MaintenanceBalAcc = CodeTableEntity.NormalizeCode(Check.Length(maintenanceBalAcc, nameof(maintenanceBalAcc), 20));
        AcquisitionCostBalAcc = CodeTableEntity.NormalizeCode(Check.Length(acquisitionCostBalAcc, nameof(acquisitionCostBalAcc), 20));
        DepreciationExpenseAcc = CodeTableEntity.NormalizeCode(Check.Length(depreciationExpenseAcc, nameof(depreciationExpenseAcc), 20));
        WriteDownExpenseAcc = CodeTableEntity.NormalizeCode(Check.Length(writeDownExpenseAcc, nameof(writeDownExpenseAcc), 20));
        AppreciationBalAccount = CodeTableEntity.NormalizeCode(Check.Length(appreciationBalAccount, nameof(appreciationBalAccount), 20));
        SalesBalAcc = CodeTableEntity.NormalizeCode(Check.Length(salesBalAcc, nameof(salesBalAcc), 20));
        SalesAccOnDispLoss = CodeTableEntity.NormalizeCode(Check.Length(salesAccOnDispLoss, nameof(salesAccOnDispLoss), 20));
        BookValAccOnDispLoss = CodeTableEntity.NormalizeCode(Check.Length(bookValAccOnDispLoss, nameof(bookValAccOnDispLoss), 20));
    }
}

/// <summary>FA Setup: one row per company.</summary>
public class FASetup : CompanyEntity
{
    public bool AllowPostingToMainAssets { get; private set; }
    public string DefaultDeprBook { get; private set; }
    public DateTime? AllowFAPostingFrom { get; private set; }
    public DateTime? AllowFAPostingTo { get; private set; }
    public string FixedAssetNos { get; private set; }

    protected FASetup() { }

    public FASetup(Guid id)
        : base(id) { }

    public void Set(
        bool allowPostingToMainAssets,
        string defaultDeprBook,
        DateTime? allowFAPostingFrom,
        DateTime? allowFAPostingTo,
        string fixedAssetNos
    )
    {
        AllowPostingToMainAssets = allowPostingToMainAssets;
        DefaultDeprBook = CodeTableEntity.NormalizeCode(Check.Length(defaultDeprBook, nameof(defaultDeprBook), 10));
        AllowFAPostingFrom = allowFAPostingFrom?.Date;
        AllowFAPostingTo = allowFAPostingTo?.Date;
        FixedAssetNos = CodeTableEntity.NormalizeCode(Check.Length(fixedAssetNos, nameof(fixedAssetNos), 20));
    }
}

public class FASetupManager : DomainService
{
    private readonly IRepository<FASetup, Guid> _repository;

    public FASetupManager(IRepository<FASetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<FASetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new FASetup(GuidGenerator.Create()), autoSave: true);
    }
}

/// <summary>Fixed Asset.</summary>
public class FixedAsset : CompanyEntity, IHasNo
{
    public string No { get; private set; }

    public string Description { get; private set; }
    public string SearchDescription { get; private set; }
    public string Description2 { get; private set; }
    public string FAClassCode { get; private set; }
    public string FASubclassCode { get; private set; }
    public string GlobalDimension1Code { get; private set; }
    public string GlobalDimension2Code { get; private set; }
    public string LocationCode { get; private set; }
    public string FALocationCode { get; private set; }
    public string VendorNo { get; private set; }
    public FAComponentType MainAssetComponent { get; private set; }
    public string ComponentOfMainAsset { get; private set; }
    public bool BudgetedAsset { get; private set; }
    public DateTime? WarrantyDate { get; private set; }
    public string ResponsibleEmployee { get; private set; }
    public string SerialNo { get; private set; }
    public bool Blocked { get; private set; }
    public string MaintenanceVendorNo { get; private set; }
    public bool UnderMaintenance { get; private set; }
    public DateTime? NextServiceDate { get; private set; }
    public bool Inactive { get; private set; }
    public string FAPostingGroup { get; private set; }

    protected FixedAsset() { }

    public FixedAsset(Guid id, string no)
        : base(id)
    {
        SetKey(no);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string no)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 20).Trim().ToUpperInvariant();
    }

    public void Set(
        string description,
        string searchDescription,
        string description2,
        string faClassCode,
        string faSubclassCode,
        string globalDimension1Code,
        string globalDimension2Code,
        string locationCode,
        string faLocationCode,
        string vendorNo,
        FAComponentType mainAssetComponent,
        string componentOfMainAsset,
        bool budgetedAsset,
        DateTime? warrantyDate,
        string responsibleEmployee,
        string serialNo,
        bool blocked,
        string maintenanceVendorNo,
        bool underMaintenance,
        DateTime? nextServiceDate,
        bool inactive,
        string faPostingGroup
    )
    {
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 100);
        SearchDescription = CodeTableEntity.NormalizeCode(Check.Length(searchDescription, nameof(searchDescription), 100));
        Description2 = Check.Length(description2, nameof(description2), 50);
        FAClassCode = CodeTableEntity.NormalizeCode(Check.Length(faClassCode, nameof(faClassCode), 10));
        FASubclassCode = CodeTableEntity.NormalizeCode(Check.Length(faSubclassCode, nameof(faSubclassCode), 10));
        GlobalDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension1Code, nameof(globalDimension1Code), 20));
        GlobalDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension2Code, nameof(globalDimension2Code), 20));
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), 10));
        FALocationCode = CodeTableEntity.NormalizeCode(Check.Length(faLocationCode, nameof(faLocationCode), 10));
        VendorNo = CodeTableEntity.NormalizeCode(Check.Length(vendorNo, nameof(vendorNo), 20));
        MainAssetComponent = mainAssetComponent;
        ComponentOfMainAsset = CodeTableEntity.NormalizeCode(Check.Length(componentOfMainAsset, nameof(componentOfMainAsset), 20));
        BudgetedAsset = budgetedAsset;
        WarrantyDate = warrantyDate?.Date;
        ResponsibleEmployee = CodeTableEntity.NormalizeCode(Check.Length(responsibleEmployee, nameof(responsibleEmployee), 20));
        SerialNo = Check.Length(serialNo, nameof(serialNo), 50);
        Blocked = blocked;
        MaintenanceVendorNo = CodeTableEntity.NormalizeCode(Check.Length(maintenanceVendorNo, nameof(maintenanceVendorNo), 20));
        UnderMaintenance = underMaintenance;
        NextServiceDate = nextServiceDate?.Date;
        Inactive = inactive;
        FAPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(faPostingGroup, nameof(faPostingGroup), 20));
    }
}

/// <summary>FA Depreciation Book.</summary>
public class FADepreciationBook : CompanyEntity
{
    public string FANo { get; private set; }
    public string DepreciationBookCode { get; private set; }

    public FADepreciationMethod DepreciationMethod { get; private set; }
    public DateTime? DepreciationStartingDate { get; private set; }
    public decimal StraightLinePct { get; private set; }
    public decimal NoOfDepreciationYears { get; private set; }
    public decimal NoOfDepreciationMonths { get; private set; }
    public decimal FixedDeprAmount { get; private set; }
    public decimal DecliningBalancePct { get; private set; }
    public decimal FinalRoundingAmount { get; private set; }
    public decimal EndingBookValue { get; private set; }
    public string FAPostingGroup { get; private set; }
    public DateTime? DepreciationEndingDate { get; private set; }
    public DateTime? AcquisitionDate { get; internal set; }
    public DateTime? GLAcquisitionDate { get; internal set; }
    public DateTime? DisposalDate { get; internal set; }
    public DateTime? LastAcquisitionCostDate { get; internal set; }
    public DateTime? LastDepreciationDate { get; internal set; }
    public DateTime? LastWriteDownDate { get; internal set; }
    public DateTime? LastAppreciationDate { get; internal set; }
    public DateTime? LastSalvageValueDate { get; internal set; }
    public DateTime? LastMaintenanceDate { get; internal set; }
    public DateTime? ProjectedDisposalDate { get; private set; }
    public decimal ProjectedProceedsOnDisposal { get; private set; }
    public bool UseHalfYearConvention { get; private set; }
    public bool DefaultFADepreciationBook { get; private set; }

    protected FADepreciationBook() { }

    public FADepreciationBook(Guid id, string faNo, string depreciationBookCode)
        : base(id)
    {
        SetKey(faNo, depreciationBookCode);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string faNo, string depreciationBookCode)
    {
        FANo = Check.NotNullOrWhiteSpace(faNo, nameof(faNo), 20).Trim().ToUpperInvariant();
        DepreciationBookCode = Check.NotNullOrWhiteSpace(depreciationBookCode, nameof(depreciationBookCode), 10).Trim().ToUpperInvariant();
    }

    public void Set(
        FADepreciationMethod depreciationMethod,
        DateTime? depreciationStartingDate,
        decimal straightLinePct,
        decimal noOfDepreciationYears,
        decimal noOfDepreciationMonths,
        decimal fixedDeprAmount,
        decimal decliningBalancePct,
        decimal finalRoundingAmount,
        decimal endingBookValue,
        string faPostingGroup,
        DateTime? depreciationEndingDate,
        DateTime? projectedDisposalDate,
        decimal projectedProceedsOnDisposal,
        bool useHalfYearConvention,
        bool defaultFADepreciationBook
    )
    {
        DepreciationMethod = depreciationMethod;
        DepreciationStartingDate = depreciationStartingDate?.Date;
        StraightLinePct = straightLinePct;
        NoOfDepreciationYears = noOfDepreciationYears;
        NoOfDepreciationMonths = noOfDepreciationMonths;
        FixedDeprAmount = fixedDeprAmount;
        DecliningBalancePct = decliningBalancePct;
        FinalRoundingAmount = finalRoundingAmount;
        EndingBookValue = endingBookValue;
        FAPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(faPostingGroup, nameof(faPostingGroup), 20));
        DepreciationEndingDate = depreciationEndingDate?.Date;
        ProjectedDisposalDate = projectedDisposalDate?.Date;
        ProjectedProceedsOnDisposal = projectedProceedsOnDisposal;
        UseHalfYearConvention = useHalfYearConvention;
        DefaultFADepreciationBook = defaultFADepreciationBook;
    }
}

/// <summary>FA Ledger Entry.</summary>
public class FALedgerEntry : LedgerEntryBase
{
    public long GLEntryNo { get; internal set; }
    public string FANo { get; internal set; }
    public DateTime? FAPostingDate { get; internal set; }
    public DateTime? PostingDate { get; internal set; }
    public GLEntryDocumentType DocumentType { get; internal set; }
    public DateTime? DocumentDate { get; internal set; }
    public string DocumentNo { get; internal set; }
    public string ExternalDocumentNo { get; internal set; }
    public string Description { get; internal set; }
    public string DepreciationBookCode { get; internal set; }
    public FALedgerEntryFAPostingCategory FAPostingCategory { get; internal set; }
    public FALedgerEntryFAPostingType FAPostingType { get; internal set; }
    public decimal Amount { get; internal set; }
    public decimal DebitAmount { get; internal set; }
    public decimal CreditAmount { get; internal set; }
    public bool ReclassificationEntry { get; internal set; }
    public bool PartOfBookValue { get; internal set; }
    public bool PartOfDepreciableBasis { get; internal set; }
    public FALedgerEntryDisposalCalculationMethod DisposalCalculationMethod { get; internal set; }
    public long DisposalEntryNo { get; internal set; }
    public int NoOfDepreciationDays { get; internal set; }
    public decimal Quantity { get; internal set; }
    public string FASubclassCode { get; internal set; }
    public string FALocationCode { get; internal set; }
    public string FAPostingGroup { get; internal set; }
    public string GlobalDimension1Code { get; internal set; }
    public string GlobalDimension2Code { get; internal set; }
    public string LocationCode { get; internal set; }
    public string UserId { get; internal set; }
    public FADepreciationMethod DepreciationMethod { get; internal set; }
    public DateTime? DepreciationStartingDate { get; internal set; }
    public decimal StraightLinePct { get; internal set; }
    public decimal NoOfDepreciationYears { get; internal set; }
    public string JournalBatchName { get; internal set; }
    public string SourceCode { get; internal set; }
    public string ReasonCode { get; internal set; }
    public long TransactionNo { get; internal set; }
    public GenJournalAccountType BalAccountType { get; internal set; }
    public string BalAccountNo { get; internal set; }
    public string FAClassCode { get; internal set; }
    public DateTime? DepreciationEndingDate { get; internal set; }
    public bool Reversed { get; internal set; }
    public long ReversedByEntryNo { get; internal set; }
    public long ReversedEntryNo { get; internal set; }

    protected FALedgerEntry() { }

    public FALedgerEntry(Guid id)
        : base(id) { }
}

/// <summary>Maintenance Registration.</summary>
public class MaintenanceRegistration : CompanyEntity
{
    public string FANo { get; private set; }
    public int LineNo { get; private set; }

    public DateTime? ServiceDate { get; private set; }
    public string MaintenanceVendorNo { get; private set; }
    public string Comment { get; private set; }
    public string ServiceAgentName { get; private set; }
    public string ServiceAgentPhoneNo { get; private set; }
    public string ServiceAgentMobilePhone { get; private set; }

    protected MaintenanceRegistration() { }

    public MaintenanceRegistration(Guid id, string faNo, int lineNo)
        : base(id)
    {
        SetKey(faNo, lineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string faNo, int lineNo)
    {
        FANo = Check.NotNullOrWhiteSpace(faNo, nameof(faNo), 20).Trim().ToUpperInvariant();
        LineNo = lineNo;
    }

    public void Set(
        DateTime? serviceDate,
        string maintenanceVendorNo,
        string comment,
        string serviceAgentName,
        string serviceAgentPhoneNo,
        string serviceAgentMobilePhone
    )
    {
        ServiceDate = serviceDate?.Date;
        MaintenanceVendorNo = CodeTableEntity.NormalizeCode(Check.Length(maintenanceVendorNo, nameof(maintenanceVendorNo), 20));
        Comment = Check.Length(comment, nameof(comment), 50);
        ServiceAgentName = Check.Length(serviceAgentName, nameof(serviceAgentName), 30);
        ServiceAgentPhoneNo = Check.Length(serviceAgentPhoneNo, nameof(serviceAgentPhoneNo), 30);
        ServiceAgentMobilePhone = Check.Length(serviceAgentMobilePhone, nameof(serviceAgentMobilePhone), 30);
    }
}

/// <summary>Main Asset Component.</summary>
public class MainAssetComponent : CompanyEntity
{
    public string MainAssetNo { get; private set; }
    public string FANo { get; private set; }

    public string Description { get; private set; }

    protected MainAssetComponent() { }

    public MainAssetComponent(Guid id, string mainAssetNo, string faNo)
        : base(id)
    {
        SetKey(mainAssetNo, faNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string mainAssetNo, string faNo)
    {
        MainAssetNo = Check.NotNullOrWhiteSpace(mainAssetNo, nameof(mainAssetNo), 20).Trim().ToUpperInvariant();
        FANo = Check.NotNullOrWhiteSpace(faNo, nameof(faNo), 20).Trim().ToUpperInvariant();
    }

    public void Set(string description)
    {
        Description = Check.Length(description, nameof(description), 100);
    }
}
