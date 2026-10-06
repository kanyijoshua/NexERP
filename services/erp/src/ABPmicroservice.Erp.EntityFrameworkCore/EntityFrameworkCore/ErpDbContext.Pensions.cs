using ABPmicroservice.Erp.Pensions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<PensionSetup> PensionSetups { get; set; }
    public DbSet<PensionScheme> PensionSchemes { get; set; }
    public DbSet<PensionSponsor> PensionSponsors { get; set; }
    public DbSet<PensionMember> PensionMembers { get; set; }
    public DbSet<MemberLedgerEntry> MemberLedgerEntries { get; set; }
    public DbSet<PensionContributionHeader> PensionContributionHeaders { get; set; }
    public DbSet<PensionContributionLine> PensionContributionLines { get; set; }
    public DbSet<PensionInterestRate> PensionInterestRates { get; set; }
    public DbSet<ExitReason> ExitReasons { get; set; }
    public DbSet<LumpsumTaxTable> LumpsumTaxTables { get; set; }
    public DbSet<LumpsumTaxBand> LumpsumTaxBands { get; set; }
    public DbSet<MemberExit> MemberExits { get; set; }
    public DbSet<Pensioner> Pensioners { get; set; }
    public DbSet<PensionPayrollHeader> PensionPayrollHeaders { get; set; }
    public DbSet<PensionPayrollLine> PensionPayrollLines { get; set; }
    public DbSet<PensionBenefitCalculation> PensionBenefitCalculations { get; set; }
    public DbSet<PensionBeneficiary> PensionBeneficiaries { get; set; }
    public DbSet<PensionContributionRate> PensionContributionRates { get; set; }
    public DbSet<PensionVestingScale> PensionVestingScales { get; set; }
    public DbSet<PensionTaxReliefLimit> PensionTaxReliefLimits { get; set; }
    public DbSet<MemberStatusEntry> MemberStatusEntries { get; set; }
    public DbSet<MemberSalaryEntry> MemberSalaryEntries { get; set; }
    public DbSet<PensionIncrement> PensionIncrements { get; set; }
    public DbSet<PensionerChangeEntry> PensionerChangeEntries { get; set; }
    public DbSet<PensionBank> PensionBanks { get; set; }
    public DbSet<PensionBankBranch> PensionBankBranches { get; set; }
    public DbSet<PensionerPayMode> PensionerPayModes { get; set; }
    public DbSet<PensionerSuspensionReason> PensionerSuspensionReasons { get; set; }
    public DbSet<PensionRevisionReason> PensionRevisionReasons { get; set; }
    public DbSet<OtherPensionScheme> OtherPensionSchemes { get; set; }
    public DbSet<PensionerPayItem> PensionerPayItems { get; set; }
    public DbSet<PensionerPayItemAssignment> PensionerPayItemAssignments { get; set; }
    public DbSet<PensionPayrollLineItem> PensionPayrollLineItems { get; set; }
    public DbSet<ExitReasonDocument> ExitReasonDocuments { get; set; }
    public DbSet<MemberExitDocument> MemberExitDocuments { get; set; }
    public DbSet<PensionAgeFactor> PensionAgeFactors { get; set; }
}

public static class ErpPensionsModelCreatingExtensions
{
    public static void ConfigureErpPensions(this ModelBuilder builder)
    {
        builder.Entity<PensionSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex();
        });

        builder.Entity<PensionScheme>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionSchemes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<PensionSponsor>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionSponsors", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
        });

        builder.Entity<PensionMember>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionMembers", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.SchemeCode, x.SponsorNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
            b.Property(x => x.SponsorNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength * 2);
            b.Ignore(x => x.IsContributing);
        });

        builder.Entity<MemberLedgerEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MemberLedgerEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.EntryNo });
            b.HasIndex(x => new { x.CompanyId, x.MemberNo, x.PostingDate });
            b.HasIndex(x => new { x.CompanyId, x.SchemeCode, x.PostingDate });
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
            b.Property(x => x.MemberNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Ignore(x => x.MoneyType);
        });

        builder.Entity<PensionContributionHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionContributionHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.TransferSchemeCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<PensionContributionLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionContributionLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PensionContributionLine.DocumentNo), nameof(PensionContributionLine.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.MemberNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<PensionInterestRate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionInterestRates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.SchemeCode, x.StartDate });
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
        });

        builder.Entity<ExitReason>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExitReasons", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Ignore(x => x.IsDeath);
        });

        builder.Entity<LumpsumTaxTable>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LumpsumTaxTables", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<LumpsumTaxBand>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "LumpsumTaxBands", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(LumpsumTaxBand.TaxTableCode), nameof(LumpsumTaxBand.LowerLimit));
            b.Property(x => x.TaxTableCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<Pensioner>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Pensioners", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.SchemeCode });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
            b.Property(x => x.MemberNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PayModeCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.BankCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.BankBranchCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.SuspensionReasonCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<PensionPayrollHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionPayrollHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.SchemeCode, x.PayPeriod });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
            b.Property(x => x.PaymentVoucherNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.TaxTableCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.Entity<PensionPayrollLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionPayrollLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PensionPayrollLine.DocumentNo), nameof(PensionPayrollLine.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.PensionerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PensionerName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.PayModeCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<PensionBeneficiary>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionBeneficiaries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PensionBeneficiary.MemberNo), nameof(PensionBeneficiary.LineNo));
            b.Property(x => x.MemberNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.GuardianName).HasMaxLength(ErpDomainConsts.MaxNameLength);
        });

        builder.Entity<PensionContributionRate>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionContributionRates", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.SponsorNo, x.StartDate });
            b.Property(x => x.SponsorNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<PensionVestingScale>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionVestingScales", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PensionVestingScale.SponsorNo), nameof(PensionVestingScale.FromServiceYears));
            b.Property(x => x.SponsorNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<PensionTaxReliefLimit>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionTaxReliefLimits", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PensionTaxReliefLimit.EffectiveDate));
        });

        builder.Entity<MemberStatusEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MemberStatusEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.MemberNo, x.EffectiveDate });
            b.HasIndex(x => new { x.CompanyId, x.SchemeCode, x.EffectiveDate });
            b.Property(x => x.MemberNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
            b.Property(x => x.SponsorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<MemberSalaryEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MemberSalaryEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(MemberSalaryEntry.MemberNo), nameof(MemberSalaryEntry.Period));
            b.Property(x => x.MemberNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SponsorNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PensionIncrement>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionIncrements", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
            b.Property(x => x.ReasonCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<PensionerChangeEntry>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionerChangeEntries", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.PensionerNo, x.EffectiveDate });
            b.Property(x => x.PensionerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
            b.Property(x => x.DocumentNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<PensionBenefitCalculation>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionBenefitCalculations", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.MemberNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.MemberNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.MemberName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.PensionerNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PaymentVoucherNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Ignore(x => x.IsOpen);
        });

        builder.ConfigureErpPensionReferenceTables();

        builder.Entity<MemberExit>(b =>
        {
            b.Property(x => x.PaymentVoucherNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.ToTable(ErpDbProperties.DbTablePrefix + "MemberExits", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.MemberNo });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.MemberNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Ignore(x => x.IsOpen);
        });
    }
}
