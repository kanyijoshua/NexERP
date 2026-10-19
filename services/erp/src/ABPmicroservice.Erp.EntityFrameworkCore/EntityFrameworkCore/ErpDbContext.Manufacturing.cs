using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Manufacturing;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<ManufacturingSetup> ManufacturingSetups { get; set; }
    public DbSet<WorkCenter> WorkCenters { get; set; }
    public DbSet<MachineCenter> MachineCenters { get; set; }
    public DbSet<ProductionBom> ProductionBoms { get; set; }
    public DbSet<ProductionBomLine> ProductionBomLines { get; set; }
    public DbSet<Routing> Routings { get; set; }
    public DbSet<RoutingLine> RoutingLines { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProdOrderComponent> ProdOrderComponents { get; set; }
    public DbSet<ProdOrderRoutingLine> ProdOrderRoutingLines { get; set; }
    public DbSet<CapacityLedgerEntry> CapacityLedgerEntries { get; set; }
    public DbSet<PlanningLine> PlanningLines { get; set; }
}

/// <summary>BOMs, routings, work and machine centers, production orders, the capacity ledger and planning.</summary>
public static class ErpManufacturingModelCreatingExtensions
{
    public static void ConfigureErpManufacturing(this ModelBuilder builder)
    {
        builder.Entity<Item>(b =>
        {
            b.Property(x => x.ProductionBomNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.RoutingNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LeadTimeCalculation).HasMaxLength(ErpDomainConsts.MaxDateFormulaLength);
            b.Property(x => x.VendorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ManufacturingSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ManufacturingSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProductionOrderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.WipAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CapacityAppliedAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VarianceAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<WorkCenter>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WorkCenters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<MachineCenter>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MachineCenters", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.WorkCenterCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<ProductionBom>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProductionBoms", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ProductionBomId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductionBomLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProductionBomLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<Routing>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Routings", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.RoutingId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RoutingLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "RoutingLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OperationNo).IsRequired().HasMaxLength(10);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ProductionOrder>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProductionOrders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Ignore(x => x.Wip);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SourceItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.HasMany(x => x.Components).WithOne().HasForeignKey(x => x.ProductionOrderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.RoutingLines).WithOne().HasForeignKey(x => x.ProductionOrderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProdOrderComponent>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProdOrderComponents", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Ignore(x => x.RemainingQuantity);
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ProdOrderRoutingLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProdOrderRoutingLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OperationNo).IsRequired().HasMaxLength(10);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<CapacityLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CapacityLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ProductionOrderNo });
            b.Property(x => x.ProductionOrderNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.OperationNo).HasMaxLength(10);
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PlanningLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PlanningLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.VendorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.CarriedOutNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });
    }
}
