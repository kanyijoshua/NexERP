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
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public static class ErpDbContextModelCreatingExtensions
{
    public static void ConfigureErp(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<GLAccount>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLAccounts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<GLEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<GeneralPostingSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GeneralPostingSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<GenJournalBatch>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenJournalBatches", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<GenJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Customer>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Customers", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<CustomerPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomerPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<CustomerLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomerLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<SalesHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SalesHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.SalesHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SalesLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SalesLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<PostedSalesHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedSalesHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedSalesHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedSalesLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedSalesLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Vendor>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Vendors", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<VendorPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VendorPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<VendorLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VendorLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<PurchaseHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchaseHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PurchaseLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchaseLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<PostedPurchaseHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedPurchaseHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedPurchaseHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedPurchaseLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedPurchaseLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Item>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Items", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<ItemCategory>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemCategories", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<UnitOfMeasure>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "UnitsOfMeasure", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ItemLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ValueEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ValueEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ItemJournalBatch>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemJournalBatches", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ItemJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Dimension>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Dimensions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DimensionValue>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DimensionValues", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DefaultDimension>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DefaultDimensions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DimensionSetEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DimensionSetEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ConfigPackage>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackages", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Tables).WithOne().HasForeignKey(x => x.ConfigPackageId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConfigPackageTable>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackageTables", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Fields).WithOne().HasForeignKey(x => x.ConfigPackageTableId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConfigPackageField>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ConfigPackageFields", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Workflow>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Workflows", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.WorkflowId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorkflowStep>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WorkflowSteps", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ApprovalEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ApprovalEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<UserProfile>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "UserProfiles", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<UserRoleCenter>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "UserRoleCenters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<AccountSchedule>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AccountSchedules", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.AccountScheduleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AccountScheduleLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AccountScheduleLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<PublishedWebService>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PublishedWebServices", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Company>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Companies", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<CompanyInformation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CompanyInformations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DocumentNote>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DocumentNotes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ActivityStreamEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ActivityStreamEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<DocumentActivityTask>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DocumentActivityTasks", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<KanbanStage>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "KanbanStages", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<AutomatedActionRule>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AutomatedActionRules", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ReportLayoutSelection>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReportLayoutSelections", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<CustomReportLayout>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomReportLayouts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
        });
    }
}
