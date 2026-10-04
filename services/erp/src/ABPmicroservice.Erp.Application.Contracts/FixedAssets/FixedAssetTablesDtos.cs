using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.FixedAssets;

/// <summary>FA Classes.</summary>
public interface IFAClassAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public class FASubclassDto : CodeTableDto
{
    public string FAClassCode { get; set; }
    public string DefaultFAPostingGroup { get; set; }
}

public class CreateUpdateFASubclassDto : CreateUpdateCodeTableDto
{
    [StringLength(10)]
    public string FAClassCode { get; set; }

    [StringLength(20)]
    public string DefaultFAPostingGroup { get; set; }
}

/// <summary>FA Subclasses.</summary>
public interface IFASubclassAppService : ICrudAppService<FASubclassDto, Guid, GetCodeTableListInput, CreateUpdateFASubclassDto, CreateUpdateFASubclassDto> { }

/// <summary>FA Locations.</summary>
public interface IFALocationAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

/// <summary>Maintenance Codes.</summary>
public interface IMaintenanceAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public class DepreciationBookDto : CodeTableDto
{
    public bool GLIntegrationAcqCost { get; set; }
    public bool GLIntegrationDepreciation { get; set; }
    public bool GLIntegrationWriteDown { get; set; }
    public bool GLIntegrationAppreciation { get; set; }
    public bool GLIntegrationDisposal { get; set; }
    public bool GLIntegrationMaintenance { get; set; }
    public DepreciationBookDisposalCalculationMethod DisposalCalculationMethod { get; set; }
    public bool AllowDeprBelowZero { get; set; }
    public bool AllowIndexation { get; set; }
    public bool UseSameFAAndGLPostingDates { get; set; }
    public bool UseRoundingInPeriodicDepr { get; set; }
    public bool AllowChangesInDeprFields { get; set; }
    public decimal DefaultFinalRoundingAmount { get; set; }
    public decimal DefaultEndingBookValue { get; set; }
    public bool MarkErrorsAsCorrections { get; set; }
    public bool AllowAcqCostBelowZero { get; set; }
    public bool AllowIdenticalDocumentNo { get; set; }
    public bool FiscalYear365Days { get; set; }
}

public class CreateUpdateDepreciationBookDto : CreateUpdateCodeTableDto
{
    public bool GLIntegrationAcqCost { get; set; }

    public bool GLIntegrationDepreciation { get; set; }

    public bool GLIntegrationWriteDown { get; set; }

    public bool GLIntegrationAppreciation { get; set; }

    public bool GLIntegrationDisposal { get; set; }

    public bool GLIntegrationMaintenance { get; set; }

    public DepreciationBookDisposalCalculationMethod DisposalCalculationMethod { get; set; }

    public bool AllowDeprBelowZero { get; set; }

    public bool AllowIndexation { get; set; }

    public bool UseSameFAAndGLPostingDates { get; set; }

    public bool UseRoundingInPeriodicDepr { get; set; }

    public bool AllowChangesInDeprFields { get; set; }

    public decimal DefaultFinalRoundingAmount { get; set; }

    public decimal DefaultEndingBookValue { get; set; }

    public bool MarkErrorsAsCorrections { get; set; }

    public bool AllowAcqCostBelowZero { get; set; }

    public bool AllowIdenticalDocumentNo { get; set; }

    public bool FiscalYear365Days { get; set; }
}

/// <summary>Depreciation Books.</summary>
public interface IDepreciationBookAppService : ICrudAppService<DepreciationBookDto, Guid, GetCodeTableListInput, CreateUpdateDepreciationBookDto, CreateUpdateDepreciationBookDto> { }

public class FAPostingGroupDto : CodeTableDto
{
    public string AcquisitionCostAccount { get; set; }
    public string AccumDepreciationAccount { get; set; }
    public string WriteDownAccount { get; set; }
    public string AppreciationAccount { get; set; }
    public string AcqCostAccOnDisposal { get; set; }
    public string AccumDeprAccOnDisposal { get; set; }
    public string WriteDownAccOnDisposal { get; set; }
    public string AppreciationAccOnDisposal { get; set; }
    public string GainsAccOnDisposal { get; set; }
    public string LossesAccOnDisposal { get; set; }
    public string BookValAccOnDispGain { get; set; }
    public string SalesAccOnDispGain { get; set; }
    public string WriteDownBalAccOnDisp { get; set; }
    public string ApprecBalAccOnDisp { get; set; }
    public string MaintenanceExpenseAccount { get; set; }
    public string MaintenanceBalAcc { get; set; }
    public string AcquisitionCostBalAcc { get; set; }
    public string DepreciationExpenseAcc { get; set; }
    public string WriteDownExpenseAcc { get; set; }
    public string AppreciationBalAccount { get; set; }
    public string SalesBalAcc { get; set; }
    public string SalesAccOnDispLoss { get; set; }
    public string BookValAccOnDispLoss { get; set; }
}

