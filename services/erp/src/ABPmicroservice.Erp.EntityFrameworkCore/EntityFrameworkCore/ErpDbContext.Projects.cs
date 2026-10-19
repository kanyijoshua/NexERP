using ABPmicroservice.Erp.Projects;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<ProjectsSetup> ProjectsSetups { get; set; }
    public DbSet<ProjectPostingGroup> ProjectPostingGroups { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<ProjectPlanningLine> ProjectPlanningLines { get; set; }
    public DbSet<ProjectJournalBatch> ProjectJournalBatches { get; set; }
    public DbSet<ProjectJournalLine> ProjectJournalLines { get; set; }
    public DbSet<ProjectLedgerEntry> ProjectLedgerEntries { get; set; }
    public DbSet<ResourceLedgerEntry> ResourceLedgerEntries { get; set; }
    public DbSet<ProjectWipEntry> ProjectWipEntries { get; set; }
    public DbSet<TimeSheetHeader> TimeSheetHeaders { get; set; }
    public DbSet<TimeSheetLine> TimeSheetLines { get; set; }
}

/// <summary>Projects, their tasks and plans, resources, time sheets, the project and resource ledgers and WIP.</summary>
public static class ErpProjectsModelCreatingExtensions
{
    public static void ConfigureErpProjects(this ModelBuilder builder)
    {
        builder.Entity<SalesLine>(b =>
        {
            b.Property(x => x.ProjectNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ProjectTaskNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });
        builder.Entity<PurchaseLine>(b =>
        {
            b.Property(x => x.ProjectNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ProjectTaskNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ProjectsSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectsSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProjectNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.ResourceNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.TimeSheetNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.DefaultPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
        });

        builder.Entity<ProjectPostingGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectPostingGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.ProjectCostsAccount).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ProjectSalesAccount).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.WipCostsAccount).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ProjectCostsAppliedAccount).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.WipSalesAccount).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.RecognizedSalesAccount).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<Resource>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Resources", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.EmployeeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });

        builder.Entity<Project>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Projects", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.PersonResponsible).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.HasMany(x => x.Tasks).WithOne().HasForeignKey(x => x.ProjectId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProjectTask>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectTasks", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.ProjectId, x.TaskNo }).IsUnique();
            b.Property(x => x.TaskNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ProjectPlanningLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectPlanningLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ProjectNo, x.TaskNo });
            b.Ignore(x => x.TotalCost);
            b.Ignore(x => x.TotalPrice);
            b.Ignore(x => x.IsBudget);
            b.Ignore(x => x.IsBillable);
            b.Ignore(x => x.QtyToInvoice);
            b.Property(x => x.ProjectNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.TaskNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.InvoiceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<ProjectJournalBatch>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectJournalBatches", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Name");
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ProjectJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.ProjectJournalBatchId, x.LineNo });
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ProjectNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.TaskNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.BalAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ProjectLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ProjectNo, x.TaskNo });
            b.Property(x => x.ProjectNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.TaskNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ResourceLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ResourceLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ResourceNo });
            b.Property(x => x.ResourceNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ProjectNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.TaskNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ProjectWipEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProjectWipEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ProjectNo });
            b.Property(x => x.ProjectNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.AccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.BalAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<TimeSheetHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TimeSheetHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Ignore(x => x.EndingDate);
            b.Ignore(x => x.TotalHours);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ResourceNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ResourceName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ApproverComment).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.TimeSheetHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TimeSheetLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TimeSheetLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Ignore(x => x.TotalHours);
            b.Ignore(x => x.Days);
            b.Property(x => x.ProjectNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.TaskNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });
    }
}
