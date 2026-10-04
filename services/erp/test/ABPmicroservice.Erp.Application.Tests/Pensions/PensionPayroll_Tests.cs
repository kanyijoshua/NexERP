using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Pensions;

public class PensionPayroll_Tests : ErpApplicationTestBase
{
    private readonly IPensionSchemeAppService _schemes;
    private readonly IPensionerAppService _pensioners;
    private readonly IPensionPayrollAppService _payrolls;
    private readonly IPensionPayrollLineAppService _lines;
    private readonly IPaymentVoucherAppService _vouchers;

    public PensionPayroll_Tests()
    {
        _schemes = GetRequiredService<IPensionSchemeAppService>();
        _pensioners = GetRequiredService<IPensionerAppService>();
        _payrolls = GetRequiredService<IPensionPayrollAppService>();
        _lines = GetRequiredService<IPensionPayrollLineAppService>();
        _vouchers = GetRequiredService<IPaymentVoucherAppService>();
    }

    [Fact]
    public async Task A_Pension_Payroll_Pays_Those_Due_Withholds_Tax_And_Is_Paid_By_Voucher()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _schemes.CreateAsync(new CreateUpdatePensionSchemeDto { Code = "OMEGA", Description = "Omega Staff Pension Scheme" });

            var first = await _pensioners.CreateAsync(new CreateUpdatePensionerDto
            {
                SchemeCode = "OMEGA",
                Name = "Njeri Kamau",
                MonthlyPension = 40000m,
                StartDate = new DateTime(2025, 7, 1),
            });
            first.No.ShouldBe("PN0010");

            await _pensioners.CreateAsync(new CreateUpdatePensionerDto { SchemeCode = "OMEGA", Name = "Barasa Wafula", MonthlyPension = 20000m, StartDate = new DateTime(2025, 7, 1) });

            // Not due: one starts after the month, one is suspended.
            await _pensioners.CreateAsync(new CreateUpdatePensionerDto { SchemeCode = "OMEGA", Name = "Later Starter", MonthlyPension = 15000m, StartDate = new DateTime(2026, 3, 1) });
            await _pensioners.CreateAsync(new CreateUpdatePensionerDto
            {
                SchemeCode = "OMEGA",
                Name = "On Hold",
                MonthlyPension = 15000m,
                StartDate = new DateTime(2025, 1, 1),
                Status = PensionerStatus.Suspended,
            });

            var payroll = await _payrolls.CreateAsync(new CreateUpdatePensionPayrollHeaderDto
            {
                SchemeCode = "OMEGA",
                PayPeriod = new DateTime(2026, 1, 20),
                PostingDate = new DateTime(2026, 1, 28),
                TaxRatePct = 10m,
                TaxFreeAmount = 25000m,
            });
            payroll.No.ShouldBe("PP-00001");
            payroll.PayPeriod.ShouldBe(new DateTime(2026, 1, 1));

            payroll = await _payrolls.SuggestLinesAsync(payroll.Id);
            payroll.NoOfPensioners.ShouldBe(2);
            payroll.TotalGross.ShouldBe(60000m);
            // 10% of the 15,000 above the tax free amount; the 20,000 pension is below it.
            payroll.TotalTax.ShouldBe(1500m);
            payroll.TotalNet.ShouldBe(58500m);

            var unreleased = await Should.ThrowAsync<BusinessException>(() => _payrolls.RunPostingAsync(payroll.Id));
            unreleased.Code.ShouldBe(ErpErrorCodes.Pensions.DocumentNotReleased);

            await _payrolls.ReleaseAsync(payroll.Id);
            payroll = await _payrolls.RunPostingAsync(payroll.Id);
            payroll.Status.ShouldBe(PensionDocumentStatus.Posted);

            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == payroll.No);
            glEntries.Sum(e => (double)e.Amount).ShouldBe(0d);
            glEntries.Single(e => e.GLAccountNo == "8620").Amount.ShouldBe(60000m);
            glEntries.Single(e => e.GLAccountNo == "2630").Amount.ShouldBe(-1500m);
            glEntries.Single(e => e.GLAccountNo == "2620").Amount.ShouldBe(-58500m);

            (await _pensioners.GetAsync(first.Id)).LastPaidPeriod.ShouldBe(new DateTime(2026, 1, 1));

            // The net is paid through a voucher on benefits payable, which clears that account.
            payroll = await _payrolls.RaisePaymentVoucherAsync(payroll.Id);
            var voucher = (await _vouchers.GetListAsync(new GetPaymentVoucherListInput { Filter = payroll.No })).Items.Single();
            voucher.No.ShouldBe(payroll.PaymentVoucherNo);
            voucher.TotalNetAmount.ShouldBe(58500m);
            voucher.SourceType.ShouldBe("PensionPayroll");

            await _vouchers.UpdateAsync(voucher.Id, new CreateUpdatePaymentVoucherHeaderDto
            {
                DocumentDate = voucher.DocumentDate,
                PostingDate = new DateTime(2026, 1, 30),
                PayMode = "BANK",
                PayingBankAccountNo = "WWB-OPERATING",
                Payee = voucher.Payee,
                PaymentNarration = voucher.PaymentNarration,
            });
            (await _vouchers.RunPostingAsync(voucher.Id)).Status.ShouldBe(DocumentStatus.Posted);

            var payable = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.GLAccountNo == "2620");
            payable.Sum(e => (double)e.Amount).ShouldBe(0d);

            // A month is paid once.
            var again = await _payrolls.CreateAsync(new CreateUpdatePensionPayrollHeaderDto { SchemeCode = "OMEGA", PayPeriod = new DateTime(2026, 1, 1), PostingDate = new DateTime(2026, 1, 29) });
            await _payrolls.SuggestLinesAsync(again.Id);
            var duplicate = await Should.ThrowAsync<BusinessException>(() => _payrolls.ReleaseAsync(again.Id));
            duplicate.Code.ShouldBe(ErpErrorCodes.Pensions.PayrollPeriodAlreadyPosted);

            // A pensioner who has been paid keeps their record.
            var paid = await Should.ThrowAsync<BusinessException>(() => _pensioners.DeleteAsync(first.Id));
            paid.Code.ShouldBe(ErpErrorCodes.Pensions.PensionerHasPayroll);

            (await _lines.GetListAsync(new GetPensionPayrollLineListInput { DocumentNo = payroll.No })).TotalCount.ShouldBe(2);
        });
    }
}
