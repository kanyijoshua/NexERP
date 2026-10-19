using ABPmicroservice.Erp.ServiceManagement;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<ServiceMgtSetup> ServiceMgtSetups { get; set; }
    public DbSet<ServiceOrderType> ServiceOrderTypes { get; set; }
    public DbSet<FaultCode> FaultCodes { get; set; }
    public DbSet<ResolutionCode> ResolutionCodes { get; set; }
    public DbSet<ServiceItem> ServiceItems { get; set; }
    public DbSet<ServiceContract> ServiceContracts { get; set; }
    public DbSet<ServiceContractLine> ServiceContractLines { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }
    public DbSet<ServiceItemLine> ServiceItemLines { get; set; }
    public DbSet<ServiceLine> ServiceLines { get; set; }
    public DbSet<ServiceLedgerEntry> ServiceLedgerEntries { get; set; }
    public DbSet<WarrantyLedgerEntry> WarrantyLedgerEntries { get; set; }
}

/// <summary>Service items, contracts and orders, their code tables, and the service and warranty ledgers.</summary>
public static class ErpServiceManagementModelCreatingExtensions
{
    public static void ConfigureErpServiceManagement(this ModelBuilder builder)
    {
        builder.Entity<ServiceMgtSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceMgtSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ServiceOrderNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.ServiceItemNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.ServiceContractNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.ServiceRevenueAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContractRevenueAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ServiceOrderType>(b => CodeTable(b, "ServiceOrderTypes"));
        builder.Entity<FaultCode>(b => CodeTable(b, "FaultCodes"));
        builder.Entity<ResolutionCode>(b => CodeTable(b, "ResolutionCodes"));

        builder.Entity<ServiceItem>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceItems", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.ItemNo, x.SerialNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<ServiceContract>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceContracts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Ignore(x => x.AnnualAmount);
            b.Ignore(x => x.PeriodAmount);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.LastInvoiceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ServiceContractId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ServiceContractLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceContractLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ServiceItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ServiceOrder>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceOrders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ServiceOrderType).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ResourceNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContractNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LastInvoiceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.HasMany(x => x.ItemLines).WithOne().HasForeignKey(x => x.ServiceOrderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ServiceOrderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ServiceItemLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceItemLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ServiceItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.FaultCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ResolutionCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.FaultComment).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ServiceLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Ignore(x => x.RemainingQuantity);
            b.Ignore(x => x.LineAmount);
            b.Property(x => x.ServiceItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
        });

        builder.Entity<ServiceLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ServiceLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ServiceOrderNo });
            b.HasIndex(x => new { x.CompanyId, x.ServiceContractNo });
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ServiceOrderNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ServiceContractNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ServiceItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<WarrantyLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "WarrantyLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ServiceItemNo });
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ServiceOrderNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ServiceItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.No).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
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
