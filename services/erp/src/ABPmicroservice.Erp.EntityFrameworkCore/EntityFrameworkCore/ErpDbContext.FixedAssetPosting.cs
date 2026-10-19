using ABPmicroservice.Erp.FixedAssets;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<FAJournalBatch> FAJournalBatches { get; set; }
    public DbSet<FAJournalLine> FAJournalLines { get; set; }
}

/// <summary>FA journals: the lines that acquire, depreciate and dispose of fixed assets.</summary>
public static class ErpFixedAssetPostingModelCreatingExtensions
{
    public static void ConfigureErpFixedAssetPosting(this ModelBuilder builder)
    {
        builder.Entity<FALedgerEntry>(b => b.HasIndex(x => new { x.CompanyId, x.FANo, x.DepreciationBookCode }));

        builder.Entity<FAJournalBatch>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FAJournalBatches", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Name");
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.DepreciationBookCode).HasMaxLength(10);
        });

        builder.Entity<FAJournalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "FAJournalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.FAJournalBatchId, x.LineNo });
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.FANo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DepreciationBookCode).HasMaxLength(10);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.BalAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });
    }
}
