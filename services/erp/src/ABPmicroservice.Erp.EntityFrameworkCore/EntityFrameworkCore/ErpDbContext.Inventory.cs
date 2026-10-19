using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<ItemVariant> ItemVariants { get; set; }
    public DbSet<ItemUnitOfMeasure> ItemUnitsOfMeasure { get; set; }
    public DbSet<ItemTrackingCode> ItemTrackingCodes { get; set; }
    public DbSet<LotNoInformation> LotNoInformation { get; set; }
    public DbSet<SerialNoInformation> SerialNoInformation { get; set; }
    public DbSet<ItemTrackingLine> ItemTrackingLines { get; set; }
    public DbSet<ItemApplicationEntry> ItemApplicationEntries { get; set; }
    public DbSet<ItemJournalTemplate> ItemJournalTemplates { get; set; }
    public DbSet<TransferHeader> TransferHeaders { get; set; }
    public DbSet<TransferLine> TransferLines { get; set; }
    public DbSet<PostedTransfer> PostedTransfers { get; set; }
    public DbSet<PostedTransferLine> PostedTransferLines { get; set; }
}

/// <summary>Costing, variants, units of measure, lot and serial tracking, item journals and transfers.</summary>
public static class ErpInventoryModelCreatingExtensions
{
    public static void ConfigureErpInventory(this ModelBuilder builder)
    {
        builder.Entity<Item>(b =>
        {
            b.Property(x => x.ItemTrackingCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });


        builder.Entity<InventorySetup>(b =>
        {
            b.Property(x => x.TransferOrderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.PostedTransferShipmentNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.PostedTransferReceiptNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
        });

        builder.Entity<ItemLedgerEntry>(b =>
        {
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LotNo).HasMaxLength(ErpDomainConsts.MaxLotNoLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxLotNoLength);
            b.HasIndex(x => new { x.CompanyId, x.ItemId, x.LocationCode, x.VariantCode, x.Open, x.Positive });
            b.HasIndex(x => x.DocumentLineId);
        });

        builder.Entity<ValueEntry>(b =>
        {
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.HasIndex(x => x.ItemLedgerEntryId);
        });

        builder.Entity<SalesLine>(b => b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength));
        builder.Entity<PurchaseLine>(b => b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength));
        builder.Entity<PostedSalesShipmentLine>(b => b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength));
        builder.Entity<PostedPurchaseReceiptLine>(b => b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength));

        builder.Entity<ItemJournalLine>(b =>
        {
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.NewLocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
            b.Property(x => x.LotNo).HasMaxLength(ErpDomainConsts.MaxLotNoLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxLotNoLength);
            b.HasIndex(x => new { x.ItemJournalBatchId, x.LineNo });
        });

        builder.Entity<ItemJournalTemplate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemJournalTemplates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Name");
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxJournalTemplateNameLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ItemVariant>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemVariants", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ItemNo, x.Code }).IsUnique();
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ItemUnitOfMeasure>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemUnitsOfMeasure", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ItemNo, x.Code }).IsUnique();
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });

        builder.Entity<ItemTrackingCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemTrackingCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<LotNoInformation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LotNoInformation", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ItemNo, x.VariantCode, x.LotNo });
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LotNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxLotNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<SerialNoInformation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SerialNoInformation", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ItemNo, x.VariantCode, x.SerialNo });
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SerialNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxLotNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ItemTrackingLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemTrackingLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.SourceType, x.SourceId });
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LotNo).HasMaxLength(ErpDomainConsts.MaxLotNoLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxLotNoLength);
        });

        builder.Entity<ItemApplicationEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ItemApplicationEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.InboundEntryId);
            b.HasIndex(x => x.OutboundEntryId);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<TransferHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TransferHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.FromLocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.ToLocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.InTransitCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.LastShipmentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LastReceiptNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.TransferHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TransferLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TransferLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });

        builder.Entity<PostedTransfer>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedTransfers", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.TransferOrderNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.TransferOrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.FromLocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.ToLocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.InTransitCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedTransferId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedTransferLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedTransferLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.TransferLineId);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });
    }
}
