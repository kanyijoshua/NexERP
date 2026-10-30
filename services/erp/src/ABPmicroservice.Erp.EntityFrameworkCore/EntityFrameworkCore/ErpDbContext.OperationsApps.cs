using ABPmicroservice.Erp.Crm;
using ABPmicroservice.Erp.Fleet;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.MaintenanceManagement;
using ABPmicroservice.Erp.PointOfSale;
using ABPmicroservice.Erp.Quality;
using ABPmicroservice.Erp.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<JobVacancy> JobVacancies { get; set; }
    public DbSet<JobApplicant> JobApplicants { get; set; }
    public DbSet<AppraisalPeriod> AppraisalPeriods { get; set; }
    public DbSet<Appraisal> Appraisals { get; set; }
    public DbSet<AppraisalLine> AppraisalLines { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<CustomerSubscription> CustomerSubscriptions { get; set; }
    public DbSet<PosTerminal> PosTerminals { get; set; }
    public DbSet<PosSession> PosSessions { get; set; }
    public DbSet<PosOrder> PosOrders { get; set; }
    public DbSet<PosOrderLine> PosOrderLines { get; set; }
    public DbSet<MaintenanceTeam> MaintenanceTeams { get; set; }
    public DbSet<MaintenanceEquipment> MaintenanceEquipment { get; set; }
    public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
    public DbSet<QualityControlPoint> QualityControlPoints { get; set; }
    public DbSet<QualityCheck> QualityChecks { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleLogEntry> VehicleLogEntries { get; set; }
    public DbSet<VehicleContract> VehicleContracts { get; set; }
    public DbSet<Segment> Segments { get; set; }
    public DbSet<SegmentLine> SegmentLines { get; set; }
    public DbSet<CampaignMailingEntry> CampaignMailingEntries { get; set; }
}

/// <summary>Recruitment and appraisals, subscriptions, point of sale, maintenance, quality checks, fleet and marketing segments.</summary>
public static class ErpOperationsAppsModelCreatingExtensions
{
    public static void ConfigureErpOperationsApps(this ModelBuilder builder)
    {
        builder.Entity<JobVacancy>(b =>
        {
            Numbered(b, "JobVacancies");
            b.Property(x => x.Title).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.Department).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(2000);
        });

        builder.Entity<JobApplicant>(b =>
        {
            Numbered(b, "JobApplicants");
            b.Ignore(x => x.FullName);
            b.HasIndex(x => new { x.CompanyId, x.VacancyNo });
            b.Property(x => x.VacancyNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.FirstName).IsRequired().HasMaxLength(30);
            b.Property(x => x.LastName).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(80);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.Notes).HasMaxLength(2000);
            b.Property(x => x.EmployeeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<AppraisalPeriod>(b => CodeTable(b, "AppraisalPeriods"));

        builder.Entity<Appraisal>(b =>
        {
            Numbered(b, "Appraisals");
            b.HasIndex(x => new { x.CompanyId, x.EmployeeNo, x.PeriodCode });
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.EmployeeName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PeriodCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ReviewerEmployeeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.EmployeeComment).HasMaxLength(2000);
            b.Property(x => x.ReviewerComment).HasMaxLength(2000);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.AppraisalId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AppraisalLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "AppraisalLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Objective).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.Target).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.Comment).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<SubscriptionPlan>(b =>
        {
            CodeTable(b, "SubscriptionPlans");
            b.Property(x => x.LineNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<CustomerSubscription>(b =>
        {
            Numbered(b, "CustomerSubscriptions");
            b.HasIndex(x => new { x.CompanyId, x.Status, x.NextBillingDate });
            b.Property(x => x.CustomerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CustomerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PlanCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.LastInvoiceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PosTerminal>(b =>
        {
            CodeTable(b, "PosTerminals");
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.WalkInCustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CashBankAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CardBankAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<PosSession>(b =>
        {
            Numbered(b, "PosSessions");
            b.Ignore(x => x.ExpectedCash);
            b.Ignore(x => x.Difference);
            b.HasIndex(x => new { x.CompanyId, x.TerminalCode, x.Status });
            b.Property(x => x.TerminalCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.OpenedBy).HasMaxLength(256);
        });

        builder.Entity<PosOrder>(b =>
        {
            Numbered(b, "PosOrders");
            b.Ignore(x => x.Subtotal);
            b.HasIndex(x => x.SessionId);
            b.Property(x => x.TerminalCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PostedInvoiceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PosOrderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PosOrderLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PosOrderLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ItemNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<MaintenanceTeam>(b => CodeTable(b, "MaintenanceTeams"));

        builder.Entity<MaintenanceEquipment>(b =>
        {
            Numbered(b, "MaintenanceEquipment");
            b.Property(x => x.Description).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.Category).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.SerialNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.FixedAssetNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.LocationCode).HasMaxLength(ErpDomainConsts.MaxLocationCodeLength);
            b.Property(x => x.TeamCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.TechnicianEmployeeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<MaintenanceRequest>(b =>
        {
            Numbered(b, "MaintenanceRequests");
            b.Ignore(x => x.IsOpen);
            b.HasIndex(x => new { x.CompanyId, x.EquipmentNo });
            b.Property(x => x.EquipmentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Subject).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.TeamCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.TechnicianEmployeeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Notes).HasMaxLength(2000);
        });

        builder.Entity<QualityControlPoint>(b =>
        {
            CodeTable(b, "QualityControlPoints");
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ItemCategoryCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.UnitOfMeasure).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Instructions).HasMaxLength(2000);
        });

        builder.Entity<QualityCheck>(b =>
        {
            Numbered(b, "QualityChecks");
            b.HasIndex(x => new { x.CompanyId, x.SourceDocumentNo });
            b.Property(x => x.ControlPointCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.ItemNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ItemDescription).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.SourceDocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.LotNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CheckedBy).HasMaxLength(256);
            b.Property(x => x.Note).HasMaxLength(2000);
        });

        builder.Entity<Vehicle>(b =>
        {
            Numbered(b, "Vehicles");
            b.Property(x => x.LicensePlate).IsRequired().HasMaxLength(20);
            b.Property(x => x.Make).HasMaxLength(50);
            b.Property(x => x.Model).HasMaxLength(50);
            b.Property(x => x.Vin).HasMaxLength(30);
            b.Property(x => x.FuelType).HasMaxLength(20);
            b.Property(x => x.DriverEmployeeNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.FixedAssetNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<VehicleLogEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VehicleLogEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.VehicleNo, x.Date });
            b.Property(x => x.VehicleNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.VendorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<VehicleContract>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "VehicleContracts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.VehicleNo });
            b.Property(x => x.VehicleNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VendorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Reference).HasMaxLength(50);
        });

        builder.Entity<Segment>(b =>
        {
            Numbered(b, "Segments");
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.SegmentId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SegmentLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "SegmentLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContactNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContactName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Email).HasMaxLength(80);
        });

        builder.Entity<CampaignMailingEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CampaignMailingEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.CampaignNo });
            b.Property(x => x.CampaignNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContactNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContactName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Email).HasMaxLength(80);
            b.Property(x => x.Subject).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.ErrorMessage).HasMaxLength(500);
        });
    }

    private static void Numbered<T>(EntityTypeBuilder<T> b, string table)
        where T : class, Companies.ICompanyScoped, Companies.IHasNo
    {
        b.ToTable(ErpDbProperties.DbTablePrefix + table, ErpDbProperties.DbSchema);
        b.ConfigureByConvention();
        b.HasCompanyUniqueIndex("No");
        b.Property<string>(nameof(Companies.IHasNo.No)).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
    }

    private static void CodeTable<T>(EntityTypeBuilder<T> b, string table)
        where T : Companies.CodeTableEntity
    {
        b.ToTable(ErpDbProperties.DbTablePrefix + table, ErpDbProperties.DbSchema);
        b.ConfigureByConvention();
        b.HasCompanyUniqueIndex("Code");
        b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
    }
}
