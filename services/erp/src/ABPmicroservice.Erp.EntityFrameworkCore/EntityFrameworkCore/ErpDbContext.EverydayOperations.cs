using ABPmicroservice.Erp.Auditing;
using ABPmicroservice.Erp.Emailing;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveLedgerEntry> LeaveLedgerEntries { get; set; }
    public DbSet<LeaveApplication> LeaveApplications { get; set; }

    public DbSet<ItemCharge> ItemCharges { get; set; }
    public DbSet<ItemChargeAssignment> ItemChargeAssignments { get; set; }

    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<SentEmailLog> SentEmailLogs { get; set; }

    public DbSet<ChangeLogSetup> ChangeLogSetups { get; set; }
    public DbSet<ChangeLogTable> ChangeLogTables { get; set; }
    public DbSet<ChangeLogEntry> ChangeLogEntries { get; set; }
}

/// <summary>Leave management, prepayments, item charges, document emailing and the change log.</summary>
public static class ErpEverydayOperationsModelCreatingExtensions
{
    public static void ConfigureErpEverydayOperations(this ModelBuilder builder)
    {
        builder.Entity<LeaveType>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LeaveTypes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.CauseOfAbsenceCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<LeaveLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LeaveLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.EntryNo });
            b.HasIndex(x => new { x.CompanyId, x.EmployeeId, x.LeaveTypeCode, x.LeavePeriodYear });
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.EmployeeName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.LeaveTypeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<LeaveApplication>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LeaveApplications", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.EmployeeId, x.StartDate });
            b.Ignore(x => x.Status);
            b.Ignore(x => x.ApprovalAmount);
            b.Ignore(x => x.HasLines);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.EmployeeName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.LeaveTypeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Reason).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.RelieverEmployeeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ApproverComment).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.RecallReason).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<HumanResourcesSetup>(b => b.Property(x => x.LeaveApplicationNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength));

        // Prepayments
        builder.Entity<GeneralPostingSetup>(b =>
        {
            b.Property(x => x.SalesPrepaymentsAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PurchPrepaymentsAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });
        builder.Entity<SalesHeader>(b =>
        {
            b.Property(x => x.LastPrepmtInvoiceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LastPrepmtCrMemoNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });
        builder.Entity<PurchaseHeader>(b =>
        {
            b.Property(x => x.LastPrepmtInvoiceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LastPrepmtCrMemoNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        // Item charges
        builder.Entity<ItemCharge>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemCharges", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.GenProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.VatProdPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
        });

        builder.Entity<ItemChargeAssignment>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemChargeAssignments", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.SourceType, x.SourceLineId });
            b.HasIndex(x => x.AppliesToEntryId);
            b.Property(x => x.SourceDocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ItemChargeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.AppliesToDocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        // Document emailing
        builder.Entity<EmailTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmailTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.SubjectTemplate).IsRequired().HasMaxLength(EmailTemplate.MaxSubjectLength);
            b.Property(x => x.BodyHtmlTemplate).IsRequired().HasMaxLength(EmailTemplate.MaxBodyLength);
        });

        builder.Entity<SentEmailLog>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SentEmailLogs", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.DocumentType, x.DocumentId });
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.PartyNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PartyName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.RecipientEmail).IsRequired().HasMaxLength(SentEmailLog.MaxEmailLength);
            b.Property(x => x.CcEmail).HasMaxLength(SentEmailLog.MaxEmailLength);
            b.Property(x => x.Subject).IsRequired().HasMaxLength(EmailTemplate.MaxSubjectLength);
            b.Property(x => x.BodyHtml).IsRequired().HasMaxLength(EmailTemplate.MaxBodyLength);
            b.Property(x => x.TemplateCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ErrorMessage).HasMaxLength(SentEmailLog.MaxErrorLength);
        });

        // Change log
        builder.Entity<ChangeLogSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ChangeLogSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            // One setup row per company.
            b.HasCompanyUniqueIndex();
        });

        builder.Entity<ChangeLogTable>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ChangeLogTables", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("EntityName");
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.TrackedFields).HasMaxLength(ChangeLogTable.MaxTrackedFieldsLength);
        });

        builder.Entity<ChangeLogEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ChangeLogEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ChangedAt });
            b.HasIndex(x => new { x.CompanyId, x.EntityName, x.RecordId });
            b.Property(x => x.UserName).HasMaxLength(256);
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PrimaryKey).HasMaxLength(ChangeLogEntry.MaxPrimaryKeyLength);
            b.Property(x => x.FieldName).IsRequired().HasMaxLength(ChangeLogEntry.MaxFieldNameLength);
            b.Property(x => x.OldValue).HasMaxLength(ChangeLogEntry.MaxValueLength);
            b.Property(x => x.NewValue).HasMaxLength(ChangeLogEntry.MaxValueLength);
        });
    }
}
