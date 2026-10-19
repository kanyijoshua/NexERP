using System;
using ABPmicroservice.Erp.Procurement;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<ProcurementPlan> ProcurementPlans { get; set; }
    public DbSet<ProcurementPlanLine> ProcurementPlanLines { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<PurchaseRequisitionLine> PurchaseRequisitionLines { get; set; }
    public DbSet<TenderHeader> TenderHeaders { get; set; }
    public DbSet<TenderLine> TenderLines { get; set; }
    public DbSet<TenderCommittee> TenderCommittees { get; set; }
    public DbSet<TenderMandatoryRequirement> TenderMandatoryRequirements { get; set; }
    public DbSet<TenderBid> TenderBids { get; set; }
}

public static class ErpProcurementModelCreatingExtensions
{
    public static void ConfigureErpProcurement(this ModelBuilder builder)
    {
        builder.Entity<ProcurementPlan>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProcurementPlans", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(30);
            b.Property(x => x.Description).IsRequired().HasMaxLength(200);
            b.Property(x => x.BudgetName).HasMaxLength(50);
            b.Property(x => x.TotalEstimatedAmount).HasPrecision(18, 2);

            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PlanId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProcurementPlanLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ProcurementPlanLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.PlanId, x.PlanItemNo });
            b.Property(x => x.PlanItemNo).IsRequired().HasMaxLength(30);
            b.Property(x => x.Description).IsRequired().HasMaxLength(200);
            b.Property(x => x.UnitOfMeasure).HasMaxLength(20);
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            b.Property(x => x.SourceOfFunds).HasMaxLength(50);
            b.Property(x => x.DepartmentCode).HasMaxLength(30);
            b.Property(x => x.Quarter).HasMaxLength(20);
        });

        builder.Entity<PurchaseRequisition>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchaseRequisitions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.Status, x.RequestDate });
            b.Property(x => x.No).IsRequired().HasMaxLength(30);
            b.Property(x => x.RequestedBy).IsRequired().HasMaxLength(50);
            b.Property(x => x.DepartmentCode).HasMaxLength(30);
            b.Property(x => x.ProcurementPlanCode).HasMaxLength(30);
            b.Property(x => x.TenderNo).HasMaxLength(30);
            b.Property(x => x.PurchaseOrderNo).HasMaxLength(30);
            b.Property(x => x.Description).HasMaxLength(250);
            b.Property(x => x.TotalEstimatedCost).HasPrecision(18, 2);

            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.RequisitionId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PurchaseRequisitionLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PurchaseRequisitionLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.RequisitionId, x.LineNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(30);
            b.Property(x => x.Description).IsRequired().HasMaxLength(200);
            b.Property(x => x.Specifications).HasMaxLength(500);
            b.Property(x => x.ProcurementPlanItemNo).HasMaxLength(30);
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.EstimatedUnitCost).HasPrecision(18, 2);
            b.Property(x => x.EstimatedTotalCost).HasPrecision(18, 2);
            b.Property(x => x.UnitOfMeasure).HasMaxLength(20);
            b.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
        });

        builder.Entity<TenderHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TenderHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.Status, x.ClosingDate });
            b.Property(x => x.No).IsRequired().HasMaxLength(30);
            b.Property(x => x.Title).IsRequired().HasMaxLength(250);
            b.Property(x => x.RequisitionNo).HasMaxLength(30);
            b.Property(x => x.ProcurementPlanCode).HasMaxLength(30);
            b.Property(x => x.ProcurementPlanItemNo).HasMaxLength(30);
            b.Property(x => x.Category).HasMaxLength(50);
            b.Property(x => x.ClosingTime).HasMaxLength(20);
            b.Property(x => x.TenderSecurityAmount).HasPrecision(18, 2);
            b.Property(x => x.AwardedVendorNo).HasMaxLength(30);
            b.Property(x => x.AwardedVendorName).HasMaxLength(100);
            b.Property(x => x.AwardedAmount).HasPrecision(18, 2);
            b.Property(x => x.PurchaseOrderNo).HasMaxLength(30);
            b.Property(x => x.TotalEstimatedAmount).HasPrecision(18, 2);

            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.TenderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Committees).WithOne().HasForeignKey(x => x.TenderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.MandatoryRequirements).WithOne().HasForeignKey(x => x.TenderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Bids).WithOne().HasForeignKey(x => x.TenderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TenderLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TenderLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.TenderId, x.LineNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(30);
            b.Property(x => x.Description).IsRequired().HasMaxLength(200);
            b.Property(x => x.Specifications).HasMaxLength(500);
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.EstimatedUnitCost).HasPrecision(18, 2);
            b.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            b.Property(x => x.UnitOfMeasure).HasMaxLength(20);
        });

        builder.Entity<TenderCommittee>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TenderCommittees", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.TenderId, x.CommitteeType });
            b.Property(x => x.StaffNo).IsRequired().HasMaxLength(30);
            b.Property(x => x.StaffName).IsRequired().HasMaxLength(100);
        });

        builder.Entity<TenderMandatoryRequirement>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TenderMandatoryRequirements", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RequirementDescription).IsRequired().HasMaxLength(250);
        });

        builder.Entity<TenderBid>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "TenderBids", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.TenderId, x.BidderNo });
            b.Property(x => x.BidderNo).IsRequired().HasMaxLength(30);
            b.Property(x => x.BidderName).IsRequired().HasMaxLength(100);
            b.Property(x => x.Email).HasMaxLength(100);
            b.Property(x => x.BidAmount).HasPrecision(18, 2);
            b.Property(x => x.TechnicalScore).HasPrecision(6, 2);
            b.Property(x => x.FinancialScore).HasPrecision(6, 2);
            b.Property(x => x.TotalScore).HasPrecision(6, 2);
            b.Property(x => x.Remarks).HasMaxLength(500);
        });
    }
}