public class CreateUpdateFAPostingGroupDto : CreateUpdateCodeTableDto
{
    [StringLength(20)]
    public string AcquisitionCostAccount { get; set; }

    [StringLength(20)]
    public string AccumDepreciationAccount { get; set; }

    [StringLength(20)]
    public string WriteDownAccount { get; set; }

    [StringLength(20)]
    public string AppreciationAccount { get; set; }

    [StringLength(20)]
    public string AcqCostAccOnDisposal { get; set; }

    [StringLength(20)]
    public string AccumDeprAccOnDisposal { get; set; }

    [StringLength(20)]
    public string WriteDownAccOnDisposal { get; set; }

    [StringLength(20)]
    public string AppreciationAccOnDisposal { get; set; }

    [StringLength(20)]
    public string GainsAccOnDisposal { get; set; }

    [StringLength(20)]
    public string LossesAccOnDisposal { get; set; }

    [StringLength(20)]
    public string BookValAccOnDispGain { get; set; }

    [StringLength(20)]
    public string SalesAccOnDispGain { get; set; }

    [StringLength(20)]
    public string WriteDownBalAccOnDisp { get; set; }

    [StringLength(20)]
    public string ApprecBalAccOnDisp { get; set; }

    [StringLength(20)]
    public string MaintenanceExpenseAccount { get; set; }

    [StringLength(20)]
    public string MaintenanceBalAcc { get; set; }

    [StringLength(20)]
    public string AcquisitionCostBalAcc { get; set; }

    [StringLength(20)]
    public string DepreciationExpenseAcc { get; set; }

    [StringLength(20)]
    public string WriteDownExpenseAcc { get; set; }

    [StringLength(20)]
    public string AppreciationBalAccount { get; set; }

    [StringLength(20)]
    public string SalesBalAcc { get; set; }

    [StringLength(20)]
    public string SalesAccOnDispLoss { get; set; }

    [StringLength(20)]
    public string BookValAccOnDispLoss { get; set; }
}

/// <summary>FA Posting Groups.</summary>
public interface IFAPostingGroupAppService : ICrudAppService<FAPostingGroupDto, Guid, GetCodeTableListInput, CreateUpdateFAPostingGroupDto, CreateUpdateFAPostingGroupDto> { }

public class FASetupDto
{
    public bool AllowPostingToMainAssets { get; set; }

    [StringLength(10)]
    public string DefaultDeprBook { get; set; }

    public DateTime? AllowFAPostingFrom { get; set; }

    public DateTime? AllowFAPostingTo { get; set; }

    [StringLength(20)]
    public string FixedAssetNos { get; set; }
}

/// <summary>FA Setup.</summary>
public interface IFASetupAppService : IApplicationService
{
    Task<FASetupDto> GetAsync();

    Task<FASetupDto> UpdateAsync(FASetupDto input);
}

public class FixedAssetDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public string SearchDescription { get; set; }
    public string Description2 { get; set; }
    public string FAClassCode { get; set; }
    public string FASubclassCode { get; set; }
    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public string LocationCode { get; set; }
    public string FALocationCode { get; set; }
    public string VendorNo { get; set; }
    public FAComponentType MainAssetComponent { get; set; }
    public string ComponentOfMainAsset { get; set; }
    public bool BudgetedAsset { get; set; }
    public DateTime? WarrantyDate { get; set; }
    public string ResponsibleEmployee { get; set; }
    public string SerialNo { get; set; }
    public bool Blocked { get; set; }
    public string MaintenanceVendorNo { get; set; }
    public bool UnderMaintenance { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public bool Inactive { get; set; }
    public string FAPostingGroup { get; set; }
}

public class CreateUpdateFixedAssetDto
{
    /// <summary>Blank takes the next number of the series.</summary>
    [StringLength(20)]
    public string No { get; set; }

    [Required]
    [StringLength(100)]
    public string Description { get; set; }

