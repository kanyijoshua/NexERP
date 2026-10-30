using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.CostAccounting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Intercompany;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<DirectDebitMandate> DirectDebitMandates { get; set; }
    public DbSet<DirectDebitCollection> DirectDebitCollections { get; set; }
    public DbSet<DirectDebitCollectionEntry> DirectDebitCollectionEntries { get; set; }
    public DbSet<BusinessUnit> BusinessUnits { get; set; }
    public DbSet<ICPartner> ICPartners { get; set; }
    public DbSet<ICOutboxTransaction> ICOutboxTransactions { get; set; }
    public DbSet<ICInboxTransaction> ICInboxTransactions { get; set; }
    public DbSet<ICInboxLine> ICInboxLines { get; set; }
    public DbSet<CostAccountingSetup> CostAccountingSetups { get; set; }
    public DbSet<CostType> CostTypes { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<CostObject> CostObjects { get; set; }
    public DbSet<CostEntry> CostEntries { get; set; }
    public DbSet<CostRegister> CostRegisters { get; set; }
    public DbSet<CostAllocationSource> CostAllocationSources { get; set; }
    public DbSet<CostAllocationTarget> CostAllocationTargets { get; set; }
    public DbSet<StandardSalesCode> StandardSalesCodes { get; set; }
    public DbSet<StandardSalesLine> StandardSalesLines { get; set; }
    public DbSet<StandardCustomerSalesCode> StandardCustomerSalesCodes { get; set; }
    public DbSet<StandardPurchaseCode> StandardPurchaseCodes { get; set; }
    public DbSet<StandardPurchaseLine> StandardPurchaseLines { get; set; }
    public DbSet<StandardVendorPurchaseCode> StandardVendorPurchaseCodes { get; set; }
}

/// <summary>
/// Payment files and direct debits, consolidation, intercompany, cost accounting, responsibility
/// centers on sales documents and standard (recurring) sales and purchase lines.
/// </summary>
public static class ErpFinanceCompletionModelCreatingExtensions
{
    public static void ConfigureErpFinanceCompletion(this ModelBuilder builder)
    {
        builder.Entity<GenJournalLine>(b => b.Property(x => x.RecipientBankAccount).HasMaxLength(ErpDomainConsts.MaxCodeLength));
        builder.Entity<SalesHeader>(b => b.Property(x => x.ResponsibilityCenter).HasMaxLength(10));

        builder.Entity<DirectDebitMandate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DirectDebitMandates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.HasIndex(x => new { x.CompanyId, x.CustomerNo });
            b.Property(x => x.Code).IsRequired().HasMaxLength(35);
            b.Property(x => x.CustomerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerBankAccountCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<DirectDebitCollection>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DirectDebitCollections", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.BankAccountNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CreditorIdentifier).HasMaxLength(35);
            b.HasMany(x => x.Entries).WithOne().HasForeignKey(x => x.DirectDebitCollectionId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DirectDebitCollectionEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "DirectDebitCollectionEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.CustomerLedgerEntryId);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.MandateCode).HasMaxLength(35);
        });

        builder.Entity<BusinessUnit>(b =>
        {
            CodeTable(b, "BusinessUnits");
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.ResidualAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ICPartner>(b =>
        {
            CodeTable(b, "ICPartners");
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VendorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DefaultGLAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ICOutboxTransaction>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ICOutboxTransactions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.TransactionNo });
            b.Property(x => x.PartnerCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<ICInboxTransaction>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ICInboxTransactions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.TransactionNo });
            b.Property(x => x.PartnerCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.PartnerDocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.CreatedDocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ICInboxTransactionId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ICInboxLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ICInboxLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });

        builder.Entity<CostAccountingSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CostAccountingSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex();
            b.Property(x => x.CostCenterDimension).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CostObjectDimension).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<CostType>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CostTypes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.GLAccountRange).HasMaxLength(250);
            b.Property(x => x.Totaling).HasMaxLength(250);
            b.Property(x => x.CostCenterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CostObjectCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<CostCenter>(b => CodeTable(b, "CostCenters"));
        builder.Entity<CostObject>(b => CodeTable(b, "CostObjects"));

        builder.Entity<CostEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CostEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.CostTypeNo, x.CostCenterCode });
            b.HasIndex(x => new { x.CompanyId, x.CostTypeNo, x.CostObjectCode });
            b.Property(x => x.CostTypeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CostCenterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CostObjectCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.GLAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.AllocationCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<CostRegister>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CostRegisters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.No });
        });

        builder.Entity<CostAllocationSource>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CostAllocationSources", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.CostTypeRange).IsRequired().HasMaxLength(250);
            b.Property(x => x.CostCenterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CostObjectCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CreditToCostTypeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.HasMany(x => x.Targets).WithOne().HasForeignKey(x => x.CostAllocationSourceId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CostAllocationTarget>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CostAllocationTargets", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CostTypeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CostCenterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CostObjectCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<StandardSalesCode>(b =>
        {
            CodeTable(b, "StandardSalesCodes");
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StandardSalesCodeId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StandardSalesLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardSalesLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });

        builder.Entity<StandardCustomerSalesCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardCustomerSalesCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("CustomerNo", "Code");
            b.Property(x => x.CustomerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<StandardPurchaseCode>(b =>
        {
            CodeTable(b, "StandardPurchaseCodes");
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StandardPurchaseCodeId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StandardPurchaseLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardPurchaseLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });

        builder.Entity<StandardVendorPurchaseCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "StandardVendorPurchaseCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("VendorNo", "Code");
            b.Property(x => x.VendorNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });
    }

    private static void CodeTable<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> b, string table)
        where T : Companies.CodeTableEntity
    {
        b.ToTable(ErpDbProperties.DbTablePrefix + table, ErpDbProperties.DbSchema);
        b.ConfigureByConvention();
        b.HasCompanyUniqueIndex("Code");
        b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
    }
}
