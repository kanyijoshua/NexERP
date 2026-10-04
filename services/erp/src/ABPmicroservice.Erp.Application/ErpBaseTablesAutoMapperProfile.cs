using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Workflows;
using AutoMapper;

namespace ABPmicroservice.Erp;

/// <summary>Entity to DTO maps of the base tables; property names match one for one.</summary>
public class ErpBaseTablesAutoMapperProfile : Profile
{
    public ErpBaseTablesAutoMapperProfile()
    {
        // Finance
        CreateMap<SourceCode, CodeTableDto>();
        CreateMap<ReasonCode, CodeTableDto>();
        CreateMap<CountryRegion, CountryRegionDto>();
        CreateMap<PostCode, PostCodeDto>();
        CreateMap<ShipmentMethod, CodeTableDto>();
        CreateMap<ResponsibilityCenter, ResponsibilityCenterDto>();
        CreateMap<UserSetup, UserSetupDto>();
        CreateMap<GLBudgetName, GLBudgetNameDto>();
        CreateMap<GLBudgetEntry, GLBudgetEntryDto>();
        CreateMap<CommentLine, CommentLineDto>();

        // Sales
        CreateMap<CustomerBankAccount, CustomerBankAccountDto>();
        CreateMap<DetailedCustLedgEntry, DetailedCustLedgEntryDto>();

        // Purchasing
        CreateMap<VendorBankAccount, VendorBankAccountDto>();
        CreateMap<DetailedVendorLedgEntry, DetailedVendorLedgEntryDto>();
        CreateMap<OrderAddress, OrderAddressDto>();
        CreateMap<ItemVendor, ItemVendorDto>();
        CreateMap<PurchCommentLine, PurchCommentLineDto>();

        // Cash Management
        CreateMap<BankAccReconciliation, BankAccReconciliationDto>();
        CreateMap<BankAccReconciliationLine, BankAccReconciliationLineDto>();
        CreateMap<BankAccountStatement, BankAccountStatementDto>();
        CreateMap<BankAccountStatementLine, BankAccountStatementLineDto>();
        CreateMap<CheckLedgerEntry, CheckLedgerEntryDto>();

        // Human Resources
        CreateMap<Relative, CodeTableDto>();
        CreateMap<MiscArticle, CodeTableDto>();
        CreateMap<Confidential, CodeTableDto>();
        CreateMap<EmployeeStatisticsGroup, CodeTableDto>();
        CreateMap<EmployeeRelative, EmployeeRelativeDto>();
        CreateMap<EmployeeQualification, EmployeeQualificationDto>();
        CreateMap<MiscArticleInformation, MiscArticleInformationDto>();
        CreateMap<ConfidentialInformation, ConfidentialInformationDto>();
        CreateMap<AlternativeAddress, AlternativeAddressDto>();
        CreateMap<HumanResourceCommentLine, HumanResourceCommentLineDto>();

        // Fixed Assets
        CreateMap<FAClass, CodeTableDto>();
        CreateMap<FASubclass, FASubclassDto>();
        CreateMap<FALocation, CodeTableDto>();
        CreateMap<Maintenance, CodeTableDto>();
        CreateMap<DepreciationBook, DepreciationBookDto>();
        CreateMap<FAPostingGroup, FAPostingGroupDto>();
        CreateMap<FASetup, FASetupDto>();
        CreateMap<FixedAsset, FixedAssetDto>();
        CreateMap<FADepreciationBook, FADepreciationBookDto>();
        CreateMap<FALedgerEntry, FALedgerEntryDto>();
        CreateMap<MaintenanceRegistration, MaintenanceRegistrationDto>();
        CreateMap<MainAssetComponent, MainAssetComponentDto>();

        // Reporting
        CreateMap<ReportSelection, ReportSelectionDto>();
        CreateMap<CustomReportSelection, CustomReportSelectionDto>();

        // Workflows
        CreateMap<WorkflowUserGroup, CodeTableDto>();
        CreateMap<WorkflowUserGroupMember, WorkflowUserGroupMemberDto>();
        CreateMap<ApprovalCommentLine, ApprovalCommentLineDto>();
    }
}
