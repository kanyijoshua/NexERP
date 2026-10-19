using ABPmicroservice.Erp.Crm;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<MarketingSetup> MarketingSetups { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<OpportunityEntry> OpportunityEntries { get; set; }
    public DbSet<CloseOpportunityCode> CloseOpportunityCodes { get; set; }
    public DbSet<ActivityType> ActivityTypes { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignTarget> CampaignTargets { get; set; }
    public DbSet<InteractionLogEntry> InteractionLogEntries { get; set; }
}

/// <summary>Contacts, leads and opportunities, campaigns and the interactions with contacts.</summary>
public static class ErpCrmModelCreatingExtensions
{
    public static void ConfigureErpCrm(this ModelBuilder builder)
    {
        builder.Entity<Kanban.KanbanStage>(b => b.HasIndex(x => new { x.CompanyId, x.PipelineType, x.Sequence }));

        builder.Entity<MarketingSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MarketingSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContactNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.OpportunityNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.CampaignNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
            b.Property(x => x.CustomerPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.GenBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.VatBusPostingGroup).HasMaxLength(ErpDomainConsts.MaxPostingGroupLength);
            b.Property(x => x.PaymentTermsCode).HasMaxLength(ErpDomainConsts.MaxPaymentTermsCodeLength);
        });

        builder.Entity<Contact>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Contacts", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.CompanyNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CompanyName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.JobTitle).HasMaxLength(50);
            b.Property(x => x.Email).HasMaxLength(80);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.MobilePhoneNo).HasMaxLength(30);
            b.Property(x => x.Address).HasMaxLength(100);
            b.Property(x => x.City).HasMaxLength(30);
            b.Property(x => x.CountryRegionCode).HasMaxLength(10);
            b.Property(x => x.SalespersonCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CustomerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.VendorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<Opportunity>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Opportunities", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.Kind, x.StageId });
            b.Ignore(x => x.WeightedRevenue);
            b.Ignore(x => x.IsClosed);
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.ContactNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContactName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.ContactCompanyName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Email).HasMaxLength(80);
            b.Property(x => x.PhoneNo).HasMaxLength(30);
            b.Property(x => x.SalespersonCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.CampaignNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.StageName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.CloseOpportunityCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SalesQuoteNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<OpportunityEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "OpportunityEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.OpportunityId);
            b.Property(x => x.StageName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Note).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<CloseOpportunityCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CloseOpportunityCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });

        builder.Entity<ActivityType>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ActivityTypes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.Icon).HasMaxLength(50);
        });

        builder.Entity<Campaign>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Campaigns", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.SalespersonCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<CampaignTarget>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CampaignTargets", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.CampaignNo, x.ContactNo }).IsUnique();
            b.Property(x => x.CampaignNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContactNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContactName).HasMaxLength(ErpDomainConsts.MaxNameLength);
        });

        builder.Entity<InteractionLogEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "InteractionLogEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.ContactNo });
            b.Property(x => x.ContactNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ContactName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.OpportunityNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CampaignNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.ActivityTypeCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.SalespersonCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });
    }
}