    [StringLength(100)]
    public string SearchDescription { get; set; }

    [StringLength(50)]
    public string Description2 { get; set; }

    [StringLength(10)]
    public string FAClassCode { get; set; }

    [StringLength(10)]
    public string FASubclassCode { get; set; }

    [StringLength(20)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(20)]
    public string GlobalDimension2Code { get; set; }

    [StringLength(10)]
    public string LocationCode { get; set; }

    [StringLength(10)]
    public string FALocationCode { get; set; }

    [StringLength(20)]
    public string VendorNo { get; set; }

    public FAComponentType MainAssetComponent { get; set; }

    [StringLength(20)]
    public string ComponentOfMainAsset { get; set; }

    public bool BudgetedAsset { get; set; }

    public DateTime? WarrantyDate { get; set; }

    [StringLength(20)]
    public string ResponsibleEmployee { get; set; }

    [StringLength(50)]
    public string SerialNo { get; set; }

    public bool Blocked { get; set; }

    [StringLength(20)]
    public string MaintenanceVendorNo { get; set; }

    public bool UnderMaintenance { get; set; }

    public DateTime? NextServiceDate { get; set; }

    public bool Inactive { get; set; }

    [StringLength(20)]
    public string FAPostingGroup { get; set; }
}

public class GetFixedAssetListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
}

/// <summary>Fixed Assets.</summary>
public interface IFixedAssetAppService : ICrudAppService<FixedAssetDto, Guid, GetFixedAssetListInput, CreateUpdateFixedAssetDto, CreateUpdateFixedAssetDto> { }

public class FADepreciationBookDto : FullAuditedEntityDto<Guid>
{
    public string FANo { get; set; }
    public string DepreciationBookCode { get; set; }
    public FADepreciationMethod DepreciationMethod { get; set; }
    public DateTime? DepreciationStartingDate { get; set; }
    public decimal StraightLinePct { get; set; }
    public decimal NoOfDepreciationYears { get; set; }
    public decimal NoOfDepreciationMonths { get; set; }
    public decimal FixedDeprAmount { get; set; }
    public decimal DecliningBalancePct { get; set; }
    public decimal FinalRoundingAmount { get; set; }
    public decimal EndingBookValue { get; set; }
    public string FAPostingGroup { get; set; }
    public DateTime? DepreciationEndingDate { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public DateTime? GLAcquisitionDate { get; set; }
    public DateTime? DisposalDate { get; set; }
    public DateTime? LastAcquisitionCostDate { get; set; }
    public DateTime? LastDepreciationDate { get; set; }
    public DateTime? LastWriteDownDate { get; set; }
    public DateTime? LastAppreciationDate { get; set; }
    public DateTime? LastSalvageValueDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? ProjectedDisposalDate { get; set; }
    public decimal ProjectedProceedsOnDisposal { get; set; }
    public bool UseHalfYearConvention { get; set; }
    public bool DefaultFADepreciationBook { get; set; }
}

public class CreateUpdateFADepreciationBookDto
{
    [Required]
    [StringLength(20)]
    public string FANo { get; set; }

    [Required]
    [StringLength(10)]
    public string DepreciationBookCode { get; set; }

    public FADepreciationMethod DepreciationMethod { get; set; }

    public DateTime? DepreciationStartingDate { get; set; }

    public decimal StraightLinePct { get; set; }

    public decimal NoOfDepreciationYears { get; set; }

    public decimal NoOfDepreciationMonths { get; set; }

    public decimal FixedDeprAmount { get; set; }

    public decimal DecliningBalancePct { get; set; }

    public decimal FinalRoundingAmount { get; set; }

    public decimal EndingBookValue { get; set; }

    [StringLength(20)]
    public string FAPostingGroup { get; set; }

    public DateTime? DepreciationEndingDate { get; set; }

    public DateTime? ProjectedDisposalDate { get; set; }

    public decimal ProjectedProceedsOnDisposal { get; set; }

    public bool UseHalfYearConvention { get; set; }

    public bool DefaultFADepreciationBook { get; set; }
}

public class GetFADepreciationBookListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string FANo { get; set; }
}

/// <summary>FA Depreciation Books.</summary>
public interface IFADepreciationBookAppService : ICrudAppService<FADepreciationBookDto, Guid, GetFADepreciationBookListInput, CreateUpdateFADepreciationBookDto, CreateUpdateFADepreciationBookDto> { }

