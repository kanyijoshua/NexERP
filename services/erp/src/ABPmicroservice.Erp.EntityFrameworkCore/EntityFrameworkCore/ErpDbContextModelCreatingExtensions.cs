using ABPmicroservice.Erp.Automations;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Integration;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Profiles;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.RapidStart;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
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
            b.HasCompanyUniqueIndex("Code");
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
            b.Property(x => x.SenderUserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.ApproverUserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Property(x => x.Comment).HasMaxLength(ErpDomainConsts.MaxCommentLength);
            b.Ignore(x => x.IsPending);
            // "Requests to approve" and "pending entries of a document" are the two hot lookups.
            b.HasIndex(x => new { x.ApproverId, x.Status });
            b.HasIndex(x => new { x.DocumentKind, x.DocumentId });
        });

        builder.Entity<ApprovalUserSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ApprovalUserSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.UserName).IsRequired().HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.HasCompanyUniqueIndex("UserId");
        });

        builder.Entity<NoSeries>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "NoSeries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.NoSeriesId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<NoSeriesLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "NoSeriesLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.StartingNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.EndingNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.WarningNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            // Two transactions that read the same last number cannot both take the next one.
            b.Property(x => x.LastNoUsed).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength).IsConcurrencyToken();
        });

        builder.Entity<SalesReceivablesSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SalesReceivablesSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // One setup row per company.
            b.HasCompanyUniqueIndex();
        });

        builder.Entity<PurchasesPayablesSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchasesPayablesSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex();
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

        builder.Entity<ColumnLayout>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ColumnLayouts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ColumnLayoutId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasCompanyUniqueIndex("Name");
        });

        builder.Entity<ColumnLayoutLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ColumnLayoutLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ColumnNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxRowNoLength);
            b.Property(x => x.ColumnHeader).HasMaxLength(ErpDomainConsts.MaxColumnHeaderLength);
            b.Property(x => x.ComparisonDateFormula).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
        });

        builder.Entity<GenJournalTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GenJournalTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.SourceCode).HasMaxLength(ErpDomainConsts.MaxSourceCodeLength);
            b.Property(x => x.NoSeriesCode).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.HasCompanyUniqueIndex("Name");
        });

        builder.Entity<GLRegister>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "GLRegisters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SourceCode).HasMaxLength(ErpDomainConsts.MaxSourceCodeLength);
            b.Property(x => x.JournalBatchName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.UserName).HasMaxLength(ErpDomainConsts.MaxUserNameLength);
            b.Ignore(x => x.IsEmpty);
            b.Ignore(x => x.IsReversible);
            b.HasCompanyUniqueIndex("No");
        });

        builder.Entity<StandardGeneralJournal>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardGeneralJournals", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.JournalTemplateName).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StandardGeneralJournalId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasCompanyUniqueIndex("JournalTemplateName", "Code");
        });

        builder.Entity<StandardGeneralJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardGeneralJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AccountNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ErpNumberSequence>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "NumberSequences", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.HasCompanyUniqueIndex("Name");
        });

        builder.Entity<PublishedWebService>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PublishedWebServices", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ServiceName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.ExcludedFields).HasMaxLength(ErpDomainConsts.MaxTotalingLength);
            b.HasCompanyUniqueIndex("ServiceName");
        });

        builder.Entity<WebhookSubscription>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WebhookSubscriptions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.EndpointUrl).IsRequired().HasMaxLength(ErpDomainConsts.MaxUrlLength);
            b.Property(x => x.Secret).HasMaxLength(ErpDomainConsts.MaxWebhookSecretLength);
            b.Property(x => x.LastError).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasIndex(x => new { x.EntityName, x.Active });
        });

        builder.Entity<WebhookDelivery>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WebhookDeliveries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.Error).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            // The worker asks for exactly this: what is due, oldest first.
            b.HasIndex(x => new { x.Status, x.NextAttemptTime });
            b.HasIndex(x => x.SubscriptionId);
        });

        builder.Entity<ExportTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExportTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.Fields).IsRequired().HasMaxLength(ErpDomainConsts.MaxTotalingLength);
            b.HasCompanyUniqueIndex("EntityName", "Name");
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
