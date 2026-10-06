using ABPmicroservice.Erp.Pensions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>The pension setup tables: banks, pay modes, reasons, other schemes, pay items, exit documents and age factors.</summary>
public static class ErpPensionReferenceTablesModelCreatingExtensions
{
    public static void ConfigureErpPensionReferenceTables(this ModelBuilder builder)
    {
        builder.Entity<PensionBank>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionBanks", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<PensionBankBranch>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionBankBranches", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PensionBankBranch.BankCode), nameof(PensionBankBranch.BranchCode));
            b.Property(x => x.BankCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.BranchCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.SwiftCode).HasMaxLength(ErpDomainConsts.MaxSwiftCodeLength);
        });

        builder.Entity<PensionerPayMode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionerPayModes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<PensionerSuspensionReason>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionerSuspensionReasons", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<PensionRevisionReason>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionRevisionReasons", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<OtherPensionScheme>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "OtherPensionSchemes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
        });

        builder.Entity<PensionerPayItem>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionerPayItems", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.AccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<PensionerPayItemAssignment>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionerPayItemAssignments", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.PensionerNo, x.PayItemCode });
            b.Property(x => x.PensionerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PayItemCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<PensionPayrollLineItem>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionPayrollLineItems", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.DocumentNo, x.LineNo });
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.PensionerNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.PayItemCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.AccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Ignore(x => x.IsEarning);
        });

        builder.Entity<ExitReasonDocument>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "ExitReasonDocuments", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(ExitReasonDocument.ExitReasonCode), nameof(ExitReasonDocument.LineNo));
            b.Property(x => x.ExitReasonCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.DocumentName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
        });

        builder.Entity<MemberExitDocument>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "MemberExitDocuments", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(MemberExitDocument.ExitNo), nameof(MemberExitDocument.LineNo));
            b.Property(x => x.ExitNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.DocumentName).IsRequired().HasMaxLength(ErpDomainConsts.MaxNameLength);
        });

        builder.Entity<PensionAgeFactor>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PensionAgeFactors", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PensionAgeFactor.SchemeCode), nameof(PensionAgeFactor.FactorType), nameof(PensionAgeFactor.Age));
            b.Property(x => x.SchemeCode).IsRequired().HasMaxLength(ErpDomainConsts.MaxDimensionValueCodeLength);
        });
    }
}
