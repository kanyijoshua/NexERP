using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// What payment vouchers need before a first one can be raised: their number series, and the
/// usual deductions with the accounts what is withheld is owed on.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedPaymentVouchersAsync()
    {
        await EnsureSeriesAsync("PV", "Payment Vouchers", "PV-00001");

        await EnsureAccountAsync("2350", "Withholding Tax Payable", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2360", "Withholding VAT Payable", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2370", "Retention Payable", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);

        if (await IsEmptyAsync<CashManagementSetup>())
        {
            var setup = new CashManagementSetup(NewId());
            setup.SetNumbering("PV");
            await Repo<CashManagementSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<PaymentDeductionCode>())
        {
            (string Code, string Description, PaymentDeductionType Type, decimal Rate, string Account)[] codes =
            [
                ("WHT-5", "Withholding tax 5%", PaymentDeductionType.WithholdingTax, 5m, "2350"),
                ("WVAT-2", "Withholding VAT 2%", PaymentDeductionType.WithholdingVat, 2m, "2360"),
                ("RET-10", "Retention 10%", PaymentDeductionType.Retention, 10m, "2370"),
            ];

            foreach (var (code, description, type, rate, account) in codes)
            {
                var deduction = new PaymentDeductionCode(NewId(), code, description);
                deduction.Set(type, rate, account);
                await Repo<PaymentDeductionCode>().InsertAsync(deduction);
            }
        }
    }
}
