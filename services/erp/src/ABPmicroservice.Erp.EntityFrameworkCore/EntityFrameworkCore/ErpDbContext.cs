using ABPmicroservice.Erp.Automations;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Profiles;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.RapidStart;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.WebServices;
using ABPmicroservice.Erp.Workflows;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

[ConnectionStringName(ErpDbProperties.ConnectionStringName)]
public class ErpDbContext : AbpDbContext<ErpDbContext>
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyInformation> CompanyInformations { get; set; }

    public DbSet<GLAccount> GLAccounts { get; set; }
    public DbSet<GLEntry> GLEntries { get; set; }
    public DbSet<GeneralPostingSetup> GeneralPostingSetups { get; set; }
    public DbSet<GenJournalBatch> GenJournalBatches { get; set; }
    public DbSet<GenJournalLine> GenJournalLines { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerPostingGroup> CustomerPostingGroups { get; set; }
    public DbSet<CustomerLedgerEntry> CustomerLedgerEntries { get; set; }
    public DbSet<SalesHeader> SalesHeaders { get; set; }
    public DbSet<SalesLine> SalesLines { get; set; }
    public DbSet<PostedSalesHeader> PostedSalesHeaders { get; set; }
    public DbSet<PostedSalesLine> PostedSalesLines { get; set; }

    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorPostingGroup> VendorPostingGroups { get; set; }
    public DbSet<VendorLedgerEntry> VendorLedgerEntries { get; set; }
    public DbSet<PurchaseHeader> PurchaseHeaders { get; set; }
    public DbSet<PurchaseLine> PurchaseLines { get; set; }
    public DbSet<PostedPurchaseHeader> PostedPurchaseHeaders { get; set; }
    public DbSet<PostedPurchaseLine> PostedPurchaseLines { get; set; }

    public DbSet<Item> Items { get; set; }
    public DbSet<ItemCategory> ItemCategories { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<ItemLedgerEntry> ItemLedgerEntries { get; set; }
    public DbSet<ValueEntry> ValueEntries { get; set; }
    public DbSet<ItemJournalBatch> ItemJournalBatches { get; set; }
    public DbSet<ItemJournalLine> ItemJournalLines { get; set; }

    public DbSet<Dimension> Dimensions { get; set; }
    public DbSet<DimensionValue> DimensionValues { get; set; }
    public DbSet<DefaultDimension> DefaultDimensions { get; set; }
    public DbSet<DimensionSetEntry> DimensionSetEntries { get; set; }

    public DbSet<ConfigPackage> ConfigPackages { get; set; }
    public DbSet<ConfigPackageTable> ConfigPackageTables { get; set; }
    public DbSet<ConfigPackageField> ConfigPackageFields { get; set; }

    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<ApprovalEntry> ApprovalEntries { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserRoleCenter> UserRoleCenters { get; set; }

    public DbSet<AccountSchedule> AccountSchedules { get; set; }
    public DbSet<AccountScheduleLine> AccountScheduleLines { get; set; }

    public DbSet<PublishedWebService> PublishedWebServices { get; set; }

    public DbSet<DocumentNote> DocumentNotes { get; set; }
    public DbSet<ActivityStreamEntry> ActivityStreamEntries { get; set; }
    public DbSet<DocumentActivityTask> DocumentActivityTasks { get; set; }

    public DbSet<KanbanStage> KanbanStages { get; set; }
    public DbSet<AutomatedActionRule> AutomatedActionRules { get; set; }

    public DbSet<ReportLayoutSelection> ReportLayoutSelections { get; set; }
    public DbSet<CustomReportLayout> CustomReportLayouts { get; set; }

    public ErpDbContext(DbContextOptions<ErpDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureErp();
    }
}
