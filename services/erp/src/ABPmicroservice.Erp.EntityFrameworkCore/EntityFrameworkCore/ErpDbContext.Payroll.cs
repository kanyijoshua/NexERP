using ABPmicroservice.Erp.Payroll;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public partial class ErpDbContext
{
    public DbSet<PayrollSetup> PayrollSetups { get; set; }
    public DbSet<PayrollEarning> PayrollEarnings { get; set; }
    public DbSet<PayrollDeduction> PayrollDeductions { get; set; }
    public DbSet<PayrollTaxBand> PayrollTaxBands { get; set; }
    public DbSet<EmployeePayItem> EmployeePayItems { get; set; }
    public DbSet<PayrollRun> PayrollRuns { get; set; }
    public DbSet<Payslip> Payslips { get; set; }
    public DbSet<PayslipLine> PayslipLines { get; set; }
}

public static class ErpPayrollModelCreatingExtensions
{
    public static void ConfigureErpPayroll(this ModelBuilder builder)
    {
        builder.Entity<PayrollSetup>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PayrollSetups", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex();
            b.Property(x => x.PayrollRunNos).HasMaxLength(ErpDomainConsts.MaxNoSeriesCodeLength);
        });

        builder.Entity<PayrollEarning>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PayrollEarnings", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.GLAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
        });

        builder.Entity<PayrollDeduction>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PayrollDeductions", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("Code");
            b.Property(x => x.GLAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.EmployerExpenseAccountNo).HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Ignore(x => x.IsIncomeTax);
        });

        builder.Entity<PayrollTaxBand>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PayrollTaxBands", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(PayrollTaxBand.LowerLimit));
        });

        builder.Entity<EmployeePayItem>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "EmployeePayItems", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.EmployeeNo });
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
        });

        builder.Entity<PayrollRun>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PayrollRuns", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex("No");
            b.Property(x => x.No).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.PaymentVoucherNo).HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
        });

        builder.Entity<Payslip>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "Payslips", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasCompanyUniqueIndex(nameof(Payslip.PayrollRunNo), nameof(Payslip.EmployeeNo));
            b.HasIndex(x => new { x.CompanyId, x.EmployeeNo });
            b.Property(x => x.PayrollRunNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.EmployeeName).HasMaxLength(ErpDomainConsts.MaxNameLength * 2);
            b.Property(x => x.JobTitle).HasMaxLength(ErpDomainConsts.MaxJobTitleLength);
            b.Property(x => x.BankAccountNo).HasMaxLength(ErpDomainConsts.MaxBankAccountNoLength);
        });

        builder.Entity<PayslipLine>(b =>
        {
            b.ToTable(ErpDbProperties.DbTablePrefix + "PayslipLines", ErpDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CompanyId, x.PayrollRunNo, x.EmployeeNo });
            b.Property(x => x.PayrollRunNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxDocumentNoLength);
            b.Property(x => x.EmployeeNo).IsRequired().HasMaxLength(ErpDomainConsts.MaxNoLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ErpDomainConsts.MaxCodeLength);
            b.Property(x => x.Description).HasMaxLength(ErpDomainConsts.MaxDescriptionLength);
        });
    }
}
