using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.CashManagement;

public class PaymentVouchers_Tests : ErpApplicationTestBase
{
    private static readonly DateTime Today = new(2026, 1, 28);

    private readonly IPaymentVoucherAppService _vouchers;
    private readonly IPaymentVoucherLineAppService _lines;
    private readonly IPaymentTypeAppService _paymentTypes;
    private readonly IPaymentDeductionCodeAppService _deductionCodes;

    public PaymentVouchers_Tests()
    {
        _vouchers = GetRequiredService<IPaymentVoucherAppService>();
        _lines = GetRequiredService<IPaymentVoucherLineAppService>();
        _paymentTypes = GetRequiredService<IPaymentTypeAppService>();
        _deductionCodes = GetRequiredService<IPaymentDeductionCodeAppService>();
    }

    private Task<PaymentVoucherHeaderDto> NewVoucherAsync(string payee = "Mwangaza Consultants")
    {
        return _vouchers.CreateAsync(new CreateUpdatePaymentVoucherHeaderDto
        {
            DocumentDate = Today,
            PayMode = "BANK",
            PayingBankAccountNo = "WWB-OPERATING",
            Payee = payee,
            PaymentNarration = "Consultancy fees",
        });
    }

    [Fact]
    public async Task A_Voucher_Withholds_Its_Deductions_And_Pays_The_Net_From_The_Bank()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Professional fees carry 16% VAT, 5% withholding tax and 2% withholding VAT.
            await _paymentTypes.CreateAsync(new CreateUpdatePaymentTypeDto
            {
                Code = "fees",
                Description = "Professional fees",
                AccountType = GenJournalAccountType.GLAccount,
                AccountNo = "6100",
                VatRatePct = 16m,
                WithholdingTaxCode = "WHT-5",
                WithholdingVatCode = "WVAT-2",
            });

            var voucher = await NewVoucherAsync();
            voucher.No.ShouldBe("PV-00001");
            voucher.Status.ShouldBe(DocumentStatus.Open);

            var empty = await Should.ThrowAsync<BusinessException>(() => _vouchers.ReleaseAsync(voucher.Id));
            empty.Code.ShouldBe(ErpErrorCodes.CashManagement.VoucherHasNoLines);

            var line = await _lines.CreateAsync(new CreateUpdatePaymentVoucherLineDto { DocumentNo = voucher.No, PaymentTypeCode = "FEES", Amount = 11600m });
            line.AccountNo.ShouldBe("6100");
            line.WithholdingTaxAmount.ShouldBe(500m);
            line.WithholdingVatAmount.ShouldBe(200m);
            line.NetAmount.ShouldBe(10900m);

            voucher = await _vouchers.ReleaseAsync(voucher.Id);
            voucher.Status.ShouldBe(DocumentStatus.Released);
            voucher.TotalAmount.ShouldBe(11600m);
            voucher.TotalNetAmount.ShouldBe(10900m);

            // A released voucher is paid as it was released; only the cheque can still be recorded.
            var frozen = await Should.ThrowAsync<BusinessException>(() =>
                _lines.CreateAsync(new CreateUpdatePaymentVoucherLineDto { DocumentNo = voucher.No, AccountNo = "6100", Amount = 100m })
            );
            frozen.Code.ShouldBe(ErpErrorCodes.CashManagement.VoucherNotOpen);

            await _vouchers.RecordChequeAsync(voucher.Id, new PaymentVoucherChequeInput { ChequeNo = "000123", ChequeDate = Today });

            voucher = await _vouchers.RunPostingAsync(voucher.Id);
            voucher.Status.ShouldBe(DocumentStatus.Posted);
            voucher.ChequeNo.ShouldBe("000123");

            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == voucher.No);
            glEntries.Sum(e => (double)e.Amount).ShouldBe(0d);
            glEntries.Single(e => e.GLAccountNo == "6100").Amount.ShouldBe(11600m);
            glEntries.Single(e => e.GLAccountNo == "2350").Amount.ShouldBe(-500m);
            glEntries.Single(e => e.GLAccountNo == "2360").Amount.ShouldBe(-200m);
            glEntries.Single(e => e.GLAccountNo == "1020").Amount.ShouldBe(-10900m);

            var bankEntries = await GetRequiredService<IRepository<BankAccountLedgerEntry, Guid>>().GetListAsync(e => e.DocumentNo == voucher.No);
            bankEntries.Single().Amount.ShouldBe(-10900m);

            var posted = await Should.ThrowAsync<BusinessException>(() => _vouchers.DeleteAsync(voucher.Id));
            posted.Code.ShouldBe(ErpErrorCodes.CashManagement.VoucherNotOpen);
        });
    }

    [Fact]
    public async Task A_Voucher_Needs_A_Payee_A_Bank_And_Deductions_Of_The_Right_Kind()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var voucher = await _vouchers.CreateAsync(new CreateUpdatePaymentVoucherHeaderDto { DocumentDate = Today });
            await _lines.CreateAsync(new CreateUpdatePaymentVoucherLineDto { DocumentNo = voucher.No, AccountNo = "6100", Amount = 1000m });

            var incomplete = await Should.ThrowAsync<BusinessException>(() => _vouchers.ReleaseAsync(voucher.Id));
            incomplete.Code.ShouldBe(ErpErrorCodes.CashManagement.VoucherFieldMissing);

            // A retention code cannot stand in for withholding tax, and a bank account cannot be paid.
            var wrongKind = await Should.ThrowAsync<BusinessException>(() =>
                _lines.CreateAsync(new CreateUpdatePaymentVoucherLineDto { DocumentNo = voucher.No, AccountNo = "6100", Amount = 1000m, WithholdingTaxCode = "RET-10" })
            );
            wrongKind.Code.ShouldBe(ErpErrorCodes.CashManagement.WrongDeductionType);

            var bankLine = await Should.ThrowAsync<BusinessException>(() =>
                _lines.CreateAsync(new CreateUpdatePaymentVoucherLineDto
                {
                    DocumentNo = voucher.No,
                    AccountType = GenJournalAccountType.BankAccount,
                    AccountNo = "WWB-OPERATING",
                    Amount = 1000m,
                })
            );
            bankLine.Code.ShouldBe(ErpErrorCodes.CashManagement.InvalidVoucherAccountType);

            // A deduction code a voucher line names cannot be deleted.
            await _lines.CreateAsync(new CreateUpdatePaymentVoucherLineDto { DocumentNo = voucher.No, AccountNo = "6100", Amount = 1000m, RetentionCode = "RET-10" });
            var retention = (await _deductionCodes.GetListAsync(new() { Filter = "RET-10" })).Items.Single();
            var inUse = await Should.ThrowAsync<BusinessException>(() => _deductionCodes.DeleteAsync(retention.Id));
            inUse.Code.ShouldBe(ErpErrorCodes.CashManagement.DeductionCodeInUse);
        });
    }
}
