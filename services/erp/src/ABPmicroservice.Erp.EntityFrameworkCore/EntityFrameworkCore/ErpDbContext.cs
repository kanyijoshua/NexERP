using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Automations;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Integration;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Modules;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Profiles;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.RapidStart;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using ABPmicroservice.Erp.Workflows;
using Volo.Abp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Volo.Abp;
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
    public DbSet<GenJournalTemplate> GenJournalTemplates { get; set; }
    public DbSet<GenJournalBatch> GenJournalBatches { get; set; }
    public DbSet<GenJournalLine> GenJournalLines { get; set; }
    public DbSet<GLRegister> GLRegisters { get; set; }
    public DbSet<StandardGeneralJournal> StandardGeneralJournals { get; set; }
    public DbSet<StandardGeneralJournalLine> StandardGeneralJournalLines { get; set; }
    public DbSet<ErpNumberSequence> NumberSequences { get; set; }

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
    public DbSet<ApprovalUserSetup> ApprovalUserSetups { get; set; }

    public DbSet<NoSeries> NumberSeries { get; set; }
    public DbSet<NoSeriesLine> NumberSeriesLines { get; set; }
    public DbSet<SalesReceivablesSetup> SalesReceivablesSetups { get; set; }
    public DbSet<PurchasesPayablesSetup> PurchasesPayablesSetups { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserRoleCenter> UserRoleCenters { get; set; }

    public DbSet<AccountSchedule> AccountSchedules { get; set; }
    public DbSet<AccountScheduleLine> AccountScheduleLines { get; set; }
    public DbSet<ColumnLayout> ColumnLayouts { get; set; }
    public DbSet<ColumnLayoutLine> ColumnLayoutLines { get; set; }

    public DbSet<PublishedWebService> PublishedWebServices { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDelivery> WebhookDeliveries { get; set; }
    public DbSet<ExportTemplate> ExportTemplates { get; set; }

    public DbSet<DocumentNote> DocumentNotes { get; set; }
    public DbSet<ActivityStreamEntry> ActivityStreamEntries { get; set; }
    public DbSet<DocumentActivityTask> DocumentActivityTasks { get; set; }

    public DbSet<ErpModuleState> ErpModuleStates { get; set; }

    public DbSet<KanbanStage> KanbanStages { get; set; }
    public DbSet<AutomatedActionRule> AutomatedActionRules { get; set; }

    public DbSet<ReportLayoutSelection> ReportLayoutSelections { get; set; }
    public DbSet<CustomReportLayout> CustomReportLayouts { get; set; }

    /// <summary>
    /// Ledger columns that may still change after posting (application and closing bookkeeping).
    /// Everything else on an ILedgerEntry row is immutable.
    /// </summary>
    private static readonly HashSet<string> MutableLedgerProperties =
    [
        "Open",
        "RemainingAmount",
        "RemainingQuantity",
        "InvoicedQuantity",
        "ClosedByEntryId",
        "ClosedByEntryNo",
        "ClosedAtDate",
        "CostPostedToGL",
        // A reversal never deletes the original; it stamps it as reversed and points at its mirror.
        "Reversed",
        "ReversedByEntryNo",
    ];

    public ErpDbContext(DbContextOptions<ErpDbContext> options)
        : base(options)
    {
    }

    // Null at design time (migrations), where no filter values are needed.
    protected ICurrentCompany CurrentCompany => LazyServiceProvider?.LazyGetService<ICurrentCompany>();

    protected Guid? CurrentCompanyId => CurrentCompany?.Id;

    protected bool IsCompanyFilterEnabled => DataFilter?.IsEnabled<ICompanyScoped>() ?? false;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureErp();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Amounts, quantities and unit costs: numeric(18,5), as Business Central stores them.
        var decimals = configurationBuilder.Properties<decimal>().HavePrecision(18, 5);

        // SQLite (tests) has no decimal type and cannot SUM or ORDER BY it as text.
        if (Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            decimals.HaveConversion<double>();
        }
    }

    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        return typeof(ICompanyScoped).IsAssignableFrom(typeof(TEntity))
            || base.ShouldFilterEntity<TEntity>(entityType);
    }

    protected override Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(
        ModelBuilder modelBuilder
    )
    {
        var expression = base.CreateFilterExpression<TEntity>(modelBuilder);

        if (typeof(ICompanyScoped).IsAssignableFrom(typeof(TEntity)))
        {
            // No ambient company means no rows, never all rows.
            Expression<Func<TEntity, bool>> companyFilter = e =>
                !IsCompanyFilterEnabled
                || EF.Property<Guid>(e, nameof(ICompanyScoped.CompanyId)) == CurrentCompanyId;

            expression =
                expression == null
                    ? companyFilter
                    : QueryFilterExpressionHelper.CombineExpressions(expression, companyFilter);
        }

        return expression;
    }

    protected override void ApplyAbpConceptsForAddedEntity(EntityEntry entry)
    {
        base.ApplyAbpConceptsForAddedEntity(entry);

        if (entry.Entity is ICompanyScoped scoped && scoped.CompanyId == Guid.Empty)
        {
            var companyId =
                CurrentCompanyId
                ?? throw new BusinessException(ErpErrorCodes.Companies.CompanyRequired).WithData(
                    "EntityType",
                    entry.Metadata.ClrType.Name
                );

            entry.Property(nameof(ICompanyScoped.CompanyId)).CurrentValue = companyId;
        }
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        GuardLedgerEntries();

        // Collected before saving, while the change tracker still says what each row is doing.
        var changes = CollectEntityChanges();

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        BufferEntityChanges(changes);
        return result;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        GuardLedgerEntries();

        var changes = CollectEntityChanges();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);

        BufferEntityChanges(changes);
        return result;
    }

    /// <summary>
    /// Notes which registered rows this save touches, so webhook subscribers can be told once the
    /// transaction has committed. Tables nobody can subscribe to cost only a dictionary lookup.
    /// </summary>
    private List<EntityChangeNotification> CollectEntityChanges()
    {
        var registry = LazyServiceProvider?.LazyGetService<ErpEntityRegistry>();
        if (registry == null)
        {
            return null;
        }

        List<EntityChangeNotification> changes = null;

        foreach (var entry in ChangeTracker.Entries())
        {
            var kind = entry.State switch
            {
                EntityState.Added => EntityChangeKind.Created,
                EntityState.Modified => EntityChangeKind.Updated,
                EntityState.Deleted => EntityChangeKind.Deleted,
                _ => EntityChangeKind.None,
            };

            if (kind == EntityChangeKind.None || entry.Entity is not IEntity<Guid> entity)
            {
                continue;
            }

            var definition = registry.Find(entry.Metadata.ClrType.Name);
            if (definition == null || definition.EntityType != entry.Metadata.ClrType)
            {
                continue;
            }

            changes ??= [];
            changes.Add(new EntityChangeNotification(definition.Name, entity.Id, kind));
        }

        return changes;
    }

    private void BufferEntityChanges(List<EntityChangeNotification> changes)
    {
        if (changes == null || changes.Count == 0)
        {
            return;
        }

        var buffer = LazyServiceProvider?.LazyGetService<IEntityChangeBuffer>();
        if (buffer == null)
        {
            return;
        }

        foreach (var change in changes)
        {
            buffer.Add(change);
        }
    }

    /// <summary>Posted ledger rows are append-only: no deletes, and updates only to the bookkeeping columns.</summary>
    private void GuardLedgerEntries()
    {
        foreach (var entry in ChangeTracker.Entries<ILedgerEntry>())
        {
            if (entry.State == EntityState.Deleted)
            {
                throw LedgerIsImmutable(entry, "Delete");
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            var illegal = entry.Properties.FirstOrDefault(p =>
                p.IsModified && !MutableLedgerProperties.Contains(p.Metadata.Name)
            );
            if (illegal != null)
            {
                throw LedgerIsImmutable(entry, illegal.Metadata.Name);
            }
        }
    }

    private static BusinessException LedgerIsImmutable(EntityEntry<ILedgerEntry> entry, string what)
    {
        return new BusinessException(ErpErrorCodes.Ledgers.LedgerEntryIsImmutable)
            .WithData("EntityType", entry.Metadata.ClrType.Name)
            .WithData("EntryNo", entry.Entity.EntryNo)
            .WithData("Change", what);
    }
}