public class FALedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public long GLEntryNo { get; set; }
    public string FANo { get; set; }
    public DateTime? FAPostingDate { get; set; }
    public DateTime? PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string DocumentNo { get; set; }
    public string ExternalDocumentNo { get; set; }
    public string Description { get; set; }
    public string DepreciationBookCode { get; set; }
    public FALedgerEntryFAPostingCategory FAPostingCategory { get; set; }
    public FALedgerEntryFAPostingType FAPostingType { get; set; }
    public decimal Amount { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public bool ReclassificationEntry { get; set; }
    public bool PartOfBookValue { get; set; }
    public bool PartOfDepreciableBasis { get; set; }
    public FALedgerEntryDisposalCalculationMethod DisposalCalculationMethod { get; set; }
    public long DisposalEntryNo { get; set; }
    public int NoOfDepreciationDays { get; set; }
    public decimal Quantity { get; set; }
    public string FASubclassCode { get; set; }
    public string FALocationCode { get; set; }
    public string FAPostingGroup { get; set; }
    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public string LocationCode { get; set; }
    public string UserId { get; set; }
    public FADepreciationMethod DepreciationMethod { get; set; }
    public DateTime? DepreciationStartingDate { get; set; }
    public decimal StraightLinePct { get; set; }
    public decimal NoOfDepreciationYears { get; set; }
    public string JournalBatchName { get; set; }
    public string SourceCode { get; set; }
    public string ReasonCode { get; set; }
    public long TransactionNo { get; set; }
    public GenJournalAccountType BalAccountType { get; set; }
    public string BalAccountNo { get; set; }
    public string FAClassCode { get; set; }
    public DateTime? DepreciationEndingDate { get; set; }
    public bool Reversed { get; set; }
    public long ReversedByEntryNo { get; set; }
    public long ReversedEntryNo { get; set; }
}

public class GetFALedgerEntryListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string FANo { get; set; }
    public string DocumentNo { get; set; }
}

/// <summary>FA Ledger Entries.</summary>
public interface IFALedgerEntryAppService : IReadOnlyAppService<FALedgerEntryDto, Guid, GetFALedgerEntryListInput> { }

public class MaintenanceRegistrationDto : FullAuditedEntityDto<Guid>
{
    public string FANo { get; set; }
    public int LineNo { get; set; }
    public DateTime? ServiceDate { get; set; }
    public string MaintenanceVendorNo { get; set; }
    public string Comment { get; set; }
    public string ServiceAgentName { get; set; }
    public string ServiceAgentPhoneNo { get; set; }
    public string ServiceAgentMobilePhone { get; set; }
}

public class CreateUpdateMaintenanceRegistrationDto
{
    [Required]
    [StringLength(20)]
    public string FANo { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    public DateTime? ServiceDate { get; set; }

    [StringLength(20)]
    public string MaintenanceVendorNo { get; set; }

    [StringLength(50)]
    public string Comment { get; set; }

    [StringLength(30)]
    public string ServiceAgentName { get; set; }

    [StringLength(30)]
    public string ServiceAgentPhoneNo { get; set; }

    [StringLength(30)]
    public string ServiceAgentMobilePhone { get; set; }
}

public class GetMaintenanceRegistrationListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string FANo { get; set; }
}

/// <summary>Maintenance Registrations.</summary>
public interface IMaintenanceRegistrationAppService : ICrudAppService<MaintenanceRegistrationDto, Guid, GetMaintenanceRegistrationListInput, CreateUpdateMaintenanceRegistrationDto, CreateUpdateMaintenanceRegistrationDto> { }

public class MainAssetComponentDto : FullAuditedEntityDto<Guid>
{
    public string MainAssetNo { get; set; }
    public string FANo { get; set; }
    public string Description { get; set; }
}

public class CreateUpdateMainAssetComponentDto
{
    [Required]
    [StringLength(20)]
    public string MainAssetNo { get; set; }

    [Required]
    [StringLength(20)]
    public string FANo { get; set; }

    [StringLength(100)]
    public string Description { get; set; }
}

public class GetMainAssetComponentListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string MainAssetNo { get; set; }
}

/// <summary>Main Asset Components.</summary>
public interface IMainAssetComponentAppService : ICrudAppService<MainAssetComponentDto, Guid, GetMainAssetComponentListInput, CreateUpdateMainAssetComponentDto, CreateUpdateMainAssetComponentDto> { }
