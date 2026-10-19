using ABPmicroservice.Erp.Pricing;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<PostedSalesShipment> PostedSalesShipments { get; set; }
    public DbSet<PostedSalesShipmentLine> PostedSalesShipmentLines { get; set; }
    public DbSet<PostedPurchaseReceipt> PostedPurchaseReceipts { get; set; }
    public DbSet<PostedPurchaseReceiptLine> PostedPurchaseReceiptLines { get; set; }
    public DbSet<PriceList> PriceLists { get; set; }
    public DbSet<PriceListLine> PriceListLines { get; set; }
    public DbSet<CustomerPriceGroup> CustomerPriceGroups { get; set; }
}

/// <summary>Orders shipped and received in parts, their posted shipments and receipts, and price lists.</summary>
public static class ErpOrdersModelCreatingExtensions
{
    public static void ConfigureErpOrders(this ModelBuilder builder)
    {
        builder.Entity<SalesHeader>(b =>
        {
            b.Property(x => x.ShipToCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.ShipToName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ShipToAddress).HasMaxLength(ErpDomainConsts.MaxAddressLength);
            b.Property(x => x.ShipToCity).HasMaxLength(ErpDomainConsts.MaxCityLength);
            b.Property(x => x.ShipToPostCode).HasMaxLength(ErpDomainConsts.MaxPostCodeLength);
            b.Property(x => x.ShipToCountryRegionCode).HasMaxLength(ErpDomainConsts.MaxCountryRegionCodeLength);
            b.Property(x => x.ShipToContact).HasMaxLength(ErpDomainConsts.MaxContactLength);
            b.Property(x => x.CustomerPriceGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.QuoteNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LastShippingNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LastPostingNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<SalesLine>(b =>
        {
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.ShipmentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.BlanketOrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.HasIndex(x => x.ShipmentLineId);
        });

        builder.Entity<PurchaseHeader>(b =>
        {
            b.Property(x => x.LastReceivingNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LastPostingNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PurchaseLine>(b =>
        {
            b.Property(x => x.ReceiptNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.BlanketOrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.HasIndex(x => x.ReceiptLineId);
        });

        builder.Entity<PostedSalesHeader>(b =>
        {
            b.Property(x => x.OrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.ExternalDocumentNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
        });

        builder.Entity<PostedSalesLine>(b =>
        {
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
            b.Property(x => x.ShipmentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PostedPurchaseHeader>(b =>
        {
            b.Property(x => x.OrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.VendorInvoiceNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
        });

        builder.Entity<PostedPurchaseLine>(b =>
        {
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
            b.Property(x => x.ReceiptNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PostedSalesShipment>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedSalesShipments", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.OrderNo });
            b.HasIndex(x => new { x.CompanyId, x.CustomerId, x.Return });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.OrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.SellToCustomerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SellToCustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.ExternalDocumentNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.ShipToName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ShipToAddress).HasMaxLength(ErpDomainConsts.MaxAddressLength);
            b.Property(x => x.ShipToCity).HasMaxLength(ErpDomainConsts.MaxCityLength);
            b.Property(x => x.ShipToPostCode).HasMaxLength(ErpDomainConsts.MaxPostCodeLength);
            b.Property(x => x.ShipToCountryRegionCode).HasMaxLength(ErpDomainConsts.MaxCountryRegionCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedSalesShipmentId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedSalesShipmentLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedSalesShipmentLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.OrderLineId);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
        });

        builder.Entity<PostedPurchaseReceipt>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedPurchaseReceipts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.OrderNo });
            b.HasIndex(x => new { x.CompanyId, x.VendorId, x.Return });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.OrderNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.BuyFromVendorNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.BuyFromVendorName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.VendorShipmentNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PostedPurchaseReceiptId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostedPurchaseReceiptLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PostedPurchaseReceiptLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.OrderLineId);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
        });

        builder.Entity<PriceList>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PriceLists", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.SourceNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PriceListId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PriceListLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PriceListLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AssetNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.UnitOfMeasureCode).HasMaxLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        });

        builder.Entity<CustomerPriceGroup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CustomerPriceGroups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(10);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });
    }
}
