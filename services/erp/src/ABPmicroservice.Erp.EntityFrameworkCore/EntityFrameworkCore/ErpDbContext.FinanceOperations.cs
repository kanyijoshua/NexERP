using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<DeferralTemplate> DeferralTemplates { get; set; }
    public DbSet<PostedDeferralHeader> PostedDeferralHeaders { get; set; }
    public DbSet<PostedDeferralLine> PostedDeferralLines { get; set; }
    public DbSet<ReminderTerms> ReminderTerms { get; set; }
    public DbSet<ReminderLevel> ReminderLevels { get; set; }
    public DbSet<FinanceChargeTerms> FinanceChargeTerms { get; set; }
    public DbSet<ReminderHeader> ReminderHeaders { get; set; }
    public DbSet<ReminderLine> ReminderLines { get; set; }
    public DbSet<FinanceChargeMemoHeader> FinanceChargeMemoHeaders { get; set; }
    public DbSet<FinanceChargeMemoLine> FinanceChargeMemoLines { get; set; }
    public DbSet<CashFlowForecast> CashFlowForecasts { get; set; }
    public DbSet<CashFlowManualEntry> CashFlowManualEntries { get; set; }
    public DbSet<CashFlowWorksheetLine> CashFlowWorksheetLines { get; set; }
    public DbSet<CashFlowForecastEntry> CashFlowForecastEntries { get; set; }
}

/// <summary>Applications, payment discounts, bank reconciliation, reminders, deferrals and cash flow.</summary>
public static class ErpFinanceOperationsModelCreatingExtensions
{
    public static void ConfigureErpFinanceOperations(this ModelBuilder builder)
    {
        // Reminders and finance charge memos share abstract bases for their logic only; each has
        // tables of its own, so the bases must not become an inheritance hierarchy.
        builder.Ignore<CollectionDocument>();
        builder.Ignore<CollectionDocumentLine>();

        builder.Entity<CustomerLedgerEntry>(b => b.HasIndex(x => new { x.CompanyId, x.CustomerId, x.Open }));
        builder.Entity<VendorLedgerEntry>(b => b.HasIndex(x => new { x.CompanyId, x.VendorId, x.Open }));
        builder.Entity<DetailedCustLedgEntry>(b =>
        {
            b.HasIndex(x => x.CustLedgerEntryNo);
            b.HasIndex(x => x.ApplicationNo);
        });
        builder.Entity<DetailedVendorLedgEntry>(b =>
        {
            b.HasIndex(x => x.VendorLedgerEntryNo);
            b.HasIndex(x => x.ApplicationNo);
        });

        builder.Entity<BankAccountLedgerEntry>(b =>
        {
            b.Property(x => x.StatementNo).HasMaxLength(20);
            b.HasIndex(x => new { x.CompanyId, x.BankAccountNo, x.StatementStatus });
        });

        builder.Entity<CustomerPostingGroup>(b =>
        {
            b.Property(x => x.PaymentDiscDebitAccNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PaymentDiscCreditAccNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.AdditionalFeeAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.InterestAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });
        builder.Entity<VendorPostingGroup>(b =>
        {
            b.Property(x => x.PaymentDiscDebitAccNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PaymentDiscCreditAccNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<SalesLine>(b => b.Property(x => x.DeferralCode).HasMaxLength(ErpDomainConsts.MaxDeferralTemplateCodeLength));
        builder.Entity<GenJournalLine>(b => b.Property(x => x.DeferralCode).HasMaxLength(ErpDomainConsts.MaxDeferralTemplateCodeLength));
        builder.Entity<SalesReceivablesSetup>(b =>
        {
            b.Property(x => x.ReminderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.IssuedReminderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.FinChargeMemoNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.IssuedFinChargeMemoNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
        });

        builder.Entity<DeferralTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DeferralTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxDeferralTemplateCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.DeferralAccountNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PeriodDescription).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<PostedDeferralHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedDeferralHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.DocumentNo });
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.DeferralCode).HasMaxLength(ErpDomainConsts.MaxDeferralTemplateCodeLength);
            b.Property(x => x.AccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DeferralAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedDeferralHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedDeferralLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedDeferralLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ReminderTerms>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReminderTerms", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxReminderTermsCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ReminderLevel>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReminderLevels", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ReminderTermsCode, x.No }).IsUnique();
            b.Property(x => x.ReminderTermsCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxReminderTermsCodeLength);
            b.Property(x => x.GracePeriod).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
            b.Property(x => x.DueDateCalculation).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
        });

        builder.Entity<FinanceChargeTerms>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FinanceChargeTerms", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxReminderTermsCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.GracePeriod).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
            b.Property(x => x.DueDateCalculation).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
        });

        builder.Entity<ReminderHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReminderHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            ConfigureCollectionDocument(b);
            b.Property(x => x.ReminderTermsCode).HasMaxLength(ErpDomainConsts.MaxReminderTermsCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ReminderHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReminderLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReminderLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            ConfigureCollectionLine(b);
        });

        builder.Entity<FinanceChargeMemoHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FinanceChargeMemoHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            ConfigureCollectionDocument(b);
            b.Property(x => x.FinChargeTermsCode).HasMaxLength(ErpDomainConsts.MaxReminderTermsCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.FinanceChargeMemoHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FinanceChargeMemoLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FinanceChargeMemoLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            ConfigureCollectionLine(b);
        });

        builder.Entity<CashFlowForecast>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CashFlowForecasts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.GLBudgetName).HasMaxLength(10);
        });

        builder.Entity<CashFlowManualEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CashFlowManualEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.RecurringFrequency).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
        });

        builder.Entity<CashFlowWorksheetLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CashFlowWorksheetLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.CashFlowForecastNo, x.LineNo });
            b.Property(x => x.CashFlowForecastNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SourceNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<CashFlowForecastEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CashFlowForecastEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.CashFlowForecastNo, x.CashFlowDate });
            b.Property(x => x.CashFlowForecastNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SourceNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });
    }

    private static void ConfigureCollectionDocument<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> b)
        where T : CollectionDocument
    {
        b.HasCompanyUniqueIndex("No");
        b.Ignore(x => x.CollectionLines);
        b.Ignore(x => x.TotalAmount);
        b.Ignore(x => x.RemainingAmount);
        b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        b.Property(x => x.CustomerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
        b.Property(x => x.IssuedNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
    }

    private static void ConfigureCollectionLine<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> b)
        where T : CollectionDocumentLine
    {
        b.Property(x => x.DocumentType).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        b.Property(x => x.AccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
    }
}
