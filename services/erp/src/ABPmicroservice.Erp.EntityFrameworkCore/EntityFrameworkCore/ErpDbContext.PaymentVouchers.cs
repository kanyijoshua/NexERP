using ABPmicroservice.Erp.CashManagement;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<CashManagementSetup> CashManagementSetups { get; set; }
    public DbSet<PaymentDeductionCode> PaymentDeductionCodes { get; set; }
    public DbSet<PaymentType> PaymentTypes { get; set; }
    public DbSet<PaymentVoucherHeader> PaymentVoucherHeaders { get; set; }
    public DbSet<PaymentVoucherLine> PaymentVoucherLines { get; set; }
}

public static class ErpPaymentVouchersModelCreatingExtensions
{
    public static void ConfigureErpPaymentVouchers(this ModelBuilder builder)
    {
        builder.Entity<CashManagementSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "CashManagementSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex();
            b.Property(x => x.PaymentVoucherNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
        });

        builder.Entity<PaymentDeductionCode>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PaymentDeductionCodes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.PayableAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<PaymentType>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PaymentTypes", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.AccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.WithholdingTaxCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.WithholdingVatCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.RetentionCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<PaymentVoucherHeader>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PaymentVoucherHeaders", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.HasIndex(x => new { x.CompanyId, x.Status, x.PostingDate });
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.PayMode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.PayingBankAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.CurrencyCode).HasMaxLength(ErpDomainConsts.MaxCurrencyCodeLength);
            b.Property(x => x.Payee).HasMaxLength(PaymentVoucherHeader.MaxPayeeLength);
            b.Property(x => x.OnBehalfOf).HasMaxLength(PaymentVoucherHeader.MaxPayeeLength);
            b.Property(x => x.PaymentNarration).HasMaxLength(PaymentVoucherHeader.MaxNarrationLength);
            b.Property(x => x.ChequeNo).HasMaxLength(ErpDomainConsts.MaxExternalDocumentNoLength);
            b.Property(x => x.SourceType).HasMaxLength(ErpDomainConsts.MaxEntityNameLength);
            b.Property(x => x.SourceNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Ignore(x => x.HasLines);
            b.Ignore(x => x.ApprovalAmount);
        });

        builder.Entity<PaymentVoucherLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PaymentVoucherLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PaymentVoucherLine.DocumentNo), nameof(PaymentVoucherLine.LineNo));
            b.Property(x => x.DocumentNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.PaymentTypeCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.AccountNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.AccountName).HasMaxLength(ErpDomainConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
            b.Property(x => x.AppliesToDocNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.WithholdingTaxCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.WithholdingVatCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.RetentionCode).HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Ignore(x => x.TotalDeductions);
            b.Ignore(x => x.AmountExcludingVat);
        });
    }
}
