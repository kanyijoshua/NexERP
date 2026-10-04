using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Workflows;
using Microsoft.EntityFrameworkCore;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>The base tables beyond the posting kernel: setup, sub-ledgers and registers.</summary>
public partial class ErpDbContext
{
    // Finance
    public DbSet<SourceCode> SourceCodes { get; set; }
    public DbSet<ReasonCode> ReasonCodes { get; set; }
    public DbSet<CountryRegion> CountriesRegions { get; set; }
    public DbSet<PostCode> PostCodes { get; set; }
    public DbSet<ShipmentMethod> ShipmentMethods { get; set; }
    public DbSet<ResponsibilityCenter> ResponsibilityCenters { get; set; }
    public DbSet<UserSetup> UserSetups { get; set; }
    public DbSet<GLBudgetName> GLBudgetNames { get; set; }
    public DbSet<GLBudgetEntry> GLBudgetEntries { get; set; }
    public DbSet<CommentLine> CommentLines { get; set; }

    // Sales
    public DbSet<CustomerBankAccount> CustomerBankAccounts { get; set; }
    public DbSet<DetailedCustLedgEntry> DetailedCustLedgEntries { get; set; }

    // Purchasing
    public DbSet<VendorBankAccount> VendorBankAccounts { get; set; }
    public DbSet<DetailedVendorLedgEntry> DetailedVendorLedgEntries { get; set; }
    public DbSet<OrderAddress> OrderAddresses { get; set; }
    public DbSet<ItemVendor> ItemVendors { get; set; }
    public DbSet<PurchCommentLine> PurchCommentLines { get; set; }

    // Cash Management
    public DbSet<BankAccReconciliation> BankAccReconciliations { get; set; }
    public DbSet<BankAccReconciliationLine> BankAccReconciliationLines { get; set; }
    public DbSet<BankAccountStatement> BankAccountStatements { get; set; }
    public DbSet<BankAccountStatementLine> BankAccountStatementLines { get; set; }
    public DbSet<CheckLedgerEntry> CheckLedgerEntries { get; set; }

    // Human Resources
    public DbSet<Relative> Relatives { get; set; }
    public DbSet<MiscArticle> MiscArticles { get; set; }
    public DbSet<Confidential> Confidentials { get; set; }
    public DbSet<EmployeeStatisticsGroup> EmployeeStatisticsGroups { get; set; }
    public DbSet<EmployeeRelative> EmployeeRelatives { get; set; }
    public DbSet<EmployeeQualification> EmployeeQualifications { get; set; }
    public DbSet<MiscArticleInformation> MiscArticleInformations { get; set; }
    public DbSet<ConfidentialInformation> ConfidentialInformations { get; set; }
    public DbSet<AlternativeAddress> AlternativeAddresses { get; set; }
    public DbSet<HumanResourceCommentLine> HumanResourceCommentLines { get; set; }

    // Fixed Assets
    public DbSet<FAClass> FAClasses { get; set; }
    public DbSet<FASubclass> FASubclasses { get; set; }
    public DbSet<FALocation> FALocations { get; set; }
    public DbSet<Maintenance> Maintenances { get; set; }
    public DbSet<DepreciationBook> DepreciationBooks { get; set; }
    public DbSet<FAPostingGroup> FAPostingGroups { get; set; }
    public DbSet<FASetup> FASetups { get; set; }
    public DbSet<FixedAsset> FixedAssets { get; set; }
    public DbSet<FADepreciationBook> FADepreciationBooks { get; set; }
    public DbSet<FALedgerEntry> FALedgerEntries { get; set; }
    public DbSet<MaintenanceRegistration> MaintenanceRegistrations { get; set; }
    public DbSet<MainAssetComponent> MainAssetComponents { get; set; }

    // Reporting
    public DbSet<ReportSelection> ReportSelections { get; set; }
    public DbSet<CustomReportSelection> CustomReportSelections { get; set; }

    // Workflows
    public DbSet<WorkflowUserGroup> WorkflowUserGroups { get; set; }
    public DbSet<WorkflowUserGroupMember> WorkflowUserGroupMembers { get; set; }
    public DbSet<ApprovalCommentLine> ApprovalCommentLines { get; set; }
}
