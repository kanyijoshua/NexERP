using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<Bin> Bins { get; set; }
    public DbSet<WarehouseEntry> WarehouseEntries { get; set; }
    public DbSet<WarehouseActivityHeader> WarehouseActivityHeaders { get; set; }
    public DbSet<WarehouseActivityLine> WarehouseActivityLines { get; set; }
    public DbSet<ReservationEntry> ReservationEntries { get; set; }
    public DbSet<PhysInvtCountingPeriod> PhysInvtCountingPeriods { get; set; }
    public DbSet<PhysInvtOrderHeader> PhysInvtOrderHeaders { get; set; }
    public DbSet<PhysInvtOrderLine> PhysInvtOrderLines { get; set; }
    public DbSet<AssemblyBomComponent> AssemblyBomComponents { get; set; }
    public DbSet<AssemblyHeader> AssemblyHeaders { get; set; }
    public DbSet<AssemblyLine> AssemblyLines { get; set; }
}

/// <summary>Bins and warehouse documents, reservations, physical inventory orders, assembly and drop shipment links.</summary>
public static class ErpWarehouseModelCreatingExtensions
{
    public static void ConfigureErpWarehouse(this ModelBuilder builder)
    {
        builder.Entity<Location>(b =>
        {
            b.Property(x => x.ReceiptBinCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ShipmentBinCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.DefaultBinCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<Item>(b => b.Property(x => x.PhysInvtCountingPeriodCode).HasMaxLength(ErpDomainConsts.MaxCodeLength));

        builder.Entity<SalesLine>(b => b.Property(x => x.PurchaseOrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength));
        builder.Entity<PurchaseLine>(b => b.Property(x => x.SalesOrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength));

        builder.Entity<Bin>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Bins", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("LocationCode", "Code");
            b.Property(x => x.LocationCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<WarehouseEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WarehouseEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.LocationCode, x.BinCode, x.ItemId });
            b.Property(x => x.LocationCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.BinCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<WarehouseActivityHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WarehouseActivityHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LocationCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.SourceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.WarehouseActivityHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarehouseActivityLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WarehouseActivityLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Ignore(x => x.OutstandingQuantity);
            b.HasIndex(x => x.SourceLineId);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.FromBinCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ToBinCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<ReservationEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ReservationEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ItemId, x.LocationCode });
            b.HasIndex(x => x.SourceId);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SourceDocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PhysInvtCountingPeriod>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PhysInvtCountingPeriods", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<PhysInvtOrderHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PhysInvtOrderHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PhysInvtOrderHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PhysInvtOrderLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PhysInvtOrderLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Ignore(x => x.QtyDifference);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.BinCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LotNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<AssemblyBomComponent>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AssemblyBomComponents", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ParentItemNo });
            b.Property(x => x.ParentItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ComponentItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<AssemblyHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AssemblyHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Ignore(x => x.RemainingQuantity);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.BinCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.AssemblyHeaderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssemblyLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AssemblyLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Ignore(x => x.RemainingQuantity);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.VariantCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
        });
    }
}
